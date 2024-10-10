using Xamarin.Essentials;
using System.Threading.Tasks;

namespace APPHUELLA
{
    public class BiometricStorage
    {
        private const string TOKEN_KEY = "biometricToken";

        public async Task SaveToken(string token)
        {
            await SecureStorage.SetAsync(TOKEN_KEY, token);
        }

        public async Task<string> GetToken()
        {
            return await SecureStorage.GetAsync(TOKEN_KEY);
        }

        public void ClearToken()
        {
            SecureStorage.Remove(TOKEN_KEY);
        }
    }
}