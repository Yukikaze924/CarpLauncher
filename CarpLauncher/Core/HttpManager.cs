using Newtonsoft.Json.Linq;
using System;

namespace CarpLauncher.Core
{
    public class HttpManager
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<List<string>?> GetVersionManifest()
        {
            try
            {
                string json = await client.GetStringAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");

                if (string.IsNullOrEmpty(json)) return default;

                var data = JObject.Parse(json);

                var list = new List<string>();

                foreach (var version in data["versions"])
                {
                    var id = version["id"].ToString();
                    if (id.Contains('.')
                        && !id.Contains("pre")
                        && !id.Contains("rc")
                        && !id.Contains("3D")
                        && !id.Contains("Pre")
                        )
                        {
                            list.Add(id);
                        }
                }

                return list;
            }
            catch { return default; }
        }

        public static async Task GetPlayerSkin(string uuid)
        {
            // 获取皮肤信息
            string json = await client.GetStringAsync($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}");

            JObject data = JObject.Parse(json);

            string skinBase64 = data["properties"][0]["value"].ToString();
        }

        public static async Task<string> GetPlayerUUID(string username)
        {
            // 获取UUID
            string json = await client.GetStringAsync($"https://api.mojang.com/users/profiles/minecraft/{username}");

            JObject data = JObject.Parse(json);

            return data["id"].ToString();
        }
    }
}
