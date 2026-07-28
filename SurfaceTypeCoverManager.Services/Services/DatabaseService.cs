using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SurfaceTypeCoverManager.Core.Interfaces;
using SurfaceTypeCoverManager.Core.Models;

namespace SurfaceTypeCoverManager.Services.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public DatabaseService()
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SurfaceTypeCoverManager");
            Directory.CreateDirectory(appData);
            _dbPath = Path.Combine(appData, "SurfaceTypeCoverManager.db");
            _connectionString = $"Data Source={_dbPath}";
        }

        public async Task InitializeAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string createConnectionHistory = @"
                CREATE TABLE IF NOT EXISTS ConnectionHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    EventType TEXT NOT NULL,
                    DeviceName TEXT NOT NULL,
                    HardwareId TEXT NOT NULL,
                    Details TEXT
                );";

            string createTypingHistory = @"
                CREATE TABLE IF NOT EXISTS TypingHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    DurationSeconds REAL NOT NULL,
                    Wpm REAL NOT NULL,
                    AccuracyPercent REAL NOT NULL,
                    TotalChars INTEGER NOT NULL,
                    Errors INTEGER NOT NULL,
                    DuplicatePresses INTEGER NOT NULL,
                    DroppedEvents INTEGER NOT NULL,
                    AverageLatencyMs REAL NOT NULL,
                    MaxLatencyMs REAL NOT NULL,
                    LatencyDataPointsJson TEXT
                );";

            string createDiagnosticHistory = @"
                CREATE TABLE IF NOT EXISTS DiagnosticHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    GeneratedAt TEXT NOT NULL,
                    IsHealthy INTEGER NOT NULL,
                    HidDeviceCount INTEGER NOT NULL,
                    KeyboardCount INTEGER NOT NULL,
                    SummaryText TEXT
                );";

            using (var cmd = new SqliteCommand(createConnectionHistory, connection)) await cmd.ExecuteNonQueryAsync();
            using (var cmd = new SqliteCommand(createTypingHistory, connection)) await cmd.ExecuteNonQueryAsync();
            using (var cmd = new SqliteCommand(createDiagnosticHistory, connection)) await cmd.ExecuteNonQueryAsync();
        }

        public async Task SaveConnectionEventAsync(DeviceConnectionEvent evt)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "INSERT INTO ConnectionHistory (Timestamp, EventType, DeviceName, HardwareId, Details) VALUES (@t, @e, @d, @h, @det);";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@t", evt.Timestamp.ToString("o"));
            cmd.Parameters.AddWithValue("@e", evt.EventType);
            cmd.Parameters.AddWithValue("@d", evt.DeviceName);
            cmd.Parameters.AddWithValue("@h", evt.HardwareId);
            cmd.Parameters.AddWithValue("@det", evt.Details);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IReadOnlyList<DeviceConnectionEvent>> GetConnectionHistoryAsync()
        {
            var list = new List<DeviceConnectionEvent>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT Id, Timestamp, EventType, DeviceName, HardwareId, Details FROM ConnectionHistory ORDER BY Id DESC LIMIT 100;";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new DeviceConnectionEvent
                {
                    Id = reader.GetInt32(0),
                    Timestamp = DateTime.Parse(reader.GetString(1)),
                    EventType = reader.GetString(2),
                    DeviceName = reader.GetString(3),
                    HardwareId = reader.GetString(4),
                    Details = reader.IsDBNull(5) ? "" : reader.GetString(5)
                });
            }

            return list;
        }

        public async Task SaveTypingTestAsync(TypingTestResult result)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"INSERT INTO TypingHistory (Timestamp, DurationSeconds, Wpm, AccuracyPercent, TotalChars, Errors, DuplicatePresses, DroppedEvents, AverageLatencyMs, MaxLatencyMs, LatencyDataPointsJson)
                           VALUES (@t, @dur, @wpm, @acc, @tc, @err, @dup, @drop, @avgL, @maxL, @json);";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@t", result.Timestamp.ToString("o"));
            cmd.Parameters.AddWithValue("@dur", result.DurationSeconds);
            cmd.Parameters.AddWithValue("@wpm", result.Wpm);
            cmd.Parameters.AddWithValue("@acc", result.AccuracyPercent);
            cmd.Parameters.AddWithValue("@tc", result.TotalChars);
            cmd.Parameters.AddWithValue("@err", result.Errors);
            cmd.Parameters.AddWithValue("@dup", result.DuplicatePresses);
            cmd.Parameters.AddWithValue("@drop", result.DroppedEvents);
            cmd.Parameters.AddWithValue("@avgL", result.AverageLatencyMs);
            cmd.Parameters.AddWithValue("@maxL", result.MaxLatencyMs);
            cmd.Parameters.AddWithValue("@json", result.LatencyDataPointsJson);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IReadOnlyList<TypingTestResult>> GetTypingHistoryAsync()
        {
            var list = new List<TypingTestResult>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT Id, Timestamp, DurationSeconds, Wpm, AccuracyPercent, TotalChars, Errors, DuplicatePresses, DroppedEvents, AverageLatencyMs, MaxLatencyMs, LatencyDataPointsJson FROM TypingHistory ORDER BY Id DESC LIMIT 50;";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new TypingTestResult
                {
                    Id = reader.GetInt32(0),
                    Timestamp = DateTime.Parse(reader.GetString(1)),
                    DurationSeconds = reader.GetDouble(2),
                    Wpm = reader.GetDouble(3),
                    AccuracyPercent = reader.GetDouble(4),
                    TotalChars = reader.GetInt32(5),
                    Errors = reader.GetInt32(6),
                    DuplicatePresses = reader.GetInt32(7),
                    DroppedEvents = reader.GetInt32(8),
                    AverageLatencyMs = reader.GetDouble(9),
                    MaxLatencyMs = reader.GetDouble(10),
                    LatencyDataPointsJson = reader.GetString(11)
                });
            }

            return list;
        }

        public async Task SaveDiagnosticReportAsync(DiagnosticReport report)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "INSERT INTO DiagnosticHistory (GeneratedAt, IsHealthy, HidDeviceCount, KeyboardCount, SummaryText) VALUES (@g, @h, @hid, @kb, @sum);";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@g", report.GeneratedAt.ToString("o"));
            cmd.Parameters.AddWithValue("@h", report.IsOverallHealthy ? 1 : 0);
            cmd.Parameters.AddWithValue("@hid", report.HidDevices.Count);
            cmd.Parameters.AddWithValue("@kb", report.Keyboards.Count);
            cmd.Parameters.AddWithValue("@sum", $"Diagnostics run at {report.GeneratedAt:yyyy-MM-dd HH:mm}. Status: {(report.IsOverallHealthy ? "Healthy" : "Warnings Detected")}");

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IReadOnlyList<DiagnosticReportSummary>> GetDiagnosticHistoryAsync()
        {
            var list = new List<DiagnosticReportSummary>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT Id, GeneratedAt, IsHealthy, HidDeviceCount, KeyboardCount, SummaryText FROM DiagnosticHistory ORDER BY Id DESC LIMIT 50;";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new DiagnosticReportSummary
                {
                    Id = reader.GetInt32(0),
                    GeneratedAt = DateTime.Parse(reader.GetString(1)),
                    IsHealthy = reader.GetInt32(2) == 1,
                    HidDeviceCount = reader.GetInt32(3),
                    KeyboardCount = reader.GetInt32(4),
                    SummaryText = reader.GetString(5)
                });
            }

            return list;
        }
    }
}
