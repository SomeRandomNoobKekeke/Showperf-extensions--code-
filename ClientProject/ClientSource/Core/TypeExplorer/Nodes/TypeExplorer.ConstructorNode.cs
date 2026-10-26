using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class TypeExplorer
{
  public record ConstructorNode(TypeExplorer explorer, MethodBase method) : MethodNode(explorer, method)
  {
    public override NodeType NodeType => NodeType.Constructor;
    public override string ToString() => MethodBaseParser.Serialize(Method);
  }
}