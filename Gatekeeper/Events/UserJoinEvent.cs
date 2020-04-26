using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Gatekeeper.Models;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace Gatekeeper.Events
{
    public class UserJoinEvent
    {
        private readonly DiscordSocketClient _client;
        private IServiceProvider _services;

        public UserJoinEvent(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _client.UserJoined += JoinGuildEventAsync;
        }

        public async Task JoinGuildEventAsync(SocketGuildUser user)
        {
            //var channel = await user.GetOrCreateDMChannelAsync();
            var socketChannel = user.Guild.Channels.SingleOrDefault(c => c.Name == "apply");
            var channel = user.Guild.GetTextChannel(socketChannel.Id);

            if (playerExists(user))
            {
                var role = user.Guild.Roles.SingleOrDefault(r => r.Name == "Citizen");
                await user.AddRoleAsync(role);
                await channel.SendMessageAsync("Hi " + user.Mention + "! \nMy name is Jasper. Welcome ***back*** to the Everneth SMP Discord server!\n\n " +
                "I see that you have already been whitelisted and have Citizen'ed you. Have fun!");
            }
            else
            {
                await channel.SendMessageAsync("Hi " + user.Mention + "! \nMy name is Jasper. Welcome to the Everneth SMP Discord server!\n " +
                "To get you started I have some information on how to apply using discord! Just follow these simple steps to get your application in.\n\n" +
                "**1)** Please visit <https://everneth.com/rules/> and at a minimum read Sections 1, 2, 4, and 7. There are more in the rules but the rest of the sections cover rules for staff and how to change our rules.\n" +
                "**2)** Go to the <#679141347994894336> channel and use the `!!apply` command.\n" +
                "**3)** Answer the questions asked by the bot and when ready, submit your application!\n" +
                "**4)** Once you put your app in, start chatting in our discord! I will begin keeping tabs on you and will move you to Pending once you have met the requirements! (Shouldn't take you but maybe 30 minutes to an hour!)\n **If you have a friend, they can confirm they know you and you skip to pending!**" +
                "**5)** Once you meet requirements, staff will vote on your application in Discord. if approved, you will get changed to Citizen automatically and whitelisted.\n\n" +
                "**And thats it!** Good luck!\n\n ***Jasper** - Your friendly guild bouncer and welcoming committee*\n\n **PS:** __If you are already whitelisted and still receieved this message, please login to the game and use `/discordsync` and link your account!__");
            }
            
            
        }
        private bool playerExists(SocketGuildUser user)
        {
            string cs = String.Format("server=localhost;userid=admin_emi;password={0};database=admin_emi", "qEe4A3U2hw");
            EMIPlayer player = new EMIPlayer();
            using (var con = new MySqlConnection(cs))
            {
                con.Open();

                var stm = String.Format("SELECT * FROM players WHERE discord_id = {0}", user.Id);
                var cmd = new MySqlCommand(stm, con);

                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        while (rdr.Read())
                        {
                            player.Id = rdr.GetInt32(0);
                            player.Name = rdr.GetString(1);
                            player.UUID = rdr.GetString(2);
                            //player.MemberId = rdr.GetInt32(3);
                            player.DiscordId = rdr.GetInt64(4);
                        }
                    }
                }

                if (player.DiscordId == 0 || player == null)
                    return false;
                else
                    return true;
            }
        }
    }
}
