using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public static class CustomPatches
{
  public static void Add(PatchInfoBase info)
  {
    Plugin.CustomPatchRepo.Add(info);
  }
}