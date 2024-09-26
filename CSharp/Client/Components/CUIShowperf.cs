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
      CUIComponent MapFrame;
      public CUIMap Map;


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
        Layout = new CUILayoutVerticalList();
        // HideChildrenOutsideFrame = false;

        CUIComponent handle = Append(new CUIComponent(0, 0, 1, null));
        handle.Absolute.Height = 15;
        handle.BorderColor = Color.Transparent;

        CUIComponent Buttons = Append(new CUIHorizontalList()
        {
          Absolute = new CUINullRect(null, null, null, 20),
          HideChildrenOutsideFrame = false,
        });




        CUIToggleButton ToggleByID = new CUIToggleButton("ToggleByID", 1 / 3f, 1);
        ToggleByID.OnStateChange += (state) =>
        {
          Capture.SetByID(CName.MapEntityDrawing, state);
          Window.Reset();
        };
        Buttons.Append(ToggleByID);

        CUIDropDown SubType = new CUIDropDown(1 / 3f, 1);
        Buttons.Append(SubType);



        CUIComponent bb = Append(new CUIButton("Click"));
        bb.OnMouseDown += (CUIMouse m) =>
        {
          if (Pages.IsOpened(TickList)) { Pages.Open(MapFrame); return; }
          if (Pages.IsOpened(MapFrame)) { Pages.Open(TickList); return; }
        };

        Pages = new CUIPages();
        Pages.FillEmptySpace = true;
        Append(Pages);



        TickList = new CUITickList();
        TickList.Relative.Set(0, 0, 1, 1);



        MapFrame = new CUIComponent(0, 0, 1, 1);
        MapFrame.Swipeable = true;
        MapFrame.OnDClick += (CUIMouse m) => MapFrame.ChildrenOffset = Vector2.Zero;
        MapFrame.BorderColor = Color.Transparent;
        MapFrame.BackgroundColor = Color.Black * 0.5f;

        Map = new CUIMap();
        MapFrame.Append(Map);

        CUIButton b1 = (CUIButton)Map.Append(new CUIButton("kokoko"));
        CUIButton b2 = (CUIButton)Map.Append(new CUIButton("kokoko"));
        CUIButton b3 = (CUIButton)Map.Append(new CUIButton("kokoko"));

        b1.Absolute.Position = new Vector2(0, 0);
        b2.Absolute.Position = new Vector2(200, 100);
        b3.Absolute.Position = new Vector2(0, 200);

        Map.Connect(b1, b2);
        Map.Connect(b2, b3, Color.Lime);

        Pages.Open(TickList);

        Capture = new CaptureManager();
      }
    }
  }
}