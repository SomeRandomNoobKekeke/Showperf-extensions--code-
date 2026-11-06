using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;


/// <summary>
/// Class for registering your patches in Showperf  
/// 1 AddAllPatchesInThisAssembly() in Mod.Init should be enough
/// </summary>
public static class CustomPatches
{
  public static void AddAllPatchesInThisAssembly()
    => AddAllPatchesInAssembly(Assembly.GetCallingAssembly());

  public static void AddAllPatchesInAssembly(Assembly assembly) { }

  public static void Add<T>() => Add(typeof(T));
  public static void Add(Type showperfPatchType) { }
}
