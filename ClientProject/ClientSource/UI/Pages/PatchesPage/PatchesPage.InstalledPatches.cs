using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class PatchesPage : CUIPage
{
  public class InstalledPatchesPage : CUIPage
  {
    public CustomPatchRepo CustomPatches { get; } = Plugin.Get<CustomPatchRepo>();
    public PatchInstaller PatchInstaller { get; } = Plugin.Get<PatchInstaller>();

    private CUIVerticalList PatchList;

    protected override void HandleOpen() => Refresh();
    public override void Refresh()
    {
      PatchList.Clear();

      foreach (PatchInfoBase info in PatchInstaller.InstalledPatches)
      {
        PatchList.Add(CreatePatchBlock(info));
      }
    }

    private CUIComponent CreatePatchBlock(PatchInfoBase info)
    {
      CUIHorizontalList block = new CUIDefault.HorizontalBackPanel()
      {
        Margin = new CUISizes(4, 4, 4, 4),
      };
      block["remove button"] = new CUIIconButton(CUISprite.CrossIcon)
      {
        Scale = 0.8f,
        CrossRelative = new CUINullRect(w: 1),
        OnClick = () => PatchInstaller.RemovePatch(info),
      };
      block["id"] = new CUITextBlock(info.ID);

      block.DeepPalette = CUICore.Palettes.Tertiary;

      return block;
    }

    public InstalledPatchesPage()
    {
      PatchInstaller.PatchInstalled += (info) => Refresh();
      PatchInstaller.PatchRemoved += (info) => Refresh();

      this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
      this["layout"]["list"] = PatchList = new CUIVerticalList()
      {
        Flex = 1,
        Padding = new CUISizes(4, 4, 4, 4),
      };

      this.DeepPalette = CUICore.Palettes.Tertiary;
    }
  }

}