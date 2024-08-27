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
        Linearity = 1.0 - Linearity * 2;
      }


      // -------------- for debug --------------
      public void fillWithLinearData(int part)
      {
        part = Math.Max(0, part);

        for (int i = 99; i > part; i--)
        {
          Values.Add(new UpdateTicks($"{i}", i * 1000));
          Sum += i * 1000;
        }

        for (int i = part; i >= 0; i--)
        {
          Values.Add(new UpdateTicks($"{i}", 0));
        }

        TopValue = Values.First().Ticks;
      }

      public void fillWithSpikeData()
      {
        Values.Add(new UpdateTicks($"{0}", 100000));

        for (int i = 1; i < 100; i++)
        {
          Values.Add(new UpdateTicks($"{i}", 0));
        }

        TopValue = 100000;
        Sum = 100000;
      }
    }
  }
}