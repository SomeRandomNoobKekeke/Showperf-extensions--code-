#define CLIENT
using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Items.Components;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class RepairToolPatch
    {
      public static CaptureState UseState;
      public static CaptureState RepairState;
      public static CaptureState FixBodyState;
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(RepairTool).GetMethod("Use", AccessTools.all),
          prefix: new HarmonyMethod(typeof(RepairToolPatch).GetMethod("RepairTool_Use_Replace"))
        );

        harmony.Patch(
          original: typeof(RepairTool).GetMethod("Repair", AccessTools.all),
          prefix: new HarmonyMethod(typeof(RepairToolPatch).GetMethod("RepairTool_Repair_Replace"))
        );

        harmony.Patch(
        original: typeof(RepairTool).GetMethod("FixBody", AccessTools.all),
        prefix: new HarmonyMethod(typeof(RepairToolPatch).GetMethod("RepairTool_FixBody_Replace"))
      );

        UseState = Capture.Get("Showperf.Update.MapEntity.Items.Use.RepairTool");
        RepairState = Capture.Get("Showperf.Update.MapEntity.Items.Use.RepairTool.Repair");
        FixBodyState = Capture.Get("Showperf.Update.MapEntity.Items.Use.RepairTool.FixBody");
      }



      public static bool RepairTool_Use_Replace(RepairTool __instance, ref bool __result, float deltaTime, Character character = null)
      {
        if (Showperf == null || !Showperf.Revealed || !UseState.IsActive) return true;
        Capture.Update.EnsureCategory(UseState);
        Stopwatch sw = new Stopwatch();

        RepairTool _ = __instance;

        if (character != null)
        {
          if (_.item.RequireAimToUse && !character.IsKeyDown(InputType.Aim)) { __result = false; return false; }
        }

        sw.Restart();
        float degreeOfSuccess = character == null ? 0.5f : _.DegreeOfSuccess(character);
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UseState, "DegreeOfSuccess");

        sw.Restart();
        bool failed = false;
        if (Rand.Range(0.0f, 0.5f) > degreeOfSuccess)
        {
          _.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character);
          failed = true;
        }
        if (_.UsableIn == RepairTool.UseEnvironment.None)
        {
          _.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character);
          failed = true;
        }
        if (_.item.InWater)
        {
          if (_.UsableIn == RepairTool.UseEnvironment.Air)
          {
            _.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character);
            failed = true;
          }
        }
        else
        {
          if (_.UsableIn == RepairTool.UseEnvironment.Water)
          {
            _.ApplyStatusEffects(ActionType.OnFailure, deltaTime, character);
            failed = true;
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UseState, "ApplyStatusEffects(ActionType.OnFailure");


        if (failed)
        {
          // Always apply ActionType.OnUse. If doesn't fail, the effect is called later.
          sw.Restart();
          _.ApplyStatusEffects(ActionType.OnUse, deltaTime, character);
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, UseState, "ApplyStatusEffects(ActionType.OnUse");
          __result = false; return false;
        }

        Vector2 rayStart;
        Vector2 rayStartWorld;
        Vector2 sourcePos = character?.AnimController == null ? _.item.SimPosition : character.AnimController.AimSourceSimPos;
        Vector2 barrelPos = _.item.SimPosition + ConvertUnits.ToSimUnits(_.TransformedBarrelPos);
        //make sure there's no obstacles between the base of the item (or the shoulder of the character) and the end of the barrel

        sw.Restart();
        if (Submarine.PickBody(sourcePos, barrelPos, collisionCategory: Physics.CollisionWall | Physics.CollisionLevel | Physics.CollisionItemBlocking) == null)
        {
          //no obstacles -> we start the raycast at the end of the barrel
          rayStart = ConvertUnits.ToSimUnits(_.item.Position + _.TransformedBarrelPos);
          rayStartWorld = ConvertUnits.ToSimUnits(_.item.WorldPosition + _.TransformedBarrelPos);
        }
        else
        {
          rayStart = rayStartWorld = Submarine.LastPickedPosition + Submarine.LastPickedNormal * 0.1f;
          if (_.item.Submarine != null) { rayStartWorld += _.item.Submarine.SimPosition; }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UseState, "Submarine.PickBody");

        //if the calculated barrel pos is in another hull, use the origin of the item to make sure the particles don't end up in an incorrect hull
        sw.Restart();
        if (_.item.CurrentHull != null)
        {
          var barrelHull = Hull.FindHull(ConvertUnits.ToDisplayUnits(rayStartWorld), _.item.CurrentHull, useWorldCoordinates: true);
          if (barrelHull != null && barrelHull != _.item.CurrentHull)
          {
            if (MathUtils.GetLineWorldRectangleIntersection(ConvertUnits.ToDisplayUnits(sourcePos), ConvertUnits.ToDisplayUnits(rayStart), _.item.CurrentHull.Rect, out Vector2 hullIntersection))
            {
              if (!_.item.CurrentHull.ConnectedGaps.Any(g => g.Open > 0.0f && Submarine.RectContains(g.Rect, hullIntersection)))
              {
                Vector2 rayDir = rayStart.NearlyEquals(sourcePos) ? Vector2.Zero : Vector2.Normalize(rayStart - sourcePos);
                rayStartWorld = ConvertUnits.ToSimUnits(hullIntersection - rayDir * 5.0f);
                if (_.item.Submarine != null) { rayStartWorld += _.item.Submarine.SimPosition; }
              }
            }
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UseState, "FindHull");

        float spread = MathHelper.ToRadians(MathHelper.Lerp(_.UnskilledSpread, _.Spread, degreeOfSuccess));

        float angle = MathHelper.ToRadians(_.BarrelRotation) + spread * Rand.Range(-0.5f, 0.5f);
        float dir = 1;
        if (_.item.body != null)
        {
          angle += _.item.body.Rotation;
          dir = _.item.body.Dir;
        }
        Vector2 rayEnd = rayStartWorld + ConvertUnits.ToSimUnits(new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * _.Range * dir);

        sw.Restart();
        _.ignoredBodies.Clear();
        if (character != null)
        {
          foreach (Limb limb in character.AnimController.Limbs)
          {
            if (Rand.Range(0.0f, 0.5f) > degreeOfSuccess) continue;
            _.ignoredBodies.Add(limb.body.FarseerBody);
          }
          _.ignoredBodies.Add(character.AnimController.Collider.FarseerBody);
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UseState, "Find ignoredBodies");

        _.IsActive = true;
        _.activeTimer = 0.1f;

        _.debugRayStartPos = ConvertUnits.ToDisplayUnits(rayStartWorld);
        _.debugRayEndPos = ConvertUnits.ToDisplayUnits(rayEnd);

        sw.Restart();
        Submarine parentSub = character?.Submarine ?? _.item.Submarine;
        if (parentSub == null)
        {
          foreach (Submarine sub in Submarine.Loaded)
          {
            Rectangle subBorders = sub.Borders;
            subBorders.Location += new Point((int)sub.WorldPosition.X, (int)sub.WorldPosition.Y - sub.Borders.Height);
            if (!MathUtils.CircleIntersectsRectangle(_.item.WorldPosition, _.Range * 5.0f, subBorders))
            {
              continue;
            }
            _.Repair(rayStartWorld - sub.SimPosition, rayEnd - sub.SimPosition, deltaTime, character, degreeOfSuccess, _.ignoredBodies);
          }
          _.Repair(rayStartWorld, rayEnd, deltaTime, character, degreeOfSuccess, _.ignoredBodies);
        }
        else
        {
          _.Repair(rayStartWorld - parentSub.SimPosition, rayEnd - parentSub.SimPosition, deltaTime, character, degreeOfSuccess, _.ignoredBodies);
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UseState, "Repair");

        sw.Restart();
        // not compiled on server
#if CLIENT
        _.UseProjSpecific(deltaTime, rayStartWorld);
#endif
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, UseState, "UseProjSpecific");

        __result = true; return false;
      }

      public static bool RepairTool_Repair_Replace(RepairTool __instance, Vector2 rayStart, Vector2 rayEnd, float deltaTime, Character user, float degreeOfSuccess, List<Body> ignoredBodies)
      {
        if (Showperf == null || !Showperf.Revealed || !RepairState.IsActive) return true;
        Capture.Update.EnsureCategory(RepairState);
        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

        RepairTool _ = __instance;

        var collisionCategories = Physics.CollisionWall | Physics.CollisionItem | Physics.CollisionLevel | Physics.CollisionRepairableWall | Physics.CollisionItemBlocking; ;
        if (!_.IgnoreCharacters)
        {
          collisionCategories |= Physics.CollisionCharacter;
        }


        sw.Restart();
        //if the item can cut off limbs, activate nearby bodies to allow the raycast to hit them
        if (_.statusEffectLists != null)
        {
          static bool CanSeverJoints(ActionType type, Dictionary<ActionType, List<StatusEffect>> effectList) =>
              effectList.TryGetValue(type, out List<StatusEffect> effects) && effects.Any(e => e.SeverLimbsProbability > 0);

          if (CanSeverJoints(ActionType.OnUse, _.statusEffectLists) || CanSeverJoints(ActionType.OnSuccess, _.statusEffectLists))
          {
            float rangeSqr = ConvertUnits.ToSimUnits(_.Range);
            rangeSqr *= rangeSqr;
            foreach (Character c in Character.CharacterList)
            {
              if (!c.Enabled || !c.AnimController.BodyInRest) { continue; }
              //do a broad check first
              if (Math.Abs(c.WorldPosition.X - _.item.WorldPosition.X) > 1000.0f) { continue; }
              if (Math.Abs(c.WorldPosition.Y - _.item.WorldPosition.Y) > 1000.0f) { continue; }
              foreach (Limb limb in c.AnimController.Limbs)
              {
                if (Vector2.DistanceSquared(limb.SimPosition, _.item.SimPosition) < rangeSqr && Vector2.Dot(rayEnd - rayStart, limb.SimPosition - rayStart) > 0)
                {
                  c.AnimController.BodyInRest = false;
                  break;
                }
              }
            }
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairState, "statusEffectLists != null");

        float lastPickedFraction = 0.0f;
        if (_.RepairMultiple)
        {
          sw.Restart();
          var bodies = Submarine.PickBodies(rayStart, rayEnd, ignoredBodies, collisionCategories,
              ignoreSensors: false,
              customPredicate: (Fixture f) =>
              {
                if (f.IsSensor)
                {
                  if (_.RepairThroughHoles && f.Body?.UserData is Structure) { return false; }
                  if (f.Body?.UserData is PhysicsBody) { return false; }
                }
                if (f.Body?.UserData is Item it && it.GetComponent<Planter>() != null) { return false; }
                if (f.Body?.UserData as string == "ruinroom") { return false; }
                if (f.Body?.UserData is VineTile && !(_.FireDamage > 0)) { return false; }
                return true;
              },
              allowInsideFixture: true);

          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, RepairState, "RepairMultiple Submarine.PickBodies");

          sw.Restart();
          RepairTool.hitBodies.Clear();
          RepairTool.hitBodies.AddRange(bodies.Distinct());

          lastPickedFraction = Submarine.LastPickedFraction;
          Type lastHitType = null;
          _.hitCharacters.Clear();
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, RepairState, "RepairMultiple hitBodies.AddRange(bodies.Distinct())");

          sw.Restart();
          foreach (Body body in RepairTool.hitBodies)
          {
            Type bodyType = body.UserData?.GetType();
            if (!_.RepairThroughWalls && bodyType != null && bodyType != lastHitType)
            {
              //stop the ray if it already hit a door/wall and is now about to hit some other type of entity
              if (lastHitType == typeof(Item) || lastHitType == typeof(Structure)) { break; }
            }
            if (!_.RepairMultipleWalls && (bodyType == typeof(Structure) || (body.UserData as Item)?.GetComponent<Door>() != null)) { break; }

            Character hitCharacter = null;
            if (body.UserData is Limb limb)
            {
              hitCharacter = limb.character;
            }
            else if (body.UserData is Character character)
            {
              hitCharacter = character;
            }
            //only do damage once to each character even if they ray hit multiple limbs
            if (hitCharacter != null)
            {
              if (_.hitCharacters.Contains(hitCharacter)) { continue; }
              _.hitCharacters.Add(hitCharacter);
            }

            //if repairing through walls is not allowed and the next wall is more than 100 pixels away from the previous one, stop here
            //(= repairing multiple overlapping walls is allowed as long as the edges of the walls are less than MaxOverlappingWallDist pixels apart)
            float thisBodyFraction = Submarine.LastPickedBodyDist(body);
            if (!_.RepairThroughWalls && lastHitType == typeof(Structure) && _.Range * (thisBodyFraction - lastPickedFraction) > _.MaxOverlappingWallDist)
            {
              break;
            }
            _.pickedPosition = rayStart + (rayEnd - rayStart) * thisBodyFraction;
            sw2.Restart();
            if (_.FixBody(user, _.pickedPosition, deltaTime, degreeOfSuccess, body))
            {
              lastPickedFraction = thisBodyFraction;
              if (bodyType != null) { lastHitType = bodyType; }
            }
            sw2.Stop();
            Capture.Update.AddTicks(sw2.ElapsedTicks, RepairState, "RepairMultiple FixBody");
          }
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, RepairState, "RepairMultiple foreach (Body body in RepairTool.hitBodies)");

        }
        else
        {
          sw.Restart();

          var pickedBody = Submarine.PickBody(rayStart, rayEnd,
              ignoredBodies, collisionCategories,
              ignoreSensors: false,
              customPredicate: (Fixture f) =>
              {
                if (f.IsSensor)
                {
                  if (_.RepairThroughHoles && f.Body?.UserData is Structure) { return false; }
                  if (f.Body?.UserData is PhysicsBody) { return false; }
                }
                if (f.Body?.UserData as string == "ruinroom") { return false; }
                if (f.Body?.UserData is VineTile && !(_.FireDamage > 0)) { return false; }

                if (f.Body?.UserData is Item targetItem)
                {
                  if (!_.HitItems) { return false; }
                  if (_.HitBrokenDoors)
                  {
                    if (targetItem.GetComponent<Door>() == null && targetItem.Condition <= 0) { return false; }
                  }
                  else
                  {
                    if (targetItem.Condition <= 0) { return false; }
                  }
                }
                return f.Body?.UserData != null;
              },
              allowInsideFixture: true);
          _.pickedPosition = Submarine.LastPickedPosition;
          _.FixBody(user, _.pickedPosition, deltaTime, degreeOfSuccess, pickedBody);
          lastPickedFraction = Submarine.LastPickedFraction;

          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, RepairState, "!RepairMultiple");
        }

        sw.Restart();
        if (_.ExtinguishAmount > 0.0f && _.item.CurrentHull != null)
        {
          _.fireSourcesInRange.Clear();
          //step along the ray in 10% intervals, collecting all fire sources in the range
          for (float x = 0.0f; x <= lastPickedFraction; x += 0.1f)
          {
            Vector2 displayPos = ConvertUnits.ToDisplayUnits(rayStart + (rayEnd - rayStart) * x);
            if (_.item.CurrentHull.Submarine != null) { displayPos += _.item.CurrentHull.Submarine.Position; }

            Hull hull = Hull.FindHull(displayPos, _.item.CurrentHull);
            if (hull == null) continue;
            foreach (FireSource fs in hull.FireSources)
            {
              if (fs.IsInDamageRange(displayPos, 100.0f) && !_.fireSourcesInRange.Contains(fs))
              {
                _.fireSourcesInRange.Add(fs);
              }
            }
            foreach (FireSource fs in hull.FakeFireSources)
            {
              if (fs.IsInDamageRange(displayPos, 100.0f) && !_.fireSourcesInRange.Contains(fs))
              {
                _.fireSourcesInRange.Add(fs);
              }
            }
          }

          foreach (FireSource fs in _.fireSourcesInRange)
          {
            fs.Extinguish(deltaTime, _.ExtinguishAmount);
#if SERVER
          if (!(fs is DummyFireSource))
          {
            GameMain.Server.KarmaManager.OnExtinguishingFire(user, deltaTime);
          }
#endif
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairState, "Extinguish");


        sw.Restart();
        if (_.WaterAmount > 0.0f && _.item.Submarine != null)
        {
          Vector2 pos = ConvertUnits.ToDisplayUnits(rayStart + _.item.Submarine.SimPosition);

          // Could probably be done much efficiently here
          foreach (Item it in Item.ItemList)
          {
            if (it.Submarine == _.item.Submarine && it.GetComponent<Planter>() is { } planter)
            {
              if (it.GetComponent<Holdable>() is { } holdable && holdable.Attachable && !holdable.Attached) { continue; }

              Rectangle collisionRect = it.WorldRect;
              collisionRect.Y -= collisionRect.Height;
              if (collisionRect.Left < pos.X && collisionRect.Right > pos.X && collisionRect.Bottom < pos.Y)
              {
                Body collision = Submarine.PickBody(rayStart, it.SimPosition, ignoredBodies, collisionCategories);
                if (collision == null)
                {
                  for (var i = 0; i < planter.GrowableSeeds.Length; i++)
                  {
                    Growable seed = planter.GrowableSeeds[i];
                    if (seed == null || seed.Decayed) { continue; }

                    seed.Health += _.WaterAmount * deltaTime;

#if CLIENT
                    float barOffset = 10f * GUI.Scale;
                    Vector2 offset = planter.PlantSlots.ContainsKey(i) ? planter.PlantSlots[i].Offset : Vector2.Zero;
                    user?.UpdateHUDProgressBar(planter, planter.Item.DrawPosition + new Vector2(barOffset, 0) + offset, seed.Health / seed.MaxWater, GUIStyle.Blue, GUIStyle.Blue, "progressbar.watering");
#endif
                  }
                }
              }
            }
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairState, "Water plants");

        sw.Restart();
        if (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer)
        {
          if (Rand.Range(0.0f, 1.0f) < _.FireProbability * deltaTime && _.item.CurrentHull != null)
          {
            Vector2 displayPos = ConvertUnits.ToDisplayUnits(rayStart + (rayEnd - rayStart) * lastPickedFraction * 0.9f);
            if (_.item.CurrentHull.Submarine != null) { displayPos += _.item.CurrentHull.Submarine.Position; }
            new FireSource(displayPos, sourceCharacter: user);
          }
        }
        sw.Stop();
        Capture.Update.AddTicks(sw.ElapsedTicks, RepairState, "Start fires");

        return false;
      }


      public static bool RepairTool_FixBody_Replace(RepairTool __instance, ref bool __result, Character user, Vector2 hitPosition, float deltaTime, float degreeOfSuccess, Body targetBody)
      {
        if (Showperf == null || !Showperf.Revealed || !FixBodyState.IsActive) return true;
        Capture.Update.EnsureCategory(FixBodyState);
        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();

        RepairTool _ = __instance;

        if (targetBody?.UserData == null) { __result = false; return false; }

        if (targetBody.UserData is Structure targetStructure)
        {
          bool guh;
          sw.Restart();
          if (targetStructure.IsPlatform) { __result = false; return false; }

          sw2.Restart();
          int sectionIndex = targetStructure.FindSectionIndex(ConvertUnits.ToDisplayUnits(_.pickedPosition));
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "targetStructure.FindSectionIndex");

          if (sectionIndex < 0) { __result = false; return false; }

          sw2.Restart();
          guh = !_.fixableEntities.Contains("structure") && !_.fixableEntities.Contains(targetStructure.Prefab.Identifier);
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "!fixableEntities.Contains");
          if (guh) { __result = true; return false; }

          sw2.Restart();
          guh = _.nonFixableEntities.Contains(targetStructure.Prefab.Identifier) || _.nonFixableEntities.Any(t => targetStructure.Tags.Contains(t));
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "nonFixableEntities.Contains");

          if (guh) { __result = false; return false; }

          sw2.Restart();
          _.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnUse, structure: targetStructure);
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "ApplyStatusEffectsOnTarget OnUse");

          sw2.Restart();
          _.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnSuccess, structure: targetStructure);
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "ApplyStatusEffectsOnTarget OnSuccess");

          sw2.Restart();
          //not compiled on server  
