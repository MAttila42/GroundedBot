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
        public int Teach { get; set; }
        public string PPlusDate { get; set; }
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

        public Member(ulong id)
        {
            ID = id;
            Floppy = 0;
            Help = 0;
            Teach = 0;
            PPlusDate = "";
            Items = new List<string>();
        }
    }
}
