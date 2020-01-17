using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Services
{
    public class RankingService
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;
        
        public RankingService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
        }
        public void Process(SocketMessage message)
        {

        }
    }
}
