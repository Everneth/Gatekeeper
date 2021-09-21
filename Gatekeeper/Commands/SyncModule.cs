using Discord.Commands;
using Discord.WebSocket;
using Gatekeeper.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    public class SyncModule : ModuleBase<SocketCommandContext>
    {
        [Command("sync")]
        [Summary("Checks if you have synced your minecraft account and gives the appropriate roles.")]
        public async Task CheckSync()
        {
            var user = Context.User as SocketGuildUser;
            var roles = user.Guild.Roles.Where(r => r.Name == "Citizen" || r.Name == "Synced");
            if (DatabaseHelper.PlayerExists(user))
            {
                if (roles.Except(user.Roles).Count() > 0)
                {
                    await user.AddRolesAsync(roles);
                    await ReplyAsync($"{user.Mention}, I have given your roles back.");
                }
            }
            else
            {
                await ReplyAsync("You have not synced your minecraft account. Please use `/discordsync` in-game.");
            }
        }
    }
}
