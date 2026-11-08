using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using CUILibs;
using System.Diagnostics;
using Barotrauma;

namespace ShowperfExtensions;

public partial class MeasurementsPage : CUIPage
{
  public IntRange SliceCountRange = new IntRange(1, 500);

  private CUIComponent CreateControlsBlock()
  {
    var controls = new CUIDefault.VerticalBackPanel();

    controls["mode"] = new CUIDefault.ButtonGroupField()
    {
      GroupName = "capture window mode",
      Options = ["Average", "Sum", "Spike"],
      SelectedOption = Capture.Window.Mode.ToString(),
      Text = "Mode:",
      OnSelected = (option) =>
      {
        Capture.Window.Mode = Enum.Parse<CaptureWindow.CaptureMode>(option);
      },
    };

    controls["layer 2"] = new CUIDefault.HorizontalBackPanel();
    controls["layer 2"]["freeze"] = new CUIToggleButton("Freeze")
    {
      Background = { Sprite = CUISprite.FrozenVertical },
      State = Capture.Window.Frozen,
      OnToggle = (state) => Capture.Window.Frozen = state,
      Style = (c) => c.MasterColor = Color.Cyan,
    };
    controls["layer 2"]["when to rotate"] = new CUIDefault.ButtonGroupField()
    {
      Flex = 1,
      GroupName = "when to rotate capture window",
      Options = ["Draw", "Update"],
      SelectedOption = CaptureCore.WhenToRotate.ToString(),
      Text = "Rotate on:",
      OnSelected = (option) =>
      {
        CaptureCore.WhenToRotate = Enum.Parse<CaptureCore.WhenToRotateOptions>(option);
      },
    };

    CUITextBlock sliceCount = null;
    controls["slice count"] = new CUIDefault.HorizontalBackPanel();
    controls["slice count"]["label"] = new CUIDefault.SolidTextBlock("Frames:");
    controls["slice count"]["count"] = sliceCount = new CUITextBlock($"{Capture.Window.SliceCount}")
    {
      Solid = false,
      Absolute = new CUINullRect(w: 40),
    };
    controls["slice count"]["slider"] = new CUIRangeInput()
    {
      Flex = 1,
      Lambda = (Capture.Window.SliceCount - SliceCountRange.Start) / (float)SliceCountRange.Length,
      OnHandleDragged = (l) =>
      {
        int slices = ValueLerpFuncs.Int(SliceCountRange.Start, SliceCountRange.End, l);
        sliceCount.Text = $"{slices}";
      },
      OnChanged = (l) =>
      {
        int slices = ValueLerpFuncs.Int(SliceCountRange.Start, SliceCountRange.End, l);
        sliceCount.Text = $"{slices}";
        Capture.Window.SliceCount = slices;
      },
      Margin = new CUISizes(right: 10),
    };


    controls["filters"] = new CUIDefault.HorizontalBackPanel();
    controls["filters"]["include"] = new CUIDefault.TextInputField("include")
    {
      Flex = 1,
      OnInput = (text) => Capture.Window.IncludeFilter = text,
    };
    controls["filters"]["exclude"] = new CUIDefault.TextInputField("exclude")
    {
      Flex = 1,
      OnInput = (text) => Capture.Window.ExcludeFilter = text,
    };

    controls.DeepPalette = Plugin.Palettes.Quaternary;

    return controls;
  }

}