#if CLIENT
          _.FixStructureProjSpecific(user, deltaTime, targetStructure, sectionIndex);
#endif
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "FixStructureProjSpecific");

          sw2.Restart();
          float structureFixAmount = _.StructureFixAmount;
          if (structureFixAmount >= 0f)
          {
            structureFixAmount *= 1 + user.GetStatValue(StatTypes.RepairToolStructureRepairMultiplier);
            structureFixAmount *= 1 + _.item.GetQualityModifier(Quality.StatType.RepairToolStructureRepairMultiplier);
          }
          else
          {
            structureFixAmount *= 1 + user.GetStatValue(StatTypes.RepairToolStructureDamageMultiplier);
            structureFixAmount *= 1 + _.item.GetQualityModifier(Quality.StatType.RepairToolStructureDamageMultiplier);
          }
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "structureFixAmount");

          sw2.Restart();
          var didLeak = targetStructure.SectionIsLeakingFromOutside(sectionIndex);
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "SectionIsLeakingFromOutside");

          sw2.Restart();
          targetStructure.AddDamage(sectionIndex, -structureFixAmount * degreeOfSuccess, user);
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "AddDamage");

          sw2.Restart();
          if (didLeak && !targetStructure.SectionIsLeakingFromOutside(sectionIndex))
          {
            user.CheckTalents(AbilityEffectType.OnRepairedOutsideLeak);
          }
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "user.CheckTalents(AbilityEffectType.OnRepairedOutsideLeak)");


          sw2.Restart();
          //if the next section is small enough, apply the effect to it as well
          //(to make it easier to fix a small "left-over" section)
          for (int i = -1; i < 2; i += 2)
          {
            int nextSectionLength = targetStructure.SectionLength(sectionIndex + i);
            if ((sectionIndex == 1 && i == -1) ||
                (sectionIndex == targetStructure.SectionCount - 2 && i == 1) ||
                (nextSectionLength > 0 && nextSectionLength < Structure.WallSectionSize * 0.3f))
            {
              //targetStructure.HighLightSection(sectionIndex + i);
              targetStructure.AddDamage(sectionIndex + i, -structureFixAmount * degreeOfSuccess);
            }
          }
          sw2.Stop();
          Capture.Update.AddTicks(sw2.ElapsedTicks, FixBodyState, "if the next section is small enough");

          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, FixBodyState, "UserData is Structure");
          __result = true; return false;
        }
        else if (targetBody.UserData is Voronoi2.VoronoiCell cell && cell.IsDestructible)
        {
          sw.Restart();
          if (Level.Loaded?.ExtraWalls.Find(w => w.Body == cell.Body) is DestructibleLevelWall levelWall)
          {
            levelWall.AddDamage(-_.LevelWallFixAmount * deltaTime, ConvertUnits.ToDisplayUnits(hitPosition));
          }
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, FixBodyState, "UserData is Voronoi2");
          __result = true; return false;
        }
        else if (targetBody.UserData is LevelObject levelObject && levelObject.Prefab.TakeLevelWallDamage)
        {
          sw.Restart();
          levelObject.AddDamage(-_.LevelWallFixAmount, deltaTime, _.item);
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, FixBodyState, "UserData is LevelObject");
          __result = true; return false;
        }
        else if (targetBody.UserData is Character targetCharacter)
        {
          sw.Restart();
          if (targetCharacter.Removed) { __result = false; return false; }
          targetCharacter.LastDamageSource = _.item;
          Limb closestLimb = null;
          float closestDist = float.MaxValue;
          foreach (Limb limb in targetCharacter.AnimController.Limbs)
          {
            if (limb.Removed || limb.IgnoreCollisions || limb.Hidden || limb.IsSevered) { continue; }
            float dist = Vector2.DistanceSquared(_.item.SimPosition, limb.SimPosition);
            if (dist < closestDist)
            {
              closestLimb = limb;
              closestDist = dist;
            }
          }

          if (closestLimb != null && !MathUtils.NearlyEqual(_.TargetForce, 0.0f))
          {
            Vector2 dir = closestLimb.WorldPosition - _.item.WorldPosition;
            dir = dir.LengthSquared() < 0.0001f ? Vector2.UnitY : Vector2.Normalize(dir);
            closestLimb.body.ApplyForce(dir * _.TargetForce, maxVelocity: 10.0f);
          }

          _.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnUse, character: targetCharacter, limb: closestLimb);
          _.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnSuccess, character: targetCharacter, limb: closestLimb);
          //not compiled on server  
