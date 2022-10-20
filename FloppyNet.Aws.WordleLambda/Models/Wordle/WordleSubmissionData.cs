namespace FloppyNet.Aws.WordleLambda.Models.Wordle
{
    internal class WordleSubmissionData
    {
        public long UserId { get; set; }
        public short WordleId { get; set; }
        public bool Success { get; set; }
        public int Attempts { get; set; }
        public string? GuessMatrix { get; set; }
    }
}