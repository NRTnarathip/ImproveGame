using ImproveGame.SpaceCore.CustomForge;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace ImproveGame.SpaceCore;
public class NewForgeMenu : MenuWithInventory
{
    public enum CraftState
    {
        MissingIngredients,
        MissingShards,
        Valid,
        InvalidRecipe
    }

    protected int _timeUntilCraft;

    protected int _clankEffectTimer;

    protected int _sparklingTimer;

    public const int region_leftIngredient = 998;

    public const int region_rightIngredient = 997;

    public const int region_startButton = 996;

    public const int region_resultItem = 995;

    public const int region_unforgeButton = 994;

    public ClickableTextureComponent craftResultDisplay;

    public ClickableTextureComponent leftIngredientSpot;

    public ClickableTextureComponent rightIngredientSpot;

    public ClickableTextureComponent startTailoringButton;

    public ClickableComponent unforgeButton;

    private Rectangle expandedLeftIngredientSpot;

    private Rectangle expandedRightIngredientSpot;

    private Rectangle expandedStartForgingButton;

    public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

    public const int region_ring_1 = 110;

    public const int region_ring_2 = 111;

    public const int CRAFT_TIME = 1600;

    public Texture2D forgeTextures;

    protected Dictionary<Item, bool> _highlightDictionary;

    protected Dictionary<string, Item> _lastValidEquippedItems;

    protected List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();

    private bool unforging;

    protected string displayedDescription = "";

    protected CraftState _craftState;

    public Vector2 questionMarkOffset;

    private Rectangle bottomInv;

    private int forgePosX;

    private int forgePosY;

    private object justCrafted = null;

    public NewForgeMenu()
        : base(null, okButton: true, trashCan: true, Game1.xEdge)
    {
        Game1.playSound("bigSelect");
        xPositionOnScreen = Game1.xEdge;
        yPositionOnScreen = 0;
        width = Game1.uiViewport.Width - Game1.xEdge * 2;
        height = Game1.uiViewport.Height;
        float num = (float)width / 1280f;
        float num2 = (float)height / 720f;
        forgePosX = Game1.uiViewport.Width / 2 - 497;
        forgePosY = yPositionOnScreen;
        int num3 = 320;
        if (height < 720)
        {
            num3 = (int)((float)num3 * ((float)height / 720f));
        }
        int num4 = Math.Min(height - num3, height / 2);
        if (num4 < height - num3)
        {
            forgePosY = (Game1.uiViewport.Height - num3 - num4) / 2;
        }
        inventory.movePosition(0, -inventory.yPositionOnScreen);
        inventory.movePosition(0, forgePosY + num3);
        int num5 = 16;
        bottomInv = new Rectangle(xPositionOnScreen - num5, forgePosY + num3 - 64, width + num5 * 2, inventory.height + 96);
        inventory.highlightMethod = HighlightItems;
        forgeTextures = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\ForgeMenu");
        _CreateButtons();
        if (trashCan != null)
        {
            trashCan.myID = 106;
        }
        if (okButton != null)
        {
            okButton.leftNeighborID = 11;
        }
        if (Game1.options.SnappyMenus)
        {
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }
        _ValidateCraft();
    }

    protected void _CreateButtons()
    {
        int num = -100;
        leftIngredientSpot = new ClickableTextureComponent(new Rectangle(forgePosX + 204, forgePosY + num + 212, 64, 64), forgeTextures, new Rectangle(142, 0, 16, 16), 4f)
        {
            myID = 998,
            downNeighborID = -99998,
            leftNeighborID = 110,
            rightNeighborID = 997,
            item = ((leftIngredientSpot != null) ? leftIngredientSpot.item : null),
            fullyImmutable = true
        };
        expandedLeftIngredientSpot = new Rectangle(forgePosX + 204 - 80, forgePosY + num + 212 - 64, 144, 128);
        rightIngredientSpot = new ClickableTextureComponent(new Rectangle(forgePosX + 348, forgePosY + num + 212, 64, 64), forgeTextures, new Rectangle(142, 0, 16, 16), 4f)
        {
            myID = 997,
            downNeighborID = 996,
            leftNeighborID = 998,
            rightNeighborID = 994,
            item = ((rightIngredientSpot != null) ? rightIngredientSpot.item : null),
            fullyImmutable = true
        };
        expandedRightIngredientSpot = new Rectangle(forgePosX + 348, forgePosY + num + 212 - 64, 144, 128);
        startTailoringButton = new ClickableTextureComponent(new Rectangle(forgePosX + 204, forgePosY + num + 308, 52, 56), forgeTextures, new Rectangle(0, 80, 13, 14), 4f)
        {
            myID = 996,
            downNeighborID = -99998,
            leftNeighborID = 111,
            rightNeighborID = 994,
            upNeighborID = 998,
            item = ((startTailoringButton != null) ? startTailoringButton.item : null),
            fullyImmutable = true
        };
        expandedStartForgingButton = new Rectangle(forgePosX + 204 - 40, forgePosY + num + 308 - 8, 132, 84);
        unforgeButton = new ClickableComponent(new Rectangle(forgePosX + 484, forgePosY + num + 312, 40, 44), "Unforge")
        {
            myID = 994,
            downNeighborID = -99998,
            leftNeighborID = 996,
            rightNeighborID = 995,
            upNeighborID = 997,
            fullyImmutable = true
        };
        if (inventory.inventory != null && inventory.inventory.Count >= 12)
        {
            for (int i = 0; i < 12; i++)
            {
                if (inventory.inventory[i] != null)
                {
                    inventory.inventory[i].upNeighborID = -99998;
                }
            }
        }
        craftResultDisplay = new ClickableTextureComponent(new Rectangle(forgePosX + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 660, forgePosY + num + IClickableMenu.spaceToClearTopBorder + 8 + 232, 64, 64), forgeTextures, new Rectangle(0, 208, 16, 16), 4f)
        {
            myID = 995,
            downNeighborID = -99998,
            leftNeighborID = 996,
            upNeighborID = 997,
            item = ((craftResultDisplay != null) ? craftResultDisplay.item : null)
        };
        equipmentIcons = new List<ClickableComponent>();
        for (int j = 0; j < equipmentIcons.Count; j++)
        {
            equipmentIcons[j].bounds.X = forgePosX - 64 + 9;
            equipmentIcons[j].bounds.Y = forgePosY + 192 + j * 64;
        }
    }

