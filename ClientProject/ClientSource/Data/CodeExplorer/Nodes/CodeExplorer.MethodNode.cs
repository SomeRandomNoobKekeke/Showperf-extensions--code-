using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class CodeExplorer
{
  public record MethodNode(CodeExplorer explorer, MethodBase method) : Node(explorer)
  {
    public MethodBase Method { get; } = method;

    public override Node GetParent()
    {
      return new TypeNode(Explorer, Method.DeclaringType);
    }
    public override IEnumerable<Node> GetChildren() => [];
    public override NodeType NodeType => NodeType.Method;
    public override string ToString() => MethodBaseParser.Serialize(Method);
  }
}