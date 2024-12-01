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
using System;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework.Graphics;
using Barotrauma.Lights;
using Barotrauma.Items.Components;



namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    [ShowperfPatch]
    public class OnRoundStartPatch
    {
      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(GameSession).GetMethod("StartRound", AccessTools.all, new Type[]{
              typeof(LevelData),
              typeof(bool),
              typeof(SubmarineInfo),
              typeof(SubmarineInfo),
            }
          ),
          postfix: new HarmonyMethod(typeof(OnRoundStartPatch).GetMethod("DoSomeStuff"))
        );
      }

      public static void DoSomeStuff()
      {
        PrintMemoryUsage("Round Start");
      }
    }
  }
}