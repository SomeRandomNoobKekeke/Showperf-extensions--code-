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
      public string Name;
      public int Hash;
      public double Ticks;

      public UpdateTicks(string name, int hash, double ticks)
      {
        Name = name;
        Hash = hash;
        Ticks = ticks;
      }
      public UpdateTicks(Identifier id, double ticks)
      {
        Name = id.Value;
        Hash = id.HashCode;
        Ticks = ticks;
      }
      public UpdateTicks(string name, double ticks)
      {
        Name = name;
        Hash = name.GetHashCode();
        Ticks = ticks;
      }


      public static UpdateTicks operator +(UpdateTicks a, UpdateTicks b)
      {
        return new UpdateTicks(a.Name, a.Hash, a.Ticks + b.Ticks);
      }
      public static UpdateTicks operator +(UpdateTicks a, double ticks)
      {
        return new UpdateTicks(a.Name, a.Hash, a.Ticks + ticks);
      }

      public static UpdateTicks operator -(UpdateTicks a, UpdateTicks b)
      {
        return new UpdateTicks(a.Name, a.Hash, a.Ticks - b.Ticks);
      }

      public static UpdateTicks operator -(UpdateTicks a)
      {
        return new UpdateTicks(a.Name, a.Hash, -a.Ticks);
      }

      public static UpdateTicks operator *(UpdateTicks a, double mult)
      {
        return new UpdateTicks(a.Name, a.Hash, a.Ticks * mult);
      }

      public static UpdateTicks operator /(UpdateTicks a, double div)
      {
        return new UpdateTicks(a.Name, a.Hash, a.Ticks / div);
      }
    }

  }
}