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
    public class CUIView : CUIComponent
    {
      public double LastUpdateTime;
      public bool ShouldUpdate => Timing.TotalTime - LastUpdateTime > Window.FrameDuration;

      public void Update()
      {
        if (Window.Frozen || GameMain.Instance.Paused) return;
        if (ShouldUpdate)
        {


          LastUpdateTime = Timing.TotalTime;
        }
      }

      public CUIView(float x, float y, float w, float h) : base(x, y, w, h)
      {

      }
    }
  }
}