using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using LightInject;

namespace ShowperfExtensions;

public partial class Plugin
{
  public CallGraphBuilder CallGraphBuilder { get; } = Get<CallGraphBuilder>();
  public CallGraphExplorer CallGraphExplorer { get; } = Get<CallGraphExplorer>();
  public CallGraphProvider CallGraphProvider { get; } = Get<CallGraphProvider>();

  public void AddCommands()
  {
    PluginCommands.Add("showperf1.5", (args) =>
    {
      if (!ShowperfFrame.IsOpen)
      {
        ShowperfFrame.Open();
      }
    });

    PluginCommands.Add("_showperf_buildcallgraph", (args) =>
    {
      if (args.Length >= 1)
      {
        if (args[0] == "metadata") CallGraphProvider.Serializer = new MetadataCallGraphSerializer();
        if (args[0] == "humanreadable") CallGraphProvider.Serializer = new FancyCallGraphSerializer();
      }

      string savePath = Path.Combine(CallGraphProvider.DirWhereCallGraphsAreStored, "call graph.json");
      if (args.Length >= 2)
      {
        savePath = Path.Combine(CallGraphProvider.DirWhereCallGraphsAreStored, $"{args[1]}.json");
      }

      CallGraphProvider.SaveTo(savePath, CallGraphBuilder.ExtractFullCallGraph());

      Logger.Default.Print($"CallGraph saved to [{savePath}]", Color.Lime);
    }, () => [["metadata", "humanreadable"], ["CallGraph"]]);
  }

}