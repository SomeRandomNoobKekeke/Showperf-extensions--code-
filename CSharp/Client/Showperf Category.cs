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
      CUI = 1000,
    }


    public class CaptureState
    {
      public CName Category;
      public string Description;
      private bool isActive; public bool IsActive
      {
        get => isActive;
        set
        {
          isActive = value;
          ByID = Capture.ById;
          if (isActive) Capture.Active.Add(this);
          else Capture.Active.Remove(this);
        }
      }
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
      public static CaptureState None = new CaptureState(CName.None);
      public static CaptureState All = new CaptureState(CName.All);
      public static CaptureState MapEntitysUpdate = new CaptureState(CName.MapEntitysUpdate);
      public static CaptureState MapEntityDrawing = new CaptureState(CName.MapEntityDrawing);
      public static CaptureState CharactersUpdate = new CaptureState(CName.CharactersUpdate);
      public static CaptureState LevelObjectsDrawing = new CaptureState(CName.LevelObjectsDrawing);
      public static CaptureState LevelMisc = new CaptureState(CName.LevelMisc);
      public static CaptureState ItemComponentsUpdate = new CaptureState(CName.ItemComponentsUpdate);


      public static HashSet<CaptureState> Active = new HashSet<CaptureState>();

      public static bool byId; public static bool ById
      {
        get => byId;
        set
        {
          byId = value;
          foreach (CaptureState s in Active)
          {
            s.byID = value;
          }
          Window.Reset();
        }
      }

    }
  }
}