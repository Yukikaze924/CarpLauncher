using Newtonsoft.Json.Linq;
using ProjBobcat.Class.Model;
using System;

namespace CarpLauncher.Core
{
    public class HttpManager
    {
        private static readonly HttpClient client = new HttpClient();

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
