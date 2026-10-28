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
    public PatchInstaller PatchInstaller { get; private set; }
    public CustomPatchRepo CustomPatchRepo { get; private set; } = new();
    public Lazy<UltimateAssemblyAnalyzer3000> Analyzer { get; private set; } = new();
    public CallGraphExplorer CallGraphExplorer { get; private set; } = new();
    public ICallGraphSerializer CallGraphSerializer { get; private set; } = new MetadataCallGraphSerializer();
    public StopwatchWrapper StopwatchWrapper { get; private set; }
    public CaptureCore CaptureCore { get; private set; } = new();
    public Settings Settings { get; private set; } = new();

    public ShowperfFrame? ShowperfFrame { get; private set; }

    public void Init()
    {
      CaptureCore.AttachToGame();
    }

    public void Dispose()
    {
      CaptureCore.DetachFromGame();
      PatchInstaller.RemoveAllPatches();
    }


    public void InitStep1()
    {
      PatchInstaller = new();
    }

    public void InitStep2()
    {
      StopwatchWrapper = new();
    }

    public void InitStep3()
    {
      ShowperfFrame = new();
    }
  }

  public static ShowperfFrame ShowperfFrame => Instance?.Singletons.ShowperfFrame!;
  public static Settings Settings => Instance?.Singletons.Settings!;
  public static ICallGraphSerializer CallGraphSerializer => Instance?.Singletons.CallGraphSerializer!;
  public static CallGraphExplorer CallGraphExplorer => Instance?.Singletons.CallGraphExplorer!;
  public static UltimateAssemblyAnalyzer3000 Analyzer => Instance?.Singletons.Analyzer.Value!;
  public static PatchInstaller PatchInstaller => Instance?.Singletons.PatchInstaller!;
  public static CustomPatchRepo CustomPatches => Instance?.Singletons.CustomPatchRepo!;
  public static StopwatchWrapper StopwatchWrapper => Instance?.Singletons.StopwatchWrapper!;
  public static CaptureCore? CaptureCore => Instance?.Singletons.CaptureCore;
}