using System;
using System.Threading.Tasks;
using Discord;
using GroundedBot.Json;

namespace GroundedBot.Events
{
    class Backup
    {
        public async static Task DoEvent()
        {
            foreach (var id in BaseConfig.GetConfig().Channels.Backups)
                try
                {
                    await ((IMessageChannel)Program._client.GetChannel(id)).SendFileAsync(@"Members.json");
                    await Program.Log($"Made a backup in <#{id}>");
                }
                catch (Exception) { }
        }
    }
}
