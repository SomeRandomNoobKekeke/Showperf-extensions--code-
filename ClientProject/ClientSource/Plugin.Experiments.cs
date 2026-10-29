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
    MethodBase mb = MethodBaseParser.Parse("Barotrauma.Item.GetComponent<T>()");

    Logger.Default.LogVars(Logger.Wrap.Props(mb));
  }
}