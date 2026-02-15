using DiscordRPCManager.Models;
using System.Text.Json;
using System.IO;
namespace DiscordRPCManager.Data
{
    public class ConfigService
    {
        private readonly string _file = "rpc_profiles.json";

        public List<RpcProfile> Load()
        {
            if (!File.Exists(_file))
                return new List<RpcProfile>();

            var json = File.ReadAllText(_file);
            return JsonSerializer.Deserialize<List<RpcProfile>>(json);
        }

        public void Save(List<RpcProfile> profiles)
        {
            var json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_file, json);
        }
    }
}