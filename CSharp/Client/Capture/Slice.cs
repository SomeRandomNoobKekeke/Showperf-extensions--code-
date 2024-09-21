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
    public class Slice
    {
      public Dictionary<int, Dictionary<int, UpdateTicks>> Categories = new Dictionary<int, Dictionary<int, UpdateTicks>>();

      public Dictionary<int, Dictionary<int, UpdateTicks>>.KeyCollection Keys => Categories.Keys;
      public void Clear() => Categories.Clear();
      public Dictionary<int, UpdateTicks> this[int category]
      {
        get => Categories[category];
        set => Categories[category] = value;
      }

      public void EnsureCategory(int cat)
      {
        if (!Categories.ContainsKey(cat)) Categories[cat] = new Dictionary<int, UpdateTicks>();
      }
      public void EnsureCategory(CName cat) => EnsureCategory((int)cat);

      public void Add(UpdateTicks t)
      {
        Categories[t.Category][t.Hash] = Categories[t.Category].ContainsKey(t.Hash) ?
        Categories[t.Category][t.Hash] + t : t;
      }

      public void Add(Slice s)
      {
        foreach (int cat in s.Categories.Keys)
        {
          if (!Categories.ContainsKey(cat)) Categories[cat] = new Dictionary<int, UpdateTicks>();

          foreach (int id in s.Categories[cat].Keys)
          {
            Categories[cat][id] = Categories[cat].ContainsKey(id) ?
            Categories[cat][id] + s.Categories[cat][id] : s.Categories[cat][id];
          }
        }
      }

      public void Substract(Slice s)
      {
        foreach (int cat in s.Categories.Keys)
        {
          if (!Categories.ContainsKey(cat)) Categories[cat] = new Dictionary<int, UpdateTicks>();

          foreach (int id in s.Categories[cat].Keys)
          {
            Categories[cat][id] = Categories[cat].ContainsKey(id) ?
            Categories[cat][id] - s.Categories[cat][id] : -s.Categories[cat][id];
          }
        }
      }
    }

  }
}