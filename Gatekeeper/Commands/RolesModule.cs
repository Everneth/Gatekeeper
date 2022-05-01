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
        private readonly DiscordSocketClient _client;
        private readonly RoleService _manager;

        public RolesModule(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _manager = services.GetRequiredService<RoleService>();
        }

        [DefaultMemberPermissions(GuildPermission.ManageRoles)]
        [SlashCommand("whitelist-role", "Add a role to the joinable whitelist.")]
        public async Task AddRoleAsync([Summary("Role", "The role you wish to make joinable")] SocketRole role)
        {
            SocketRole highestRole = Context.Guild.CurrentUser.Roles.OrderByDescending(role => role.Position).First();
            if (role.Position >= highestRole.Position)
            {
                await RespondAsync("That role's permissions are greater than mine.", ephemeral: true);
                return;
            }

            if (_manager.AddRole(role))
                await RespondAsync($"**{role.Mention}** added to the whitelist.", allowedMentions: AllowedMentions.None);
            else
                await RespondAsync($"**{role.Mention}** could not be added to the whitelist.", allowedMentions: AllowedMentions.None);

        }

        [DefaultMemberPermissions(GuildPermission.ManageRoles)]
        [SlashCommand("unwhitelist-role", "Remove a role from the joinable whitelist.")]
        public async Task RemoveRoleAsync([Summary("Role", "The role you wish to make unjoinable")] SocketRole role)
        {
            if (_manager.RemoveRole(role))
                await RespondAsync($"**{role.Mention}** was removed from the whitelist.", allowedMentions: AllowedMentions.None);
            else
                await RespondAsync($"**{role.Mention}** is not on the whitelist.", allowedMentions: AllowedMentions.None);
        }

        [DefaultMemberPermissions(GuildPermission.SendMessages)]
        [SlashCommand("join-role", "Join a role on the joinable whitelist.")]
        public async Task JoinRoleAsync([Summary("Role", "The role you wish to join")] SocketRole role)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            if (_manager.IsJoinable(role))
            {
                if (!user.Roles.Contains(role))
                {
                    await user.AddRoleAsync(role);
                    await RespondAsync($"{user.Mention}, gave you the **{role.Mention}** role.", allowedMentions: AllowedMentions.None);
                }
            }
            else
            {
                await RespondAsync($"{Context.User.Mention}, that role is not joinable.");
            }
        }

        [DefaultMemberPermissions(GuildPermission.SendMessages)]
        [SlashCommand("leave-role", "Leave a role on the joinable whitelist.")]
        public async Task LeaveRoleAsync([Summary("Role", "The role you wish to leave")] SocketRole role)
        {
            SocketGuildUser user = Context.User as SocketGuildUser;
            if (_manager.IsJoinable(role) && user.Roles.Contains(role))
            {
                await user.RemoveRoleAsync(role);
                await RespondAsync($"{user.Mention}, removed the **{role.Mention}** role.", allowedMentions: AllowedMentions.None);
            }
        }

        [DefaultMemberPermissions(GuildPermission.SendMessages)]
        [SlashCommand("list-roles", "List all joinable roles")]
        public async Task ListRolesAsync()
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
