using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [Group("audit", "All commands pertaining to the behavior of the audit service.")]
    public class AuditModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AuditService _audit;
        private readonly DataService _data;

        public AuditModule(IServiceProvider services)
        {
            _audit = services.GetRequiredService<AuditService>();
            _data = services.GetRequiredService<DataService>();
        }

        [SlashCommand("ignore", "Have the audit logs ignore any events in the specified channel.")]
        public async Task IgnoreChannel([Summary("Channel", "The channel you want the log to ignore")] ISocketMessageChannel channel = null)
        {
            if (channel == null)
            {
                channel = Context.Channel;
            }
            if (_audit.IgnoredChannelIds.Contains(channel.Id))
                await RespondAsync($"<#{channel.Id}> is already in the ignored channels list.", ephemeral: true);

            _audit.IgnoredChannelIds.Add(channel.Id);
            _data.Save("ignored_channels", _audit.IgnoredChannelIds);
            await RespondAsync($"Added <#{channel.Id}> channel to the ignored channels list!");
        }

        [SlashCommand("unignore", "Have the audit log stop ignoring all events in the specified channel.")]
        public async Task UnignoreChannel([Summary("Channel", "The channel you want the log to stop ignoring")] ISocketMessageChannel channel = null)
        {
            if (channel == null)
            {
                channel = Context.Channel;
            }
            if (!_audit.IgnoredChannelIds.Contains(channel.Id))
                await RespondAsync($"<#{channel.Id}> is not in the ignored channels list.", ephemeral: true);

            _audit.IgnoredChannelIds.Remove(channel.Id);
            _data.Save("ignored_channels", _audit.IgnoredChannelIds);
            await RespondAsync($"Removed <#{channel.Id}> channel from the ignored channels list!");
        }
    }
}
