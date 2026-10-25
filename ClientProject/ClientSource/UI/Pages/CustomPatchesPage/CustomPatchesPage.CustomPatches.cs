using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class CustomPatchesPage : CUIPage
{
  public class CustomPatchesComponent : CUIDefault.VerticalBackPanel
  {
    public CustomPatchRepo CustomPatches => Plugin.CustomPatches;
    public PatchInstaller PatchInstaller => Plugin.PatchInstaller;

    private CUIVerticalList PatchList;

    public override void Refresh()
    {
      PatchList.Clear();

      foreach (var (id, info) in CustomPatches.Patches)
      {
        PatchList.Add(CreatePatchBlock(info));
      }
    }

    private void TogglePatch(PatchInfo info)
    {
      Commands.SendUp(TogglePatch_Command, info);
    }

    private CUIComponent CreatePatchBlock(PatchInfo info)
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
          // AbsoluteMax = new CUINullRect(h: 50),
          ResizeStrategy = ResizeStrategy.Wrap,
        };
      }


      //TODO use grid component
      block["table"] = new CUIDefault.HorizontalBackPanel();
      block["table"]["keys"] = new CUIDefault.VerticalBackPanel()
      {
        FitContent = new CUIBool2(true, true)
      };
      block["table"]["values"] = new CUIDefault.VerticalBackPanel()
      {
        Flex = 1,
        FitContent = new CUIBool2(false, true)
      };


      block["table"]["keys"]["target"] = new CUIDefault.SolidTextBlock("Target: ");
      block["table"]["keys"]["source"] = new CUIDefault.SolidTextBlock("Source: ");
      block["table"]["keys"]["type"] = new CUIDefault.SolidTextBlock("Type: ");

      block["table"]["values"]["target"] = new CUITextBlock(MethodBaseParser.Serialize(info.TargetMethod));
      block["table"]["values"]["source"] = new CUITextBlock(MethodBaseParser.Serialize(info.PatchMethod));
      block["table"]["values"]["type"] = new CUITextBlock($"{info.PatchType}");

      block.DeepPalette = CUICore.Palettes.Tertiary;

      return block;
    }


    public CustomPatchesComponent()
    {
      this["caption"] = new CUIDefault.SolidTextBlock("Custom Patches")
      {
        TextAnchor = CUIAnchor.Center,
      };
      this["list"] = PatchList = new CUIDefault.VerticalBackPanel() { Flex = 1 };

      this.DeepPalette = CUICore.Palettes.Secondary;
    }
  }

}