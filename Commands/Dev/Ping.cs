using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GroundedBot.Json;
using Discord;

namespace GroundedBot.Commands
{
    class Ping
    {
        public static List<ulong> RequiredRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.PtanB);

        public static string[] Aliases =
        {
            "ping",
            "latency"
        };
        public static string Description = "Calculates the bot's ping.";
        public static string[] Usages = { "ping" };
        public static string Permission = "Anyone can use it.";
        public static string Trello = "https://trello.com/c/2wXQDMBX/31-ping";

        public async static Task DoCommand(bool isResponse)
        {
            var message = Recieved.Message;
            if (message.Content.Split().Length > 1)
                return;

            if (isResponse)
            {
                var latency = DateTime.Now - Recieved.PingTime;
                await ((IUserMessage)message).ModifyAsync(m => m.Content = $"Pong! `{latency.TotalMilliseconds:f0}ms`");
            }
            else
            {
                await Program.Log();
                Recieved.PingTime = DateTime.Now;
                await message.Channel.SendMessageAsync($"Pinging...");
            }
        }
    }
}
