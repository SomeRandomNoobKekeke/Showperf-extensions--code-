using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class CodeExplorer
{
  public record ConstructorNode(CodeExplorer explorer, MethodBase method) : MethodNode(explorer, method)
  {
    public override NodeType NodeType => NodeType.Constructor;
    public override string ToString() => MethodBaseParser.Serialize(Method);
  }
}