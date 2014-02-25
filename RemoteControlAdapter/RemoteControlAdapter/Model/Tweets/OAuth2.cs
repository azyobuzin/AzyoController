using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RemoteControlAdapter.Model.Tweets
{
    public static class OAuth2
    {
        public static async Task<string> GetBearerToken()
        {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(
                Settings.ConsumerKey + ":" + Settings.ConsumerSecret
            ));
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);
                var res = await client.PostAsync("https://api.twitter.com/oauth2/token", new FormUrlEncodedContent(
                    new Dictionary<string, string>() { { "grant_type", "client_credentials" } }
                ));
                return (string)JObject.Parse(await res.Content.ReadAsStringAsync())["access_token"];
            }
        }

        public static HttpClient CreateOAuth2Client(string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            return client;
        }
    }
}
