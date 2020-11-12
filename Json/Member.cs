using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GroundedBot.Json
{
    public class Member
    {
        public ulong ID { get; set; }
        public int Floppy { get; set; }
        public int Help { get; set; }
        public string PPlusDate { get; set; }
        public int PPlusRank { get; set; }
        public List<string> Items { get; set; }

        public static List<Member> PullData()
        {
            try { return JsonSerializer.Deserialize<List<Member>>(File.ReadAllText("Members.json")); }
            catch (Exception) { File.WriteAllText("Members.json", "[]"); }
            return JsonSerializer.Deserialize<List<Member>>(File.ReadAllText("Members.json"));
        }
        public static void PushData(List<Member> list)
        {
            File.WriteAllText("Members.json", JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }

        public Member() { } // Ez a sztupid Json deserialize miatt kell. Használd a másik konstruktort!

        public Member(ulong id) // Alap konstruktor, egy ID-t megadsz, és létrehozza a saját objektumát a JSON-ben.
        {
            ID = id;
            Floppy = 0;
            Help = 0;
            PPlusDate = "";
            PPlusRank = 0;
            Items = new List<string>();
        }
        public Member(ulong id, byte help) // Direkt a segítség feljegyzésére létrehozott konstruktor. Kezdeti segítségpont értéket kap.
        {
            ID = id;
            Floppy = 0;
            Help = help;
            PPlusDate = "";
            PPlusRank = 0;
            Items = new List<string>();
        }
        public Member(ulong id, int floppy) // Ugyan olyan, mint az előző, csak Floppy-val.
        {
            ID = id;
            Floppy = floppy;
            Help = 0;
            PPlusDate = "";
            PPlusRank = 0;
            Items = new List<string>();
        }
    }
}
