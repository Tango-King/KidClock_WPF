namespace KidClock.Core.Models
{
    public record WeeklyReportItem(
        string GameKey,
        double Accuracy,
        int TotalQuestions,
        double AvgDurationSeconds,
        string TopMistakeTagsCsv
    );

    public record WeeklyReport(
        string FromUtcIso,
        string ToUtcIso,
        WeeklyReportItem[] Items
    );
}

