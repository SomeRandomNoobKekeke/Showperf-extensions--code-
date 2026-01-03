
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
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using System.Globalization;
using MoonSharp.Interpreter;
using Barotrauma.Abilities;


namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public partial class CharacterHealthPatch
    {
      public static CaptureState SEState;

      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(CharacterHealth).GetMethod("ApplyAfflictionStatusEffects", AccessTools.all),
          prefix: new HarmonyMethod(typeof(CharacterHealthPatch).GetMethod("CharacterHealth_ApplyAfflictionStatusEffects_Replace"))
        );

        SEState = Capture.Get("Showperf.Update.Character.Update.StatusEffects.CharacterHealth");
      }

      public static void CaptureAffliction(long ticks, Character character, Affliction affliction, ActionType type, Limb limb)
      {
        if (SEState.ByID)
        {
          Capture.Update.AddTicks(ticks, SEState, $"{character.Info?.DisplayName ?? character.ToString()} {affliction}.{type}");
        }
        else
        {
          Capture.Update.AddTicks(ticks, SEState, $"{affliction}.{type}");
        }
      }

      public static bool CharacterHealth_ApplyAfflictionStatusEffects_Replace(CharacterHealth __instance, ActionType type)
      {
        if (Showperf == null || !Showperf.Revealed || !SEState.IsActive) return true;
        Capture.Update.EnsureCategory(SEState);

        Stopwatch sw = new Stopwatch();

        CharacterHealth _ = __instance;

        if (_.isApplyingAfflictionStatusEffects)
        {
          //pretty hacky: if we're already in the process of applying afflictions' status effects
          //(i.e. calling this method caused some additional afflictions to appear and trigger status effects)
          //let's instantiate a new list so we don't end up modifying afflictionsCopy while enumerating it
          foreach (Affliction affliction in _.afflictions.Keys.ToList())
          {
            sw.Restart();
            Limb limb = _.GetAfflictionLimb(affliction);
            affliction.ApplyStatusEffects(type, 1.0f, _, targetLimb: limb);
            sw.Stop();
            CaptureAffliction(sw.ElapsedTicks, _.Character, affliction, type, limb);
          }
        }
        else
        {
          _.isApplyingAfflictionStatusEffects = true;
          _.afflictionsCopy.Clear();
          _.afflictionsCopy.AddRange(_.afflictions.Keys);
          _.isApplyingAfflictionStatusEffects = true;
          foreach (Affliction affliction in _.afflictionsCopy)
          {
            sw.Restart();
            Limb limb = _.GetAfflictionLimb(affliction);
            affliction.ApplyStatusEffects(type, 1.0f, _, targetLimb: limb);
            sw.Stop();
            CaptureAffliction(sw.ElapsedTicks, _.Character, affliction, type, limb);
          }
          _.isApplyingAfflictionStatusEffects = false;
        }

        return false;
      }
    }
  }
}