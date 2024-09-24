using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using CrabUI;

using System.Runtime.CompilerServices;
[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public static Harmony harmony = new Harmony("showperf");
    public static string ModName = "Showperf extensions";
    public static Mod mod;

    public bool debug = true;


    public static CaptureWindow Window;
    public static CUIShowperf Showperf;
    public CUIMainComponent CUI;


    public static void EnsureCategory(int cat) => Window.EnsureCategory(cat);
    public static void Capture(double ticks, int category, string name, int hash) => Window.AddTicks(new UpdateTicks(ticks, category, name, hash));
    public static void Capture(double ticks, int category, string name) => Window.AddTicks(new UpdateTicks(ticks, category, name));

    public void Initialize()
    {
      mod = this;

      GameMain.PerformanceCounter.DrawTimeGraph = new Graph(1000);
      Window = new CaptureWindow(duration: 3, fps: 30);
      CUI = new CUIMainComponent();
      Showperf = new CUIShowperf(0.6f, 0.1f, 0.4f, 0.8f);

      CUI.Append(Showperf);
      CUI.OnUpdate += () => Showperf.Update();

      Showperf.Capture.Toggle(CName.MapEntityDrawing);

      //CUI.Load(CUITest.ClickSounds);

      // CUI.OnUpdate += () => log($"{String.Format("{0:000000}", CUI.DrawTime)} {String.Format("{0:000000}", CUI.UpdateTime)}");

      PatchAll();
      addCommands();

      info($"{ModName} Initialized");
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

    public void Dispose()
    {
      removeCommands();
      info($"{ModName} Disposed");
    }
  }
}