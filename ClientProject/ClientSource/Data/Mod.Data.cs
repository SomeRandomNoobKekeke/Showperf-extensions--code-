using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace ShowperfExtensions;

public partial class Mod
{
  public CodeExplorer CodeExplorer { get; } = new CodeExplorer(typeof(GameMain).Assembly);
  public FlowExplorer FlowExplorer { get; } = new();
  public FlowInfoBuilder FlowInfoBuilder { get; } = new();

  private UltimateAssemblyAnalyzer3000 _Analyzer;
  public static UltimateAssemblyAnalyzer3000 Analyzer => Instance._Analyzer ??= new();

  public string FlowInfoPath => Path.Combine(ModInfo.Dir, "Data", "FlowInfo.json");

  private void LoadData()
  {
    Stopwatch sw = new Stopwatch();


    FlowInfo info = FlowInfo.LoadFrom(FlowInfoPath);

    sw.Restart();
    info.Merge(FlowInfoBuilder.GetFlowInfo(typeof(GameSession)));
    sw.Stop();
    Logger.LogVars(sw.ElapsedMilliseconds);

    FlowExplorer.FlowInfo = info;
    info.SaveTo(FlowInfoPath);

  }
}