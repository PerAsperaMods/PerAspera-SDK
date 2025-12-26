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
        /// Create all necessary database tables with improved structure
        /// </summary>
        private void CreateTables()
        {
            // Resources table with indexed properties for robust queries
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS resources (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL,
                    display_name TEXT,
                    category TEXT,
                    is_atmospheric BOOLEAN DEFAULT 0,
                    atmospheric_priority INTEGER DEFAULT 0,
                    yaml_data TEXT NOT NULL,
                    yaml_checksum TEXT NOT NULL,
                    schema_version INTEGER DEFAULT 1,
                    mod_id TEXT,
                    is_native BOOLEAN DEFAULT 0,
                    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    validation_status TEXT DEFAULT 'pending'
                )");

            // Buildings table with indexed properties
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS buildings (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL,
                    display_name TEXT,
                    category TEXT,
                    building_type TEXT,
                    energy_production REAL,
                    yaml_data TEXT NOT NULL,
                    yaml_checksum TEXT NOT NULL,
                    schema_version INTEGER DEFAULT 1,
                    mod_id TEXT,
                    is_native BOOLEAN DEFAULT 0,
                    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    validation_status TEXT DEFAULT 'pending'
                )");

            // Technologies table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS technologies (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL,
                    display_name TEXT,
                    category TEXT,
                    yaml_data TEXT NOT NULL,
                    yaml_checksum TEXT NOT NULL,
                    schema_version INTEGER DEFAULT 1,
                    mod_id TEXT,
                    is_native BOOLEAN DEFAULT 0,
                    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    validation_status TEXT DEFAULT 'pending'
                )");

            // Knowledge table
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS knowledge (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT UNIQUE NOT NULL,
                    display_name TEXT,
                    category TEXT,
                    yaml_data TEXT NOT NULL,
                    yaml_checksum TEXT NOT NULL,
                    schema_version INTEGER DEFAULT 1,
                    mod_id TEXT,
                    is_native BOOLEAN DEFAULT 0,
                    last_updated DATETIME DEFAULT CURRENT_TIMESTAMP,
                    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    validation_status TEXT DEFAULT 'pending'
                )");

            // Mod metadata table with versioning
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS mod_metadata (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    mod_id TEXT UNIQUE NOT NULL,
                    version TEXT,
                    schema_version INTEGER DEFAULT 1,
                    last_parsed DATETIME DEFAULT CURRENT_TIMESTAMP,
                    yaml_checksum TEXT,
                    data_types TEXT, -- JSON array of data types provided by this mod
                    validation_status TEXT DEFAULT 'pending'
                )");

            // Schema versions table for migration tracking
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS schema_versions (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    data_type TEXT NOT NULL,
                    version INTEGER NOT NULL,
                    applied_at DATETIME DEFAULT CURRENT_TIMESTAMP,
                    description TEXT,
                    UNIQUE(data_type, version)
                )");

            // Create comprehensive indexes for performance
            CreateIndexes();
        }

        /// <summary>
        /// Create indexes for optimal query performance
        /// </summary>
        private void CreateIndexes()
        {
            // Resources indexes
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_resources_name ON resources(name)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_resources_category ON resources(category)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_resources_mod ON resources(mod_id)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_resources_checksum ON resources(yaml_checksum)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_resources_validation ON resources(validation_status)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_resources_native ON resources(is_native)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_resources_atmospheric ON resources(is_atmospheric)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_resources_atmospheric_priority ON resources(atmospheric_priority)");

            // Buildings indexes
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_buildings_name ON buildings(name)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_buildings_category ON buildings(category)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_buildings_type ON buildings(building_type)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_buildings_energy ON buildings(energy_production)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_buildings_mod ON buildings(mod_id)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_buildings_validation ON buildings(validation_status)");

            // Technologies indexes
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_technologies_name ON technologies(name)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_technologies_category ON technologies(category)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_technologies_mod ON technologies(mod_id)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_technologies_validation ON technologies(validation_status)");

            // Knowledge indexes
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_knowledge_name ON knowledge(name)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_knowledge_category ON knowledge(category)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_knowledge_mod ON knowledge(mod_id)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_knowledge_validation ON knowledge(validation_status)");

            // Mod metadata indexes
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_mod_metadata_mod_id ON mod_metadata(mod_id)");
            ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_mod_metadata_checksum ON mod_metadata(yaml_checksum)");
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
        /// Store parsed YAML data in the database with enhanced validation and indexing
        /// </summary>
        /// <param name="dataType">Type of data (resources, buildings, etc.)</param>
        /// <param name="modId">Mod identifier</param>
        /// <param name="yamlData">Parsed YAML data</param>
        /// <param name="checksum">Checksum of the source YAML</param>
        /// <param name="isNative">Whether this is native game data</param>
        public void StoreYAMLData(string dataType, string modId, Dictionary<string, object> yamlData, string checksum, bool isNative = false)
        {
            try
            {
                var tableName = GetTableName(dataType);
                if (tableName == null) return;

                // Begin transaction for batch insert
                using var transaction = _connection.BeginTransaction();

                foreach (var kvp in yamlData)
                {
                    // Validate and extract properties for indexing
                    var validationResult = ValidateAndExtractProperties(dataType, kvp.Key, kvp.Value);
                    if (!validationResult.IsValid)
                    {
                        _logger.LogWarning($"Skipping invalid {dataType} entry '{kvp.Key}': {validationResult.ErrorMessage}");
                        continue;
                    }

                    // Serialize the object data to JSON for storage
                    var yamlDataJson = SerializeToJson(kvp.Value);

                    // Insert or replace with enhanced properties
                    var sql = BuildInsertSQL(tableName, validationResult.Properties);
                    var parameters = BuildInsertParameters(kvp.Key, yamlDataJson, modId ?? "base", checksum, isNative, validationResult.Properties);

                    ExecuteNonQuery(sql, parameters.ToArray());
                }

                // Update mod metadata with enhanced information
                UpdateModMetadata(modId ?? "base", checksum, dataType, yamlData.Count);

                transaction.Commit();

                _logger.LogInfo($"✅ Stored {yamlData.Count} validated {dataType} entries for mod '{modId}'");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to store YAML data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validation result for YAML data entries
        /// </summary>
        private class ValidationResult
        {
            public bool IsValid { get; set; } = true;
            public string ErrorMessage { get; set; } = "";
            public Dictionary<string, object> Properties { get; set; } = new();
        }

        /// <summary>
        /// Validate YAML data entry and extract properties for indexing
        /// </summary>
        private ValidationResult ValidateAndExtractProperties(string dataType, string key, object data)
        {
            var result = new ValidationResult();

            try
            {
                // Basic validation - ensure data is a dictionary
                if (!(data is Dictionary<object, object> dict))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Data must be a dictionary";
                    return result;
                }

                // Extract common properties based on data type
                switch (dataType.ToLower())
                {
                    case "resources":
                        ExtractResourceProperties(dict, result.Properties);
                        break;
                    case "buildings":
                        ExtractBuildingProperties(dict, result.Properties);
                        break;
                    case "technologies":
                        ExtractTechnologyProperties(dict, result.Properties);
                        break;
                    case "knowledge":
                        ExtractKnowledgeProperties(dict, result.Properties);
                        break;
                }

                // Validate required fields
                if (!result.Properties.ContainsKey("display_name") || string.IsNullOrEmpty(result.Properties["display_name"]?.ToString()))
                {
                    result.Properties["display_name"] = key; // Fallback to key
                }

                result.IsValid = true;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Validation error: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Extract properties specific to resources for indexing
        /// </summary>
        private void ExtractResourceProperties(Dictionary<object, object> dict, Dictionary<string, object> properties)
        {
            // Common properties
            ExtractCommonProperties(dict, properties);

            // Resource-specific properties
            if (dict.TryGetValue("category", out var category))
                properties["category"] = category?.ToString() ?? "";

            // ATMOSPHERIC RESOURCE DETECTION
            // Automatically detect atmospheric resources based on naming patterns
            DetectAtmosphericProperties(dict, properties);
        }

        /// <summary>
        /// Detect and set atmospheric properties for resources
        /// This automatically identifies real atmospheric gases vs derived/calculated resources
        /// Enhanced to support mod-added atmospheric resources like MoreResources mod
        /// </summary>
        private void DetectAtmosphericProperties(Dictionary<object, object> dict, Dictionary<string, object> properties)
        {
            // Get resource name for pattern matching
            var resourceName = dict.TryGetValue("name", out var nameObj) ? nameObj?.ToString() ?? "" : "";
            var materialType = dict.TryGetValue("materialType", out var matObj) ? matObj?.ToString() ?? "" : "";
            var category = dict.TryGetValue("category", out var catObj) ? catObj?.ToString() ?? "" : "";

            // REAL ATMOSPHERIC GASES (native game atmosphere composition)
            var realAtmosphericGases = new Dictionary<string, int>
            {
                // Primary atmospheric gases - highest priority for display
                ["resource_oxygen_release"] = 100,      // O2 in atmosphere
                ["resource_carbon_dioxide_release"] = 90, // CO2 in atmosphere
                ["resource_nitrogen_release"] = 80,     // N2 in atmosphere
                ["resource_ghg_release"] = 70,          // Greenhouse gases

                // Additional real atmospheric gases (if they exist)
                ["resource_methane_release"] = 60,      // CH4
                ["resource_argon_release"] = 50,        // Ar
                ["resource_water_vapor_release"] = 40,  // H2O vapor
            };

            // MOD-ADDED ATMOSPHERIC RESOURCES
            // Resources added by mods that should be considered atmospheric
            var modAtmosphericGases = new Dictionary<string, int>
            {
                // MoreResources mod additions
                ["resource_co2"] = 65,                  // CO2 from MoreResources
                ["resource_co2_deep"] = 55,             // Deep CO2 from MoreResources

                // Future mod resources can be added here
                // Pattern: ["resource_name"] = priority (1-99, lower than natives)
            };

            // DERIVED/CALCULATED RESOURCES (used by buildings, not real atmosphere)
            var derivedResources = new HashSet<string>
            {
                // Oxygen derivatives
                "resource_O2_Up", "resource_O2_Down",
                "resource_oxygen_capture", "resource_oxygen_respiration",

                // Carbon dioxide derivatives
                "resource_CO2_Up", "resource_CO2_Down",
                "resource_carbon_dioxide_capture", "resource_carbon_dioxide_production",

                // Nitrogen derivatives
                "resource_N2_Up", "resource_N2_Down",
                "resource_nitrogen_capture", "resource_nitrogen_fixation",

                // Energy and other calculated resources
                "resource_energy", "resource_power",
                "resource_heat", "resource_cold",

                // Water derivatives
                "resource_water_purified", "resource_water_contaminated",
                "resource_water_hot", "resource_water_cold",

                // Generic up/down patterns (catch-all for building I/O)
                // These are typically building input/output resources
            };

            // Check if this is a real atmospheric gas (native)
            if (realAtmosphericGases.TryGetValue(resourceName, out var priority))
            {
                properties["is_atmospheric"] = true;
                properties["atmospheric_priority"] = priority;
                _logger.LogDebug($"✅ Marked {resourceName} as NATIVE atmospheric gas (priority: {priority})");
            }
            // Check if this is a mod-added atmospheric gas
            else if (modAtmosphericGases.TryGetValue(resourceName, out priority))
            {
                properties["is_atmospheric"] = true;
                properties["atmospheric_priority"] = priority;
                _logger.LogDebug($"🆕 Marked {resourceName} as MOD atmospheric gas (priority: {priority})");
            }
            // Check if this is a derived resource (should NOT be displayed in atmosphere)
            else if (derivedResources.Contains(resourceName) ||
                     resourceName.Contains("_Up") ||
                     resourceName.Contains("_Down") ||
                     resourceName.Contains("_capture") ||
                     resourceName.Contains("_production") ||
                     resourceName.Contains("_respiration") ||
                     resourceName.Contains("_fixation"))
            {
                properties["is_atmospheric"] = false;
                properties["atmospheric_priority"] = 0;
                _logger.LogDebug($"🚫 Marked {resourceName} as derived resource (not atmospheric)");
            }
            // Auto-detection based on material type and category
            else
            {
                var isAtmospheric = false;
                var autoPriority = 0;

                // Check material type
                if (materialType.ToLower() == "gases")
                {
                    isAtmospheric = true;
                    autoPriority = 20; // Medium priority for gas-type resources
                    _logger.LogDebug($"💨 Auto-detected {resourceName} as atmospheric (materialType: Gases)");
                }
                // Check category
                else if (category.ToLower().Contains("atmospher") ||
                        category.ToLower().Contains("gas") ||
                        category.ToLower().Contains("air"))
                {
                    isAtmospheric = true;
                    autoPriority = 15; // Lower priority for category-based detection
                    _logger.LogDebug($"🌬️ Auto-detected {resourceName} as atmospheric (category: {category})");
                }
                // Check name patterns for atmospheric gases
                else if (resourceName.Contains("oxygen") ||
                        resourceName.Contains("carbon") ||
                        resourceName.Contains("nitrogen") ||
                        resourceName.Contains("methane") ||
                        resourceName.Contains("argon") ||
                        resourceName.Contains("helium") ||
                        resourceName.Contains("vapor"))
                {
                    // Additional check - if it has "_release" it's likely atmospheric
                    if (resourceName.Contains("_release"))
                    {
                        isAtmospheric = true;
                        autoPriority = 25; // Higher priority for release-pattern resources
                        _logger.LogDebug($"🔄 Marked {resourceName} as atmospheric (release pattern, priority: {autoPriority})");
                    }
                    else
                    {
                        // Could be atmospheric, but lower confidence
                        isAtmospheric = true;
                        autoPriority = 10; // Low priority - needs verification
                        _logger.LogDebug($"❓ Marked {resourceName} as potentially atmospheric (name pattern, priority: {autoPriority})");
                    }
                }

                properties["is_atmospheric"] = isAtmospheric;
                properties["atmospheric_priority"] = autoPriority;
            }
        }

        /// <summary>
        /// Extract properties specific to buildings for indexing
        /// </summary>
        private void ExtractBuildingProperties(Dictionary<object, object> dict, Dictionary<string, object> properties)
        {
            // Common properties
            ExtractCommonProperties(dict, properties);

            // Building-specific properties
            if (dict.TryGetValue("category", out var category))
                properties["category"] = category?.ToString() ?? "";

            if (dict.TryGetValue("buildingType", out var buildingType))
                properties["building_type"] = buildingType?.ToString() ?? "";

            // Try to extract energy production for indexing
            if (dict.TryGetValue("energyProduction", out var energyProd) && double.TryParse(energyProd?.ToString(), out var energy))
                properties["energy_production"] = energy;
        }

        /// <summary>
        /// Extract properties specific to technologies for indexing
        /// </summary>
        private void ExtractTechnologyProperties(Dictionary<object, object> dict, Dictionary<string, object> properties)
        {
            // Common properties
            ExtractCommonProperties(dict, properties);

            // Technology-specific properties
            if (dict.TryGetValue("category", out var category))
                properties["category"] = category?.ToString() ?? "";
        }

        /// <summary>
        /// Extract properties specific to knowledge for indexing
        /// </summary>
        private void ExtractKnowledgeProperties(Dictionary<object, object> dict, Dictionary<string, object> properties)
        {
            // Common properties
            ExtractCommonProperties(dict, properties);

            // Knowledge-specific properties
            if (dict.TryGetValue("category", out var category))
                properties["category"] = category?.ToString() ?? "";
        }

        /// <summary>
        /// Extract common properties shared across all data types
        /// </summary>
        private void ExtractCommonProperties(Dictionary<object, object> dict, Dictionary<string, object> properties)
        {
            // Display name
            if (dict.TryGetValue("displayName", out var displayName))
                properties["display_name"] = displayName?.ToString() ?? "";
            else if (dict.TryGetValue("name", out var name))
                properties["display_name"] = name?.ToString() ?? "";
        }

        /// <summary>
        /// Build INSERT SQL statement with appropriate columns
        /// </summary>
        private string BuildInsertSQL(string tableName, Dictionary<string, object> properties)
        {
            var columns = new List<string> { "name", "yaml_data", "yaml_checksum", "mod_id", "is_native", "validation_status" };
            var values = new List<string> { "@name", "@yamlData", "@checksum", "@modId", "@isNative", "'valid'" };

            // Add type-specific columns
            switch (tableName)
            {
                case "resources":
                    if (properties.ContainsKey("category"))
                    {
                        columns.Add("category");
                        values.Add("@category");
                    }
                    break;
                case "buildings":
                    if (properties.ContainsKey("category"))
                    {
                        columns.Add("category");
                        values.Add("@category");
                    }
                    if (properties.ContainsKey("building_type"))
                    {
                        columns.Add("building_type");
                        values.Add("@buildingType");
                    }
                    if (properties.ContainsKey("energy_production"))
                    {
                        columns.Add("energy_production");
                        values.Add("@energyProduction");
                    }
                    break;
                case "technologies":
                case "knowledge":
                    if (properties.ContainsKey("category"))
                    {
                        columns.Add("category");
                        values.Add("@category");
                    }
                    break;
            }

            // Always include display_name if available
            if (properties.ContainsKey("display_name"))
            {
                columns.Add("display_name");
                values.Add("@displayName");
            }

            return $"INSERT OR REPLACE INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
        }

        /// <summary>
        /// Build parameters for INSERT statement
        /// </summary>
        private List<SQLiteParameter> BuildInsertParameters(string name, string yamlData, string modId, string checksum, bool isNative, Dictionary<string, object> properties)
        {
            var parameters = new List<SQLiteParameter>
            {
                new SQLiteParameter("@name", name),
                new SQLiteParameter("@yamlData", yamlData),
                new SQLiteParameter("@checksum", checksum),
                new SQLiteParameter("@modId", modId),
                new SQLiteParameter("@isNative", isNative ? 1 : 0)
            };

            // Add type-specific parameters
            if (properties.TryGetValue("display_name", out var displayName))
                parameters.Add(new SQLiteParameter("@displayName", displayName?.ToString() ?? ""));

            if (properties.TryGetValue("category", out var category))
                parameters.Add(new SQLiteParameter("@category", category?.ToString() ?? ""));

            if (properties.TryGetValue("building_type", out var buildingType))
                parameters.Add(new SQLiteParameter("@buildingType", buildingType?.ToString() ?? ""));

            if (properties.TryGetValue("energy_production", out var energyProd))
                parameters.Add(new SQLiteParameter("@energyProduction", Convert.ToDouble(energyProd)));

            return parameters;
        }

        /// <summary>
        /// Update mod metadata with enhanced information
        /// </summary>
        private void UpdateModMetadata(string modId, string checksum, string dataType, int entryCount)
        {
            // Get existing data types for this mod
            var existingDataTypes = GetModDataTypes(modId);
            if (!existingDataTypes.Contains(dataType))
            {
                existingDataTypes.Add(dataType);
            }

            ExecuteNonQuery(@"
                INSERT OR REPLACE INTO mod_metadata (mod_id, last_parsed, yaml_checksum, data_types, validation_status)
                VALUES (@modId, CURRENT_TIMESTAMP, @checksum, @dataTypes, 'valid')",
                new SQLiteParameter("@modId", modId),
                new SQLiteParameter("@checksum", checksum),
                new SQLiteParameter("@dataTypes", string.Join(",", existingDataTypes)));
        }

        /// <summary>
        /// Get existing data types for a mod
        /// </summary>
        private List<string> GetModDataTypes(string modId)
        {
            var dataTypes = new List<string>();
            try
            {
                using var reader = ExecuteQuery(
                    "SELECT data_types FROM mod_metadata WHERE mod_id = @modId",
                    new SQLiteParameter("@modId", modId));

                if (reader.Read() && !reader.IsDBNull(0))
                {
                    var dataTypesStr = reader.GetString(0);
                    if (!string.IsNullOrEmpty(dataTypesStr))
                    {
                        dataTypes = dataTypesStr.Split(',').ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to get mod data types: {ex.Message}");
            }
            return dataTypes;
        }

        /// <summary>
        /// Retrieve stored YAML data with enhanced filtering and validation
        /// </summary>
        /// <param name="dataType">Type of data to retrieve</param>
        /// <param name="modId">Mod identifier (null for all)</param>
        /// <param name="filters">Optional filters for advanced queries</param>
        /// <returns>Dictionary of parsed and validated data</returns>
        public Dictionary<string, object> RetrieveYAMLData(string dataType, string modId = null, Dictionary<string, object> filters = null)
        {
            var result = new Dictionary<string, object>();
            var tableName = GetTableName(dataType);
            if (tableName == null) return result;

            try
            {
                // Build query with filters
                var (sql, parameters) = BuildSelectQuery(tableName, modId, filters);

                using var reader = ExecuteQuery(sql, parameters.ToArray());
                while (reader.Read())
                {
                    var name = reader.GetString(reader.GetOrdinal("name"));
                    var yamlDataJson = reader.GetString(reader.GetOrdinal("yaml_data"));
                    var validationStatus = reader.GetString(reader.GetOrdinal("validation_status"));

                    // Only return validated data
                    if (validationStatus == "valid")
                    {
                        try
                        {
                            var data = DeserializeFromJson(yamlDataJson);
                            result[name] = data;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to deserialize {dataType} '{name}': {ex.Message}");
                            // Mark as invalid for future queries
                            MarkAsInvalid(tableName, name, $"Deserialization error: {ex.Message}");
                        }
                    }
                }

                _logger.LogInfo($"✅ Retrieved {result.Count} validated {dataType} entries");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to retrieve YAML data: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Build SELECT query with filters
        /// </summary>
        private (string sql, SQLiteParameter[] parameters) BuildSelectQuery(string tableName, string modId, Dictionary<string, object> filters)
        {
            var sql = $"SELECT name, yaml_data, validation_status FROM {tableName} WHERE validation_status = 'valid'";
            var parameters = new List<SQLiteParameter>();

            // Add mod filter
            if (!string.IsNullOrEmpty(modId))
            {
                sql += " AND mod_id = @modId";
                parameters.Add(new SQLiteParameter("@modId", modId));
            }

            // Add custom filters
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    switch (filter.Key.ToLower())
                    {
                        case "category":
                            sql += " AND category = @category";
                            parameters.Add(new SQLiteParameter("@category", filter.Value?.ToString() ?? ""));
                            break;
                        case "building_type":
                            sql += " AND building_type = @buildingType";
                            parameters.Add(new SQLiteParameter("@buildingType", filter.Value?.ToString() ?? ""));
                            break;
                        case "energy_production_min":
                            if (double.TryParse(filter.Value?.ToString(), out var minEnergy))
                            {
                                sql += " AND energy_production >= @minEnergy";
                                parameters.Add(new SQLiteParameter("@minEnergy", minEnergy));
                            }
                            break;
                        case "energy_production_max":
                            if (double.TryParse(filter.Value?.ToString(), out var maxEnergy))
                            {
                                sql += " AND energy_production <= @maxEnergy";
                                parameters.Add(new SQLiteParameter("@maxEnergy", maxEnergy));
                            }
                            break;
                        case "is_native":
                            if (bool.TryParse(filter.Value?.ToString(), out var isNative))
                            {
                                sql += " AND is_native = @isNative";
                                parameters.Add(new SQLiteParameter("@isNative", isNative ? 1 : 0));
                            }
                            break;
                    }
                }
            }

            sql += " ORDER BY name";
            return (sql, parameters.ToArray());
        }

        /// <summary>
        /// Mark an entry as invalid with error message
        /// </summary>
        private void MarkAsInvalid(string tableName, string name, string errorMessage)
        {
            try
            {
                ExecuteNonQuery($@"
                    UPDATE {tableName}
                    SET validation_status = 'invalid', last_updated = CURRENT_TIMESTAMP
                    WHERE name = @name",
                    new SQLiteParameter("@name", name));

                _logger.LogWarning($"Marked {tableName} entry '{name}' as invalid: {errorMessage}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to mark entry as invalid: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all atmospheric resources (real atmosphere composition, not derived)
        /// Ordered by priority for display purposes
        /// </summary>
        public Dictionary<string, object> GetAtmosphericResources()
        {
            return RetrieveYAMLData("resources", null, new Dictionary<string, object>
            {
                ["is_atmospheric"] = true
            });
        }

        /// <summary>
        /// Get atmospheric resources ordered by display priority
        /// Higher priority = more important for atmosphere display
        /// </summary>
        public List<KeyValuePair<string, object>> GetAtmosphericResourcesOrdered()
        {
            try
            {
                var result = new List<KeyValuePair<string, object>>();
                var tableName = GetTableName("resources");

                using var reader = ExecuteQuery($@"
                    SELECT name, yaml_data FROM {tableName}
                    WHERE is_atmospheric = 1 AND validation_status = 'valid'
                    ORDER BY atmospheric_priority DESC, name ASC",
                    new SQLiteParameter("@tableName", tableName));

                while (reader.Read())
                {
                    var name = reader.GetString(0);
                    var yamlDataJson = reader.GetString(1);

                    try
                    {
                        var data = DeserializeFromJson(yamlDataJson);
                        result.Add(new KeyValuePair<string, object>(name, data));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to deserialize atmospheric resource '{name}': {ex.Message}");
                    }
                }

                _logger.LogInfo($"✅ Retrieved {result.Count} atmospheric resources (ordered by priority)");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get ordered atmospheric resources: {ex.Message}");
                return new List<KeyValuePair<string, object>>();
            }
        }

        /// <summary>
        /// Get derived/calculated resources (not real atmosphere)
        /// These are used by buildings for I/O but shouldn't be displayed in atmosphere
        /// </summary>
        public Dictionary<string, object> GetDerivedResources()
        {
            return RetrieveYAMLData("resources", null, new Dictionary<string, object>
            {
                ["is_atmospheric"] = false
            });
        }

        /// <summary>
        /// Manually mark a resource as atmospheric (for mod compatibility)
        /// </summary>
        public void MarkResourceAsAtmospheric(string resourceName, int priority = 30)
        {
            try
            {
                ExecuteNonQuery(@"
                    UPDATE resources
                    SET is_atmospheric = 1, atmospheric_priority = @priority, validation_status = 'valid'
                    WHERE name = @name",
                    new SQLiteParameter("@name", resourceName),
                    new SQLiteParameter("@priority", priority));

                _logger.LogInfo($"✅ Marked resource '{resourceName}' as atmospheric (priority: {priority})");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to mark resource as atmospheric: {ex.Message}");
            }
        }

        /// <summary>
        /// Get atmosphere composition summary for display
        /// Returns only real atmospheric gases with their priorities
        /// </summary>
        public Dictionary<string, AtmosphericResourceInfo> GetAtmosphereComposition()
        {
            var composition = new Dictionary<string, AtmosphericResourceInfo>();

            try
            {
                using var reader = ExecuteQuery(@"
                    SELECT name, display_name, atmospheric_priority
                    FROM resources
                    WHERE is_atmospheric = 1 AND validation_status = 'valid'
                    ORDER BY atmospheric_priority DESC");

                while (reader.Read())
                {
                    var name = reader.GetString(0);
                    var displayName = reader.IsDBNull(1) ? name : reader.GetString(1);
                    var priority = reader.GetInt32(2);

                    composition[name] = new AtmosphericResourceInfo
                    {
                        Name = name,
                        DisplayName = displayName,
                        Priority = priority,
                        IsNative = priority >= 70, // Native resources have priority >= 70
                        IsModAdded = priority < 70 && priority >= 50 // Mod resources have priority 50-69
                    };
                }

                _logger.LogInfo($"✅ Retrieved atmosphere composition: {composition.Count} gases");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get atmosphere composition: {ex.Message}");
            }

            return composition;
        }

        /// <summary>
        /// Information about an atmospheric resource
        /// </summary>
        public class AtmosphericResourceInfo
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public int Priority { get; set; }
            public bool IsNative { get; set; }
            public bool IsModAdded { get; set; }

            public string GetSourceDescription()
            {
                if (IsNative) return "Native Game";
                if (IsModAdded) return "Mod Added";
                return "Auto-detected";
            }
        }

        /// <summary>
        /// Get entries by category
        /// </summary>
        public Dictionary<string, object> GetEntriesByCategory(string dataType, string category)
        {
            return RetrieveYAMLData(dataType, null, new Dictionary<string, object> { ["category"] = category });
        }

        /// <summary>
        /// Get building entries by energy production range
        /// </summary>
        public Dictionary<string, object> GetBuildingsByEnergyRange(double minEnergy, double maxEnergy)
        {
            return RetrieveYAMLData("buildings", null, new Dictionary<string, object>
            {
                ["energy_production_min"] = minEnergy,
                ["energy_production_max"] = maxEnergy
            });
        }

        /// <summary>
        /// Validate all entries for a data type
        /// </summary>
        public ValidationReport ValidateDataType(string dataType)
        {
            var report = new ValidationReport { DataType = dataType };
            var tableName = GetTableName(dataType);
            if (tableName == null) return report;

            try
            {
                using var reader = ExecuteQuery($"SELECT name, yaml_data, validation_status FROM {tableName}");
                while (reader.Read())
                {
                    var name = reader.GetString(0);
                    var yamlDataJson = reader.GetString(1);
                    var currentStatus = reader.GetString(2);

                    try
                    {
                        // Attempt to deserialize
                        var data = DeserializeFromJson(yamlDataJson);

                        // Basic validation
                        if (data is Dictionary<object, object> dict)
                        {
                            report.ValidEntries++;
                            if (currentStatus != "valid")
                            {
                                // Update status to valid
                                ExecuteNonQuery($@"
                                    UPDATE {tableName} SET validation_status = 'valid'
                                    WHERE name = @name", new SQLiteParameter("@name", name));
                            }
                        }
                        else
                        {
                            report.InvalidEntries++;
                            report.Errors.Add($"{name}: Data is not a valid dictionary");
                        }
                    }
                    catch (Exception ex)
                    {
                        report.InvalidEntries++;
                        report.Errors.Add($"{name}: {ex.Message}");

                        // Mark as invalid
                        if (currentStatus != "invalid")
                        {
                            MarkAsInvalid(tableName, name, ex.Message);
                        }
                    }
                }

                report.TotalEntries = report.ValidEntries + report.InvalidEntries;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to validate {dataType}: {ex.Message}");
                report.Errors.Add($"Validation failed: {ex.Message}");
            }

            return report;
        }

        /// <summary>
        /// Validation report for data integrity checking
        /// </summary>
        public class ValidationReport
        {
            public string DataType { get; set; }
            public int TotalEntries { get; set; }
            public int ValidEntries { get; set; }
            public int InvalidEntries { get; set; }
            public List<string> Errors { get; set; } = new();
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
        /// Get database schema version for a data type
        /// </summary>
        public int GetSchemaVersion(string dataType)
        {
            try
            {
                using var reader = ExecuteQuery(
                    "SELECT version FROM schema_versions WHERE data_type = @dataType ORDER BY version DESC LIMIT 1",
                    new SQLiteParameter("@dataType", dataType));

                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }

                return 1; // Default version
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Apply schema migration for a data type
        /// </summary>
        public void ApplySchemaMigration(string dataType, int newVersion, string description)
        {
            try
            {
                // Record the migration
                ExecuteNonQuery(@"
                    INSERT INTO schema_versions (data_type, version, description)
                    VALUES (@dataType, @newVersion, @description)",
                    new SQLiteParameter("@dataType", dataType),
                    new SQLiteParameter("@newVersion", newVersion),
                    new SQLiteParameter("@description", description));

                // Update schema version in all relevant entries
                var tableName = GetTableName(dataType);
                if (tableName != null)
                {
                    ExecuteNonQuery($@"
                        UPDATE {tableName} SET schema_version = @newVersion
                        WHERE schema_version < @newVersion",
                        new SQLiteParameter("@newVersion", newVersion));
                }

                _logger.LogInfo($"Applied schema migration for {dataType} to version {newVersion}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to apply schema migration: {ex.Message}");
            }
        }

        /// <summary>
        /// Get enhanced database statistics
        /// </summary>
        public EnhancedStats GetEnhancedStats()
        {
            var stats = new EnhancedStats();

            try
            {
                // Basic counts
                var tables = new[] { "resources", "buildings", "technologies", "knowledge" };
                foreach (var table in tables)
                {
                    using var reader = ExecuteQuery($"SELECT COUNT(*) FROM {table}");
                    if (reader.Read())
                    {
                        stats.TotalEntries[table] = reader.GetInt32(0);
                    }

                    // Valid entries
                    using var validReader = ExecuteQuery($"SELECT COUNT(*) FROM {table} WHERE validation_status = 'valid'");
                    if (validReader.Read())
                    {
                        stats.ValidEntries[table] = validReader.GetInt32(0);
                    }

                    // Invalid entries
                    using var invalidReader = ExecuteQuery($"SELECT COUNT(*) FROM {table} WHERE validation_status = 'invalid'");
                    if (invalidReader.Read())
                    {
                        stats.InvalidEntries[table] = invalidReader.GetInt32(0);
                    }

                    // Native vs mod entries
                    using var nativeReader = ExecuteQuery($"SELECT COUNT(*) FROM {table} WHERE is_native = 1");
                    if (nativeReader.Read())
                    {
                        stats.NativeEntries[table] = nativeReader.GetInt32(0);
                    }
                }

                // Mod statistics
                stats.TotalMods = GetModCount();

                // Schema versions
                foreach (var table in tables)
                {
                    stats.SchemaVersions[table] = GetSchemaVersion(table);
                }

                // Recent updates
                using var recentReader = ExecuteQuery(@"
                    SELECT COUNT(*) FROM (
                        SELECT last_updated FROM resources
                        UNION ALL SELECT last_updated FROM buildings
                        UNION ALL SELECT last_updated FROM technologies
                        UNION ALL SELECT last_updated FROM knowledge
                    ) WHERE last_updated > datetime('now', '-1 hour')");

                if (recentReader.Read())
                {
                    stats.RecentUpdates = recentReader.GetInt32(0);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get enhanced stats: {ex.Message}");
            }

            return stats;
        }

        /// <summary>
        /// Enhanced statistics with validation and schema information
        /// </summary>
        public class EnhancedStats
        {
            public Dictionary<string, int> TotalEntries { get; } = new();
            public Dictionary<string, int> ValidEntries { get; } = new();
            public Dictionary<string, int> InvalidEntries { get; } = new();
            public Dictionary<string, int> NativeEntries { get; } = new();
            public Dictionary<string, int> SchemaVersions { get; } = new();
            public int TotalMods { get; set; }
            public int RecentUpdates { get; set; }

            public double GetValidationRate(string dataType)
            {
                if (TotalEntries.TryGetValue(dataType, out var total) && total > 0)
                {
                    if (ValidEntries.TryGetValue(dataType, out var valid))
                    {
                        return (double)valid / total * 100;
                    }
                }
                return 0;
            }
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
        /// Get a single entry from the database
        /// </summary>
        public object GetEntry(string dataType, string name)
        {
            var tableName = GetTableName(dataType);
            if (tableName == null) return null;

            try
            {
                using var reader = ExecuteQuery($@"
                    SELECT yaml_data FROM {tableName}
                    WHERE name = @name AND validation_status = 'valid'",
                    new SQLiteParameter("@name", name));

                if (reader.Read())
                {
                    var yamlDataJson = reader.GetString(0);
                    return DeserializeFromJson(yamlDataJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get entry {dataType} '{name}': {ex.Message}");
            }

            return null;
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