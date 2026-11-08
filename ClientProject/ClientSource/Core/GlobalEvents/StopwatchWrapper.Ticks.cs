using CUILibs;
using HarmonyLib;
using System.Collections.Immutable;
using System.Reflection;

namespace ShowperfExtensions;

// Now i want to track ticks just from stopwatch patches to color MethodLine components
public partial class StopwatchWrapper
{
  public static Dictionary<MethodBase, long> TicksFromStopwatchPatches { get; } = new();
  public FunnyDictOfWeakSets<MethodBase, ITrackStopwatchTicks> TicksListeners { get; } = new();

  private CaptureWindow Window = Plugin.Get<CaptureCore>().Window;

  private void ConnectToCaptureWindow()
  {
    Window.Rotated += () =>
    {
      foreach (var (mb, ticks) in TicksFromStopwatchPatches)
      {
        if (mb is MethodInfo mi && mi.IsVirtual && mi.GetBaseDefinition() != mi)
        {
          if (TicksListeners.TryGetValue(mi.GetBaseDefinition(), out var baseListeners))
          {
            foreach (ITrackStopwatchTicks listener in baseListeners)
            {
              listener.UpdateStopwatchTicks(ticks);
            }
          }
        }


        if (TicksListeners.TryGetValue(mb, out var listeners))
        {
          foreach (ITrackStopwatchTicks listener in listeners)
          {
            listener.UpdateStopwatchTicks(ticks);
          }
        }
      }

      TicksFromStopwatchPatches.Clear();
    };
  }

}