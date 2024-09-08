using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public struct UpdateTicks
    {
      public string ID;
      public double Ticks;

      public UpdateTicks(string id, double ticks)
      {
        this.ID = id;
        this.Ticks = ticks;
      }

      public override string ToString()
      {
        return $"{View.ConverToUnits(Ticks)} {ID}";
      }

      public static UpdateTicks operator +(UpdateTicks a, UpdateTicks b)
      {
        return new UpdateTicks(a.ID, a.Ticks + b.Ticks);
      }
      public static UpdateTicks operator +(UpdateTicks a, double ticks)
      {
        return new UpdateTicks(a.ID, a.Ticks + ticks);
      }

      public static UpdateTicks operator -(UpdateTicks a, UpdateTicks b)
      {
        return new UpdateTicks(a.ID, a.Ticks - b.Ticks);
      }

      public static UpdateTicks operator -(UpdateTicks a)
      {
        return new UpdateTicks(a.ID, -a.Ticks);
      }

      public static UpdateTicks operator *(UpdateTicks a, double mult)
      {
        return new UpdateTicks(a.ID, a.Ticks * mult);
      }

      public static UpdateTicks operator /(UpdateTicks a, double div)
      {
        return new UpdateTicks(a.ID, a.Ticks / div);
      }
    }

  }
}