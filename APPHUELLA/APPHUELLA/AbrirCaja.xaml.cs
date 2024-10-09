using Plugin.Fingerprint.Abstractions;
using Plugin.Fingerprint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace APPHUELLA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AbrirCaja : ContentPage
    {
        public AbrirCaja()
        {
            InitializeComponent();
        }
        private async void OnCaptureHuellaClicked(object sender, EventArgs e)
        {
            try
            {
                var authConfig = new AuthenticationRequestConfiguration(
                    "Verifica tu huella",
                    "Por favor, coloca tu huella en el lector");
                var result = await CrossFingerprint.Current.AuthenticateAsync(authConfig);

                if (result.Authenticated)
                {
                    await DisplayAlert("Éxito", "Caja abierta correctamente", "OK");
                    // Aquí puedes agregar lógica adicional para abrir la caja
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
    }
}    