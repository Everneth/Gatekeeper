using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gatekeeper.Preconditions
{
    public class RequireRole : PreconditionAttribute
    {
        private readonly string _name;

        public RequireRole(string name) => _name = name;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is SocketGuildUser user)
            {
                if (user.Roles.Any(r => r.Name == _name))
                    return Task.FromResult(PreconditionResult.FromSuccess());
                else
                    return Task.FromResult(PreconditionResult.FromError($"You do not have the required roles to run this command."));
            }
            else
                return Task.FromResult(PreconditionResult.FromError($"You are not in a guild!"));
        }
    }
}
