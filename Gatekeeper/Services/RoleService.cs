using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gatekeeper.Services
{
    public class RoleService
    {
        private readonly DataService _data;
        public HashSet<ulong> JoinableRoleIds { get; private set; }

        public RoleService(IServiceProvider services)
        {
            _data = services.GetRequiredService<DataService>();
            JoinableRoleIds = _data.Load("roles", JoinableRoleIds);
        }

        public bool AddRole(SocketRole role)
        {
            bool wasAdded = JoinableRoleIds.Add(role.Id);
            _data.Save("roles", JoinableRoleIds);

            return wasAdded;
        }

        public bool RemoveRole(SocketRole role)
        {
            bool wasRemoved = JoinableRoleIds.Remove(role.Id);
            _data.Save("roles", JoinableRoleIds);

            return wasRemoved;
        }

        public bool IsJoinable(SocketRole role)
        {
            return JoinableRoleIds.Contains(role.Id);
        }

        public string GetJoinableRoles(SocketGuild guild)
        {
            List<string> joinableRoles = new List<string>();
            foreach (var id in JoinableRoleIds)
            {
                joinableRoles.Add(guild.GetRole(id).Mention);
            }
            return string.Join(" | ", joinableRoles);
        }
    }
}
