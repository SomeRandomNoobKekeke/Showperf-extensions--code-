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
  public static BindingFlags Pls => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

  public MethodBase target = typeof(Item).GetMethod("Use")!;

  public void Experiment()
  {
    var info1 = new StopWatchWrapper.StopWatchWrapperPatch(
      $"Wrapper around: {MethodBaseParser.Serialize(target)}",
      target
    );

    var info2 = new StopWatchWrapper.StopWatchWrapperPatch(
      $"Wrapper around: {MethodBaseParser.Serialize(target)}",
      target
    );

    Logger.Default.LogVars(info1 == info2);
    Logger.Default.LogVars(info1.ID == info2.ID);
    Logger.Default.LogVars(info1.TargetMethod == info2.TargetMethod);
    Logger.Default.LogVars(info1.Description == info2.Description);
  }
}