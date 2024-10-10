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
                var authConfig = new AuthenticationRequestConfiguration(
                    "Verificar huella",
                    "Por favor, coloque su dedo en el sensor de huella para abrir la caja")
                {
                    AllowAlternativeAuthentication = false
                };
                var result = await CrossFingerprint.Current.AuthenticateAsync(authConfig);
                if (result.Authenticated)
                {
                    await VerificarHuellaYAbrirCaja();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo autenticar la huella", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al abrir la caja: {ex.Message}", "OK");
            }
        }

        private async Task VerificarHuellaYAbrirCaja()
        {
            try
            {
                string storedToken = await biometricStorage.GetToken();
                if (string.IsNullOrEmpty(storedToken))
                {
                    await DisplayAlert("Error", "No hay huella registrada. Por favor, registre una primero.", "OK");
                    return;
                }

                var datos = new
                {
                    usuario = username,
                    token = storedToken
                };
                string jsonData = JsonConvert.SerializeObject(datos);
                peticion.PedirComunicacion("checkHuella", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);
                string responseJson = peticion.ObtenerJson();
                var response = JsonConvert.DeserializeObject<VerificarHuellaResponse>(responseJson);
                if (response.status == "success")
                {
                    await DisplayAlert("Éxito", "Huella verificada. La caja se puede abrir.", "OK");
                    // Aquí puedes agregar código adicional para abrir la caja si es necesario
                }
                else
                {
                    await DisplayAlert("Error", response.message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al verificar la huella y abrir la caja: {ex.Message}", "OK");
            }
        }
    }

    public class VerificarHuellaResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}