using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class CodeExplorer
{
  public Assembly TargetAssembly { get; set; }
  public Node Current { get; set; }

  public bool FilterGeneratedStuff { get; set; } = true;

  public CodeExplorer(Assembly targetAssembly)
  {
    TargetAssembly = targetAssembly;
    Current = new AssemblyNode(this);
  }
}