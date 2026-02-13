using KidClock.Core.Interfaces;
using KidClock.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KidClock.Tests.TestHelpers
{
    public class FakeDataService : IDataService
    {
        private readonly Dictionary<string, string> _settings = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<GameSessionResult> _sessions = new();

        public void Initialize() { }

        public void SaveSetting(string key, string value)
        {
            _settings[key] = value;
        }

        public string GetSetting(string key, string defaultValue = "")
        {
            return _settings.TryGetValue(key, out var v) ? v : defaultValue;
        }

        public void SaveGameSession(GameSessionResult result)
        {
            _sessions.Add(result);
        }

        public GameSessionResult? GetBestRecord(string gameKey, string mode)
        {
            return _sessions
                .Where(s => s.GameKey == gameKey && s.Mode == mode)
                .OrderByDescending(s => s.StarRating)
                .ThenByDescending(s => s.TotalCount <= 0 ? 0 : (double)s.CorrectCount / s.TotalCount)
                .ThenByDescending(s => s.TotalCount)
                .ThenBy(s => s.DurationSeconds)
                .FirstOrDefault();
        }

        public WeeklyReport GetWeeklyReport(string fromUtcIso, string toUtcIso)
        {
            var from = DateTime.Parse(fromUtcIso, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var to = DateTime.Parse(toUtcIso, null, System.Globalization.DateTimeStyles.RoundtripKind);

            var list = _sessions
                .Where(s =>
                {
                    var t = DateTime.Parse(s.CreatedAtUtcIso, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    return t >= from && t <= to;
                })
                .ToList();

            var items = list
                .GroupBy(s => s.GameKey)
                .Select(g =>
                {
                    int correct = g.Sum(x => x.CorrectCount);
                    int total = g.Sum(x => x.TotalCount);
                    double acc = total <= 0 ? 0 : (double)correct / total;
                    double avgDur = g.Any() ? g.Average(x => (double)x.DurationSeconds) : 0;

                    var tags = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (var s in g)
                    {
                        foreach (var t in (s.MistakeTagsCsv ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var key = t.Trim();
                            if (key.Length == 0) continue;
                            tags[key] = tags.TryGetValue(key, out var v) ? v + 1 : 1;
                        }
                    }
                    string top = string.Join(",", tags.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).Take(5).Select(kv => kv.Key));

                    return new WeeklyReportItem(g.Key, acc, total, avgDur, top);
                })
                .ToArray();

            return new WeeklyReport(fromUtcIso, toUtcIso, items);
        }
    }
}

