using System.Threading.Tasks;
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
            if ((message.Content == $"<@!{id}>" || message.Content == $"<@{id}>") && Program.BotChannel())
                await message.Channel.SendMessageAsync($"" +
                    $"Hi, I'm the main Bot of the **ProgramTan** server! I have many features, you can see them with `{prefix}commands`.\n" +
                    $"My prefix is: `{prefix}`\n" +
                    $"Github repository: https://github.com/ExAtom/GroundedBot \n" +
                    $"Trello board: https://trello.com/b/Ns1WcpEB/groundedbot");
        }
    }
}
