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
        public static DiscordSocketClient _client;
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
            if (Fleux.Aliases().Contains(command) && Fleux.HasPerm(message))
                Fleux.DoCommand(message);
            if (Minesweeper.Aliases().Contains(command))
                Minesweeper.DoCommand(message);

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
        public static ulong GetUserId(SocketMessage message, string inputName)
        {
            ulong id = 0;
            try { id = ulong.Parse(inputName); }
            catch (Exception)
            {
                try { id = message.MentionedUsers.First().Id; }
                catch (Exception)
                {
                    try
                    {
                        var users = ((SocketGuildChannel)message.Channel).Guild.Users;
                        string[] userStr = inputName.Split('#');
                        if (userStr.Length > 2)
                        {
                            message.Channel.SendMessageAsync("❌ Unknown user!");
                            return 0;
                        }
                        bool userMissing = true;
                        bool multipleFound = false;
                        if (userStr.Length == 2)
                        {
                            foreach (var user in users)
                            {
                                if (user.Username == userStr.First() && user.Discriminator == userStr.Last())
                                {
                                    id = user.Id;
                                    userMissing = false;
                                    break;
                                }
                            }
                        }
                        if (userMissing && userStr.Length == 2)
                        {
                            int usersFound = 0;
                            foreach (var user in users)
                            {
                                if (user.Username.ToLower() == userStr.First().ToLower() && user.Discriminator == userStr.Last())
                                {
                                    id = user.Id;
                                    usersFound++;
                                }
                            }
                            if (usersFound == 1)
                                userMissing = false;
                            else if (usersFound > 1)
                                multipleFound = true;
                        }
                        if (userStr.Length == 1)
                        {
                            int usersFound = 0;
                            foreach (var user in users)
                            {
                                if (user.Username == userStr.First())
                                {
                                    id = user.Id;
                                    usersFound++;
                                }
                            }
                            if (usersFound == 1)
                                userMissing = false;
                            else if (usersFound > 1)
                                multipleFound = true;
                        }
                        if (userMissing && userStr.Length == 1)
                        {
                            int usersFound = 0;
                            foreach (var user in users)
                            {
                                if (user.Username.ToLower() == userStr.First().ToLower())
                                {
                                    id = user.Id;
                                    usersFound++;
                                }
                            }
                            if (usersFound == 1)
                                userMissing = false;
                            else if (usersFound > 1)
                                multipleFound = true;
                        }

                        if (userMissing)
                        {
                            if (multipleFound)
                                message.Channel.SendMessageAsync("❌ Multiple users found!");
                            else
                                message.Channel.SendMessageAsync("❌ Unknown user!");
                            return 0;
                        }
                    }
                    catch (Exception)
                    {
                        message.Channel.SendMessageAsync("❌ Unknown user!");
                        return 0;
                    }
                }
            }
            return id;
        }
    }
}
