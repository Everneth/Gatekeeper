using Discord;
using Discord.Interactions;
using Gatekeeper.Helpers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gatekeeper.Commands
{
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [Group("jar", "Set of commands that allow the modification of the server jar directly.")]
    public class PaperJarModule : InteractionModuleBase<SocketInteractionContext>
    { 
        [SlashCommand("update", "Update the server jar to the latest release of the specified Minecraft version.")]
        public async Task UpdateJar([Summary("Version", "The version of minecraft you want to run")] string version)
        {
            string pattern = @"^1\.\d+(\.\d+)?$";
            if (Regex.IsMatch(version, pattern))
            {
                ScriptHelper.Run("/home/servers/volumes/main/process/update-jar.sh " + version);
                await RespondAsync("Running paper jar update script for minecraft version `" + version + "`.", ephemeral: true);
            }
            else
            {
                await RespondAsync("That's not a valid minecraft version.", ephemeral: true);
            }
        }
    }
}
