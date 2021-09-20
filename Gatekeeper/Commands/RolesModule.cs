using Discord.Commands;
using Discord.WebSocket;
using Gatekeeper.Commands;
using Gatekeeper.Preconditions;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Modules
{
    [Group("role")]
    public class RolesModule : ModuleBase<SocketCommandContext>
    {
        private readonly RoleService _manager;

        public RolesModule(IServiceProvider services)
        {
            _manager = services.GetRequiredService<RoleService>();
        }

        [Command("add")]
        [RequireRole("Staff")]
        [Summary("Add a role to the joinable whitelist.")]
        private async Task AddRoleAsync(SocketRole role)
        {
            if (_manager.AddRole(role))
                await ReplyAsync($"**{role.Name}** added to the whitelist.");
            else
                await ReplyAsync($"**{role.Name}** could not be added to the whitelist.");

        }

        [Command("remove")]
        [RequireRole("Staff")]
        [Summary("Remove a role from the joinable whitelist.")]
        private async Task RemoveRoleAsync(SocketRole role)
        {
            if (_manager.RemoveRole(role))
                await ReplyAsync($"**{role.Name}** was removed from the whitelist.");
            else
                await ReplyAsync($"**{role.Name}** is not on the whitelist.");
        }

        [Command("join")]
        [Summary("Join a role on the joinable whitelist.")]
        private async Task JoinRoleAsync(SocketRole role)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            if (_manager.IsJoinable(role))
            {
                if (!user.Roles.Contains(role))
                {
                    await user.AddRoleAsync(role);
                    await ReplyAsync($"{user.Mention}, gave you the **{role.Name}** role.");
                }
            }
            else
            {
                await ReplyAsync($"{Context.User.Mention}, that role is not joinable.");
            }
        }

        [Command("leave")]
        [Summary("Leave a role on the joinable whitelist.")]
        private async Task LeaveRoleAsync(SocketRole role)
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (_manager.IsJoinable(role) && user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);
                await ReplyAsync($"{user.Mention}, removed the **{role}** role.");
            }
        }

        [Command("list")]
        [Summary("List all joinable roles")]
        private async Task ListRolesAsync()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Here are the joinable roles: ");

            string joinableRoles = _manager.GetJoinableRoles();
            if (!joinableRoles.Equals(""))
                builder.Append($"**{joinableRoles}**");
            
            await ReplyAsync(builder.ToString());
        }
    }
}
