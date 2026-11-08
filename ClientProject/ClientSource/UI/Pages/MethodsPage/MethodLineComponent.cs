using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using System.Runtime.CompilerServices;
using LightInject;
using CUILibs;
namespace ShowperfExtensions;

public class MethodLineComponent : CUIDefault.HorizontalBackPanel,
  ITrackStopwatchPatches,
  ITrackMethodBookmarks,
  ITrackStopwatchTicks
{
  private enum IsTogglingState { No, TogglingOn, TogglingOff }

  static MethodLineComponent()
  {
    CUI.Main.GlobalEvents.MouseUp.Add((e) =>
    {
      IsToggling = IsTogglingState.No;
    });
  }

  private static IsTogglingState IsToggling;

  private PluginSettings Settings = Plugin.Get<PluginSettings>();
  private CaptureWindow CaptureWindow = Plugin.Get<CaptureCore>().Window;
  private Assembly BarotraumaAssembly => typeof(GameMain).Assembly;
  public StopwatchWrapper StopwatchWrapper { get; } = Plugin.Get<StopwatchWrapper>();
  public MethodBookmarksList MethodBookmarksList { get; } = Plugin.Get<MethodBookmarksList>();
  public CustomPatchRepo CustomPatches { get; } = Plugin.Get<CustomPatchRepo>();



  private MethodBase _Method; public MethodBase Method
  {
    get => _Method;
    set
    {
      if (Method == value) return;

      if (_Method != null)
      {
        StopwatchWrapper.StopwatchPatcherListeners.Remove(_Method, this);
        StopwatchWrapper.TicksListeners.Remove(_Method, this);
        MethodBookmarksList.MethodBookmarksListeners.Remove(_Method, this);
      }
      _Method = value;
      if (_Method != null)
      {
        StopwatchWrapper.StopwatchPatcherListeners.Add(_Method, this);
        StopwatchWrapper.TicksListeners.Add(_Method, this);
        MethodBookmarksList.MethodBookmarksListeners.Add(_Method, this);
      }

      Refresh();
    }
  }

  public Color Color { set => Get<CUIButton>("select method").MasterColor = value; }

  public void NotifyStopwatchToggled(MethodBase mb, bool state)
  {
    if (ToggleStopwatchButton is null) return;
    ToggleStopwatchButton.State = StopwatchWrapper.IsPatched(Method);
    Get<CUIButton>("select method")?.ReapplyStyles();
  }

  public void NotifyMethodBookmarksChanged()
  {
    if (this["bookmark"] is null) return;
    this.Get<CUIToggleIconButton>("bookmark").State = MethodBookmarksList.Contains(Method);
  }

  public void UpdateStopwatchTicks(long ticks)
  {
    Color = Settings.Gradient((float)(ticks / CaptureWindow.TotalSlice.MaxValue));
  }



  private CUIToggleIconButton ToggleStopwatchButton;

  private bool CanMethodBeSelected(MethodBase mb) => mb.DeclaringType?.Assembly == BarotraumaAssembly;


  private void SelectMethod(MethodBase mb)
  {
    Commands.SendUp(MethodsPage.SelectMethod_Command, mb);
  }

  public override void Refresh()
  {
    this.Clear();

    if (Method is null)
    {
      this["no method text"] = new CUITextBlock("No Method");
      return;
    }

    this["toggle stopwatch"] = ToggleStopwatchButton = new CUIToggleIconButton(CUISprite.BurningTimerIcon)
    {
      IsAllowedToToggle = () => IsToggling == IsTogglingState.No,
      OnMouseEnter = (e) =>
      {
        if (IsToggling == IsTogglingState.TogglingOn) ToggleStopwatchButton.ReactiveState = true;
        if (IsToggling == IsTogglingState.TogglingOff) ToggleStopwatchButton.ReactiveState = false;
      },
      State = StopwatchWrapper.IsPatched(Method),
      OnToggle = (state) =>
      {
        StopwatchWrapper.TogglePatch(Method);
        ToggleStopwatchButton.State = StopwatchWrapper.IsPatched(Method);

        if (IsToggling == IsTogglingState.No)
        {
          IsToggling = state ? IsTogglingState.TogglingOn : IsTogglingState.TogglingOff;
        }
      },
      Scale = 0.8f,
      Padding = new CUISizes(1, 1, 1, 1),
      Background = { Sprite = CUISprite.GlowingEdgesSolid },
      Style = (c) =>
      {
        c.MasterColor = Color.White;
        c.OnColor = Color.Lime;
      },
    };

    if (CustomPatches.PatchesByMethod.ContainsKey(Method))
    {
      this["go to custom patch"] = new CUIIconButton(CUISprite.WrenchIcon)
      {
        CrossRelative = new CUINullRect(w: 1),
        Background = { Sprite = CUISprite.GlowingEdgesSolid },
        MasterColor = new Color(255, 255, 0),
        Style = (c) =>
        {
          c.Background.Color = new Color(200, 200, 200, 200);
        },
        Scale = 0.8f,
        Padding = new CUISizes(1, 1, 1, 1),
        OnClick = () =>
        {
          Commands.SendUp(ShowperfFrame.OpenCustomPatch_Command, CustomPatches.PatchesByMethod[Method].First());
        }
      };
    }

    this["select method"] = new CUIDefault.GlowingButton(MethodBaseParser.Serialize(Method))
    {
      TextAnchor = CUIAnchor.LeftCenter,
      OnClick = () => SelectMethod(Method),
      Style = (btn) => btn.MasterColor = new Color(200, 200, 200, 200),
      Disabled = !CanMethodBeSelected(Method),
      Flex = 1,
    };

    this["bookmark"] = new CUIToggleIconButton(CUISprite.StarOnIcon, CUISprite.StarOffIcon)
    {
      State = MethodBookmarksList.Contains(Method),
      OnToggle = (state) => MethodBookmarksList.Toggle(Method),
      Scale = 0.8f,
      Padding = new CUISizes(1, 1, 1, 1),
      Background = { Sprite = CUISprite.GlowingEdgesSolid },
      MasterColor = Color.White,
      ClickSound = GUISoundType.TickBox,
    };
  }



  public MethodLineComponent(MethodBase method = null)
  {
    Method = method;
    DeepPaletteImmunity = true;

    Refresh();
  }
}