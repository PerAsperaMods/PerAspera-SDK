using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using BepInEx.Logging;

namespace PerAspera.GameAPI.Database
{
    /// <summary>
    /// Persistance SQLite pour le système Economy SDK.
    /// Stocke : entités terrestres (réputation/budget), stock orbital, transactions, historique réputation.
    /// <para>
    /// Utilise le même fichier que <see cref="ModDatabase"/> (<c>BepInEx/plugins/Database/moddata.db</c>)
    /// avec une connexion séparée en mode WAL (lectures concurrentes sans blocage).
    /// Tables préfixées <c>economy_</c> pour isolation dans la DB partagée.
    /// </para>
    /// <para>
    /// Identifiant de save (<c>save_id</c>) : le jeu n'expose pas d'ID de save natif trivial.
    /// Stratégie : hash déterministe (nom faction joueur + sol de démarrage) fourni par l'appelant.
    /// Fallback single-slot documenté si l'appelant passe une chaîne vide.
    /// </para>
    /// <code>
    /// // Initialisation dans le plugin BepInEx :
    /// EconomyStore.Instance.Initialize();
    ///
    /// // Snapshot de save :
    /// var snap = BuildSnapshot(entities, orbitalStock, sol);
    /// EconomyStore.Instance.SaveSnapshot("save_abc123", snap);
    ///
    /// // Restauration :
    /// var loaded = EconomyStore.Instance.LoadSnapshot("save_abc123");
    /// if (loaded != null) ApplySnapshot(loaded);
    /// </code>
    /// </summary>
    public sealed class EconomyStore
    {
        private static EconomyStore? _instance;
        private static readonly object _instanceLock = new object();

        private SQLiteConnection? _connection;
        private readonly ManualLogSource _log;

        private EconomyStore()
        {
            _log = Logger.CreateLogSource("EconomyStore");
        }

        /// <summary>Singleton — appeler <see cref="Initialize"/> avant tout usage.</summary>
        public static EconomyStore Instance
        {
            get
            {
                if (_instance == null)
                    lock (_instanceLock)
                        _instance ??= new EconomyStore();
                return _instance;
            }
        }

        // ─────────────────────────── Init / DDL ───────────────────────────

        /// <summary>
        /// Initialise la connexion et crée les tables si nécessaire.
        /// Doit être appelé une seule fois au démarrage du plugin.
        /// </summary>
        /// <param name="dbPath">
        /// Chemin vers le fichier SQLite. Si null, utilise le même répertoire que ModDatabase
        /// (<c>&lt;pluginsDir&gt;/Database/moddata.db</c>).
        /// </param>
        public void Initialize(string? dbPath = null)
        {
            if (_connection != null) return; // déjà initialisé

            try
            {
                dbPath ??= ResolveDefaultDbPath();
                var dir = Path.GetDirectoryName(dbPath)!;
                Directory.CreateDirectory(dir);

                _connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
                _connection.Open();

                // Mode WAL pour coexister avec ModDatabase sur le même fichier
                using var walCmd = new SQLiteCommand("PRAGMA journal_mode=WAL;", _connection);
                walCmd.ExecuteNonQuery();

                CreateTables();
                _log.LogInfo($"✅ EconomyStore initialisé : {dbPath}");
            }
            catch (Exception ex)
            {
                _log.LogError($"Échec init EconomyStore : {ex.Message}");
                throw;
            }
        }

        private static string ResolveDefaultDbPath()
        {
            var pluginsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var dbDir = Path.Combine(pluginsPath, "Database");
            return Path.Combine(dbDir, "moddata.db");
        }

