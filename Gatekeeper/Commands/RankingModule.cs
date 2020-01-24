using Discord.Commands;
using Discord.WebSocket;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("ranking")]
    public class RankingModule : ModuleBase<SocketCommandContext>
    {
        private readonly RankingService _ranking;
        public RankingModule(IServiceProvider services)
        {
            _ranking = services.GetRequiredService<RankingService>();
        }
        
        [Command("score")]
        public async Task CheckScore()
        {
            var role = Context.Guild.Roles.SingleOrDefault(r => r.Name == "Applicant");
            if (Context.Guild.GetUser(Context.User.Id).Roles.Contains(role))
            {
                var applicant = _ranking.Applicants.SingleOrDefault(u => u.DiscordId == Context.User.Id);
                await ReplyAsync(Context.User.Mention + " has " + applicant.Score + " points.");
            }
        }

        [Command("clean")]
        public async Task CleanRankData()
        {
            var role = Context.Guild.Roles.SingleOrDefault(r => r.Name == "Applicant");
            var staffRole = Context.Guild.Roles.SingleOrDefault(r => r.Name == "Staff");
            if (Context.Guild.GetUser(Context.User.Id).Roles.Contains(staffRole))
            {
                int numErrors = 0, numSuccesses = 0;
                foreach (var user in _ranking.Applicants)
                {
                    if (!Context.Guild.GetUser(user.DiscordId).Roles.Contains(role))
                    {
                        if (_ranking.Remove(user))
                        {
                            ++numSuccesses;
                            _ranking.Save();
                        }
                        else
                            ++numErrors;
                    }
                }
                await ReplyAsync(numSuccesses + " orphaned data have been purged. " +
                    numErrors + " errors when processing.");
            }
        }
    }
}
