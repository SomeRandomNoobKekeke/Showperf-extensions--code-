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
        Append(Header = new CUIVerticalList()
        {
          BackgroundColor = Color.Black * 0.5f,
          FitContent = new CUIBool2(false, true),
        });

        Header.Append(CategoryLine = new CUITextBlock("CategoryLine"));
        Header.Append(SumLine = new CUITextBlock("SumLine"));




        this["buttons1"] = new CUIHorizontalList() { FitContent = new CUIBool2(false, true) };


        this["buttons1"].Append(ById = new CUIToggleButton("By Id")
        {
          FillEmptySpace = new CUIBool2(true, false)
        });
        ById.OnStateChange += (state) =>
        {
          Capture.SetByID(CName.MapEntityDrawing, state);
          Window.Reset();
        };

        this["buttons1"].Append(Accumulate = new CUIToggleButton("Mean")
        {
          FillEmptySpace = new CUIBool2(true, false)
        });
        Accumulate.OnStateChange += v => Accumulate.Text = v ? "Sum" : "Mean";


        this["buttons1"].Append(SubTypeDD = new CUIDropDown()
        {
          FillEmptySpace = new CUIBool2(true, false)
        });
        foreach (SubType st in Enum.GetValues(typeof(SubType)))
        {
          SubTypeDD.Add(st.ToString());
        }
        SubTypeDD.Select(SubmarineType.Player.ToString());


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

        Map = new CUIMap(0, 0, 1, 1);


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
        HideChildrenOutsideFrame = false;

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