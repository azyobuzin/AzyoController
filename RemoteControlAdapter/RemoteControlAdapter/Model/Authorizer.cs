using System;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;

namespace RemoteControlAdapter.Model
{
    public class Authorizer
    {
        private OAuth.OAuthSession session;

        public Task<Uri> GetRequestTokenAsync()
        {
            return Task.Run(() =>
            {
                this.session = OAuth.Authorize(Settings.ConsumerKey, Settings.ConsumerSecret);
                return this.session.AuthorizeUri;
            });
        }

        public Task GetAccessTokenAsync(string verifier)
        {
            return Task.Run(() =>
            {
                var tokens = this.session.GetTokens(verifier);
                var user = tokens.Account.VerifyCredentials();
                if (Settings.Instance.Users.All(u => u.UserId != user.ID))
                {
                    Settings.Instance.Users.Add(new User()
                    {
                        OAuthToken = tokens.AccessToken,
                        OAuthTokenSecret = tokens.AccessTokenSecret,
                        UserId = user.ID.Value,
                        ScreenName = user.ScreenName,
                        ProfileImage = user.ProfileImageUrlHttps.ToString()
                    });
                    Settings.Instance.Save();
                }
            });
        }
    }
}
