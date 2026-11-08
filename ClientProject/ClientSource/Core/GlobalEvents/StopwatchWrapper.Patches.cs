using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

public partial class StopwatchWrapper
{
  public FunnyDictOfWeakSets<MethodBase, ITrackStopwatchPatches> StopwatchPatcherListeners { get; } = new();

  private void NotifyStopwatchPatcherListeners(MethodBase mb, bool state)
  {
    foreach (var (method, listeners) in StopwatchPatcherListeners)
    {
      foreach (ITrackStopwatchPatches listener in listeners)
      {
        listener.NotifyStopwatchToggled(mb, state);
      }
    }
  }

  private void ConnectToPatchInstaller()
  {
    PatchInstaller.PatchInstalled += (info) =>
    {
      if (info is StopwatchWrapperPatch swp)
      {
        NotifyStopwatchPatcherListeners(swp.TargetMethod, true);
      }
    };

    PatchInstaller.PatchRemoved += (info) =>
    {
      if (info is StopwatchWrapperPatch swp)
      {
        NotifyStopwatchPatcherListeners(swp.TargetMethod, false);
      }
    };
  }





}