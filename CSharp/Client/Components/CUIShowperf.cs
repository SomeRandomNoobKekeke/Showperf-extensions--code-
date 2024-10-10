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
      public CUIMultiButton ModeButton;
      public CUIToggleButton Accumulate;

      public bool ShouldCapture(Entity e)
      {
        return CaptureFrom == SubType.All || (int)e.Submarine.Info.Type == (int)CaptureFrom;
      }

      public void Update()
      {
        if (Revealed && Capture.Active.Count != 0)
        {
          Window.Update();
          TickList.Update();
        }
      }

      public void CreateGUI()
      {
        this["handle"] = new CUIComponent()
        {
          Absolute = new CUINullRect(h: 20),
          BorderColor = Color.Transparent,
        };

        this["handle"]["closebutton"] = new CUIButton("X")
        {
          Anchor = new CUIAnchor(CUIAnchorType.RightCenter),
          InactiveColor = new Color(32, 0, 0),
          MouseOverColor = new Color(64, 0, 0),
          MousePressedColor = new Color(128, 0, 0),
          AddOnMouseDown = (e) => Close(),
        };

        this["header"] = new CUIVerticalList()
        {
          BackgroundColor = Color.Black * 0.5f,
          BorderColor = CUIPallete.Default.Secondary.Border,
          FitContent = new CUIBool2(false, true),
        };

        this["header"].Append(CategoryLine = new CUITextBlock("CategoryLine"));
        this["header"].Append(SumLine = new CUITextBlock("SumLine"));


        this["buttons1"] = new CUIHorizontalList()
        {
          FitContent = new CUIBool2(false, true),
          HideChildrenOutsideFrame = false,
        };


        this["buttons1"]["ById"] = ById = new CUIToggleButton("By Id")
        {
          FillEmptySpace = new CUIBool2(true, false),
          AddOnStateChange = (state) => Capture.ById = state,
        };


        this["buttons1"]["mode"] = ModeButton = new CUIMultiButton()
        {
          FillEmptySpace = new CUIBool2(true, false),
          AddOnSelect = (b, i) => Window.Mode = (CaptureWindowMode)b.Data,
        };

        ModeButton.Add(new CUIButton("Mean") { Data = CaptureWindowMode.Mean });
        ModeButton.Add(new CUIButton("Sum") { Data = CaptureWindowMode.Sum });

        ModeButton.Select(0);


        this["buttons1"]["SubTypeDD"] = SubTypeDD = new CUIDropDown()
        {
          FillEmptySpace = new CUIBool2(true, false),
          AddOnSelect = (v) => CaptureFrom = Enum.Parse<SubType>(v),
        };

        foreach (SubType st in Enum.GetValues(typeof(SubType)))
        {
          SubTypeDD.Add(st);
        }

        SubTypeDD.Select(SubType.All);

        this["CaptureButton"] = new CUIButton("Capture")
        {
          AddOnMouseDown = (e) =>
          {
            if (Pages.IsOpened(TickList)) { Pages.Open(Map); return; }
            if (Pages.IsOpened(Map)) { Pages.Open(TickList); return; }
          },
        };


        this["pages"] = Pages = new CUIPages()
        {
          FillEmptySpace = new CUIBool2(false, true),
        };

        TickList = new CUITickList(0, 0, 1, 1);

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