using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [DontAutoRegister]
    public class AboutModule : InteractionModuleBase<SocketInteractionContext>
    {
        public AboutModule()
        {

        }
        
        [SlashCommand("about", "General Information about Jasper")]
        public async Task About()
        {
            EmbedBuilder eb = new EmbedBuilder
            {
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Color = new Color(52, 85, 235),
                Title = "All About Jasper",
                Description = "Created through strenuous effort by <@!153863968945995776> and <@!197350115818733569>",
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Currently Running: Version {Program.VERSION}"
                }
            };
            eb.AddField("What am I for?", "I was developed as the primary handler of Discord interactions " +
                "for the Everneth community! As of **Version 2.0** I handle all applications and watch over any " +
                "applicants as they interact with our community, and assign each message they send a 'score' based " +
                "on the quality of messages being sent by the individual. Whenever an applicant's messages send them " +
                "over a points threshold their application is submitted for voting by our staff members for whitelisting!");
            eb.AddField("How are points decided?", "We keep this intentionally vague for our own purposes. A good general rule " +
                "is that low effort messages equate to low scoring messages. We're trying to promote real contributions to " +
                "conversation before we approve applicants so we can get an idea of the kind of person they are!");

            await RespondAsync(embed: eb.Build(), allowedMentions: AllowedMentions.None);
        }
    }
}