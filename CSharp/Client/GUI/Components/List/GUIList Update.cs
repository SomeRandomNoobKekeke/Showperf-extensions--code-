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
    public partial class GUIList
    {
      public void Update()
      {
        Clear();

        foreach (CaptureCategory cat in Window.TotalTicks.Categories.Keys)
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
          return;
        }

        Linearity = (Values.First().Ticks * Values.Count / 2 - Sum) / Values.First().Ticks / Values.Count;
        Linearity *= 2; // because naturally it's [0..0.5] and i want it to be [0..1]
      }
    }
  }
}