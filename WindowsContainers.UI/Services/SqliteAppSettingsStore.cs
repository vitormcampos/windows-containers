using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using WindowsContainers.UI.Models;
using WindowsContainers.UI.Services.Interfaces;

namespace WindowsContainers.UI.Services;

public sealed class SqliteAppSettingsStore : IAppSettingsStore
{
    private const int CurrentSchemaVersion = 1;
    private readonly string _connectionString;

    public SqliteAppSettingsStore()
    {
        var dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsContainers",
            "data");

        Directory.CreateDirectory(dataDirectory);
        var databasePath = Path.Combine(dataDirectory, "app.db");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
        }.ToString();
    }

    public async Task<ApplicationSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT SettingKey, SettingValue FROM AppSettings;";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            values[reader.GetString(0)] = reader.GetString(1);

        return ApplicationSettingsMapper.FromValues(values);
    }

    public async Task SaveAsync(ApplicationSettings settings, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var values = ApplicationSettingsMapper.ToValues(settings);

        foreach (var pair in values)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO AppSettings (SettingKey, SettingValue)
                VALUES ($key, $value)
                ON CONFLICT(SettingKey) DO UPDATE SET SettingValue = excluded.SettingValue;
                """;
            command.Parameters.AddWithValue("$key", pair.Key);
            command.Parameters.AddWithValue("$value", pair.Value);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS SchemaInfo (
                Id INTEGER PRIMARY KEY CHECK (Id = 1),
                Version INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS AppSettings (
                SettingKey TEXT PRIMARY KEY NOT NULL,
                SettingValue TEXT NOT NULL
            );
            INSERT INTO SchemaInfo (Id, Version) VALUES (1, $version)
            ON CONFLICT(Id) DO UPDATE SET Version = excluded.Version;
            """;
        command.Parameters.AddWithValue("$version", CurrentSchemaVersion);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return connection;
    }
}

internal static class ApplicationSettingsMapper
{
    public static Dictionary<string, string> ToValues(ApplicationSettings settings) => new()
    {
        ["appearance.theme"] = settings.Theme,
        ["appearance.compact_mode"] = settings.CompactMode.ToString(),
        ["appearance.show_status_bar"] = settings.ShowStatusBar.ToString(),
        ["runtime.gpu_enabled"] = settings.RuntimeGpuEnabled.ToString(),
        ["runtime.auto_start_session"] = settings.AutoStartSession.ToString(),
        ["session.name"] = settings.SessionName,
        ["session.storage_path"] = settings.StoragePath,
        ["session.cpu_count"] = settings.CpuCount.ToString(),
        ["session.memory_mb"] = settings.MemoryMb.ToString(),
        ["session.timeout_seconds"] = settings.TimeoutSeconds.ToString(),
        ["session.gpu_enabled"] = settings.SessionGpuEnabled.ToString(),
        ["session.vhd_type"] = settings.VhdType,
        ["session.vhd_size_gb"] = settings.VhdSizeGb.ToString(),
        ["container.default_image"] = settings.DefaultImage,
        ["container.network_mode"] = settings.NetworkMode,
        ["container.hostname"] = settings.HostName,
        ["container.domain_name"] = settings.DomainName,
        ["container.working_directory"] = settings.WorkingDirectory,
        ["container.auto_remove"] = settings.AutoRemove.ToString(),
        ["container.privileged"] = settings.Privileged.ToString(),
        ["container.gpu_enabled"] = settings.ContainerGpuEnabled.ToString(),
        ["powershell.aliases"] = JsonSerializer.Serialize(settings.PowerShellAliases),
    };

    public static ApplicationSettings FromValues(IReadOnlyDictionary<string, string> values)
    {
        var settings = new ApplicationSettings();
        settings.Theme = GetTheme(values, settings.Theme);
        settings.CompactMode = GetBool(values, "appearance.compact_mode", settings.CompactMode);
        settings.ShowStatusBar = GetBool(values, "appearance.show_status_bar", settings.ShowStatusBar);
        settings.RuntimeGpuEnabled = GetBool(values, "runtime.gpu_enabled", settings.RuntimeGpuEnabled);
        settings.AutoStartSession = GetBool(values, "runtime.auto_start_session", settings.AutoStartSession);
        settings.SessionName = Get(values, "session.name", settings.SessionName);
        settings.StoragePath = Get(values, "session.storage_path", settings.StoragePath);
        settings.CpuCount = GetInt(values, "session.cpu_count", settings.CpuCount);
        settings.MemoryMb = GetInt(values, "session.memory_mb", settings.MemoryMb);
        settings.TimeoutSeconds = GetInt(values, "session.timeout_seconds", settings.TimeoutSeconds);
        settings.SessionGpuEnabled = GetBool(values, "session.gpu_enabled", settings.SessionGpuEnabled);
        settings.VhdType = Get(values, "session.vhd_type", settings.VhdType);
        settings.VhdSizeGb = GetInt(values, "session.vhd_size_gb", settings.VhdSizeGb);
        settings.DefaultImage = Get(values, "container.default_image", settings.DefaultImage);
        settings.NetworkMode = Get(values, "container.network_mode", settings.NetworkMode);
        settings.HostName = Get(values, "container.hostname", settings.HostName);
        settings.DomainName = Get(values, "container.domain_name", settings.DomainName);
        settings.WorkingDirectory = Get(values, "container.working_directory", settings.WorkingDirectory);
        settings.AutoRemove = GetBool(values, "container.auto_remove", settings.AutoRemove);
        settings.Privileged = GetBool(values, "container.privileged", settings.Privileged);
        settings.ContainerGpuEnabled = GetBool(values, "container.gpu_enabled", settings.ContainerGpuEnabled);
        if (values.TryGetValue("powershell.aliases", out var aliases))
        {
            try
            {
                settings.PowerShellAliases = JsonSerializer.Deserialize<List<PowerShellAliasSettings>>(aliases) ?? new();
            }
            catch (JsonException)
            {
                settings.PowerShellAliases = new();
            }
        }
        return settings;
    }

    private static string GetTheme(IReadOnlyDictionary<string, string> values, string fallback)
    {
        var value = Get(values, "appearance.theme", fallback);
        return value is "System" or "Light" or "Dark" ? value : fallback;
    }

    private static string Get(IReadOnlyDictionary<string, string> values, string key, string fallback) =>
        values.TryGetValue(key, out var value) ? value : fallback;

    private static bool GetBool(IReadOnlyDictionary<string, string> values, string key, bool fallback) =>
        values.TryGetValue(key, out var value) && bool.TryParse(value, out var result) ? result : fallback;

    private static int GetInt(IReadOnlyDictionary<string, string> values, string key, int fallback) =>
        values.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : fallback;
}
