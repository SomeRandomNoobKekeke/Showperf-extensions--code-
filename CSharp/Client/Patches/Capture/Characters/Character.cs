
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
    public partial class CharacterPatch
    {
      public static CaptureState UpdateAllState;
      public static CaptureState UpdateState;
      public static CaptureState ControlState;
      public static CaptureState TalentsState;
      public static CaptureState SEState;

      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(Character).GetMethod("UpdateAll", AccessTools.all),
          prefix: new HarmonyMethod(typeof(CharacterPatch).GetMethod("Character_UpdateAll_Replace"))
        );

        harmony.Patch(
          original: typeof(Character).GetMethod("Update", AccessTools.all),
          prefix: new HarmonyMethod(typeof(CharacterPatch).GetMethod("Character_Update_Replace"))
        );

        harmony.Patch(
          original: typeof(Character).GetMethod("Control", AccessTools.all),
          prefix: new HarmonyMethod(typeof(CharacterPatch).GetMethod("Character_Control_Replace"))
        );

        harmony.Patch(
          original: typeof(Character).GetMethod("ApplyStatusEffects", AccessTools.all),
          prefix: new HarmonyMethod(typeof(CharacterPatch).GetMethod("Character_ApplyStatusEffects_Replace"))
        );

        UpdateAllState = Capture.Get("Showperf.Update.Character");
        UpdateState = Capture.Get("Showperf.Update.Character.Update");
        ControlState = Capture.Get("Showperf.Update.Character.Update.Control");
        TalentsState = Capture.Get("Showperf.Update.Character.Update.Talents");
        SEState = Capture.Get("Showperf.Update.Character.Update.StatusEffects");
      }
    }
  }
}