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
        private string password;

        public Huella(string user)
        {
            InitializeComponent();
            username = user;
            

            VerificarCredencialesYToken(username, password);
        }

        private async Task<bool> VerificarCredencialesYToken(string username, string password)
        {
            try
            {
                Usuario usuario = new Usuario
                {
                    Nombre = username,
                    Contraseña = password
                };

                string jsonData = JsonConvert.SerializeObject(usuario);
                peticion.PedirComunicacion("verificarUsuario", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);

                string responseJson = peticion.ObtenerJson();
                var response = JsonConvert.DeserializeObject<VerificarUsuarioResponse>(responseJson);

                if (response.status == "success" && !string.IsNullOrEmpty(response.tokenHuella))
                {
                    await DisplayAlert("Información", "Ya tienes un token de huella registrado", "OK");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al verificar credenciales: {ex.Message}", "OK");
                return false;
            }
        }

        private async void OnCaptureHuellaClicked(object sender, EventArgs e)
        {
            try
            {
                var authConfig = new AuthenticationRequestConfiguration(
                    "Por favor, escanee su huella",
                    "Necesitamos verificar su huella digital");
                var result = await CrossFingerprint.Current.AuthenticateAsync(authConfig);

                if (result.Authenticated)
                {
                    string nuevoToken = GenerarTokenHuella();
                    await GuardarTokenHuella(nuevoToken);
                }
                else
                {
                    await DisplayAlert("Error", "Falló la autenticación de la huella", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error durante la autenticación: {ex.Message}", "OK");
            }
        }

        private string GenerarTokenHuella()
        {
            return Guid.NewGuid().ToString("N"); // Genera un token único
        }

        private async Task GuardarTokenHuella(string nuevoToken)
        {
            try
            {
                var tokenData = new
                {
                    usuario = username,
                    huella = nuevoToken
                };

                string jsonData = JsonConvert.SerializeObject(tokenData);
                peticion.PedirComunicacion("saveHuella", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);

                string responseJson = peticion.ObtenerJson();
                var response = JsonConvert.DeserializeObject<GuardarHuellaResponse>(responseJson);

                if (response.status == "success")
                {
                    await DisplayAlert("Éxito", "Token de huella registrado correctamente", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", response.message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al registrar el token de huella: {ex.Message}", "OK");
            }
        }
    }

    public class VerificarUsuarioResponse
    {
        public string status { get; set; }
        public string tokenHuella { get; set; }
    }

    public class GuardarHuellaResponse
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}