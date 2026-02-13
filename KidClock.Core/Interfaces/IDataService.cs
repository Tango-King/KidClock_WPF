using KidClock.Core.Models;

namespace KidClock.Core.Interfaces
{
    public interface IDataService
    {
        void Initialize();
        void SaveSetting(string key, string value);
        string GetSetting(string key, string defaultValue = "");

        void SaveGameSession(GameSessionResult result);
        GameSessionResult? GetBestRecord(string gameKey, string mode);
        WeeklyReport GetWeeklyReport(string fromUtcIso, string toUtcIso);
    }
}