#if CLIENT
          _.FixCharacterProjSpecific(user, deltaTime, targetCharacter);
#endif

          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, FixBodyState, "UserData is Character");
          __result = true; return false;
        }
        else if (targetBody.UserData is Limb targetLimb)
        {
          sw.Restart();
          if (targetLimb.character == null || targetLimb.character.Removed) { __result = false; return false; }

          if (!MathUtils.NearlyEqual(_.TargetForce, 0.0f))
          {
            Vector2 dir = targetLimb.WorldPosition - _.item.WorldPosition;
            dir = dir.LengthSquared() < 0.0001f ? Vector2.UnitY : Vector2.Normalize(dir);
            targetLimb.body.ApplyForce(dir * _.TargetForce, maxVelocity: 10.0f);
          }

          targetLimb.character.LastDamageSource = _.item;
          _.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnUse, character: targetLimb.character, limb: targetLimb);
          _.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnSuccess, character: targetLimb.character, limb: targetLimb);
          //not compiled on server  
#if CLIENT
          _.FixCharacterProjSpecific(user, deltaTime, targetLimb.character);
#endif

          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, FixBodyState, "UserData is Limb");
          __result = true; return false;
        }
        else if (targetBody.UserData is Barotrauma.Item or Holdable)
        {
          Item targetItem = targetBody.UserData is Holdable holdable ? holdable.Item : (Item)targetBody.UserData;
          sw.Restart();
          if (!_.HitItems || !targetItem.IsInteractable(user)) { __result = false; return false; }

          var levelResource = targetItem.GetComponent<LevelResource>();
          if (levelResource != null && levelResource.Attached &&
              levelResource.RequiredItems.Any() &&
              levelResource.HasRequiredItems(user, addMessage: false))
          {
            float addedDetachTime = deltaTime *
                _.DeattachSpeed *
                (1f + user.GetStatValue(StatTypes.RepairToolDeattachTimeMultiplier)) *
                (1f + _.item.GetQualityModifier(Quality.StatType.RepairToolDeattachTimeMultiplier));
            levelResource.DeattachTimer += addedDetachTime;
#if CLIENT
            if (targetItem.Prefab.ShowHealthBar && Character.Controlled != null &&
                (user == Character.Controlled || Character.Controlled.CanSeeTarget(_.item)))
            {
              Character.Controlled.UpdateHUDProgressBar(
                  _,
                  targetItem.WorldPosition,
                  levelResource.DeattachTimer / levelResource.DeattachDuration,
                  GUIStyle.Red, GUIStyle.Green, "progressbar.deattaching");
            }

            //  not compiled on server
            _.FixItemProjSpecific(user, deltaTime, targetItem, showProgressBar: false);
#endif
            __result = true; return false;
          }

          if (!targetItem.Prefab.DamagedByRepairTools) { __result = false; return false; }

          if (_.HitBrokenDoors)
          {
            if (targetItem.GetComponent<Door>() == null && targetItem.Condition <= 0) { __result = false; return false; }
          }
          else
          {
            if (targetItem.Condition <= 0) { __result = false; return false; }
          }

          targetItem.IsHighlighted = true;

          _.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnUse, targetItem);
          _.ApplyStatusEffectsOnTarget(user, deltaTime, ActionType.OnSuccess, targetItem);

          if (targetItem.body != null && !MathUtils.NearlyEqual(_.TargetForce, 0.0f))
          {
            Vector2 dir = targetItem.WorldPosition - _.item.WorldPosition;
            dir = dir.LengthSquared() < 0.0001f ? Vector2.UnitY : Vector2.Normalize(dir);
            targetItem.body.ApplyForce(dir * _.TargetForce, maxVelocity: 10.0f);
          }

          //not compiled on server
#if CLIENT
          _.FixItemProjSpecific(user, deltaTime, targetItem, showProgressBar: true);
#endif
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, FixBodyState, "UserData is Item");
          __result = true; return false;
        }
        else if (targetBody.UserData is BallastFloraBranch branch)
        {
          sw.Restart();
          if (branch.ParentBallastFlora is { } ballastFlora)
          {
            ballastFlora.DamageBranch(branch, _.FireDamage * deltaTime, BallastFloraBehavior.AttackType.Fire, user);
          }
          sw.Stop();
          Capture.Update.AddTicks(sw.ElapsedTicks, FixBodyState, "UserData is BallastFloraBranch");
        }
        __result = false; return false;
      }

    }
  }
}