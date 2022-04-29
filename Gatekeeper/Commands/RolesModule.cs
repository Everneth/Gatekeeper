using Discord;
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
    public class RolesModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RoleService _manager;

        public RolesModule(IServiceProvider services)
        {
            _manager = services.GetRequiredService<RoleService>();
        }

        [DefaultMemberPermissions(GuildPermission.ManageRoles)]
        [SlashCommand("whitelist-role", "Add a role to the joinable whitelist.")]
        private async Task AddRoleAsync(SocketRole role)
        {
            if (_manager.AddRole(role))
                await RespondAsync($"**{role.Name}** added to the whitelist.");
            else
                await RespondAsync($"**{role.Name}** could not be added to the whitelist.");

        }

        [DefaultMemberPermissions(GuildPermission.ManageRoles)]
        [SlashCommand("unwhitelist-role", "Remove a role from the joinable whitelist.")]
        private async Task RemoveRoleAsync(SocketRole role)
        {
            if (_manager.RemoveRole(role))
                await RespondAsync($"**{role.Name}** was removed from the whitelist.");
            else
                await RespondAsync($"**{role.Name}** is not on the whitelist.");
        }

        [DefaultMemberPermissions(GuildPermission.SendMessages)]
        [SlashCommand("join-role", "Join a role on the joinable whitelist.")]
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

        [DefaultMemberPermissions(GuildPermission.SendMessages)]
        [SlashCommand("leave-role", "Leave a role on the joinable whitelist.")]
        private async Task LeaveRoleAsync(SocketRole role)
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (_manager.IsJoinable(role) && user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);
                await RespondAsync($"{user.Mention}, removed the **{role}** role.");
            }
        }

        [DefaultMemberPermissions(GuildPermission.SendMessages)]
        [SlashCommand("list-roles", "List all joinable roles")]
        private async Task ListRolesAsync()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Here are the joinable roles:\n");

            string joinableRoles = _manager.GetJoinableRoles(Context.Guild);
            if (!joinableRoles.Equals(""))
                builder.Append($"{joinableRoles}");
            
            await RespondAsync(builder.ToString(), allowedMentions: AllowedMentions.None);
        }
    }
}
