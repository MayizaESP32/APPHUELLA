using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using HTTPupt;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using APPHUELLA.Modelos;

namespace APPHUELLA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Huella : ContentPage
    {
        PeticionHTTP peticion = new PeticionHTTP("http://192.168.1.1");
        private string username;
        private BiometricStorage biometricStorage;

        public Huella(string user)
        {
            InitializeComponent();
            username = user;
            biometricStorage = new BiometricStorage();
        }

        private async void OnCaptureHuellaClicked(object sender, EventArgs e)
        {
            try
            {
                var availability = await CrossFingerprint.Current.IsAvailableAsync();
                if (!availability)
                {
                    await DisplayAlert("Error", "La autenticación de huella no está disponible en este dispositivo", "OK");
                    return;
                }

                var authConfig = new AuthenticationRequestConfiguration(
                    "Registrar huella",
                    "Por favor, coloque su dedo en el sensor de huella para registrar")
                {
                    AllowAlternativeAuthentication = false
                };

                var result = await CrossFingerprint.Current.AuthenticateAsync(authConfig);
                if (result.Authenticated)
                {
                    string token = Guid.NewGuid().ToString("N");
                    await GuardarHuella(token);
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo autenticar la huella", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error durante la captura de huella: {ex.Message}", "OK");
            }
        }

        private async Task GuardarHuella(string token)
        {
            try
            {
                var datos = new
                {
                    usuario = username,
                    token = token
                };
                string jsonData = JsonConvert.SerializeObject(datos);
                peticion.PedirComunicacion("saveHuella", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);
                string responseJson = peticion.ObtenerJson();
                var response = JsonConvert.DeserializeObject<GuardarHuellaResponse>(responseJson);
                if (response.status == "success")
                {
                    await biometricStorage.SaveToken(token);
                    await DisplayAlert("Éxito", "Huella registrada correctamente", "OK");
                    await Navigation.PushAsync(new AbrirCaja(username));
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