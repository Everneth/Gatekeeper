using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("commands", "Test commands pertaining to removing slash commands and interactions")]
    public class UnregisterCommandsModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactions;

        public UnregisterCommandsModule(IServiceProvider services, DiscordSocketClient client, InteractionService interactions)
        {
            _client = client;
            _interactions = interactions;
        }

        [SlashCommand("removeall", "Remove all slash commands registered to this guild")]
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task RemoveCommands()
        {
            await _client.GetGuild(177976693942779904).DeleteApplicationCommandsAsync();
            await Context.Interaction.RespondAsync("All slash commands have been successfully deleted.");
        }
    }
}
