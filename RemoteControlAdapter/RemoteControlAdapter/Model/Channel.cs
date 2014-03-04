namespace RemoteControlAdapter.Model
{
    public class Channel
    {
        public Channel(int num, string name, string hashtag)
        {
            this.Number = num;
            this.Name = name;
            this.Hashtag = hashtag;
        }

        public int Number { get; private set; }
        public string Name { get; private set; }
        public string Hashtag { get; private set; }
    }
}
