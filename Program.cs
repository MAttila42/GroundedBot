using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Discord;
using Discord.API;
using Discord.Audio;
using Discord.Commands;
using Discord.Net;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using GroundedBot.Commands.Administration;
using GroundedBot.Commands.Fun;
using System.Linq;
using Discord.Audio.Streams;

namespace GroundedBot
{
    public class Role
    {
        public ulong[] Admin { get; set; }
        public ulong[] Mod { get; set; }
        public ulong[] PtanP { get; set; }
    }
    public class Channel
    {
        public ulong[] BotTerminal { get; set; }
    }
    public class BaseConfig
    {
        public string Token { get; set; }
        public char Prefix { get; set; }
        public ulong Owner { get; set; }
        public Role Roles { get; set; }
        public Channel Channels { get; set; }

        public static BaseConfig GetConfig()
        {
            return JsonSerializer.Deserialize<BaseConfig>(File.ReadAllText("BaseConfig.json"));
        }
    }
    class Program
    {
        private static DiscordSocketClient _client;
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

        private async Task<Task> CommandHandler(SocketMessage message)
        {
            if (!message.Content.StartsWith(BaseConfig.GetConfig().Prefix) || message.Author.IsBot)
                return Task.CompletedTask;
            string firstWord = message.Content.Split()[0];
            string command = firstWord.Substring(1, firstWord.Length - 1).ToLower();

            // Administration
            if (Helper.Aliases().Contains(command) && Helper.HasPerm(message))
                Helper.DoCommand(message);

            // Fun
            if (Fleux.Aliases().Contains(command) && Fleux.HasPerm(message))
                Fleux.DoCommand(message);
            if (Minesweeper.Aliases().Contains(command))
                Minesweeper.DoCommand(message);

            await Log("command", message);

            return Task.CompletedTask;
        }

        public static async Task Log(string mode, SocketMessage message)
        {
            Console.Write(DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss") + " ");
            switch (mode)
            {
                case "command":
                    string output = $"Command run - {message.Author.Username}#{message.Author.Discriminator} in #{message.Channel}: {message.Content}";
                    Console.WriteLine(output);
                    foreach (var id in BaseConfig.GetConfig().Channels.BotTerminal)
                        await ((IMessageChannel)_client.GetChannel(id)).SendMessageAsync(output);
                    break;
            }
        }
    }
}
