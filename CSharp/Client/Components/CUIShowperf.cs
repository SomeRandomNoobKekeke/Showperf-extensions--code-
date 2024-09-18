using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using CrabUI;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public class CUIShowperf : CUIFrame
    {
      public CaptureManager Capture;
      public HashSet<SubmarineType> CaptureFrom = new HashSet<SubmarineType>()
      {
        // SubmarineType.Player,
      };

      public CUIView View;


      public bool ShouldCapture(Entity e)
      {
        return CaptureFrom.Count == 0 || (e.Submarine != null && CaptureFrom.Contains(e.Submarine.Info.Type));
      }

      public void Update()
      {
        if (Capture.Active.Count != 0)
        {
          Window.Update();
          View.Update();
        }
      }

      public CUIShowperf(float x, float y, float w, float h) : base(x, y, w, h)
      {
        Layout = new CUILayoutVerticalList(this);


        CUIButton b = new CUIButton("By ID");
        b.OnMouseDown += (CUIMouse m) =>
        {
          Capture.ToggleByID(CName.MapEntityDrawing);
          Window.Reset();
        };
        Append(b);


        View = new CUIView();
        View.FillEmptySpace = true;
        Append(View);

        Capture = new CaptureManager();
        Capture.Toggle(CName.MapEntityDrawing);
      }
    }
  }
}