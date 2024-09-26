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
      public CaptureManager Capture = new CaptureManager();
      public SubmarineType? CaptureFrom = null;



      public CUIPages Pages;
      public CUITickList TickList;
      public CUIMap Map;


      public bool ShouldCapture(Entity e)
      {
        return !CaptureFrom.HasValue || e.Submarine.Info.Type == CaptureFrom.Value;
      }

      public void Update()
      {
        if (Capture.Active.Count != 0)
        {
          Window.Update();
          TickList.Update();
        }
      }

      public void CreateGUI()
      {
        this["handle"] = new CUIComponent(0, 0, 1, null);
        this["handle"].Absolute.Height = 22;
        this["handle"].BorderColor = Color.Transparent;


        this["buttons1"] = new CUIHorizontalList()
        {
          Absolute = new CUINullRect(null, null, null, 22),
          HideChildrenOutsideFrame = false,
        };


        CUIToggleButton ToggleByID = new CUIToggleButton("ToggleByID", 1 / 3f, 1);
        ToggleByID.OnStateChange += (state) =>
        {
          Capture.SetByID(CName.MapEntityDrawing, state);
          Window.Reset();
        };
        Remember(this["buttons1"]["byID"] = ToggleByID);




        CUIDropDown SubType = new CUIDropDown(1 / 3f, 1);
        Remember(this["buttons1"]["SubType"] = SubType);


        CUIComponent bb = Append(new CUIButton("Click"));
        bb.OnMouseDown += (CUIMouse m) =>
        {
          if (Pages.IsOpened(TickList)) { Pages.Open(Map); return; }
          if (Pages.IsOpened(Map)) { Pages.Open(TickList); return; }
        };

        Pages = (CUIPages)Append(new CUIPages());
        Pages.FillEmptySpace = true;




        TickList = new CUITickList();
        TickList.Relative.Set(0, 0, 1, 1);

        Map = new CUIMap(0, 0, 1, 1);


        CUIButton b1 = (CUIButton)Map.Add(new CUIButton("kokoko"));
        CUIButton b2 = (CUIButton)Map.Add(new CUIButton("kokoko"));
        CUIButton b3 = (CUIButton)Map.Add(new CUIButton("kokoko"));

        b1.Absolute.Position = new Vector2(0, 0);
        b2.Absolute.Position = new Vector2(200, 100);
        b3.Absolute.Position = new Vector2(0, 200);

        Map.Connect(b1, b2);
        Map.Connect(b2, b3, Color.Lime);

        Pages.Open(TickList);

      }

      public CUIShowperf(float x, float y, float w, float h) : base(x, y, w, h)
      {
        Layout = new CUILayoutVerticalList();

        CreateGUI();
      }
    }
  }
}