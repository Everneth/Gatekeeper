using Discord.Commands;
using Discord.WebSocket;
using Gatekeeper.Preconditions;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("audit")]
    [RequireRole("Staff")]
    public class AuditModule : ModuleBase<SocketCommandContext>
    {
        private readonly AuditService _audit;
        private readonly DataService _data;

        public AuditModule(IServiceProvider services)
        {
            _audit = services.GetRequiredService<AuditService>();
            _data = services.GetRequiredService<DataService>();
        }

        [Command("ignore")]
        public async Task IgnoreChannel(ISocketMessageChannel channel = null)
        {
            if (channel == null)
            {
                channel = Context.Channel;
            }
            if (_audit.IgnoredChannelIds.Contains(channel.Id)) return;

            _audit.IgnoredChannelIds.Add(channel.Id);
            _data.Save("ignored_channels", _audit.IgnoredChannelIds);
            await channel.SendMessageAsync("Added this channel to the ignored channels list!");
        }

        [Command("unignore")]
        public async Task UnignoreChannel(ISocketMessageChannel channel = null)
        {
            if (channel == null)
            {
                channel = Context.Channel;
            }
            if (!_audit.IgnoredChannelIds.Contains(channel.Id)) return;

            _audit.IgnoredChannelIds.Remove(channel.Id);
            _data.Save("ignored_channels", _audit.IgnoredChannelIds);
            await channel.SendMessageAsync("Removed this channel from the ignored channels list!");
        }
    }
}
