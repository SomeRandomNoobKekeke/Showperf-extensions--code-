using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    #region cringe
    public class WindowView : IDisposable
    {
      public CaptureWindow window;
      public Dictionary<CaptureCategory, List<ItemUpdateTicks>> categories;
      public HashSet<string> tracked;

      //TODO this should be per category 
      public int showedItemsCount = 50;
      public double listShift;
      public int listOffset;
      public int lastMWScroll; // PlayerInput.ScrollWheelSpeed is garbage lol

      public float defaultStringWidth = 240f;
      public float stringHeight = GUI.AdjustForTextScale(12);

      public bool frozen = false;

      public WindowView(CaptureWindow window)
      {
        categories = new Dictionary<CaptureCategory, List<ItemUpdateTicks>>();
        tracked = new HashSet<string>();

        this.window = window;
      }

      public void Clear()
      {

      }

      // this is for showperf_track hints
      public string[] getAllIds()
      {
        List<string> all = new List<string>();
        foreach (CaptureCategory cat in categories.Keys)
        {
          all.AddRange(categories[cat].Select(t => t.ID));
        }

        return all.ToArray();
      }

      public void ensureCategory(CaptureCategory cat)
      {
        if (!categories.ContainsKey(cat)) categories[cat] = new List<ItemUpdateTicks>();
      }

      public void Update()
      {
        categories.Clear();

        Func<ItemUpdateTicks, ItemUpdateTicks> transform = window.Accumulate ?
        (t) => t :
        (t) => new ItemUpdateTicks()
        {
          ID = t.ID,
          ticks = (int)Math.Round((double)t.ticks / (double)window.frames * (double)window.FPS),
        };

        foreach (CaptureCategory cat in window.totalTicks.categories.Keys)
        {
          ensureCategory(cat);

          foreach (ItemUpdateTicks t in window.totalTicks[cat].Values)
          {
            categories[cat].Add(transform(t));
          }

          categories[cat].Sort((a, b) => (int)(b.ticks - a.ticks));
        }
      }


      public void UpdateScroll()
      {
        if (PlayerInput.IsShiftDown())
        {
          listShift -= (PlayerInput.mouseState.ScrollWheelValue - lastMWScroll) / 80.0;

          int maxLength = 0;
          foreach (var cat in categories.Values)
          {
            maxLength = Math.Max(maxLength, cat.Count);
          }

          listShift = Math.Min(listShift, maxLength - showedItemsCount);
          listShift = Math.Max(0, listShift);

          listOffset = (int)Math.Round(listShift);
        }
        lastMWScroll = PlayerInput.mouseState.ScrollWheelValue;
      }

      public void DrawCategory(SpriteBatch spriteBatch, CaptureCategory cat, Vector2 pos, string caption = "", long topTicks = -1, Vector2? size = null)
      {
        if (topTicks == -1) topTicks = categories[cat].FirstOrDefault().ticks;
        if (caption == "") caption = $"{cat}";

        Vector2 realSize = size ?? new Vector2(defaultStringWidth, stringHeight * showedItemsCount);

        GUI.DrawString(spriteBatch, new Vector2(pos.X, pos.Y - 16), caption, Color.White, Color.Black * 0.8f, 0, GUIStyle.SmallFont);

        GUI.DrawRectangle(spriteBatch, pos, realSize, Color.Black * 0.8f, true);

        Color getGradient(float f)
        {
          return ToolBox.GradientLerp(f,
                Color.MediumSpringGreen,
                Color.Yellow,
                Color.Orange,
                Color.Red,
                Color.Magenta,
                Color.Magenta
          );
        }

        Func<ItemUpdateTicks, Color> getColor = tracked.Count > 0 ?
        (t) => tracked.Contains(t.ID) ? getGradient((float)t.ticks / topTicks) : Color.DarkSlateGray :
        (t) => getGradient((float)t.ticks / topTicks);

        float y = 0;
        foreach (var t in categories[cat].Take(new Range(listOffset, listOffset + showedItemsCount)))
        {
          GUIStyle.MonospacedFont.DrawString(
            spriteBatch,
            text: $"{t.ticks} {t.ID}",
            position: new Vector2(pos.X, pos.Y + y),
            color: getColor(t),
            rotation: 0,
            origin: new Vector2(0, 0),
            scale: new Vector2(0.8f, 0.8f),
            spriteEffects: SpriteEffects.None,
            layerDepth: 0.1f
          );

          y += stringHeight;
        }
      }

      public void Dispose()
      {
        categories.Clear();
        categories = null;
        tracked.Clear();
        tracked = null;
      }
    }

    public static WindowView view;
    public static double lastTime;
    public static void GUI_Draw_Postfix(Camera cam, SpriteBatch spriteBatch)
    {
      view.UpdateScroll();

      if (activeCategory != ShowperfCategories.none)
      {
        if (!view.frozen && Timing.TotalTime - lastTime > window.frameDuration)
        {
          view.Update();

          window.Rotate();
          lastTime = Timing.TotalTime;
        }
      }

      if (activeCategory == ShowperfCategories.items)
      {
        view.ensureCategory(CaptureCategory.ItemsOnMainSub);
        view.ensureCategory(CaptureCategory.ItemsOnOtherSubs);

        long topTicks = view.categories[CaptureCategory.ItemsOnMainSub].FirstOrDefault().ticks;

        view.DrawCategory(spriteBatch, CaptureCategory.ItemsOnMainSub, new Vector2(850, 50), "Items from main sub:", topTicks);
        view.DrawCategory(spriteBatch, CaptureCategory.ItemsOnOtherSubs, new Vector2(850 + view.defaultStringWidth, 50), "Items from other subs:", topTicks);
      }

      if (activeCategory == ShowperfCategories.characters)
      {
        view.ensureCategory(CaptureCategory.Characters);
        view.DrawCategory(spriteBatch, CaptureCategory.Characters, new Vector2(850, 50), "Characters:", size: new Vector2(480, 600));
      }
    }

    #endregion
  }
}