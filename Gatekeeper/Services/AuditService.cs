﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Services
{
    public class AuditService
    {
        private readonly DiscordSocketClient _client;

        public AuditService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _client.MessageDeleted += LogMessageDeleted;
            _client.GuildMemberUpdated += LogGuildMemberUpdated;
            _client.UserUpdated += LogUserUpdated;
            _client.UserBanned += LogUserBanned;
            _client.UserUnbanned += LogUserPardoned;
            _client.UserJoined += LogUserJoined;
            _client.UserLeft += LogUserLeft;
        }

        private async Task SendAudit(string audit, string emote)
        {
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            var socketGuildChannel = _client.GetGuild(177976693942779904).Channels.Where(x => x.Name == "admin-log").FirstOrDefault();
            var channel = socketGuildChannel.Guild.GetTextChannel(socketGuildChannel.Id);

            await channel.SendMessageAsync($"{emote} `[{currentTime:hh\\:mm\\:ss}]` {audit}");
        }

        private async Task LogMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, ISocketMessageChannel channel)
        {
            var socketGuildChannel = _client.GetGuild(177976693942779904).Channels.Where(x => x.Name == "admin-log").FirstOrDefault();
            if (cachedMessage.Value == null || cachedMessage.Value.Channel.Id == socketGuildChannel.Id) return;

            IMessage message = cachedMessage.Value;
            string audit = $"**{message.Author}'s** message in <#{message.Channel.Id}> was deleted. Content: \n{message.Content}";

            await SendAudit(audit, ":wastebasket:");
        }

        private async Task LogGuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"**{before}'s** ");

            if (before.Roles.Count != after.Roles.Count)
            {
                builder.AppendLine("roles have changed.");
                if (before.Roles.Count < after.Roles.Count)
                {
                    SocketRole newRole = after.Roles.Except(before.Roles).First();
                    builder.AppendLine($"Gained Role: `{newRole}`");
                }
                else
                {
                    SocketRole lostRole = before.Roles.Except(after.Roles).First();
                    builder.AppendLine($"Lost Role: `{lostRole}`");
                }

                // skip the @everyone role and sort from highest ranking role to lowest
                builder.Append($"Current roles: `{string.Join(", ", after.Roles.Skip(1).OrderByDescending(x => x.Position))}`");
            }
            else if (before.Nickname != null || after.Nickname != null)
            {
                // use the null state of the nickname to determine what nickname was changed to, if changed at all
                if (before.Nickname == null && after.Nickname != null)
                {
                    builder.Append($"nickname has been changed from **none** to **{after.Nickname}**.");
                }
                else if (before.Nickname != null && after.Nickname == null)
                {
                    builder.Append($"nickname has been changed from **{before.Nickname}** to **none**.");
                }
                else if ((before.Nickname != null && after.Nickname != null) && !before.Nickname.Equals(after.Nickname))
                {
                    builder.Append($"nickname has been changed from **{before.Nickname}** to **{after.Nickname}**.");
                }
            }

            // if we have appended something to the string, send it as a log, otherwise do nothing
            if (!builder.ToString().Equals($"**{before}'s** "))
            {
                await SendAudit(builder.ToString(), ":wrench:");
            }
        }

        private async Task LogUserUpdated(SocketUser before, SocketUser after)
        {
            if (!before.Username.Equals(after.Username))
            {
                await SendAudit($"**{before}'s** username has been changed to **{after}**.", ":wrench:");
            }
        }

        private async Task LogUserBanned(SocketUser user, SocketGuild guild)
        {
            await SendAudit($"**{user}** was banned from the server.", ":hammer:");
        }

        private async Task LogUserPardoned(SocketUser user, SocketGuild guild)
        {
            await SendAudit($"**{user}** was unbanned from the server.", ":magic_wand:");
        }

        private async Task LogUserJoined(SocketGuildUser user)
        {
            await SendAudit($"**{user}** joined the server.", ":door:");
        }

        private async Task LogUserLeft(SocketGuildUser user)
        {
            await SendAudit($"**{user}** left the server.", ":door:");
        }
    }
}
