using KidClock.Core.Models;
using KidClock.Core.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KidClock.Services
{
    public class SqliteDataService : IDataService
    {
        private const string DbName = "kidclock.db";
        private string _connectionString;

        public SqliteDataService()
        {
            string dbPath = Path.Combine(Environment.CurrentDirectory, DbName);
            _connectionString = $"Data Source={dbPath}";
        }

        public void Initialize()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = 
                @"
                    CREATE TABLE IF NOT EXISTS Settings (
                        Key TEXT PRIMARY KEY,
                        Value TEXT
                    );

                    CREATE TABLE IF NOT EXISTS GameSessionResults (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        GameKey TEXT NOT NULL,
                        Mode TEXT NOT NULL,
                        DifficultyTier INTEGER NOT NULL,
                        CorrectCount INTEGER NOT NULL,
                        TotalCount INTEGER NOT NULL,
                        DurationSeconds INTEGER NOT NULL,
                        StarRating INTEGER NOT NULL,
                        CreatedAtUtcIso TEXT NOT NULL,
                        MistakeTagsCsv TEXT NOT NULL
                    );
                ";
                command.ExecuteNonQuery();
            }
        }

        public void SaveSetting(string key, string value)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = 
                @"
                    INSERT OR REPLACE INTO Settings (Key, Value)
                    VALUES ($key, $value);
                ";
                command.Parameters.AddWithValue("$key", key);
                command.Parameters.AddWithValue("$value", value);
                command.ExecuteNonQuery();
            }
        }

        public string GetSetting(string key, string defaultValue = "")
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = 
                @"
                    SELECT Value FROM Settings WHERE Key = $key;
                ";
                command.Parameters.AddWithValue("$key", key);
                
                var result = command.ExecuteScalar();
                return result != null ? result.ToString() : defaultValue;
            }
        }

        public void SaveGameSession(GameSessionResult result)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO GameSessionResults
                    (GameKey, Mode, DifficultyTier, CorrectCount, TotalCount, DurationSeconds, StarRating, CreatedAtUtcIso, MistakeTagsCsv)
                    VALUES ($gameKey, $mode, $difficultyTier, $correctCount, $totalCount, $durationSeconds, $starRating, $createdAtUtcIso, $mistakeTagsCsv);
                ";
                command.Parameters.AddWithValue("$gameKey", result.GameKey);
                command.Parameters.AddWithValue("$mode", result.Mode);
                command.Parameters.AddWithValue("$difficultyTier", result.DifficultyTier);
                command.Parameters.AddWithValue("$correctCount", result.CorrectCount);
                command.Parameters.AddWithValue("$totalCount", result.TotalCount);
                command.Parameters.AddWithValue("$durationSeconds", result.DurationSeconds);
                command.Parameters.AddWithValue("$starRating", result.StarRating);
                command.Parameters.AddWithValue("$createdAtUtcIso", result.CreatedAtUtcIso);
                command.Parameters.AddWithValue("$mistakeTagsCsv", result.MistakeTagsCsv ?? "");
                command.ExecuteNonQuery();
            }
        }

        public GameSessionResult? GetBestRecord(string gameKey, string mode)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT GameKey, Mode, DifficultyTier, CorrectCount, TotalCount, DurationSeconds, StarRating, CreatedAtUtcIso, MistakeTagsCsv
                    FROM GameSessionResults
                    WHERE GameKey = $gameKey AND Mode = $mode
                    ORDER BY
                        StarRating DESC,
                        CASE WHEN TotalCount = 0 THEN 0 ELSE (CAST(CorrectCount AS REAL) / CAST(TotalCount AS REAL)) END DESC,
                        TotalCount DESC,
                        DurationSeconds ASC,
                        CreatedAtUtcIso DESC
                    LIMIT 1;
                ";
                command.Parameters.AddWithValue("$gameKey", gameKey);
                command.Parameters.AddWithValue("$mode", mode);

                using var reader = command.ExecuteReader();
                if (!reader.Read()) return null;

                return new GameSessionResult(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    reader.GetInt32(3),
                    reader.GetInt32(4),
                    reader.GetInt32(5),
                    reader.GetInt32(6),
                    reader.GetString(7),
                    reader.GetString(8)
                );
            }
        }

        public WeeklyReport GetWeeklyReport(string fromUtcIso, string toUtcIso)
        {
            var sessions = GetSessionsInRange(fromUtcIso, toUtcIso);

            var items = sessions
                .GroupBy(s => s.GameKey)
                .Select(g =>
                {
                    int correct = g.Sum(x => x.CorrectCount);
                    int total = g.Sum(x => x.TotalCount);
                    double accuracy = total <= 0 ? 0 : (double)correct / total;
                    double avgDuration = g.Any() ? g.Average(x => (double)x.DurationSeconds) : 0;

                    var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (var s in g)
                    {
                        var parts = (s.MistakeTagsCsv ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var p in parts)
                        {
                            var key = p.Trim();
                            if (key.Length == 0) continue;
                            tagCounts[key] = tagCounts.TryGetValue(key, out var v) ? v + 1 : 1;
                        }
                    }

                    string topTags = string.Join(",",
                        tagCounts
                            .OrderByDescending(kv => kv.Value)
                            .ThenBy(kv => kv.Key)
                            .Take(5)
                            .Select(kv => kv.Key));

                    return new WeeklyReportItem(
                        g.Key,
                        accuracy,
                        total,
                        avgDuration,
                        topTags
                    );
                })
                .OrderBy(i => i.GameKey)
                .ToArray();

            return new WeeklyReport(fromUtcIso, toUtcIso, items);
        }

        private List<GameSessionResult> GetSessionsInRange(string fromUtcIso, string toUtcIso)
        {
            var results = new List<GameSessionResult>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT GameKey, Mode, DifficultyTier, CorrectCount, TotalCount, DurationSeconds, StarRating, CreatedAtUtcIso, MistakeTagsCsv
                    FROM GameSessionResults
                    WHERE CreatedAtUtcIso >= $fromUtcIso AND CreatedAtUtcIso <= $toUtcIso;
                ";
                command.Parameters.AddWithValue("$fromUtcIso", fromUtcIso);
                command.Parameters.AddWithValue("$toUtcIso", toUtcIso);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new GameSessionResult(
                        reader.GetString(0),
                        reader.GetString(1),
                        reader.GetInt32(2),
                        reader.GetInt32(3),
                        reader.GetInt32(4),
                        reader.GetInt32(5),
                        reader.GetInt32(6),
                        reader.GetString(7),
                        reader.GetString(8)
                    ));
                }
            }

            return results;
        }
    }
}
