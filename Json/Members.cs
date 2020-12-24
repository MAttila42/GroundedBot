using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GroundedBot.Json
{
    /// <summary>
    /// Fő adatbázis, a tagok általános adataival.
    /// <para>ID, Floppy, Help, PPlusDate, PPlusRank, Items</para>
    /// </summary>
    class Members
    {
        public ulong ID { get; set; }
        public int Floppy { get; set; }
        public int Help { get; set; }
        public int XP { get; set; }
        public int Rank { get; set; }
        public string PPlusDate { get; set; }

        /// <summary>
        /// Tagok adatainak lekérése a Json-ből.
        /// </summary>
        /// <returns></returns>
        public static List<Members> PullData()
        {
            try { return JsonSerializer.Deserialize<List<Members>>(File.ReadAllText("Members.json")); }
            catch (Exception) { File.WriteAllText("Members.json", "[]"); }
            return JsonSerializer.Deserialize<List<Members>>(File.ReadAllText("Members.json"));
        }
        /// <summary>
        /// Tagok adatainak feltöltése a Json-be.
        /// </summary>
        /// <param name="list"></param>
        public static void PushData(List<Members> list)
        {
            File.WriteAllText("Members.json", JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// A megadott felhasználó indexék megkeresi a beadott adatbázis listából
        /// </summary>
        /// <param name="message"></param>
        /// <param name="list"></param>
        /// <param name="user">ID, Név#0000, Név</param>
        /// <returns></returns>
        public static int GetMemberIndex(List<Members> list, string user) => list.IndexOf(list.Find(x => x.ID == Program.GetUserId(user)));

        /// <summary>
        /// Ez a sztupid Json deserialize miatt kell. Használd a másik konstruktort!
        /// </summary>
        public Members() { }

        /// <summary>
        /// Alap, egyszerű konstruktor, egy ID-t megadsz, és létrehozza a saját objektumát a JSON-ben, üres adatokkal.
        /// </summary>
        /// <param name="id"></param>
        public Members(ulong id)
        {
            ID = id;
            Floppy = 0;
            Help = 0;
            XP = 0;
            Rank = 0;
            PPlusDate = "";
        }
    }
}
