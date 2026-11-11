using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public partial class PatchesPage : CUIPage
{
  public class InstalledPatchesPage : CUIPage
  {
    public CustomPatchRepo.EntryInstaller EntryInstaller { get; } = Plugin.Get<CustomPatchRepo.EntryInstaller>();
    public StopwatchWrapper StopwatchWrapper { get; } = Plugin.Get<StopwatchWrapper>();

    private CUIVerticalList PatchList;

    protected override void HandleOpen() => Refresh();
    public override void Refresh()
    {
      PatchList.Clear();

      foreach (CustomPatchRepo.Entry entry in EntryInstaller.Installed)
      {
        PatchList.Add(CreateCustomPatchBlock(entry));
      }

      foreach (MethodBase patchedMethod in StopwatchWrapper.Installed)
      {
        PatchList.Add(CreateStopwatchPatchBlock(patchedMethod));
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

    //BRUH
    private CUIComponent CreateStopwatchPatchBlock(MethodBase method)
    {
      CUIHorizontalList block = new CUIDefault.HorizontalBackPanel();

      CUIIconButton btn = new CUIIconButton(CUISprite.CrossIcon)
      {
        Scale = 0.8f,
        Padding = new CUISizes(1, 1, 1, 1),
        CrossRelative = new CUINullRect(w: 1),
        OnMouseDown = (e) => StopwatchWrapper.Remove(method),
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
            StopwatchWrapper.Remove(method);
            RequestRefresh();
          }
        },
      };

      block["remove button"] = btn;

      block["icon"] = new CUIIconBlock(CUISprite.BurningTimerIcon)
      {
        Scale = 0.8f,
        Padding = new CUISizes(1, 1, 1, 1),
        Borders = new CUISizes(left: 2),
      };

      block["id"] = new CUITextBlock(MethodBaseParser.Serialize(method));

      block.DeepPalette = Plugin.Palettes.Tertiary;

      return block;
    }

    //BRUH x2
    private CUIComponent CreateCustomPatchBlock(CustomPatchRepo.Entry entry)
    {
      CUIHorizontalList block = new CUIDefault.HorizontalBackPanel();

      CUIIconButton btn = new CUIIconButton(CUISprite.CrossIcon)
      {
        Scale = 0.8f,
        Padding = new CUISizes(1, 1, 1, 1),
        CrossRelative = new CUINullRect(w: 1),
        OnMouseDown = (e) => EntryInstaller.Remove(entry),
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
            EntryInstaller.Remove(entry);
            RequestRefresh();
          }
        },
      };


      block["remove button"] = btn;

      block["id"] = new CUITextBlock(entry.ID);

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
        Scrollable = true,
      };

      this.DeepPalette = Plugin.Palettes.Tertiary;
    }
  }

}