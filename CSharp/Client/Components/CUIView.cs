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
    public class CUIView : CUIVerticalList
    {
      public CUITickList TickList;
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

          TickList.Update();

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

      public CUIView() : base()
      {
        TickList = new CUITickList(this);
        Append(TickList);
        BackgroundColor = Color.Black * 0.75f;
        BorderColor = Color.Transparent;
        HideChildrenOutsideFrame = false;
      }
    }
  }
}