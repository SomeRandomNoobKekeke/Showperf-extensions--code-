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
      public CUITickList TickList;
      public CUIMap Menu;


      public bool ShouldCapture(Entity e)
      {
        return CaptureFrom.Count == 0 || (e.Submarine != null && CaptureFrom.Contains(e.Submarine.Info.Type));
      }

      public void Update()
      {
        if (Capture.Active.Count != 0)
        {
          Window.Update();
          TickList.Update();
        }
      }

      public CUIShowperf(float x, float y, float w, float h) : base(x, y, w, h)
      {
        Layout = new CUILayoutVerticalList(this);
        // HideChildrenOutsideFrame = false;

        CUIComponent handle = Append(new CUIComponent(0, 0, 1, null));
        handle.Absolute.Height = 15;
        handle.BorderColor = Color.Transparent;


        CUIComponent b = Append(new CUIButton("ToggleByID"));
        b.OnMouseDown += (CUIMouse m) =>
        {
          Capture.ToggleByID(CName.MapEntityDrawing);
          Window.Reset();
        };


        CUIComponent bb = Append(new CUIButton("Click"));
        bb.OnMouseDown += (CUIMouse m) =>
        {
          if (Pages.IsOpened(TickList)) { Pages.Open(Menu); return; }
          if (Pages.IsOpened(Menu)) { Pages.Open(TickList); return; }
        };

        Pages = new CUIPages();
        Pages.FillEmptySpace = true;
        Append(Pages);



        TickList = new CUITickList();
        TickList.Relative.Set(0, 0, 1, 1);



        Menu = new CUIMap(0, 0, 1, 1);

        CUIButton b1 = (CUIButton)Menu.Append(new CUIButton("kokoko"));
        CUIButton b2 = (CUIButton)Menu.Append(new CUIButton("kokoko"));
        CUIButton b3 = (CUIButton)Menu.Append(new CUIButton("kokoko"));

        b1.Absolute.Position = new Vector2(0, 0);
        b2.Absolute.Position = new Vector2(200, 100);
        b3.Absolute.Position = new Vector2(0, 200);

        Menu.OnDClick += (CUIMouse m) => Menu.Absolute.Position = Vector2.Zero;
        Menu.Draggable = true;
        Menu.Connect(b1, b2);
        Menu.Connect(b2, b3, Color.Lime);



        Pages.Open(TickList);

        Capture = new CaptureManager();
      }
    }
  }
}