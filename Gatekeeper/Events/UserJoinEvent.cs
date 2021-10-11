using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Gatekeeper.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Gatekeeper.Events
{
    public class UserJoinEvent
    {
        private readonly DiscordSocketClient _client;

        public UserJoinEvent(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _client.UserJoined += JoinGuildEventAsync;
        }

        public async Task JoinGuildEventAsync(SocketGuildUser user)
        {
            if (user.IsBot) return;
            
            var applyChannel = user.Guild.Channels.SingleOrDefault(c => c.Name == "apply") as SocketTextChannel;
            var generalChannel = user.Guild.Channels.SingleOrDefault(c => c.Name == "town-square") as SocketTextChannel;

            if (DatabaseService.PlayerExists(user))
            {
                var roles = user.Guild.Roles.Where(r => r.Name == "Citizen" || r.Name == "Synced");
                await user.AddRolesAsync(roles);
                
                // since users with the citizen role do not have access to #apply, we should notify them in town-square
                await generalChannel.SendMessageAsync("Hi " + user.Mention + "! \nMy name is Jasper. Welcome ***back*** to the Everneth SMP Discord server!\n\n " +
                "I see that you have already been whitelisted and have Citizen'ed you. Have fun!");
            }
            else
            {
                await applyChannel.SendMessageAsync("Hi " + user.Mention + "! \nMy name is Jasper. Welcome to the Everneth SMP Discord server!\n " +
                "To get you started I have some information on how to apply using discord! Just follow these simple steps to get your application in.\n\n" +
                "**1)** Please visit <https://everneth.com/rules/> and at a minimum read Sections 1, 2, 4, and 7. There are more in the rules but the rest of the sections cover rules for staff and how to change our rules.\n" +
                "**2)** Use this channel to run the `!!apply` command.\n" +
                "**3)** Answer the questions asked by the bot and when ready, submit your application!\n" +
                "**4)** Once you put your app in, start chatting in our discord! I will begin keeping tabs on you and will move you to Pending once you have met the requirements! (Shouldn't take you but maybe 30 minutes to an hour!)\n **If you have a friend, they can confirm they know you and you skip to pending!**\n" +
                "**5)** Once you meet requirements, staff will vote on your application in Discord. if approved, you will get changed to Citizen automatically and whitelisted.\n\n" +
                "**And thats it!** Good luck!\n\n ***Jasper** - Your friendly guild bouncer and welcoming committee*\n\n **PS:** __If you are already whitelisted and still receieved this message, please login to the game and use `/discordsync` and link your account!__");
            }
        }
    }
}
