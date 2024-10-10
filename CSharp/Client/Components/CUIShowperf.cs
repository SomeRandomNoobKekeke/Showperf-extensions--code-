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
      public class MapButton : CUIToggleButton
      {
        public static Dictionary<CaptureState, MapButton> Buttons = new Dictionary<CaptureState, MapButton>();
        public CaptureState CState;
        public MapButton(int x, int y, string name, CaptureState cs) : base(name)
        {
          Absolute = new CUINullRect(x: x, y: y);
          CState = cs;
          State = cs.IsActive;
          OnStateChange += (state) =>
          {
            CState.IsActive = state;
            Showperf.SetCategoryText();
          };
          Buttons[cs] = this;
        }
      }

      //TODO mb this should be in Window like all the other props
      private SubType captureFrom; public SubType CaptureFrom
      {
        get => captureFrom;
        set { captureFrom = value; Window.Reset(); }
      }

      public CUIVerticalList Header;
      //TODO CategoryLine and SumLine probably should be in TickList
      public CUITextBlock CategoryLine;
      public CUITextBlock SumLine;

      public CUIPages Pages;
      public CUITickList TickList;
      public CUIMap Map;

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
        string s = String.Join(", ", Capture.Active.ToList().Select(cs => MapButton.Buttons[cs].Text));
        CategoryLine.Text = s;
      }

      public bool ShouldCapture(Entity e)
      {
        return CaptureFrom == SubType.All || (int)e.Submarine.Info.Type == (int)CaptureFrom;
      }

      public void Update()
      {
        if (Revealed)
        {
          Window.Update();
          TickList.Update();
        }
      }

      public void CreateGUI()
      {
        this["handle"] = new CUIComponent()
        {
          Absolute = new CUINullRect(h: 18),
          BorderColor = Color.Transparent,
        };

        this["handle"]["closebutton"] = new CUIButton("X")
        {
          Anchor = new CUIAnchor(CUIAnchorType.RightCenter),
          Absolute = new CUINullRect(w: 15, h: 18),
          Padding = new Vector2(0, 0),
          Font = GUIStyle.MonospacedFont,
          InactiveColor = new Color(32, 0, 0),
          MouseOverColor = new Color(64, 0, 0),
          MousePressedColor = new Color(128, 0, 0),
          AddOnMouseDown = (e) => Close(),
        };



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
        Map = new CUIMap(0, 0, 1, 1)
        {
          BackgroundColor = Color.Transparent,
          BorderColor = Color.Transparent,
          AddOnDClick = (e) => Map.ChildrenOffset = Vector2.Zero,
        };

        FillMap();

        Pages.Open(Map);
      }

      public void FillMap()
      {
        Map.Add(new MapButton(0, 0, "bebe", Capture.MapEntityDrawing));

        // Map.Connect(0, 1);
        // Map.Connect(1, 2, Color.Lime);
      }

      public CUIShowperf() : base()
      {
        Layout = new CUILayoutVerticalList();

        CreateGUI();
        SetCategoryText();

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