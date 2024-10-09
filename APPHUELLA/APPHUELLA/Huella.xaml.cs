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
        private string tokenGuardado; // Almacena el token de huella guardado

        public Huella(string user, string pass)
        {
            InitializeComponent();
            username = user;
            password = pass;

            // Verificar si el usuario ya tiene huella registrada
            VerificarCredencialesYHuella(username, password);
        }

        // Método que se llama al hacer login (verifica si ya tiene huella o no)
        private async Task<bool> VerificarCredencialesYHuella(string username, string password)
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
                    tokenGuardado = response.tokenHuella;
                    return true; // Ya tiene huella registrada
                }

                return false; // No tiene huella registrada
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al verificar credenciales: {ex.Message}", "OK");
                return false;
            }
        }

        // Método para capturar y guardar la huella
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
                    string tokenHuella = GenerarTokenHuella();
                    await GuardarHuellaData(tokenHuella);
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

        // Generar token único (simula el hash de la huella)
        private string GenerarTokenHuella()
        {
            return Guid.NewGuid().ToString("N"); // Token como un GUID
        }

        // Guardar el token de la huella
        private async Task GuardarHuellaData(string tokenHuella)
        {
            try
            {
                Usuario usuario = new Usuario
                {
                    Nombre = username,
                    Contraseña = password,
                    Huella = tokenHuella // Guardamos el token como huella
                };

                string jsonData = JsonConvert.SerializeObject(usuario);
                peticion.PedirComunicacion("guardarHuella", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);

                string responseJson = peticion.ObtenerJson();
                var response = JsonConvert.DeserializeObject<GuardarHuellaResponse>(responseJson);

                if (response.status == "success")
                {
                    await DisplayAlert("Éxito", "Huella registrada correctamente", "OK");
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

        // Método para abrir la caja comparando la huella
        private async void OnAbrirCajaClicked(object sender, EventArgs e)
        {
            try
            {
                var authConfig = new AuthenticationRequestConfiguration(
                    "Verifica tu huella",
                    "Necesitamos verificar tu identidad para abrir la caja");
                var result = await CrossFingerprint.Current.AuthenticateAsync(authConfig);

                if (result.Authenticated)
                {
                    // Aquí suponemos que el token de la huella ya fue guardado en tokenGuardado
                    if (tokenGuardado != null)
                    {
                        await DisplayAlert("Éxito", "Caja abierta correctamente", "OK");
                        // Lógica para abrir la caja
                    }
                    else
                    {
                        await DisplayAlert("Error", "La huella no coincide con la registrada", "OK");
                    }
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
    }

    // Modelos de respuesta para las solicitudes
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
