using Discord.Commands;
using Discord.WebSocket;
using Gatekeeper.Services;
using Gatekeeper.Preconditions;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Gatekeeper.Commands
{
    public class SyncModule : ModuleBase<SocketCommandContext>
    {
        private readonly DatabaseService _database;

        public SyncModule(IServiceProvider services)
        {
            _database = services.GetRequiredService<DatabaseService>();
        }

        [Command("sync")]
        [Summary("Checks if you have synced your minecraft account and gives the appropriate roles.")]
        public async Task CheckSync()
        {
            var user = Context.User as SocketGuildUser;
            var roles = user.Guild.Roles.Where(r => r.Name == "Citizen" || r.Name == "Synced");
            if (_database.PlayerExists(user))
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

        [Command("unsync")]
        [RequireRole("Citizen")]
        [Summary("If you have lost access to your minecraft account, you may run this command to remove the sync and allow a sync with another account.")]
        public async Task Unsync()
        {
            var user = Context.User as SocketGuildUser;
            var syncedRole = user.Guild.Roles.FirstOrDefault(r => r.Name == "Synced");
            if (_database.PlayerExists(user))
            {
                if (_database.RemoveSync(user))
                {
                    await user.RemoveRoleAsync(syncedRole.Id);
                    await ReplyAsync("Your discord account has been successfully unsynced. `/discordsync` in-game if you wish to sync another minecraft account.");
                }
                else
                {
                    await ReplyAsync("Your discord account is not synced with a minecraft account.");
                }
            }
        }
    }
}
