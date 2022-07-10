using Discord.WebSocket;
using Gatekeeper.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Services
{
    public class HttpClientService
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly ConfigService _config;
        private readonly DataService _data;
        private HttpClient _restClient;

        public HttpClientService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _config = services.GetRequiredService<ConfigService>();
            _data = services.GetRequiredService<DataService>();
            _restClient = new HttpClient();
            _services = services;
        }

        public async Task<CommandResponse> AddToWhitelist(string name)
        {
            CommandResponse commandResponse = new CommandResponse();
            string path = String.Format($"{_config.BotConfig.ApiServerUri}/cmd/add/{name}?={_config.BotConfig.ApiToken}");
            HttpResponseMessage response = await _restClient.PostAsJsonAsync(path, commandResponse);
            response.EnsureSuccessStatusCode();
                   
            await response.Content.ReadAsAsync<CommandResponse>();

            return commandResponse;
        }

        public async Task<CommandResponse> RemoveFromWhitelist(string name)
        {
            CommandResponse commandResponse = new CommandResponse();
            string path = String.Format($"{_config.BotConfig.ApiServerUri}/cmd/remove/{name}?={_config.BotConfig.ApiToken}");
            HttpResponseMessage response = await _restClient.PostAsJsonAsync(path, commandResponse);
            response.EnsureSuccessStatusCode();

            commandResponse = await response.Content.ReadAsAsync<CommandResponse>();

            return commandResponse;
        }
    }
}
