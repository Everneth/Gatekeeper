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
    [Group("config")]
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        private readonly ConfigService _config;
        public ConfigModule(IServiceProvider services)
        {
            _config = services.GetRequiredService<ConfigService>();
        }
        [Command("basecharreq")]
        public async Task setBaseCharReq(int amt)
        {
            if (IsStaff())
            {
                int oldAmt = _config.Config.BaseCharReq;
                _config.Config.BaseCharReq = amt;
                _config.Save();
                await ReplyAsync("Base characters required updated to **" + amt + "**! OLD: " + oldAmt);
            }
        }
        [Command("basescore")]
        public async Task setBaseScore(int amt)
        {
            if (IsStaff())
            {
                int oldAmt = _config.Config.BaseScore;
                _config.Config.BaseScore = amt;
                _config.Save();
                await ReplyAsync("Base score for qualified mesages updated to **" + amt + "**! OLD: " + oldAmt);
            }
        }
        [Command("additionalcharscore")]
        public async Task setAdditionalCharsScore(int amt)
        {
            if (IsStaff())
            {
                int oldAmt = _config.Config.AdditionalCharsScore;
                _config.Config.AdditionalCharsScore = amt;
                _config.Save();
                await ReplyAsync("Score for additional characters past base updated to **" + amt + "**! OLD: " + oldAmt);
            }
        }
        [Command("basecharreq")]
        public async Task setPromoThreshold(int amt)
        {
            if (IsStaff())
            {
                int oldAmt = _config.Config.PromoThreshold;
                _config.Config.PromoThreshold = amt;
                _config.Save();
                await ReplyAsync("Score threshold for promotion to pending updated to **" + amt + "**! OLD: " + oldAmt);
            }
        }
        [Command("requiredwords")]
        public async Task setRequiredWords(int amt)
        {
            if (IsStaff())
            {
                int oldAmt = _config.Config.RequiredWords;
                _config.Config.RequiredWords = amt;
                _config.Save();
                await ReplyAsync("Required amount of words to score a message updated to **" + amt + "**! OLD: " + oldAmt);
            }
        }
        [Command("show")]
        public async Task showConfig()
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

        private bool IsStaff()
        {
            var staffRole = Context.Guild.Roles.SingleOrDefault(r => r.Name == "Staff");
            if (Context.Guild.GetUser(Context.User.Id).Roles.Contains(staffRole))
                return true;
            else
                return false;
        }
    }
}
