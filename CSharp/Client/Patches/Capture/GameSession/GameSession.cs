using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Barotrauma.IO;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Barotrauma.PerkBehaviors;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class GameSessionPatch
    {
      public static CaptureState UpdateGameSession;
      public static CaptureState UpdateEvents;
      public static CaptureState UpdateGameMode;
      public static CaptureState UpdateMissions;
      public static CaptureState UpdateGUI;

      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(GameSession).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(GameSessionPatch).GetMethod("GameSession_Update_Replace"))
        );

        UpdateGameSession = Capture.Get("Showperf.Update.GameSession");
        UpdateEvents = Capture.Get("Showperf.Update.GameSession.Events");
        UpdateGameMode = Capture.Get("Showperf.Update.GameSession.GameMode");
        UpdateMissions = Capture.Get("Showperf.Update.GameSession.Missions");
        UpdateGUI = Capture.Get("Showperf.Update.GameSession.GUI");
      }

      public static bool GameSession_Update_Replace(float deltaTime, GameSession __instance)
      {
        if (!Showperf.Revealed) return true;


        Capture.Update.EnsureCategory(UpdateGameSession);

        GameSession _ = __instance;
        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

        _.RoundDuration += deltaTime;

        sw.Restart();
        _.EventManager?.Update(deltaTime);
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateGameSession, "Events");

        sw.Restart();
        _.GameMode?.Update(deltaTime);
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateGameSession, "GameMode");
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateGameMode, _.GameMode.ToString());

        sw.Restart();
        //backwards for loop because the missions may get completed and removed from the list in Update()
        Capture.Update.EnsureCategory(UpdateMissions);
        for (int i = _.missions.Count - 1; i >= 0; i--)
        {
          sw2.Restart();
          _.missions[i].Update(deltaTime);
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, UpdateMissions, _.missions[i].ToString());
        }

        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateGameSession, "Missions");

        sw.Restart();
        _.UpdateProjSpecific(deltaTime);
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateGameSession, "GUI");
        Capture.Update.AddTicksOnce(sw.ElapsedTicks, UpdateGUI, "GUI");

        return false;
      }
    }


  }
}