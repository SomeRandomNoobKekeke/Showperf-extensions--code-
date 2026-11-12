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
using System;
using System.Runtime.CompilerServices;
using Barotrauma.Steam;
using Steamworks;
using Steamworks.Data;



namespace ShowperfPatches;

[ShowperfPatch("SteamTimelineManager.PollScreenChange")]
public static partial class SteamTimelineManager_PollScreenChange
{


  [PatchMethod("SteamTimelineManager.PollScreenChange", "1.15.30.0")]
  public static bool SteamTimelineManager_PollScreenChange_Replace()
  {
    DisposableStopwatch sw = new();


    TimeSpan pausesBefore = GC.GetTotalPauseDuration();

    bool condition;
    using (sw.Restart())
    {
      condition = !SteamManager.IsInitialized;
    }

    TimeSpan pausesAfter = GC.GetTotalPauseDuration();

    sw.Capture($"PollScreenChange | !SteamManager.IsInitialized");
    Capture.Ticks($"PollScreenChange | GC pause: ", (pausesAfter - pausesBefore).Ticks);

    if (condition) { return false; }




    if (Screen.Selected == SteamTimelineManager.prevScreen) { return false; }

    SteamTimelineManager.TimelineGameMode newMode = Screen.Selected switch
    {
      GameScreen _ => SteamTimelineManager.TimelineGameMode.Playing,
      NetLobbyScreen _ => SteamTimelineManager.TimelineGameMode.Staging,
      EditorScreen _ => SteamTimelineManager.TimelineGameMode.Playing,
      MainMenuScreen _ => SteamTimelineManager.TimelineGameMode.Menus,
      _ => SteamTimelineManager.TimelineGameMode.LoadingScreen // Default to Menus for other screens for now
    };

    if (GameMain.Instance != null && GameMain.Instance.LoadingScreenOpen)
    {
      newMode = SteamTimelineManager.TimelineGameMode.LoadingScreen;
    }

    if (newMode == SteamTimelineManager.gameMode) { return false; }

    SteamTimelineManager.SetTimelineGameMode(newMode);
    SteamTimelineManager.gameMode = newMode;

    DebugConsole.NewMessage($"Timeline game mode set to {newMode}");

    SteamTimelineManager.prevScreen = Screen.Selected;

    return false;
  }
}