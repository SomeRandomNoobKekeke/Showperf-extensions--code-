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

namespace ShowperfExtensions;

public partial class Plugin
{
  public void AddCommands()
  {
    PluginCommands.Add("_showperf_buildcallgraph", (args) =>
    {
      CallGraphBuilder.ExtractFullCallGraph().SaveTo(CallGraphPath);
      Logger.Default.Print($"CallGraph saved to [{CallGraphPath}]", Color.Lime);
    });
  }

}