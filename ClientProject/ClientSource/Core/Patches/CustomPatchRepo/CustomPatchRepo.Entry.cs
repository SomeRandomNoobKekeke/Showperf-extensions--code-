using CursedUI;
using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class CustomPatchRepo
{
  public class Entry(PatchInfoBase patchInfo)
  {
    public string ID => PatchInfo.ID;
    public PatchInfoBase PatchInfo { get; } = patchInfo;
    public string? Description { get; set; }
    public CUIComponent CustomPage { set => CustomPageFactory = () => value; }
    public Func<CUIComponent>? CustomPageFactory { get; set; }
  }
}