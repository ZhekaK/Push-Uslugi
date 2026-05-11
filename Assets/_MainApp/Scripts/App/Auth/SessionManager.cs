using System.Threading.Tasks;
using PushPelmesh.App.Api;
using PushPelmesh.App.Models;

namespace PushPelmesh.App.Auth
{
    public static class SessionManager
    {
        public static UserProfileResponse CurrentProfile { get; private set; }

        public static bool IsAuthorized =>
            TokenStorage.HasToken();

        public static async Task<bool> TryAutoLoginAsync()
        {
            if (!TokenStorage.HasToken())
                return false;

            try
            {
                CurrentProfile =
                    await AuthService.GetProfileAsync();

                return true;
            }
            catch (ApiException exception)
            {
                if (exception.StatusCode == 401)
                {
                    Logout();
                    return false;
                }

                throw;
            }
        }

        public static void SetProfile(UserProfileResponse profile)
        {
            CurrentProfile = profile;
        }

        public static void Logout()
        {
            CurrentProfile = null;

            TokenStorage.ClearToken();
        }
    }
}