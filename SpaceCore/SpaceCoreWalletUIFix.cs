using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;

namespace ImproveGame;

[HarmonyPatch]
class SpaceCoreWalletUIFix : BasePatcher
{
    static SpaceCoreWalletUIFix Instance;
    public static void Init()
    {
        Instance = new SpaceCoreWalletUIFix();
    }

    SpaceCoreWalletUIFix()
    {
        NewSkillsPage = GetType("SpaceCore", "SpaceCore.Interface.NewSkillsPage");
        PatchPostfix(
            GetConstructor(NewSkillsPage, [typeof(int), typeof(int), typeof(int), typeof(int)]),
            nameof(Fix_SkillsPage_WalletUI));
    }

    static Type NewSkillsPage;
    static readonly Rectangle WalletIconAreaSource = new(293, 360, 24, 24);
    const string CustomSkillPrefix = "C";
    const int SkillRegionStartId = 0;
    const int SkillIdIncrement = 1;
    const int SkillProfessionIncrement = 100;
    const int WalletRegionStartId = 10250;
    const int WalletIdIncrement = 1;
    const int WalletUpArrowRegionId = 10201;
    const int WalletDownArrowRegionId = 10202;
    const int PlayerPanelRegionId = 10275;

    static void Fix_SkillsPage_WalletUI(object __instance)
    {
        var gameMenu = Game1.activeClickableMenu as GameMenu;
        var skillPage = __instance;

        var xPositionOnScreen = gameMenu.xPositionOnScreen;
        var yPositionOnScreen = gameMenu.yPositionOnScreen;
        var heightMod = gameMenu.heightMod;
        var widthMod = gameMenu.widthMod;
        var width = gameMenu.width;
        var height = gameMenu.height;


        // Wallet conatiner UI
        int iconAreaWidth = WalletIconAreaSource.Width * Game1.pixelZoom;
        int iconAreaHeight = WalletIconAreaSource.Height * Game1.pixelZoom;
        int walletWidth = 72 + iconAreaHeight;
        int walletHeight = height;
        const int walletXOffset = -100;//addjust here offset from right side
        int walletX = xPositionOnScreen + width - walletWidth + walletXOffset; // from right side
        int walletY = yPositionOnScreen;

        var walletArea = new Rectangle(walletX, walletY, walletWidth, walletHeight);
        Instance.WriteField(skillPage, "walletArea", walletArea);

        var walletIconArea = new Rectangle(walletArea.X + ((walletArea.Width - iconAreaWidth) / 2),
            walletArea.Y + (iconAreaHeight / 2) - 8, iconAreaWidth, iconAreaHeight);
        Instance.WriteField(skillPage, "walletIconArea", walletIconArea);


        // Wallet item icons for navigation

        var iconWidth = 32;
        var walletUpArrow = new ClickableTextureComponent(
            name: "", bounds: new Rectangle(walletArea.X + ((walletArea.Width - iconWidth) / 2), walletArea.Y + 148, iconWidth, iconWidth),
            label: null, hoverText: null,
            texture: Game1.mouseCursors, sourceRect: new Rectangle(442, 96, iconWidth, iconWidth), scale: 1f, drawShadow: false);
        var walletDownArrow = new ClickableTextureComponent(
            name: "", bounds: new Rectangle(walletUpArrow.bounds.X, walletUpArrow.bounds.Y + walletArea.Height - 224, walletUpArrow.bounds.Width, walletUpArrow.bounds.Height),
            label: null, hoverText: null,
            texture: walletUpArrow.texture, sourceRect: walletUpArrow.sourceRect, scale: walletUpArrow.scale, drawShadow: walletUpArrow.drawShadow);



        // Wallet item icons for navigation
        const int padRight = 4;
        const int padTop = 16;

        iconWidth = 16;
        int iconCount = (walletDownArrow.bounds.Y - walletUpArrow.bounds.Y - (padTop * 2)) / iconWidth / Game1.pixelZoom;
        var specialItems = Instance.ReadField(skillPage, "specialItems") as List<ClickableTextureComponent>;
        var specialItemComponents = Instance.ReadField(skillPage, "specialItemComponents") as List<ClickableTextureComponent>;
        bool shouldNavButtonsBeShown = specialItems.Count > iconCount;

        for (int index = 0; index < iconCount; ++index)
        {
            int iconDestWidth = iconWidth * Game1.pixelZoom;
            int x = walletUpArrow.bounds.X
                + (walletUpArrow.bounds.Width / 2) - (iconDestWidth / 2);
            int y = walletUpArrow.bounds.Y
                + (specialItems.Count > iconCount ? walletUpArrow.bounds.Height : 0) + padTop + (index * iconDestWidth);

            var textureComponent = new ClickableTextureComponent(
                name: "",
                bounds: new Rectangle(x, y, iconDestWidth, iconDestWidth),
                label: "", hoverText: "",
                texture: Game1.mouseCursors, sourceRect: new Rectangle(-1, -1, iconWidth, iconWidth),
                scale: Game1.pixelZoom, drawShadow: true)
            {
                myID = WalletRegionStartId + specialItemComponents.Count
            };

            // left/right neighbour IDs are omitted here, navigating to SkillRegionStartId confuses the snapping logic
            textureComponent.upNeighborID = index == 0 ?
                shouldNavButtonsBeShown ?
                    WalletUpArrowRegionId : -1 : textureComponent.myID - WalletIdIncrement;
            textureComponent.downNeighborID = index < specialItems.Count - 1 ?
                index == iconCount - 1 ?
                    shouldNavButtonsBeShown ?
                        WalletDownArrowRegionId : -1 : textureComponent.myID + WalletIdIncrement : -1;

            specialItemComponents[index] = textureComponent;
        }
        Instance.WriteField(skillPage, "specialItemComponents", specialItemComponents);

        // Wallet nav arrow navigation
        walletDownArrow.myID = WalletDownArrowRegionId;
        walletDownArrow.upNeighborID = specialItemComponents.Last().myID;
        Instance.WriteField(skillPage, "walletDownArrow", walletDownArrow);

        walletUpArrow.myID = WalletUpArrowRegionId;
        walletUpArrow.downNeighborID = specialItemComponents.First().myID;
        Instance.WriteField(skillPage, "walletUpArrow", walletUpArrow);

        Console.WriteLine("done rewrite Wallet UI From SpaceCore Mod");
    }
}
