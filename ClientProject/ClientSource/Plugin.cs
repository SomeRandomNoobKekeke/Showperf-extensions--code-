using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using LightInject;

namespace ShowperfExtensions;

public partial class Plugin
{
  public static Plugin? Instance { get; private set; }

  public static ILogger FrameLogger => Instance?.ShowperfFrame?.Logger;

  public static CUIPalettes Palettes { get; } = new();

  public static CaptureCore Capture { get; set; } = Get<CaptureCore>();
  public CustomPatchRepo CustomPatchRepo { get; } = Get<CustomPatchRepo>();
  public PatchInstaller PatchInstaller { get; } = Get<PatchInstaller>();
  public ShowperfFrame ShowperfFrame { get; } = Get<ShowperfFrame>();


  public PluginSettings PluginSettings { get; } = Get<PluginSettings>();

  public TypeExplorer TypeExplorer { get; } = new TypeExplorer(typeof(GameMain).Assembly);

  public void Init()
  {
    CUI.Logger.Log("ShowperfExtensions compiled");

    Instance = this;

    Capture.AttachToGame();
    AddCommands();

    UTest.CollapseTestPackIfSucceed = false;
    UTest.ScanThisAssembly();
    UTest.TryRunPrevTest();


    Palettes.Primary = CUIPalette.FromColor(new Color(92, 92, 92, 255));
    Palettes.Secondary = CUIPalette.FromColor(new Color(0, 191, 221, 255));
    Palettes.Tertiary = CUIPalette.FromColor(new Color(0, 113, 183, 255));
    Palettes.Quaternary = CUIPalette.FromColor(new Color(32, 32, 32, 255));


    ShowperfFrame.Open();
    ShowperfFrame.MethodsPage.Current = typeof(Item).GetMethod("Update")!;

    Utils.RunWithDelay(Experiment, 500);
  }

  public void Dispose()
  {
    // SaveData(); // vanilla plugins normally don't get disposed so all data should be saved on edit


    Capture.DetachFromGame();
    Capture = null;

    PatchInstaller.RemoveAllPatches();

    ServiceContainer.Dispose();
    ServiceContainer = null;

    Instance = null;
  }

  public void OnContentLoaded()
  {

  }
}