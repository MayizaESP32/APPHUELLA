using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using HTTPupt;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using Xamarin.Essentials;
using APPHUELLA.Modelos;

namespace APPHUELLA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Huella : ContentPage
    {
        PeticionHTTP peticion = new PeticionHTTP("http://192.168.1.1");
        private string username;
        private string token;

        public Huella(string user, string token)
        {
            InitializeComponent();
            username = user;
            this.token = token;
        }

        private async void OnCaptureHuellaClicked(object sender, EventArgs e)
        {
            try
            {
                var authConfig = new AuthenticationRequestConfiguration(
                    "Por favor, escanee su huella",
                    "Necesitamos registrar su huella digital");
                var result = await CrossFingerprint.Current.AuthenticateAsync(authConfig);

                if (result.Authenticated)
                {
                    string huellaCapturada = GenerarHuellaDummy(); // Reemplazar con la captura real de huella
                    await GuardarHuella(huellaCapturada);
                }
                else
                {
                    await DisplayAlert("Error", "Falló la captura de la huella", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error durante la captura de huella: {ex.Message}", "OK");
            }
        }

        private string GenerarHuellaDummy()
        {
            return Guid.NewGuid().ToString("N"); // Simula una huella única
        }

        private async Task GuardarHuella(string huella)
        {
            try
            {
                var datos = new
                {
                    usuario = username,
                    huella = huella,
                    token = token
                };

                string jsonData = JsonConvert.SerializeObject(datos);
                peticion.PedirComunicacion("saveHuella", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);

                string responseJson = peticion.ObtenerJson();
                var response = JsonConvert.DeserializeObject<GuardarHuellaResponse>(responseJson);

                if (response.status == "success")
                {
                    await SecureStorage.SetAsync("huellaToken", token);
                    await DisplayAlert("Éxito", "Huella registrada correctamente", "OK");
                    await Navigation.PushAsync(new AbrirCaja(username, huella, token));
                }
                else
                {
                    await DisplayAlert("Error", response.message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al registrar la huella: {ex.Message}", "OK");
            }
        }
    }

    public class GuardarHuellaResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}