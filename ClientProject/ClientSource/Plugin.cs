using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class Plugin
{
  public static Plugin? Instance { get; private set; }
  public SingletonsContainer Singletons { get; private set; } = new();




  public ShowperfFrame ShowperfFrame { get; } = new();


  public void Init()
  {
    Instance = this;

    CUI.Logger.Log("ShowperfExtensions compiled");

    // PluginCommands.Add("showperf", (args) => ShowperfFrame.Open());

    LoadData();

    CustomPatches.Add(new PatchInfo(
      "bruh",
      typeof(Item).GetMethod("Update")!,
      typeof(Item_Patch).GetMethod("Update")!,
      HarmonyLib.HarmonyPatchType.Postfix
    ));

    ShowperfFrame.Open();
    UTest.ScanThisAssembly();
    UTest.TryRunPrevTest();



    // CUIPageTestingFrame.Instance.Open();
    // CUIPageTestingFrame.Instance.ScanForCUIPages();

    Utils.RunWithDelay(Experiment, 500);
    // Experiment();

    PluginCommands.Add("print_assemblies", (args) =>
    {
      CUI.Logger.Log(Logger.Wrap.IEnumerable(AppDomain.CurrentDomain.GetAssemblies(), true));
    });
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