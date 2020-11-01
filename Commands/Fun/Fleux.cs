using Discord.WebSocket;
using System;
using System.Threading;

namespace GroundedBot.Commands.Fun
{
    public class Fleux
    {
        public static void DoCommand(SocketMessage message)
        {
            try
            {
                string firstWord = message.Content.Split()[1];
                if (firstWord == "Cica")
                {
                    message.Channel.SendMessageAsync($"*IGEEN, ÜGYES VAGY*");
                }
                else
                {
                    int db = int.Parse(firstWord);

                    for (int i = 0; i < db; i++)
                    {
                        message.Channel.SendMessageAsync($"Hello {message.Author.Mention}!");
                        Thread.Sleep(1000);
                        message.Channel.SendMessageAsync($"*Szerintem cica vagy más állat*");



                    }
                }
            }
            catch (System.Exception)
            {

                message.Channel.SendMessageAsync($"Nem!!!");
            }

            message.Channel.SendMessageAsync($"Nesze");
        
        }
    }
}
