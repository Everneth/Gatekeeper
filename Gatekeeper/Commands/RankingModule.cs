using Discord.Commands;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("ranking")]
    public class RankingModule : JasperBase
    {
        private readonly RankingService _ranking;
        private readonly DataService _data;
        public RankingModule(IServiceProvider services)
        {
            _data = services.GetRequiredService<DataService>();
            _ranking = services.GetRequiredService<RankingService>();
        }
        
        [Command("score")]
        [Summary("Pull up a single applicant's score.")]
        public async Task CheckScore(ulong id = 0)
        {
            var role = Context.Guild.Roles.SingleOrDefault(r => r.Name == "Staff");
            if (Context.Guild.GetUser(Context.User.Id).Roles.Contains(role))
            {
                if (id != 0)
                {
                    var applicant = _ranking.Applicants.SingleOrDefault(u => u.DiscordId == id);
                    if(applicant == null)
                    {
                        await ReplyAsync("User not found!");

                    }
                    else
                    {
                        await ReplyAsync(Context.Guild.GetUser(id).Mention + " has " + applicant.Score + " points.");
                    }

                }
                else
                {
                    
                    await ReplyAsync("Please supply a Discord ID. [$.ranking score <id>]");
                }
            }
        }

        [Command("allscores")]
        [Summary("Retrieve all scores for applicants.")]
        public async Task ShowAllScores()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("```asciidoc\n= Current Applicants =\n");
            foreach(var user in _ranking.Applicants.ToList())
            {
                sb.Append(string.Format("{0}#{1} :: {2}\n", user.DiscordUsername, user.Discriminator, user.Score));
            }
            sb.Append("```");
            await ReplyAsync(sb.ToString());
        }

        [Command("clean")]
        [Summary("Clean out orphaned data in the ranking cache. Use this command if you suspect tracking has stopped working or if several applicants leave the discord.")]
        public async Task CleanRankData()
        {
            
            var staffRole = Context.Guild.Roles.SingleOrDefault(r => r.Name == "Staff");
            if (Context.Guild.GetUser(Context.User.Id).Roles.Contains(staffRole))
            {
                await ReplyAsync(_ranking.Clean(Context.Guild));
            }
        }

        [Command("reload")]
        [Summary("Reload the list of applicant data into memory.")]
        public async Task Reload()
        {
            if (IsStaff())
            {
                _data.Load("applicants", _ranking.Applicants);
                await ReplyAsync("Applicant list reloaded.");
            }
        }
    }
}
