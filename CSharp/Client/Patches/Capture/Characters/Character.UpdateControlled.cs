
using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Barotrauma.Abilities;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
#if SERVER
using System.Text;
#endif


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    public partial class CharacterPatch
    {
      public static bool Character_UpdateControlled_Replace(Character __instance, float deltaTime, Camera cam)
      {
        if (Showperf == null || !Showperf.Revealed || !UpdateControlledState.IsActive) return true;
        Capture.Update.EnsureCategory(UpdateControlledState);

        Stopwatch sw = new Stopwatch();

        Character _ = __instance;

        if (Character.controlled != _) { return false; }

        sw.Restart();
        _.ControlLocalPlayer(deltaTime, cam);
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateControlledState, $"ControlLocalPlayer");

        sw.Restart();
        Barotrauma.Lights.LightManager.ViewTarget = _;
        CharacterHUD.Update(deltaTime, _, cam);
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateControlledState, $"CharacterHUD.Update");

        sw.Restart();
        if (_.hudProgressBars.Any())
        {
          foreach (var progressBar in _.hudProgressBars)
          {
            if (progressBar.Value.FadeTimer <= 0.0f)
            {
              _.progressBarRemovals.Add(progressBar);
              continue;
            }
            progressBar.Value.Update(deltaTime);
          }
          if (_.progressBarRemovals.Any())
          {
            _.progressBarRemovals.ForEach(pb => _.hudProgressBars.Remove(pb.Key));
            _.progressBarRemovals.Clear();
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateControlledState, $"hudProgressBars");

        return false;
      }


    }
  }
}