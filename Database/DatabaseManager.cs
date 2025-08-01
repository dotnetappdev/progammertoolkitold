using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace screenshareav.Database
{
    public static class DatabaseManager
    {
        private static readonly string DbPath = GetSecureDbPath();
        private static readonly string ConnectionString = $"Data Source={DbPath}";

        private static string GetSecureDbPath()
        {
            string dbName = "screenshareav.db";
#if WINDOWS
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "ScreenShareAV", dbName);
#elif OSX
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(appData, "Library", "Application Support", "ScreenShareAV", dbName);
#else
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "ScreenShareAV", dbName);
#endif
        }

        static DatabaseManager()
        {
            // Always ensure the directory exists
            var dbDir = Path.GetDirectoryName(DbPath);
            if (!Directory.Exists(dbDir))
                Directory.CreateDirectory(dbDir!);
            if (!File.Exists(DbPath))
                InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            var dbDir = Path.GetDirectoryName(DbPath);
            if (!Directory.Exists(dbDir))
                Directory.CreateDirectory(dbDir!);
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                );
                CREATE TABLE IF NOT EXISTS AccessCodes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Code TEXT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                CREATE TABLE IF NOT EXISTS RdpConnections (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT,
                    IpAddress TEXT,
                    Username TEXT,
                    PasswordEnc TEXT,
                    LastUsed DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                CREATE TABLE IF NOT EXISTS ApiTests (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT UNIQUE,
                    TestData TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                CREATE TABLE IF NOT EXISTS GeneralData (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Category TEXT,
                    Key TEXT,
                    Value TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE(Category, Key)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // Simple XOR encryption for demonstration (replace with DPAPI or platform crypto for production)
        private static string Encrypt(string plain, string key)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(plain);
            var k = System.Text.Encoding.UTF8.GetBytes(key);
            for (int i = 0; i < data.Length; i++)
                data[i] ^= k[i % k.Length];
            return Convert.ToBase64String(data);
        }
        private static string Decrypt(string enc, string key)
        {
            var data = Convert.FromBase64String(enc);
            var k = System.Text.Encoding.UTF8.GetBytes(key);
            for (int i = 0; i < data.Length; i++)
                data[i] ^= k[i % k.Length];
            return System.Text.Encoding.UTF8.GetString(data);
        }

        public static void SaveRdpConnection(string name, string ip, string username, string password, string encKey)
        {
            var encPwd = Encrypt(password, encKey);
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO RdpConnections (Name, IpAddress, Username, PasswordEnc) VALUES ($name, $ip, $username, $pwd)";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$ip", ip);
            cmd.Parameters.AddWithValue("$username", username);
            cmd.Parameters.AddWithValue("$pwd", encPwd);
            cmd.ExecuteNonQuery();
        }

        public static List<(string Name, string Ip, string Username, string Password)> GetRdpConnections(string encKey)
        {
            var list = new List<(string, string, string, string)>();
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Name, IpAddress, Username, PasswordEnc FROM RdpConnections";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                var ip = reader.GetString(1);
                var user = reader.GetString(2);
                var pwd = Decrypt(reader.GetString(3), encKey);
                list.Add((name, ip, user, pwd));
            }
            return list;
        }

        public static void DeleteRdpConnection(string ip)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM RdpConnections WHERE IpAddress = $ip";
            cmd.Parameters.AddWithValue("$ip", ip);
            cmd.ExecuteNonQuery();
        }

        public static void SetSetting(string key, string value)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Settings (Key, Value) VALUES ($key, $value)
                ON CONFLICT(Key) DO UPDATE SET Value = $value;";
            cmd.Parameters.AddWithValue("$key", key);
            cmd.Parameters.AddWithValue("$value", value);
            cmd.ExecuteNonQuery();
        }

        public static string? GetSetting(string key)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Value FROM Settings WHERE Key = $key";
            cmd.Parameters.AddWithValue("$key", key);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader.GetString(0) : null;
        }

        public static string GenerateAndStoreAccessCode()
        {
            var code = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO AccessCodes (Code) VALUES ($code)";
            cmd.Parameters.AddWithValue("$code", code);
            cmd.ExecuteNonQuery();
            return code;
        }

        public static string? GetLatestAccessCode()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Code FROM AccessCodes ORDER BY Timestamp DESC LIMIT 1";
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader.GetString(0) : null;
        }

        // API Test Management
        public static void SaveApiTest(string name, string testData)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO ApiTests (Name, TestData) VALUES ($name, $data)
                ON CONFLICT(Name) DO UPDATE SET TestData = $data, UpdatedAt = CURRENT_TIMESTAMP;";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$data", testData);
            cmd.ExecuteNonQuery();
        }

        public static string? GetApiTest(string name)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TestData FROM ApiTests WHERE Name = $name";
            cmd.Parameters.AddWithValue("$name", name);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader.GetString(0) : null;
        }

        public static List<string> GetApiTestNames()
        {
            var names = new List<string>();
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Name FROM ApiTests ORDER BY UpdatedAt DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                names.Add(reader.GetString(0));
            }
            return names;
        }

        public static void DeleteApiTest(string name)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM ApiTests WHERE Name = $name";
            cmd.Parameters.AddWithValue("$name", name);
            cmd.ExecuteNonQuery();
        }

        // General CRUD Operations
        public static void SaveData(string category, string key, string value)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO GeneralData (Category, Key, Value) VALUES ($category, $key, $value)
                ON CONFLICT(Category, Key) DO UPDATE SET Value = $value, UpdatedAt = CURRENT_TIMESTAMP;";
            cmd.Parameters.AddWithValue("$category", category);
            cmd.Parameters.AddWithValue("$key", key);
            cmd.Parameters.AddWithValue("$value", value);
            cmd.ExecuteNonQuery();
        }

        public static string? GetData(string category, string key)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Value FROM GeneralData WHERE Category = $category AND Key = $key";
            cmd.Parameters.AddWithValue("$category", category);
            cmd.Parameters.AddWithValue("$key", key);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader.GetString(0) : null;
        }

        public static void DeleteData(string category, string key)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM GeneralData WHERE Category = $category AND Key = $key";
            cmd.Parameters.AddWithValue("$category", category);
            cmd.Parameters.AddWithValue("$key", key);
            cmd.ExecuteNonQuery();
        }
    }
}
