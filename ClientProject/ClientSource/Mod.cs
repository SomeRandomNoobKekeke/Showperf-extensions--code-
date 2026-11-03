using Barotrauma;
using ShowperfExtensions;
using CUILibs;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ShowperfPatches;

public partial class Mod
{
  [MethodImpl(MethodImplOptions.NoInlining)]
  public void Init()
  {
    CustomPatches.AddAllPatchesInThisAssembly();
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