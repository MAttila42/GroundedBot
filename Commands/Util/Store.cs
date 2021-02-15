using System;
using System.Globalization;
using System.Linq;
using Discord;
using Discord.WebSocket;
using GroundedBot.Json;

namespace GroundedBot.Commands
{
    class Store
    {
        public static string[] Aliases =
        {
            "store",
            "shop",
            "bolt",
            "ptan+",
            "ptan"
        };
        public static string Description = "Store to spend your Floppies. You can buy here Ptan+ and other stuff.";
        public static string[] Usages =
        {
            "store",
            "store buy <név>"
        };
        public static string Permission = "Anyone can use it.";
        public static string Trello = "https://trello.com/c/LCLzeAYa/32-store";
        public async static void DoCommand()
        {
            await Program.Log();
            var message = Recieved.Message;
            string[] m = message.Content.Split();
            var members = Members.PullData();
            var items = Items.PullData();
            string output = "";

            if (m.Length == 1)
            {
                output += $"Buying something works like this: `{BaseConfig.GetConfig().Prefix}store buy <itemName>`.\n\n";

                foreach (var i in items)
                {
                    output += $"**{i.Name}** - **{i.Price}**:floppy_disk:{(i.Type == "ptan+" ? "/month" : "")}\n" +
                        $"*{i.Description}*\n" +
                        $"__You get:__\n";
                    foreach (var j in i.YouGet)
                        output += $"­ - {j}\n";
                    output += "\n";
                }
                if (items.Count == 0)
                    output += "Wow, such empty!";

                var embed = new EmbedBuilder()
                    .WithAuthor(author =>
                    {
                        author
                            .WithName("Store")
                            .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/808302834738135060/noun_Store_840490.png"); // Store by Gregor Cresnar from the Noun Project
                    })
                    .WithDescription(output)
                    .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                    .WithColor(new Color(0x067BCE)).Build();
                await message.Channel.SendMessageAsync(null, embed: embed);
            }
            else if (m[1] == "buy")
            {
                if (m.Length == 2)
                {
                    await message.Channel.SendMessageAsync("Please specify an item you want to buy!");
                    return;
                }

                SocketRole role = null;
                foreach (var i in ((SocketGuildChannel)message.Channel).Guild.GetUser(message.Author.Id).Roles)
                {
                    if (!BaseConfig.GetConfig().Roles.Mod.Contains(i.Id))
                    {
                        if (BaseConfig.GetConfig().Roles.PtanB.Contains(i.Id) ||
                            BaseConfig.GetConfig().Roles.PtanS.Contains(i.Id) ||
                            BaseConfig.GetConfig().Roles.PtanP.Contains(i.Id))
                        {
                            role = i;
                            break;
                        }
                    }
                }

                Members member = members[Members.GetMemberIndex(members, message.Author.Id.ToString())];
                Items item = null;
                try { item = items[items.IndexOf(items.Find(x => x.Name.ToLower() == m[2].ToLower()))]; }
                catch (Exception) { await message.Channel.SendMessageAsync("❌ Unkown item!"); }

                if (item == null)
                {
                    await message.Channel.SendMessageAsync("❌ Unkown item!");
                    return;
                }
                if (member.Floppy < item.Price)
                {
                    await message.Channel.SendMessageAsync("❌ You don't have enough :floppy_disk:!");
                    return;
                }
                if (item.Type == "ptan+")
                {
                    if (!((item.Name.ToLower() == "ptan+budget" && role == null) ||
                        (item.Name.ToLower() == "ptan+standard" && (role == null ||
                        !BaseConfig.GetConfig().Roles.PtanB.Contains(role.Id))) ||
                        (item.Name.ToLower() == "ptan+pro" && (role == null ||
                        !BaseConfig.GetConfig().Roles.PtanB.Contains(role.Id) ||
                        !BaseConfig.GetConfig().Roles.PtanS.Contains(role.Id)))))
                    {
                        await message.Channel.SendMessageAsync($"❌ You already have {role.Name}!");
                        return;
                    }
                }
                Purchase(item);
            }
        }
        static async void Purchase(Items item)
        {
            var message = Recieved.Message;
            var members = Members.PullData();
            int memberIndex = members.IndexOf(members.Find(x => x.ID == message.Author.Id));
            await ((SocketGuildUser)message.Author).AddRoleAsync(Program._client.GetGuild(642864087088234506).Roles.First(x => x.Id == item.RoleID));
            if (item.Type == "ptan+")
                members[memberIndex].PPlusDate = DateTime.Now.AddMonths(1).ToString("dd/MM/yyyy", DateTimeFormatInfo.InvariantInfo);
            members[memberIndex].Floppy -= item.Price;
            //await message.Channel.SendMessageAsync($"**The purchase was successful!**");
            Members.PushData(members);

            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName($"{item.Name}")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/808367856235446332/noun_cart_1832887.png"); // cart by Nanda Ririz from the Noun Project
                })
                .WithDescription("Your purchase was successful!")
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x00DD00)).Build();
            await message.Channel.SendMessageAsync(null, embed: embed);
        }
    }
}
