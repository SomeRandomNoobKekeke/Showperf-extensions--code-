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

public partial class Mod
{
  public static BindingFlags Pls => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;


  public void Experiment()
  {
    Analyzer.PrintInstructions(typeof(GameSession).GetMethod("ShouldIgnorePerksThatCanNotApplyWithoutSubmarine", Pls));
  }
}