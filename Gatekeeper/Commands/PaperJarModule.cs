using Discord.Commands;
using Gatekeeper.Helpers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [Group("jar")]
    public class PaperJarModule : JasperBase
    {
        [Command("update")]
        [Summary("Takes a specified minecraft version and updates the main/test server jars to the latest build under that version")]
        public async Task UpdateJar(string minecraftVersion)
        {
            if (!IsAdmin()) return;

            string pattern = @"1.\d+.?\d+";
            if (Regex.IsMatch(minecraftVersion, pattern))
            {
                ScriptHelper.Run("/home/servers/volumes/main/process/update-jar.sh " + minecraftVersion);
                await Context.Channel.SendMessageAsync("Running paper jar update script for minecraft version `" + minecraftVersion + "`.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("That's not a valid minecraft version.");
            }
        }
    }
}
