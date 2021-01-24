using System;
using System.Collections.Generic;
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
                        await message.Channel.SendMessageAsync("Segítségért járó Floppyk kiosztva.");
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
                        await message.Channel.SendMessageAsync("Lelépett tagok adatai törölve.");
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
