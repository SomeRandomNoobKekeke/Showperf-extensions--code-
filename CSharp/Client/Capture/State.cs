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
    public class CaptureState
    {
      public Identifier ID;
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
        if (isActive) Capture.Active.Add(this);
        else Capture.Active.Remove(this);
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

      public CaptureState(string id) { ID = new Identifier(id); }

      public override string ToString() => ID.ToString();
      public static CaptureState Parse(string id) => Capture.Get(id);
    }
  }
}