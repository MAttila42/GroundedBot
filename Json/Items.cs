using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GroundedBot.Json
{
    class Items
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public byte Price { get; set; }
        public string Type { get; set; }
        public string[] YouGet { get; set; }
        public ulong RoleID { get; set; }

        /// <summary>
        /// Tárgyak lekérése a Json-ből.
        /// </summary>
        /// <returns></returns>
        public static List<Items> PullData()
        {
            try { return JsonSerializer.Deserialize<List<Items>>(File.ReadAllText("Items.json")); }
            catch (Exception) { File.WriteAllText("Items.json", "[]"); }
            return JsonSerializer.Deserialize<List<Items>>(File.ReadAllText("Items.json"));
        }
        /// <summary>
        /// Tárgyak feltöltése a Json-be.
        /// </summary>
        /// <param name="list"></param>
        public static void PushData(List<Items> list)
        {
            File.WriteAllText("Items.json", JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));
        }

        public Items() { }
    }
}
