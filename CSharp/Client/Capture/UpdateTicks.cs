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
        Name = name;
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
        Name = name;
        Hash = name.GetHashCode();
        Ticks = ticks;
        Category = category;
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
    }

  }
}