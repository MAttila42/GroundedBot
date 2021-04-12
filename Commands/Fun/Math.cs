using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GroundedBot.Json;
using Discord;
using Discord.WebSocket;
using NCalc2;

namespace GroundedBot.Commands
{
    public class MathEval
    {
        public static List<ulong> RequiredRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.PtanS);

        public static string[] Aliases =
        {
            "math",
            "math-eval",
            "matheval",
            "eval-math",
            "evalmath",
            "matek"
        };
        public static string Description = "Evaluates simple math expressions";
        public static string[] Usages = { "math <expression>" };
        public static string Permission = "Only members with Ptan+ Budget or better can use it.";
        public static string Trello = "Coming Soon!";

        public async static Task DoCommand()
        {
            await Program.Log();
            var message = Recieved.Message;
            string[] input = message.Content.Split();

            if (!Program.HasPerm(RequiredRoles) && input.Length < 2)
            {
                await message.Channel.SendMessageAsync("❌ Only members with Ptan+ Budget and above can evaluate math expressions!");
                return;
            }

            try
            {
                Expression expr = new Expression(message.ToString().Replace(input[0], ""));

                var embed = new EmbedBuilder()
                    .WithAuthor(author =>
                    {
                        author
                            .WithName("Math evaluation")
                            .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/831126332481470484/noun_Math_3492780.png"); // Math by Clea Doltz from the Noun Project
                    })
                    .WithDescription($"Here is your result: **{expr.Evaluate()}**")
                    .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                    .WithColor(new Color(0x65794F)).Build();

                await message.Channel.SendMessageAsync(
                    null,
                    embed: embed)
                    .ConfigureAwait(false);
            }
            catch (Exception e) { await Program.Log(e); }
        }
    }
}
