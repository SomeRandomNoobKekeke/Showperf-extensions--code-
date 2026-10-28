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
namespace ShowperfExtensions;

public class MethodLineComponent : CUIDefault.HorizontalBackPanel,
  ITrackStopwatchPatches,
  ITrackMethodBookmarks
{
  private Assembly BarotraumaAssembly => typeof(GameMain).Assembly;
  public StopwatchWrapper StopwatchWrapper { get; } = Plugin.Get<StopwatchWrapper>();
  public MethodBookmarksList MethodBookmarksList { get; } = Plugin.Get<MethodBookmarksList>();

  private MethodBase _Method; public MethodBase Method
  {
    get => _Method;
    set
    {
      if (Method == value) return;

      if (_Method != null) StopwatchWrapper.StopwatchPatcherListeners.Remove(_Method, this);
      _Method = value;
      if (_Method != null) StopwatchWrapper.StopwatchPatcherListeners.Add(_Method, this);

      Refresh();
    }
  }

  public Color Color { set => Get<CUIButton>("select method").MasterColor = value; }

  public void NotifyStopwatchToggled(MethodBase mb, bool state)
  {
    if (ToggleStopwatchButton is null) return;
    ToggleStopwatchButton.State = StopwatchWrapper.IsPatched(Method);
  }

  public void NotifyMethodBookmarks()
  {
    if (this["bookmark"] is null) return;
    this.Get<CUIToggleButton>("bookmark").State = MethodBookmarksList.Contains(Method);
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
      State = StopwatchWrapper.IsPatched(Method),
      DeepPaletteImmunity = true,
      OnToggle = (state) =>
      {
        StopwatchWrapper.TogglePatch(Method);
        ToggleStopwatchButton.State = StopwatchWrapper.IsPatched(Method);
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

    this["select method"] = new CUIDefault.GlowingButton(MethodBaseParser.Serialize(Method))
    {
      TextAnchor = CUIAnchor.LeftCenter,
      OnClick = () => SelectMethod(Method),
      MasterColor = new Color(200, 200, 200, 200),
      DeepPaletteImmunity = true,
      Disabled = !CanMethodBeSelected(Method),
      Flex = 1,
    };

    this["bookmark"] = new CUIToggleButton("bookmark")
    {
      State = MethodBookmarksList.Contains(Method),
      OnToggle = (state) => MethodBookmarksList.Toggle(Method),
    };
  }



  public MethodLineComponent(MethodBase method = null)
  {
    Method = method;
    Refresh();
  }
}