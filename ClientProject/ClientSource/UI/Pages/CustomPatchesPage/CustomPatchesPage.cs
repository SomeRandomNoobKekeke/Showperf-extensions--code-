using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class CustomPatchesPage : CUIPage
{
  private CUIVerticalList PatchList;

  public CustomPatchRepo CustomPatches => Plugin.CustomPatches;
  public static PatchInstaller PatchInstaller => Plugin.PatchInstaller;


  protected override void HandleOpen() => Refresh();

  public override void Refresh()
  {
    base.Refresh();

    PatchList.Clear();

    foreach (var (id, info) in CustomPatches.Patches.OrderBy(kvp => kvp.Key))
    {
      PatchList.Add(CreatePatchButton(id, info));
    }
  }


  public CustomPatchesPage()
  {
    this["layout"] = PatchList = new CUIVerticalList()
    {
      Relative = new CUINullRect(0, 0, 1, 1),
    };
  }

}