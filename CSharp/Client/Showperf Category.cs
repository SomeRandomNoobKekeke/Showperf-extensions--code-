using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;

namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
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
      public static Dictionary<CName, CaptureState> States = new Dictionary<CName, CaptureState>();
      public static CaptureState Get(CName cat)
      {
        if (!States.ContainsKey(cat)) States[cat] = new CaptureState(cat);
        return States[cat];
      }
      public CName Category;
      public string Description;
      public void ToggleIsActive() => IsActive = !IsActive;
      private bool isActive; public bool IsActive
      {
        get => isActive;
        set
        {
          SetIsActive(value);
          Capture.InvokeOnStateChange(this);
        }
      }
      public void SetIsActive(bool value)
      {
        isActive = value;
        if (isActive) Capture.Active.Add(Category);
        else Capture.Active.Remove(Category);
      }


      public void ToggleByID() => ByID = !ByID;
      private bool byID; public bool ByID
      {
        get => byID;
        set
        {
          SetByID(value);
          Capture.InvokeOnStateChange(this);
        }
      }
      public void SetByID(bool value)
      {
        byID = value;
      }

      public CaptureState(CName cat)
      {
        Category = cat;

        if (States.ContainsKey(cat)) throw new Exception($"Capture name {cat} already added to CaptureState.States\nReplacing this CaptureState object will lead to sneaky state desync so enjoy an exception\nUse CaptureState.Get(CName cat) instead");

        States[cat] = this;
      }

      public override string ToString() => Category.ToString();
      public static CaptureState Parse(string s) => Get(Enum.Parse<CName>(s));
    }

    public static class Capture
    {
      public static CaptureState ItemUpdate = new CaptureState(CName.ItemUpdate);
      public static CaptureState StructureUpdate = new CaptureState(CName.StructureUpdate);
      public static CaptureState HullUpdate = new CaptureState(CName.HullUpdate);
      public static CaptureState GapUpdate = new CaptureState(CName.GapUpdate);
      public static CaptureState WholeSubmarineUpdate = new CaptureState(CName.WholeSubmarineUpdate);
      public static CaptureState MapEntityDrawing = new CaptureState(CName.MapEntityDrawing);
      public static CaptureState CharactersUpdate = new CaptureState(CName.CharactersUpdate) { ByID = true, };
      public static CaptureState LevelObjectsDrawing = new CaptureState(CName.LevelObjectsDrawing);
      public static CaptureState LevelMisc = new CaptureState(CName.LevelMisc);
      public static CaptureState ItemComponentsUpdate = new CaptureState(CName.ItemComponentsUpdate);

      public static event Action<CaptureState> OnStateChange;
      public static void InvokeOnStateChange(CaptureState cs) => OnStateChange?.Invoke(cs);

      public static event Action OnGlobalStateChange;
      public static void InvokeOnGlobalStateChange() => OnGlobalStateChange?.Invoke();

      public static HashSet<CName> Active = new HashSet<CName>();

      private static bool globalByID; public static bool GlobalByID
      {
        get => globalByID;
        set
        {
          globalByID = value;
          foreach (CName c in Capture.Active)
          {
            CaptureState.States[c].SetByID(value);
          }
          InvokeOnGlobalStateChange();
        }
      }

      private static bool globalIsActive; public static bool GlobalIsActive
      {
        get => globalIsActive;
        set
        {
          globalIsActive = value;
          foreach (CName c in Capture.Active)
          {
            CaptureState.States[c].SetIsActive(value);
          }
          InvokeOnGlobalStateChange();
        }
      }


      //TODO think, where should i put those funny relations?
      static Capture()
      {
        OnStateChange += (cs) => Window.Reset();
        OnGlobalStateChange += () => Window.Reset();

        OnStateChange += (CaptureState cs) =>
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

          if (cs == ItemUpdate && cs.IsActive)
          {
            ItemComponentsUpdate.IsActive = false;
          }

          if (cs == ItemComponentsUpdate && cs.IsActive)
          {
            ItemUpdate.IsActive = false;
          }

        };
      }

      public static CaptureState GetByName(string name)
      {
        FieldInfo fi = typeof(Capture).GetField(name, BindingFlags.Static | BindingFlags.Public);
        return (CaptureState)fi?.GetValue(null);
      }

      public static List<string> GetAllNames()
      {
        List<string> l = new List<string>();

        foreach (FieldInfo fi in typeof(Capture).GetFields(BindingFlags.Static | BindingFlags.Public))
        {
          if (fi.FieldType == typeof(CaptureState)) l.Add(fi.Name);
        }

        return l;
      }

    }
  }
}