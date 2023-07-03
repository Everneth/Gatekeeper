using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gatekeeper.Services
{
    public class AuditService
    {
        // Controls how long the role change thread will sleep for
        private const int MINUTES_BETWEEN_DISPATCH = 2;
        private readonly Mutex mutex = new Mutex();

        private readonly DiscordSocketClient _client;
        private readonly DataService _data;
        private readonly ConfigService _config;
        public List<ulong> IgnoredChannelIds { get; }
        
        // List of users used to compare difference in roles
        private readonly List<SocketGuildUser> cachedUsers = new List<SocketGuildUser>();
        private readonly List<SocketGuildUser> updatedUsers = new List<SocketGuildUser>();

        public AuditService(DiscordSocketClient client, DataService data, ConfigService config)
        {
            _client = client;
            _client.MessageDeleted += LogMessageDeleted;
            _client.MessageUpdated += LogMessageUpdated;
            _client.GuildMemberUpdated += LogGuildMemberUpdated;
            _client.UserUpdated += LogUserUpdated;
            _client.UserBanned += LogUserBanned;
            _client.UserUnbanned += LogUserPardoned;
            _client.UserJoined += LogUserJoined;
            _client.UserLeft += LogUserLeft;

            // To reduce audit spam for role changes (which often happen in bulk) we spawn a thread to
            // dispatch bunched changes every so often
            Thread thread = new Thread(DispatchRoleChanges);
            thread.Start();

            _data = data;
            _config = config;
            IgnoredChannelIds = _data.Load("ignored_channels", IgnoredChannelIds);
        }

        private async Task SendAudit(string audit, string emote)
        {
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            var socketGuildChannel = _client.GetGuild(_config.BotConfig.GuildId).Channels.FirstOrDefault(x => x.Name.Equals("admin-log"));
            var channel = socketGuildChannel.Guild.GetTextChannel(socketGuildChannel.Id);

            await channel.SendMessageAsync($"{emote} `[{currentTime:hh\\:mm\\:ss}]` {audit}", allowedMentions: AllowedMentions.None);
        }

        private async Task LogMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel)
        {
            if (cachedMessage.Value == null || IgnoredChannelIds.Contains(channel.Id)) return;
            
            var message = await cachedMessage.GetOrDownloadAsync();
            string audit = $"**{message.Author}'s** message in <#{channel.Id}> was deleted. Content: \n{message.Content}";
            foreach (var attachment in message.Attachments)
            {
                audit += $"\n{attachment.Url}";
            }

            await SendAudit(audit, ":wastebasket:");
        }

        private async Task LogMessageUpdated(Cacheable<IMessage, ulong> cachedMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            var message = cachedMessage.Value;
            if (message == null ||
                message.Content == null ||
                IgnoredChannelIds.Contains(channel.Id) ||
                message.Content.Equals(newMessage.Content)) return;

            if (newMessage.Equals($"*{cachedMessage}*")) return;
            
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"**{message.Author}'s** message in <#{channel.Id}> was updated.");
            builder.AppendLine($"**Old content**: {message}");
            builder.AppendLine($"**New content**: {newMessage}");

            await SendAudit(builder.ToString(), ":pencil:");
        }

        private async Task LogGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after)
        {
            var beforeValue = before.Value;
            StringBuilder builder = new StringBuilder();

            if (beforeValue.Roles.Count != after.Roles.Count)
            {
                // Acquire mutex lock for shared lists
                mutex.WaitOne();
                if (cachedUsers.FirstOrDefault(user => user.Id == beforeValue.Id) == null)
                {
                    // Save the cached before user and the new updated user object
                    cachedUsers.Add(beforeValue);
                    updatedUsers.Add(after);
                }
                else
                {
                    // Remove latest reference to updated user and add new one
                    updatedUsers.RemoveAll(user => user.Id == after.Id);
                    updatedUsers.Add(after);
                }
                // Dispatch thread can now take lists if needed
                mutex.ReleaseMutex();
                return;
            }
            else if (beforeValue.Nickname != null || after.Nickname != null)
            {
                builder.Append($"**{beforeValue}'s** ");
                // use the null state of the nickname to determine what nickname was changed to, if changed at all
                if (beforeValue.Nickname == null && after.Nickname != null)
                {
                    builder.Append($"nickname has been changed from **none** to **{after.Nickname}**.");
                }
                else if (beforeValue.Nickname != null && after.Nickname == null)
                {
                    builder.Append($"nickname has been changed from **{beforeValue.Nickname}** to **none**.");
                }
                else if ((beforeValue.Nickname != null && after.Nickname != null) && !beforeValue.Nickname.Equals(after.Nickname))
                {
                    builder.Append($"nickname has been changed from **{beforeValue.Nickname}** to **{after.Nickname}**.");
                }
            }

            // if we have appended something to the string, send it as a log, otherwise do nothing
            if (!builder.ToString().Equals($"**{beforeValue}'s** "))
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
            await SendAudit($"**{user}** joined the server. Total Guild Members: **{user.Guild.MemberCount}**", ":door:");
        }

        private async Task LogUserLeft(SocketGuild guild, SocketUser user)
        {
            await SendAudit($"**{user}** left the server. Total Guild Members: **{guild.MemberCount}**", ":door:");
        }

        private void DispatchRoleChanges()
        {
            while(true)
            {
                // Acquire mutex lock for shared lists
                mutex.WaitOne();
                foreach (var cachedUser in cachedUsers)
                {
                    var user = updatedUsers.First(user => user.Id == cachedUser.Id);
                    StringBuilder audit = new StringBuilder();
                    audit.AppendLine($"**{user}'s** roles have changed.");

                    var rolesGained = user.Roles.Except(cachedUser.Roles).ToList();
                    var rolesLost = cachedUser.Roles.Except(user.Roles).ToList();

                    // Their roles were added/removed at some point but ultimately didn't change
                    if (rolesGained.Count == 0 && rolesLost.Count == 0)
                        continue;

                    if (rolesGained.Count != 0)
                    {
                        audit.Append("Gained: ");
                        rolesGained.ForEach(role => audit.Append($"{role.Mention} "));
                        audit.AppendLine();
                    }

                    if (rolesLost.Count != 0)
                    {
                        audit.Append("Lost: ");
                        rolesLost.ForEach(role => audit.Append($"{role.Mention} "));
                        audit.AppendLine();
                    }

                    audit.Append("Current Roles: ");
                    // We skip 1 because @everyone is always included as first in the roles list
                    user.Roles.Skip(1).ToList().ForEach(role => audit.Append($"{role.Mention} "));
                    SendAudit(audit.ToString(), ":wrench:");
                }

                // Clean up users from queue now that they've been processed
                cachedUsers.Clear();
                updatedUsers.Clear();

                // Make sure mutex is released before thread sleeps
                mutex.ReleaseMutex();
                Thread.Sleep(1000 * 60 * MINUTES_BETWEEN_DISPATCH);
            }
        }
    }
}
