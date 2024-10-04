using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using HTTPupt;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace APPHUELLA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AbrirCaja : ContentPage
    {
        PeticionHTTP peticion = new PeticionHTTP("http://192.168.1.1");
        private string deviceIdentifier;
        private string username;
        private string password;

        public AbrirCaja(string user, string pass)
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
                    HuellaResult.Text = "Huella capturada exitosamente. Validando...";
                    await ValidateHuellaWithESP32();
                }
                else
                {
                    HuellaResult.Text = "Falló la autenticación de la huella";
                    await DisplayAlert("Error", "No se pudo autenticar la huella", "OK");
                }
            }
            catch (Exception ex)
            {
                HuellaResult.Text = $"Error durante la autenticación: {ex.Message}";
                await DisplayAlert("Error", $"Error durante la autenticación: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
            }
        }

        private async Task ValidateHuellaWithESP32()
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
                    huella = deviceIdentifier
                };
                string jsonData = JsonConvert.SerializeObject(userData);
                peticion.PedirComunicacion("validarHuella", MetodoHTTP.POST, TipoContenido.JSON);
                peticion.enviarDatos(jsonData);
                string responseJson = peticion.ObtenerJson();
                var respuesta = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);

                if (respuesta["status"] == "success")
                {
                    await DisplayAlert("Éxito", "Caja abierta", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "La huella no pertenece al usuario", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al validar la huella: {ex.Message}", "OK");
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
    namespace APPHUELLA
    {
        public interface IDeviceIdentifier
        {
            string GetIdentifier();
        }
    }


}