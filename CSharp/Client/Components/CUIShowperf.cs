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
        Append(Header = new CUIVerticalList()
        {
          BackgroundColor = Color.Black * 0.5f,
          FitContent = new CUIBool2(false, true),
        });

        Header.Append(CategoryLine = new CUITextBlock("CategoryLine"));
        Header.Append(SumLine = new CUITextBlock("SumLine"));


        this["buttons1"] = new CUIHorizontalList()
        {
          FitContent = new CUIBool2(false, true),
          HideChildrenOutsideFrame = false,

        };

        this["buttons2"] = new CUIHorizontalList()
        {
          BackgroundColor = Color.Yellow,
          Absolute = new CUINullRect(0, 0, null, 30),
        };
        this["buttons3"] = new CUIHorizontalList()
        {
          FitContent = new CUIBool2(false, true),
          HideChildrenOutsideFrame = false,
        };


        ById = new CUIToggleButton("By Id")
        {
          FillEmptySpace = new CUIBool2(true, false),
        };
        ById.OnStateChange += (state) => Capture.SetByID(CName.MapEntityDrawing, state);
        this["buttons1"].Append(ById);

        CUIMultiButton m = new CUIMultiButton()
        {
          FillEmptySpace = new CUIBool2(true, false),
        };

        m.BackgroundColor = Color.Red;
        m.Add(new CUIButton("Mean")).Data = CaptureWindowMode.Mean;
        m.Add(new CUIButton("Sum")).Data = CaptureWindowMode.Sum;
        // m.Add(new CUIButton("Spike")).Data = CaptureWindowMode.Spike;

        m.OnSelect += (b) => Window.Mode = (CaptureWindowMode)b.Data;
        m.Select(0);

        this["buttons1"].Append(m);




        SubTypeDD = new CUIDropDown()
        {
          AbsoluteMin = new CUINullRect(null, null, null, 40),
        };

        foreach (SubType st in Enum.GetValues(typeof(SubType)))
        {
          SubTypeDD.Add(st);
        }

        SubTypeDD.OnSelect += (v) => CaptureFrom = Enum.Parse<SubType>(v);
        SubTypeDD.Select(SubType.All);
        this["buttons3"].Append(SubTypeDD);




        CUIComponent bb = Append(new CUIButton("Click"));
        bb.OnMouseDown += (CUIMouse m) =>
        {
          if (Pages.IsOpened(TickList)) { Pages.Open(Map); return; }
          if (Pages.IsOpened(Map)) { Pages.Open(TickList); return; }
        };
        Pages = (CUIPages)Append(new CUIPages());
        Pages.FillEmptySpace = new CUIBool2(false, true);




        TickList = new CUITickList();
        TickList.Relative = new CUINullRect(0, 0, 1, 1);

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
          this.ApplyState(State["init"]);
        };
      }
      public CUIShowperf(float x, float y, float w, float h) : this()
      {
        Relative = new CUINullRect(x, y, w, h);
      }
    }
  }
}