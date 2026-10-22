using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using Barotrauma;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace ShowperfExtensions;

public class FlowBrowserPage : CUIPage
{
  public FlowExplorer FlowExplorer { get; } = new();

  public MethodBase Current
  {
    get => FlowExplorer.Current;
    set
    {
      FlowExplorer.Current = value;
      Refresh();
    }
  }

  public override void Refresh()
  {
    base.Refresh();


  }


  public FlowBrowserPage()
  {

  }
}