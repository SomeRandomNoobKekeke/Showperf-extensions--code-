using CursedUI;
using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class CustomPatchRepo
{
  public class Entry(string id, PatchInfoBase patchInfo)
  {
    public string ID { get; } = id;
    public PatchInfoBase PatchInfo { get; } = patchInfo;

    public string? Description { get; set; }
    public CUIComponent SettingsPage { set => SettingsPageFactory = () => value; }
    public Func<CUIComponent>? SettingsPageFactory { get; set; }
    public Action<Harmony>? InstallMethod { get; set; }
    public Action<Harmony>? RemoveMethod { get; set; }
  }
}