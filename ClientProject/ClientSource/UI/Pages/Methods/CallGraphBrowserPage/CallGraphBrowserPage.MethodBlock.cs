using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace ShowperfExtensions;

public partial class CallGraphBrowserPage : CUIPage
{
  public class MethodBlock : CUIDefault.VerticalPanel
  {
    //HACK idk where to put it
    private Assembly BarotraumaAssembly => typeof(GameMain).Assembly;

    public string Caption
    {
      get => this.Get<CUITextBlock>("caption").Text;
      set => this.Get<CUITextBlock>("caption").Text = value;
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
          this["methods"].Children.Add(CreateMethodButton(mb));
        }
      }
    }

    private CUIComponent CreateMethodButton(MethodBase mb)
    {
      return new CUIDefault.GlowingButton(MethodBaseParser.Serialize(mb))
      {
        TextAnchor = CUIAnchor.LeftCenter,
        OnClick = () => SelectMethod(mb),
        MasterColor = new Color(200, 200, 200, 200),
        Disabled = !IsMethodAllowed(mb),
        // Margin = new CUISizes(bottom: 4),
      };
    }

    private bool IsMethodAllowed(MethodBase mb) => mb.DeclaringType?.Assembly == BarotraumaAssembly;

    private void SelectMethod(MethodBase mb)
    {
      Commands.SendUp(MethodsPage.SelectMethod_Command, mb);
    }

    public MethodBlock(string caption)
    {
      Margin = new CUISizes(4, 4, 4, 4);

      this["caption"] = new CUIDefault.SolidTextBlock(caption);
      this["methods"] = new CUIDefault.VerticalBackPanel();
    }
  }
}