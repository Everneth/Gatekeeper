using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gatekeeper.Services
{
    public class DataService
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        public DataService(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
        }

        public void Save<T, U>(T data, U type)
        {
            string path = String.Format(@"Data/{0}.json", data);
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, type);
            }
        }

        public U Load<T, U>(T data, U type)
        {
            string path = String.Format(@"Data/{0}.json", data);
            using (StreamReader file = File.OpenText(path))

            {
                JsonSerializer serializer = new JsonSerializer();
                return (U)serializer.Deserialize(file, typeof(U));
            }
        }
    }
}
