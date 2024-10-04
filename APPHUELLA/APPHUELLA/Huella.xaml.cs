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
    public partial class Huella : ContentPage
    {
        PeticionHTTP peticion = new PeticionHTTP("http://192.168.1.1");
        private string deviceIdentifier;
        private string username;
        private string password;

        public Huella(string user, string pass)
        {
            InitializeComponent();
            username = user;
            password = pass;
            deviceIdentifier = GetDeviceIdentifier();
        }

        private async void OnCaptureHuellaClicked(object sender, EventArgs e)
        {
            try
            {
                LoadingIndicator.IsRunning = true;
                var authConfig = new AuthenticationRequestConfiguration(
                    "Por favor, escanee su huella",
                    "Necesitamos verificar su identidad");
                var result = await CrossFingerprint.Current.AuthenticateAsync(authConfig);

                if (result.Authenticated)
                {
                    HuellaResult.Text = "Huella capturada exitosamente";
                    await SaveHuellaData();
                }
                else
                {
                    HuellaResult.Text = "Falló la autenticación de la huella";
                }
            }
            catch (Exception ex)
            {
                HuellaResult.Text = $"Error durante la autenticación: {ex.Message}";
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
            }
        }

        private async Task SaveHuellaData()
        {
            if (string.IsNullOrEmpty(deviceIdentifier))
            {
                await DisplayAlert("Error", "No se pudo obtener el identificador del dispositivo", "OK");
                return;
            }

            try
            {
                var userData = new
                {
                    usuario = username,
                    contrasena = password,
                    huella = deviceIdentifier
                };
                string jsonData = JsonConvert.SerializeObject(userData);
                peticion.PedirComunicacion("saveHuella", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);
                string responseJson = peticion.ObtenerJson();
                await DisplayAlert("Éxito", "Registro completo con huella enviada correctamente", "OK");
                await Navigation.PopToRootAsync();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al enviar datos de huella: {ex.Message}", "OK");
            }
        }

        private string GetDeviceIdentifier()
        {
            try
            {
                return DependencyService.Get<IDeviceIdentifier>().GetIdentifier();
            }
            catch (Exception)
            {
                return Guid.NewGuid().ToString("N");
            }
        }
    }

    public interface IDeviceIdentifier
    {
        string GetIdentifier();
    }
}