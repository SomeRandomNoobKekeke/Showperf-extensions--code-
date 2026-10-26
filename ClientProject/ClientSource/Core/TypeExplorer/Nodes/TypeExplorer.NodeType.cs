using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class TypeExplorer
{
  public enum NodeType
  {
    Assembly, Namespace, Type, Method, Constructor,
  }
}