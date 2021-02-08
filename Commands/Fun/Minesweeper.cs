using System;
using System.Collections.Generic;
using System.Linq;
using GroundedBot.Json;

namespace GroundedBot.Commands
{
    class Minesweeper
    {
        public static List<ulong> RequiredRoles =
            new List<ulong>(BaseConfig.GetConfig().Roles.PtanS);

        public static string[] Aliases =
        {
            "minesweeper",
            "ms",
            "aknakereso",
            "aknakereső",
            "ak"
        };
        public static string Description = "Generates a client-side minesweeper table.";
        public static string[] Usages = { ".minesweeper [row] [column] [mines] [help(0/1)]" };
        public static string Permission = "Anyone can us it, but only members with Ptan+ Standard can create custom boards.";
        public static string Trello = "https://trello.com/c/onWdynw5/1-minesweeper";

        public async static void DoCommand()
        {
            await Program.Log("command", "");

            var message = Recieved.Message;
            Random r = new Random();
            int row = r.Next(6, 10);
            int column = r.Next(6, 10);
            int mines = r.Next(row * column / 10, row * column / 4);
            int help = 1;
            string[] input = message.Content.Split();

            if (!Program.HasPerm(RequiredRoles) && input.Length > 1)
            {
                await message.Channel.SendMessageAsync("❌ Only members with Ptan+ Standard can generate custom boards!");
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

            if (msg.Count(x => x == '|') / 4 > 100)
            {
                await message.Channel.SendMessageAsync("❌ 100+ fields!");
                return;
            }

            await message.Channel.SendMessageAsync(msg);
        }
    }
}
