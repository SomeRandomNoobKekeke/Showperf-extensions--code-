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
      public static ShowperfCategory None = new ShowperfCategory("");
      public static ShowperfCategory MapEntitysUpdate = new ShowperfCategory("");
      public static ShowperfCategory CharactersUpdate = new ShowperfCategory("");
      public static ShowperfCategory MapEntityDrawing = new ShowperfCategory("");
      public static ShowperfCategory LevelObjectsDrawing = new ShowperfCategory("");
      public static ShowperfCategory LevelMisc = new ShowperfCategory("");



      public bool isActive = false; public bool IsActive
      {
        get => isActive;
        set => isActive = value;
      }
      public string description;
      public bool byID;

      public ShowperfCategory(string description, bool byID = true)
      {
        this.description = description;
        this.byID = byID;
      }
    }
  }
}