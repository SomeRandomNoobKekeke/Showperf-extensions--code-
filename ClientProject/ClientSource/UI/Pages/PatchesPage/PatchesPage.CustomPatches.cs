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

    private CUIVerticalList PatchList;

    protected override void HandleOpen() => Refresh();
    public override void Refresh()
    {
      PatchList.Clear();

      foreach (var (id, info) in CustomPatches.Patches)
      {
        PatchList.Add(CreatePatchBlock(info));
      }
    }

    private void TogglePatch(PatchInfoBase info)
    {
      Commands.SendUp(TogglePatch_Command, info);
    }

    private CUIComponent CreatePatchBlock(PatchInfoBase info)
    {
      CUIVerticalList block = new CUIDefault.VerticalBackPanel()
      {
        Margin = new CUISizes(4, 4, 4, 4),
        Borders = new CUISizes(1, 1, 1, 1),
      };

      block["id"] = new CUIToggleButton(info.ID)
      {
        State = PatchInstaller.IsInstalled(info),
        OnToggle = (state) => TogglePatch(info),
      };

      if (!string.IsNullOrEmpty(info.Description))
      {
        block["desctiption"] = new CUIDefault.SolidTextBlock(info.Description)
        {
          AbsoluteMax = new CUINullRect(h: 50),
          ResizeStrategy = ResizeStrategy.Wrap,
          CullChildren = true,
        };
      }




      if (info.Targets.Length != 0)
      {
        if (info.Targets.Length == 1)
        {
          block["targets"] = new CUIDefault.HorizontalBackPanel();
          block["targets"]["caption"] = new CUIDefault.SolidTextBlock($"Target: ");
          block["targets"]["id"] = new CUITextBlock(MethodBaseParser.Serialize(info.Targets[0]));
        }

        if (info.Targets.Length > 1)
        {
          block["targets"] = new CUIDefault.VerticalBackPanel();
          block["targets"]["caption"] = new CUIDefault.SolidTextBlock("Targets:");
          foreach (MethodBase mb in info.Targets)
          {
            block["targets"].Children.Add(new CUITextBlock(MethodBaseParser.Serialize(mb)));
          }
        }
      }

      block.DeepPalette = CUICore.Palettes.Tertiary;

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

      this.DeepPalette = CUICore.Palettes.Tertiary;
    }
  }

}