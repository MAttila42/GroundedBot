using System;
using System.Collections.Generic;
using System.Linq;
using GroundedBot.Json;

namespace GroundedBot.Commands
{
    class Balance
    {
        public static string[] Aliases =
        {
            "balance",
            "bal",
            "money",
            "floppy"
        };

        public static async void DoCommand()
        {
            await Program.Log("command");

            var message = Recieved.Message;
            string[] m = message.Content.Split();
            ulong id = message.Author.Id;
            if (m.Length == 2)
                id = Program.GetUserId(m[1]);
            if (m.Length > 2)
            {
                await message.Channel.SendMessageAsync("❌ Too many parameters!");
                return;
            }
            if (id == 0)
                return;
            var members = Members.PullData();
            int memberIndex = Members.GetMemberIndex(members, id.ToString());
            if (memberIndex == -1)
            {
                memberIndex = members.Count();
                members.Add(new Members(id));
            }
            int bal = members[memberIndex].Floppy;
        }
    }
}
