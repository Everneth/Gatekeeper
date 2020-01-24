using System;
using System.Collections.Generic;
using System.Text;

namespace Gatekeeper.Models
{
    // This class will be serializable to JSON
    public class Applicant
    {
        public ulong DiscordId { get; set; }
        public string DiscordUsername { get; set; }
        public int Discriminator { get; set; }
        public int Score { get; set; }

        public Applicant(ulong id, string username, int discriminator, int score)
        {
            DiscordId = id;
            DiscordUsername = username;
            Discriminator = discriminator;
            Score = score;
        }
    }
}
