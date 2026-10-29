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
  private CUIVerticalList ChildNodeList;
  private CUITextBlock Caption;
  private CUITextInput SearchBox;

  private TypeExplorer TypeExplorer = new TypeExplorer(typeof(GameMain).Assembly);

  public TypeExplorer.Node CurrentNode
  {
    get => TypeExplorer.Current;
    set => GoTo(value);
  }

  protected override void HandleOpen() => Refresh();

  public override void Refresh()
  {
    base.Refresh();

    Caption.Text = $"{TypeExplorer.Current.NodeType}: {TypeExplorer.Current}";
    ChildNodeList.Clear();
    ChildNodeList.Scroll = 0;



    IEnumerable<TypeExplorer.Node> nodes = TypeExplorer.Current.GetChildren();



    if (SearchBox.Text.Trim() != "")
    {
      nodes = nodes.Where(node => node.ToString().IndexOf(SearchBox.Text, 0, StringComparison.OrdinalIgnoreCase) != -1);
    }

    nodes = nodes.OrderBy(node => node.NodeType).ThenBy(node => node.ToString());

    Color GetNodeColor(TypeExplorer.Node node) => node switch
    {
      TypeExplorer.AssemblyNode => Color.Gray,
      TypeExplorer.NamespaceNode => Color.Purple.To(Color.Gray, 0.5f),
      TypeExplorer.TypeNode => Color.Blue.To(Color.Gray, 0.5f),
      TypeExplorer.ConstructorNode => Color.Orange.To(Color.Gray, 0.5f),
    };

    CUIComponent CreateNodeButton(TypeExplorer.Node node)
    {
      return node switch
      {
        TypeExplorer.ConstructorNode cn => new MethodLineComponent(cn.Method) { Color = Color.Orange },
        TypeExplorer.MethodNode mn => new MethodLineComponent(mn.Method),
        _ => new CUIDefault.GlowingButton(node.ToString())
        {
          TextAnchor = CUIAnchor.LeftCenter,
          OnMouseDown = (e) => GoTo(node),
          Padding = new CUISizes(1, 1, 1, 1),
          Style = (c) => c.MasterColor = GetNodeColor(node),
        },
      };
    }

    foreach (TypeExplorer.Node node in nodes)
    {
      ChildNodeList.Add(CreateNodeButton(node));
    }
  }


  public void GoUp() => GoTo(TypeExplorer.Current.GetParent());
  public void GoTo(TypeExplorer.Node node)
  {
    if (node == CurrentNode) return;

    if (node is TypeExplorer.MethodNode mn)
    {
      Commands.SendUp(MethodsPage.SelectMethod_Command, mn.Method);
      return;
    }

    TypeExplorer.Current = node;
    SearchBox.Text = "";
    Refresh();
  }

  public void GoToMethod(MethodBase mb)
  {
    GoTo(new TypeExplorer.TypeNode(TypeExplorer, mb.DeclaringType));
  }

  public void GoToUnknown(object sus)
  {
    if (sus is MethodBase mb) GoTo(new TypeExplorer.TypeNode(TypeExplorer, mb.DeclaringType));
    if (sus is Type T) GoTo(new TypeExplorer.TypeNode(TypeExplorer, T));
  }


  public TypeExplorerPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["header"] = new CUIVerticalList() { FitContent = new CUIBool2(false, true), };
    this["layout"]["main"] = new CUIVerticalList() { Flex = 1 };

    this["layout"]["header"]["location panel"] = new CUIDefault.HorizontalPanel();
    this["layout"]["header"]["location panel"]["go up"] = new CUIIconButton()
    {
      Icon = CUISprite.ArrowUpIcon,
      CrossRelative = new CUINullRect(w: 1),
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
      OnInput = (text) => CUICore.Debounce(Refresh),
      OnKeyPressed = (e) =>
      {
        if (e.Key == Keys.Enter) { Refresh(); } // <- old way
      }
    };


    this["layout"]["main"]["nodes"] = new CUIDefault.ListWithScrollbar()
    {
      Flex = 1,
      List = { BottomGap = ResizeHandle.DefaultSize.Y * 2 },
    };

    ChildNodeList = Get<CUIVerticalList>("layout.main.nodes.list");

    DeepPalette = CUICore.Palettes.Tertiary;


  }

}