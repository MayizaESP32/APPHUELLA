using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using HTTPupt;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace APPHUELLA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AbrirCaja : ContentPage
    {
        PeticionHTTP peticion = new PeticionHTTP("http://192.168.1.1");
        private string username;
        private string huella;

        public AbrirCaja(string username, string huella)
        {
            InitializeComponent();
            this.username = username;
            this.huella = huella;
        }

        private async void OnAbrirCajaClicked(object sender, EventArgs e)
        {
            try
            {
                var authConfig = new AuthenticationRequestConfiguration(
                    "Verifica tu huella",
                    "Necesitamos verificar su identidad para abrir la caja");
                var result = await CrossFingerprint.Current.AuthenticateAsync(authConfig);
                if (result.Authenticated)
                {
                    await VerificarHuellaYAbrirCaja();
                }
                else
                {
                    await DisplayAlert("Error", "Falló la autenticación de la huella", "OK");
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
                var datos = new
                {
                    Nombre = username,
                    Huella = huella
                };
                string jsonData = JsonConvert.SerializeObject(datos);
                peticion.PedirComunicacion("verifyHuella", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);
                string responseJson = peticion.ObtenerJson();
                var response = JsonConvert.DeserializeObject<VerificarHuellaResponse>(responseJson);

                if (response.status == "success")
                {
                    string storedToken = await SecureStorage.GetAsync("huellaToken");
                    if (storedToken == response.token)
                    {
                        await DisplayAlert("Éxito", "Huella verificada. La caja se puede abrir.", "OK");
                        // Aquí puedes agregar código adicional para abrir la caja si es necesario
                    }
                    else
                    {
                        await DisplayAlert("Error", "Token de huella no coincide. No se puede abrir la caja.", "OK");
                    }
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
        public string token { get; set; }
    }
}