using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class PatchesPage : CUIPage
{
  public class CustomPatchesPage : CUIPage
  {
    public CustomPatchRepo CustomPatches { get; } = Plugin.Get<CustomPatchRepo>();
    public PatchInstaller PatchInstaller { get; } = Plugin.Get<PatchInstaller>();

    public bool ShowTargets { get; set; } = true;

    private CUIVerticalList PatchList;

    protected override void HandleOpen() => Refresh();
    public override void Refresh()
    {
      PatchList.Clear();

      foreach (var (id, entry) in CustomPatches.Patches)
      {
        PatchList.Add(CreatePatchBlock(entry));
      }
    }


    private CUIComponent CreatePatchBlock(CustomPatchRepo.Entry entry)
    {
      CUIVerticalList block = new CUIDefault.VerticalBackPanel()
      {
        Margin = new CUISizes(4, 4, 4, 4),
        Borders = new CUISizes(1, 1, 1, 1),
      };

      block["id"] = new CUIToggleButton(entry.ID)
      {
        State = PatchInstaller.IsInstalled(entry.PatchInfo),
        OnToggle = (state) =>
        {
          PatchInstaller.TogglePatch(entry.PatchInfo);
          block.Get<CUIToggleButton>("id").State = PatchInstaller.IsInstalled(entry.PatchInfo);
        },
      };

      if (!string.IsNullOrEmpty(entry.Description))
      {
        block["desctiption"] = new CUIDefault.SolidTextBlock(entry.Description)
        {
          AbsoluteMax = new CUINullRect(h: 50),
          ResizeStrategy = ResizeStrategy.Wrap,
          CullChildren = true,
        };
      }

      if (ShowTargets && entry.PatchInfo.Targets.Length != 0)
      {
        if (entry.PatchInfo.Targets.Length == 1)
        {
          block["targets"] = new CUIDefault.HorizontalBackPanel();
          block["targets"]["caption"] = new CUIDefault.SolidTextBlock($"Target: ");
          block["targets"]["id"] = new CUITextBlock(MethodBaseParser.Serialize(entry.PatchInfo.Targets[0]));
        }

        if (entry.PatchInfo.Targets.Length > 1)
        {
          block["targets"] = new CUIDefault.VerticalBackPanel();
          block["targets"]["caption"] = new CUIDefault.SolidTextBlock("Targets:");
          foreach (MethodBase mb in entry.PatchInfo.Targets)
          {
            block["targets"].Children.Add(new CUITextBlock(MethodBaseParser.Serialize(mb)));
          }
        }
      }

      block.DeepPalette = Plugin.Palettes.Tertiary;

      if (entry.SettingsPageFactory != null)
      {
        block["custom page"] = entry.SettingsPageFactory();
        block["custom page"].DeepPalette = Plugin.Palettes.Quaternary;
      }

      return block;
    }


    public CustomPatchesPage()
    {
      this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };

      this["layout"]["scroll wrapper"] = new CUIDefault.ListWithScrollbar()
      {
        Flex = 1,
        List = { Padding = new CUISizes(4, 4, 4, 4) },
      };

      PatchList = this.Get<CUIVerticalList>("layout.scroll wrapper.list");

      this.DeepPalette = Plugin.Palettes.Tertiary;
    }
  }

}