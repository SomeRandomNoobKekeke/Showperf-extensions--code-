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

using Barotrauma.Items.Components;


namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public static bool Character_UpdateAll_Replace(float deltaTime, Camera cam)
    {
      if (ActiveCategory != ShowperfCategory.Characters) return true;
      Window.ensureCategory(CaptureCategory.Characters);

      var sw = new System.Diagnostics.Stopwatch();

      if (GameMain.NetworkMember == null || !GameMain.NetworkMember.IsClient)
      {
        foreach (Character c in Character.CharacterList)
        {
          if (c is not AICharacter && !c.IsRemotePlayer) { continue; }

          if (c.IsPlayer || (c.IsBot && !c.IsDead))
          {
            c.Enabled = true;
          }
          else if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
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
          else if (Submarine.MainSub != null)
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

      for (int i = 0; i < Character.CharacterList.Count; i++)
      {
        try
        {
          sw.Restart();

          var character = Character.CharacterList[i];
          System.Diagnostics.Debug.Assert(character != null && !character.Removed);
          character.Update(deltaTime, cam);

          string info = character.Info == null ? "" : $":{character.Info.DisplayName}";
          string enabled = character.Enabled ? "Enabled" : "Disabled";
          string alive = character.IsDead ? "Dead" : "Alive";
          string simplified = character.AnimController.SimplePhysicsEnabled ? "Simplified" : "Simulated";



          if (CaptureFrom.Count == 0 || (character.Submarine != null && CaptureFrom.Contains(character.Submarine.Info.Type)))
          {
            Window.tryAddTicks((int)character.ID, $"{character.ID}|{character}{info} - {alive}:{enabled}:{simplified}", CaptureCategory.Characters, sw.ElapsedTicks);
          }
        }
        catch (Exception e) { err(e); }
      }

      // #if CLIENT
      Character.UpdateSpeechBubbles(deltaTime);
      // #endif

      sw.Stop();

      return false;
    }

  }
}