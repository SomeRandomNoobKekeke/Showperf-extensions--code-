
using System.Reflection;
using Barotrauma;
using HarmonyLib;

namespace ShowperfExtensions;

public class CaptureCore
{
  private static BindingFlags Pls => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
  private Harmony Harmony { get; } = new Harmony("showperf.capture");

  public bool Active { get; set; } = true;
  public CaptureWindow Update { get; } = new();




  private static void GameScreen_Update_Postfix()
  {
    if (!Plugin.Capture?.Active == true) return;
    Plugin.Capture?.Update.Rotate();
  }

  internal void AttachToGame()
  {
    Harmony.Patch(
      original: typeof(GameScreen).GetMethod("Update", Pls),
      postfix: new HarmonyMethod(typeof(CaptureCore).GetMethod("GameScreen_Update_Postfix", Pls))
    );
  }

  internal void DetachFromGame()
  {
    Harmony.Unpatch(
      typeof(GameScreen).GetMethod("Update", Pls),
      typeof(CaptureCore).GetMethod("GameScreen_Update_Postfix", Pls)
    );
  }

  internal void Dispose()
  {
    Harmony.UnpatchSelf();
  }
}

