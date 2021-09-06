using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;
        
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
                Description = "This is a complete list of all the commands you can use.",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Bot created by @Faceman and @Riki.",
                    IconUrl = Context.Guild.IconUrl
                }
            };

            foreach (var module in _commands.Modules)
            {
                if (module.Name.Equals(this.GetType().Name)) continue;

                foreach(var command in module.Commands)
                {
                    // Get the command Summary attribute information
                    string embedFieldText = command.Summary ?? "No description available\n";
                    StringBuilder builder = new StringBuilder();

                    var result = await command.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                    {
                        builder.Append($"$.{module.Name} {command.Name} ");

                        foreach (var parameter in command.Parameters)
                            builder.Append($"[{parameter.Name}]");

                        eb.AddField(builder.ToString(), embedFieldText);
                    }
                }
            }

            await ReplyAsync(embed: eb.Build());
        }
    }
}