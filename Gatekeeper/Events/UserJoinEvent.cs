using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Gatekeeper.Events
{
    public class UserJoinEvent
    {
        private readonly DiscordSocketClient _client;
        private IServiceProvider _services;

        public UserJoinEvent(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _client.UserJoined += JoinGuildEventAsync;
        }

        public async Task JoinGuildEventAsync(SocketGuildUser user)
        {
            var channel = await user.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync("Hi " + user.Username + "! \nMy name is Jasper. Welcome to the Everneth SMP Discord server! " +
                "To get you started I have some information on how to apply using discord! No need for a website account, just use my " +
                "companion bot The Wench to submit your application.");
        }
    }
}
