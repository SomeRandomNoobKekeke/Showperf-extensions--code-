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
    [ShowperfPatch]
    public class AICharacterPatch
    {
      public static CaptureState CaptureUpdate;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(AICharacter).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(AICharacterPatch).GetMethod("AICharacter_Update_Replace"))
        );

        CaptureUpdate = Capture.Get("Showperf.Update.Character.Update");
      }

      public static void CaptureCharacter(long ticks, AICharacter character, string text)
      {
        if (CaptureUpdate.ByID)
        {
          Capture.Update.AddTicks(ticks, CaptureUpdate, $"{character.Info?.DisplayName ?? character.ToString()}.{text}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, CaptureUpdate, text);
        }
      }

      public static bool AICharacter_Update_Replace(float deltaTime, Camera cam, AICharacter __instance)
      {
        if (Showperf == null || !Showperf.Revealed || !CaptureUpdate.IsActive) return true;
        Capture.Update.EnsureCategory(CaptureUpdate);

        Stopwatch sw = new Stopwatch();

        AICharacter _ = __instance;

        //base.Update(deltaTime, cam);
        CharacterPatch.Character_Update_Replace(deltaTime, cam, _);

        if (!_.Enabled) { return false; }

        sw.Restart();
        if (!_.IsRemotePlayer && _.AIController is EnemyAIController enemyAi)
        {
          enemyAi.PetBehavior?.Update(deltaTime);
        }
        sw.Stop();
        CaptureCharacter(sw.ElapsedTicks, _, "PetBehavior");

        if (_.IsDead || _.Vitality <= 0.0f || _.Stun > 0.0f || _.IsIncapacitated)
        {
          //don't enable simple physics on dead/incapacitated characters
          //the ragdoll controls the movement of incapacitated characters instead of the collider,
          //but in simple physics mode the ragdoll would get disabled, causing the character to not move at all
          _.AnimController.SimplePhysicsEnabled = false;
          return false;
        }


        if (!_.IsRemotePlayer && _.AIController is not HumanAIController)
        {
          float characterDistSqr = _.GetDistanceSqrToClosestPlayer();
          if (characterDistSqr > MathUtils.Pow2(_.Params.DisableDistance * 0.5f))
          {
            _.AnimController.SimplePhysicsEnabled = true;
          }
          else if (characterDistSqr < MathUtils.Pow2(_.Params.DisableDistance * 0.5f * 0.9f))
          {
            _.AnimController.SimplePhysicsEnabled = false;
          }
        }
        else
        {
          _.AnimController.SimplePhysicsEnabled = false;
        }

        if (GameMain.NetworkMember != null && !GameMain.NetworkMember.IsServer) { return false; }
        if (Character.Controlled == _) { return false; }

        sw.Restart();
        if (!_.IsRemotelyControlled && _.aiController != null && _.aiController.Enabled)
        {
          _.aiController.Update(deltaTime);
        }
        sw.Stop();
        CaptureCharacter(sw.ElapsedTicks, _, "aiController");

        return false;
      }
    }
  }
}