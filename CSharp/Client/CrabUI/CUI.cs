

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;

namespace CrabUI
{
  public static partial class CUI
  {
    public static CUIMainComponent Main => CUIMainComponent.Main;
    public static CUIInput Input = new CUIInput();

    public static string ModDir => ShowPerfExtensions.Plugin.Mod.ModDir;
    public static string IgnoreDir => ModDir + "/Ignore";

    public static void log(object msg, Color? cl = null)
    {
      cl ??= Color.Yellow;
      LuaCsLogger.LogMessage($"{msg ?? "null"}", cl * 0.8f, cl);
    }

    private static Harmony harmony;
    public static bool Initialized;
    public static void Initialize()
    {
      if (Initialized) return;

      harmony = new Harmony("crabui");
      patchAll();
      AddCommands();

      Initialized = true;
    }

    public static void Dispose()
    {
      RemoveCommands();
      Initialized = false;
    }

    private static void patchAll()
    {
      harmony.Patch(
        original: typeof(GUI).GetMethod("Draw", AccessTools.all),
        prefix: new HarmonyMethod(typeof(CUI).GetMethod("CUIDraw", AccessTools.all))
      );

      harmony.Patch(
        original: typeof(GameMain).GetMethod("Update", AccessTools.all),
        postfix: new HarmonyMethod(typeof(CUI).GetMethod("CUIUpdate", AccessTools.all))
      );

      harmony.Patch(
        original: typeof(GUI).GetMethod("UpdateMouseOn", AccessTools.all),
        postfix: new HarmonyMethod(typeof(CUI).GetMethod("CUIBlockClicks", AccessTools.all))
      );

      harmony.Patch(
        original: typeof(Camera).GetMethod("MoveCamera", AccessTools.all),
        prefix: new HarmonyMethod(typeof(CUI).GetMethod("CUIBlockScroll", AccessTools.all))
      );
    }

    private static void CUIUpdate(GameTime gameTime)
    {
      try { Main.Update(gameTime.TotalGameTime.TotalSeconds); }
      catch (Exception e) { CUI.log($"CUI: {e}", Color.Yellow); }
    }

    private static void CUIDraw(SpriteBatch spriteBatch)
    {
      try { Main.Draw(spriteBatch); }
      catch (Exception e) { CUI.log($"CUI: {e}", Color.Yellow); }
    }

    private static void CUIBlockClicks(ref GUIComponent __result)
    {
      if (GUI.MouseOn == null && Main.MouseOn != null && Main.MouseOn != Main) GUI.MouseOn = CUIComponent.dummyComponent;
    }

    private static void CUIBlockScroll(float deltaTime, ref bool allowMove, ref bool allowZoom, bool allowInput, bool? followSub)
    {
      if (GUI.MouseOn == CUIComponent.dummyComponent) allowZoom = false;
    }
  }
}