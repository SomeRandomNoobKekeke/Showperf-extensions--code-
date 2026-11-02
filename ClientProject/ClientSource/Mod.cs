using Barotrauma;
using ShowperfExtensions;
using CUILibs;
using System.Reflection;


namespace ShowperfPatches;

public partial class Mod
{
  public void Init()
  {
    CustomPatches.AddAllPatchesInThisAssembly();
  }

  public void Dispose()
  {

  }

  public void OnContentLoaded()
  {

  }
}