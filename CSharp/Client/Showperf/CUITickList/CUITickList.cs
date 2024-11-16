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
      public List<UpdateTicksView> Values = new List<UpdateTicksView>();

      public double Sum;
      public double Linearity;
      public double Slope;
      public double TopValue;
      public HashSet<string> Tracked = new HashSet<string>();
      public HashSet<string> Highlighted = new HashSet<string>();
      public bool ToggleHighlight(string name)
      {
        bool was = Highlighted.Contains(name);
        if (was) Highlighted.Remove(name);
        else Highlighted.Add(name);
        return was;
      }
      public bool ToggleTracking(string name)
      {
        bool was = Tracked.Contains(name);
        if (was) Tracked.Remove(name);
        else Tracked.Add(name);
        return was;
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

      public bool IsTracked(UpdateTicks t)
      {
        return Tracked.Count == 0 || Tracked.Any(s => t.Name.Contains(s));
      }

      public string GetName(UpdateTicks t)
      {
        return $"{ConverToUnits(t.Ticks)} {t.Name}";
      }
      public Color GetColor(UpdateTicksView t)
      {
        Color cl = ShowperfGradient(t.Ticks / TopValue);
        return Highlighted.Count != 0 && !Highlighted.Any(s => t.OriginalName.Contains(s)) ? Color.DarkSlateGray : cl;
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


      public void Update()
      {
        if (!Capture.Frozen && !GameMain.Instance.Paused && (Timing.TotalTime - Showperf.lastUpdateTime >= Showperf.UpdateInterval))
        {
          Showperf.lastUpdateTime = Timing.TotalTime;

          Clear();

          foreach (int cat in Capture.Draw.TotalTicks.Keys)
          {
            foreach (int id in Capture.Draw.TotalTicks[cat].Keys)
            {
              UpdateTicks t = Capture.Draw.GetTotal(cat, id);
              if (!IsTracked(t)) continue;
              Values.Add(new UpdateTicksView(t, GetName(t)));
              Sum += t.Ticks;

              TopValue = Math.Max(TopValue, t.Ticks);
            }
          }

          foreach (int cat in Capture.Update.TotalTicks.Keys)
          {
            foreach (int id in Capture.Update.TotalTicks[cat].Keys)
            {
              UpdateTicks t = Capture.Update.GetTotal(cat, id);
              if (!IsTracked(t)) continue;
              Values.Add(new UpdateTicksView(t, GetName(t)));
              Sum += t.Ticks;

              TopValue = Math.Max(TopValue, t.Ticks);
            }
          }

          Values.Sort((a, b) => (int)(b.Ticks - a.Ticks));

          foreach (int cat in Capture.MonoGame.TotalTicks.Keys)
          {
            foreach (int id in Capture.MonoGame.TotalTicks[cat].Keys)
            {
              UpdateTicks t = Capture.MonoGame.GetTotal(cat, id);
              if (!IsTracked(t)) continue;
              Values.Add(new UpdateTicksView(t, $"{Math.Round(t.Ticks)} {t.Name}"));

              TopValue = 1000;
            }
          }

          foreach (int cat in Capture.Farseer.TotalTicks.Keys)
          {
            foreach (int id in Capture.Farseer.TotalTicks[cat].Keys)
            {
              UpdateTicks t = Capture.Farseer.GetTotal(cat, id);
              if (!IsTracked(t)) continue;
              Values.Add(new UpdateTicksView(t, $"{String.Format("{0:0000.0000}", t.Ticks)} {t.Name}"));

              TopValue = 1000;
            }
          }

          if (Values.Count < 2 || Values.First().Ticks == 0)
          {
            Linearity = 0;
          }
          else
          {
            Linearity = (Values.First().Ticks * Values.Count / 2 - Sum) / Values.First().Ticks / Values.Count;
            Linearity = 1.0 - Linearity * 2;
          }
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
            Highlighted.Clear();
            e.ClickConsumed = true;
          }
        };

        OnMouseDown += e =>
        {
          if (e.CurrentKeyboardState.IsKeyDown(Keys.LeftShift))
          {
            int i = (int)Math.Floor((e.MousePosition.Y - Real.Top - Scroll) / TickBlock.StringHeight);
            if (i >= 0 && i <= Values.Count - 1)
            {
              ToggleHighlight(Values[i].OriginalName);
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