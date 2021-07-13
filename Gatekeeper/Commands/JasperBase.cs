using Discord.Commands;
using System;
using System.Linq;

namespace Gatekeeper.Commands
{
    public class JasperBase : ModuleBase<SocketCommandContext>
    {
        public JasperBase()
        {

        }

        protected bool IsStaff()
        {
            var staffRole = Context.Guild.Roles.SingleOrDefault(r => r.Name == "Staff");
            return Context.Guild.GetUser(Context.User.Id).Roles.Contains(staffRole);
        }
    }
}
