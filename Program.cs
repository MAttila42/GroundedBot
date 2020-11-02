using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Discord;
using Discord.WebSocket;
using GroundedBot.Commands.Administration;
using GroundedBot.Commands.Fun;
using System.Linq;

namespace GroundedBot
{
    class BaseConfig
    {
        public string Token { get; set; }
        public char Prefix { get; set; }
        public string Owner { get; set; }

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

            // Administration
            if (Helper.Aliases().Contains(command))
                Helper.DoCommand(message);

            // Fun
            if (Fleux.Aliases().Contains(command))
                Fleux.DoCommand(message);
            if (Minesweeper.Aliases().Contains(command))
                Minesweeper.DoCommand(message);

            return Task.CompletedTask;
        }
    }
}
