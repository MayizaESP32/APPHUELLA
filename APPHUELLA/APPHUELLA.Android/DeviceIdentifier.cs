using Xamarin.Forms;
using APPHUELLA.Droid;
using Android.Provider;
using Android.App;

[assembly: Dependency(typeof(DeviceIdentifier))]
namespace APPHUELLA.Droid
{
    public class DeviceIdentifier : IDeviceIdentifier
    {
        public string GetIdentifier()
        {
            return Settings.Secure.GetString(
                Android.App.Application.Context.ContentResolver,
                Settings.Secure.AndroidId);
        }
    }
}