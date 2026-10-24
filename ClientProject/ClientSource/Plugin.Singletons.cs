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
  public class SingletonsContainer
  {
    public PatchInstaller Patcher { get; } = new();
    public CustomPatchRepo CustomPatchRepo { get; } = new();
    public Lazy<UltimateAssemblyAnalyzer3000> Analyzer { get; } = new();
    public CallGraphExplorer CallGraphExplorer { get; } = new();
    public ICallGraphSerializer CallGraphSerializer { get; } = new MetadataCallGraphSerializer();

    public void Dispose()
    {
      Patcher.RemoveAllPatches();
    }
  }

  public static ICallGraphSerializer CallGraphSerializer => Instance?.Singletons.CallGraphSerializer!;
  public static CallGraphExplorer CallGraphExplorer => Instance?.Singletons.CallGraphExplorer!;
  public static UltimateAssemblyAnalyzer3000 Analyzer => Instance?.Singletons.Analyzer.Value!;
  public static PatchInstaller PatchInstaller => Instance?.Singletons.Patcher!;
  public static CustomPatchRepo CustomPatches => Instance?.Singletons.CustomPatchRepo!;

}