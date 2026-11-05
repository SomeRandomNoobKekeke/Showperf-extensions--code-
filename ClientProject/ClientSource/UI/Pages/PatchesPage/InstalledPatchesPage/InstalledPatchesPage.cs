using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class PatchesPage : CUIPage
{
  public class InstalledPatchesPage : CUIPage
  {
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



    private bool refreshRequested;
    private void RequestRefresh()
    {
      if (refreshRequested) return;
      refreshRequested = true;
      CUI.Main.GlobalEvents.MouseUp.Add(HandleGlobalMouseUp);
    }
    private void HandleGlobalMouseUp(CUIMouseUpEvent e)
    {
      CUI.Main.GlobalEvents.MouseUp.Remove(HandleGlobalMouseUp);
      Refresh();
      refreshRequested = false;
    }

    private CUIComponent CreatePatchBlock(PatchInfoBase info)
    {
      CUIHorizontalList block = new CUIDefault.HorizontalBackPanel();

      CUIIconButton btn = new CUIIconButton(CUISprite.CrossIcon)
      {
        Scale = 0.8f,
        CrossRelative = new CUINullRect(w: 1),
        OnMouseDown = (e) => PatchInstaller.RemovePatch(info),
        OnMouseUp = (e) => Refresh(),
        OnMouseLeave = (e) =>
        {
          if (e.Mouse.M1.Pressed)
          {
            block.DeepTransparency = 0.5f;
            RequestRefresh();
          }
        },
        OnMouseEnter = (e) =>
        {
          if (e.Mouse.M1.Pressed)
          {
            block.DeepTransparency = 0.5f;
            PatchInstaller.RemovePatch(info);
            RequestRefresh();
          }
        },
      };


      block["remove button"] = btn;

      if (info is StopwatchWrapper.StopwatchWrapperPatch)
      {
        block["icon"] = new CUIIconBlock(CUISprite.BurningTimerIcon)
        {
          Scale = 0.8f,
          Padding = new CUISizes(1, 1, 1, 1),
          Borders = new CUISizes(left: 2),
        };
      }

      block["id"] = new CUITextBlock(info.ID);

      block.DeepPalette = Plugin.Palettes.Tertiary;

      return block;
    }

    public InstalledPatchesPage()
    {
      // PatchInstaller.PatchInstalled += (info) => Refresh();
      // PatchInstaller.PatchRemoved += (info) => Refresh();

      this["layout"] = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
      this["layout"]["list"] = PatchList = new CUIVerticalList()
      {
        Flex = 1,
        Padding = new CUISizes(8, 8, 8, 8),
      };

      this.DeepPalette = Plugin.Palettes.Tertiary;
    }
  }

}