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
    public class SelectedPatchComponent : CUIDefault.Panel
    {
      public PatchInstaller PatchInstaller { get; } = Plugin.Get<PatchInstaller>();

      private CustomPatchRepo.Entry _SelectedEntry; public CustomPatchRepo.Entry SelectedEntry
      {
        get => _SelectedEntry;
        set
        {
          _SelectedEntry = value;
          Refresh();
        }
      }

      public bool ShowTargets { get; set; } = true;

      public override void Refresh()
      {
        if (SelectedEntry is null)
        {
          this["selected entry"] = new CUITextBlock("Select Patch")
          {
            Anchor = CUIAnchor.Center,
          };

          return;
        }

        this["selected entry"] = CreateEntryContainer();
      }

      private CUIComponent CreateEntryContainer()
      {
        CUIDefault.VerticalBackPanel container = new CUIDefault.VerticalBackPanel()
        {
          Anchor = CUIAnchor.Center,
          Margin = new CUISizes(16, 16, 16, 16),
          Relative = new CUINullRect(x: 0, y: 0, w: 1),
          Borders = new CUISizes(1, 1, 1, 1),
        };

        container["install"] = new CUIDefault.ToggleButtonField()
        {
          Button =
          {
            AbsoluteMin = new CUINullRect(w:80),
            OnState = { Text = "Remove" },
            OffState = { Text = "Install" },
            State = PatchInstaller.IsInstalled(SelectedEntry.PatchInfo),
            Palette = CUIPalette.FromColor(Color.Cyan),
            DeepPaletteImmunity = true,
            OnToggle = (state) =>
            {
              PatchInstaller.TogglePatch(SelectedEntry.PatchInfo);
              container["install"].Get<CUIToggleButton>("button").State = PatchInstaller.IsInstalled(SelectedEntry.PatchInfo);
            }
          },
          LabelText = SelectedEntry.ID,
        };

        if (!string.IsNullOrEmpty(SelectedEntry.Description))
        {
          container["desctiption"] = new CUIDefault.SolidTextBlock(SelectedEntry.Description)
          {
            AbsoluteMax = new CUINullRect(h: 50),
            ResizeStrategy = ResizeStrategy.Wrap,
            CullChildren = true,
          };
        }



        container.DeepPalette = Plugin.Palettes.Tertiary;

        if (SelectedEntry.SettingsPageFactory != null)
        {
          container["custom page"] = SelectedEntry.SettingsPageFactory();
          container["custom page"].DeepPalette = Plugin.Palettes.Quaternary;
        }

        return container;
      }


      public SelectedPatchComponent()
      {
        this.Background.Sprite = CUISprite.BoxWithALamp;
        FitContent = new CUIBool2(false, true);
        AbsoluteMin = new CUINullRect(h: 60);
        Absolute = new CUINullRect(h: 170);
        RelativeMax = new CUINullRect(h: 0.8f);

        this.Children.Add(new ResizeHandle(0.5f, 1.0f)
        {
          Background = { Sprite = CUISprite.Trapezoid },
          Absolute = new CUINullRect(w: 60, h: 10),
          OnlyVertical = true,
        });
      }
    }
  }
}