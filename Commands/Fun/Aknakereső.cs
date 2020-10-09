using System;
using System.Collections.Generic;
using Discord.WebSocket;

namespace GroundedBot.Commands.Fun
{
    class Aknakereső
    {
        static void ErrorMsg(SocketMessage message)
        {
            message.Channel.SendMessageAsync("Hibás használat!\n`.aknakereso <sor> <oszlop> <aknaDb> <segítség[0/1]>`\nAmelyik paraméternek nem adsz értéket, annak a Bot fog véletlenszerűen.");
        }

        public static void DoCommand(SocketMessage message)
        {
            Random r = new Random();
            int sor = r.Next(6, 15);
            int oszlop = r.Next(6, 15);
            int aknaDb = r.Next(sor * oszlop / 10, sor * oszlop / 3);
            int segítség = 1;

            string[] input = message.Content.Split();

            try
            {
                if (input.Length >= 2)
                {
                    sor = int.Parse(input[1]);
                    aknaDb = r.Next(sor * oszlop / 10, sor * oszlop / 3);
                }
                if (input.Length >= 3)
                {
                    oszlop = int.Parse(input[2]);
                    aknaDb = r.Next(sor * oszlop / 10, sor * oszlop / 3);
                }
                if (input.Length >= 4)
                    aknaDb = int.Parse(input[3]);
                if (input.Length >= 5)
                    segítség = int.Parse(input[4]);
            }
            catch (Exception)
            {
                ErrorMsg(message);
                return;
            }

            if (input.Length >= 6 || aknaDb > sor * oszlop)
            {
                ErrorMsg(message);
                return;
            }

            int[,] tábla = new int[sor + 2, oszlop + 2];
            for (int i = 0; i < aknaDb; i++)
            {
                int x;
                int y;
                do
                {
                    x = r.Next(1, sor + 1);
                    y = r.Next(1, oszlop + 1);
                } while (tábla[x, y] == -1);

                tábla[x, y] = -1;
                for (int j = x - 1; j < x + 2; j++)
                    for (int k = y - 1; k < y + 2; k++)
                        if (tábla[j, k] != -1)
                            tábla[j, k]++;
            }

            List<string> ki = new List<string>();
            bool első = true;
            ki.Add($"{sor}x{oszlop}, {aknaDb}db akna");
            for (int i = 1; i <= sor; i++)
            {
                string currentSor = "";
                for (int j = 1; j <= oszlop; j++)
                    switch (tábla[i, j])
                    {
                        case 0:
                            if (első && segítség == 1)
                            {
                                currentSor += ":zero:";
                                első = false;
                            }
                            else currentSor += "||:zero:||";
                            break;
                        case 1:
                            currentSor += "||:one:||";
                            break;
                        case 2:
                            currentSor += "||:two:||";
                            break;
                        case 3:
                            currentSor += "||:three:||";
                            break;
                        case 4:
                            currentSor += "||:four:||";
                            break;
                        case 5:
                            currentSor += "||:five:||";
                            break;
                        case 6:
                            currentSor += "||:six:||";
                            break;
                        case 7:
                            currentSor += "||:seven:||";
                            break;
                        case 8:
                            currentSor += "||:eight:||";
                            break;
                        case -1:
                            currentSor += "||:boom:||";
                            break;
                    }
                ki.Add(currentSor);
            }

            string msg = "";
            foreach (var i in ki)
                msg += i + "\n";

            message.Channel.SendMessageAsync(msg.ToString());
        }
    }
}
