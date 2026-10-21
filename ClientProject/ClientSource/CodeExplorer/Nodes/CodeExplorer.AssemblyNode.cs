using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class CodeExplorer
{
  public record AssemblyNode(CodeExplorer explorer) : Node(explorer)
  {
    public override Node GetParent() => this;
    public override IEnumerable<Node> GetChildren()
      => Explorer.TargetAssembly.GetTypes()
                 .Select(t => t.Namespace)
                 .Distinct()
                 .Select(name => new NamespaceNode(Explorer, name));
    public override NodeType NodeType => NodeType.Assembly;
    public override string ToString() => Explorer.TargetAssembly.GetName().Name;
  }
}