        private void CreateTables()
        {
            Exec(@"
                CREATE TABLE IF NOT EXISTS economy_entities (
                    save_id     TEXT NOT NULL,
                    entity_id   TEXT NOT NULL,
                    reputation  REAL NOT NULL DEFAULT 0,
                    budget      REAL NOT NULL DEFAULT 0,
                    state_json  TEXT,
                    updated_at  INTEGER NOT NULL DEFAULT 0,
                    PRIMARY KEY (save_id, entity_id)
                )");

            Exec(@"
                CREATE TABLE IF NOT EXISTS economy_orbital_stock (
                    save_id      TEXT NOT NULL,
                    resource_key TEXT NOT NULL,
                    quantity     REAL NOT NULL DEFAULT 0,
                    PRIMARY KEY (save_id, resource_key)
                )");

            Exec(@"
                CREATE TABLE IF NOT EXISTS economy_transactions (
                    id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                    save_id             TEXT NOT NULL,
                    sol                 REAL NOT NULL,
                    entity_id           TEXT NOT NULL,
                    resource_key        TEXT NOT NULL,
                    quantity            REAL NOT NULL,
                    price_per_unit      REAL NOT NULL,
                    direction           INTEGER NOT NULL,
                    reputation_delta    REAL NOT NULL DEFAULT 0
                )");

            Exec(@"
                CREATE TABLE IF NOT EXISTS economy_reputation_history (
                    id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    save_id   TEXT NOT NULL,
                    entity_id TEXT NOT NULL,
                    sol       REAL NOT NULL,
                    delta     REAL NOT NULL,
                    reason    TEXT
                )");

            Exec("CREATE INDEX IF NOT EXISTS idx_eco_tx_save    ON economy_transactions(save_id)");
            Exec("CREATE INDEX IF NOT EXISTS idx_eco_tx_entity  ON economy_transactions(save_id, entity_id)");
            Exec("CREATE INDEX IF NOT EXISTS idx_eco_rep_entity ON economy_reputation_history(save_id, entity_id)");
        }

        // ─────────────────────────── Snapshot save/load ───────────────────────────

        /// <summary>Snapshot de l'état Economy à persister.</summary>
        public sealed class EconomySnapshot
        {
            /// <summary>Sol courant au moment du snapshot.</summary>
            public double Sol { get; init; }
            /// <summary>États des entités : entityId → (reputation, budget).</summary>
            public IReadOnlyDictionary<string, (float Reputation, float Budget)> Entities { get; init; }
                = new Dictionary<string, (float, float)>();
            /// <summary>Stock orbital : resourceKey → quantité.</summary>
            public IReadOnlyDictionary<string, float> OrbitalStock { get; init; }
                = new Dictionary<string, float>();
        }

        /// <summary>
        /// Persiste un snapshot complet de l'état Economy pour une save.
        /// Écrase les données précédentes du même <c>save_id</c>.
        /// </summary>
        public void SaveSnapshot(string saveId, EconomySnapshot snapshot)
        {
            if (_connection == null) { _log.LogWarning("SaveSnapshot : non initialisé"); return; }
            if (string.IsNullOrEmpty(saveId)) saveId = "default";

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            using var tx = _connection.BeginTransaction();
            try
            {
                // Entités
                Exec($"DELETE FROM economy_entities WHERE save_id = @sid",
                    tx, P("@sid", saveId));

                foreach (var (entityId, (rep, budget)) in snapshot.Entities)
                {
                    Exec(@"INSERT INTO economy_entities (save_id, entity_id, reputation, budget, updated_at)
                           VALUES (@sid, @eid, @rep, @bud, @ts)",
                        tx,
                        P("@sid", saveId), P("@eid", entityId),
                        P("@rep", rep),    P("@bud", budget), P("@ts", now));
                }

                // Stock orbital
                Exec($"DELETE FROM economy_orbital_stock WHERE save_id = @sid",
                    tx, P("@sid", saveId));

                foreach (var (resKey, qty) in snapshot.OrbitalStock)
                {
                    if (qty <= 0f) continue;
                    Exec(@"INSERT INTO economy_orbital_stock (save_id, resource_key, quantity)
                           VALUES (@sid, @rk, @qty)",
                        tx, P("@sid", saveId), P("@rk", resKey), P("@qty", qty));
                }

                tx.Commit();
                _log.LogInfo($"Snapshot sauvé : save={saveId} ({snapshot.Entities.Count} entités, {snapshot.OrbitalStock.Count} ressources)");
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _log.LogError($"SaveSnapshot échec : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Charge un snapshot Economy depuis la DB. Retourne null si aucune donnée pour ce save_id.
        /// </summary>
        public EconomySnapshot? LoadSnapshot(string saveId)
        {
            if (_connection == null) { _log.LogWarning("LoadSnapshot : non initialisé"); return null; }
            if (string.IsNullOrEmpty(saveId)) saveId = "default";

            var entities = new Dictionary<string, (float, float)>();
            var orbital  = new Dictionary<string, float>();

            using (var cmd = new SQLiteCommand(
                "SELECT entity_id, reputation, budget FROM economy_entities WHERE save_id = @sid",
                _connection))
            {
                cmd.Parameters.AddWithValue("@sid", saveId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    entities[reader.GetString(0)] = (reader.GetFloat(1), reader.GetFloat(2));
            }

            using (var cmd = new SQLiteCommand(
                "SELECT resource_key, quantity FROM economy_orbital_stock WHERE save_id = @sid",
                _connection))
            {
                cmd.Parameters.AddWithValue("@sid", saveId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    orbital[reader.GetString(0)] = reader.GetFloat(1);
            }

            if (entities.Count == 0 && orbital.Count == 0) return null;

            return new EconomySnapshot { Entities = entities, OrbitalStock = orbital };
        }

        // ─────────────────────────── Transactions ───────────────────────────

        /// <summary>
        /// Enregistre une transaction commerciale dans l'historique.
        /// </summary>
        /// <param name="saveId">Identifiant de save.</param>
        /// <param name="sol">Sol de la transaction.</param>
        /// <param name="entityId">Id entité terrestre.</param>
        /// <param name="resourceKey">Clé YAML ressource.</param>
        /// <param name="quantity">Quantité échangée.</param>
        /// <param name="pricePerUnit">Prix unitaire.</param>
        /// <param name="direction">0 = MarsToEarth, 1 = EarthToMars.</param>
        /// <param name="reputationDelta">Delta de réputation appliqué.</param>
        public void AppendTransaction(string saveId, double sol, string entityId,
            string resourceKey, float quantity, float pricePerUnit,
            int direction, float reputationDelta = 0f)
        {
            if (_connection == null) return;
            if (string.IsNullOrEmpty(saveId)) saveId = "default";

            Exec(@"INSERT INTO economy_transactions
                       (save_id, sol, entity_id, resource_key, quantity, price_per_unit, direction, reputation_delta)
                   VALUES (@sid, @sol, @eid, @rk, @qty, @ppu, @dir, @rep)",
                null,
                P("@sid",  saveId),
                P("@sol",  sol),
                P("@eid",  entityId),
                P("@rk",   resourceKey),
                P("@qty",  quantity),
                P("@ppu",  pricePerUnit),
                P("@dir",  direction),
                P("@rep",  reputationDelta));
        }

        /// <summary>
        /// Retourne les transactions d'une entité depuis un sol donné.
        /// </summary>
        public List<(double Sol, string ResourceKey, float Quantity, float Price, int Direction)>
            GetTransactions(string saveId, string entityId, double sinceSol = 0)
        {
            var result = new List<(double, string, float, float, int)>();
            if (_connection == null) return result;
            if (string.IsNullOrEmpty(saveId)) saveId = "default";

            using var cmd = new SQLiteCommand(@"
                SELECT sol, resource_key, quantity, price_per_unit, direction
                FROM economy_transactions
                WHERE save_id = @sid AND entity_id = @eid AND sol >= @sol
                ORDER BY sol ASC",
                _connection);

            cmd.Parameters.AddWithValue("@sid", saveId);
            cmd.Parameters.AddWithValue("@eid", entityId);
            cmd.Parameters.AddWithValue("@sol", sinceSol);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add((reader.GetDouble(0), reader.GetString(1),
                    reader.GetFloat(2), reader.GetFloat(3), reader.GetInt32(4)));
            return result;
        }

        // ─────────────────────────── Historique réputation ───────────────────────────

        /// <summary>Enregistre un delta de réputation dans l'historique.</summary>
        public void AppendReputationDelta(string saveId, string entityId,
            double sol, float delta, string reason)
        {
            if (_connection == null) return;
            if (string.IsNullOrEmpty(saveId)) saveId = "default";

            Exec(@"INSERT INTO economy_reputation_history (save_id, entity_id, sol, delta, reason)
                   VALUES (@sid, @eid, @sol, @delta, @reason)",
                null,
                P("@sid",    saveId),
                P("@eid",    entityId),
                P("@sol",    sol),
                P("@delta",  delta),
                P("@reason", reason));
        }

        // ─────────────────────────── Helpers SQL ───────────────────────────

        private void Exec(string sql, SQLiteTransaction? tx = null, params SQLiteParameter[] parms)
        {
            using var cmd = new SQLiteCommand(sql, _connection, tx);
            cmd.Parameters.AddRange(parms);
            cmd.ExecuteNonQuery();
        }

        private static SQLiteParameter P(string name, object? value)
            => new SQLiteParameter(name, value ?? DBNull.Value);
    }
}