    public override void snapToDefaultClickableComponent()
    {
        currentlySnappedComponent = getComponentWithID(0);
        snapCursorToCurrentSnappedComponent();
    }

    public bool IsBusy()
    {
        if (_timeUntilCraft <= 0)
        {
            return _sparklingTimer > 0;
        }
        return true;
    }

    public override bool readyToClose()
    {
        if (base.readyToClose() && heldItem == null)
        {
            return !IsBusy();
        }
        return false;
    }

    public bool HighlightItems(Item i)
    {
        if (i == null)
        {
            return false;
        }
        if (i != null && !IsValidCraftIngredient(i))
        {
            return false;
        }
        if (_highlightDictionary == null)
        {
            GenerateHighlightDictionary();
        }
        if (!_highlightDictionary.ContainsKey(i))
        {
            _highlightDictionary = null;
            GenerateHighlightDictionary();
        }
        return _highlightDictionary[i];
    }

    public void GenerateHighlightDictionary()
    {
        _highlightDictionary = new Dictionary<Item, bool>();
        List<Item> list = new List<Item>(inventory.actualInventory);
        if (Game1.player.leftRing.Value != null)
        {
            list.Add(Game1.player.leftRing.Value);
        }
        if (Game1.player.rightRing.Value != null)
        {
            list.Add(Game1.player.rightRing.Value);
        }
        foreach (Item item in list)
        {
            if (item == null)
            {
                continue;
            }
            if (Utility.IsNormalObjectAtParentSheetIndex(item, 848))
            {
                _highlightDictionary[item] = true;
            }
            else if (leftIngredientSpot.item == null && rightIngredientSpot.item == null)
            {
                bool value = false;
                if (item is Ring)
                {
                    value = true;
                }
                if (item is Tool && BaseEnchantment.GetAvailableEnchantmentsForItem(item as Tool).Count > 0)
                {
                    value = true;
                }
                if (BaseEnchantment.GetEnchantmentFromItem(null, item) != null)
                {
                    value = true;
                }
                _highlightDictionary[item] = value;
            }
            else if (leftIngredientSpot.item != null && rightIngredientSpot.item != null)
            {
                _highlightDictionary[item] = false;
            }
            else if (leftIngredientSpot.item != null)
            {
                _highlightDictionary[item] = IsValidCraft(leftIngredientSpot.item, item);
            }
            else
            {
                _highlightDictionary[item] = IsValidCraft(item, rightIngredientSpot.item);
            }
        }
    }

    private void _leftIngredientSpotClicked()
    {
        Item item = leftIngredientSpot.item;
        if (heldItem != null && !IsValidCraftIngredient(heldItem))
        {
            return;
        }
        if (heldItem != null && !(heldItem is Tool) && !(heldItem is Ring))
        {
            heldItem = null;
            inventory.currentlySelectedItem = -1;
            inventory.GamePadHideInfoPanel();
            return;
        }
        Game1.playSound("stoneStep");
        if (heldItem != null)
        {
            int num = ((inventory.dragItem != -1) ? inventory.dragItem : inventory.currentlySelectedItem);
            if (num != -1)
            {
                Utility.removeItemFromInventory(num, inventory.actualInventory);
            }
            inventory.currentlySelectedItem = -1;
            inventory.dragItem = -1;
        }
        leftIngredientSpot.item = heldItem;
        heldItem = item;
        if (item != null)
        {
            Utility.CollectOrDrop(item);
            heldItem = null;
            inventory.currentlySelectedItem = -1;
            inventory.GamePadHideInfoPanel();
        }
        _highlightDictionary = null;
        _ValidateCraft();
    }

    public bool IsValidCraftIngredient(Item item)
    {
        if (!item.canBeTrashed() && (!(item is Tool) || BaseEnchantment.GetAvailableEnchantmentsForItem(item as Tool).Count <= 0))
        {
            return false;
        }
        return true;
    }

    private void _rightIngredientSpotClicked()
    {
        Item item = rightIngredientSpot.item;
        if (heldItem != null && !IsValidCraftIngredient(heldItem))
        {
            return;
        }
        if (heldItem is Tool || (heldItem != null && (int)heldItem.parentSheetIndex == 848))
        {
            heldItem = null;
            inventory.currentlySelectedItem = -1;
            inventory.GamePadHideInfoPanel();
            return;
        }
        Game1.playSound("stoneStep");
        if (heldItem != null)
        {
            int num = ((inventory.dragItem != -1) ? inventory.dragItem : inventory.currentlySelectedItem);
            if (num != -1)
            {
                Utility.removeItemFromInventory(num, inventory.actualInventory);
            }
            inventory.currentlySelectedItem = -1;
            inventory.dragItem = -1;
        }
        rightIngredientSpot.item = heldItem;
        heldItem = item;
        if (item != null)
        {
            Utility.CollectOrDrop(item);
            heldItem = null;
            inventory.currentlySelectedItem = -1;
            inventory.GamePadHideInfoPanel();
        }
        _highlightDictionary = null;
        _ValidateCraft();
    }

    public override void receiveKeyPress(Keys key)
    {
        if (key == Keys.Delete)
        {
            if (heldItem != null && IsValidCraftIngredient(heldItem))
            {
                Utility.trashItem(heldItem);
                heldItem = null;
            }
        }
        else
        {
            base.receiveKeyPress(key);
        }
    }

