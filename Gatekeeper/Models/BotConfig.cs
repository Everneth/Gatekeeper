using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gatekeeper.Models
{
    public class BotConfig
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("prefix")]
        public string CommandPrefix { get; set; }
        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }
        [JsonProperty("database_username")]
        public string DatabaseUsername { get; set; }
        [JsonProperty("database_ip")]
        public string DatabaseIp { get; set; }
        [JsonProperty("database")]
        public string Database { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("apitoken")]
        public string ApiToken { get; set; }
        [JsonProperty("apiserveruri")]
        public string ApiServerUri { get; set; }
    }
}
