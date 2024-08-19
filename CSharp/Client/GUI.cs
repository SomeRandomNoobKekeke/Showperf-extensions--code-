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

      public int showedItemsCount = 50;
      public double listShift;
      public int listOffset;
      public int lastMWScroll; // PlayerInput.ScrollWheelSpeed is garbage lol
      public Vector2 drawPos = new Vector2(850, 50);
      public Vector2 drawStep = new Vector2(240, GUI.AdjustForTextScale(12));

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

      public void ensureCategory(CaptureCategory cat)
      {
        if (!categories.ContainsKey(cat)) categories[cat] = new List<ItemUpdateTicks>();
      }

      public void Update()
      {
        categories.Clear();

        Func<ItemUpdateTicks, ItemUpdateTicks> transform = window.accumulate ? (t) => t :
        (t) => new ItemUpdateTicks()
        {
          ID = t.ID,
          ticks = (int)Math.Round((double)t.ticks / (double)window.frames * (double)window.FPS),
        };

        foreach (CaptureCategory cat in window.firstSlice.categories.Keys)
        {
          if (!categories.ContainsKey(cat)) categories[cat] = new List<ItemUpdateTicks>();

          foreach (ItemUpdateTicks t in window.firstSlice.categories[cat].Values)
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
          // foreach(var cat in categories.Values){
          //   listShift = Math.Min(
          //     listShift,
          //     Math.Max(cat.Count - showedItemsCount, cat.Count - showedItemsCount)
          //   );
          // }


          listShift = Math.Max(0, listShift);

          listOffset = (int)Math.Round(listShift);
        }
        lastMWScroll = PlayerInput.mouseState.ScrollWheelValue;
      }

      public void DrawCategory(CaptureCategory cat, SpriteBatch spriteBatch)
      {
        ensureCategory(cat);

        long topTicks = categories[cat].FirstOrDefault().ticks;

        GUI.DrawString(spriteBatch, new Vector2(drawPos.X, drawPos.Y - 16), $"{cat}", Color.White, Color.Black * 0.8f, 0, GUIStyle.SmallFont);

        GUI.DrawRectangle(spriteBatch, drawPos, new Vector2(drawStep.X, drawStep.Y * showedItemsCount), Color.Black * 0.8f, true);

        Func<ItemUpdateTicks, Color> getColor = tracked.Count > 0 ?
        (t) => tracked.Contains(t.ID) ? Color.Lime : Color.Gray :
        (t) => ToolBox.GradientLerp((float)t.ticks / topTicks * 3.0f,
                Color.MediumSpringGreen,
                Color.Yellow,
                Color.Orange,
                Color.Red,
                Color.Magenta
        );

        float y = 0;
        foreach (var t in categories[cat].Take(new Range(listOffset, listOffset + showedItemsCount)))
        {
          GUIStyle.MonospacedFont.DrawString(
            spriteBatch,
            text: $"{t.ticks} {t.ID}",
            position: new Vector2(drawPos.X, drawPos.Y + y),
            color: getColor(t),
            rotation: 0,
            origin: new Vector2(0, 0),
            scale: new Vector2(0.8f, 0.8f),
            spriteEffects: SpriteEffects.None,
            layerDepth: 0.1f
          );

          y += drawStep.Y;
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

      if (!DrawItemUpdateTimes) return;

      if (!view.frozen && Timing.TotalTime - lastTime > window.frameDuration)
      {
        view.Update();

        window.Rotate();
        lastTime = Timing.TotalTime;
      }

      view.DrawCategory(CaptureCategory.ItemsOnMainSub, spriteBatch);

    }

    #endregion
  }
}