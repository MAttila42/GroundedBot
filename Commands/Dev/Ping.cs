using System;
using System.Collections.Generic;
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
            "latency",
            "pong"
        };

        public static async void DoCommand(bool isResponse)
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
                await Program.Log("command");
                Recieved.PingTime = DateTime.Now;
                await message.Channel.SendMessageAsync($"Pinging...");
            }
        }
    }
}
