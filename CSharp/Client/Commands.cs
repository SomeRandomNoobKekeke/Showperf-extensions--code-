using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.IO;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
  {
    public List<DebugConsole.Command> AddedCommands = new List<DebugConsole.Command>();

    public void AddCommands()
    {
      AddedCommands.Add(new DebugConsole.Command("showperf_frames|s_frames", "", (string[] args) =>
      {
        if (args.Length > 0 && int.TryParse(args[0], out int frames))
        {
          Capture.Frames = frames;
        }

        log($"Capture.Frames = {Capture.Frames}");
      }));

      AddedCommands.Add(new DebugConsole.Command("showperf_fps|s_fps", "", (string[] args) =>
      {
        if (args.Length > 0 && double.TryParse(args[0], out double fps))
        {
          Showperf.FPS = fps;
        }

        log($"Showperf.FPS = {Showperf.FPS}");
      }));

      AddedCommands.Add(new DebugConsole.Command("showperf_exposure|s_exposure", "showperf_exposure size [graph]", (string[] args) =>
      {
        bool draw = true;
        bool update = true;

        if (args.Length > 1)
        {
          if (args[1] == "DrawTimeGraph") update = false;
          if (args[1] == "UpdateTimeGraph") draw = false;
        }

        if (args.Length > 0 && int.TryParse(args[0], out int ticks))
        {
          ticks = Math.Clamp(ticks, 10, 100000);

          if (draw) GameMain.PerformanceCounter.DrawTimeGraph = new Graph(ticks);
          if (update) GameMain.PerformanceCounter.UpdateTimeGraph = new Graph(ticks);
        }

        log($"GameMain.PerformanceCounter.DrawTimeGraph.values.Length: {GameMain.PerformanceCounter.DrawTimeGraph.values.Length}");
        log($"GameMain.PerformanceCounter.UpdateTimeGraph.values.Length: {GameMain.PerformanceCounter.UpdateTimeGraph.values.Length}");
      }, () => new string[][] { new string[0], new string[] { "DrawTimeGraph", "UpdateTimeGraph" } }));


      AddedCommands.Add(new DebugConsole.Command("printcolors", "", (string[] args) =>
      {
        foreach (PropertyInfo prop in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
          log(prop, (Color)prop.GetValue(null));
        }
      }));

      DebugConsole.Commands.AddRange(AddedCommands);
    }

    public void RemoveCommands()
    {
      AddedCommands.ForEach(c => DebugConsole.Commands.Remove(c));
      AddedCommands.Clear();
    }

    public static void PermitCommands(Identifier command, ref bool __result)
    {
      if (Mod.AddedCommands.Any(c => c.Names.Contains(command.Value))) __result = true;
    }
  }
}