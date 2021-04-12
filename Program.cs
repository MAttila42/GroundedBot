using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using GroundedBot.Commands;
using GroundedBot.Events;
using GroundedBot.Json;

namespace GroundedBot
{
    public class Recieved
    {
        public static SocketMessage Message;
        public static DateTime PingTime;
    }

    class Program
    {
        public static DiscordSocketClient _client;
        static void Main() => new Program().MainAsync().GetAwaiter().GetResult();
        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.MessageReceived += MessageHandler;
            _client.UserLeft += LeaveHandler;
            _client.Log += ClientLog;
            _client.Ready += Ready;
            var token = BaseConfig.GetConfig().Token;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task ClientLog(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task MessageHandler(SocketMessage message)
        {
            string firstWord = message.Content.Split()[0];
            bool pong = message.Author.Id == _client.CurrentUser.Id && firstWord == "Pinging...";

            if (pong || (!message.Author.IsBot && !message.Author.IsWebhook))
                Recieved.Message = message;
            else
                return Task.CompletedTask;

            try
            {
                // Events
                BotMention.DoEvent().Wait();
                Xp.DoEvent().Wait();


                if (pong)
                    Ping.DoCommand(true).Wait();
                if (!message.Content.StartsWith(BaseConfig.GetConfig().Prefix) || message.Author.IsBot)
                    return Task.CompletedTask;

                string command = firstWord.Substring(1, firstWord.Length - 1).ToLower();

                // Commands
                // Dev
                if (Evaluate.Aliases.Contains(command) && HasPerm(Evaluate.AllowedRoles))
                    Evaluate.DoCommand().Wait();
                if (Ping.Aliases.Contains(command) && BotChannel())
                    Ping.DoCommand(false).Wait();
                if (Restart.Aliases.Contains(command) && BotChannel() && HasPerm(Restart.AllowedRoles))
                    Restart.DoCommand().Wait();
                if (Test.Aliases.Contains(command) && BotChannel() && HasPerm(Test.AllowedRoles))
                    Test.DoCommand().Wait();
                // Fun
                if (Minesweeper.Aliases.Contains(command) && BotChannel())
                    Minesweeper.DoCommand().Wait();
                if (MathEval.Aliases.Contains(command) && BotChannel())
                    MathEval.DoCommand().Wait();
                // Info
                if (Commands.Commands.Aliases.Contains(command) && BotChannel())
                    Commands.Commands.DoCommand().Wait();
                if (EmojiList.Aliases.Contains(command) && BotChannel())
                    EmojiList.DoCommand().Wait();
                if (Leaderboard.Aliases.Contains(command) && BotChannel())
                    Leaderboard.DoCommand().Wait();
                if (UserInfo.Aliases.Contains(command) && BotChannel())
                    UserInfo.DoCommand().Wait();
                // Util
                if (AnswerRequest.Aliases.Contains(command))
                    AnswerRequest.DoCommand().Wait();
                if (PingRequest.Aliases.Contains(command))
                    PingRequest.DoCommand().Wait();
                if (Store.Aliases.Contains(command) && BotChannel())
                    Store.DoCommand().Wait();
            }
            catch (Exception e)
            {
                foreach (var i in BaseConfig.GetConfig().Channels.BotTerminal)
                    ((IMessageChannel)_client.GetChannel(i)).SendMessageAsync($"```{e}```");
            }
            return Task.CompletedTask;
        }

        private Task LeaveHandler(SocketGuildUser arg)
        {
            RemoveWhoLeft.DoEvent(arg).Wait();
            return Task.CompletedTask;
        }

        private Task Ready()
        {
            HourlyEvents().Wait();
            return Task.CompletedTask;
        }

        private async static Task HourlyEvents()
        {
            while (true)
            {
                Backup.DoEvent().Wait();
                PtanCheck.DoEvent().Wait();
                await Task.Delay(3600000);
            }
        }

        /// <summary>
        /// Parancs logolás a terminálba és a BaseConfigban beállított szobákba.
        /// </summary>
        /// <returns></returns>
        public async static Task Log()
        {
            var message = Recieved.Message;
            Console.Write(DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss") + " ");
            string output = $"Command run - {message.Author.Username}#{message.Author.Discriminator} in #{message.Channel}: {message.Content}";
            foreach (var id in BaseConfig.GetConfig().Channels.BotTerminal)
                try { await ((IMessageChannel)_client.GetChannel(id)).SendMessageAsync(output, allowedMentions: AllowedMentions.None); }
                catch (Exception) { }
            Console.WriteLine(output);
        }
        /// <summary>
        /// Event logolás a terminálba és a BaseConfigban beállított szobákba.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async static Task Log(string text)
        {
            var message = Recieved.Message;
            Console.Write(DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss") + " ");
            string output = $"Event - {text}";
            foreach (var id in BaseConfig.GetConfig().Channels.BotTerminal)
                try { await ((IMessageChannel)_client.GetChannel(id)).SendMessageAsync(output, allowedMentions: AllowedMentions.None); }
                catch (Exception) { }
            Console.WriteLine(output);
        }
        /// <summary>
        /// Ellenőrzi, hogy az üzenetküldőnek van-e megfelelő rangja
        /// </summary>
        /// <param name="allowedRoles"></param>
        /// <returns></returns>
        public static bool HasPerm(List<ulong> allowedRoles)
        {
            foreach (var role in (Recieved.Message.Author as SocketGuildUser).Roles)
                if (allowedRoles.Contains(role.Id) ||
                    BaseConfig.GetConfig().Roles.Admin.Contains(role.Id) ||
                    role.Permissions.Administrator)
                    return true;
            return false;
        }
        /// <summary>
        /// Ellenőrzi, hogy az üzenet bot szobába volt-e küldve.
        /// </summary>
        /// <returns></returns>
        public static bool BotChannel()
        {
            if (BaseConfig.GetConfig().Channels.BotChannel.Contains(Recieved.Message.Channel.Id))
                return true;
            return false;
        }
        /// <summary>
        /// ID, név alapján megkeresi a keresett rangot és visszaadja az ID-jét.
        /// </summary>
        /// <param name="inputName"></param>
        /// <returns></returns>
        public static ulong GetRoleId(string inputName)
        {
            var message = Recieved.Message;
            ulong id = 0;
            try
            {
                id = ulong.Parse(inputName);
                if (((SocketGuildChannel)message.Channel).Guild.GetRole(id) == null)
                    throw new Exception();
            }
            catch (Exception)
            {
                try { id = message.MentionedRoles.First().Id; }
                catch (Exception)
                {
                    var roles = ((SocketGuildChannel)message.Channel).Guild.Roles;
                    foreach (var role in roles)
                        if (inputName.ToLower() == role.Name.ToLower())
                            return role.Id;
                }
            }
            if (id == 0)
                message.Channel.SendMessageAsync("❌ Unknown role!");
            return id;
        }
        /// <summary>
        /// ID, ping, név alapján megkeresi a keresett felhasználót és visszaadja az ID-jét.
        /// </summary>
        /// <param name="inputName"></param>
        /// <returns></returns>
        public static ulong GetUserId(string inputName)
        {
            var message = Recieved.Message;
            ulong id = 0;
            try
            {
                id = ulong.Parse(inputName);
                if (_client.GetUser(id) == null)
                    throw new Exception();
            }
            catch (Exception)
            {
                try { id = message.MentionedUsers.First().Id; }
                catch (Exception)
                {
                    try
                    {
                        var users = ((SocketGuildChannel)message.Channel).Guild.Users;
                        string[] userStr = inputName.Split('#');
                        if (userStr.Length > 2) { return 0; }
                        bool userMissing = true;
                        bool multipleFound = false;
                        if (userStr.Length == 2)
                            foreach (var user in users)
                                if (user.Username == userStr.First() && user.Discriminator == userStr.Last())
                                {
                                    id = user.Id;
                                    userMissing = false;
                                    break;
                                }
                        if (userMissing && userStr.Length == 2)
                        {
                            int usersFound = 0;
                            foreach (var user in users)
                                if (user.Username.ToLower() == userStr.First().ToLower() && user.Discriminator == userStr.Last())
                                {
                                    id = user.Id;
                                    usersFound++;
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
                                if (user.Username == userStr.First())
                                {
                                    id = user.Id;
                                    usersFound++;
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
                                if (user.Username.ToLower() == userStr.First().ToLower())
                                {
                                    id = user.Id;
                                    usersFound++;
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
                            return 0;
                        }
                    }
                    catch (Exception) { return 0; }
                }
            }
            return id;
        }
    }
}
