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

      //TODO mb this should be in Window like all the other props
      private SubType captureFrom; public SubType CaptureFrom
      {
        get => captureFrom;
        set { captureFrom = value; Window.Reset(); }
      }


      public CUIVerticalList Header;
      public CUITextBlock CategoryLine;
      public CUITextBlock SumLine;

      public CUIPages Pages;
      public CUITickList TickList;
      public CUIMap Map;

      public CUIDropDown SubTypeDD;
      public CUIToggleButton ById;
      public CUIToggleButton Accumulate;

      public bool ShouldCapture(Entity e)
      {
        return CaptureFrom == SubType.All || (int)e.Submarine.Info.Type == (int)CaptureFrom;
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
        this["header"] = new CUIVerticalList()
        {
          BackgroundColor = Color.Black * 0.5f,
          FitContent = new CUIBool2(false, true),
          debug = true,
        };

        this["header"].Append(CategoryLine = new CUITextBlock("CategoryLine"));
        this["header"].Append(SumLine = new CUITextBlock("SumLine"));


        this["buttons1"] = new CUIComponent()
        {
          FitContent = new CUIBool2(false, true),
          debug = true,
        };



        this["buttons1"]["ById"] = new CUIToggleButton("By Id")
        {
          Relative = new CUINullRect(0, 0, 0.5f, null),
          AddOnStateChange = (state) => Capture.SetByID(CName.MapEntityDrawing, state),
        };

        CUIMultiButton m = new CUIMultiButton()
        {
          FillEmptySpace = new CUIBool2(true, false),
          Relative = new CUINullRect(0.5f, 0, 0.5f, null),
          AddOnSelect = (b, i) => Window.Mode = (CaptureWindowMode)b.Data,
        };

        m.Add(new CUIButton("Mean")).Data = CaptureWindowMode.Mean;
        m.Add(new CUIButton("Sum")).Data = CaptureWindowMode.Sum;
        // m.Add(new CUIButton("Spike")).Data = CaptureWindowMode.Spike;

        m.Select(0);

        this["buttons1"]["mode"] = m;


        SubTypeDD = new CUIDropDown()
        {
          AddOnSelect = (v) => CaptureFrom = Enum.Parse<SubType>(v),
        };

        foreach (SubType st in Enum.GetValues(typeof(SubType)))
        {
          SubTypeDD.Add(st);
        }

        SubTypeDD.Select(SubType.All);
        this["SubTypeDD"] = SubTypeDD;





        this["bb"] = new CUIButton("Click")
        {
          AddOnMouseDown = (CUIMouse m) =>
          {
            if (Pages.IsOpened(TickList)) { Pages.Open(Map); return; }
            if (Pages.IsOpened(Map)) { Pages.Open(TickList); return; }
          },
        };


        Pages = (CUIPages)Append(new CUIPages()
        {
          FillEmptySpace = new CUIBool2(false, true),
          Debug = true,
        });





        TickList = new CUITickList()
        {
          Relative = new CUINullRect(0, 0, 1, 1),
          Debug = true,
        };



        Map = new CUIMap(0, 0, 1, 1)
        {
          BorderColor = Color.Transparent
        };


        CUIButton b1 = (CUIButton)Map.Add(new CUIButton("kokoko"));
        CUIButton b2 = (CUIButton)Map.Add(new CUIButton("kokoko"));
        CUIButton b3 = (CUIButton)Map.Add(new CUIButton("kokoko"));

        b1.Absolute = new CUINullRect(0, 0, null, null);
        b2.Absolute = new CUINullRect(200, 100, null, null);
        b3.Absolute = new CUINullRect(0, 200, null, null);

        Map.Connect(b1, b2);
        Map.Connect(b2, b3, Color.Lime);

        Pages.Open(TickList);



      }

      public CUIShowperf() : base()
      {
        Layout = new CUILayoutVerticalList();

        CreateGUI();

        OnDClick += m =>
        {
          m.ClickConsumed = true;
          this.ApplyState(States["init"]);
        };
      }
      public CUIShowperf(float x, float y, float w, float h) : this()
      {
        Relative = new CUINullRect(x, y, w, h);
      }
    }
  }
}