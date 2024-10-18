using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using HTTPupt;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;

namespace APPHUELLA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AbrirCaja : ContentPage
    {
        PeticionHTTP peticion = new PeticionHTTP("http://192.168.1.1");
        private string username;
        private BiometricStorage biometricStorage;

        public AbrirCaja(string username)
        {
            InitializeComponent();
            this.username = username;
            biometricStorage = new BiometricStorage();
        }

        private async void OnAbrirCajaClicked(object sender, EventArgs e)
        {
            try
            {
                string token = await biometricStorage.GetToken();
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Error", "No hay huella registrada. Por favor, registre una primero.", "OK");
                    return;
                }

                var datos = new
                {
                    usuario = username,
                    token = token
                };
                string jsonData = JsonConvert.SerializeObject(datos);
                peticion.PedirComunicacion("openBox", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);
                string responseJson = peticion.ObtenerJson();
                var response = JsonConvert.DeserializeObject<OpenBoxResponse>(responseJson);

                if (response.status == "success")
                {
                    await DisplayAlert("Éxito", "Solicitud de apertura procesando", "OK");
                    await VerificarHuella();
                }
                else
                {
                    await DisplayAlert("Error", response.message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al abrir la caja: {ex.Message}", "OK");
            }
        }

        private async Task VerificarHuella()
        {
            try
            {
                var authConfig = new AuthenticationRequestConfiguration(
                    "Verificar huella",
                    "Por favor, coloque su dedo en el sensor de huella para confirmar")
                {
                    AllowAlternativeAuthentication = false
                };
                var result = await CrossFingerprint.Current.AuthenticateAsync(authConfig);
                if (result.Authenticated)
                {
                    await DisplayAlert("Éxito", "Huella verificada. La caja está abierta.", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo verificar la huella", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al verificar la huella: {ex.Message}", "OK");
            }
        }
    }

    public class OpenBoxResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}