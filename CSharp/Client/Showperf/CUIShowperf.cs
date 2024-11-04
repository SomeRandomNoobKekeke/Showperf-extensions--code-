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
  public partial class Plugin : IAssemblyPlugin
  {
    public class CUIShowperf : CUIFrame
    {
      public CUIVerticalList Header;
      //TODO CategoryLine and SumLine probably should be in TickList
      public CUITextBlock CategoryLine;
      public CUITextBlock SumLine;

      public CUIPages Pages;
      public CUITickList TickList;
      public CUICaptureMap Map;

      public CUIDropDown SubTypeDD;
      public CUIToggleButton ById;
      public CUIMultiButton ModeButton;
      public CUIToggleButton Accumulate;
      public void Clear()
      {
        TickList.Clear();
        CategoryLine.Text = "";
        SumLine.Text = "";
      }
      public void SetCategoryText()
      {
        string s = String.Join(", ",
          Capture.Active.ToList()
          .Select(cs =>
            MapButton.Buttons.GetValueOrDefault(cs)?.FirstOrDefault()?.Text ??
            cs.ToString()
          )
        );
        CategoryLine.Text = s;
      }

      public void Update()
      {
        try
        {
          if (Revealed) TickList.Update();
        }
        catch (Exception e)
        {
          log($"ShowPerf Extensions: {e}");
        }
      }



      public void CreateGUI()
      {
        this["handle"] = new CUIHorizontalList()
        {
          Direction = CUIDirection.Reverse,
          FitContent = new CUIBool2(false, true),
          BorderColor = Color.Transparent,
        };


        this["handle"]["closebutton"] = new CUIButton("X")
        {
          Relative = new CUINullRect(w: 0.1f),
          Font = GUIStyle.MonospacedFont,
          InactiveColor = new Color(32, 0, 0),
          MouseOverColor = new Color(64, 0, 0),
          MousePressedColor = new Color(128, 0, 0),
          AddOnMouseDown = (e) => Close(),
        };

        //TODO mb this should be multibutton, but multibutton is bugged
        this["handle"]["savestate"] = new CUIToggleButton()
        {
          Absolute = new CUINullRect(w: 65),
          OnText = "Locked",
          OffText = "Unlocked",
          AddOnStateChange = (state) =>
          {
            if (state)
            {
              States["old"] = States["init"];
              States["init"] = Clone();
            }
            else
            {
              States["init"] = States["old"];
              States["old"] = null;
            }
          },
        };

        this["buttons1"] = new CUIHorizontalList()
        {
          FitContent = new CUIBool2(false, true),
          HideChildrenOutsideFrame = false,
        };


        this["buttons1"]["ById"] = ById = new CUIToggleButton("By Id")
        {
          FillEmptySpace = new CUIBool2(true, false),
          AddOnStateChange = (state) =>
          {
            Capture.GlobalByID = state;
          },
        };


        this["buttons1"]["mode"] = ModeButton = new CUIMultiButton()
        {
          FillEmptySpace = new CUIBool2(true, false),
          AddOnSelect = (b, i) => Capture.Mode = (CaptureMode)b.Data,
        };

        ModeButton.Add(new CUIButton("Mean") { Data = CaptureMode.Mean });
        ModeButton.Add(new CUIButton("Sum") { Data = CaptureMode.Sum });

        ModeButton.Select(0);


        this["buttons1"]["SubTypeDD"] = SubTypeDD = new CUIDropDown()
        {
          FillEmptySpace = new CUIBool2(true, false),
          AddOnSelect = (v) => Capture.CaptureFrom = Enum.Parse<SubType>(v),
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

        this["header"] = new CUIVerticalList()
        {
          BackgroundColor = Color.Black * 0.75f,
          BorderColor = CUIPallete.Default.Secondary.Border,
          FitContent = new CUIBool2(false, true),
        };

        this["header"].Append(CategoryLine = new CUITextBlock("CategoryLine")
        {
          Ghost = true,
          Font = GUIStyle.MonospacedFont,
          TextScale = 0.75f,
        });
        this["header"].Append(SumLine = new CUITextBlock("SumLine")
        {
          Ghost = true,
          Font = GUIStyle.MonospacedFont,
          TextScale = 0.75f,
        });



        this["pages"] = Pages = new CUIPages()
        {
          FillEmptySpace = new CUIBool2(false, true),
          BackgroundColor = Color.Black * 0.75f,
        };

        TickList = new CUITickList(0, 0, 1, 1);
        LoadMap();


        SetCategoryText();
      }

      public void LoadMap()
      {
        MapButton.Buttons.Clear();
        Capture.Active.Clear();

        Map = CUIComponent.LoadFromFile<CUICaptureMap>(Mod.ModDir + "/XML/CUICaptureMap.xml");
        Pages.Open(Map);
        OnGlobalCaptureStateChange();
      }

      public void OnMapButtonClicked(MapButton b)
      {
        if (b.CState != null)
        {
          b.CState.IsActive = b.State;
        }
      }

      public void OnCaptureStateChange(CaptureState cs)
      {
        Clear();

        if (MapButton.Buttons.ContainsKey(cs))
        {
          foreach (MapButton b in MapButton.Buttons[cs])
          {
            b.SetState(cs.IsActive);
          }
        }

        ById.SetState(cs.ByID);
        SetCategoryText();
      }
      public void OnGlobalCaptureStateChange()
      {
        Clear();
        foreach (CaptureState cs in Capture.Active)
        {
          if (MapButton.Buttons.ContainsKey(cs))
          {
            foreach (MapButton b in MapButton.Buttons[cs])
            {
              b.SetState(cs.IsActive);
            }
          }
        }
        ById.SetState(Capture.GlobalByID);
        SetCategoryText();
      }

      public void OnWindowCaptureFromChanged(SubType s)
      {
        SubTypeDD.Select(s, silent: true);
      }

      public void OnWindowModeChanged(CaptureMode m)
      {
        ModeButton.Select(m, silent: true);
      }


      public CUIShowperf() : base()
      {
        Layout = new CUILayoutVerticalList();


        Capture.OnStateChange += (cs) => OnCaptureStateChange(cs);
        Capture.OnGlobalStateChange += () => OnGlobalCaptureStateChange();
        Capture.OnCaptureFromChanged += (s) => OnWindowCaptureFromChanged(s);
        Capture.OnModeChanged += (m) => OnWindowModeChanged(m);

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