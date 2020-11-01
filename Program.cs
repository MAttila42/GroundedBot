using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

using Discord;
using Discord.WebSocket;

using GroundedBot.Commands.Fun;

namespace GroundedBot
{
    class BaseConfig
    {
        public string Token { get; set; }
        public char Prefix { get; set; }
        public string[] Commands { get; set; }

        public static BaseConfig GetConfig()
        {
            return JsonSerializer.Deserialize<BaseConfig>(File.ReadAllText("BaseConfig.json"));
        }
    }
    class Program
    {
        private DiscordSocketClient _client;

        static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.MessageReceived += CommandHandler;
            _client.Log += Log;

            var token = BaseConfig.GetConfig().Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task CommandHandler(SocketMessage message)
        {
            if (!message.Content.StartsWith(BaseConfig.GetConfig().Prefix) || message.Author.IsBot)
                return Task.CompletedTask;
            string firstWord = message.Content.Split()[0];
            string command = firstWord.Substring(1, firstWord.Length - 1).ToLower();

            if (BaseConfig.GetConfig().Commands.Contains(command))
                switch (command)
                {
                    // Fun
                    case "aknakereso":
                        Aknakereső.DoCommand(message);
                        break;
                    case "hello":
                        Hello.DoCommand(message);
                        break;
                    case "say":
                        Say.DoCommand(message);
                        break;
                    case "fleux":
                        Fleux.DoCommand(message);
                        break;
                }

            return Task.CompletedTask;
        }
    }
}
