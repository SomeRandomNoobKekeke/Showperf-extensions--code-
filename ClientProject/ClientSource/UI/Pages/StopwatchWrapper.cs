using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace ShowperfExtensions;

public class StopwatchWrapper : CUIPage
{
  public static ICUIStyle DefaultStyle => new CUIDefaultStyle<StopwatchWrapper>((c) =>
  {
    c.Background.Color = Color.Lerp(c.Palette["back"], c.Palette["main"], 0.2f);
  });


  private CUIVerticalList ChildNodeList;
  private CUITextBlock Caption;
  private CUITextInput SearchBox;

  private CodeExplorer CodeExplorer = new CodeExplorer(typeof(GameMain).Assembly);

  public override void Refresh()
  {
    base.Refresh();

    Caption.Text = $"{CodeExplorer.Current.NodeTypeName}: {CodeExplorer.Current}";
    ChildNodeList.Clear();

    IEnumerable<CodeExplorer.Node> nodes = CodeExplorer.Current.GetChildren();

    if (SearchBox.Text.Trim() != "")
    {
      nodes = nodes.Where(node => node.ToString().IndexOf(SearchBox.Text, 0, StringComparison.OrdinalIgnoreCase) != -1);
    }

    nodes = nodes.OrderBy(node => node.GetType()).ThenBy(node => node.ToString());

    foreach (CodeExplorer.Node node in nodes)
    {
      if (node is CodeExplorer.MethodNode methodNode)
      {
        ChildNodeList.Add(new CUIToggleButton(node.ToString())
        {
          TextAnchor = CUIAnchor.LeftCenter,
          Palette = CUICore.Palettes.Quaternary,
        });
      }
      else
      {
        ChildNodeList.Add(new CUIButton(node.ToString())
        {
          TextAnchor = CUIAnchor.LeftCenter,
          Palette = CUICore.Palettes.Tertiary,
          OnMouseDown = (e) => GoTo(node),
        });
      }
    }

  }

  public void TogglePatch(MethodBase mb)
  {

  }

  public void GoUp()
  {
    CodeExplorer.Current = CodeExplorer.Current.GetParent();
    Refresh();
  }
  public void GoTo(CodeExplorer.Node node)
  {
    CodeExplorer.Current = node;
    Refresh();
  }

  public StopwatchWrapper()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["header"] = new CUIVerticalList()
    {
      FitContent = new CUIBool2(false, true),
    };

    this["layout"]["header"]["location panel"] = new CUIDefault.HorizontalPanel()
    {
    };
    this["layout"]["header"]["location panel"]["go up"] = new CUIIconButton()
    {
      Icon = new CUISprite(CUISprite.AtIconIndex(null, 4, 2)) { Effects = SpriteEffects.FlipVertically },
      OnMouseDown = (e) => GoUp(),
    };

    this["layout"]["header"]["location panel"]["caption"] = Caption = new CUITextBlock()
    {
      Flex = 1,
      TextAnchor = CUIAnchor.LeftCenter,
      Padding = new CUISizes(4, 4, 4, 4),
    };

    this["layout"]["header"]["search panel"] = new CUIDefault.HorizontalPanel()
    {

    };
    this["layout"]["header"]["search panel"]["search label"] = new CUITextBlock("Search:");
    this["layout"]["header"]["search panel"]["search"] = SearchBox = new CUITextInput()
    {
      Flex = 1,
      OnKeyPressed = (e) =>
      {
        if (e.Key == Keys.Enter) { Refresh(); }
      }
    };

    this["layout"]["main"] = new CUIVerticalList() { Flex = 1 };
    this["layout"]["main"]["caption"] = new CUIDefault.TextPanel("Nodes:")
    {
      TextAnchor = CUIAnchor.LeftCenter,
    };
    this["layout"]["main"]["nodes"] = new CUIDefault.ListWithScrollbar() { Flex = 1 };

    ChildNodeList = Get<CUIVerticalList>("layout.main.nodes.list");

    DeepPalette = CUICore.Palettes.Tertiary;


  }

}