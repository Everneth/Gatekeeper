using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("info")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("getid")]
        public async Task GetDiscordID(string info)
        {
            var users = Context.Guild.Users.Where(u => u.Username.Contains(info)).ToList();
            string dummy = "lol";
            if (users.Count == 0)
            {
                await ReplyAsync("```asciidoc\n= NO RESULTS FOUND =```");
            }
            else
            {
                await ReplyAsync(BuildMessage(users, info));
            }
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
        [Command("getmention")]
        public async Task GetMention(ulong id)
        {
            await ReplyAsync(Context.Guild.GetUser(id).Mention);
        }
    }
}