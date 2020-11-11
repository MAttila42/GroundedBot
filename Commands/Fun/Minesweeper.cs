using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace GroundedBot.Commands.Fun
{
    class Minesweeper
    {
        public static string[] Aliases()
        {
            string[] aliases =
            {
                "minesweeper",
                "ms",
                "aknakereso",
                "aknakereső",
                "ak"
            };
            return aliases;
        }
        private static bool HasPerm(SocketMessage message)
        {
            bool hasPerm = false;
            foreach (var i in (message.Author as SocketGuildUser).Roles)
                if (BaseConfig.GetConfig().Roles.Admin.Contains(i.Id) ||
                    BaseConfig.GetConfig().Roles.Mod.Contains(i.Id) ||
                    BaseConfig.GetConfig().Roles.PtanP.Contains(i.Id))
                {
                    hasPerm = true;
                    break;
                }
            return hasPerm;
        }

        public static async void DoCommand(SocketMessage message)
        {
            Random r = new Random();
            int row = r.Next(6, 15);
            int column = r.Next(6, 15);
            int mines = r.Next(row * column / 10, row * column / 3);
            int help = 1;

            string[] input = message.Content.Split();
            if (!HasPerm(message) && input.Length > 1)
            {
                await message.Channel.SendMessageAsync("❌ Only Ptan+ members can generate custom boards!");
                return;
            }

            try
            {
                if (input.Length >= 2)
                {
                    row = int.Parse(input[1]);
                    mines = r.Next(row * column / 10, row * column / 3);
                }
                if (input.Length >= 3)
                {
                    column = int.Parse(input[2]);
                    mines = r.Next(row * column / 10, row * column / 3);
                }
                if (input.Length >= 4)
                    mines = int.Parse(input[3]);
                if (input.Length >= 5)
                    help = int.Parse(input[4]);
            }
            catch (Exception)
            {
                await message.Channel.SendMessageAsync("❌ Uh, oh! Something's not right.");
                return;
            }

            if (input.Length >= 6)
            {
                await message.Channel.SendMessageAsync("❌ Too many parameters!");
                return;
            }
            else if (row > 20 || column > 20)
            {
                await message.Channel.SendMessageAsync("❌ Don't spam! No more than 20 rows or columns are allowed.");
                return;
            }
            else if (mines > row * column)
            {
                await message.Channel.SendMessageAsync("❌ Too many mines!");
                return;
            }

            int[,] table = new int[row + 2, column + 2];
            for (int i = 0; i < mines; i++)
            {
                int x;
                int y;
                do
                {
                    x = r.Next(1, row + 1);
                    y = r.Next(1, column + 1);
                } while (table[x, y] == -1);

                table[x, y] = -1;
                for (int j = x - 1; j < x + 2; j++)
                    for (int k = y - 1; k < y + 2; k++)
                        if (table[j, k] != -1)
                            table[j, k]++;
            }

            List<string> output = new List<string>();
            bool first = true;
            output.Add($"{row}x{column}, {mines} mine");
            for (int i = 1; i <= row; i++)
            {
                string currentRow = "";
                for (int j = 1; j <= column; j++)
                    switch (table[i, j])
                    {
                        case 0:
                            if (first && help == 1)
                            {
                                currentRow += ":zero:";
                                first = false;
                            }
                            else currentRow += "||:zero:||";
                            break;
                        case 1:
                            currentRow += "||:one:||";
                            break;
                        case 2:
                            currentRow += "||:two:||";
                            break;
                        case 3:
                            currentRow += "||:three:||";
                            break;
                        case 4:
                            currentRow += "||:four:||";
                            break;
                        case 5:
                            currentRow += "||:five:||";
                            break;
                        case 6:
                            currentRow += "||:six:||";
                            break;
                        case 7:
                            currentRow += "||:seven:||";
                            break;
                        case 8:
                            currentRow += "||:eight:||";
                            break;
                        case -1:
                            currentRow += "||:boom:||";
                            break;
                    }
                output.Add(currentRow);
            }

            string msg = "";
            foreach (var i in output)
                msg += i + "\n";

            if (msg.Length > 2000)
                await message.Channel.SendMessageAsync("❌ 2000+ characters!");

            await message.Channel.SendMessageAsync(msg.ToString());
            await Program.Log("command", message);
        }
    }
}
