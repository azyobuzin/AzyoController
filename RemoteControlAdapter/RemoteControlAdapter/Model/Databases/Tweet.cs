using CoreTweet;

namespace RemoteControlAdapter.Model.Databases
{
    public class Tweet
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public long CreatedAt { get; set; }
        public long UserId { get; set; }

        public static Tweet FromStatus(Status status)
        {
            return new Tweet()
            {
                Id = status.ID,
                Text = status.Text,
                CreatedAt = status.CreatedAt.UtcTicks,
                UserId = status.User.ID.Value
            };
        }
    }
}
