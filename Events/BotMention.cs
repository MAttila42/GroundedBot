using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using GroundedBot.Json;

namespace GroundedBot.Events
{
    class BotMention
    {
        public static async Task DoEvent()
        {
            var message = Recieved.Message;
            ulong id = Program._client.CurrentUser.Id;
            if (!((message.Content == $"<@!{id}>" || message.Content == $"<@{id}>") && Program.BotChannel()))
                return;
            char prefix = BaseConfig.GetConfig().Prefix;
            string responseString = "";
            
            List<EmbedFieldBuilder> fieldBuilders = new List<EmbedFieldBuilder>();
            Embed contEmbed;

            using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), BaseConfig.GetConfig().GithubRepoUrl))
                    {
                        var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"username:{BaseConfig.GetConfig().GithubToken}"));
                        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}"); 
                        request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github.v3+json");

                        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("GroundedBot", "1.0"));
                        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("(+https://github.com/ExAtom/GroundedBot)"));

                        var response = httpClient.SendAsync(request).Result;
                        using(StreamReader reader = new StreamReader(response.Content.ReadAsStream()))
                        {
                            responseString = reader.ReadToEnd();
                        }
                    }
                }
            try
            {
                //looping through the response of the GitHub API
                JArray Response = JArray.Parse(responseString);
                foreach(JObject Contributor in Response)
                {
                    fieldBuilders.Add(new EmbedFieldBuilder().WithIsInline(false)
                            .WithName(Contributor["login"].ToString())
                            .WithValue(Contributor["html_url"].ToString()));
                }
            }
            catch(Exception e) {Program.Log($"```{e}```\n```Response contents: \n{responseString}```").Wait();} //nice log

            if(fieldBuilders.Count == 0)
            {
                contEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Contributors of the GroundedBot project")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/801852346350174208/noun_Information_405516.png"); // Information by Viktor Ostrovsky from the Noun Project
                })
                .WithDescription("Cannot communicate to the GitHub API :cry:")
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x7289DA)).Build();
            }
            else
            {
                contEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Contributors of the GroundedBot project")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/801852346350174208/noun_Information_405516.png"); // Information by Viktor Ostrovsky from the Noun Project
                })
                .WithFields(fieldBuilders)
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x7289DA)).Build();
            }

            var infoEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Information about the Bot")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/801852346350174208/noun_Information_405516.png"); // Information by Viktor Ostrovsky from the Noun Project
                })
                .WithDescription($"" +
                    $"Hi, I'm the main Bot of the **ProgramTan** server! I have many features, you can see them with `{prefix}commands`.\n" +
                    $"My prefix is: `{prefix}`\n" +
                    $"Github repository: https://github.com/ExAtom/GroundedBot \n" +
                    $"Trello board: https://trello.com/b/Ns1WcpEB/groundedbot")
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x7289DA)).Build();

            if((message.Content == $"<@!{id}>" || message.Content == $"<@{id}>") && Program.BotChannel())
            {
                await message.Channel.SendMessageAsync(
                    null,
                    embed: infoEmbed)
                    .ConfigureAwait(false);
                await message.Channel.SendMessageAsync(
                    null,
                    embed: contEmbed)
                    .ConfigureAwait(false);
            }
        }
    }
}
