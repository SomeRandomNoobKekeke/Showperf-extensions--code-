using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public class ShowperfCategory
    {
      public bool IsActive;
      public string description;
      public bool byID;

      public ShowperfCategory(string description, bool byID = true)
      {
        this.description = description;
        this.byID = byID;
      }
    }

    public class ShowperfCategories
    {
      public ShowperfCategory None = new ShowperfCategory("");
      public ShowperfCategory MapEntitysUpdate = new ShowperfCategory("");
      public ShowperfCategory CharactersUpdate = new ShowperfCategory("");
      public ShowperfCategory MapEntityDrawing = new ShowperfCategory("");
      public ShowperfCategory LevelObjectsDrawing = new ShowperfCategory("");
      public ShowperfCategory LevelMisc = new ShowperfCategory("");
      public ShowperfCategory ItemComponentsUpdate = new ShowperfCategory("");

      public void Activate(ShowperfCategory c)
      {
        if (!c.IsActive)
        {
          c.IsActive = true;
          ActiveCount++;
        }
      }

      public void Deactivate(ShowperfCategory c)
      {
        if (c.IsActive)
        {
          c.IsActive = false;
          ActiveCount--;
        }
      }

      public int ActiveCount = 0;


      public ShowperfCategories() { }
    }
  }
}