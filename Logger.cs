using StardewModdingAPI;

namespace ImproveGame;

internal static class Logger
{
    static Mod mod;
    public static IMonitor monitor;
    public static void Init(Mod mod)
    {
        Logger.mod = mod;
        monitor = mod.Monitor;
    }

    public static void Log(string msg)
    {
        monitor.Log(msg, LogLevel.Info);
    }
    public static void Log(object msg) => Log(msg.ToString());
    public static void Log(string msg, LogLevel level) => monitor.Log(msg, level);

    public static void Alert(string msg) => Log(msg, LogLevel.Alert);
    public static void Error(string msg) => Log(msg, LogLevel.Error);

}
