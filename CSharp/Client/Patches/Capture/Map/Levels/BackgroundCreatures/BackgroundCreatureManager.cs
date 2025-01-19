using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class BackgroundCreatureManagerPatch
    {
      public static CaptureState BackCreatures;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(BackgroundCreatureManager).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(BackgroundCreatureManagerPatch).GetMethod("BackgroundCreatureManager_Update_Replace"))
        );

        BackCreatures = Capture.Get("Showperf.Update.Level.BackgroundCreatureManager");
      }

      public static bool BackgroundCreatureManager_Update_Replace(float deltaTime, Camera cam, BackgroundCreatureManager __instance)
      {
        if (Showperf == null || !Showperf.Revealed || !BackCreatures.IsActive) return true;

        BackgroundCreatureManager _ = __instance;

        Stopwatch sw = new Stopwatch();
        Capture.Update.EnsureCategory(BackCreatures);

        if (_.checkVisibleTimer < 0.0f)
        {
          int margin = 500;
          foreach (BackgroundCreature creature in _.creatures)
          {
            sw.Restart();
            Rectangle extents = creature.GetExtents(cam);
            bool wasVisible = creature.Visible;
            creature.Visible =
                extents.Right >= cam.WorldView.X - margin &&
                extents.X <= cam.WorldView.Right + margin &&
                extents.Bottom >= cam.WorldView.Y - cam.WorldView.Height - margin &&
                extents.Y <= cam.WorldView.Y + margin;

            sw.Stop();
            if (BackCreatures.ByID)
            {
              Capture.Update.AddTicks(sw.ElapsedTicks, BackCreatures, $"{creature.Prefab.Name} in swarm of [{creature.Swarm?.Members?.Count}]");
            }
            else
            {
              Capture.Update.AddTicks(sw.ElapsedTicks, BackCreatures, $"{creature.Prefab.Name}");
            }

          }

          _.checkVisibleTimer = BackgroundCreatureManager.VisibilityCheckInterval;
        }
        else
        {
          _.checkVisibleTimer -= deltaTime;
        }

        foreach (BackgroundCreature creature in _.creatures)
        {
          if (!creature.Visible) { continue; }
          sw.Restart();
          creature.Update(deltaTime);
          sw.Stop();
          if (BackCreatures.ByID)
          {
            Capture.Update.AddTicks(sw.ElapsedTicks, BackCreatures, $"{creature.Prefab.Name} in swarm of [{creature.Swarm?.Members?.Count}]");
          }
          else
          {
            Capture.Update.AddTicks(sw.ElapsedTicks, BackCreatures, $"{creature.Prefab.Name}");
          }
        }

        return false;
      }

    }
  }
}