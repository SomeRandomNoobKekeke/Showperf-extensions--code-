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
    public class Slice
    {
      public double Total;

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

      public void Add(UpdateTicks t)
      {
        Categories[t.Category][t.Hash] = Categories[t.Category].ContainsKey(t.Hash) ?
        Categories[t.Category][t.Hash] + t : t;
      }

      public void ReplaceWithMax(Slice s)
      {
        foreach (int cat in s.Categories.Keys)
        {
          if (!Categories.ContainsKey(cat)) Categories[cat] = new Dictionary<int, UpdateTicks>();

          foreach (int id in s.Categories[cat].Keys)
          {
            Categories[cat][id] = Categories[cat].ContainsKey(id) ?
            UpdateTicks.Max(Categories[cat][id], s.Categories[cat][id]) : s.Categories[cat][id];
          }
        }
      }

      public void RemoveMatches(Slice s)
      {
        foreach (int cat in s.Categories.Keys)
        {
          if (!Categories.ContainsKey(cat)) Categories[cat] = new Dictionary<int, UpdateTicks>();

          foreach (int id in s.Categories[cat].Keys)
          {
            if (Categories[cat].ContainsKey(id))
            {
              if (Categories[cat][id].Ticks == s.Categories[cat][id].Ticks)
              {
                Categories[cat][id] = Categories[cat][id] with { Ticks = 0 };
              }
            }
          }
        }
      }

      public void Add(Slice s)
      {
        Total += s.Total;

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
        Total -= s.Total;

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