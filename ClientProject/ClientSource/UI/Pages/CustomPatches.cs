using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public class CustomPatchesPage : CUIPage
{
  private CUIVerticalList PatchList;

  public Dictionary<string, PatchInfo> Patches { get; } = new();

  public void AddPatch(PatchInfo info) => Patches[info.ID] = info;

  public override void Refresh()
  {
    base.Refresh();

    PatchList.Clear();
    foreach (var (id, info) in Patches)
    {
      PatchList.Add(new CUIToggleButton(id)
      {
        State = Mod.Patcher.IsInstalled(info),
        OnToggle = (state) => Mod.Patcher.TogglePatch(info),
      });
    }
  }


  public CustomPatchesPage()
  {
    this["layout"] = PatchList = new CUIVerticalList() { Relative = new CUINullRect(0, 0, 1, 1) };
  }

}