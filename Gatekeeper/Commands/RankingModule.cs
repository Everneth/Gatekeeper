﻿using Discord.Commands;
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
            
            var staffRole = Context.Guild.Roles.SingleOrDefault(r => r.Name == "Staff");
            if (Context.Guild.GetUser(Context.User.Id).Roles.Contains(staffRole))
            {
                await ReplyAsync(_ranking.Clean(Context.Guild));
            }
        }
    }
}