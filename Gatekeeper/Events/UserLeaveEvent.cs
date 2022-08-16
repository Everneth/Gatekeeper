using Discord.WebSocket;
using Gatekeeper.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Events
{
    public class UserLeaveEvent
    {
        private readonly DiscordSocketClient _client;
        private readonly WhitelistAppService _whitelist;
        public UserLeaveEvent(DiscordSocketClient client, WhitelistAppService whitelist)
        {
            _client = client;
            _whitelist = whitelist;
            _client.UserLeft += LeaveGuildEventAsync;
        }

        public async Task LeaveGuildEventAsync(SocketGuild guild, SocketUser user)
        {
            if (user.IsBot) return;

            if (_whitelist.UserHasActiveApplication(user.Id))
            {
                // the user left before their vote ever started, we can close their application
                _whitelist.CloseApplication(user);
            }
        }
    }
}
