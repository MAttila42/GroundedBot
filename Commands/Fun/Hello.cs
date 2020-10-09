using Discord.WebSocket;

namespace GroundedBot.Commands.Fun
{
    public class Hello
    {
        public static void DoCommand(SocketMessage message)
        {
            message.Channel.SendMessageAsync($"Hello {message.Author.Mention}!");
        }
    }
}
