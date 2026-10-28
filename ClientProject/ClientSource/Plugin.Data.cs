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
  public TypeExplorer TypeExplorer { get; } = new TypeExplorer(typeof(GameMain).Assembly);

  public CallGraphBuilder CallGraphBuilder { get; private set; }
  public CallGraphSaver CallGraphSaver { get; private set; }
  public CallGraphExplorer CallGraphExplorer { get; private set; }


  public string CallGraphPath => Path.Combine(ModInfo.Dir, "Data", "CallGraph.json");


  private void LoadData()
  {
    Stopwatch sw = new Stopwatch();

    sw.Restart();
    try
    {
      // CallGraphExplorer.CallGraph = CallGraphBuilder.ExtractFullCallGraph(); // takes ~674ms
      CallGraphExplorer.CallGraph = CallGraphSaver.LoadFrom(CallGraphPath); // takes ~32ms
    }
    catch (Exception e)
    {
      Logger.Default.Error(e);
    }
    sw.Stop();

    CUI.Logger.LogVars(sw.ElapsedMilliseconds);
  }
}