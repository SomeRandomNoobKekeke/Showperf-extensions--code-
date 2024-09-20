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
      public Dictionary<int, UpdateTicks> Values = new Dictionary<int, UpdateTicks>();

      public Dictionary<int, UpdateTicks>.KeyCollection Keys => Values.Keys;
      public void Clear() => Values.Clear();
      public UpdateTicks this[int hash]
      {
        get => Values[hash];
        set => Values[hash] = value;
      }

      public void Add(UpdateTicks t)
      {
        Values[t.Hash] = Values.ContainsKey(t.Hash) ? Values[t.Hash] + t : t;
      }

      public void Add(Slice s)
      {
        foreach (int id in s.Values.Keys)
        {
          Values[id] = Values.ContainsKey(id) ? Values[id] + s.Values[id] : s.Values[id];
        }
      }

      public void Substract(Slice s)
      {
        foreach (int id in s.Values.Keys)
        {
          Values[id] = Values.ContainsKey(id) ? Values[id] - s.Values[id] : -s.Values[id];
        }
      }
    }

  }
}