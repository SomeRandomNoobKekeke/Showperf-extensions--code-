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
    public enum CName
    {
      None,
      All,
      MapEntitysUpdate,
      CharactersUpdate,
      MapEntityDrawing,
      LevelObjectsDrawing,
      LevelMisc,
      ItemComponentsUpdate,
    }


    public class CaptureState
    {
      public bool IsActive;
      public string Description;
      public bool ByID;

      public CaptureState(string description = "", bool byID = true)
      {
        Description = description;
        ByID = byID;
      }
    }

    public class CaptureManager
    {
      public Dictionary<CName, CaptureState> Capture = new Dictionary<CName, CaptureState>()
      {
        {CName.None , new CaptureState()},
        {CName.All , new CaptureState()},
        {CName.MapEntitysUpdate , new CaptureState()},
        {CName.MapEntityDrawing , new CaptureState()},
        {CName.CharactersUpdate , new CaptureState()},
        {CName.LevelObjectsDrawing , new CaptureState()},
        {CName.LevelMisc ,new CaptureState()},
        {CName.ItemComponentsUpdate , new CaptureState()},
      };
      public CaptureState this[CName name]
      {
        get => Capture[name];
        set => Capture[name] = value;
      }
      public HashSet<CName> Active = new HashSet<CName>();

      public void Toggle(CName name)
      {
        if (Capture[name].IsActive) Active.Remove(name); else Active.Add(name);
        Capture[name].IsActive = !Capture[name].IsActive;
      }
      public void ToggleByID(CName name)
      {
        Capture[name].ByID = !Capture[name].ByID;
      }

      public CaptureManager() { }
    }
  }
}