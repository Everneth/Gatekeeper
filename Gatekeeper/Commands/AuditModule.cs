using Discord.Interactions;
using Discord.WebSocket;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("audit", "All commands pertaining to the behavior of the audit service.")]
    [RequireRole("High Council (Admin)")]
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
        public async Task IgnoreChannel(ISocketMessageChannel channel = null)
        {
            if (channel == null)
            {
                channel = Context.Channel;
            }
            if (_audit.IgnoredChannelIds.Contains(channel.Id)) return;

            _audit.IgnoredChannelIds.Add(channel.Id);
            _data.Save("ignored_channels", _audit.IgnoredChannelIds);
            await RespondAsync($"Added {channel.Name} channel to the ignored channels list!");
        }

        [SlashCommand("unignore", "Have the audit log stop ignoring all events in the specified channel.")]
        public async Task UnignoreChannel(ISocketMessageChannel channel = null)
        {
            if (channel == null)
            {
                channel = Context.Channel;
            }
            if (!_audit.IgnoredChannelIds.Contains(channel.Id)) return;

            _audit.IgnoredChannelIds.Remove(channel.Id);
            _data.Save("ignored_channels", _audit.IgnoredChannelIds);
            await RespondAsync($"Removed {channel.Name} channel from the ignored channels list!");
        }
    }
}
