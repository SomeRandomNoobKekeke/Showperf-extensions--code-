using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class CustomPatchesPage : CUIPage
{

  public const string TogglePatch_Command = "TogglePatch";

  private CUIVerticalList PatchList;

  public CustomPatchRepo CustomPatches => Plugin.CustomPatches;
  public PatchInstaller PatchInstaller => Plugin.PatchInstaller;

  private CustomPatchesComponent CustomPatchesBlock;
  private InstalledPatchesComponent InstalledPatchesBlock;

  protected override void HandleOpen() => Refresh();

  public override void Refresh()
  {
    CustomPatchesBlock.Refresh();
    InstalledPatchesBlock.Refresh();
  }

  public void TogglePatch(PatchInfoBase info)
  {
    PatchInstaller.TogglePatch(info);
    Refresh();
  }


  public CustomPatchesPage()
  {
    this["layout"] = new CUIHorizontalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["custom patches"] = CustomPatchesBlock = new CustomPatchesComponent()
    {
      Flex = 1,
    };
    this["layout"]["installed patches"] = InstalledPatchesBlock = new InstalledPatchesComponent()
    {
      Flex = 1,
    };

    Commands.ListenFor<PatchInfoBase>(TogglePatch_Command, TogglePatch);
  }

}