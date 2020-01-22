using Discord.Commands;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    public class RankingModule : ModuleBase<SocketCommandContext>
    {
        private readonly RankingService _ranking;
        public RankingModule(RankingService ranking)
        {
            _ranking = ranking;
        }
        
        [Command("score")]
        public async Task CheckScore()
        {
            var role = Context.Guild.Roles.SingleOrDefault(r => r.Name == "Applicant");
            if (Context.Guild.CurrentUser.Roles.Contains(role))
            {
                var applicant = _ranking.Applicants.SingleOrDefault(u => u.DiscordId == Context.User.Id);
                await ReplyAsync(Context.User.Mention + " has " + applicant.Score + " points.");
            }
        }

    }
}
