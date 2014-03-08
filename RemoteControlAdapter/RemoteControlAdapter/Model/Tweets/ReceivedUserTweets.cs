using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using RemoteControlAdapter.Model.Databases;

namespace RemoteControlAdapter.Model.Tweets
{
    public static class ReceivedUserTweets
    {
        private static ConcurrentBag<long> tweetIds = new ConcurrentBag<long>();
        private static ConcurrentBag<WordCount> words = new ConcurrentBag<WordCount>();

        public static async void LoadFromDatabase()
        {
            using (var exec = await Database.ConnectAsync())
            {
                await Task.Run(() =>
                {
                    var ids = exec.SelectDynamic("select Id from MyTweets")
                        .Select(d => (long)d.Id);
                    foreach (var id in ids)
                        tweetIds.Add(id);

                    foreach (var w in exec.Select<WordCount>("select * from Words"))
                        words.Add(w);
                });
            }
        }

        public static bool ContainsTweet(long id)
        {
            return tweetIds.Contains(id);
        }

        public static async Task AddTweet(Status status)
        {
            tweetIds.Add(status.ID);
            using (var exec = await Database.ConnectAsync())
                await Task.Run(() =>
                {
                    exec.Insert("MyTweets", Tweet.FromStatus(status));
                    exec.TransactionComplete();
                });
        }

        public static async Task IncrementWordCount(long userId, string word, int add)
        {
            var w = await Task.Run(() => words.FirstOrDefault(x => x.UserId == userId && x.Word == word));
            using (var exec = await Database.ConnectAsync())
            {
                if (w == null)
                {
                    w = new WordCount()
                    {
                        UserId = userId,
                        Word = word,
                        Count = add
                    };
                    words.Add(w);
                    await Task.Run(() => exec.Insert("Words", w));
                }
                else
                {
                    w.Count += add;
                    await Task.Run(() => exec.Update("Words", w, new { UserId = userId, Word = word }));
                }
            }
        }

        public static WordCount[] GetWordList(long userId)
        {
            return words.Where(w => w.UserId == userId).ToArray();
        }
    }
}
