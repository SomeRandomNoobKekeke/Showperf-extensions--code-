using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;


public static class CustomPatches
{
  public static void AddAllPatchesInThisAssembly()
    => AddAllPatchesInAssembly(Assembly.GetCallingAssembly());

  public static void AddAllPatchesInAssembly(Assembly assembly) { }

  public static void Add<T>() => Add(typeof(T));
  public static void Add(Type showperfPatchType) { }
}
