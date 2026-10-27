using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using System.Runtime.CompilerServices;

namespace ShowperfExtensions;

public class MethodLineComponent : CUIDefault.HorizontalBackPanel
{
  public static ConditionalWeakTable<MethodBase, MethodLineComponent> Components { get; } = new();


  private Assembly BarotraumaAssembly => typeof(GameMain).Assembly;
  public StopwatchWrapper StopwatchWrapper => Plugin.StopwatchWrapper;

  private MethodBase _Method; public MethodBase Method
  {
    get => _Method;
    set
    {
      if (Method == value) return;
      _Method = value;

      Refresh();
    }
  }

  public void NotifyMethodPatched(MethodBase mb)
  {
    if (Method != mb) return;
    ToggleStopwatchButton.State = StopwatchWrapper.IsPatched(Method);
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
      OnToggle = (state) =>
      {
        StopwatchWrapper.TogglePatch(Method);
        ToggleStopwatchButton.State = StopwatchWrapper.IsPatched(Method);
      },
      Scale = 0.8f,
      Padding = new CUISizes(1, 1, 1, 1),
      Background = { Sprite = CUISprite.VerticalGlowSolid },
      Border = {
        Sprite = CUISprite.VerticalGlowSolid,
        Sizes = new CUISizes( right: 1),
      },
      Style = (c) =>
      {
        c.OnColor = Color.Cyan.MultOpaque(1.0f);
        c.OffColorHovered = new Color(200, 200, 200, 200).MultOpaque(0.9f);
        c.OffColor = new Color(200, 200, 200, 200).MultOpaque(0.8f);
        c.Border.Color = new Color(200, 200, 200, 200).MultOpaque(0.8f);
        c.DetermineColor();
      },
    };

    this["select method"] = new CUIDefault.GlowingButton(MethodBaseParser.Serialize(Method))
    {
      TextAnchor = CUIAnchor.LeftCenter,
      OnClick = () => SelectMethod(Method),
      MasterColor = new Color(200, 200, 200, 200),
      PaletteImmunity = true,
      Disabled = !CanMethodBeSelected(Method),
      Flex = 1,
    };
  }


  public MethodLineComponent(MethodBase method = null)
  {
    Method = method;
    Refresh();
  }
}