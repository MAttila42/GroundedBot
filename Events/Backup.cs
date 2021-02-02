using System;
using Discord;
using GroundedBot.Json;

namespace GroundedBot.Events
{
    class Backup
    {
        public static async void DoEvent()
        {
            await Program.Log("event", "Made a backup.");
            foreach (var id in BaseConfig.GetConfig().Channels.Backups)
                try { await ((IMessageChannel)Program._client.GetChannel(id)).SendFileAsync(@"Members.json"); }
                catch (Exception) { }
        }
    }
}
