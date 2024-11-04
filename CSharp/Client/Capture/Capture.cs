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
    public enum CaptureMode { Sum, Mean, Spike }
    public enum SubType { Player, Outpost, OutpostModule, Wreck, BeaconStation, EnemySubmarine, Ruin, All }

    public class CaptureClass
    {

      public CaptureWindow Draw = new CaptureWindow();
      public CaptureWindow Update = new CaptureWindow();

      public bool Frozen { get; set; }


      public void Reset()
      {
        Draw.Reset();
        Update.Reset();
      }

      public event Action<CaptureMode> OnModeChanged;
      private CaptureMode mode = CaptureMode.Mean; public CaptureMode Mode
      {
        get => mode;
        set
        {
          mode = value;
          Reset();
          OnModeChanged?.Invoke(mode);
        }
      }

      public event Action<SubType> OnCaptureFromChanged;

      private SubType captureFrom; public SubType CaptureFrom
      {
        get => captureFrom;
        set
        {
          captureFrom = value;
          Reset();
          OnCaptureFromChanged?.Invoke(captureFrom);
        }
      }

      public bool ShouldCapture(Entity e)
      {
        if (CaptureFrom == SubType.All) return true;
        if (e == null) return false;
        if (e.Submarine == null || e.Submarine.Info == null) return false;
        return (int)e.Submarine.Info.Type == (int)CaptureFrom;
      }


      private Dictionary<string, CaptureState> States = new Dictionary<string, CaptureState>();

      public CaptureState Get(string id)
      {
        if (!States.ContainsKey(id)) States[id] = new CaptureState(id);
        return States[id];
      }

      public CaptureState Set(string id, CaptureState cs = null)
      {
        if (!States.ContainsKey(id)) States[id] = cs ?? new CaptureState(id);
        return States[id];
      }

      public void PrintStates()
      {
        foreach (string key in States.Keys)
        {
          log($"{key} - {States[key]}");
        }
      }

      // Tag names joined with '.'
      public void LoadFromFile(string path = null)
      {
        path ??= Mod.ModDir + "/XML/CaptureStates.xml";
        XDocument xdoc = XDocument.Load(path);

        void FromElementRec(XElement e, string full = "")
        {
          full = full + e.Name.ToString();
          bool byID;
          bool isActive;

          bool.TryParse(e.Attribute("ById")?.Value, out byID);
          bool.TryParse(e.Attribute("IsActive")?.Value, out isActive);


          if (e.Attribute("AKA") == null)
          {
            Set(full, new CaptureState(full)
            {
              ByID = byID,
              IsActive = isActive,
              Description = e.Attribute("Description")?.Value ?? ""
            });
          }
          else
          {
            string realName = e.Attribute("AKA").Value;
            CaptureState cs = Set(realName, new CaptureState(realName)
            {
              ByID = byID,
              IsActive = isActive,
              Description = e.Attribute("Description")?.Value ?? ""
            });

            Set(full, cs); /// alias
          }



          foreach (XElement child in e.Elements())
          {
            FromElementRec(child, full + '.');
          }
        }

        foreach (XElement child in xdoc.Root.Elements())
        {
          FromElementRec(child);
        }
      }


      public event Action<CaptureState> OnStateChange;
      public void InvokeOnStateChange(CaptureState cs) => OnStateChange?.Invoke(cs);

      public event Action OnGlobalStateChange;
      public void InvokeOnGlobalStateChange() => OnGlobalStateChange?.Invoke();

      public HashSet<CaptureState> Active = new HashSet<CaptureState>();

      private bool globalByID; public bool GlobalByID
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

      private bool globalIsActive; public bool GlobalIsActive
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




      public CaptureClass()
      {
        OnStateChange += (cs) => Reset();
        OnGlobalStateChange += () => Reset();
      }
    }
  }
}