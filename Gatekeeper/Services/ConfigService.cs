using Discord.WebSocket;
using Gatekeeper.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Gatekeeper.Services
{
    public class ConfigService
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;
        public BotConfig Config { get; set; }

        public ConfigService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            Config = Load();
        }

        private BotConfig Load()
        {
            using (StreamReader file = File.OpenText(@"..\..\..\Data\config.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                return Config = (BotConfig)serializer.Deserialize(file, typeof(BotConfig));
            }
        }

        public void Save()
        {
            using (StreamWriter file = File.CreateText(@"..\..\..\Data\applicants.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Config);
            }
        }
    }
}
