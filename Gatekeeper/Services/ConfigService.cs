using Discord.WebSocket;
using Gatekeeper.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Newtonsoft.Json;

namespace Gatekeeper.Services
{
    public class ConfigService
    {
        private DiscordSocketClient _client;
        private DataService _data;
        private IServiceProvider _services;
        public RankingConfig RankingConfig { get; set; }
        public BotConfig BotConfig { get; set; }

        public ConfigService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _data = services.GetRequiredService<DataService>();
            _services = services;
            RankingConfig = _data.Load("ranking_config", RankingConfig);
            BotConfig = _data.Load("config", BotConfig);
        }
    }
}
