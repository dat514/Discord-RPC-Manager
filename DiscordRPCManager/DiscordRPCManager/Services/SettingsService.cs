using Microsoft.Win32;
using System;
using System.IO;
using System.Text.Json;

namespace DiscordRPCManager.Services
{
    public class AppConfig
    {
        public bool RunAtStartup { get; set; }
        public bool AutoStartRpc { get; set; }
        public string LastProfileId { get; set; }
        public int DetectionIntervalSeconds { get; set; } = 5;
    }

    public class SettingsService
    {
        private readonly string _file = "app_settings.json";
        private const string AppName = "DiscordRPCManager";

        public AppConfig Load()
        {
            if (!File.Exists(_file))
                return new AppConfig();

            try
            {
                var json = File.ReadAllText(_file);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }

        public void Save(AppConfig config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_file, json);

            SetStartup(config.RunAtStartup);
        }

        private void SetStartup(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (enable)
                    {
                        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                        key.SetValue(AppName, $"\"{exePath}\"");
                    }
                    else
                    {
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set startup: {ex.Message}");
            }
        }
    }
}
