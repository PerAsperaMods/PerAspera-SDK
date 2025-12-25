using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using YamlDotNet.Serialization;

namespace PerAspera.GameAPI.Database
{
    /// <summary>
    /// SQLite-based mod data storage system for Per Aspera
    /// Provides persistent storage, caching, and advanced querying for mod data
    /// </summary>
    public class ModDatabase
    {
        private static ModDatabase _instance;
        private static readonly object _lock = new object();
        private SQLiteConnection _connection;
        private readonly ManualLogSource _logger;

        // Database file path
        private readonly string _dbPath;

        /// <summary>
        /// Get the singleton instance of ModDatabase
        /// </summary>
        public static ModDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ModDatabase();
                    }
                }
                return _instance;
            }
        }

        private ModDatabase()
        {
            _logger = BepInEx.Logging.Logger.CreateLogSource("ModDatabase");

            // Database location: BepInEx/plugins/Database/
            var pluginsPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var dbDir = System.IO.Path.Combine(pluginsPath, "Database");
            Directory.CreateDirectory(dbDir);
            _dbPath = System.IO.Path.Combine(dbDir, "moddata.db");

            InitializeDatabase();
        }

        /// <summary>
        /// Initialize the SQLite database and create tables
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                _connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
                _connection.Open();

                // Create tables
                CreateTables();

                _logger.LogInfo($"✅ ModDatabase initialized at {_dbPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize ModDatabase: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create all necessary database tables
        /// </summary>
        private void CreateTables()
        {
            // Resources table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS resources (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL,
                    yaml_data TEXT NOT NULL,
                    mod_id TEXT,
                    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    checksum TEXT
                )");

            // Buildings table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS buildings (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL,
                    yaml_data TEXT NOT NULL,
                    mod_id TEXT,
                    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    checksum TEXT
                )");

            // Technologies table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS technologies (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL,
                    yaml_data TEXT NOT NULL,
                    mod_id TEXT,
                    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    checksum TEXT
                )");

            // Knowledge table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS knowledge (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL,
                    yaml_data TEXT NOT NULL,
                    mod_id TEXT,
                    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    checksum TEXT
                )");

            // Mod metadata table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS mod_metadata (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    mod_id TEXT UNIQUE NOT NULL,
                    version TEXT,
                    last_parsed DATETIME DEFAULT CURRENT_TIMESTAMP,
                    yaml_checksum TEXT
                )");

            // Create indexes for performance
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_resources_name ON resources(name)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_resources_mod ON resources(mod_id)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_mod_metadata_mod_id ON mod_metadata(mod_id)");
        }

        /// <summary>
        /// Execute a non-query SQL command
        /// </summary>
        private void ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using var cmd = new SQLiteCommand(sql, _connection);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Execute a query and return results
        /// </summary>
        private SQLiteDataReader ExecuteQuery(string sql, params SQLiteParameter[] parameters)
        {
            using var cmd = new SQLiteCommand(sql, _connection);
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            return cmd.ExecuteReader();
        }

        /// <summary>
        /// Store parsed YAML data in the database
        /// </summary>
        /// <param name="dataType">Type of data (resources, buildings, etc.)</param>
        /// <param name="modId">Mod identifier</param>
        /// <param name="yamlData">Parsed YAML data</param>
        /// <param name="checksum">Checksum of the source YAML</param>
        public void StoreYAMLData(string dataType, string modId, Dictionary<string, object> yamlData, string checksum = null)
        {
            try
            {
                var tableName = GetTableName(dataType);
                if (tableName == null) return;

                // Begin transaction for batch insert
                using var transaction = _connection.BeginTransaction();

                foreach (var kvp in yamlData)
                {
                    // Serialize the object data to JSON for storage
                    var yamlDataJson = SerializeToJson(kvp.Value);

                    // Insert or replace
                    ExecuteNonQuery($@"
                        INSERT OR REPLACE INTO {tableName} (name, yaml_data, mod_id, last_updated, checksum)
                        VALUES (@name, @yamlData, @modId, CURRENT_TIMESTAMP, @checksum)",
                        new SQLiteParameter("@name", kvp.Key),
                        new SQLiteParameter("@yamlData", yamlDataJson),
                        new SQLiteParameter("@modId", modId ?? "base"),
                        new SQLiteParameter("@checksum", checksum ?? ""));
                }

                // Update mod metadata
                ExecuteNonQuery(@"
                    INSERT OR REPLACE INTO mod_metadata (mod_id, last_parsed, yaml_checksum)
                    VALUES (@modId, CURRENT_TIMESTAMP, @checksum)",
                    new SQLiteParameter("@modId", modId ?? "base"),
                    new SQLiteParameter("@checksum", checksum ?? ""));

                transaction.Commit();

                _logger.LogInfo($"✅ Stored {yamlData.Count} {dataType} entries for mod '{modId}'");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to store YAML data: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieve stored YAML data
        /// </summary>
        /// <param name="dataType">Type of data to retrieve</param>
        /// <param name="modId">Mod identifier (null for all)</param>
        /// <returns>Dictionary of parsed data</returns>
        public Dictionary<string, object> RetrieveYAMLData(string dataType, string modId = null)
        {
            var result = new Dictionary<string, object>();
            var tableName = GetTableName(dataType);
            if (tableName == null) return result;

            try
            {
                string sql = $"SELECT name, yaml_data FROM {tableName}";
                var parameters = new List<SQLiteParameter>();

                if (!string.IsNullOrEmpty(modId))
                {
                    sql += " WHERE mod_id = @modId";
                    parameters.Add(new SQLiteParameter("@modId", modId));
                }

                using var reader = ExecuteQuery(sql, parameters.ToArray());
                while (reader.Read())
                {
                    var name = reader.GetString(0);
                    var yamlDataJson = reader.GetString(1);
                    var data = DeserializeFromJson(yamlDataJson);
                    result[name] = data;
                }

                _logger.LogInfo($"✅ Retrieved {result.Count} {dataType} entries");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to retrieve YAML data: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Check if data needs updating based on checksum
        /// </summary>
        public bool NeedsUpdate(string modId, string checksum)
        {
            try
            {
                using var reader = ExecuteQuery(
                    "SELECT yaml_checksum FROM mod_metadata WHERE mod_id = @modId",
                    new SQLiteParameter("@modId", modId ?? "base"));

                if (reader.Read())
                {
                    var storedChecksum = reader.GetString(0);
                    return storedChecksum != checksum;
                }

                return true; // No stored data, needs update
            }
            catch
            {
                return true; // Error, assume needs update
            }
        }

        /// <summary>
        /// Get table name for data type
        /// </summary>
        private string GetTableName(string dataType)
        {
            return dataType.ToLower() switch
            {
                "resources" or "resourcetype" => "resources",
                "buildings" or "buildingtype" => "buildings",
                "technologies" or "technology" => "technologies",
                "knowledge" => "knowledge",
                _ => null
            };
        }

        /// <summary>
        /// Serialize object to JSON for storage
        /// </summary>
        private string SerializeToJson(object data)
        {
            // Simple JSON serialization - could use Newtonsoft.Json if available
            return System.Text.Json.JsonSerializer.Serialize(data);
        }

        /// <summary>
        /// Deserialize object from JSON
        /// </summary>
        private object DeserializeFromJson(string json)
        {
            // Simple JSON deserialization
            return System.Text.Json.JsonSerializer.Deserialize<object>(json);
        }

        /// <summary>
        /// Clean up old data for a mod
        /// </summary>
        public void CleanupModData(string modId)
        {
            try
            {
                // Remove old data (keep last 10 versions or data older than 30 days)
                ExecuteNonQuery(@"
                    DELETE FROM resources WHERE mod_id = @modId AND id NOT IN (
                        SELECT id FROM resources WHERE mod_id = @modId
                        ORDER BY last_updated DESC LIMIT 10
                    )", new SQLiteParameter("@modId", modId));

                _logger.LogInfo($"🧹 Cleaned up old data for mod '{modId}'");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to cleanup mod data: {ex.Message}");
            }
        }

        /// <summary>
        /// Get database statistics
        /// </summary>
        public Dictionary<string, int> GetStats()
        {
            var stats = new Dictionary<string, int>();

            try
            {
                // Count records in each table
                var tables = new[] { "resources", "buildings", "technologies", "knowledge" };

                foreach (var table in tables)
                {
                    using var reader = ExecuteQuery($"SELECT COUNT(*) FROM {table}");
                    if (reader.Read())
                    {
                        stats[table] = reader.GetInt32(0);
                    }
                }

                stats["total_mods"] = GetModCount();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get stats: {ex.Message}");
            }

            return stats;
        }

        private int GetModCount()
        {
            try
            {
                using var reader = ExecuteQuery("SELECT COUNT(DISTINCT mod_id) FROM mod_metadata");
                return reader.Read() ? reader.GetInt32(0) : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Close database connection
        /// </summary>
        public void Close()
        {
            _connection?.Close();
            _instance = null;
        }
    }
}