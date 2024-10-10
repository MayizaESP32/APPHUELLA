using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using HTTPupt;
using Newtonsoft.Json;
using Xamarin.Essentials;
using APPHUELLA.Modelos;

namespace APPHUELLA
{
    public partial class InicioSesion : ContentPage
    {
        PeticionHTTP peticion = new PeticionHTTP("http://192.168.1.1");
        
        public InicioSesion()
        {
            InitializeComponent();
        }

        private async void Iniciar_Clicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            
            // Validación de campos
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Todos los campos son obligatorios", "OK");
                return;
            }

            Usuario usuario = new Usuario()
            {
                Nombre = username,
                Contraseña = password,
            };

            try
            {
                Iniciar.IsEnabled = false;
                // Solicitud HTTP POST para iniciar sesión
                peticion.PedirComunicacion("login", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(JsonConvert.SerializeObject(usuario));
                String json = peticion.ObtenerJson();

                if (!string.IsNullOrEmpty(json))
                {
                    var respuesta = JsonConvert.DeserializeObject<RespuestaLogin>(json);

                    if (respuesta.status == "success")
                    {
                        if (respuesta.huellaRegistrada)
                        {
                            // Verificar si ya existe el token de huella en almacenamiento seguro
                            var huellaToken = await SecureStorage.GetAsync("huellaToken");

                            if (!string.IsNullOrEmpty(huellaToken))
                            {
                                // Si existe el token, abrir caja
                                await Navigation.PushAsync(new AbrirCaja(username, respuesta.huella));
                            }
                            else
                            {
                                // Si no existe el token, navegar a la vista de registro de huella
                                await SecureStorage.SetAsync("huellaToken", respuesta.token);
                                await Navigation.PushAsync(new Huella(username));
                            }
                        }
                        else
                        {
                            // Si la huella no está registrada, navegar a la vista de huella
                            await Navigation.PushAsync(new Huella(username));
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", respuesta.message, "Aceptar");
                        UsernameEntry.Text = "";
                        PasswordEntry.Text = "";
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se recibió respuesta del servidor", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
            finally
            {
                Iniciar.IsEnabled = true;
            }
        }
    }

    // Clases de modelo para la respuesta JSON
    public class RespuestaLogin
    {
        public string status { get; set; }
        public string message { get; set; }
        public bool huellaRegistrada { get; set; }
        public string huella { get; set; }
        public string token { get; set; }  // Campo de token para abrir caja
    }
}
