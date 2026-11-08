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
  public IntRange FrameCountRange = new IntRange(1, 500);

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

    CUITextBlock frameCount = null;
    controls["frame count"] = new CUIDefault.HorizontalBackPanel();
    controls["frame count"]["label"] = new CUIDefault.SolidTextBlock("Frames:");
    controls["frame count"]["count"] = frameCount = new CUITextBlock($"{Capture.Window.FrameCount}")
    {
      Solid = false,
      Absolute = new CUINullRect(w: 40),
    };
    controls["frame count"]["slider"] = new CUIRangeInput()
    {
      Flex = 1,
      Lambda = (Capture.Window.FrameCount - FrameCountRange.Start) / (float)FrameCountRange.Length,
      OnHandleDragged = (l) =>
      {
        int frames = ValueLerpFuncs.Int(FrameCountRange.Start, FrameCountRange.End, l);
        frameCount.Text = $"{frames}";
      },
      OnChanged = (l) =>
      {
        int frames = ValueLerpFuncs.Int(FrameCountRange.Start, FrameCountRange.End, l);
        frameCount.Text = $"{frames}";
        Capture.Window.FrameCount = frames;
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