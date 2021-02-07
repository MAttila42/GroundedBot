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
                    case "helpfloppy": // Give out the floppies based on the amount of help
                        for (int i = 0; i < members.Count; i++)
                        {
                            members[i].Floppy += members[i].Help;
                            members[i].LastHelp = members[i].Help;
                            members[i].Help = 0;
                        }
                        Members.PushData(members);
                        await message.Channel.SendMessageAsync("Done.");
                        return;

                    case "searchhelps": // List of all the members who helped this month
                        foreach (var i in members.Where(x => x.Help > 0))
                            await message.Channel.SendMessageAsync($"{Program._client.GetUser(i.ID).Mention} - {i.Help}");
                        await message.Channel.SendMessageAsync("Done.", allowedMentions: AllowedMentions.None);
                        return;

                    case "removeduplicates": // Remove duplicated objects from the database
                        var fixedMembers = new List<Members>();
                        foreach (var i in members)
                            if (fixedMembers.Find(x => x.ID == i.ID) == null)
                                fixedMembers.Add(i);
                        Members.PushData(fixedMembers);
                        await message.Channel.SendMessageAsync("Done.");
                        return;

                    case "getaptan+date": // Puts the current date as the expiry date of Ptan+
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
