using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using GroundedBot.Json;

namespace GroundedBot.Commands.Dev
{
    class Test
    {
        public static List<ulong> RequiredRoles =
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

            var msg = await message.Channel.SendMessageAsync("teszt");
            //await msg.ModifyAsync(m => m.Content = "edited");

            var oldMessage = await ((IMessageChannel)Program._client.GetChannel(msg.Channel.Id)).GetMessageAsync(msg.Id);
            var oldMsg = (IUserMessage)oldMessage;
            await oldMsg.ModifyAsync(m => m.Content = "edited");

        }
    }
}
