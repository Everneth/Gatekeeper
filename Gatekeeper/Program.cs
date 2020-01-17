using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Gatekeeper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Gatekeeper
{
    class Program
    {
		private DiscordSocketClient _client;
		private CommandService _commands;

		public static void Main(string[] args)
		=> new Program().MainAsync().GetAwaiter().GetResult();	

		public async Task MainAsync()
		{
			using (var services = ConfigureServices())
			{
				_client = new DiscordSocketClient();
				var handler = new CommandHandlerService(services);

				_client.Log += Log;

				await _client.LoginAsync(TokenType.Bot,
					Environment.GetEnvironmentVariable("DiscordToken"));
				await _client.StartAsync();

				await services.GetRequiredService<CommandHandlerService>().InstallCommandsAsync();

				// Block this task until the program is closed.
				await Task.Delay(-1);
			}
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		private ServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlerService>()
				.AddSingleton<RankingService>()
				.BuildServiceProvider();
		}
	}
}
