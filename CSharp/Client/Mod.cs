using System;
using System.Reflection;
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

    public static bool debug = false;

    public static bool DrawItemUpdateTimes = false;

    public static CaptureWindow window;


    public void Initialize()
    {
      harmony = new Harmony("show.perf");
      addCommands();

      window = new CaptureWindow(length: 1, sections: 10);
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