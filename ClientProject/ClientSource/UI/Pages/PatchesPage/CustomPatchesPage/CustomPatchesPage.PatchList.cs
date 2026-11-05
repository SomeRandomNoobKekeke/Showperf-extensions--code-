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

        foreach (var (id, entry) in CustomPatches.Patches)
        {
          List.Add(new CUIButton(id)
          {
            Margin = new CUISizes(4, 4, 4, 4),
            OnClick = () => Commands.SendUp(SelectPatch_Command, entry),
          });
        }
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