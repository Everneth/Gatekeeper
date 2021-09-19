using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gatekeeper.Services
{
    public class RoleService
    {
        private readonly DataService _data;
        private readonly HashSet<SocketRole> joinableRoles;

        public RoleService(IServiceProvider services)
        {
            _data = services.GetRequiredService<DataService>();
            joinableRoles = _data.Load("roles", joinableRoles);
        }

        public bool AddRole(SocketRole role, SocketGuild guild)
        {
            bool wasAdded = joinableRoles.Add(role);
            _data.Save("roles", joinableRoles);

            return wasAdded;
        }

        public bool RemoveRole(SocketRole role, SocketGuild guild)
        {
            bool wasRemoved = joinableRoles.Remove(role);
            _data.Save("roles", joinableRoles);

            return wasRemoved;
        }

        public bool IsJoinable(SocketRole role)
        {
            return joinableRoles.Contains(role);
        }

        public string GetJoinableRoles()
        {
            return string.Join(" | ", joinableRoles);
        }
    }
}
