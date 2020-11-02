using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GroundedBot.Json
{
    public class Members
    {
        public string ID { get; set; }
        public int Floppy { get; set; }
        public int Help { get; set; }
        public int Teach { get; set; }
        public string PPlusDate { get; set; }
        public List<string> Items { get; set; }

        public static List<Members> PullData()
        {
            try { return JsonSerializer.Deserialize<List<Members>>(File.ReadAllText("Members.json")); }
            catch (Exception) { File.WriteAllText("Members.json", "[]"); }
            return JsonSerializer.Deserialize<List<Members>>(File.ReadAllText("Members.json"));
        }
        public static void PushData(List<Members> list)
        {
            File.WriteAllText("Members.json", JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }

        public Members() { } // Ez a sztupid Json deserialize miatt kell. Használd a másik konstruktort!

        public Members(string id)
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
