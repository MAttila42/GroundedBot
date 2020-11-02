using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using Discord.WebSocket;
using GroundedBot.Json;
using Discord.Commands;

namespace GroundedBot.Commands.Administration
{
    public class Helper
    {
        public static void DoCommand(SocketMessage message)
        {
            var members = Members.PullData();
            try { members.Add(new Members(message.Content.Split()[1])); }
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

            message.Channel.SendMessageAsync(output);

            Members.PushData(members);
        }
    }
}
