using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class CustomPatchesPage : CUIPage
{
  public class InstalledPatchesComponent : CUIDefault.VerticalBackPanel
  {
    public CustomPatchRepo CustomPatches => Plugin.CustomPatches;
    public PatchInstaller PatchInstaller => Plugin.PatchInstaller;

    private CUIVerticalList PatchList;

    public override void Refresh()
    {
      PatchList.Clear();

      foreach (PatchInfo info in PatchInstaller.InstalledPatches)
      {
        PatchList.Add(CreatePatchBlock(info));
      }
    }

    private CUIComponent CreatePatchBlock(PatchInfo info)
    {
      CUIHorizontalList block = new CUIDefault.HorizontalBackPanel()
      {
        Margin = new CUISizes(4, 4, 4, 4),
      };
      block["remove"] = new CUIButton("Remove")
      {
        OnClick = () => Commands.SendUp(TogglePatch_Command, info),
      };
      block["id"] = new CUITextBlock(info.ID);

      block.DeepPalette = CUICore.Palettes.Tertiary;

      return block;
    }

    public InstalledPatchesComponent()
    {
      this["caption"] = new CUIDefault.SolidTextBlock("Installed Patches")
      {
        TextAnchor = CUIAnchor.Center,
      };
      this["list"] = PatchList = new CUIDefault.VerticalBackPanel()
      {
        Flex = 1,
        Padding = new CUISizes(4, 4, 4, 4),
      };

      this.DeepPalette = CUICore.Palettes.Secondary;
    }
  }

}