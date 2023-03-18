using System;
using System.Security.Permissions;

namespace Gatekeeper.Models
{
    public class EMIPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid? Uuid { get; set; }
        public string AltName { get; set; }
        public Guid? AltUuid { get; set; }
        public DateTime? AltAdded { get; set; }
        public ulong DiscordId { get; set; }
        public DateTime? CanNextReferFriend { get; set; }

        public EMIPlayer() {}
        public EMIPlayer(int id, string name, Guid? uuid, ulong discordid)
        {
            Id = id;
            Name = name;
            Uuid = uuid;
            DiscordId = discordid;
        }

        public EMIPlayer(int id, string name, Guid? uuid, string altName, Guid? altUuid,
            DateTime? altAdded, ulong discordId, DateTime? canNextReferFriend)
        {
            Id = id;
            Name = name;
            Uuid = uuid;
            AltName = altName;
            AltUuid = altUuid;
            AltAdded = altAdded;
            DiscordId = discordId;
            CanNextReferFriend = canNextReferFriend;
        }
    }
}
