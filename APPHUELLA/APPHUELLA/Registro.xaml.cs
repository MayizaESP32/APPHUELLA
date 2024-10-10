using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using HTTPupt;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace APPHUELLA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Registro : ContentPage
    {
        PeticionHTTP peticion = new PeticionHTTP("http://192.168.1.1");
        public Registro()
        {
            InitializeComponent();
        }
        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Usuario o contraseña no pueden estar vacíos", "OK");
            }
            else
            {
                await GuardarUsuario(username, password);
            }
        }
        private async Task GuardarUsuario(string username, string password)
        {
            try
            {
                var userData = new
                {
                    usuario = username,
                    contrasena = password,
                    huella = ""  // Huella vacía inicialmente
                };
                string jsonData = JsonConvert.SerializeObject(userData);
                peticion.PedirComunicacion("save", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);
                string responseJson = peticion.ObtenerJson();
                var respuesta = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);
                if (respuesta["status"] == "success")
                {
                    await DisplayAlert("Éxito", respuesta["message"], "OK");
                    PasswordEntry.Text = "";
                    UsernameEntry.Text = "";
                }
                else
                {
                    await DisplayAlert("Error", respuesta["message"], "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al enviar datos: {ex.Message}", "OK");
            }
        }
    }
}