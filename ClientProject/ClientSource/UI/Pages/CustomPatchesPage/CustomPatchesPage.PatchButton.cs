using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class CustomPatchesPage : CUIPage
{
  public CUIComponent CreatePatchButton(string id, PatchInfoBase info)
  {
    return new CUIToggleButton(id)
    {
      State = PatchInstaller.IsInstalled(info),
      OnToggle = (state) => PatchInstaller.TogglePatch(info),
    };
  }
}