using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace APPHUELLA
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Menu : ContentPage
    {
        public Menu()
        {
            InitializeComponent();
        }

        private async void OnRegistrarUsuarioClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Registro());
        }

        private async void OnAbrirCajaFuerteClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Inicio());
        }
    }
}