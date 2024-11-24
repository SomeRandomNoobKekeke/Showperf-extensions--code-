using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class AIControllerPatch
    {
      public static void Initialize()
      {
        // harmony.Patch(
        //   original: typeof(AIController).GetMethod("Update", AccessTools.all),
        //   prefix: new HarmonyMethod(typeof(AIControllerPatch).GetMethod("AIController_Update_Replace"))
        // );
      }

      public static bool Update(float deltaTime, AIController __instance)
      {
        if (__instance.hullVisibilityTimer > 0)
        {
          __instance.hullVisibilityTimer--;
        }
        else
        {
          __instance.hullVisibilityTimer = AIController.hullVisibilityInterval;
          __instance.VisibleHulls = __instance.Character.GetVisibleHulls();
        }

        return false;
      }
    }
  }
}