using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gatekeeper.Models
{
    public class CommandResponse
    {
        public string PlayerName { get; set; }
        public string Command { get; set; }
        public string Message { get; set; }
        public CommandResponse() { }
        public CommandResponse(string playerName, string command, string message)
        {
            PlayerName = playerName;
            Command = command;
            Message = message;
        }

        public Embed RespondAsEmbed(SocketInteractionContext context)
        {
            EmbedBuilder eb = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = context.Client.CurrentUser.GetAvatarUrl(),
                    Name = context.Client.CurrentUser.Username
                },
                Color = new Color(52, 85, 235),
                Title = "Command Executed",
                Description = "This message was returned by the API.",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Bot created by @Faceman and @Riki.",
                    IconUrl = context.Guild.IconUrl
                }
            };
            eb.AddField(new EmbedFieldBuilder()
            {
                Name = "Minecraft Name",
                Value = PlayerName,
                IsInline = true
            }).AddField(new EmbedFieldBuilder()
            {
                Name = "Command Run",
                Value = Command,
                IsInline = true
            }).AddField(new EmbedFieldBuilder()
            {
                Name = "Message returned from the API",
                Value = Message,
                IsInline = false
            });
            return eb.Build();
        }
    }
}
