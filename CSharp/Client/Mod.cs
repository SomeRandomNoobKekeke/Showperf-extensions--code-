using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;

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

    public bool debug = false;

    public void Initialize()
    {
      mod = this;
      info($"{ModName} Initialized");
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

    public void Dispose()
    {

    }
  }
}