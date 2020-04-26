using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            EmbedBuilder eb = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                    Name = Context.Client.CurrentUser.Username
                },
                Color = new Color(),
                Title = "Jasper's Commands Help",
                Description = "Change the configuration of Jasper and view players scores.",
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Bot created by @Faceman.",
                    IconUrl = Context.Guild.IconUrl
                }
            }
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.help",
                Value = "List all commands available"
            })
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.ranking score <DiscordID>",
                Value = "Pull up a single applicant's score."
            })
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.ranking allscores",
                Value = "Retrieve all scores for applicants."
            })
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.ranking clean",
                Value = "Clean out orphaned data in the ranking cache. Use this command if you suspect tracking has stopped working or if several applicants leave the discord."
            })
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.config show",
                Value = "Show the current values that Jasper uses in the formula to score messages."
            })
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.config basecharreq <new value>",
                Value = "Change the minimum characters required in each message in order to score."
            })
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.config basescore <new value>",
                Value = "Change the initial score awarded for qualified messages."
            })
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.config additionalcharscore <new value>",
                Value = "Change the bonus score awarded for additional characters in a message."
            })
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.config promothreshold <new value>",
                Value = "Change the amount of points required for the applicant to reach in order to be promoted to Pending."
            })
            .AddField(new EmbedFieldBuilder()
            {
                Name = "$.config requiredwords <new value>",
                Value = "Change the amount of words required in a message for it to be scored."
            });
            await ReplyAsync(embed: eb.Build());
        }
    }
}