using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using RemoteControlAdapter.Model.Databases;

namespace RemoteControlAdapter.Model
{
    public static class UsualSuggesting
    {
        private static ConcurrentBag<Tuple<Channel, Time, Time>> history = new ConcurrentBag<Tuple<Channel, Time, Time>>();
        private static Timer timer;

        public static async void Initialize()
        {
            using (var exec = await Database.ConnectAsync())
            {
                var a = await Task.Run(() => exec.Select<Operation>("select * from Operations")
                    .Select(o => Tuple.Create(
                        Settings.Channels.First(c => c.Number == o.Channel),
                        new Time(new DateTime(o.Start)),
                        new Time(new DateTime(o.End))
                    ))
                    .ToArray()
                );
                foreach (var t in a)
                    history.Add(t);
            }

            timer = new Timer(60 * 1000);
            timer.Elapsed += (sender, e) => Suggest();
            timer.Start();
            Suggest();
        }

        private static Channel suggestedChannel;
        public static Channel SuggestedChannel
        {
            get
            {
                return suggestedChannel;
            }
            set
            {
                if (suggestedChannel != value)
                {
                    suggestedChannel = value;
                    if (SuggestedChannelChanged != null)
                        SuggestedChannelChanged(null, EventArgs.Empty);
                }
            }
        }

        public static event EventHandler SuggestedChannelChanged;

        public static void Suggest()
        {
            var match = history.Where(t => Time.IsInTimeSpan(DateTime.Now, t.Item2, t.Item3)).ToArray();
            if (match.Any())
            {
                SuggestedChannel = match.GroupBy(t => t.Item1)
                    .OrderByDescending(l => l.Count())
                    .First().Key;
            }
            else
            {
                SuggestedChannel = null;
            }
        }

        private static DateTime start;
        private static int watching;

        public static void SetWatchingChannel(int channel)
        {
            EndWatching();
            watching = channel;
            start = DateTime.Now;
        }

        public static void EndWatching()
        {
            if (watching != 0)
            {
                var end = DateTime.Now;
                history.Add(Tuple.Create(Settings.Channels.First(c => c.Number == watching), new Time(start), new Time(end)));
                var operation = new Operation()
                {
                    Channel = watching,
                    Start = start.Ticks,
                    End = end.Ticks
                };
                using (var exec = Database.Connect())
                {
                    exec.Insert("Operations", operation);
                }
                watching = 0;
            }
        }
    }
}
