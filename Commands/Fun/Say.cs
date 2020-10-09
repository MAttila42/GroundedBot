using Discord.WebSocket;

namespace GroundedBot.Commands.Fun
{
    class Say
    {
        static void ErrorMsg(SocketMessage message)
        {
            message.Channel.SendMessageAsync("Hibás használat!\n`.say Szöveg`");
        }
        public static void DoCommand(SocketMessage message)
        {
            if (message.Content.Length < 6)
            {
                ErrorMsg(message);
                return;
            }

            string command = message.Content.Substring(5, message.Content.Length - 5);

            string msg = "";
            foreach (var i in command)
            {
                msg += i.ToString();
            }

            message.Channel.SendMessageAsync(msg);
        }
    }
}
