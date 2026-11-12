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
using static Barotrauma.SteamTimelineManager;


namespace ShowperfPatches;

[ShowperfPatch("SteamTimelineManager.PollSubmarineChange")]
public static partial class SteamTimelineManager_PollSubmarineChange
{
  [PatchMethod("SteamTimelineManager.PollSubmarineChange", "1.15.30.0")]
  public static bool SteamTimelineManager_PollSubmarineChange_Replace(float deltaTime)
  {
    DisposableStopwatch sw = new();


    bool condition;
    using (sw.Restart())
    {
      condition = !SteamManager.IsInitialized;
    }
    sw.Stop().Capture($"PollSubmarineChange | !SteamManager.IsInitialized");
    if (condition) { return false; }



    if (trackedCharacter == null) { return false; }
    if (Screen.Selected is not GameScreen) { return false; }

    Submarine? trackedCharacterSubmarine = trackedCharacter.Submarine;

    // timer makes sure only time-stable state changes are registered
    if (submarineStateChangeTimer > 0f)
    {
      submarineStateChangeTimer -= deltaTime;

      if (submarineStateChangeTimer <= 0f)
      {
        // actually register our pending state change
        CharacterSubChanged(trackedCharacter, trackedCharacterSubmarine);
      }
    }

    // detect instantaneous submarine change and start the delay timer
    if (previousTrackedSubmarine != trackedCharacterSubmarine)
    {
      submarineStateChangeTimer = SubmarineStateChangeDelay;
    }
    previousTrackedSubmarine = trackedCharacterSubmarine;

    return false;
  }
}