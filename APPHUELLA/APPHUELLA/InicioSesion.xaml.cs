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
                peticion.PedirComunicacion("login", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(JsonConvert.SerializeObject(usuario));
                String json = peticion.ObtenerJson();

                if (!string.IsNullOrEmpty(json))
                {
                    var respuesta = JsonConvert.DeserializeObject<RespuestaLogin>(json);
                    if (respuesta != null && respuesta.status == "success")
                    {
                        if (!respuesta.huellaRegistrada)
                        {
                            await Navigation.PushAsync(new Huella(username, respuesta.token));
                        }
                        else
                        {
                            await Navigation.PushAsync(new AbrirCaja(username, respuesta.huella, respuesta.token));
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", respuesta?.message ?? "Respuesta inválida del servidor", "Aceptar");
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

    public class RespuestaLogin
    {
        public string status { get; set; }
        public string message { get; set; }
        public bool huellaRegistrada { get; set; }
        public string huella { get; set; }
        public string token { get; set; }
    }
}