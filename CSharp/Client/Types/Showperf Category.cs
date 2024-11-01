using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;

using System.Xml;
using System.Xml.Linq;

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

    public static class Capture
    {
      public static Dictionary<string, CaptureState> States = new Dictionary<string, CaptureState>();

      public static CaptureState Get(string id)
      {
        if (!States.ContainsKey(id)) States[id] = new CaptureState(id);
        return Capture.States[id];
      }

      public static CaptureState Set(string id, CaptureState cs = null)
      {
        if (!States.ContainsKey(id)) States[id] = cs;
        return States[id];
      }

      public static void LoadFromFile(string path = null)
      {
        path ??= Mod.ModDir + "/XML/SourceCodeTree.xml";
        XDocument xdoc = XDocument.Load(path);

        void FromElementRec(XElement e, string full)
        {
          full = full + e.Name.ToString();
          Set(full);

          foreach (XElement child in e.Elements())
          {
            FromElementRec(child, full + '.');
          }
        }

        FromElementRec(xdoc.Root, "");
      }




      // public static CaptureState ItemUpdate = new CaptureState(CName.ItemUpdate);
      // public static CaptureState StructureUpdate = new CaptureState(CName.StructureUpdate);
      // public static CaptureState HullUpdate = new CaptureState(CName.HullUpdate);
      // public static CaptureState GapUpdate = new CaptureState(CName.GapUpdate);
      // public static CaptureState WholeSubmarineUpdate = new CaptureState(CName.WholeSubmarineUpdate);
      // public static CaptureState MapEntityDrawing = new CaptureState(CName.MapEntityDrawing);
      // public static CaptureState CharactersUpdate = new CaptureState(CName.CharactersUpdate) { ByID = true, };
      // public static CaptureState LevelObjectsDrawing = new CaptureState(CName.LevelObjectsDrawing);
      // public static CaptureState LevelMisc = new CaptureState(CName.LevelMisc);
      // public static CaptureState ItemComponentsUpdate = new CaptureState(CName.ItemComponentsUpdate);


      public static event Action<CaptureState> OnStateChange;
      public static void InvokeOnStateChange(CaptureState cs) => OnStateChange?.Invoke(cs);

      public static event Action OnGlobalStateChange;
      public static void InvokeOnGlobalStateChange() => OnGlobalStateChange?.Invoke();

      public static HashSet<CaptureState> Active = new HashSet<CaptureState>();

      private static bool globalByID; public static bool GlobalByID
      {
        get => globalByID;
        set
        {
          globalByID = value;
          foreach (CaptureState cs in Capture.Active)
          {
            cs.SetByID(value);
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
          foreach (CaptureState cs in Capture.Active)
          {
            cs.SetIsActive(value);
          }
          InvokeOnGlobalStateChange();
        }
      }


      //TODO think, where should i put those funny relations?
      static Capture()
      {
        OnStateChange += (cs) => Window.Reset();
        OnGlobalStateChange += () => Window.Reset();

        // OnStateChange += (CaptureState cs) =>
        // {
        //   if (cs == WholeSubmarineUpdate && cs.IsActive)
        //   {
        //     ItemUpdate.IsActive = false;
        //     StructureUpdate.IsActive = false;
        //     HullUpdate.IsActive = false;
        //     GapUpdate.IsActive = false;
        //   }

        //   if (
        //     (cs == ItemUpdate && cs.IsActive) ||
        //     (cs == StructureUpdate && cs.IsActive) ||
        //     (cs == HullUpdate && cs.IsActive) ||
        //     (cs == GapUpdate && cs.IsActive)
        //   )
        //   {
        //     WholeSubmarineUpdate.IsActive = false;
        //   }

        //   if (cs == ItemUpdate && cs.IsActive)
        //   {
        //     ItemComponentsUpdate.IsActive = false;
        //   }

        //   if (cs == ItemComponentsUpdate && cs.IsActive)
        //   {
        //     ItemUpdate.IsActive = false;
        //   }

        // };
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