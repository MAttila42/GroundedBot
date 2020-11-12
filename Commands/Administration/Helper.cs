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
                "helped"
            };
            return aliases;
        }
        static bool HasPerm(SocketMessage message)
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
            string[] m = message.Content.Split();
            string subCommand;

            try { subCommand = m[1]; }
            catch (Exception)
            {
                await message.Channel.SendMessageAsync("❌ Missing parameters!");
                return;
            }

            switch (subCommand)
            {
                case "test":
                    break;
                case "request":
                    break;
                case "approve":
                    if (HasPerm(message))
                        Approve(message);
                    break;
                default:
                    await message.Channel.SendMessageAsync("❌ Unkown subcommand!");
                    break;
            }
        }
        static async void Approve(SocketMessage message)
        {
            var members = Member.PullData();
            string[] m = message.Content.Split();
            byte help;

            if (m.Length < 4 || m.Length > 4 || message.MentionedUsers.Count() > 1)
            {
                await message.Channel.SendMessageAsync($"❌ {(m.Length < 4 ? "Missing" : "Too many")} parameters!");
                return;
            }
            try { help = byte.Parse(m[3]); }
            catch (Exception)
            {
                await message.Channel.SendMessageAsync("❌ Invalid score!");
                return;
            }
            if (help < 1 || help > 5)
            {
                await message.Channel.SendMessageAsync("❌ Invalid score!");
                return;
            }

            ulong id = Program.GetUserId(message, m[2]);
            if (id == 0)
                return;

            try { members[members.IndexOf(members.Find(x => x.ID == id))].Help += help; }
            catch (Exception) { members.Add(new Member(id, help)); }

            try
            {
                var user = Program._client.GetUser(id);
                await message.Channel.SendMessageAsync($"**{user.Username}**'s helping has been approved with the score: **{help}**.");
            }
            catch (Exception)
            {
                await message.Channel.SendMessageAsync("❌ Unknown user!");
                return;
            }

            Member.PushData(members);
        }
    }
}
