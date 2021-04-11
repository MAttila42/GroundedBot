using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using GroundedBot.Json;
using Discord;
using Discord.WebSocket;
using Octokit;

namespace GroundedBot.Commands
{
    class Contributors
    {
        public static string[] Aliases = 
        {
            "contributors"
        };
        public static string Description = "A command to show dthe contributors of https://github.com/ExAtom/GroundedBot";
        public static string[] Usages = { "contributors" };
        public static string Permission = "Anyone can use it.";
        public static string Trello = "Coming Soon!";
        
        public static async Task DoCommand()
        {
            await Program.Log();

            var message = Recieved.Message;
            string url = "https://api.github.com/repos/ExAtom/GroundedBot/contributors";
            JsonDocument Response;

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), url))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github.v3+json");

                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GroundedBot", "1.0"));
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue("(+https://github.com/ExAtom/GroundedBot)"));

                    var response = await httpClient.SendAsync(request);
                    using(StreamReader reader = new StreamReader(response.Content.ReadAsStream()))
                    {
                        Response = JsonDocument.Parse(reader.ReadToEnd());
                    }
                }
            }

            var rootElement = Response.RootElement;
            var enumerator = rootElement.EnumerateArray();
            List<EmbedFieldBuilder> fieldBuilders = new List<EmbedFieldBuilder>();

            foreach(var idk in enumerator)
            {
                fieldBuilders.Add(new EmbedFieldBuilder().WithIsInline(false)
                                        .WithName(idk.GetProperty("login").ToString())
                                        .WithValue(idk.GetProperty("html_url").ToString()));
            }

            var embed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author.WithName("Contributors of the GroundedBot project")
                          .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/801852346350174208/noun_Information_405516.png"); // Information by Viktor Ostrovsky from the Noun Project
                })
                .WithFields(fieldBuilders)
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x763179)).Build();

            await message.Channel.SendMessageAsync(
                        null,
                        embed: embed)
                        .ConfigureAwait(false);
        }
    }
}