using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using LightInject;

namespace ShowperfExtensions;

public partial class Plugin
{
  public static Plugin? Instance { get; private set; }

  public static ILogger FrameLogger => Instance!.ShowperfFrame.Logger;

  public ShowperfFrame ShowperfFrame { get; } = Get<ShowperfFrame>();
  public static CaptureCore Capture { get; set; } = Get<CaptureCore>();
  public PatchInstaller PatchInstaller { get; } = Get<PatchInstaller>();

  public void Init()
  {
    CUI.Logger.Log("ShowperfExtensions compiled");
    Instance = this;

    Capture.AttachToGame();
    AddCommands();

    UTest.ScanThisAssembly();
    UTest.TryRunPrevTest();

    CUI.Palettes.Primary = CUIPalette.FromColor(new Color(92, 92, 92, 255));
    CUI.Palettes.Secondary = CUIPalette.FromColor(new Color(0, 191, 221, 255));
    CUI.Palettes.Tertiary = CUIPalette.FromColor(new Color(0, 113, 183, 255));
    CUI.Palettes.Quaternary = CUIPalette.FromColor(new Color(255, 0, 0, 255));


    Get<CustomPatchRepo>().Add(new PatchInfo(
      "Deep item analysis",
      typeof(Item).GetMethod("Update")!,
      typeof(Item_Patch).GetMethod("Item_Update_Replace")!,
      HarmonyLib.HarmonyPatchType.Prefix
    ));


    LoadData();

    ShowperfFrame.Open();
    ShowperfFrame.MethodsPage.Current = typeof(Item).GetMethod("Update")!;

    Utils.RunWithDelay(Experiment, 500);
  }

  public void Dispose()
  {
    SaveData();
    Capture.DetachFromGame();
    PatchInstaller.RemoveAllPatches();

    ServiceContainer.Dispose();
    ServiceContainer = null;

    Instance = null;
  }

  public void OnContentLoaded()
  {

  }
}