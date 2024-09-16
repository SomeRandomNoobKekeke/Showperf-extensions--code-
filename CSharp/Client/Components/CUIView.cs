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
      public void Update()
      {

      }

      public CUIView(float x, float y, float w, float h) : base(x, y, w, h)
      {

      }
    }
  }
}