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
  public partial class Plugin : IAssemblyPlugin
  {
    public struct UpdateTicks
    {
      public string Name;
      public int Hash;
      public int Category;
      public double Ticks;

      public UpdateTicks(double ticks, int category, string name, int hash)
      {
        Name = name ?? "";
        Hash = hash;
        Ticks = ticks;
        Category = category;
      }
      public UpdateTicks(double ticks, int category, Identifier id)
      {
        Name = id.Value;
        Hash = id.HashCode;
        Ticks = ticks;
        Category = category;
      }
      public UpdateTicks(double ticks, int category, string name)
      {
        Name = name ?? "";
        Hash = Name.GetHashCode();
        Ticks = ticks;
        Category = category;
      }

      public UpdateTicks(double ticks, CaptureState cs, string name, int hash) : this(ticks, cs.ID.HashCode, name, hash) { }
      public UpdateTicks(double ticks, CaptureState cs, Identifier id) : this(ticks, cs.ID.HashCode, id) { }
      public UpdateTicks(double ticks, CaptureState cs, string name) : this(ticks, cs.ID.HashCode, name) { }

      public UpdateTicks Max(UpdateTicks a)
      {
        if (a.Ticks > this.Ticks) return a;
        else return this;
      }

      public static UpdateTicks Max(UpdateTicks a, UpdateTicks b)
      {
        if (a.Ticks > b.Ticks) return a;
        else return b;
      }

      public static UpdateTicks operator +(UpdateTicks a, UpdateTicks b)
      {
        return new UpdateTicks(a.Ticks + b.Ticks, a.Category, a.Name, a.Hash);
      }
      public static UpdateTicks operator +(UpdateTicks a, double ticks)
      {
        return new UpdateTicks(a.Ticks + ticks, a.Category, a.Name, a.Hash);
      }

      public static UpdateTicks operator -(UpdateTicks a, UpdateTicks b)
      {
        return new UpdateTicks(a.Ticks - b.Ticks, a.Category, a.Name, a.Hash);
      }

      public static UpdateTicks operator -(UpdateTicks a)
      {
        return new UpdateTicks(-a.Ticks, a.Category, a.Name, a.Hash);
      }

      public static UpdateTicks operator *(UpdateTicks a, double mult)
      {
        return new UpdateTicks(a.Ticks * mult, a.Category, a.Name, a.Hash);
      }

      public static UpdateTicks operator /(UpdateTicks a, double div)
      {
        return new UpdateTicks(a.Ticks / div, a.Category, a.Name, a.Hash);
      }

      public override string ToString() => $"{Name} {Ticks}";
    }

  }
}