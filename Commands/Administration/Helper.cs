using System;
using System.Linq;
using Discord.WebSocket;
using GroundedBot.Json;

namespace GroundedBot.Commands.Administration
{
    public class Helper
    {
        public static string[] Aliases()
        {
            string[] aliases =
            {
                "helper",
                "helped",
                "help"
            };
            return aliases;
        }
        public static bool HasPerm(SocketMessage message)
        {
            bool hasPerm = false;
            foreach (var i in (message.Author as SocketGuildUser).Roles)
                if (BaseConfig.GetConfig().Roles.Admin.Contains(i.Id) ||
                    BaseConfig.GetConfig().Roles.Mod.Contains(i.Id))
                {
                    hasPerm = true;
                    break;
                }
            return hasPerm;
        }
        public static async void DoCommand(SocketMessage message)
        {
            var members = Member.PullData();
            try { members.Add(new Member(ulong.Parse(message.Content.Split()[1]))); }
            catch (Exception) { }

            string output = "";
            foreach (var i in members)
            {
                output += $"{i.ID} ID-jű felhasználónak {i.Help} segítségpontja van.\nItemjei:\n";
                if (i.Items.Count != 0)
                    foreach (var j in i.Items)
                        output += $" - {j}\n";
                else
                    output += " - a no.\n";
                output += "\nyear? ";
                try
                {
                    output += $"{DateTime.Parse(i.PPlusDate).Year}\n";
                }
                catch (Exception)
                {
                    output += "i dunno :(\n";
                }
                output += "\n";
            }

            await message.Channel.SendMessageAsync(output);

            Member.PushData(members);
        }
    }
}
