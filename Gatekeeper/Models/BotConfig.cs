﻿using Newtonsoft.Json;
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
        [JsonProperty("database_username")]
        public string DatabaseUsername { get; set; }
        [JsonProperty("database")]
        public string Database { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
