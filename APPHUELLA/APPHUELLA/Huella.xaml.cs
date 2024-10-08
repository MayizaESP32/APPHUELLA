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
        private string deviceImei;
        private string username;
        private string password;

        public Huella(string user, string pass)
        {
            InitializeComponent();
            username = user;
            password = pass;
            deviceImei = GetDeviceImei();
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
            if (string.IsNullOrEmpty(deviceImei))
            {
                await DisplayAlert("Error", "No se pudo obtener el IMEI del dispositivo", "OK");
                return;
            }
            try
            {
                Usuario usuario = new Usuario()
                {
                    Nombre = username,
                    Contraseña = password,
                    Huella = deviceImei
                };
                string jsonData = JsonConvert.SerializeObject(usuario);
                peticion.PedirComunicacion("saveHuella", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);
                string responseJson = peticion.ObtenerJson();
                var response = JsonConvert.DeserializeObject<UpdateHuellaResponse>(responseJson);

                if (response.status == "success")
                {
                    await DisplayAlert("Éxito", "Huella (IMEI) registrada correctamente", "OK");
                    await Navigation.PopAsync();
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

        private string GetDeviceImei()
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

    public class UpdateHuellaResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}