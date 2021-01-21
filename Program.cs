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
            _client.MessageReceived += EventHandler;
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

        private Task EventHandler(SocketMessage message)
        {
            if ((message.Author.Id == BaseConfig.GetConfig().BotID && message.Content == "Pinging...") || !message.Author.IsBot)
                Recieved.Message = message;
            else
                return Task.CompletedTask;

            Xp.DoEvent();

            return Task.CompletedTask;
        }
        private Task CommandHandler(SocketMessage message)
        {
            string firstWord = message.Content.Split()[0];
            if (message.Author.Id == BaseConfig.GetConfig().BotID && firstWord == "Pinging...")
                Ping.DoCommand(true);
            if (!message.Content.StartsWith(BaseConfig.GetConfig().Prefix) || message.Author.IsBot)
                return Task.CompletedTask;
            string command = firstWord.Substring(1, firstWord.Length - 1).ToLower();

            // Dev
            if (Evaluate.Aliases.Contains(command) && HasPerm(Evaluate.AllowedRoles))
                Evaluate.DoCommand();
            if (Ping.Aliases.Contains(command) && BotChannel())
                Ping.DoCommand(false);
            if (Restart.Aliases.Contains(command) && BotChannel() && HasPerm(Restart.AllowedRoles))
                Restart.DoCommand();
            if (Test.Aliases.Contains(command) && BotChannel() && HasPerm(Test.AllowedRoles))
                Test.DoCommand();
            // Fun
            if (Minesweeper.Aliases.Contains(command) && BotChannel())
                Minesweeper.DoCommand();
            // Info
            if (Commands.Commands.Aliases.Contains(command) && BotChannel())
                Commands.Commands.DoCommand();
            if (Leaderboard.Aliases.Contains(command) && BotChannel())
                Leaderboard.DoCommand();
            if (UserInfo.Aliases.Contains(command) && BotChannel())
                UserInfo.DoCommand();
            // Util
            if (AnswerRequest.Aliases.Contains(command))
                AnswerRequest.DoCommand();
            if (PingRequest.Aliases.Contains(command))
                PingRequest.DoCommand();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Meghatározható típusú logolás a terminálba és a BaseConfigban beállított szobákba.
        /// </summary>
        /// <param name="mode">command, rankup</param>
        /// <returns></returns>
        public async static Task Log(string mode)
        {
            var message = Recieved.Message;
            Console.Write(DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss") + " ");
            string output;
            switch (mode)
            {
                case "command":
                    output = $"Command run - {message.Author.Username}#{message.Author.Discriminator} in #{message.Channel}: {message.Content}";
                    break;

                default:
                    return;
            }
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
                    BaseConfig.GetConfig().Roles.Admin.Contains(role.Id))
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
                        if (userStr.Length > 2)
                        {
                            message.Channel.SendMessageAsync("❌ Unknown user!");
                            return 0;
                        }
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
