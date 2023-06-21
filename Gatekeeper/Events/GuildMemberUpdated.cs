using Discord;
using Discord.WebSocket;
using Gatekeeper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Events
{
    public class GuildMemberUpdated
    {
        private readonly DiscordSocketClient _client;
        private readonly WhitelistAppService _whitelist;
        public GuildMemberUpdated(DiscordSocketClient client, WhitelistAppService whitelist)
        {
            _client = client;
            _whitelist = whitelist;
            _client.GuildMemberUpdated += GuildMemberUpdatedEventAsync;
        }

        public async Task GuildMemberUpdatedEventAsync(Cacheable<SocketGuildUser,ulong> beforeCache, SocketGuildUser after)
        {
            if (after.IsBot) return;
            SocketGuildUser before = beforeCache.Value;

            if (before.Roles.Count < after.Roles.Count)
            {
                // If the user has gained the pending role, we can close the application
                SocketRole newRole = after.Roles.Except(before.Roles).First();
                if (newRole.Name == "Pending" && _whitelist.UserHasActiveApplication(after.Id))
                {
                    _whitelist.CloseApplication(after);
                }
            }
        }
    }
}
