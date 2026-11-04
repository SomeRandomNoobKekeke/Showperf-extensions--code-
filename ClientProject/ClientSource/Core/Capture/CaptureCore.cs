
using System.Reflection;
using Barotrauma;
using HarmonyLib;

namespace ShowperfExtensions;

public class CaptureCore
{
  public enum WhenToRotateOptions { Update, Draw }

  private static BindingFlags Pls => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
  private Harmony Harmony { get; } = new Harmony("showperf.capture");

  public static WhenToRotateOptions WhenToRotate = WhenToRotateOptions.Update;


  public static bool Active { get; set; } = true;

  // 1 window to capture them all
  public CaptureWindow Window { get; } = new();




  private static void GameScreen_Update_Postfix()
  {
    if (!Active || WhenToRotate != WhenToRotateOptions.Update) return;
    Plugin.Capture?.Window.Rotate();
  }

  private static void GameScreen_Draw_Postfix()
  {
    if (!Active || WhenToRotate != WhenToRotateOptions.Draw) return;
    Plugin.Capture?.Window.Rotate();
  }

  internal void AttachToGame()
  {
    Harmony.Patch(
      original: typeof(GameScreen).GetMethod("Update", Pls),
      postfix: new HarmonyMethod(typeof(CaptureCore).GetMethod("GameScreen_Update_Postfix", Pls))
    );

    Harmony.Patch(
      original: typeof(GameScreen).GetMethod("Draw", Pls),
      postfix: new HarmonyMethod(typeof(CaptureCore).GetMethod("GameScreen_Draw_Postfix", Pls))
    );
  }


  internal void Dispose()
  {
    Harmony.UnpatchSelf();
  }
}

