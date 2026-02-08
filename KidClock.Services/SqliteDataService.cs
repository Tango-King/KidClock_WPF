using KidClock.Core.Interfaces;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

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
    }
}