using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Diagnostics;
using Barotrauma;

namespace ShowperfExtensions;

public partial class PatchesPage : CUIPage
{
  public partial class CustomPatchesPage : CUIPage
  {
    public class PatchListComponent : CUIVerticalList
    {
      public enum SortingByEnum { Name, Target }

      public CustomPatchRepo CustomPatches { get; } = Plugin.Get<CustomPatchRepo>();
      private CUIVerticalList List;

      private SortingByEnum _SortingBy; public SortingByEnum SortingBy
      {
        get => _SortingBy;
        set
        {
          _SortingBy = value;
          Refresh();
        }
      }

      public override void Refresh()
      {
        List.Clear();

        switch (SortingBy)
        {
          case SortingByEnum.Name:
            foreach (var (id, entry) in CustomPatches.Patches.OrderBy(kvp => kvp.Key))
            {
              List.Add(CreateEntryBlockByName(id, entry));
            }
            break;

          case SortingByEnum.Target:
            foreach (var (target, entries) in CustomPatches.PatchesByMethod)
            {
              List.Add(CreateEntryBlockByTarget(target, entries));
            }
            break;
        }

        List.DeepPalette = Plugin.Palettes.Tertiary;
      }

      private CUIComponent CreateEntryBlockByName(string id, CustomPatchRepo.Entry entry)
      {
        CUIButtonContainer block = new()
        {
          Margin = new CUISizes(4, 4, 4, 4),
          OnClick = () => Commands.SendUp(SelectPatch_Command, entry),
          Borders = new CUISizes(1, 1, 1, 1),
          FitContent = new CUIBool2(false, true)
        };

        block["layout"] = new CUIVerticalList()
        {
          Relative = new CUINullRect(0, 0, 1, 1),
          FitContent = new CUIBool2(false, true),
        };

        block["layout"]["id"] = new CUITextBlock(id);

        if (entry.PatchInfo.Targets.Length != 0)
        {
          if (entry.PatchInfo.Targets.Length == 1)
          {
            block["layout"]["targets"] = new CUIDefault.HorizontalBackPanel();
            block["layout"]["targets"]["open in call graph"] = new CUIIconButton(CUISprite.ShareIcon)
            {
              OnClick = () => Commands.SendUp(ShowperfFrame.OpenMethodInCallGraph_Command, entry.PatchInfo.Targets[0]),
            };
            block["layout"]["targets"]["caption"] = new CUIDefault.SolidTextBlock($"Target: ");
            block["layout"]["targets"]["id"] = new CUITextBlock(MethodBaseParser.Serialize(entry.PatchInfo.Targets[0]));
          }

          if (entry.PatchInfo.Targets.Length > 1)
          {
            block["layout"]["targets"] = new CUIDefault.VerticalBackPanel();
            block["layout"]["targets"]["caption"] = new CUIDefault.SolidTextBlock("Targets:");
            foreach (MethodBase mb in entry.PatchInfo.Targets)
            {
              block["layout"]["targets"].Children.Add(new CUIDefault.IconButtonField()
              {
                Text = MethodBaseParser.Serialize(mb),
                Button = { Icon = CUISprite.ShareIcon },
                OnClick = () => Commands.SendUp(ShowperfFrame.OpenMethodInCallGraph_Command, mb),
              });
            }
          }
        }

        return block;
      }

      private CUIComponent CreateEntryBlockByTarget(MethodBase target, HashSet<CustomPatchRepo.Entry> entries)
      {
        CUIDefault.VerticalBackPanel block = new()
        {
          Margin = new CUISizes(4, 4, 4, 4),
          Borders = new CUISizes(1, 1, 1, 1),
        };

        var targetLine = block["target line"] = new CUIDefault.HorizontalBackPanel()
        {
          DeepPaletteImmunity = true,
        };

        targetLine["open in call graph"] = new CUIIconButton(CUISprite.ShareIcon)
        {
          OnClick = () => Commands.SendUp(ShowperfFrame.OpenMethodInCallGraph_Command, target),
          Palette = Plugin.Palettes.Secondary2,
        };

        targetLine["target"] = new CUIDefault.SolidTextBlock(MethodBaseParser.Serialize(target))
        {
          Flex = 1,
          Palette = Plugin.Palettes.Secondary,
        };

        block["entries"] = new CUIDefault.VerticalBackPanel()
        {
          Padding = new CUISizes(4, 4, 4, 4),
        };
        foreach (var entry in entries)
        {
          block["entries"].Children.Add(new CUIButton(entry.ID)
          {
            OnClick = () => Commands.SendUp(SelectPatch_Command, entry),
            Palette = Plugin.Palettes.Tertiary3,
            DeepPaletteImmunity = true,
            TextAnchor = CUIAnchor.LeftCenter,
            // Margin = new CUISizes(4, 4, 4, 4),
          });
        }

        return block;
      }

      public PatchListComponent()
      {
        this["controls"] = new CUIDefault.VerticalBackPanel();

        this["controls"]["sort by"] = new CUIDefault.ButtonGroup()
        {
          GroupName = "patch list sorting by",
          Options = ["Sort By Name", "Sort By Target"],
          SelectedOption = "Sort By Name",
          OnSelected = (name) =>
          {
            SortingBy = name switch
            {
              "Sort By Name" => SortingByEnum.Name,
              "Sort By Target" => SortingByEnum.Target,
            };
          },
          DeepPaletteImmunity = true,
          DeepPalette = Plugin.Palettes.Tertiary2,
          Borders = new CUISizes(1, 1, 1, 1),
        };



        this["content"] = new CUIDefault.ListWithScrollbar()
        {
          Flex = 1,
          List = { Padding = new CUISizes(4, 4, 4, 4) }
        };
        List = this["content"].Get<CUIVerticalList>("list");
      }
    }
  }
}