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
        private readonly HashSet<string> joinableRoles;

        public RoleService(IServiceProvider services)
        {
            _data = services.GetRequiredService<DataService>();
            joinableRoles = _data.Load("roles", joinableRoles);
        }

        public bool AddRole(string roleName, SocketGuild guild)
        {
            bool wasAdded = false;
            if (guild.Roles.SingleOrDefault(x => x.Name == roleName) != null)
            {
                wasAdded = joinableRoles.Add(roleName);
                _data.Save("roles", joinableRoles);
            }

            return wasAdded;
        }

        public bool RemoveRole(string roleName, SocketGuild guild)
        {
            bool wasRemoved = false;
            if (guild.Roles.SingleOrDefault(x => x.Name == roleName) != null)
            {
                wasRemoved = joinableRoles.Remove(roleName);
                _data.Save("roles", joinableRoles);
            }

            return wasRemoved;
        }

        public bool IsJoinable(string role)
        {
            return joinableRoles.Contains(role);
        }

        public string GetJoinableRoles()
        {
            return string.Join(", ", joinableRoles);
        }
    }
}
