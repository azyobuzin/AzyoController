using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToTwitter;

namespace RemoteControlAdapter.Model
{
    public class Authorizer
    {
        private SingleUserAuthorizer authorizer = new SingleUserAuthorizer()
        {
            CredentialStore = new InMemoryCredentialStore()
            {
                ConsumerKey = Settings.ConsumerKey,
                ConsumerSecret = Settings.ConsumerSecret
            }
        };

        public async Task<string> GetRequestTokenAsync()
        {
            await this.authorizer.GetRequestTokenAsync("oob");
            return this.authorizer.PrepareAuthorizeUrl(false);
        }

        public async Task GetAccessTokenAsync(string verifier)
        {
            await this.authorizer.GetAccessTokenAsync(new Dictionary<string, string>()
            {
                { "oauth_verifier", verifier }
            });
            if (!Settings.Instance.Users.Any(u => u.UserId == this.authorizer.CredentialStore.UserID))
                Settings.Instance.Users.Add(new User()
                {
                    OAuthToken = this.authorizer.CredentialStore.OAuthToken,
                    OAuthTokenSecret = this.authorizer.CredentialStore.OAuthTokenSecret,
                    UserId = this.authorizer.CredentialStore.UserID,
                    ScreenName = this.authorizer.CredentialStore.ScreenName
                });
        }
    }
}
