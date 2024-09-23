using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using HarmonyLib;
using CrabUI;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public class CUITickList : CUIVerticalList
    {
      private class CUITickBlock : CUIComponent
      {
        public CUITickList TickList;

        public float StringHeight = 12f;

        public Color TextColor { get; set; } = Color.White;
        public GUIFont Font { get; set; } = GUIStyle.MonospacedFont;
        public float TextScale { get; set; } = 0.8f;

        public int ScrollSurround = 0;

        public void Update()
        {
          Absolute.Height = (TickList.Values.Count + 2) * StringHeight; // +2 just for gap
        }

        protected override void Draw(SpriteBatch spriteBatch)
        {
          float y = 0;

          int start = (int)Math.Floor(-TickList.Scroll / StringHeight) - ScrollSurround;
          int end = (int)Math.Ceiling((-TickList.Scroll + TickList.Real.Height) / StringHeight) + ScrollSurround;
          start = Math.Max(0, Math.Min(start, TickList.Values.Count));
          end = Math.Max(0, Math.Min(end, TickList.Values.Count));

          for (int i = start; i < end; i++)
          {
            Font.DrawString(
                spriteBatch,
                TickList.GetName(TickList.Values[i]),
                Real.Position + new Vector2(0, i * StringHeight),
                TickList.GetColor(TickList.Values[i]),
                rotation: 0,
                origin: Vector2.Zero,
                TextScale,
                spriteEffects: SpriteEffects.None,
                layerDepth: 0.1f
              );
          }

        }

        public CUITickBlock(CUITickList tickList)
        {
          TickList = tickList;
          BackgroundColor = Color.Transparent;
          BorderColor = Color.Transparent;

          Dragable = true;
          OnDrag += (x, y) => TickList.Scroll = y;
        }
      }




      private CUITickBlock TickBlock;
      public List<UpdateTicks> Values = new List<UpdateTicks>();
      public HashSet<string> Tracked = new HashSet<string>();
      public double Sum;
      public double Linearity;
      public double Slope;
      public double TopValue;

      public void Clear()
      {
        Values.Clear();
        Sum = 0;
        Linearity = 0;
        TopValue = 0;
        Slope = 0;
      }

      public static double TicksToMs = 1000.0 / Stopwatch.Frequency;
      public bool ShowInMs = true;
      public string UnitsName { get => ShowInMs ? "ms" : "ticks"; }
      public string ConverToUnits(double t) => ShowInMs ?
      String.Format("{0:0.000000}", t * TicksToMs) :
      String.Format("{0:000000}", t);

      public string GetName(UpdateTicks t)
      {
        return $"{ConverToUnits(t.Ticks)} {t.Name}";

      }
      public Color GetColor(UpdateTicks t)
      {
        Color cl = ShowperfGradient(t.Ticks / TopValue);
        return Tracked.Count != 0 && !Tracked.Contains(t.Name) ? Color.DarkSlateGray : cl;
      }
      public Color ShowperfGradient(double f) => ShowperfGradient((float)f);

      public Color ShowperfGradient(float f)
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

      public double LastUpdateTime;

      public bool ShouldUpdate => Timing.TotalTime - LastUpdateTime > Window.FrameDuration;

      public void Update()
      {
        if (Window.Frozen || GameMain.Instance.Paused) return;
        if (ShouldUpdate)
        {
          Clear();


          foreach (int cat in Window.TotalTicks.Keys)
          {
            foreach (int id in Window.TotalTicks[cat].Keys)
            {
              UpdateTicks t = Window.GetTotal(cat, id);
              Values.Add(t);
              Sum += t.Ticks;

              TopValue = Math.Max(TopValue, t.Ticks);
            }
          }


          Values.Sort((a, b) => (int)(b.Ticks - a.Ticks));

          TickBlock.Update();

          if (Values.Count < 2 || Values.First().Ticks == 0)
          {
            Linearity = 0;
            return;
          }

          Linearity = (Values.First().Ticks * Values.Count / 2 - Sum) / Values.First().Ticks / Values.Count;
          Linearity = 1.0 - Linearity * 2;

          LastUpdateTime = Timing.TotalTime;
        }
      }

      public CUITickList() : base()
      {
        TickBlock = new CUITickBlock(this);
        Append(TickBlock);
        BackgroundColor = Color.Black * 0.75f;
        BorderColor = Color.Transparent;
        HideChildrenOutsideFrame = false;
      }
    }
  }
}