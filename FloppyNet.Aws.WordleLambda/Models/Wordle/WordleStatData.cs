namespace FloppyNet.Aws.WordleLambda.Models.Wordle
{
    public class WordleStatData
    {
        public long UserId { get; set; }
        public double Average { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
    }
}