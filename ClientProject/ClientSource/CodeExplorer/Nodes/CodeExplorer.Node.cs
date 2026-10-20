using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class CodeExplorer
{
  public abstract record Node(CodeExplorer explorer)
  {
    public CodeExplorer Explorer { get; } = explorer;

    public abstract Node GetParent();
    public abstract IEnumerable<Node> GetChildren();
    public abstract NodeType NodeType { get; }
  }
}