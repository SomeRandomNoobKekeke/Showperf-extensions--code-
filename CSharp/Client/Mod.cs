using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using CrabUI;

using System.Runtime.CompilerServices;
[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
  {
    public static string ModName = "Showperf extensions";
    public static Plugin Mod;

    public string ModDir = "";
    public string ModVersion = "1.0.0";
    public bool Debug;
    public Harmony __harmony;
    public static Harmony harmony => Mod.__harmony;

    public static CaptureClass Capture;
    public static CUIShowperf Showperf;

    public void Initialize()
    {
      Mod = this;
      FindModFolder();

      if (ModDir.Contains("LocalMods"))
      {
        Debug = true;
        info($"found {ModName} in LocalMods, debug: {Debug}");
      }

      __harmony = new Harmony("showperf");

      CaptureState.FromHash.Clear();
      LightSource_Parent.Clear();
      MapButton.Buttons.Clear();

      Capture = new CaptureClass();
      Capture.LoadFromFile();


      //GameMain.PerformanceCounter.DrawTimeGraph = new Graph(1000);

      CUI.Initialize();

      Showperf = new CUIShowperf()
      {
        Absolute = new CUINullRect(null, null, 350, 550),
      };

      Showperf.CreateGUI();

      Showperf.Absolute = new CUINullRect(
        CUIAnchor.GetChildPos(CUI.Main.Real, new Vector2(1, 0.5f), new Vector2(-1, 0), Showperf.Absolute.Size),
        Showperf.Absolute.Size
      );

      Showperf.States["init"] = Showperf.Clone();
      Showperf.OnUpdate += () => Showperf.Update();

      CUI.Main["showperfButton"] = new CUIButton("SHOWPERF")
      {
        Anchor = new Vector2(1, 0.5f),
        Font = GUIStyle.MonospacedFont,
        TextScale = 0.8f,
        Wrap = true,
        AddOnMouseDown = (e) =>
        {
          CUI.Main["showperfButton"].Revealed = false;
          Showperf.Open();
        },
        InactiveColor = new Color(0, 0, 32, 128),
        MouseOverColor = new Color(0, 0, 64, 128),
        MousePressedColor = new Color(0, 0, 128, 128),
      };

      Showperf.OnClose += () => CUI.Main["showperfButton"].Revealed = true;

      //CUI.Main["showperfButton"].Click();
      //Showperf.Pages.Open(Showperf.Map);


      //CUIMain.Load(CUITest.NestedScale);

      //CUIDebugWindow.Open();


      PatchAll();

      AddCommands();

      FindLightSourceParents.Find();
      FindLevelTriggerParents.Find();

      info($"{ModName} Initialized");
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

    public void Dispose()
    {
      RemoveCommands();
      CUI.Dispose();
      info($"{ModName} Disposed");
    }
  }
}