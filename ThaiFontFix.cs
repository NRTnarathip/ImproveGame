using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System.Reflection;
using System.Text;
using static StardewValley.BellsAndWhistles.SpriteText;

namespace ImproveGame.ThaiFont;

public static class ThaiFontFix
{
    public static string FixText(string text)
    {
        //wtich cache == 0.002ms

        //without cache 0.002ms -> 0.004ms
        if (ThaiFontAdjuster.IsThaiString(text))
            return ThaiFontAdjuster.Adjust(text);
        return text;
    }

    //get method by name in current  type & check only static | private
    public static MethodInfo GetMethod(string newMethod)
    {
        var methods = typeof(ThaiFontFix).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        return methods.SingleOrDefault(m => m.Name == newMethod);
    }

    public static void Init()
    {
        var harmony = new Harmony(typeof(ThaiFontFix).FullName);
        var DrawStringName = nameof(SpriteBatch.DrawString);
        var spriteBatch = typeof(SpriteBatch);
        {
            var method = spriteBatch.GetMethod(DrawStringName,
                [typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color)]);
            harmony.Patch(method,
                new(GetMethod(nameof(PrefixDrawString1))));
        }
        {
            var method = spriteBatch.GetMethod(DrawStringName,
                [typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color), typeof(float),
            typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float)]);
            harmony.Patch(method,
                new(GetMethod(nameof(PrefixDrawString2))));
        }
        {
            //call all reference
            //class IClickableMenu methods: drawHoverText
            //class SaveGameMenu methods: draw
            //class Item methods: drawTooltip
            Type[] allParam = [typeof(SpriteFont), typeof(StringBuilder), typeof(Vector2), typeof(Color)];
            var method = spriteBatch.GetMethod(DrawStringName, allParam);
            harmony.Patch(method, new(GetMethod(nameof(PrefixDrawString3))));
        }
        {
            //call main by public static void DrawString(font, stringBuilder, ,,,)

            //all reference
            //Game1._draw
            //class SV.Utility public static void drawTextWithShadow
            //but this method SV.Utillity.drawTextWithShadow not found any reference

            //Type[] allParam = [typeof(SpriteFont), typeof(StringBuilder), typeof(Vector2), typeof(Color),
            //    typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float)];
            //var method = spriteBatch.GetMethod(DrawStringName, allParam);
            //harmony.Patch(method, new(GetMethod(nameof(PrefixDrawString4))));
        }

        {
            var method = typeof(Utility).GetMethod(nameof(Utility.drawMultiLineTextWithShadow));
            harmony.Patch(method, new(GetMethod(nameof(PrefixDrawMultiLineTextWithShadow))));
        }
        {
            var method = typeof(SpriteText).GetMethod(nameof(SpriteText.drawString));
            harmony.Patch(method, new(GetMethod(nameof(PrefixSpriteTextDrawString))));
        }

    }

    static void PrefixDrawString1(SpriteFont spriteFont, ref string text, Vector2 position, Color color)
    {
        text = FixText(text);
    }

    static void PrefixDrawString2(SpriteFont spriteFont, ref string text, Vector2 position, Color color,
        float rotation, Vector2 origin, Vector2 scale,
        SpriteEffects effects, float layerDepth)
    {
        text = FixText(text);
    }

    static StringBuilder lastDrawStringBuilder = null;
    static void PrefixDrawString3(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
    {
        if (lastDrawStringBuilder != text)
        {
            var allText = text.ToString();
            text.Clear();
            text.Append($"{FixText(allText)}");
            lastDrawStringBuilder = text;
        }
    }
    //static void PrefixDrawString4(SpriteFont spriteFont, ref StringBuilder text, Vector2 position,
    //    Color color, float rotation, Vector2 origin, Vector2 scale,
    //    SpriteEffects effects, float layerDepth)
    //{
    //}

    static void PrefixDrawMultiLineTextWithShadow(SpriteBatch b, ref string text, SpriteFont font, Vector2 position,
        int width, int height, Color col, bool centreY = true, bool actuallyDrawIt = true,
        bool drawShadows = true, bool centerX = true, bool bold = false,
        bool close = false, float scale = 1f)
    {
        text = FixText(text);
    }

    //NPC Talking & NPC Name
    static void PrefixSpriteTextDrawString(SpriteBatch b, ref string s, int x, int y, int characterPosition = 999999,
        int width = -1, int height = 999999, float alpha = 1f, float layerDepth = 0.88f,
        bool junimoText = false, int drawBGScroll = -1, string placeHolderScrollWidthText = "",
        int color = -1, ScrollTextAlignment scroll_text_alignment = ScrollTextAlignment.Left)
    {
        s = FixText(s);
    }
}
