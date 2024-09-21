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
    // mb this should just be derived from dict
    public class Slice
    {
      public Dictionary<int, UpdateTicks> Ticks = new Dictionary<int, UpdateTicks>();

      public Dictionary<int, UpdateTicks>.KeyCollection Keys => Ticks.Keys;
      public void Clear() => Ticks.Clear();
      public UpdateTicks this[int hash]
      {
        get => Ticks[hash];
        set => Ticks[hash] = value;
      }

      public void Add(UpdateTicks t)
      {
        Ticks[t.Hash] = Ticks.ContainsKey(t.Hash) ? Ticks[t.Hash] + t : t;
      }

      public void Add(Slice s)
      {
        foreach (int id in s.Ticks.Keys)
        {
          Ticks[id] = Ticks.ContainsKey(id) ? Ticks[id] + s.Ticks[id] : s.Ticks[id];
        }
      }

      public void Substract(Slice s)
      {
        foreach (int id in s.Ticks.Keys)
        {
          Ticks[id] = Ticks.ContainsKey(id) ? Ticks[id] - s.Ticks[id] : -s.Ticks[id];
        }
      }
    }

  }
}