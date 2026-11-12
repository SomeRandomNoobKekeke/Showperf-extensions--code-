using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using CUILibs;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;

namespace ShowperfPatches;

[ShowperfPatch("SteamTimelineManager.Update")]
public static partial class SteamTimelineManager_Update
{
  [PatchMethod("SteamTimelineManager.Update", "1.15.30.0")]
  public static bool SteamTimelineManager_Update_Replace(float deltaTime)
  {
    DisposableStopwatch sw = new DisposableStopwatch();

    using (sw.Restart())
    {
      SteamTimelineManager.PollScreenChange();
    }
    sw.Capture("PollScreenChange");

    using (sw.Restart())
    {
      SteamTimelineManager.PollCharacterChange(deltaTime);
    }
    sw.Capture("PollCharacterChange");

    using (sw.Restart())
    {
      SteamTimelineManager.PollSubmarineChange(deltaTime);
    }
    sw.Capture("PollSubmarineChange");

    return false;
  }
}