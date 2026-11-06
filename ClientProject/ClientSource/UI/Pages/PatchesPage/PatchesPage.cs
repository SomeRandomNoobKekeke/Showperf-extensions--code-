using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class PatchesPage : CUIPage
{
  private CUIPages Main;
  private CUIHorizontalList PageButtons;

  public PatchInstaller PatchInstaller { get; } = Plugin.Get<PatchInstaller>();


  private CustomPatchesPage CustomPatchesBlock = Plugin.Get<CustomPatchesPage>();
  private InstalledPatchesPage InstalledPatchesBlock = Plugin.Get<InstalledPatchesPage>();

  public void OpenEntry(CustomPatchRepo.Entry entry)
  {
    PageButtons.Get<CUIRadioButton>("Custom Patches").Select();
    CustomPatchesBlock.OpenEntry(entry);
  }


  protected override void HandleOpen() => Refresh();

  public override void Refresh()
  {
    CustomPatchesBlock.Refresh();
    InstalledPatchesBlock.Refresh();
  }

  public PatchesPage()
  {
    this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
    this["layout"]["header"] = new CUIDefault.VerticalPanel();
    this["layout"]["main"] = Main = new CUIPages() { Flex = 1 };

    this["layout"]["header"]["page buttons"] = PageButtons = new CUIDefault.HorizontalPanel();

    using (new CUIContextStyle<CUIRadioButton>(btn =>
    {
      btn.Flex = 1;
      btn.GroupName = "CustomPatchesPage subpages";
    }))
    {
      this["layout"]["header"]["page buttons"]["Custom Patches"] = new CUIRadioButton("Custom Patches")
      {
        OnSelected = () => Main.OpenPage(CustomPatchesBlock),
      };

      this["layout"]["header"]["page buttons"]["Installed Patches"] = new CUIRadioButton("Installed Patches")
      {
        OnSelected = () => Main.OpenPage(InstalledPatchesBlock),
      };
    }
    this["layout"]["header"]["page buttons"].DeepPalette = Plugin.Palettes.Secondary;

    PageButtons.Get<CUIRadioButton>("Custom Patches").Select();
  }

}