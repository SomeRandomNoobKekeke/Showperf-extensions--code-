using Barotrauma;
using ShowperfExtensions;
using CUILibs;
using System.Reflection;
using System.Runtime.CompilerServices;
using CursedUI;
using Barotrauma.Plugins;
using HarmonyLib;

namespace ShowperfPatches;

public partial class Mod
{
  public void Experiment()
  {
    // var plugin = PluginLoader.LoadedPlugins.First(p => p.Metadata.Name == "HomeAndHarborClient");
    // Type reactorType = plugin.Assembly.GetType("HomeAndHarbor.SingularityReactor");


    // harmony.Patch(
    //   original: reactorType.GetMethod("Update"),
    //   prefix: new HarmonyMethod(typeof(FunnyDLCPatch).GetMethod("FunnyPatch"))
    // );
  }
}