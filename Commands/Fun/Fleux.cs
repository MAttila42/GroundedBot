using Discord.WebSocket;
using System;
using System.Threading;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace GroundedBot.Commands.Fun
{

    class FleuxFloppy
    {
        public string Floppy { get; set; }
        public string parancsok { get; set; }

        public static FleuxFloppy GetConfig()
        {
            return JsonSerializer.Deserialize<FleuxFloppy>(File.ReadAllText("cica.json"));
        }

    }
    public class Fleux
    {


        public static void DoCommand(SocketMessage message)
        {
            parancsadd();
            int bedbi = 0;
            string firstWord = message.Content.Split()[1];
            if (firstWord == "floppy")
            {
                int floppydb = int.Parse(FleuxFloppy.GetConfig().Floppy);
                message.Channel.SendMessageAsync($"{message.Author}-nak/nek {floppydb} db :floppy_disk: van");

            }
            else if ( firstWord == "id")
            {
                message.Channel.SendMessageAsync($"{message.Author} egyedi azonosítója: {message.Author.Id}");
            }
            else if (firstWord == "cmd")
            {
                int floppydb = int.Parse(FleuxFloppy.GetConfig().parancsok);
                message.Channel.SendMessageAsync($"{message.Author} eddig {floppydb} db parancsot adott a botnak.");

            }
            else if (firstWord == "floppyadd")
            {
                try
                {
                    string bedb = message.Content.Split()[2];
                    bedbi = int.Parse(bedb);
                }
                catch (Exception)
                {

                    throw;
                }
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                FleuxFloppy floppyadd = FleuxFloppy.GetConfig();

                int fdb = int.Parse(floppyadd.Floppy);
                fdb += bedbi;
                floppyadd.Floppy = "" + fdb;

                File.WriteAllText("cica.json", JsonSerializer.Serialize(floppyadd, options));
                message.Channel.SendMessageAsync($"{message.Author}-nak/nek jóváírva {bedbi} :floppy_disk:");

            }
            else if (firstWord == "floppyel")
            {
                try
                {
                    string bedb = message.Content.Split()[2];
                    bedbi = int.Parse(bedb);
                }
                catch (Exception)
                {

                    throw;
                }
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                FleuxFloppy floppyadd = FleuxFloppy.GetConfig();

                int fdb = int.Parse(floppyadd.Floppy);
                fdb -= bedbi;
                floppyadd.Floppy = "" + fdb;

                File.WriteAllText("cica.json", JsonSerializer.Serialize(floppyadd, options));
                message.Channel.SendMessageAsync($"{message.Author}-tól/től levonva {bedbi} :floppy_disk:");

            }
        }

        private static void parancsadd()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            FleuxFloppy floppyadd = FleuxFloppy.GetConfig();

            int fdb = int.Parse(floppyadd.parancsok);
            fdb++;
            floppyadd.parancsok = "" + fdb;

            File.WriteAllText("cica.json", JsonSerializer.Serialize(floppyadd, options));
        }
    }
}

