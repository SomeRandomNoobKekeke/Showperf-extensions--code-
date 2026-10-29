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

    this["caption"] = new CUIDefault.SolidTextBlock(caption);
    this["methods"] = new CUIDefault.VerticalBackPanel();
  }
}