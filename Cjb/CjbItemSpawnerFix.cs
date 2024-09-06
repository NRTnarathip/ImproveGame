namespace ImproveGame.Cjb;
internal static class CjbItemSpawnerFix
{
    public static void TryInit()
    {
        if (ModEntry.IsModLoaded(CjbItemSpawnerAPI.ModID) == false)
            return;

        CjbItemSpawnerAPI.Init();
        CjbSearchBoxFix.Init();
        CjbItemSpawnerUIFix.Init();
    }
}
