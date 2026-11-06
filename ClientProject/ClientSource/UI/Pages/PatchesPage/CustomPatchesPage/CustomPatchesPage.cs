using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class PatchesPage : CUIPage
{
  public partial class CustomPatchesPage : CUIPage
  {
    public const string SelectPatch_Command = "SelectPatch";
    public CustomPatchRepo CustomPatches { get; } = Plugin.Get<CustomPatchRepo>();


    private SelectedPatchComponent SelectedPatchBlock;
    private PatchListComponent PatchList;

    protected override void HandleOpen() => Refresh();
    public override void Refresh()
    {
      SelectedPatchBlock.Refresh();
      PatchList.Refresh();
    }

    public void OpenEntry(CustomPatchRepo.Entry entry)
    {
      SelectedPatchBlock.SelectedEntry = entry;
    }


    public CustomPatchesPage()
    {

      this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };

      this["layout"]["current patch"] = SelectedPatchBlock = new SelectedPatchComponent();
      this["layout"]["patch list"] = PatchList = new PatchListComponent() { Flex = 1 };
      this.DeepPalette = Plugin.Palettes.Tertiary;

      Commands.ListenFor<CustomPatchRepo.Entry>(
         SelectPatch_Command,
         (e) => SelectedPatchBlock.SelectedEntry = e
       );
    }
  }

}