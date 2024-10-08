using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using HTTPupt;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using System.Collections.Generic;
using Xamarin.Essentials;
using APPHUELLA.Modelos;
using APPHUELLA;

namespace APPHUELLA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
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
                // Realizar la solicitud HTTP POST para iniciar sesión
                peticion.PedirComunicacion("login", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(JsonConvert.SerializeObject(usuario));
                String json = peticion.ObtenerJson();

                // Verificar si la respuesta JSON no es null o vacía
                if (!string.IsNullOrEmpty(json))
                {
                    // Deserializar la respuesta JSON
                    var respuesta = JsonConvert.DeserializeObject<RespuestaLogin>(json);

                    if (respuesta.status == "success")
                    {
                        // Inicio de sesión exitoso, navegar a la página de Huella
                        await Navigation.PushAsync(new Huella(username, password));
                    }
                    else
                    {
                        // Mostrar alerta si los datos son incorrectos
                        await DisplayAlert("Alerta", respuesta.message, "Aceptar");
                        // Vaciar los campos de usuario y contraseña
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
                // Manejar cualquier excepción que pueda ocurrir durante la comunicación con la API
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }
    }

    public class RespuestaLogin
    {
        public string status { get; set; }
        public string message { get; set; }
    }
}