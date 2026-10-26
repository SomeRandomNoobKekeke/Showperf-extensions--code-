using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class Plugin
{
  public static Plugin? Instance { get; private set; }
  public SingletonsContainer Singletons { get; private set; } = new();

  public ShowperfFrame ShowperfFrame { get; private set; }


  public void Init()
  {
    Instance = this;
    LoadData();

    CUI.Palettes.Primary = CUIPalette.FromColor(new Color(92, 92, 92, 255));
    CUI.Palettes.Secondary = CUIPalette.FromColor(new Color(0, 191, 221, 255));
    CUI.Palettes.Tertiary = CUIPalette.FromColor(new Color(0, 113, 183, 255));
    CUI.Palettes.Quaternary = CUIPalette.FromColor(new Color(255, 0, 0, 255));

    ShowperfFrame = new();

    CUI.Logger.Log("ShowperfExtensions compiled");

    // PluginCommands.Add("showperf", (args) => ShowperfFrame.Open());



    CustomPatches.Add(new PatchInfo(
      "Deep item analysis",
      typeof(Item).GetMethod("Update")!,
      typeof(Item_Patch).GetMethod("Item_Update_Replace")!,
      HarmonyLib.HarmonyPatchType.Prefix
    ));


    UTest.ScanThisAssembly();
    UTest.TryRunPrevTest();



    // CUIPageTestingFrame.Instance.Open();
    // CUIPageTestingFrame.Instance.ScanForCUIPages();

    Utils.RunWithDelay(Experiment, 500);
    // Experiment();

    Singletons.Init();
    AddCommands();

    ShowperfFrame.Open();
    ShowperfFrame.MethodsPage.Current = typeof(Item).GetMethod("Update")!;
  }

  public void Dispose()
  {
    Singletons.Dispose();
    Singletons = null;
    Instance = null;
  }

  public void OnContentLoaded()
  {

  }
}