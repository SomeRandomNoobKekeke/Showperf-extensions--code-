using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class CustomPatchesPage : CUIPage
{

  public const string TogglePatch_Command = "TogglePatch";


  public CustomPatchRepo CustomPatches { get; } = Plugin.Get<CustomPatchRepo>();
  public PatchInstaller PatchInstaller { get; } = Plugin.Get<PatchInstaller>();


  private CustomPatchesBlockComponent CustomPatchesBlock;
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
    this["layout"]["custom patches"] = CustomPatchesBlock = new CustomPatchesBlockComponent()
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