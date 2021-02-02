using System;
using Discord;
using GroundedBot.Json;

namespace GroundedBot.Events
{
    class Backup
    {
        public static async void DoEvent()
        {
            foreach (var id in BaseConfig.GetConfig().Channels.Backups)
                try
                {
                    await ((IMessageChannel)Program._client.GetChannel(id)).SendFileAsync(@"Members.json");
                    await Program.Log("event", $"Made a backup in <#{id}>.");
                }
                catch (Exception) { }
        }
    }
}
