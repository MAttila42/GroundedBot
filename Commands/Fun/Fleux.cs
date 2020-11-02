using Discord.WebSocket;
using System;
using System.Threading;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Discord;
using System.Threading.Channels;
using Discord.Commands;
using Discord.Rest;
using System.Threading.Tasks;

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
        public static string[] Aliases()
        {
            string[] aliases =
            {
                "fleux"
            };
            return aliases;
        }

        public static async void DoCommand(SocketMessage message)
        {
            //parancsadd();
            int bedbi = 0;
            string firstWord = message.Content.Split()[1];

            if (firstWord == "cc")//Törlés
            {
                string amount = message.Content.Split()[2]; // Bekéred a darabot
                int iamount = int.Parse(amount);
                var messages = await message.Channel.GetMessagesAsync(iamount).FlattenAsync();//A darab nélkül mindent töröl.
                    await ((ITextChannel)message.Channel).DeleteMessagesAsync(messages);
                
            }


            if (firstWord == "szabalyzat")
            {
                await message.Channel.SendMessageAsync("A szabályzatot megtalálod a <#770941212948824084> -ben.");

            }
            if (firstWord == "szam")
            {

                var zold = new Emoji("\U0001F7E2");
                var piros = new Emoji("\U0001F7E5");
                await message.Channel.SendMessageAsync("Gondolj egy számra és jegyezd meg.");
                var rMessage = (RestUserMessage)await message.Channel.GetMessageAsync(message.Id);
                await message.AddReactionAsync(zold, new RequestOptions());
                await message.AddReactionAsync(piros, new RequestOptions());

            }


            if (firstWord == "floppy")
            {
                int floppydb = int.Parse(FleuxFloppy.GetConfig().Floppy);
                await message.Channel.SendMessageAsync($"{message.Author}-nak/nek {floppydb} db :floppy_disk: van");

            }
            else if (firstWord == "id")
            {
                await message.Channel.SendMessageAsync($"{message.Author} egyedi azonosítója: {message.Author.Id}");
                dynamic adatok = new JObject();
                adatok.id = message.Author.Id;
                adatok.floppy = 0;

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                File.WriteAllText("cica.json", JsonSerializer.Serialize(adatok.ToString(), options));

                await message.Channel.SendMessageAsync($"Dinamikus JSON hozzáadva.");

            }
            else if (firstWord == "cmd")
            {
                int floppydb = int.Parse(FleuxFloppy.GetConfig().parancsok);
                await message.Channel.SendMessageAsync($"{message.Author} eddig {floppydb} db parancsot adott a botnak.");

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
                await message.Channel.SendMessageAsync($"{message.Author}-nak/nek jóváírva {bedbi} :floppy_disk:");

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
                await message.Channel.SendMessageAsync($"{message.Author}-tól/től levonva {bedbi} :floppy_disk:");

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

