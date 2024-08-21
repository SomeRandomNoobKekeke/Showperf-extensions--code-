using System;
using System.Diagnostics;
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

    public class Stats
    {
      public List<ItemUpdateTicks> values = new List<ItemUpdateTicks>();
      public double sum;
      public double linearity;
    }

    public static void DrawStringWithScale(SpriteBatch spriteBatch, GUIFont font, string text, Vector2 position, Color color, Vector2 scale)
    {
      Vector2 textSize = font.MeasureString(text);
      GUI.DrawRectangle(spriteBatch, position, textSize, Color.Black * 0.8f, true);

      font.DrawString(spriteBatch, text, position, color, rotation: 0, origin: new Vector2(0, 0), scale, spriteEffects: SpriteEffects.None, layerDepth: 0.1f);
    }

    public class WindowView : IDisposable
    {
      public CaptureWindow window;
      public Dictionary<CaptureCategory, Stats> categories;
      public HashSet<string> tracked;

      //TODO this should be per category 
      public int showedItemsCount = 50;
      public double listShift;
      public int listOffset;
      public int lastMWScroll; // PlayerInput.ScrollWheelSpeed is garbage lol

      public float defaultStringWidth = 260f;
      public float stringHeight = GUI.AdjustForTextScale(12);

      public bool frozen = false;

      public bool showInMs = true;
      public string UnitsName { get => showInMs ? "ms" : "ticks"; }

      public string converToUnits(double t) => showInMs ?
      String.Format("{0:0.000000}", t * TicksToMs) :
      String.Format("{0:000000}", t);

      // this is for showperf_track hints
      public string[] getAllIds()
      {
        List<string> all = new List<string>();
        foreach (CaptureCategory cat in categories.Keys)
        {
          all.AddRange(categories[cat].values.Select(t => t.ID));
        }

        return all.ToArray();
      }

      public WindowView(CaptureWindow window)
      {
        categories = new Dictionary<CaptureCategory, Stats>();

        tracked = new HashSet<string>();

        this.window = window;
      }

      public void Clear()
      {
        categories.Clear();
      }

      public void ensureCategory(CaptureCategory cat)
      {
        if (!categories.ContainsKey(cat)) categories[cat] = new Stats();
      }

      public void Update()
      {
        Clear();

        foreach (CaptureCategory cat in window.totalTicks.categories.Keys)
        {
          ensureCategory(cat);

          foreach (ItemUpdateTicks t in window.totalTicks[cat].Values)
          {
            ItemUpdateTicks real = window.Accumulate ? t : t / window.frames * window.FPS;

            categories[cat].values.Add(real);
            categories[cat].sum += real.ticks;
          }

          categories[cat].values.Sort((a, b) => (int)(b.ticks - a.ticks));
        }
      }


      public void UpdateScroll()
      {
        if (PlayerInput.IsShiftDown() && PlayerInput.IsAltDown())
        {
          listShift -= (PlayerInput.mouseState.ScrollWheelValue - lastMWScroll) / 80.0;

          int maxLength = 0;
          foreach (var cat in categories.Values)
          {
            maxLength = Math.Max(maxLength, cat.values.Count);
          }

          listShift = Math.Min(listShift, maxLength - showedItemsCount);
          listShift = Math.Max(0, listShift);

          listOffset = (int)Math.Round(listShift);
        }
        lastMWScroll = PlayerInput.mouseState.ScrollWheelValue;
      }

      public void DrawCategory(SpriteBatch spriteBatch, CaptureCategory cat, Vector2 pos, string caption = "", double topTicks = -1, Vector2? size = null)
      {
        if (topTicks == -1) topTicks = categories[cat].values.FirstOrDefault().ticks;
        if (caption == "") caption = $"{cat}";
        caption += $" (in {UnitsName}/sec):";

        Vector2 realSize = size ?? new Vector2(defaultStringWidth, stringHeight * showedItemsCount);


        GUI.DrawRectangle(spriteBatch, pos - new Vector2(0, 16), new Vector2(realSize.X, 16), Color.Black * 0.8f, true);
        GUI.DrawRectangle(spriteBatch, pos, realSize, Color.Black * 0.8f, true);

        GUI.DrawString(spriteBatch, new Vector2(pos.X, pos.Y - 32), caption, Color.White, Color.Black * 0.8f, 0, GUIStyle.SmallFont);

        GUIStyle.MonospacedFont.DrawString(
            spriteBatch,
            text: $"sum:{converToUnits(categories[cat].sum)} {UnitsName}",
            position: new Vector2(pos.X, pos.Y - 16),
            color: ShowperfGradient(categories[cat].sum / 1500000.0),
            rotation: 0,
            origin: new Vector2(0, 0),
            scale: new Vector2(0.8f, 0.8f),
            spriteEffects: SpriteEffects.None,
            layerDepth: 0.1f
          );



        Func<ItemUpdateTicks, Color> getColor = tracked.Count > 0 ?
        (t) => tracked.Contains(t.ID) ? ShowperfGradient(t.ticks / topTicks) : Color.DarkSlateGray :
        (t) => ShowperfGradient(t.ticks / topTicks);

        float y = 0;
        foreach (var t in categories[cat].values.Take(new Range(listOffset, listOffset + showedItemsCount)))
        {
          GUIStyle.MonospacedFont.DrawString(
            spriteBatch,
            text: $"{converToUnits(t.ticks)} {t.ID}",
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

      if (activeCategory != ShowperfCategories.None)
      {
        if (!view.frozen && Timing.TotalTime - lastTime > window.frameDuration)
        {
          while (Timing.TotalTime - lastTime > window.frameDuration)
          {
            window.Rotate();
            lastTime += window.frameDuration;
          }

          view.Update();
        }
      }

      if (activeCategory == ShowperfCategories.ItemsUpdate)
      {
        view.ensureCategory(CaptureCategory.ItemsOnMainSub);
        view.ensureCategory(CaptureCategory.ItemsOnOtherSubs);

        double topTicks = view.categories[CaptureCategory.ItemsOnMainSub].values.FirstOrDefault().ticks;

        view.DrawCategory(spriteBatch, CaptureCategory.ItemsOnMainSub, new Vector2(830, 50), "Items from main sub", topTicks);
        view.DrawCategory(spriteBatch, CaptureCategory.ItemsOnOtherSubs, new Vector2(830 + view.defaultStringWidth, 50), "Items from other subs", topTicks);
      }

      if (activeCategory == ShowperfCategories.Characters)
      {
        view.ensureCategory(CaptureCategory.Characters);
        view.DrawCategory(spriteBatch, CaptureCategory.Characters, new Vector2(830, 50), "Characters update", size: new Vector2(view.defaultStringWidth * 2, 600));
      }

      if (activeCategory == ShowperfCategories.ItemsDrawing)
      {
        view.ensureCategory(CaptureCategory.ItemsDrawing);
        view.DrawCategory(spriteBatch, CaptureCategory.ItemsDrawing, new Vector2(830, 50), "Items drawing", size: new Vector2(view.defaultStringWidth * 2, 600));
      }

      if (activeCategory == ShowperfCategories.LevelObjectsDrawing)
      {
        view.ensureCategory(CaptureCategory.LevelObjectsDrawing);
        view.ensureCategory(CaptureCategory.OtherLevelStuff);

        double topTicks = view.categories[CaptureCategory.LevelObjectsDrawing].values.FirstOrDefault().ticks;

        view.DrawCategory(spriteBatch, CaptureCategory.LevelObjectsDrawing, new Vector2(830, 50), "Level objects drawing", topTicks);
        view.DrawCategory(spriteBatch, CaptureCategory.OtherLevelStuff, new Vector2(830 + view.defaultStringWidth, 50), "Other level stuff", topTicks);
      }
    }

    #endregion
  }
}