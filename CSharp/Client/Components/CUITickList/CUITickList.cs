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
  public partial class Plugin : IAssemblyPlugin
  {
    public partial class CUITickList : CUIVerticalList
    {
      private CUITickBlock TickBlock;
      public List<UpdateTicks> Values = new List<UpdateTicks>();

      public double Sum;
      public double Linearity;
      public double Slope;
      public double TopValue;
      public HashSet<string> Tracked = new HashSet<string>();
      public bool ToggleTracking(string name)
      {
        bool wasTracked = Tracked.Contains(name);
        if (wasTracked) Tracked.Remove(name);
        else Tracked.Add(name);
        return wasTracked;
      }

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
        if (!Window.Frozen && !GameMain.Instance.Paused && ShouldUpdate)
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

          if (Values.Count < 2 || Values.First().Ticks == 0)
          {
            Linearity = 0;
          }
          else
          {
            Linearity = (Values.First().Ticks * Values.Count / 2 - Sum) / Values.First().Ticks / Values.Count;
            Linearity = 1.0 - Linearity * 2;
          }

          LastUpdateTime = Timing.TotalTime;
        }

        TickBlock.Update();
        Showperf.SumLine.Text = $"Sum:{ConverToUnits(Sum)}{UnitsName} Linearity:{String.Format("{0:0.000000}", Linearity)}";
      }

      internal override CUIBoundaries ChildOffsetBounds => new CUIBoundaries(
        minX: 0,
        maxX: 0,
        maxY: TopGap,
        minY: Math.Min(Real.Height - Values.Count * TickBlock.StringHeight - BottomGap, 0)
      );


      public CUITickList() : base()
      {
        Append(TickBlock = new CUITickBlock(this));
        ConsumeDragAndDrop = true;

        BorderColor = Color.Transparent;
        Scrollable = true;
        Swipeable = true;

        BottomGap = TickBlock.StringHeight * 2;

        OnDClick += e =>
        {
          if (e.CurrentKeyboardState.IsKeyDown(Keys.LeftShift))
          {
            Tracked.Clear();
            e.ClickConsumed = true;
          }
        };

        OnMouseDown += e =>
        {
          if (e.CurrentKeyboardState.IsKeyDown(Keys.LeftShift))
          {
            int i = (int)Math.Floor((e.MousePosition.Y - Real.Top - Scroll) / TickBlock.StringHeight);
            if (i >= 0 && i < Values.Count - 1)
            {
              ToggleTracking(Values[i].Name);
            }
          }
        };
      }

      public CUITickList(float? x = null, float? y = null, float? w = null, float? h = null) : this()
      {
        Relative = new CUINullRect(x, y, w, h);
      }
    }
  }
}