using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GroundedBot.Json;

namespace GroundedBot.Events
{
    class BotMention
    {
        public async static Task DoEvent()
        {
            var message = Recieved.Message;
            ulong id = Program._client.CurrentUser.Id;
            char prefix = BaseConfig.GetConfig().Prefix;
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
                    using (StreamReader reader = new StreamReader(response.Content.ReadAsStream()))
                    {
                        Response = JsonDocument.Parse(reader.ReadToEnd());
                    }
                }
            }

            var rootElement = Response.RootElement;
            var enumerator = rootElement.EnumerateArray();
            List<EmbedFieldBuilder> fieldBuilders = new List<EmbedFieldBuilder>();

            foreach (var idk in enumerator)
            {
                fieldBuilders.Add(new EmbedFieldBuilder().WithIsInline(false)
                    .WithName(idk.GetProperty("login").ToString())
                    .WithValue(idk.GetProperty("html_url").ToString()));
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
                .WithColor(new Color(0x763179)).Build();

            var contEmbed = new EmbedBuilder()
                .WithAuthor(author =>
                {
                    author
                        .WithName("Contributors of the GroundedBot project")
                        .WithIconUrl("https://cdn.discordapp.com/attachments/782305154342322226/801852346350174208/noun_Information_405516.png"); // Information by Viktor Ostrovsky from the Noun Project
                })
                .WithFields(fieldBuilders)
                .WithFooter(((SocketGuildChannel)message.Channel).Guild.Name)
                .WithColor(new Color(0x763179)).Build();

            if ((message.Content == $"<@!{id}>" || message.Content == $"<@{id}>") && Program.BotChannel())
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
