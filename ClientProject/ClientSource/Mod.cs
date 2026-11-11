using Barotrauma;
using ShowperfExtensions;
using CUILibs;
using System.Reflection;
using System.Runtime.CompilerServices;
using CursedUI;

namespace ShowperfPatches;

public partial class Mod
{
  [MethodImpl(MethodImplOptions.NoInlining)]
  public void Init()
  {
    CustomPatches.AddAllPatchesInThisAssembly();
    Experiment();
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  public void Dispose()
  {

  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  public void OnContentLoaded()
  {

  }
}