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
using Microsoft.Xna.Framework.Input;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {

    public Harmony harmony;

    public static bool debug = true;

    public static bool DrawItemUpdateTimes = false;

    public static CaptureWindow window;


    public void Initialize()
    {
      harmony = new Harmony("show.perf");
      if (debug) DrawItemUpdateTimes = true;

      addCommands();

      window = new CaptureWindow(duration: 3, fps: 30);
      view = new WindowView(window);


      patchAll();

      info($"no errors");
    }

    public void patchAll()
    {
      harmony.Patch(
        original: typeof(MapEntity).GetMethod("UpdateAll", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("MapEntity_UpdateAll_Replace"))
      );

      harmony.Patch(
        original: typeof(GUI).GetMethod("Draw", AccessTools.all),
        postfix: new HarmonyMethod(typeof(Mod).GetMethod("GUI_Draw_Postfix"))
      );

      harmony.Patch(
        original: typeof(LuaGame).GetMethod("IsCustomCommandPermitted"),
        postfix: new HarmonyMethod(typeof(Mod).GetMethod("permitCommands"))
      );
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

    public void Dispose()
    {
      harmony.UnpatchAll(harmony.Id);
      harmony = null;

      window.Dispose();
      window = null;

      view.Dispose();
      view = null;

      removeCommands();
    }
  }
}