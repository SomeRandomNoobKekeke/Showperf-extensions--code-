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

using System.IO;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public Harmony harmony;

    public static string ModName = "Showperf extensions";
    public static string ModDir = "";
    public static bool debug = false;

    public enum ShowperfCategory
    {
      None,
      CharactersUpdate,
      MapEntityDrawing,
      LevelObjectsDrawing,
      LevelMisc,
      ItemComponentsUpdate,
      HullUpdate,
      StructureUpdate,
      ItemUpdate,
      GapUpdate,
      WholeSubmarineUpdate,
    }

    public static Dictionary<ShowperfCategory, string> CategoryNames = new Dictionary<ShowperfCategory, string>()
    {
      {ShowperfCategory.None, ""},
      {ShowperfCategory.CharactersUpdate, "Characters update"},
      {ShowperfCategory.ItemComponentsUpdate, "Item components update"},
      {ShowperfCategory.MapEntityDrawing, "MapEntity drawing"},
      {ShowperfCategory.LevelObjectsDrawing, "Level objects drawing"},
      {ShowperfCategory.LevelMisc, "Other level stuff drawing"},
      {ShowperfCategory.HullUpdate, "Hull update"},
      {ShowperfCategory.ItemUpdate, "Item update"},
      {ShowperfCategory.StructureUpdate, "Structure update"},
      {ShowperfCategory.GapUpdate, "Gap update"},
      {ShowperfCategory.WholeSubmarineUpdate, "Whole submarine update"},
    };

    public static ShowperfCategory activeCategory = ShowperfCategory.None;
    public static ShowperfCategory ActiveCategory
    {
      get => activeCategory;
      set
      {
        activeCategory = value;
        View.SetCategory(value);
        Window.Reset();
        if (value == ShowperfCategory.None)
        {
          CaptureFrom.Clear();
          captureById = false;
        }
      }
    }
    public static HashSet<SubmarineType> CaptureFrom = new HashSet<SubmarineType>()
    {
      // SubmarineType.Player,
    };

    public static bool captureById = false;
    public static bool CaptureById
    {
      get => captureById;
      set
      {
        captureById = value;
        Window.Reset();
      }
    }

    public static CaptureWindow Window;
    public static WindowView View;

    public static double TicksToMs = 1000.0 / Stopwatch.Frequency;


    public void Initialize()
    {
      harmony = new Harmony("showperf");

      findModFolder();
      if (ModDir.Contains("LocalMods"))
      {
        debug = true;
        log($"found {ModName} in LocalMods, debug: {debug}");
      }

      addCommands();

      Window = new CaptureWindow(duration: 3, fps: 30);
      View = new WindowView();

      GameMain.PerformanceCounter.DrawTimeGraph = new Graph(1000);

      patchAll();

      if (debug) ActiveCategory = ShowperfCategory.ItemUpdate;

      info($"{ModName} compiled!");
    }

    public void patchAll()
    {
      harmony.Patch(
        original: typeof(MapEntity).GetMethod("UpdateAll", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("MapEntity_UpdateAll_Replace"))
      );

      harmony.Patch(
        original: typeof(Character).GetMethod("UpdateAll", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("Character_UpdateAll_Replace"))
      );

      harmony.Patch(
        original: typeof(Submarine).GetMethod("DrawFront", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("Submarine_DrawFront_Replace"))
      );

      harmony.Patch(
        original: typeof(Submarine).GetMethod("DrawBack", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("Submarine_DrawBack_Replace"))
      );

      harmony.Patch(
        original: typeof(LevelObjectManager).GetMethod("DrawObjects", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("LevelObjectManager_DrawObjects_Replace"))
      );

      harmony.Patch(
        original: typeof(LevelRenderer).GetMethod("DrawBackground", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("LevelRenderer_DrawBackground_Replace"))
      );

      harmony.Patch(
        original: typeof(Item).GetMethod("Update", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("Item_Update_Replace"))
      );


      harmony.Patch(
        original: typeof(Camera).GetMethod("MoveCamera", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("Camera_MoveCamera_Prefix"))
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

      Window.Dispose();
      Window = null;

      View.Dispose();
      View = null;

      removeCommands();
    }

    public void findModFolder()
    {
      bool found = false;

      foreach (ContentPackage p in ContentPackageManager.EnabledPackages.All)
      {
        if (p.Name.Contains(ModName))
        {
          found = true;
          ModDir = Path.GetFullPath(p.Dir);
          break;
        }
      }

      if (!found) err($"Couldn't find {ModName} mod folder");
    }
  }
}