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
    public static double lastTime;

    public class WindowView : IDisposable
    {
      public CaptureWindow window;
      public List<ItemUpdateTicks> mainSub;
      public List<ItemUpdateTicks> otherSubs;
      public HashSet<string> tracked;

      public int showedItemsCount = 50;
      public double listShift;
      public int listOffset;
      public int lastScroll; // PlayerInput.ScrollWheelSpeed is garbage lol

      public bool frozen = false;

      public WindowView(CaptureWindow window)
      {
        mainSub = new List<ItemUpdateTicks>();
        otherSubs = new List<ItemUpdateTicks>();
        tracked = new HashSet<string>();

        this.window = window;
      }

      public void Update()
      {
        if (window.accumulate)
        {
          mainSub.Clear();
          foreach (ItemUpdateTicks first in window.firstSlice.mainSub.Values) mainSub.Add(first);
          mainSub.Sort((a, b) => (int)(b.ticks - a.ticks));

          otherSubs.Clear();
          foreach (ItemUpdateTicks first in window.firstSlice.otherSubs.Values) otherSubs.Add(first);
          otherSubs.Sort((a, b) => (int)(b.ticks - a.ticks));
        }
        else
        {
          mainSub.Clear();
          foreach (ItemUpdateTicks total in window.totalTicks.mainSub.Values) mainSub.Add(new ItemUpdateTicks()
          {
            ID = total.ID,
            ticks = (int)Math.Round((double)total.ticks / (double)window.frames * (double)window.FPS),
          });
          mainSub.Sort((a, b) => (int)(b.ticks - a.ticks));

          otherSubs.Clear();
          foreach (ItemUpdateTicks total in window.totalTicks.otherSubs.Values) otherSubs.Add(new ItemUpdateTicks()
          {
            ID = total.ID,
            ticks = (int)Math.Round((double)total.ticks / (double)window.frames * (double)window.FPS),
          });
          otherSubs.Sort((a, b) => (int)(b.ticks - a.ticks));
        }
      }

      public void updateScroll()
      {
        if (PlayerInput.IsShiftDown())
        {
          listShift -= (PlayerInput.mouseState.ScrollWheelValue - lastScroll) / 80.0;
          listShift = Math.Min(
            listShift,
            Math.Max(mainSub.Count - showedItemsCount, otherSubs.Count - showedItemsCount)
          );
          listShift = Math.Max(0, listShift);

          listOffset = (int)Math.Round(listShift);
        }
        lastScroll = PlayerInput.mouseState.ScrollWheelValue;
      }

      public void Dispose()
      {
        mainSub.Clear();
        otherSubs.Clear();
        tracked.Clear();
        tracked = null;
        mainSub = null;
        otherSubs = null;
      }
    }

    public static WindowView view;

    public static void GUI_Draw_Postfix(Camera cam, SpriteBatch spriteBatch)
    {
      view.updateScroll();

      if (!DrawItemUpdateTimes) return;

      if (!view.frozen && Timing.TotalTime - lastTime > window.frameDuration)
      {
        view.Update();

        window.Rotate();
        lastTime = Timing.TotalTime;
      }


      float yStep = GUI.AdjustForTextScale(12);

      float xStep = 240.0f;
      long topTicks = view.mainSub.FirstOrDefault().ticks;


      drawUpdateTime(850, 50, view.mainSub, new Range(view.listOffset, view.listOffset + view.showedItemsCount), "Items from main sub:");
      drawUpdateTime(850 + xStep, 50, view.otherSubs, new Range(view.listOffset, view.listOffset + view.showedItemsCount), "Items from other subs:");

      void drawUpdateTime(float x, float y, List<ItemUpdateTicks> list, Range r, string caption)
      {
        GUI.DrawString(spriteBatch, new Vector2(x, y - 16), caption, Color.White, Color.Black * 0.8f, 0, GUIStyle.SmallFont);

        GUI.DrawRectangle(spriteBatch, new Vector2(x, y), new Vector2(xStep, yStep * view.showedItemsCount), Color.Black * 0.8f, true);



        if (view.tracked.Count > 0)
        {
          foreach (var t in list.Take(r))
          {
            GUIStyle.MonospacedFont.DrawString(
              spriteBatch,
              text: $"{t.ticks} {t.ID}",
              position: new Vector2(x, y),
              color: view.tracked.Contains(t.ID) ? Color.Lime : Color.Gray,
              rotation: 0,
              origin: new Vector2(0, 0),
              scale: new Vector2(0.8f, 0.8f),
              spriteEffects: SpriteEffects.None,
              layerDepth: 0.1f
            );

            y += yStep;
          }
        }
        else
        {
          foreach (var t in list.Take(r))
          {
            GUIStyle.MonospacedFont.DrawString(
              spriteBatch,
              text: $"{t.ticks} {t.ID}",
              position: new Vector2(x, y),
              color: getGradient((float)t.ticks / topTicks * 3.0f),
              rotation: 0,
              origin: new Vector2(0, 0),
              scale: new Vector2(0.8f, 0.8f),
              spriteEffects: SpriteEffects.None,
              layerDepth: 0.1f
            );

            y += yStep;
          }
        }


      }

      Color getGradient(float d)
      {
        return ToolBox.GradientLerp(d,
              Color.MediumSpringGreen,
              Color.Yellow,
              Color.Orange,
              Color.Red,
              Color.Magenta
        );
      }
    }
  }
}