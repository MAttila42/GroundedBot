using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using GroundedBot.Json;

namespace GroundedBot.Commands
{
    class UserInfo
    {
        public static List<ulong> RequiredRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.PtanS);

        public static string[] Aliases =
        {
            "userinfo",
            "user-info",
            "user",
            "memberinfo",
            "member-info",
            "member",
            "rank",
            "level",
            "lvl",
            "xp",
            "szint",
            "balance",
            "bal",
            "money",
            "floppy"
        };
        public async static void DoCommand()
        {
            await Program.Log("command");

            var message = Recieved.Message;
            string[] m = message.Content.Split();
            ulong id = message.Author.Id;
            if (m.Length == 2)
                if (Program.HasPerm(RequiredRoles))
                    id = Program.GetUserId(m[1]);
                else
                {
                    await message.Channel.SendMessageAsync("❌ Only members with Ptan+ Standard can search for other members!");
                    return;
                }
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
            int xp = members[memberIndex].XP;
            int bal = members[memberIndex].Floppy;
            string progressBar = "";
            int partXp = xp;
            int rankup = 30;
            int totalXpNeeded = rankup;
            byte rank = 0;

            List<Members> xpOrderedMembers = new List<Members>();
            foreach (var i in members.OrderByDescending(x => x.XP))
                xpOrderedMembers.Add(i);
            int xpPosition = Members.GetMemberIndex(xpOrderedMembers, id.ToString()) + 1;
            int xpPercent = (int)Math.Ceiling((double)xpPosition / ((SocketGuildChannel)message.Channel).Guild.MemberCount * 100);

            List<Members> balOrderedMembers = new List<Members>();
            foreach (var i in members.OrderByDescending(x => x.Floppy))
                balOrderedMembers.Add(i);
            int balPosition = Members.GetMemberIndex(balOrderedMembers, id.ToString()) + 1;
            int balPercent = (int)Math.Ceiling((double)balPosition / ((SocketGuildChannel)message.Channel).Guild.MemberCount * 100);

            while (partXp >= rankup)
            {
                rank++;
                partXp -= rankup;
                rankup += rankup / 5;
                totalXpNeeded += rankup;
            }

            byte progress = (byte)((double)partXp / rankup * 100);
            progress = (byte)(progress / 100.0 * 32);
            for (int i = 0; i < progress; i++)
                progressBar += "█";
            for (int i = 0; i < 32 - progress; i++)
                progressBar += " ";

            string output =
                ":dollar: __**Balance**__\n" +
                $"­ ­ ­ ­ ­ ­ :trophy: Position: #**{balPosition}** (Top **{balPercent}**%)\n" +
                $"­ ­ ­ ­ ­ ­ :floppy_disk: Floppy: **{bal}**\n" +
                $"\n" +
                $":star: __**Rank**__\n" +
                $"­ ­ ­ ­ ­ ­ :trophy: Position: #**{xpPosition}** (Top **{xpPercent}**%)\n" +
                $"­ ­ ­ ­ ­ ­ :beginner: XP: **{xp}** /{totalXpNeeded}\n" +
                $"­ ­ ­ ­ ­ ­ :medal: Rank: **{rank}**\n" +
                $"\n" +
                $"Progress:\n" +
                $"`{progressBar}`"; // Figyelem! Az üres helyek tele vannak "láthatatlan" karakterekkel.

            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName(Program._client.GetUser(id).Username)
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/791613388769067008/noun_profile_956157.png"); // profile by icongeek from the Noun Project
                })
                .WithDescription(output)
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithThumbnailUrl(Program._client.GetUser(id).GetAvatarUrl())
                .WithColor(new Color(0xFFCC00)).Build();
            await message.Channel.SendMessageAsync(
                null,
                embed: embed)
                .ConfigureAwait(false);
        }
    }
}
