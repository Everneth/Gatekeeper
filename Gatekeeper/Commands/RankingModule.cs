using Discord.Interactions;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("ranking", "All the commands pertaining to user ranking for the application process.")]
    [RequireRole("High Council (Admin)")]
    public class RankingModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly RankingService _ranking;
        private readonly DataService _data;
        public RankingModule(IServiceProvider services)
        {
            _data = services.GetRequiredService<DataService>();
            _ranking = services.GetRequiredService<RankingService>();
        }
        
        [SlashCommand("score", "Get the user's current application score.")]
        public async Task CheckScore(ulong id = 0)
        {
            if (id != 0)
            {
                var applicant = _ranking.Applicants.SingleOrDefault(u => u.DiscordId == id);
                if(applicant == null)
                {
                    await RespondAsync("User not found!");

                }
                else
                {
                    await RespondAsync(Context.Guild.GetUser(id).Mention + " has " + applicant.Score + " points.");
                }

            }
            else
            {
                await RespondAsync("Please supply a Discord ID. [$.ranking score <id>]");
            }
        }

        [SlashCommand("allscores", "Retrieve all scores for applicants.")]
        public async Task ShowAllScores()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("```asciidoc\n= Current Applicants =\n");
            foreach(var user in _ranking.Applicants.ToList())
            {
                sb.Append(string.Format("{0}#{1} :: {2}\n", user.DiscordUsername, user.Discriminator, user.Score));
            }
            sb.Append("```");
            await RespondAsync(sb.ToString());
        }

        [SlashCommand("clean", "Clean out orphaned data in the ranking cache.")]
        public async Task CleanRankData()
        {
            await RespondAsync(_ranking.Clean(Context.Guild));
        }

        [SlashCommand("reload", "Reload the list of applicant data into memory.")]
        public async Task Reload()
        {
            _data.Load("applicants", _ranking.Applicants);
            await RespondAsync("Applicant list reloaded.");
        }
    }
}
