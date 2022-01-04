using Discord.Interactions;
using Discord.WebSocket;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Modules
{
    [Group("role", "All commands pertaining to role management.")]
    public class RolesModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RoleService _manager;

        public RolesModule(IServiceProvider services)
        {
            _manager = services.GetRequiredService<RoleService>();
        }

        [SlashCommand("add", "Add a role to the joinable whitelist.")]
        [RequireRole("Staff")]
        private async Task AddRoleAsync(SocketRole role)
        {
            if (_manager.AddRole(role))
                await RespondAsync($"**{role.Name}** added to the whitelist.");
            else
                await RespondAsync($"**{role.Name}** could not be added to the whitelist.");

        }

        [SlashCommand("remove", "Remove a role from the joinable whitelist.")]
        [RequireRole("Staff")]
        private async Task RemoveRoleAsync(SocketRole role)
        {
            if (_manager.RemoveRole(role))
                await RespondAsync($"**{role.Name}** was removed from the whitelist.");
            else
                await RespondAsync($"**{role.Name}** is not on the whitelist.");
        }

        [SlashCommand("join", "Join a role on the joinable whitelist.")]
        private async Task JoinRoleAsync(SocketRole role)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            if (_manager.IsJoinable(role))
            {
                if (!user.Roles.Contains(role))
                {
                    await user.AddRoleAsync(role);
                    await RespondAsync($"{user.Mention}, gave you the **{role.Name}** role.");
                }
            }
            else
            {
                await RespondAsync($"{Context.User.Mention}, that role is not joinable.");
            }
        }

        [SlashCommand("leave", "Leave a role on the joinable whitelist.")]
        private async Task LeaveRoleAsync(SocketRole role)
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (_manager.IsJoinable(role) && user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);
                await RespondAsync($"{user.Mention}, removed the **{role}** role.");
            }
        }

        [SlashCommand("list", "List all joinable roles")]
        private async Task ListRolesAsync()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Here are the joinable roles: ");

            string joinableRoles = _manager.GetJoinableRoles(Context.Guild);
            if (!joinableRoles.Equals(""))
                builder.Append($"**{joinableRoles}**");
            
            await RespondAsync(builder.ToString());
        }
    }
}
