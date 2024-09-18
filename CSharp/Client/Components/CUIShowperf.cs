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
      public CUIScheme Menu;


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


        CUIButton b = new CUIButton("Click");
        b.OnMouseDown += (CUIMouse m) =>
        {
          CUIComponent next = null;
          if (Pages.IsOpened(View)) { Pages.Open(Menu); return; }
          if (Pages.IsOpened(Menu)) { Pages.Open(View); return; }

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

        Menu = new CUIScheme(0, 0, 1, 1);
        Menu.BackgroundColor = Color.Yellow * 0.5f;
        CUIButton b1 = (CUIButton)Menu.Append(new CUIButton("kokoko"));
        CUIButton b2 = (CUIButton)Menu.Append(new CUIButton("kokoko"));
        CUIButton b3 = (CUIButton)Menu.Append(new CUIButton("kokoko"));

        b1.Absolute.Position = new Vector2(0, 0);
        b2.Absolute.Position = new Vector2(200, 100);
        b3.Absolute.Position = new Vector2(0, 200);

        b2.Dragable = true;

        Menu.Connect(b1, b2);


        Pages.Open(Menu);

        Capture = new CaptureManager();
        Capture.Toggle(CName.MapEntityDrawing);
      }
    }
  }
}