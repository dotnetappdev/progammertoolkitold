using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace ProgrammersToolKit.Database
{
    public static class DatabaseManager
    {
        private static readonly string DbPath = GetSecureDbPath();
        private static readonly string ConnectionString = $"Data Source={DbPath}";

        private static string GetSecureDbPath()
        {
            string dbName = "ProgrammersToolKit.db";
#if WINDOWS
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "ProgrammersToolKit", dbName);
#elif OSX
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(appData, "Library", "Application Support", "ProgrammersToolKit", dbName);
#else
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "ProgrammersToolKit", dbName);
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
                CREATE TABLE IF NOT EXISTS ApiCalls (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Method TEXT,
                    Url TEXT,
                    Headers TEXT,
                    JsonBody TEXT,
                    ExpectedStatus INTEGER,
                    ExpectedJson TEXT,
                    ExpectedXml TEXT
                );
                CREATE TABLE IF NOT EXISTS CtfFavorites (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Type TEXT NOT NULL, -- 'jwt', 'hash', 'headers'
                    Name TEXT,
                    Value TEXT,
                    Created DATETIME DEFAULT CURRENT_TIMESTAMP
                );
            ";
            cmd.ExecuteNonQuery();
        }

        // CTF Favorites CRUD
        public static void SaveCtfFavorite(string type, string name, string value)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO CtfFavorites (Type, Name, Value) VALUES ($type, $name, $value)";
            cmd.Parameters.AddWithValue("$type", type);
            cmd.Parameters.AddWithValue("$name", name);
            cmd.Parameters.AddWithValue("$value", value);
            cmd.ExecuteNonQuery();
        }

        public static List<(int Id, string Type, string Name, string Value)> GetCtfFavorites(string type)
        {
            var list = new List<(int, string, string, string)>();
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Type, Name, Value FROM CtfFavorites WHERE Type = $type ORDER BY Created DESC";
            cmd.Parameters.AddWithValue("$type", type);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add((reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
            }
            return list;
        }

        public static void DeleteCtfFavorite(int id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM CtfFavorites WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // API Tester CRUD
        public static void SaveApiCall(object callObj)
        {
            // Accepts ApiTesterView.ApiCall
            dynamic call = callObj;
            string headersJson = System.Text.Json.JsonSerializer.Serialize(call.Headers);
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            if (call.Id > 0)
            {
                cmd.CommandText = @"UPDATE ApiCalls SET Method=$m, Url=$u, Headers=$h, JsonBody=$b, ExpectedStatus=$es, ExpectedJson=$ej, ExpectedXml=$ex WHERE Id=$id";
                cmd.Parameters.AddWithValue("$id", call.Id);
            }
            else
            {
                cmd.CommandText = @"INSERT INTO ApiCalls (Method, Url, Headers, JsonBody, ExpectedStatus, ExpectedJson, ExpectedXml) VALUES ($m, $u, $h, $b, $es, $ej, $ex)";
            }
            cmd.Parameters.AddWithValue("$m", call.Method);
            cmd.Parameters.AddWithValue("$u", call.Url);
            cmd.Parameters.AddWithValue("$h", headersJson);
            cmd.Parameters.AddWithValue("$b", call.JsonBody);
            cmd.Parameters.AddWithValue("$es", (object?)call.ExpectedStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$ej", (object?)call.ExpectedJson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$ex", (object?)call.ExpectedXml ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteApiCall(int id)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM ApiCalls WHERE Id = $id";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        public static System.Collections.Generic.List<object> GetApiCalls()
        {
            var list = new System.Collections.Generic.List<object>();
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Method, Url, Headers, JsonBody, ExpectedStatus, ExpectedJson, ExpectedXml FROM ApiCalls ORDER BY Id DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var method = reader.GetString(1);
                var url = reader.GetString(2);
                var headersJson = reader.GetString(3);
                var jsonBody = reader.GetString(4);
                var expectedStatus = !reader.IsDBNull(5) ? reader.GetInt32(5) : (int?)null;
                var expectedJson = !reader.IsDBNull(6) ? reader.GetString(6) : null;
                var expectedXml = !reader.IsDBNull(7) ? reader.GetString(7) : null;
                var headers = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<dynamic>>(headersJson) ?? new System.Collections.Generic.List<dynamic>();
                list.Add(new {
                    Id = id,
                    Method = method,
                    Url = url,
                    Headers = headers,
                    JsonBody = jsonBody,
                    ExpectedStatus = expectedStatus,
                    ExpectedJson = expectedJson,
                    ExpectedXml = expectedXml
                });
            }
            return list;
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
    }
}
