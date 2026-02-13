namespace KidClock.Core.Models
{
    public record GameSessionResult(
        string GameKey,
        string Mode,
        int DifficultyTier,
        int CorrectCount,
        int TotalCount,
        int DurationSeconds,
        int StarRating,
        string CreatedAtUtcIso,
        string MistakeTagsCsv
    );
}

