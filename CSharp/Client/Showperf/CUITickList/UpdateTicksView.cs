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
    public struct UpdateTicksView
    {
      public double Ticks;
      public string Name;
      public string OriginalName;

      public UpdateTicksView(UpdateTicks t, string name)
      {
        Ticks = t.Ticks;
        Name = name;
        OriginalName = t.Name;
      }
    }

  }
}