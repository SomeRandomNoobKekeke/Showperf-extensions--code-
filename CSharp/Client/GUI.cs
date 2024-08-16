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

      public WindowView(CaptureWindow window)
      {
        mainSub = new List<ItemUpdateTicks>();
        otherSubs = new List<ItemUpdateTicks>();
        this.window = window;
      }

      public void Update()
      {
        if (window.frozen)
        {
          mainSub.Clear();
          foreach (ItemUpdateTicks v in window.firstSlice.mainSub.Values) mainSub.Add(v);
          mainSub.Sort((a, b) => (int)(b.ticks - a.ticks));

          otherSubs.Clear();
          foreach (ItemUpdateTicks v in window.firstSlice.otherSubs.Values) otherSubs.Add(v);
          otherSubs.Sort((a, b) => (int)(b.ticks - a.ticks));
        }
        else
        {
          mainSub.Clear();
          foreach (ItemUpdateTicks v in window.totalTicks.mainSub.Values) mainSub.Add(new ItemUpdateTicks()
          {
            ID = v.ID,
            ticks = v.ticks / window.captureWindowSections,
          });
          mainSub.Sort((a, b) => (int)(b.ticks - a.ticks));

          otherSubs.Clear();
          foreach (ItemUpdateTicks v in window.totalTicks.otherSubs.Values) otherSubs.Add(new ItemUpdateTicks()
          {
            ID = v.ID,
            ticks = v.ticks / window.captureWindowSections,
          });
          otherSubs.Sort((a, b) => (int)(b.ticks - a.ticks));
        }
      }

      public void Dispose()
      {
        mainSub.Clear();
        otherSubs.Clear();
        mainSub = null;
        otherSubs = null;
      }
    }

    public static WindowView view;

    public static void GUI_Draw_Postfix(Camera cam, SpriteBatch spriteBatch)
    {
      if (!DrawItemUpdateTimes) return;


      if (Timing.TotalTime - lastTime > window.captureWindowSectionLength)
      {
        view.Update();

        window.Rotate();
        lastTime = Timing.TotalTime;
      }


      float yStep = GUI.AdjustForTextScale(12);

      float xStep = 240.0f;
      int listStep = 50;
      long topTicks = view.mainSub.FirstOrDefault().ticks;


      drawUpdateTime(850, 50, view.mainSub, new Range(0, listStep), "Items from main sub:");
      drawUpdateTime(850 + xStep, 50, view.otherSubs, new Range(0, listStep), "Items from other subs:");

      void drawUpdateTime(float x, float y, List<ItemUpdateTicks> list, Range r, string caption)
      {
        GUI.DrawString(spriteBatch, new Vector2(x, y - 16), caption, Color.White, Color.Black * 0.7f, 0, GUIStyle.SmallFont);

        GUI.DrawRectangle(spriteBatch, new Vector2(x, y), new Vector2(xStep, yStep * listStep), Color.Black * 0.7f, true);

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