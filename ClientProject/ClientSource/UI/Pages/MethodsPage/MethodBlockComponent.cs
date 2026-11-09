using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace ShowperfExtensions;

public class MethodBlockComponent : CUIDefault.VerticalPanel
{
  public string Caption
  {
    get => this.Get<CUITextBlock>("caption").Text;
    set => this.Get<CUITextBlock>("caption").Text = value;
  }

  public Color Color
  {
    get => this["caption"].Background.Color;
    set => this["caption"].Background.Color = value;
  }

  public List<MethodBase> Methods
  {
    set
    {
      this["methods"].Children.Clear();
      Displayed = value.Count != 0;

      //TODO serialized twice, bruh
      foreach (MethodBase mb in value.OrderBy(MethodBaseParser.Serialize))
      {
        this["methods"].Children.Add(new MethodLineComponent(mb));
      }
    }
  }

  public MethodBlockComponent(string caption)
  {
    Margin = new CUISizes(4, 4, 4, 4);

    this["caption line"] = new CUIHorizontalList() { FitContent = new CUIBool2(false, true) };

    this["caption line"]["toggle all"] = new CUIToggleIconButton(CUISprite.BurningTimerIcon)
    {
      Scale = 0.8f,
      Padding = new CUISizes(1, 1, 1, 1),
      OnToggle = (state) =>
      {
        foreach (MethodLineComponent c in this["methods"].Children)
        {
          c.State = state;
        }
      },
    };
    this["caption line"]["caption"] = new CUIDefault.SolidTextBlock(caption) { Flex = 1 };

    this["methods"] = new CUIDefault.VerticalBackPanel();
  }
}