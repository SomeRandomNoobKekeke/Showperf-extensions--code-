using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public partial class Mod
{
  public static Mod Instance { get; private set; }

  public Logger Logger { get; } = new();

  public static Patcher Patcher { get; private set; } = new();
  public ShowperfFrame ShowperfFrame { get; } = new();

  public void AddPatch(PatchInfo info) => ShowperfFrame.CustomPatchesPage.AddPatch(info);

  public void Init()
  {
    Instance = this;
    Logger.Log("ShowperfExtensions compiled");

    // PluginCommands.Add("showperf", (args) => ShowperfFrame.Open());

    LoadData();

    AddPatch(new PatchInfo(
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
      Logger.Log(Logger.Wrap.IEnumerable(AppDomain.CurrentDomain.GetAssemblies(), true));
    });
  }

  public void Dispose()
  {
    Patcher.RemoveAllPatches();
    Patcher = null;
    Instance = null;
  }

  public void OnContentLoaded()
  {

  }
}