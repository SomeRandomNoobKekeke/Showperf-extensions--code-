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
      public ShowperfCategories Categories = new ShowperfCategories();
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
        if (Categories.ActiveCount != 0)
        {
          Window.Update();
          View.Update();
        }
      }

      public CUIShowperf(float x, float y, float w, float h) : base(x, y, w, h)
      {
        View = new CUIView(0, 0, 1, 1);
        Append(View);
      }
    }
  }
}