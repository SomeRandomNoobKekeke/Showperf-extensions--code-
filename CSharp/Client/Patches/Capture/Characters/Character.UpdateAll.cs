
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
      public static void CaptureCharacter(long ticks, Character character)
      {
        if (UpdateAllState.ByID)
        {
          string info = character.Info?.DisplayName == null ? "" : " " + character.Info?.DisplayName;
          string enabled = character.Enabled ? "Enabled" : "Disabled";
          string alive = character.IsDead ? "Dead" : "Alive";
          string simplified = character.AnimController.SimplePhysicsEnabled ? "Simple" : "Complex";

          Capture.Update.AddTicks(ticks, UpdateAllState, $"{character.ID}|{character}{info} - {alive}:{enabled}:{simplified}", (int)character.ID);
        }
        else
        {
          Capture.Update.AddTicks(ticks, UpdateAllState, character.ToString());
        }
      }


      public static bool Character_UpdateAll_Replace(float deltaTime, Camera cam)
      {
        if (Showperf == null || !Showperf.Revealed || !UpdateAllState.IsActive) return true;
        Capture.Update.EnsureCategory(UpdateAllState);

        Stopwatch sw = new Stopwatch();

        sw.Restart();
        if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient) // single player or server
        {
          foreach (Character c in Character.CharacterList)
          {
            // TODO: The logic below seems to be overly complicated and quite confusing
            if (c is not AICharacter && !c.IsRemotePlayer) { continue; } // confusing -> what this line is intended for? local player? But that's handled below...
            if (c.IsRemotePlayer)
            {
              // Let the client tell when to enable the character. If we force it enabled here, it may e.g. get killed while still loading a round.
              continue;
            }
            if (c.IsLocalPlayer || (c.IsBot && !c.IsDead))
            {
              c.Enabled = true;
            }
            else if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer) // mp server
            {
              //disable AI characters that are far away from all clients and the host's character and not controlled by anyone
              float closestPlayerDist = c.GetDistanceToClosestPlayer();
              if (closestPlayerDist > c.Params.DisableDistance)
              {
                c.Enabled = false;
                if (c.IsDead && c.AIController is EnemyAIController)
                {
                  Character.Spawner?.AddEntityToRemoveQueue(c);
                }
              }
              else if (closestPlayerDist < c.Params.DisableDistance * 0.9f)
              {
                c.Enabled = true;
              }
            }
            else if (Submarine.MainSub != null) // sp only?
            {
              //disable AI characters that are far away from the sub and the controlled character
              float distSqr = Vector2.DistanceSquared(Submarine.MainSub.WorldPosition, c.WorldPosition);
              if (Character.Controlled != null)
              {
                distSqr = Math.Min(distSqr, Vector2.DistanceSquared(Character.Controlled.WorldPosition, c.WorldPosition));
              }
              else
              {
                distSqr = Math.Min(distSqr, Vector2.DistanceSquared(GameMain.GameScreen.Cam.GetPosition(), c.WorldPosition));
              }

              if (distSqr > MathUtils.Pow2(c.Params.DisableDistance))
              {
                c.Enabled = false;
                if (c.IsDead && c.AIController is EnemyAIController)
                {
                  Entity.Spawner?.AddEntityToRemoveQueue(c);
                }
              }
              else if (distSqr < MathUtils.Pow2(c.Params.DisableDistance * 0.9f))
              {
                c.Enabled = true;
              }
            }
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateAllState, "Disable far away characters");

        Character.characterUpdateTick++;



        if (Character.characterUpdateTick % Character.CharacterUpdateInterval == 0)
        {
          for (int i = 0; i < Character.CharacterList.Count; i++)
          {
            if (GameMain.LuaCs.Game.UpdatePriorityCharacters.Contains(Character.CharacterList[i])) continue;

            sw.Restart();
            Character.CharacterList[i].Update(deltaTime * Character.CharacterUpdateInterval, cam);
            sw.Stop();
            CaptureCharacter(sw.ElapsedTicks, Character.CharacterList[i]);
          }
        }

        foreach (Character character in GameMain.LuaCs.Game.UpdatePriorityCharacters)
        {
          if (character.Removed) { continue; }
          System.Diagnostics.Debug.Assert(character is { Removed: false });
          sw.Restart();
          character.Update(deltaTime, cam);
          sw.Stop();
          CaptureCharacter(sw.ElapsedTicks, character);
        }

#if CLIENT
        sw.Restart();
        Character.UpdateSpeechBubbles(deltaTime);
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UpdateAllState, "UpdateSpeechBubbles");
#endif

        return false;
      }



    }
  }
}