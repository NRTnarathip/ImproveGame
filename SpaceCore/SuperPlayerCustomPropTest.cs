using HarmonyLib;
using StardewValley;
using System.Xml.Serialization;

namespace ImproveGame;


public class SuperPlayer
{
    public string playerName = "Noob Nick Name Eiei";
    public List<string> says = new() { "Woooooo", "Moooooooo", "Hoooooooooo" };
}
internal static class SuperPlayerCustomPropTest
{

    static string superPlayer_FieldName = "superPlayer";
    static SuperPlayer superPlayerInstance = new();
    public static SuperPlayer get_superPlayer(this SaveGame save)
    {
        Console.WriteLine("on getter super player: " + superPlayerInstance + ", base on SaveGame: " + save);
        return superPlayerInstance;
    }
    public static void set_superPlayer(this SaveGame save, object newValue)
    {
        Console.WriteLine("on setter super player: " + newValue + ", on save: " + save);
        superPlayerInstance = newValue as SuperPlayer;
    }
    public static void Apply()
    {
        //Regiser Class Type
        CustomSerializer.RegisterCustomProperty(
            typeof(SaveGame),
            superPlayer_FieldName,
            superPlayerInstance.GetType(),
            AccessTools.Method(typeof(SuperPlayerCustomPropTest), nameof(get_superPlayer)),
            AccessTools.Method(typeof(SuperPlayerCustomPropTest), nameof(set_superPlayer))
            );


        //Save Data it
        var saveGame = new SaveGame();
        saveGame.currentSeason = "Hiii Seasons";
        var path = Game1.savesPath + "/TestSaveGame.xml";

        TestWriteSave(saveGame, path);

        var loadSaveGame = TestLoadSave(path);
        Console.WriteLine("new load save game: " + loadSaveGame);
    }
    static void TestWriteSave(SaveGame saveGame, string path)
    {
        var serializer = new XmlSerializer(saveGame.GetType());
        Console.WriteLine("qwe save file path: " + path);
        using var stream = File.OpenWrite(path);
        try
        {
            serializer.Serialize(stream, saveGame);
            Console.WriteLine("done save game");
        }
        catch (Exception ex)
        {
            Console.WriteLine("qwe failed try to save ex: " + ex);
        }
    }
    static SaveGame TestLoadSave(string path)
    {
        var serializer = new XmlSerializer(typeof(SaveGame));
        Console.WriteLine("qwe save file path: " + path);
        using var stream = File.OpenRead(path);
        try
        {
            var save = serializer.Deserialize(stream);
            Console.WriteLine("done loaded save game: " + save);
            return save as SaveGame;
        }
        catch (Exception ex)
        {
            Console.WriteLine("qwe failed try to save ex: " + ex);
        }
        return null;
    }
}
