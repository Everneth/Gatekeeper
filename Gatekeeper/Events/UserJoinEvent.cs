using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Gatekeeper.Helpers;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Gatekeeper.Events
{
    public class UserJoinEvent
    {
        private readonly DiscordSocketClient _client;
        private readonly DatabaseService _database;

        public UserJoinEvent(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _database = services.GetRequiredService<DatabaseService>();
            _client.UserJoined += JoinGuildEventAsync;
        }

        public async Task JoinGuildEventAsync(SocketGuildUser user)
        {
            if (user.IsBot) return;
            
            var applyChannel = user.Guild.Channels.SingleOrDefault(c => c.Name == "apply") as SocketTextChannel;
            var generalChannel = user.Guild.Channels.SingleOrDefault(c => c.Name == "town-square") as SocketTextChannel;

            if (_database.PlayerExists(user))
            {
                var roles = user.Guild.Roles.Where(r => r.Name == "Citizen" || r.Name == "Synced");
                await user.AddRolesAsync(roles);
                
                // since users with the citizen role do not have access to #apply, we should notify them in town-square
                await generalChannel.SendMessageAsync("Hi " + user.Mention + "! \nMy name is Jasper. Welcome ***back*** to the Everneth SMP Discord server!\n\n " +
                "I see that you have already been whitelisted and have Citizen'ed you. Have fun!");
            }
        }
    }
}
