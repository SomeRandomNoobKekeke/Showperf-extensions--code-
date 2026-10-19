using Barotrauma;
using CursedUI;
using CUILibs;

namespace ShowperfExtensions;

public partial class Mod
{
  public Logger Logger { get; } = new();

  public static Patcher Patcher { get; private set; } = new();
  public ShowperfFrame ShowperfFrame { get; } = new();

  public void AddPatch(PatchInfo info) => ShowperfFrame.CustomPatchesPage.AddPatch(info);

  public void Init()
  {
    Logger.Log("ShowperfExtensions compiled");

    // PluginCommands.Add("showperf", (args) => ShowperfFrame.Open());

    AddPatch(new PatchInfo(
      typeof(Item).GetMethod("Update")!,
      typeof(Item_Patch).GetMethod("Update")!,
      HarmonyLib.HarmonyPatchType.Postfix
    ));

    ShowperfFrame.Open();
    UTest.ScanThisAssembly();
    UTest.TryRunPrevTest();

    Utils.RunWithDelay(Experiment, 500);

    PluginCommands.Add("print_assemblies", (args) =>
    {
      Logger.Log(Logger.Wrap.IEnumerable(AppDomain.CurrentDomain.GetAssemblies(), true));
    });
  }

  public void Dispose()
  {
    Patcher.RemoveAllPatches();
    Patcher = null;
  }

  public void OnContentLoaded()
  {

  }
}