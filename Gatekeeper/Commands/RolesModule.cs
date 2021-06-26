using Discord.Commands;
using Discord.WebSocket;
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
        [Summary("Add a role to the joinable whitelist.")]
        private async Task AddRoleAsync([Remainder] string roleName)
        {
            if (IsStaff())
            {
                if (_manager.AddRole(roleName, Context.Guild))
                    await ReplyAsync($"**{roleName}** added to the whitelist.");
                else
                    await ReplyAsync($"**{roleName}** could not be added to the whitelist.");

            }
        }

        [Command("remove")]
        [Summary("Remove a role from the joinable whitelist.")]
        private async Task RemoveRoleAsync([Remainder] string roleName)
        {
            if (IsStaff())
            {
                if (_manager.RemoveRole(roleName, Context.Guild))
                    await ReplyAsync($"**{roleName}** was removed from the whitelist.");
                else
                    await ReplyAsync($"**{roleName}** is not on the whitelist.");
            }
        }

        [Command("join")]
        [Summary("Join a role on the joinable whitelist.")]
        private async Task JoinRoleAsync([Remainder] string roleName)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            SocketRole role = Context.Guild.Roles.Where(x => x.Name == roleName).FirstOrDefault();
            if (_manager.IsJoinable(roleName))
            {
                if (!user.Roles.Contains(role))
                {
                    await user.AddRoleAsync(role);
                    await ReplyAsync($"{user.Mention} Gave you the **{role}** role.");
                }
            }
            else
            {
                await ReplyAsync($"{Context.User.Mention} That role is not joinable.");
            }
        }

        [Command("leave")]
        [Summary("Leave a role on the joinable whitelist.")]
        private async Task LeaveRoleAsync([Remainder] string roleName)
        {
            SocketRole role = Context.Guild.Roles.Where(x => x.Name == roleName).FirstOrDefault();
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            if (_manager.IsJoinable(roleName) && user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);
                await ReplyAsync($"{user.Mention} Removed the **{role}** role.");
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

        private bool IsStaff()
        {
            var staffRole = Context.Guild.Roles.SingleOrDefault(x => x.Name == "Staff");
            return Context.Guild.GetUser(Context.User.Id).Roles.Contains(staffRole);
        }
    }
}
