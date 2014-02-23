using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using CoreTweet;

namespace RemoteControlAdapter.Model.Tweets
{
    public static class TweetAnalyzer
    {
        public static async Task<Tuple<string, int>[]> Analyze(Status status)
        {
            var text = status.Text;
            if (status.Entities != null)
            {
                if (status.Entities.UserMentions != null)
                    foreach (var entity in status.Entities.UserMentions)
                        text = text.Replace("@" + entity.ScreenName, "");
                if (status.Entities.HashTags != null)
                    foreach (var entity in status.Entities.HashTags)
                        text = text.Replace("#" + entity.Text, "");
                if (status.Entities.Urls != null)
                    foreach (var entity in status.Entities.Urls)
                        text = text.Replace(entity.Uri.ToString(), "");
                if (status.Entities.Media != null)
                    foreach (var entity in status.Entities.Media)
                        text = text.Replace(entity.Url.ToString(), "");
            }

            return (await ParseText(text)).Concat(
                status.Entities.HashTags
                    .GroupBy(entity => entity.Text.ToLower())
                    .Select(g => Tuple.Create("#" + g.Key, g.Count()))
            ).ToArray();
        }

        private static async Task<Tuple<string, int>[]> ParseText(string str)
        {
            using (var client = new HttpClient())
            {
                XNamespace ns = "urn:yahoo:jp:jlp";
                return await client.GetStringAsync("http://jlp.yahooapis.jp/MAService/V1/parse?appid=dj0zaiZpPU5GU1B2OTNWSEUwaSZzPWNvbnN1bWVyc2VjcmV0Jng9ZjU-&results=uniq&filter=9&sentence=" + Uri.EscapeDataString(str))
                    .ContinueWith(t => XDocument.Parse(t.Result).Root
                        .Element(ns + "uniq_result").Element(ns + "word_list").Elements()
                        .Select(x => Tuple.Create((string)x.Element(ns + "surface"), (int)x.Element(ns + "count")))
                        .ToArray()
                    );
            }
        }
    }
}
