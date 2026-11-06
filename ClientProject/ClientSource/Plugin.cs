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

  public static CaptureCore Capture { get; set; } = Get<CaptureCore>();
  public CustomPatchRepo CustomPatchRepo { get; } = Get<CustomPatchRepo>();
  public PatchInstaller PatchInstaller { get; } = Get<PatchInstaller>();
  public ShowperfFrame ShowperfFrame { get; } = Get<ShowperfFrame>();


  public static CUIPalettes Palettes => PluginSettings.Palettes;
  public static PluginSettings PluginSettings { get; private set; } = Get<PluginSettings>();

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



    Utils.RunWithDelay(Experiment, 500);
  }

  public void Dispose()
  {
    // SaveData(); // vanilla plugins normally don't get disposed so all data should be saved on edit
    PluginSettings = null;

    Capture.Dispose();
    Capture = null;

    PatchInstaller.Dispose();

    ServiceContainer.Dispose();
    ServiceContainer = null;

    Instance = null;
  }

  public void OnLoadCompleted()
  {
    if (ModStorage.TryGetValue("showperf frame was open", out bool yes))
    {
      if (yes) ShowperfFrame.Open();
    }
  }
  public void PreInitPatching() { }
}