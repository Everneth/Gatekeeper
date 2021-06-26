using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private CommandService _commands;
        
        public HelpModule(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
        }

        [Command("help")]
        [Summary("List all commands available")]
        public async Task Help()
        {
            EmbedBuilder eb = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                    Name = Context.Client.CurrentUser.Username
                },
                Color = new Color(52, 85, 235),
                Title = "Jasper's Commands Help",
                Description = "Change the configuration of Jasper and view players scores.",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Bot created by @Faceman and @Riki.",
                    IconUrl = Context.Guild.IconUrl
                }
            }
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.help",
                Value = "List all commands available"
            });
            List<CommandInfo> commands = _commands.Commands.ToList();
            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "No description available\n";
                StringBuilder builder = new StringBuilder();

                if (!command.Module.Name.Equals("HelpModule"))
                {
                    builder.Append($"$.{command.Module.Name} ");
                    builder.Append($"{command.Name} ");

                    if (command.Parameters.Count > 0)
                    {
                        builder.Append($"<{string.Join(" ", command.Parameters)}>");
                    }

                    eb.AddField(builder.ToString(), embedFieldText);
                }
            }
            
            await ReplyAsync(embed: eb.Build());
        }
    }
}