namespace KidClock.Core.Interfaces
{
    public interface IDataService
    {
        void Initialize();
        void SaveSetting(string key, string value);
        string GetSetting(string key, string defaultValue = "");
    }
}