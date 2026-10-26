using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class TypeExplorer
{
  public abstract record Node(TypeExplorer explorer)
  {
    public TypeExplorer Explorer { get; } = explorer;

    public abstract Node GetParent();
    public abstract IEnumerable<Node> GetChildren();
    public abstract NodeType NodeType { get; }
  }
}