using CursedUI;
using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class CustomPatchRepo
{
  public class EntryInstaller
  {
    public PatchInstaller PatchInstaller { get; } = Plugin.Get<PatchInstaller>();

    public HashSet<Entry> Installed { get; } = [];

    public bool IsInstalled(Entry entry) => Installed.Contains(entry);

    public bool Toggle(Entry entry)
    {
      if (IsInstalled(entry)) Remove(entry); else Install(entry);

      return IsInstalled(entry);
    }

    public void Install(Entry entry)
    {
      if (Installed.Add(entry))
      {
        PatchInstaller.InstallPatch(entry.PatchInfo);
        entry.InstallMethod?.Invoke(PatchInstaller.Harmony);
      }
    }

    public void Remove(Entry entry)
    {
      if (Installed.Remove(entry))
      {
        PatchInstaller.RemovePatch(entry.PatchInfo);
        entry.RemoveMethod?.Invoke(PatchInstaller.Harmony);
      }
    }
  }
}