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

        public static async void DoCommand()
        {
            await Program.Log("command");

            var message = Recieved.Message;

            //await message.Channel.SendMessageAsync(((SocketGuildChannel)message.Channel).Guild.IconUrl);
            //var dataList = new Dictionary<ulong, int>();
            //string data = "ExAtom#6674;28&Raffy#6576;24&davidfegyver#8572;22&vrolandd#4645;17&Levev#3306;15&An0#2513;15&AdvancedAntiSkid#0223;14&Pasa#1252;12&CoolAndEasy#2114;11&A kecskék kecskéje#6482;6&Zolkaa#8062;6&karolyia#7695;6&ktom#2134;5&program#6969;5&Nyquist-Shannon-theoren#2749;4&Proci#6377;4&r4dCl0ud#0001;4&Tömi#9175;4&MistScript#4799;3&HMiki#4234;2&v0xel#9999;2&Pelcz#5821;2&LansPeti#2655;2&Jєsυs#1131;2&BigLev#2342;2&Derikehh#4555;2&Fleux#2058;2&Bendi#2924;2&csanszan1#0001;2&Nevem Senki#0029;1&Tibix#3679;1&Dimitri#9956;1&chlkrisz#2217;1&Calimtr7#8615;1&levi#4652;1&vencgotro#2245;1&Gray#6714;1&Sylvester2003#6594;1&kecskécske6#4166;1&Dex#6367;1&( ͡° ͜ʖ ͡°)A n d r i s#4833;1&シナイ#1337;1";
            //var list = new List<string>(data.Split('&'));
            //var counter = 0;
            //foreach (var i in list)
            //{
            //    counter++;
            //    string[] m = i.Split(';');
            //    dataList.Add(Program.GetUserId(m[0]), int.Parse(m[1]));
            //    //await message.Channel.SendMessageAsync($"{counter}. {m[0]} ({Program.GetUserId(m[0])}) - {int.Parse(m[1])}");
            //}
            //var members = Members.PullData();
            //foreach (var i in dataList)
            //{
            //    try
            //    {
            //        members[Members.GetMemberIndex(members, i.Key.ToString())].Floppy += i.Value * 10;
            //    }
            //    catch (Exception)
            //    {
            //        members.Add(new Members(i.Key));
            //        members[Members.GetMemberIndex(members, i.Key.ToString())].Floppy += i.Value * 10;
            //    }
            //}
            //Members.PushData(members);

            var members = Members.PullData();
            try
            {
                switch (message.Content.Split()[1].ToLower())
                {
                    case "rankupfloppy":
                        for (int i = 0; i < members.Count; i++)
                            members[i].Floppy += members[i].Rank;
                        Members.PushData(members);
                        await message.Channel.SendMessageAsync("Szintlépésért járó Floppyk kiosztva.");
                        return;
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
