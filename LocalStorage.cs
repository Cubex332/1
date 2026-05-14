using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

public static class LocalStorage
{
    private static readonly string Folder =
        Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData), "MyApp");

    private static readonly string FilePath =
        Path.Combine(Folder, "prefs.json");

    private static Dictionary<string, string> data;

    // =========================
    // INIT
    // =========================
    private static void EnsureLoaded()
    {
        if (data != null) return;

        if (!File.Exists(FilePath))
        {
            data = new Dictionary<string, string>();
            return;
        }

        string json = File.ReadAllText(FilePath);

        data = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
               ?? new Dictionary<string, string>();
    }

    private static void Save()
    {
        Directory.CreateDirectory(Folder);

        string json = JsonSerializer.Serialize(data,
            new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(FilePath, json);
    }

    // =========================
    // STRING
    // =========================
    public static void SetString(string key, string value)
    {
        EnsureLoaded();
        data[key] = value;
        Save();
    }

    public static string GetString(string key, string defaultValue = "")
    {
        EnsureLoaded();
        return data.TryGetValue(key, out var value) ? value : defaultValue;
    }

    // =========================
    // INT
    // =========================
    public static void SetInt(string key, int value)
    {
        EnsureLoaded();
        data[key] = value.ToString();
        Save();
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        EnsureLoaded();

        return int.TryParse(data.GetValueOrDefault(key), out var value)
            ? value
            : defaultValue;
    }

    // =========================
    // FLOAT
    // =========================
    public static void SetFloat(string key, float value)
    {
        EnsureLoaded();
        data[key] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Save();
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        EnsureLoaded();

        return float.TryParse(data.GetValueOrDefault(key),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var value)
            ? value
            : defaultValue;
    }

    // =========================
    // BOOL
    // =========================
    public static void SetBool(string key, bool value)
    {
        EnsureLoaded();
        data[key] = value ? "1" : "0";
        Save();
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        EnsureLoaded();

        if (!data.TryGetValue(key, out var value))
            return defaultValue;

        return value == "1";
    }

    // =========================
    // EXISTS
    // =========================
    public static bool HasKey(string key)
    {
        EnsureLoaded();
        return data.ContainsKey(key);
    }

    // =========================
    // DELETE
    // =========================
    public static void DeleteKey(string key)
    {
        EnsureLoaded();

        if (data.Remove(key))
            Save();
    }

    // =========================
    // CLEAR ALL
    // =========================
    public static void Clear()
    {
        EnsureLoaded();

        data.Clear();
        Save();
    }
}
