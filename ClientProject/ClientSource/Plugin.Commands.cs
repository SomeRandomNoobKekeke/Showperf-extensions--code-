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
        if (args[0] == "metadata") CallGraphSaver.Serializer = new MetadataCallGraphSerializer();
        if (args[0] == "humanreadable") CallGraphSaver.Serializer = new FancyCallGraphSerializer();
      }

      string savePath = CallGraphPath;
      if (args.Length >= 2)
      {
        savePath = Path.Combine(ModInfo.Dir, "Data", $"{args[1]}.json");
      }

      CallGraphSaver.SaveTo(savePath, CallGraphBuilder.ExtractFullCallGraph());

      Logger.Default.Print($"CallGraph saved to [{savePath}]", Color.Lime);
    }, () => [["metadata", "humanreadable"], ["CallGraph"]]);
  }

}