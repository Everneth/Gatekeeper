using System;
using System.Collections.Generic;
using System.Text;

namespace Gatekeeper.Models
{
    public class EMIPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UUID { get; set; }
        public int MemberId { get; set; }
        public long DiscordId { get; set; }

        public EMIPlayer() {}
        public EMIPlayer(int id, string name, string uuid, int memberid, long discordid)
        {
            Id = id;
            Name = name;
            UUID = uuid;
            MemberId = memberid;
            DiscordId = discordid;
        }
    }
}
