using System;
using System.Collections.Generic;
using System.Text;

namespace Gatekeeper.Models
{
    public class DiscordToken
    {
        public string Token { get; set; }
        public DiscordToken (string token)
        {
            Token = token;
        }
    }
}
