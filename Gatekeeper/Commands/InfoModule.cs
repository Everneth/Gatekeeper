using Discord.Interactions;
using Discord.WebSocket;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("info", "All commands pertaining to getting user information")]
    public class InfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DatabaseService _database;
        public InfoModule(IServiceProvider services)
        {
            _database = services.GetRequiredService<DatabaseService>();
        }

        [SlashCommand("getid", "Search users and return a list of IDs")]
        public async Task GetDiscordID(string info)
        {
            var users = Context.Guild.Users.Where(u => u.Username.IndexOf(info, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            
            if (users.Count == 0)
            {
                await ReplyAsync("```asciidoc\n= NO RESULTS FOUND =```");
            }
            else
            {
                await ReplyAsync(BuildMessage(users, info));
            }

        }

        [SlashCommand("getmention", "Return user as a mention for quick access to their account/view roles.")]
        public async Task GetMention(ulong id)
        { 
            await RespondAsync(Context.Guild.GetUser(id).Mention);
        }

        private string BuildMessage(List<SocketGuildUser> users, string search)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("```asciidoc\n= Search results for: {0} =\n", search));
            foreach (var user in users)
            {
                sb.Append(String.Format("{0}#{1} [{2}] :: {3}\n", user.Username, user.Discriminator, user.Nickname, user.Id));
            }
            sb.Append("```");
            return sb.ToString();
        }
    }
}