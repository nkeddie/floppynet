using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using FloppyNet.Aws.WordleLambda.Models.Wordle;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FloppyNet.Aws.WordleLambda
{
    public partial class WordleHandler
    {
        private readonly Regex regex = new("Wordle (?<WordleId>[0-9]{3,4}) (?<Attempts>[1-6X])/6(\\*)?\\n\\n(?<GuessMatrix>(.|\\n)*)");
        private readonly ILambdaContext lambdaContext;
        private readonly IAmazonS3 client;
        private readonly string bucketName;

        private readonly string[] ValidCharacters = new string[]
        {
            "⬜️",
            "🟨",
            "🟩",
            "⬛",
            "\n"
        };

        public WordleHandler(ILambdaContext lambdaContext, IAmazonS3 client, string bucketName)
        {
            this.lambdaContext = lambdaContext;
            this.client = client;
            this.bucketName = bucketName;
        }

        public async Task ExecuteAsync(long userId, string message)
        {
            var data = ParseMessage(userId, message);
            if (data == null)
            {
                lambdaContext.Logger.Log("Message was not wordle-related or could not be parsed.");
                return;
            };

            await Task.WhenAll(
                // if new user, add to users.json
                AddUserAsync(userId),

                // if new wordle, add to wordles.json
                AddWordleAsync(data.WordleId),

                // add to wordle-results-{userId}.json (validate submission does not already exist)
                AddWordleResultsAsync(data)
            );
        }

        private async Task AddUserAsync(long userId)
        {
            var users = await GetJsonFile<List<UserData>>("users") ?? new List<UserData>();

            if (users.Any(u => u.UserId == userId))
                return;

            users.Add(new UserData { UserId = userId });
            await PutJsonFile("users", users);
        }

        private async Task AddWordleAsync(int wordleId)
        {
            var wordles = await GetJsonFile<List<WordleData>>("wordles") ?? new List<WordleData>();

            if (wordles.Any(w => w.WordleId == wordleId))
                return;

            wordles.Add(new WordleData { WordleId = wordleId });
            await PutJsonFile("wordles", wordles);
        }

        private async Task AddWordleResultsAsync(WordleSubmissionData data)
        {
            var groupHistoryData = await GetJsonFile<List<WordleSubmissionData>>($"history") ?? new List<WordleSubmissionData>();
            var userHistoryData = await GetJsonFile<List<WordleSubmissionData>>($"history-{data.UserId}") ?? new List<WordleSubmissionData>();
            var statData = await GetJsonFile<List<WordleStatData>>($"stats-{data.UserId}") ?? new List<WordleStatData>();
            var leaderboardData = await GetJsonFile<List<WordleStatData>>("leaderboard") ?? new List<WordleStatData>();

            // Only one submission allowed for a user per day
            if (userHistoryData.Any(d => d.UserId == data.UserId && d.WordleId == data.WordleId))
                return;

            userHistoryData.Add(data);
            groupHistoryData.Add(data);

            // Update rolling average and leaderboard
            var recentEntries = userHistoryData
                .OrderByDescending(h => h.WordleId)
                .Take(10)
                .ToList();

            var stats = new WordleStatData {
                UserId = data.UserId,
                Average = recentEntries.Average(h => h.Attempts),
                Max = recentEntries.Max(h => h.Attempts),
                Min = recentEntries.Min(h => h.Attempts)
            };

            statData.Add(stats);

            leaderboardData.RemoveAll(w => w.UserId == data.UserId);
            leaderboardData.Add(stats);

            await Task.WhenAll(
                // Update combined history
                PutJsonFile("history", groupHistoryData),

                // Update last 10 history records
                PutJsonFile("history-top", groupHistoryData.OrderByDescending(h => h.WordleId).Take(10)),

                // Update history for user
                PutJsonFile($"history-{data.UserId}", userHistoryData),
                
                // Update history top 10 for user
                PutJsonFile($"history-top-{data.UserId}", recentEntries),

                // Update user stats
                PutJsonFile($"stats-{data.UserId}", statData),

                // Update leaderboard
                PutJsonFile("leaderboard", leaderboardData)
            );
        }

        private bool IsGuessMatrixValid(string guessMatrix)
        {
            var unicodeMatrix = new StringInfo(guessMatrix);

            if (unicodeMatrix.LengthInTextElements > 36)
                return false;

            for (var i = 0; i < unicodeMatrix.LengthInTextElements; i++)
            {
                var character = unicodeMatrix.SubstringByTextElements(i, 1);

                switch(character)
                {
                    case "⬜️":
                    case "🟨":
                    case "🟩":
                    case "⬛":
                    case "\n":
                        break;
                    default:
                        return false;
                }

            }

            return true;
        }

        private WordleSubmissionData? ParseMessage(long userId, string message)
        {
            var matchData = regex.Match(message);

            if (!matchData.Success)
                return null;

            var wordleId = short.Parse(matchData.Groups["WordleId"].Value);
            var attemptString = matchData.Groups["Attempts"].Value;
            var guessMatrix = matchData.Groups["GuessMatrix"].Value;

            if (!IsGuessMatrixValid(guessMatrix))
                return null;

            var success = true;

            if (!int.TryParse(attemptString, out var attempts))
            {
                success = false;
                attempts = 6;
            }

            return new WordleSubmissionData
            {
                UserId = userId,
                WordleId = wordleId,
                Success = success,
                Attempts = attempts,
                GuessMatrix = guessMatrix
            };
        }

        private async Task PutJsonFile<T>(string fileName, T data)
        {
            using (var memoryStream = new MemoryStream())
            {
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = $"{fileName}.json",
                    InputStream = memoryStream,
                    ContentType = "application/json",
                    CannedACL = S3CannedACL.PublicRead
                };

                await JsonSerializer.SerializeAsync(request.InputStream, data);
                await client.PutObjectAsync(request);
            }
        }

        private async Task<T?> GetJsonFile<T>(string fileName)
        {
            try
            {
                using var stream = await client.GetObjectStreamAsync(bucketName, $"{fileName}.json", null);
                return JsonSerializer.Deserialize<T>(stream)!;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound) throw;
            }

            return default;
        }
    }
}