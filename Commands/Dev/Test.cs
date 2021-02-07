using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Discord;
using GroundedBot.Json;

namespace GroundedBot.Commands
{
    class Test
    {
        public static List<ulong> AllowedRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.Admin);

        public static string[] Aliases =
        {
            "test",
            "teszt"
        };
        public static string Description = "A simple command to test stuff.";
        public static string[] Usages = { ".test [made parameters]" };
        public static string Permission = "Only Devs can use it.";
        public static string Trello = "https://trello.com/c/FTc2lM9h/7-test";

        public static async void DoCommand()
        {
            await Program.Log("command", "");

            var message = Recieved.Message;
            var members = Members.PullData();

            try
            {
                switch (message.Content.Split()[1].ToLower())
                {
                    case "helpfloppy":
                        for (int i = 0; i < members.Count; i++)
                        {
                            members[i].Floppy += members[i].Help;
                            members[i].LastHelp = members[i].Help;
                            members[i].Help = 0;
                        }
                        Members.PushData(members);
                        await message.Channel.SendMessageAsync("Done.");
                        return;

                    case "removewholeft":
                        var lefts = new List<Members>();
                        foreach (var i in members)
                            try
                            {
                                if (Program._client.GetUser(i.ID) == null)
                                    lefts.Add(i);
                            }
                            catch (Exception) { lefts.Add(i); }
                        foreach (var i in lefts)
                            members.Remove(i);
                        Members.PushData(members);
                        await message.Channel.SendMessageAsync("Done.");
                        return;

                    case "revertdata":
                        var ranks = new List<string>(File.ReadAllLines("ranks.txt"));
                        var floppies = new List<string>(File.ReadAllLines("floppies.txt"));
                        var ids = new HashSet<ulong>();

                        foreach (var i in ranks)
                            try { ids.Add(Program.GetUserId(i.Split('&')[0])); }
                            catch (Exception) { }
                        foreach (var i in floppies)
                            try { ids.Add(Program.GetUserId(i.Split()[0])); }
                            catch (Exception) { }
                        foreach (var i in ids)
                            if (i != 0)
                                members.Add(new Members(i));

                        foreach (var i in ranks)
                        {
                            try
                            {
                                var memberIndex = members.IndexOf(members.Find(x => x.ID == Program.GetUserId(i.Split('&')[0])));
                                if (memberIndex == -1)
                                    continue;

                                var rank = int.Parse(i.Split('&')[1]);
                                int rankup = 30;
                                int xp = 0;
                                for (int j = 0; j < rank; j++)
                                {
                                    xp += rankup;
                                    rankup += rankup / 5;
                                }

                                members[memberIndex].XP = xp;
                                members[memberIndex].Rank = rank;
                                members[memberIndex].Floppy += rank;
                            }
                            catch (Exception) { }
                        }

                        foreach (var i in floppies)
                        {
                            try
                            {
                                var memberIndex = members.IndexOf(members.Find(x => x.ID == ulong.Parse(i.Split()[0])));
                                if (memberIndex == -1)
                                    continue;
                                var floppy = int.Parse(i.Split()[1]);
                                members[memberIndex].Floppy += floppy * 10;
                            }
                            catch (Exception) { }
                        }

                        Members.PushData(members);
                        await message.Channel.SendMessageAsync("Done.");

                        return;

                    case "searchhelps":
                        foreach (var i in members.Where(x => x.Help > 0))
                            await message.Channel.SendMessageAsync($"{Program._client.GetUser(i.ID).Mention} - {i.Help}");
                        await message.Channel.SendMessageAsync("Done.");
                        return;

                    case "searchlasthelps":
                        foreach (var i in members.Where(x => x.LastHelp > 0))
                            await message.Channel.SendMessageAsync($"{Program._client.GetUser(i.ID).Mention} - {i.LastHelp}");
                        await message.Channel.SendMessageAsync("Done.");
                        return;

                    case "revertlasthelps":
                        foreach (var i in File.ReadAllLines("lasthelps.txt"))
                        {
                            ulong id = ulong.Parse(i.Split()[0]);
                            members.Add(new Members(id));
                            members[members.IndexOf(members.Find(x => x.ID == id))].LastHelp = int.Parse(i.Split()[1]);
                        }
                        Members.PushData(members);
                        await message.Channel.SendMessageAsync("Done.");
                        return;

                    case "removeduplicates":
                        var fixedMembers = new List<Members>();
                        foreach (var i in members)
                            if (fixedMembers.Find(x => x.ID == i.ID) == null)
                                fixedMembers.Add(i);
                        Members.PushData(fixedMembers);
                        await message.Channel.SendMessageAsync("Done.");
                        return;

                    case "getptan+date":
                        members[Members.GetMemberIndex(members, message.Author.Id.ToString())].PPlusDate = DateTime.Now.ToString("dd/MM/yyyy");
                        Members.PushData(members);
                        await message.Channel.SendMessageAsync("Done.");
                        return;

                    default:
                        await message.Channel.SendMessageAsync($"ping||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||{message.Author.Mention}");
                        await message.Channel.SendMessageAsync($"{message.Author.Mention}", allowedMentions: AllowedMentions.None);
                        break;
                }
            }
            catch (Exception)
            {
                await message.Channel.SendMessageAsync($"ping||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||||​||{message.Author.Mention}");
                await message.Channel.SendMessageAsync($"{message.Author.Mention}", allowedMentions: AllowedMentions.None);
            }
        }
    }
}
