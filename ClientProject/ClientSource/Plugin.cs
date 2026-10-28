using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using LightInject;

namespace ShowperfExtensions;

public partial class Plugin
{
  public static Plugin? Instance { get; private set; }



  public ShowperfFrame ShowperfFrame { get; set; }
  public static CaptureCore Capture { get; set; }
  public PatchInstaller PatchInstaller { get; set; }

  public void Init()
  {
    Instance = this;



    CUI.Palettes.Primary = CUIPalette.FromColor(new Color(92, 92, 92, 255));
    CUI.Palettes.Secondary = CUIPalette.FromColor(new Color(0, 191, 221, 255));
    CUI.Palettes.Tertiary = CUIPalette.FromColor(new Color(0, 113, 183, 255));
    CUI.Palettes.Quaternary = CUIPalette.FromColor(new Color(255, 0, 0, 255));



    Capture = Get<CaptureCore>();
    Capture.AttachToGame();

    CallGraphBuilder = Get<CallGraphBuilder>();
    CallGraphSaver = Get<CallGraphSaver>();
    CallGraphExplorer = Get<CallGraphExplorer>();

    CUI.Logger.Log("ShowperfExtensions compiled");

    // PluginCommands.Add("showperf", (args) => ShowperfFrame.Open());

    PatchInstaller = Get<PatchInstaller>();

    Get<CustomPatchRepo>().Add(new PatchInfo(
      "Deep item analysis",
      typeof(Item).GetMethod("Update")!,
      typeof(Item_Patch).GetMethod("Item_Update_Replace")!,
      HarmonyLib.HarmonyPatchType.Prefix
    ));

    LoadData();
    ShowperfFrame = Get<ShowperfFrame>();

    UTest.ScanThisAssembly();
    UTest.TryRunPrevTest();



    // CUIPageTestingFrame.Instance.Open();
    // CUIPageTestingFrame.Instance.ScanForCUIPages();

    Utils.RunWithDelay(Experiment, 500);
    // Experiment();

    AddCommands();



    ShowperfFrame.Open();
    ShowperfFrame.MethodsPage.Current = typeof(Item).GetMethod("Update")!;
  }

  public void Dispose()
  {
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