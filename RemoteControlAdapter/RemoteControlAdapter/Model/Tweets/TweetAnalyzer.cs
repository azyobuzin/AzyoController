using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using NMeCab;

namespace RemoteControlAdapter.Model.Tweets
{
    public static class TweetAnalyzer
    {
        public static Task<Tuple<string, int>[]> Analyze(Status status)
        {
            return Task.Run(() =>
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

                return EnumerateNodes(text)
                    .Where(n => n.Feature.Split(',')[0] == "名詞" && !n.Surface.All(IsSpecialUnicode))
                    .GroupBy(n => n.Surface.ToLower())
                    .Select(g => Tuple.Create(g.Key, g.Count()))
                    .Concat(
                        status.Entities.HashTags
                            .GroupBy(entity => entity.Text.ToLower())
                            .Select(g => Tuple.Create("#" + g.Key, g.Count()))
                    )
                    .ToArray();
            });
        }

        private static IEnumerable<MeCabNode> EnumerateNodes(string str)
        {
            var node = MeCabTagger.Create().ParseToNode(str);
            while (node != null)
            {
                yield return node;
                node = node.Next;
            }
        }

        private static readonly char[] specialChars = Range(0, 0x40)
                .Concat(Range(0x5B, 0x60))
                .Concat(Range(0x7B, 0x7E))
                .Concat(Range(0x2000, 0x206F)) //一般句読点
                .Concat(Range(0xFE50, 0xFE6F)) //小字形
                .Concat(Range(0x2E00, 0x2E7F)) //補助句読点
                .Concat(Range(0x3000, 0x303F)) //CJKの記号及び句読点
                .Concat(Range(0xFE30, 0xFE4F)) //CJK互換形
                .Concat(Range(0xFE10, 0xFE1F)) //縦書き形
                .Concat(Range(0x2100, 0x214F)) //文字様記号
                .Concat(Range(0x2460, 0x24FF)) //囲み英数字
                .Concat(Range(0x3200, 0x32FF)) //囲みCJK文字・月
                .Concat(Range(0x3300, 0x33FF)) //CJK互換用文字
                .Concat(Range(0x2400, 0x243F)) //制御機能用記号
                .Concat(Range(0x2440, 0x245F)) //光学的文字認識（OCR）
                .Concat(Range(0x2300, 0x23FF)) //その他の技術用記号
                .Concat(Range(0x2190, 0x21FF)) //矢印
                .Concat(Range(0x27F0, 0x27FF)) //補助矢印A
                .Concat(Range(0x2900, 0x297F)) //補助矢印B
                .Concat(Range(0x2200, 0x22FF)) //数学記号
                .Concat(Range(0x2A00, 0x2AFF)) //補助数学記号
                .Concat(Range(0x27C0, 0x27EF)) //その他の数学記号A
                .Concat(Range(0x2980, 0x29FF)) //その他の数学記号B
                .Concat(Range(0x25A0, 0x25FF)) //幾何学模様
                .Concat(Range(0x2500, 0x257F)) //罫線素片
                .Concat(Range(0x2580, 0x259F)) //ブロック要素
                .Concat(Range(0xFF01, 0xFF20)) //全角ASCII
                .Concat(Range(0xFF3B, 0xFF40))
                .Concat(Range(0xFF5B, 0xFF64)) //全角ASCII, 全角括弧, 半角CJK句読点
                .Concat(Range(0xFFE0, 0xFFEE)) //全角記号, 半角記号
                .Select(i => (char)i)
                .ToArray();

        private static bool IsSpecialUnicode(char c)
        {
            return specialChars.Contains(c);
        }

        private static IEnumerable<int> Range(int start, int end)
        {
            return Enumerable.Range(start, end - start);
        }
    }
}
