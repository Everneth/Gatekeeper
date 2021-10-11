﻿using Discord.Commands;
using Gatekeeper.Preconditions;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("config")]
    [RequireRole("High Council (Admin)")]
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        private readonly ConfigService _config;
        private readonly DataService _data;
        public ConfigModule(IServiceProvider services)
        {
            _config = services.GetRequiredService<ConfigService>();
            _data = services.GetRequiredService<DataService>();
        }

        [Command("basecharreq")]
        [Summary("Change the minimum characters required in each message in order to score.")]
        public async Task SetBaseCharReq(int amount)
        {
            int oldAmt = _config.RankingConfig.BaseCharReq;
            _config.RankingConfig.BaseCharReq = amount;
            _data.Save("config", _config.RankingConfig);
            await ReplyAsync("Base characters required updated to **" + amount + "**! OLD: " + oldAmt);
        }

        [Command("basescore")]
        [Summary("Change the initial score awarded for qualified messages.")]
        public async Task SetBaseScore(int amount)
        {
            int oldAmt = _config.RankingConfig.BaseScore;
            _config.RankingConfig.BaseScore = amount;
            _data.Save("config", _config.RankingConfig);
            await ReplyAsync("Base score for qualified mesages updated to **" + amount + "**! OLD: " + oldAmt);
        }

        [Command("additionalcharscore")]
        [Summary("Change the bonus score awarded for additional characters in a message.")]
        public async Task SetAdditionalCharsScore(int amount)
        {
            int oldAmt = _config.RankingConfig.AdditionalCharsScore;
            _config.RankingConfig.AdditionalCharsScore = amount;
            _data.Save("config", _config.RankingConfig);
            await ReplyAsync("Score for additional characters past base updated to **" + amount + "**! OLD: " + oldAmt);
        }

        [Command("promothreshold")]
        [Summary("Change the amount of points required for the applicant to reach in order to be promoted to Pending.")]
        public async Task SetPromoThreshold(int amount)
        {
            int oldAmt = _config.RankingConfig.PromoThreshold;
            _config.RankingConfig.PromoThreshold = amount;
            _data.Save("config", _config.RankingConfig);
            await ReplyAsync("Score threshold for promotion to pending updated to **" + amount + "**! OLD: " + oldAmt);
        }
        [Command("requiredwords")]
        [Summary("Change the amount of words required in a message for it to be scored.")]
        public async Task SetRequiredWords(int amount)
        {
            int oldAmt = _config.RankingConfig.RequiredWords;
            _config.RankingConfig.RequiredWords = amount;
            _data.Save("config", _config.RankingConfig);
            await ReplyAsync("Required amount of words to score a message updated to **" + amount + "**! OLD: " + oldAmt);
        }
        [Command("show")]
        [Summary("Show the current values that Jasper uses in the formula to score messages.")]
        public async Task ShowConfig()
        {
            string msg = String.Format("```asciidoc\n" +
                "= RANKING CONFIG =\n" +
                "Base Characters Required :: {0}\n" +
                "Base Score per msg :: {1}\n" +
                "Score for Additional Chars :: {2}\n" +
                "Threshold for Promotion :: {3}\n" +
                "Required words per msg :: {4}```", +
                _config.RankingConfig.BaseCharReq,
                _config.RankingConfig.BaseScore,
                _config.RankingConfig.AdditionalCharsScore,
                _config.RankingConfig.PromoThreshold,
                _config.RankingConfig.RequiredWords);
            await ReplyAsync(msg);
        }
    }
}
