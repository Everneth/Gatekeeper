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
        private readonly HashSet<ulong> joinableRoleIds;

        public RoleService(IServiceProvider services)
        {
            _data = services.GetRequiredService<DataService>();
            joinableRoleIds = _data.Load("roles", joinableRoleIds);
        }

        public bool AddRole(SocketRole role)
        {
            bool wasAdded = joinableRoleIds.Add(role.Id);
            _data.Save("roles", joinableRoleIds);

            return wasAdded;
        }

        public bool RemoveRole(SocketRole role)
        {
            bool wasRemoved = joinableRoleIds.Remove(role.Id);
            _data.Save("roles", joinableRoleIds);

            return wasRemoved;
        }

        public bool IsJoinable(SocketRole role)
        {
            return joinableRoleIds.Contains(role.Id);
        }

        public string GetJoinableRoles()
        {
            return string.Join(" | ", joinableRoleIds);
        }
    }
}
