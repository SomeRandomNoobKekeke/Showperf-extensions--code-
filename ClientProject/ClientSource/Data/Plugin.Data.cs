using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace ShowperfExtensions;

public partial class Plugin
{
  public CodeExplorer CodeExplorer { get; } = new CodeExplorer(typeof(GameMain).Assembly);

  public CallGraphBuilder CallGraphBuilder { get; } = new();


  public string CallGraphPath => Path.Combine(ModInfo.Dir, "Data", "CallGraph.json");


  private void LoadData()
  {
    Stopwatch sw = new Stopwatch();

    sw.Restart();
    try
    {
      // CallGraphExplorer.CallGraph = CallGraphBuilder.ExtractFullCallGraph();
      CallGraphExplorer.CallGraph = CallGraph.LoadFrom(CallGraphPath);
    }
    catch (Exception e)
    {
      Logger.Default.Error(e);
    }
    sw.Stop();

    CUI.Logger.LogVars(sw.ElapsedMilliseconds);
  }
}