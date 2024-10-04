using HTTPupt;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace APPHUELLA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Inicio : ContentPage
    {
        PeticionHTTP peticion = new PeticionHTTP("http://192.168.1.1");

        public Inicio()
        {
            InitializeComponent();
        }

        private async void OnIniciarSesionClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Usuario o contraseña no pueden estar vacíos", "OK");
            }
            else
            {
                await ValidarUsuario(username, password);
            }
        }

        private async Task ValidarUsuario(string username, string password)
        {
            try
            {
                var userData = new
                {
                    usuario = username,
                    contrasena = password
                };
                string jsonData = JsonConvert.SerializeObject(userData);
                peticion.PedirComunicacion("validar", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);
                string responseJson = peticion.ObtenerJson();
                var respuesta = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);

                if (respuesta["status"] == "success")
                {
                    // Usuario registrado, navegar a AbrirCaja
                    await Navigation.PushAsync(new AbrirCaja(username,password));
                    PasswordEntry.Text = "";
                    UsernameEntry.Text = "";
                }
                else if (respuesta["status"] == "not_registered")
                {
                    // Usuario no registrado
                    await DisplayAlert("Error", "Usuario no registrado. Por favor, regístrese primero.", "OK");
                }
                else
                {
                    // Otro tipo de error
                    await DisplayAlert("Error", respuesta["message"], "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Usuario no registrado. Por favor, regístrese primero.", "OK");
                await Navigation.PushAsync(new Menu());
            }
        }
    }
}