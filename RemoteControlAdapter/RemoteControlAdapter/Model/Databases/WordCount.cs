namespace RemoteControlAdapter.Model.Databases
{
    public class WordCount
    {
        public long UserId { get; set; }
        public string Word { get; set; }
        public long Count { get; set; } //キャストに失敗するので long にしてある
    }
}
