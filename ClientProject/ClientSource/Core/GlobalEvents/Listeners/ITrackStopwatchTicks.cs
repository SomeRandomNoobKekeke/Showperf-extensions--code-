using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

public interface ITrackStopwatchTicks
{
  public void UpdateStopwatchTicks(long ticks);
}