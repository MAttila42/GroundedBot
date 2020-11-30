using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;

namespace GroundedBot.Json
{
    /// <summary>
    /// Answer requesteknek létrehozott adatbázis.
    /// <para>ID, Ping, ChannelID</para>
    /// </summary>
    class AnswerRequests
    {
        public ulong ID { get; set; }
        public ulong UserID { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Answer requestek lekérése a Json-ből.
        /// </summary>
        /// <returns></returns>
        public static List<AnswerRequests> PullData()
        {
            try { return JsonSerializer.Deserialize<List<AnswerRequests>>(File.ReadAllText("AnswerRequests.json")); }
            catch (Exception) { File.WriteAllText("AnswerRequests.json", "[]"); }
            return JsonSerializer.Deserialize<List<AnswerRequests>>(File.ReadAllText("AnswerRequests.json"));
        }
        /// <summary>
        /// Answer requestek feltöltése a Json-be.
        /// </summary>
        /// <param name="list"></param>
        public static void PushData(List<AnswerRequests> list)
        {
            File.WriteAllText("AnswerRequests.json", JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// Ez a sztupid Json deserialize miatt kell. Használd a másik konstruktort!
        /// </summary>
        public AnswerRequests() { }

        /// <summary>
        /// Alap konstruktor az összes, kötelező adattal.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ping"></param>
        public AnswerRequests(ulong id, ulong userId, string description)
        {
            ID = id;
            UserID = userId;
            Description = description;
        }
    }
}
