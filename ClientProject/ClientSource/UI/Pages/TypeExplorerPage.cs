using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace ShowperfExtensions;

public class TypeExplorerPage : CUIPage
{
  public static ICUIStyle DefaultStyle => new CUIDefaultStyle<TypeExplorerPage>((c) =>
  {
    c.Background.Color = Color.Lerp(c.Palette["back"], c.Palette["main"], 0.2f);
  });


  private CUIVerticalList ChildNodeList;
  private CUITextBlock Caption;
  private CUITextInput SearchBox;

  private CodeExplorer CodeExplorer = new CodeExplorer(typeof(GameMain).Assembly);

  public CodeExplorer.Node CurrentNode
  {
    get => CodeExplorer.Current;
    set => GoTo(value);
  }

  protected override void HandleOpen() => Refresh();

  public override void Refresh()
  {
    base.Refresh();

    Caption.Text = $"{CodeExplorer.Current.NodeType}: {CodeExplorer.Current}";
    ChildNodeList.Clear();
    ChildNodeList.Scroll = 0;



    IEnumerable<CodeExplorer.Node> nodes = CodeExplorer.Current.GetChildren();



    if (SearchBox.Text.Trim() != "")
    {
      nodes = nodes.Where(node => node.ToString().IndexOf(SearchBox.Text, 0, StringComparison.OrdinalIgnoreCase) != -1);
    }

    nodes = nodes.OrderBy(node => node.NodeType).ThenBy(node => node.ToString());

    Color GetNodeColor(CodeExplorer.Node node) => node switch
    {
      CodeExplorer.AssemblyNode => Color.Gray,
      CodeExplorer.NamespaceNode => Color.Gray,
      CodeExplorer.TypeNode => Color.Blue.To(Color.Gray, 0.5f),
      CodeExplorer.ConstructorNode => Color.Orange.To(Color.Gray, 0.5f),
      CodeExplorer.MethodNode => Color.Lime.To(Color.Gray, 0.5f),
    };

    CUIComponent CreateNodeButton(CodeExplorer.Node node)
    {
      return new CUIDefault.GlowingButton(node.ToString())
      {
        TextAnchor = CUIAnchor.LeftCenter,
        OnMouseDown = (e) => GoTo(node),
        Style = (c) => c.MasterColor = GetNodeColor(node),
      };
    }

    foreach (CodeExplorer.Node node in nodes)
    {
      ChildNodeList.Add(CreateNodeButton(node));
    }
  }


  public void GoUp() => GoTo(CodeExplorer.Current.GetParent());
  public void GoTo(CodeExplorer.Node node)
  {
    if (node == CurrentNode) return;

    if (node is CodeExplorer.MethodNode mn)
    {
      Commands.SendUp(MethodsPage.SelectMethod_Command, mn.Method);
      return;
    }

    CodeExplorer.Current = node;
    SearchBox.Text = "";
    Refresh();
  }

  public void GoToMethod(MethodBase mb)
  {
    GoTo(new CodeExplorer.TypeNode(CodeExplorer, mb.DeclaringType));
  }

  public void GoToUnknown(object sus)
  {
    if (sus is MethodBase mb) GoTo(new CodeExplorer.TypeNode(CodeExplorer, mb.DeclaringType));
    if (sus is Type T) GoTo(new CodeExplorer.TypeNode(CodeExplorer, T));
  }

  public TypeExplorerPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["header"] = new CUIVerticalList()
    {
      FitContent = new CUIBool2(false, true),
    };

    this["layout"]["main"] = new CUIVerticalList() { Flex = 1 };

    this["layout"]["header"]["location panel"] = new CUIDefault.HorizontalPanel();
    this["layout"]["header"]["location panel"]["go up"] = new CUIIconButton()
    {
      Icon = CUISprite.ArrowUpIcon,
      OnMouseDown = (e) => GoUp(),
    };

    this["layout"]["header"]["location panel"]["caption"] = Caption = new CUITextBlock()
    {
      Flex = 1,
      TextAnchor = CUIAnchor.LeftCenter,
      Padding = new CUISizes(4, 4, 4, 4),
    };

    this["layout"]["header"]["search panel"] = new CUIDefault.HorizontalPanel();
    this["layout"]["header"]["search panel"]["search label"] = new CUIButton("Search:")
    {
      OnMouseDown = (e) => { Refresh(); },
    };
    this["layout"]["header"]["search panel"]["search"] = SearchBox = new CUITextInput()
    {
      Flex = 1,
      OnKeyPressed = (e) =>
      {
        if (e.Key == Keys.Enter) { Refresh(); }
      }
    };


    this["layout"]["main"]["nodes"] = new CUIDefault.ListWithScrollbar() { Flex = 1 };

    ChildNodeList = Get<CUIVerticalList>("layout.main.nodes.list");

    DeepPalette = CUICore.Palettes.Tertiary;


  }

}