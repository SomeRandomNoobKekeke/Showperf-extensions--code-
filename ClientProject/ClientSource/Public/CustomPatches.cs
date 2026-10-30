using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public static class CustomPatches
{
  public static void Add(PatchInfoBase info, string desctiption = "", Func<CUIComponent> customPageFactory = null)
  {
    Plugin.CustomPatchRepo.Add(
      new CustomPatchRepo.Entry(info)
      {
        Description = desctiption,
        CustomPageFactory = customPageFactory,
      }
    );
  }
}