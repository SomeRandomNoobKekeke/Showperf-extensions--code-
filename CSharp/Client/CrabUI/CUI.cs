

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
  public partial class CUI
  {
    public static CUI Instance;

    public static CUIMainComponent Main;
    public static CUIInput Input = new CUIInput();

    public static string ModDir => ShowPerfExtensions.Plugin.Instance.ModDir;
    public static string IgnoreDir => ModDir + "/Ignore";

    public static void log(object msg, Color? cl = null)
    {
      cl ??= Color.Yellow;
      LuaCsLogger.LogMessage($"{msg ?? "null"}", cl * 0.8f, cl);
    }

    private Harmony harmony;

    public static void Initialize()
    {
      if (Instance != null)
      {
        Dispose();
        Instance = null;
      }

      Instance = new CUI();
      Instance.harmony = new Harmony("crabui");

      CUIComponent.MaxID = 0;
      CUITypes.Clear();
      CUITypeMetaData.TypeMetaData.Clear();
      CUIComponent.ComponentsById.Clear();
      CUIDebugEventComponent.CapturedIDs.Clear();

      Main = new CUIMainComponent();

      Instance.PatchAll();
      Instance.AddCommands();
    }

    public static void Dispose()
    {
      Instance.RemoveCommands();
      Instance.harmony.UnpatchSelf();
    }

    private void PatchAll()
    {
      //harmony.UnpatchAll("crabui");

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