﻿using Discord.Commands;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("config")]
    public class ConfigModule : JasperBase
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
        public async Task SetBaseCharReq(int amt)
        {
            if (IsStaff())
            {
                int oldAmt = _config.Config.BaseCharReq;
                _config.Config.BaseCharReq = amt;
                _data.Save("config", _config.Config);
                await ReplyAsync("Base characters required updated to **" + amt + "**! OLD: " + oldAmt);
            }
        }

        [Command("basescore")]
        [Summary("Change the initial score awarded for qualified messages.")]
        public async Task SetBaseScore(int amt)
        {
            if (IsStaff())
            {
                int oldAmt = _config.Config.BaseScore;
                _config.Config.BaseScore = amt;
                _data.Save("config", _config.Config);
                await ReplyAsync("Base score for qualified mesages updated to **" + amt + "**! OLD: " + oldAmt);
            }
        }

        [Command("additionalcharscore")]
        [Summary("Change the bonus score awarded for additional characters in a message.")]
        public async Task SetAdditionalCharsScore(int amt)
        {
            if (IsStaff())
            {
                int oldAmt = _config.Config.AdditionalCharsScore;
                _config.Config.AdditionalCharsScore = amt;
                _data.Save("config", _config.Config);
                await ReplyAsync("Score for additional characters past base updated to **" + amt + "**! OLD: " + oldAmt);
            }
        }

        [Command("promothreshold")]
        [Summary("Change the amount of points required for the applicant to reach in order to be promoted to Pending.")]
        public async Task SetPromoThreshold(int amt)
        {
            if (IsStaff())
            {
                int oldAmt = _config.Config.PromoThreshold;
                _config.Config.PromoThreshold = amt;
                _data.Save("config", _config.Config);
                await ReplyAsync("Score threshold for promotion to pending updated to **" + amt + "**! OLD: " + oldAmt);
            }
        }
        [Command("requiredwords")]
        [Summary("Change the amount of words required in a message for it to be scored.")]
        public async Task SetRequiredWords(int amt)
        {
            if (IsStaff())
            {
                int oldAmt = _config.Config.RequiredWords;
                _config.Config.RequiredWords = amt;
                _data.Save("config", _config.Config);
                await ReplyAsync("Required amount of words to score a message updated to **" + amt + "**! OLD: " + oldAmt);
            }
        }
        [Command("show")]
        [Summary("Show the current values that Jasper uses in the formula to score messages.")]
        public async Task ShowConfig()
        {
            if (IsStaff())
            {

                string msg = String.Format("```asciidoc\n" +
                    "= RANKING CONFIG =\n" +
                    "Base Characters Required :: {0}\n" +
                    "Base Score per msg :: {1}\n" +
                    "Score for Additional Chars :: {2}\n" +
                    "Threshold for Promotion :: {3}\n" +
                    "Required words per msg :: {4}```", +
                    _config.Config.BaseCharReq,
                    _config.Config.BaseScore,
                    _config.Config.AdditionalCharsScore,
                    _config.Config.PromoThreshold,
                    _config.Config.RequiredWords);
                await ReplyAsync(msg);
            }
        }
    }
}