    public bool IsHoldingEquippedItem()
    {
        if (heldItem == null)
        {
            return false;
        }
        if (!Game1.player.IsEquippedItem(heldItem))
        {
            return Game1.player.IsEquippedItem(Utility.PerformSpecialItemGrabReplacement(heldItem));
        }
        return true;
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        Item item = heldItem;
        bool flag = Game1.player.IsEquippedItem(item);
        base.receiveLeftClick(x, y, playSound: true);
        foreach (ClickableComponent equipmentIcon in equipmentIcons)
        {
            if (!equipmentIcon.containsPoint(x, y))
            {
                continue;
            }
            string name = equipmentIcon.name;
            if (!(name == "Ring1"))
            {
                if (!(name == "Ring2") || (!HighlightItems(Game1.player.rightRing.Value) && Game1.player.rightRing.Value != null))
                {
                    return;
                }
                Item item2 = heldItem;
                Item value = Game1.player.rightRing.Value;
                if (value != heldItem && (item2 == null || item2 is Ring))
                {
                    if (Game1.player.rightRing.Value != null)
                    {
                        Game1.player.rightRing.Value.onUnequip(Game1.player, Game1.currentLocation);
                    }
                    Game1.player.rightRing.Value = item2 as Ring;
                    heldItem = value;
                    if (Game1.player.rightRing.Value != null)
                    {
                        Game1.player.rightRing.Value.onEquip(Game1.player, Game1.currentLocation);
                        Game1.playSound("crit");
                    }
                    else if (heldItem != null)
                    {
                        Game1.playSound("dwop");
                    }
                    _highlightDictionary = null;
                    _ValidateCraft();
                }
            }
            else
            {
                if (!HighlightItems(Game1.player.leftRing.Value) && Game1.player.leftRing.Value != null)
                {
                    return;
                }
                Item item3 = heldItem;
                Item value2 = Game1.player.leftRing.Value;
                if (value2 != heldItem && (item3 == null || item3 is Ring))
                {
                    if (Game1.player.leftRing.Value != null)
                    {
                        Game1.player.leftRing.Value.onUnequip(Game1.player, Game1.currentLocation);
                    }
                    Game1.player.leftRing.Value = item3 as Ring;
                    heldItem = value2;
                    if (Game1.player.leftRing.Value != null)
                    {
                        Game1.player.leftRing.Value.onEquip(Game1.player, Game1.currentLocation);
                        Game1.playSound("crit");
                    }
                    else if (heldItem != null)
                    {
                        Game1.playSound("dwop");
                    }
                    _highlightDictionary = null;
                    _ValidateCraft();
                }
            }
            return;
        }
        if (item != heldItem && heldItem != null)
        {
            if (heldItem is Tool || (heldItem is Ring && leftIngredientSpot.item == null))
            {
                _leftIngredientSpotClicked();
            }
            else
            {
                _rightIngredientSpotClicked();
            }
        }
        if (IsBusy())
        {
            return;
        }
        if (leftIngredientSpot.containsPoint(x, y))
        {
            _leftIngredientSpotClicked();
            if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && heldItem != null)
            {
                if (Game1.player.IsEquippedItem(heldItem))
                {
                    heldItem = null;
                }
                else
                {
                    heldItem = inventory.tryToAddItem(heldItem, "");
                }
            }
        }
        else if (rightIngredientSpot.containsPoint(x, y))
        {
            _rightIngredientSpotClicked();
            if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && heldItem != null)
            {
                if (Game1.player.IsEquippedItem(heldItem))
                {
                    heldItem = null;
                }
                else
                {
                    heldItem = inventory.tryToAddItem(heldItem, "");
                }
            }
        }
        else if (expandedStartForgingButton.Contains(x, y))
        {
            if (heldItem == null)
            {
                bool flag2 = false;
                if (!CanFitCraftedItem())
                {
                    Game1.playSound("cancel");
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                    _timeUntilCraft = 0;
                    flag2 = true;
                }
                if (!flag2 && IsValidCraft(leftIngredientSpot.item, rightIngredientSpot.item) && Game1.player.hasItemInInventory(848, GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item)))
                {
                    Game1.playSound("bigSelect");
                    startTailoringButton.scale = startTailoringButton.baseScale;
                    _timeUntilCraft = 1600;
                    _clankEffectTimer = 300;
                    _UpdateDescriptionText();
                    int num = forgePosX;
                    int num2 = forgePosY - 100;
                    int forgeCost = GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item);
                    for (int i = 0; i < forgeCost; i++)
                    {
                        tempSprites.Add(new TemporaryAnimatedSprite("", new Rectangle(143, 17, 14, 15), new Vector2(num + 276, num2 + 300), flipped: false, 0.1f, Color.White)
                        {
                            texture = forgeTextures,
                            motion = new Vector2(-4f, -4f),
                            scale = 4f,
                            layerDepth = 1f,
                            startSound = "boulderCrack",
                            delayBeforeAnimationStart = 1400 / forgeCost * i
                        });
                    }
                    if (rightIngredientSpot.item != null && (int)rightIngredientSpot.item.parentSheetIndex == 74)
                    {
                        _sparklingTimer = 900;
                        Rectangle bounds = leftIngredientSpot.bounds;
                        bounds.Offset(-32, -32);
                        List<TemporaryAnimatedSprite> list = Utility.sparkleWithinArea(bounds, 6, Color.White, 80, 1600);
                        list.First().startSound = "discoverMineral";
                        tempSprites.AddRange(list);
                        bounds = rightIngredientSpot.bounds;
                        bounds.Inflate(-16, -16);
                        Vector2 randomPositionInThisRectangle = Utility.getRandomPositionInThisRectangle(bounds, Game1.random);
                        int num3 = 30;
                        for (int j = 0; j < num3; j++)
                        {
                            randomPositionInThisRectangle = Utility.getRandomPositionInThisRectangle(bounds, Game1.random);
                            tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 48, 2, 2), randomPositionInThisRectangle, flipped: false, 0f, Color.White)
                            {
                                motion = new Vector2(-4f, 0f),
                                yPeriodic = true,
                                yPeriodicRange = 16f,
                                yPeriodicLoopTime = 1200f,
                                scale = 4f,
                                layerDepth = 1f,
                                animationLength = 12,
                                interval = Game1.random.Next(20, 40),
                                totalNumberOfLoops = 1,
                                delayBeforeAnimationStart = _clankEffectTimer / num3 * j
                            });
                        }
                    }
                }
                else
                {
                    Game1.playSound("sell");
                }
            }
            else
            {
                Game1.playSound("sell");
            }
        }
        else if (unforgeButton.containsPoint(x, y))
        {
            if (rightIngredientSpot.item == null)
            {
                if (IsValidUnforge())
                {
                    if (leftIngredientSpot.item is MeleeWeapon && !Game1.player.couldInventoryAcceptThisObject(848, (leftIngredientSpot.item as MeleeWeapon).GetTotalForgeLevels() * 5 + ((leftIngredientSpot.item as MeleeWeapon).GetTotalForgeLevels() - 1) * 2))
                    {
                        displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_noroom");
                        Game1.playSound("cancel");
                    }
                    else if (leftIngredientSpot.item is CombinedRing && Game1.player.freeSpotsInInventory() < 2)
                    {
                        displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_noroom");
                        Game1.playSound("cancel");
                    }
                    else
                    {
                        unforging = true;
                        _timeUntilCraft = 1600;
                        int num4 = GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item) / 2;
                        for (int k = 0; k < num4; k++)
                        {
                            Vector2 motion = new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-4, 5));
                            if (motion.X == 0f && motion.Y == 0f)
                            {
                                motion = new Vector2(-4f, -4f);
                            }
                            tempSprites.Add(new TemporaryAnimatedSprite("", new Rectangle(143, 17, 14, 15), new Vector2(leftIngredientSpot.bounds.X, leftIngredientSpot.bounds.Y), flipped: false, 0.1f, Color.White)
                            {
                                alpha = 0.01f,
                                alphaFade = -0.1f,
                                alphaFadeFade = -0.005f,
                                texture = forgeTextures,
                                motion = motion,
                                scale = 4f,
                                layerDepth = 1f,
                                startSound = "boulderCrack",
                                delayBeforeAnimationStart = 1100 / num4 * k
                            });
                        }
                        Game1.playSound("debuffHit");
                    }
                }
                else
                {
                    displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_invalid");
                    Game1.playSound("cancel");
                }
            }
            else
            {
                if (IsValidUnforge(ignore_right_slot_occupancy: true))
                {
                    displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_right_slot");
                }
                else
                {
                    displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_invalid");
                }
                Game1.playSound("cancel");
            }
        }
        if (heldItem == null || isWithinBounds(x, y) || !heldItem.canBeTrashed())
        {
            return;
        }
        if (Game1.player.IsEquippedItem(heldItem))
        {
            if (heldItem == Game1.player.hat.Value)
            {
                Game1.player.hat.Value = null;
            }
            else if (heldItem == Game1.player.shirtItem.Value)
            {
                Game1.player.shirtItem.Value = null;
            }
            else if (heldItem == Game1.player.pantsItem.Value)
            {
                Game1.player.pantsItem.Value = null;
            }
        }
        Game1.playSound("throwDownITem");
        Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
        heldItem = null;
    }

    protected virtual bool CheckHeldItem(Func<Item, bool> f = null)
    {
        return f?.Invoke(heldItem) ?? (heldItem != null);
    }

    public virtual int GetForgeCostAtLevel(int level)
    {
        return 10 + level * 5;
    }

    public virtual int GetForgeCost(Item left_item, Item right_item)
    {
        if (right_item != null && (int)right_item.parentSheetIndex == 896)
        {
            return 20;
        }
        if (right_item != null && (int)right_item.parentSheetIndex == 74)
        {
            return 20;
        }
        if (right_item != null && (int)right_item.parentSheetIndex == 72)
        {
            return 10;
        }
        if (left_item is MeleeWeapon && right_item is MeleeWeapon)
        {
            return 10;
        }
        if (left_item != null && left_item is Tool)
        {
            return GetForgeCostAtLevel((left_item as Tool).GetTotalForgeLevels());
        }
        if (left_item != null && left_item is Ring && right_item != null && right_item is Ring)
        {
            return 20;
        }

        //<MINE>
        var recipes = CustomForgeAPI.GetRecipes() as System.Collections.IList;
        foreach (var recipe in recipes)
        {
            if (CustomForgeAPI.Recipe_BaseItem_HasEnoughFor(recipe, left_item)
                && CustomForgeAPI.Recipe_IngredientItem_HasEnoughFor(recipe, right_item))
                return CustomForgeAPI.Get_CinderShardCost(recipe);
        }
        //</MINE>

        return 1;
    }

    protected void _ValidateCraft()
    {
        Item item = leftIngredientSpot.item;
        Item item2 = rightIngredientSpot.item;
        if (item == null || item2 == null)
        {
            _craftState = CraftState.MissingIngredients;
        }
        else if (IsValidCraft(item, item2))
        {
            _craftState = CraftState.Valid;
            Item one = item.getOne();
            if (item2 != null && Utility.IsNormalObjectAtParentSheetIndex(item2, 72))
            {
                (one as Tool).AddEnchantment(new DiamondEnchantment());
                craftResultDisplay.item = one;
            }
            else
            {
                craftResultDisplay.item = CraftItem(one, item2.getOne());
            }
        }
        else
        {
            _craftState = CraftState.InvalidRecipe;
        }
        _UpdateDescriptionText();
    }

    protected void _UpdateDescriptionText()
    {
        if (IsBusy())
        {
            if (rightIngredientSpot.item != null && (int)rightIngredientSpot.item.parentSheetIndex == 74)
            {
                displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_enchanting");
            }
            else
            {
                displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_forging");
            }
        }
        else if (_craftState == CraftState.MissingIngredients)
        {
            displayedDescription = (displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_description1") + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Forge_description2"));
        }
        else if (_craftState == CraftState.MissingShards)
        {
            if (heldItem != null && heldItem.ParentSheetIndex == 848)
            {
                displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_shards");
            }
            else
            {
                displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_notenoughshards");
            }
        }
        else if (_craftState == CraftState.Valid)
        {
            if (!CanFitCraftedItem())
            {
                displayedDescription = Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588");
            }
            else
            {
                displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_valid");
            }
        }
        else if (_craftState == CraftState.InvalidRecipe)
        {
            displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_wrongorder");
        }
        else
        {
            displayedDescription = "";
        }
    }

    public bool IsValidCraft(Item left_item, Item right_item)
    {
        if (left_item == null || right_item == null)
        {
            return false;
        }
        if (left_item is Tool && (left_item as Tool).CanForge(right_item))
        {
            return true;
        }
        if (left_item is Ring && right_item is Ring && (left_item as Ring).CanCombine(right_item as Ring))
        {
            return true;
        }
        return false;
    }

    public Item CraftItem(Item left_item, Item right_item, bool forReal = false)
    {
        if (left_item == null || right_item == null)
        {
            return null;
        }
        if (left_item is Tool && !(left_item as Tool).Forge(right_item, forReal))
        {
            return null;
        }
        if (left_item is Ring && right_item is Ring)
        {
            left_item = (left_item as Ring).Combine(right_item as Ring);
        }
        return left_item;
    }

    public void SpendRightItem()
    {
        if (rightIngredientSpot.item != null)
        {
            rightIngredientSpot.item.Stack--;
            if (rightIngredientSpot.item.Stack <= 0 || rightIngredientSpot.item.maximumStackSize() == 1)
            {
                rightIngredientSpot.item = null;
            }
        }
    }

    public void SpendLeftItem()
    {
        if (leftIngredientSpot.item != null)
        {
            leftIngredientSpot.item.Stack--;
            if (leftIngredientSpot.item.Stack <= 0 || leftIngredientSpot.item.maximumStackSize() == 1)
            {
                leftIngredientSpot.item = null;
            }
        }
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        if (!IsBusy())
        {
            base.receiveRightClick(x, y, playSound: true);
        }
    }

    public override void performHoverAction(int x, int y)
    {
        if (IsBusy())
        {
            return;
        }
        hoveredItem = null;
        base.performHoverAction(x, y);
        hoverText = "";
        for (int i = 0; i < equipmentIcons.Count; i++)
        {
            if (equipmentIcons[i].containsPoint(x, y))
            {
                if (equipmentIcons[i].name == "Ring1")
                {
                    hoveredItem = Game1.player.leftRing.Value;
                }
                else if (equipmentIcons[i].name == "Ring2")
                {
                    hoveredItem = Game1.player.rightRing.Value;
                }
            }
        }
        if (craftResultDisplay.visible && craftResultDisplay.containsPoint(x, y) && craftResultDisplay.item != null)
        {
            hoveredItem = craftResultDisplay.item;
        }
        if (leftIngredientSpot.containsPoint(x, y) && leftIngredientSpot.item != null)
        {
            hoveredItem = leftIngredientSpot.item;
        }
        if (rightIngredientSpot.containsPoint(x, y) && rightIngredientSpot.item != null)
        {
            hoveredItem = rightIngredientSpot.item;
        }
        if (unforgeButton.containsPoint(x, y))
        {
            hoverText = Game1.content.LoadString("Strings\\UI:Forge_Unforge");
        }
        if (_craftState == CraftState.Valid && CanFitCraftedItem())
        {
            startTailoringButton.tryHover(x, y, 0.33f);
        }
        else
        {
            startTailoringButton.tryHover(-999, -999);
        }
    }

    public bool CanFitCraftedItem()
    {
        if (craftResultDisplay.item != null && !Utility.canItemBeAddedToThisInventoryList(craftResultDisplay.item, inventory.actualInventory))
        {
            return false;
        }
        return true;
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        base.gameWindowSizeChanged(oldBounds, newBounds);
        int yPosition = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
        inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPosition, playerInventory: false, null, inventory.highlightMethod);
        _CreateButtons();
    }

    public override void emergencyShutDown()
    {
        _OnCloseMenu();
        base.emergencyShutDown();
    }

    public override void update(GameTime time)
    {
        base.update(time);
        for (int num = tempSprites.Count - 1; num >= 0; num--)
        {
            if (tempSprites[num].update(time))
            {
                tempSprites.RemoveAt(num);
            }
        }
        if (leftIngredientSpot.item != null && rightIngredientSpot.item != null && !Game1.player.hasItemInInventory(848, GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item)))
        {
            if (_craftState != CraftState.MissingShards)
            {
                _craftState = CraftState.MissingShards;
                craftResultDisplay.item = null;
                _UpdateDescriptionText();
            }
        }
        else if (_craftState == CraftState.MissingShards)
        {
            _ValidateCraft();
        }
        descriptionText = displayedDescription;
        questionMarkOffset.X = (float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.5) * 4f;
        questionMarkOffset.Y = (float)Math.Cos(time.TotalGameTime.TotalSeconds * 5.0) * -4f;
        bool flag = CanFitCraftedItem();
        if (_craftState == CraftState.Valid && !IsBusy() && flag)
        {
            craftResultDisplay.visible = true;
        }
        else
        {
            craftResultDisplay.visible = false;
        }
        if (_timeUntilCraft <= 0 && _sparklingTimer <= 0)
        {
            return;
        }
        startTailoringButton.tryHover(startTailoringButton.bounds.Center.X, startTailoringButton.bounds.Center.Y, 0.33f);
        _timeUntilCraft -= (int)time.ElapsedGameTime.TotalMilliseconds;
        _clankEffectTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
        if (_timeUntilCraft <= 0 && _sparklingTimer > 0)
        {
            _sparklingTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
        }
        else if (_clankEffectTimer <= 0 && !unforging)
        {
            _clankEffectTimer = 450;
            if (rightIngredientSpot.item != null && (int)rightIngredientSpot.item.parentSheetIndex == 74)
            {
                Rectangle bounds = rightIngredientSpot.bounds;
                bounds.Inflate(-16, -16);
                Vector2 randomPositionInThisRectangle = Utility.getRandomPositionInThisRectangle(bounds, Game1.random);
                int num2 = 30;
                for (int i = 0; i < num2; i++)
                {
                    randomPositionInThisRectangle = Utility.getRandomPositionInThisRectangle(bounds, Game1.random);
                    tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 48, 2, 2), randomPositionInThisRectangle, flipped: false, 0f, Color.White)
                    {
                        motion = new Vector2(-4f, 0f),
                        yPeriodic = true,
                        yPeriodicRange = 16f,
                        yPeriodicLoopTime = 1200f,
                        scale = 4f,
                        layerDepth = 1f,
                        animationLength = 12,
                        interval = Game1.random.Next(20, 40),
                        totalNumberOfLoops = 1,
                        delayBeforeAnimationStart = _clankEffectTimer / num2 * i
                    });
                }
            }
            else
            {
                Game1.playSound("crafting");
                Game1.playSound("clank");
                Rectangle bounds2 = leftIngredientSpot.bounds;
                bounds2.Inflate(-21, -21);
                Vector2 randomPositionInThisRectangle2 = Utility.getRandomPositionInThisRectangle(bounds2, Game1.random);
                tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), randomPositionInThisRectangle2, flipped: false, 0.015f, Color.White)
                {
                    motion = new Vector2(-1f, -10f),
                    acceleration = new Vector2(0f, 0.6f),
                    scale = 4f,
                    layerDepth = 1f,
                    animationLength = 12,
                    interval = 30f,
                    totalNumberOfLoops = 1
                });
                tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), randomPositionInThisRectangle2, flipped: false, 0.015f, Color.White)
                {
                    motion = new Vector2(0f, -8f),
                    acceleration = new Vector2(0f, 0.48f),
                    scale = 4f,
                    layerDepth = 1f,
                    animationLength = 12,
                    interval = 30f,
                    totalNumberOfLoops = 1
                });
                tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), randomPositionInThisRectangle2, flipped: false, 0.015f, Color.White)
                {
                    motion = new Vector2(1f, -10f),
                    acceleration = new Vector2(0f, 0.6f),
                    scale = 4f,
                    layerDepth = 1f,
                    animationLength = 12,
                    interval = 30f,
                    totalNumberOfLoops = 1
                });
                tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), randomPositionInThisRectangle2, flipped: false, 0.015f, Color.White)
                {
                    motion = new Vector2(-2f, -8f),
                    acceleration = new Vector2(0f, 0.6f),
                    scale = 2f,
                    layerDepth = 1f,
                    animationLength = 12,
                    interval = 30f,
                    totalNumberOfLoops = 1
                });
                tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), randomPositionInThisRectangle2, flipped: false, 0.015f, Color.White)
                {
                    motion = new Vector2(2f, -8f),
                    acceleration = new Vector2(0f, 0.6f),
                    scale = 2f,
                    layerDepth = 1f,
                    animationLength = 12,
                    interval = 30f,
                    totalNumberOfLoops = 1
                });
            }
        }
        if (_timeUntilCraft > 0 || _sparklingTimer > 0)
        {
            return;
        }
        if (unforging)
        {
            if (leftIngredientSpot.item is MeleeWeapon)
            {
                MeleeWeapon meleeWeapon = leftIngredientSpot.item as MeleeWeapon;
                int num3 = 0;
                if (meleeWeapon != null)
                {
                    int totalForgeLevels = meleeWeapon.GetTotalForgeLevels(for_unforge: true);
                    for (int j = 0; j < totalForgeLevels; j++)
                    {
                        num3 += GetForgeCostAtLevel(j);
                    }
                    if (meleeWeapon.hasEnchantmentOfType<DiamondEnchantment>())
                    {
                        num3 += GetForgeCost(leftIngredientSpot.item, new Object(72, 1));
                    }
                    for (int num4 = meleeWeapon.enchantments.Count - 1; num4 >= 0; num4--)
                    {
                        if (meleeWeapon.enchantments[num4].IsForge())
                        {
                            meleeWeapon.RemoveEnchantment(meleeWeapon.enchantments[num4]);
                        }
                    }
                    if (meleeWeapon.appearance.Value >= 0)
                    {
                        meleeWeapon.appearance.Value = -1;
                        meleeWeapon.IndexOfMenuItemView = meleeWeapon.getDrawnItemIndex();
                        num3 += 10;
                    }
                    Game1.playSound("coin");
                }
                Utility.CollectOrDrop(new Object(848, num3 / 2));
            }
            else if (leftIngredientSpot.item is CombinedRing)
            {
                if (leftIngredientSpot.item is CombinedRing combinedRing)
                {
                    List<Ring> list = new List<Ring>(combinedRing.combinedRings);
                    combinedRing.combinedRings.Clear();
                    foreach (Ring item2 in list)
                    {
                        Utility.CollectOrDrop(item2);
                    }
                    leftIngredientSpot.item = null;
                    Game1.playSound("coin");
                }
                Utility.CollectOrDrop(new Object(848, 10));
            }
            unforging = false;
            _timeUntilCraft = 0;
            _ValidateCraft();
            return;
        }
        Game1.player.removeItemsFromInventory(848, GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item));
        Item item = CraftItem(leftIngredientSpot.item, rightIngredientSpot.item, forReal: true);
        if (item != null && !Utility.canItemBeAddedToThisInventoryList(item, inventory.actualInventory))
        {
            Game1.playSound("cancel");
            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
            _timeUntilCraft = 0;
            return;
        }
        if (leftIngredientSpot.item == item)
        {
            leftIngredientSpot.item = null;
        }
        else
        {
            SpendLeftItem();
        }
        SpendRightItem();
        Game1.playSound("coin");
        leftIngredientSpot.item = item;
        _timeUntilCraft = 0;
        _ValidateCraft();
    }

    public virtual bool IsValidUnforge(bool ignore_right_slot_occupancy = false)
    {
        if (!ignore_right_slot_occupancy && rightIngredientSpot.item != null)
        {
            return false;
        }
        if (leftIngredientSpot.item != null && leftIngredientSpot.item is MeleeWeapon && ((leftIngredientSpot.item as MeleeWeapon).GetTotalForgeLevels() > 0 || (leftIngredientSpot.item as MeleeWeapon).appearance.Value >= 0))
        {
            return true;
        }
        if (leftIngredientSpot.item != null && leftIngredientSpot.item is CombinedRing)
        {
            return true;
        }
        return false;
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
        Color value = new Color(116, 11, 3);
        Game1.drawDialogueBox(bottomInv.X, bottomInv.Y, bottomInv.Width, bottomInv.Height, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: true, value.R, value.G, value.B);
        base.draw(b, drawUpperPortion: true, drawDescriptionArea: true, 116, 11, 3);
        int newDrawForgePosX = forgePosX + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4;
        int newDrawForgePosY = forgePosY;
        Game1.DrawBox(newDrawForgePosX, newDrawForgePosY, 568, 320, value);
        b.Draw(forgeTextures, new Vector2(newDrawForgePosX, newDrawForgePosY), new Rectangle(0, 0, 142, 80), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        newDrawForgePosX += 552;
        Game1.DrawBox(newDrawForgePosX, newDrawForgePosY, 426, 320, value);
        drawDescriptionArea(b, newDrawForgePosX, newDrawForgePosY, value.R, value.G, value.B);
        Color color = Color.White;
        if (_craftState == CraftState.MissingShards)
        {
            color = Color.Gray * 0.75f;
        }
        int num3 = -100;
        b.Draw(forgeTextures, new Vector2(forgePosX + 276, forgePosY + num3 + 300), new Rectangle(142, 16, 17, 17), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
        if (leftIngredientSpot.item != null && rightIngredientSpot.item != null && IsValidCraft(leftIngredientSpot.item, rightIngredientSpot.item))
        {
            //<Original>
            //int forgeCost = GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item);
            //int num4 = (forgeCost - 10) / 5;
            //if (num4 >= 0 && num4 <= 2)
            //{
            //    b.Draw(forgeTextures, new Vector2(forgePosX + 344, forgePosY + num3 + 320), new Rectangle(142, 38 + num4 * 10, 17, 10), Color.White * ((_craftState == CraftState.MissingShards) ? 0.5f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            //}
            //<MINE>
            int cost = this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item);
            if (cost is not (10 or 15 or 20))
            {
                Game1.spriteBatch.DrawString(Game1.dialogueFont, "x" + cost,
                    new Vector2(this.xPositionOnScreen + 345, this.yPositionOnScreen + 320),
                    new Color(226, 124, 65));
            }
            else
            {
                int source_offset = (cost - 10) / 5;
                if (source_offset is >= 0 and <= 2)
                {
                    b.Draw(this.forgeTextures,
                        new Vector2(this.xPositionOnScreen + 344, this.yPositionOnScreen + 320),
                        new Rectangle(142, 38 + source_offset * 10, 17, 10),
                        Color.White * ((this._craftState == CraftState.MissingShards) ? 0.5f : 1f), 0f,
                        Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
                }
            }
            //</MINE>
        }
        if (IsValidUnforge())
        {
            b.Draw(forgeTextures, new Vector2(unforgeButton.bounds.X, unforgeButton.bounds.Y), new Rectangle(143, 69, 11, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
        }
        if (_craftState == CraftState.Valid)
        {
            startTailoringButton.draw(b, Color.White, 0.96f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 200 % 12);
            startTailoringButton.drawItem(b, 16, 16);
        }
        Point point = new Point(0, 0);
        bool flag = false;
        bool flag2 = false;
        Item item = hoveredItem;
        if (heldItem != null)
        {
            item = heldItem;
        }
        if (item != null && item != leftIngredientSpot.item && item != rightIngredientSpot.item && item != craftResultDisplay.item)
        {
            if (item is Tool)
            {
                if (leftIngredientSpot.item is Tool)
                {
                    flag2 = true;
                }
                else
                {
                    flag = true;
                }
            }
            if (BaseEnchantment.GetEnchantmentFromItem(leftIngredientSpot.item, item) != null)
            {
                flag2 = true;
            }
            if (item is Ring && !(item is CombinedRing) && (leftIngredientSpot.item == null || leftIngredientSpot.item is Ring) && (rightIngredientSpot.item == null || rightIngredientSpot.item is Ring))
            {
                flag = true;
                flag2 = true;
            }
        }
        foreach (ClickableComponent equipmentIcon in equipmentIcons)
        {
            string name = equipmentIcon.name;
            if (!(name == "Ring1"))
            {
                if (!(name == "Ring2"))
                {
                    continue;
                }
                if (Game1.player.rightRing.Value != null)
                {
                    b.Draw(forgeTextures, equipmentIcon.bounds, new Rectangle(0, 96, 16, 16), Color.White);
                    float transparency = 1f;
                    if (!HighlightItems((Ring)Game1.player.rightRing))
                    {
                        transparency = 0.5f;
                    }
                    if (Game1.player.rightRing.Value == heldItem)
                    {
                        transparency = 0.5f;
                    }
                    Game1.player.rightRing.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), equipmentIcon.scale, transparency, 0.866f, StackDrawType.Hide);
                }
                else
                {
                    b.Draw(forgeTextures, equipmentIcon.bounds, new Rectangle(16, 96, 16, 16), Color.White);
                }
            }
            else if (Game1.player.leftRing.Value != null)
            {
                b.Draw(forgeTextures, equipmentIcon.bounds, new Rectangle(0, 96, 16, 16), Color.White);
                float transparency2 = 1f;
                if (!HighlightItems((Ring)Game1.player.leftRing))
                {
                    transparency2 = 0.5f;
                }
                if (Game1.player.leftRing.Value == heldItem)
                {
                    transparency2 = 0.5f;
                }
                Game1.player.leftRing.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), equipmentIcon.scale, transparency2, 0.866f, StackDrawType.Hide);
            }
            else
            {
                b.Draw(forgeTextures, equipmentIcon.bounds, new Rectangle(16, 96, 16, 16), Color.White);
            }
        }
        if (!IsBusy())
        {
            if (flag)
            {
                leftIngredientSpot.draw(b, Color.White, 0.87f);
            }
        }
        else if (_clankEffectTimer > 300 || (_timeUntilCraft > 0 && unforging))
        {
            point.X = Game1.random.Next(-1, 2);
            point.Y = Game1.random.Next(-1, 2);
        }
        leftIngredientSpot.drawItem(b, point.X * 4, point.Y * 4);
        if (craftResultDisplay.visible)
        {
            string text = Game1.content.LoadString("Strings\\UI:Tailor_MakeResult");
            Utility.drawTextWithColoredShadow(position: new Vector2((float)craftResultDisplay.bounds.Center.X - Game1.smallFont.MeasureString(text).X / 2f, (float)craftResultDisplay.bounds.Top - Game1.smallFont.MeasureString(text).Y), b: b, text: text, font: Game1.smallFont, color: Game1.textColor * 0.75f, shadowColor: Color.Black * 0.2f);
            if (craftResultDisplay.item != null)
            {
                craftResultDisplay.drawItem(b);
            }
        }
        if (!IsBusy() && flag2)
        {
            rightIngredientSpot.draw(b, Color.White, 0.87f);
        }
        rightIngredientSpot.drawItem(b);
        foreach (TemporaryAnimatedSprite tempSprite in tempSprites)
        {
            tempSprite.draw(b, localPosition: true);
        }
        if (!hoverText.Equals(""))
        {
            IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, (heldItem != null) ? 32 : 0, (heldItem != null) ? 32 : 0);
        }
        else if (hoveredItem != null)
        {
            if (hoveredItem == craftResultDisplay.item && Utility.IsNormalObjectAtParentSheetIndex(rightIngredientSpot.item, 74))
            {
                BaseEnchantment.hideEnchantmentName = true;
            }
            IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, heldItem != null);
            BaseEnchantment.hideEnchantmentName = false;
        }
        if (heldItem != null && Game1.player.IsEquippedItem(heldItem))
        {
            Vector2 location = new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY() - 128);
            heldItem.drawInMenu(b, location, 2f, 0.5f, 0.0865f);
        }
        inventory.drawDragItem(b);
        inventory.drawInfoPanel(b, force: true);
        drawMouse(b);
    }

    protected override void cleanupBeforeExit()
    {
        _OnCloseMenu();
    }

    protected void _OnCloseMenu()
    {
        if (!Game1.player.IsEquippedItem(heldItem))
        {
            Utility.CollectOrDrop(heldItem, 2);
        }
        if (!Game1.player.IsEquippedItem(leftIngredientSpot.item))
        {
            Utility.CollectOrDrop(leftIngredientSpot.item, 2);
        }
        if (!Game1.player.IsEquippedItem(rightIngredientSpot.item))
        {
            Utility.CollectOrDrop(rightIngredientSpot.item, 2);
        }
        if (!Game1.player.IsEquippedItem(startTailoringButton.item))
        {
            Utility.CollectOrDrop(startTailoringButton.item, 2);
        }
        heldItem = null;
        leftIngredientSpot.item = null;
        rightIngredientSpot.item = null;
        startTailoringButton.item = null;
    }

    protected void drawDescriptionArea(SpriteBatch b, int x, int y, int red = -1, int green = -1, int blue = -1)
    {
        Color color = ((red == -1) ? Color.White : new Color(red, green, blue));
        Texture2D texture = ((red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture);
        int num = x - 32 - 8;
        int num2 = 264;
        int num3 = y - 32;
        b.Draw(texture, new Vector2(num, num3), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 44), color);
        b.Draw(texture, new Vector2(num, num3 + num2 + 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 39), color);
        if (!descriptionText.Equals(""))
        {
            int num4 = x + IClickableMenu.borderWidth + ((wiggleWordsTimer > 0) ? Game1.random.Next(-2, 3) : 0);
            int num5 = y + IClickableMenu.borderWidth + ((wiggleWordsTimer > 0) ? Game1.random.Next(-2, 3) : 0);
            int num6 = 320;
            float num7 = 0f;
            string text = "";
            do
            {
                num7 = ((num7 != 0f) ? (num7 - 0.1f) : 1f);
                text = Game1.parseText(descriptionText, Game1.smallFont, (int)((float)(426 - IClickableMenu.borderWidth * 2) / num7));
            }
            while (Game1.smallFont.MeasureString(text).Y > (float)num6 / num7 && num7 > 0.5f);
            if (Game1.options.bigFonts)
            {
                num5 -= 24;
            }
            if (red == -1)
            {
                Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2(num4, num5), Game1.textColor * 0.75f, num7);
            }
            else
            {
                Utility.drawTextWithColoredShadow(b, text, Game1.smallFont, new Vector2(num4, num5), Game1.textColor * 0.75f, Color.Black * 0.2f, num7);
            }
        }
    }

    public override void releaseLeftClick(int x, int y)
    {
        if (IsBusy())
        {
            base.releaseLeftClick(x, y);
            return;
        }
        if (expandedLeftIngredientSpot.Contains(x, y))
        {
            _leftIngredientSpotClicked();
            return;
        }
        if (expandedRightIngredientSpot.Contains(x, y))
        {
            _rightIngredientSpotClicked();
            return;
        }
        base.releaseLeftClick(x, y);
        heldItem = null;
        inventory.currentlySelectedItem = -1;
        inventory.GamePadHideInfoPanel();
    }
}
