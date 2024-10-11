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
      ItemUpdate,
      StructureUpdate,
      HullUpdate,
      GapUpdate,
      WholeSubmarineUpdate,
      CharactersUpdate,
      MapEntityDrawing,
      LevelObjectsDrawing,
      LevelMisc,
      ItemComponentsUpdate,
      CUI = 1000,
    }


    public class CaptureState
    {
      public CName Category;
      public string Description;
      public void ToggleIsActive() => IsActive = !IsActive;
      private bool isActive; public bool IsActive
      {
        get => isActive;
        set
        {
          isActive = value;
          if (isActive) Capture.Active.Add(this);
          else Capture.Active.Remove(this);
          Capture.InvokeOnIsActiveChange(this);
          Window.Reset();
        }
      }
      public void ToggleByID() => ByID = !ByID;
      public bool byID; public bool ByID
      {
        get => byID;
        set
        {
          byID = value;
          Window.Reset();
        }
      }

      public CaptureState(CName cat)
      {
        Category = cat;
      }
    }

    public static class Capture
    {
      public static CaptureState ItemUpdate = new CaptureState(CName.ItemUpdate);
      public static CaptureState StructureUpdate = new CaptureState(CName.StructureUpdate);
      public static CaptureState HullUpdate = new CaptureState(CName.HullUpdate);
      public static CaptureState GapUpdate = new CaptureState(CName.GapUpdate);
      public static CaptureState WholeSubmarineUpdate = new CaptureState(CName.WholeSubmarineUpdate);
      public static CaptureState MapEntityDrawing = new CaptureState(CName.MapEntityDrawing);
      public static CaptureState CharactersUpdate = new CaptureState(CName.CharactersUpdate)
      {
        ByID = true,
      };
      public static CaptureState LevelObjectsDrawing = new CaptureState(CName.LevelObjectsDrawing);
      public static CaptureState LevelMisc = new CaptureState(CName.LevelMisc);
      public static CaptureState ItemComponentsUpdate = new CaptureState(CName.ItemComponentsUpdate);

      public static event Action<CaptureState> OnIsActiveChange;
      public static void InvokeOnIsActiveChange(CaptureState cs) => OnIsActiveChange?.Invoke(cs);

      public static HashSet<CaptureState> Active = new HashSet<CaptureState>();



      //TODO think, where should i put those funny relations?
      static Capture()
      {
        OnIsActiveChange += (CaptureState cs) =>
        {
          if (cs == WholeSubmarineUpdate && cs.IsActive)
          {
            ItemUpdate.IsActive = false;
            StructureUpdate.IsActive = false;
            HullUpdate.IsActive = false;
            GapUpdate.IsActive = false;
          }

          if (
            (cs == ItemUpdate && cs.IsActive) ||
            (cs == StructureUpdate && cs.IsActive) ||
            (cs == HullUpdate && cs.IsActive) ||
            (cs == GapUpdate && cs.IsActive)
          )
          {
            WholeSubmarineUpdate.IsActive = false;
          }
        };
      }

    }
  }
}