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
      public Dictionary<CName, Dictionary<int, UpdateTicks>> Categories;

      public Slice()
      {
        Categories = new Dictionary<CName, Dictionary<int, UpdateTicks>>();
      }

      public void Clear()
      {
        Categories.Clear();
      }

      public Dictionary<int, UpdateTicks> this[CName cat]
      {
        get => Categories[cat];
        set => Categories[cat] = value;
      }

      public void Add(Slice s)
      {
        foreach (CName cat in s.Categories.Keys)
        {
          if (!Categories.ContainsKey(cat)) Categories[cat] = new Dictionary<int, UpdateTicks>();

          foreach (int id in s.Categories[cat].Keys)
          {
            if (!Categories[cat].ContainsKey(id)) Categories[cat][id] = s.Categories[cat][id];
            else Categories[cat][id] += s.Categories[cat][id];
          }
        }
      }

      public void Substract(Slice s)
      {
        foreach (CName cat in s.Categories.Keys)
        {
          if (!Categories.ContainsKey(cat)) Categories[cat] = new Dictionary<int, UpdateTicks>();

          foreach (int id in s.Categories[cat].Keys)
          {
            if (!Categories[cat].ContainsKey(id)) Categories[cat][id] = -s.Categories[cat][id];
            else Categories[cat][id] -= s.Categories[cat][id];
          }
        }
      }
    }

  }
}