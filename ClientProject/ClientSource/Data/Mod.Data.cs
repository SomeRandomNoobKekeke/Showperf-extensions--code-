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
  public CallGraphExplorer CallGraphExplorer { get; } = new();
  public CallGraphBuilder CallGraphBuilder { get; } = new();



  public string CallGraphPath => Path.Combine(ModInfo.Dir, "Data", "CallGraph.json");

  private void LoadData()
  {
    Stopwatch sw = new Stopwatch();
    CallGraph info = null;

    try
    {
      sw.Restart();
      info = CallGraphBuilder.ExtractFullCallGraph();
    }
    catch (Exception e)
    {
      CUI.Logger.Error(e);
    }
    finally
    {
      sw.Stop();
    }



    CUI.Logger.LogVars(sw.ElapsedMilliseconds);

    CallGraphExplorer.CallGraph = info;
    info.SaveTo(CallGraphPath);

  }
}