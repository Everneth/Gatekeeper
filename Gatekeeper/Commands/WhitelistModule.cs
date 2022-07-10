using Discord.Interactions;
using Discord.WebSocket;
using Gatekeeper.Models;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("whitelist", "Remote whitelisting commands available to staff.")]
    public class WhitelistModule : InteractionModuleBase<SocketInteractionContext>
    {
        HttpClientService _restClient;
        DiscordSocketClient _client;
        
        public WhitelistModule(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _restClient = services.GetRequiredService<HttpClientService>();
        }

        [DefaultMemberPermissions(Discord.GuildPermission.MentionEveryone)]
        [SlashCommand("add", "Add a player to the whitelist manually.")]
        public async Task AddToWhitelistAsync([Summary("name", "The player's minecraft name.")] string name)
        {
            CommandResponse commandResponse = _restClient.AddToWhitelist(name).Result;
            await RespondAsync(embed: commandResponse.RespondAsEmbed(Context));
        }

        [DefaultMemberPermissions(Discord.GuildPermission.MentionEveryone)]
        [SlashCommand("remove", "Remove a player from the whitelist manually.")]
        public async Task RemoveFromWhitelistAsync([Summary("name", "The player's minecraft name.")] string name)
        {
            CommandResponse commandResponse = _restClient.RemoveFromWhitelist(name).Result;
            await RespondAsync(embed: commandResponse.RespondAsEmbed(Context));
        }

    }
}
