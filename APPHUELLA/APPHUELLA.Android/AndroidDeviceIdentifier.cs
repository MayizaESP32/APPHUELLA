using Android.Content;
using Android.Telephony;
using APPHUELLA.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidDeviceIdentifier))]
namespace APPHUELLA.Droid
{
    public class AndroidDeviceIdentifier : IDeviceIdentifier
    {
        public string GetIdentifier()
        {
            TelephonyManager telephonyManager = (TelephonyManager)Android.App.Application.Context.GetSystemService(Context.TelephonyService);
            return telephonyManager.Imei ?? Android.OS.Build.Serial ?? Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
        }
    }
}