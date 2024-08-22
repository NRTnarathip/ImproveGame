using HarmonyLib;
using Microsoft.Xml.Serialization.GeneratedAssembly;
using StardewValley;

namespace ImproveGame
{
    [HarmonyPatch]
    class SaveGameFixed
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(XmlSerializationWriter1), "Write168_NPC")]
        static bool Write168_NPC(string n, string ns, NPC o, bool isNullable, bool needType)
        {
            if (o.GetType().Name.Contains("MapCompanion"))
            {
                //Console.WriteLine("nrt debug] npc name: " + o.name);
                //Console.WriteLine("nrt debug] npc type: " + o.GetType());
                //Console.WriteLine("nrt debug] isnullable: " + isNullable);
                //Console.WriteLine("nrt debug] needType: " + needType);
                return false;
            }

            return true;
        }
    }
}
