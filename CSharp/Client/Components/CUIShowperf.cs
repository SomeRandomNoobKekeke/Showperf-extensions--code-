using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
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
      CUIPages Pages;
      public CUIView View;
      public CUIComponent Menu;


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

        CUIComponent handle = Append(new CUIComponent(0, 0, 1, null));
        handle.Absolute.Height = 15;


        CUIButton b = new CUIButton("By ID");
        b.OnMouseDown += (CUIMouse m) =>
        {
          CUIComponent next = null;
          if (Pages.IsOpened(View)) next = Menu;
          if (Pages.IsOpened(Menu)) next = View;
          Pages.Open(next);
          // Capture.ToggleByID(CName.MapEntityDrawing);
          // Window.Reset();
        };
        Append(b);

        Pages = new CUIPages();
        Pages.FillEmptySpace = true;
        Append(Pages);

        View = new CUIView();
        View.Relative.Set(0, 0, 1, 1);

        //Append(View);

        Menu = new CUIVerticalList(0, 0, 1, 1);
        Menu.Append(new CUIButton("kokoko"));
        Menu.Append(new CUIButton("kokoko"));
        Menu.Append(new CUIButton("kokoko"));
        Menu.Append(new CUIButton("kokoko"));
        Menu.Append(new CUIButton("kokoko"));

        Pages.Open(Menu);

        Capture = new CaptureManager();
        Capture.Toggle(CName.MapEntityDrawing);
      }
    }
  }
}