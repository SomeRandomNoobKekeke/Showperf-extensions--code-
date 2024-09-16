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

    public void Initialize()
    {
      mod = this;

      Window = new CaptureWindow(duration: 3, fps: 30);
      CUI = new CUIMainComponent();
      Showperf = new CUIShowperf(0.7f, 0.1f, 0.3f, 0.8f);

      CUI.Append(Showperf);


      PatchAll();

      info($"{ModName} Initialized");
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

    public void Dispose()
    {
      info($"{ModName} Disposed");
    }
  }
}