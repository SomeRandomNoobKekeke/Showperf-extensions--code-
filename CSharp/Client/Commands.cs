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
  public partial class Mod : IAssemblyPlugin
  {
    public static List<DebugConsole.Command> addedCommands = new List<DebugConsole.Command>();

    public static DebugConsole.Command vanillaShowperfCommand;

    public static void addCommands()
    {
      addedCommands ??= new List<DebugConsole.Command>();

      vanillaShowperfCommand = DebugConsole.Commands.Find(c => c.Names.Contains("showperf"));
      DebugConsole.Commands.Remove(vanillaShowperfCommand);



      addedCommands.Add(new DebugConsole.Command("showperf|s", "showperf [category]", (string[] args) =>
      {
        if (args.Length == 0)
        {
          vanillaShowperfCommand.Execute(args);
          return;
        }

        if (args[0].Equals("id", StringComparison.OrdinalIgnoreCase))
        {
          CaptureById = !CaptureById;
          log($"CaptureById: {CaptureById}");
          return;
        }


        if (Enum.TryParse<ShowperfCategory>(args[0], out ShowperfCategory c))
        {
          ActiveCategory = ActiveCategory == c ? ShowperfCategory.None : c;
        }

      }, () => new string[][] { Enum.GetValues<ShowperfCategory>().Select(c => $"{c}").ToArray() }));


      addedCommands.Add(new DebugConsole.Command("showperf_accumulate", "toggles between average and sum", (string[] args) =>
      {
        Window.Accumulate = !Window.Accumulate;
        log($"Window.Accumulate: {Window.Accumulate}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_freeze", "", (string[] args) =>
      {
        Window.Frozen = !Window.Frozen;
        log($"Window.Frozen: {Window.Frozen}");
      }));


      addedCommands.Add(new DebugConsole.Command("showperf_track", "toggles tracking of some ID", (string[] args) =>
      {
        if (args.Length > 0)
        {
          if (!View.Tracked.Contains(args[0]))
          {
            if (View.Tracked.Add(args[0])) log($"{args[0]} Tracked");
            else log($"{args[0]} not found :(");
          }
          else
          {
            if (View.Tracked.Remove(args[0])) log($"{args[0]} Untracked");
            else log($"{args[0]} not found :(");
          }
        }
      }, () => new string[][] { View.getAllIds() }));

      addedCommands.Add(new DebugConsole.Command("showperf_untrack", "", (string[] args) =>
      {
        if (args.Length == 0 || args[0] == "all")
        {
          View.Tracked.Clear();
          log("untracked all");
          return;
        }

        if (args.Length > 0)
        {
          if (View.Tracked.Remove(args[0])) log($"{args[0]} Untracked");
          else log($"{args[0]} not found :(");

        }
      }, () => new string[][] { View.Tracked.ToArray().Append("all").ToArray() }));

      addedCommands.Add(new DebugConsole.Command("showperf_duration", "", (string[] args) =>
      {
        if (args.Length > 0 && double.TryParse(args[0], out double d))
        {
          Window.Duration = d;
        }

        log($"Window.Duration: {Window.Duration}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_units", "toggles between ticks and ms", (string[] args) =>
      {
        View.ShowInMs = !View.ShowInMs;
        log($"View.ShowInMs: {View.ShowInMs}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_fps", "", (string[] args) =>
      {
        if (args.Length > 0 && int.TryParse(args[0], out int fps))
        {
          Window.FPS = fps;
        }

        log($"Window.FPS: {Window.FPS}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_frames", "", (string[] args) =>
      {
        if (args.Length > 0 && int.TryParse(args[0], out int frames))
        {
          Window.Frames = frames;
        }

        log($"Window.Frames: {Window.Frames}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_exposure", "showperf_exposure size [graph]", (string[] args) =>
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


      addedCommands.Add(new DebugConsole.Command("printcolors", "", (string[] args) =>
      {
        foreach (PropertyInfo prop in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
          log(prop, (Color)prop.GetValue(null));
        }
      }));


      DebugConsole.Commands.InsertRange(0, addedCommands);
    }

    public static void removeCommands()
    {
      addedCommands.ForEach(c => DebugConsole.Commands.RemoveAll(which => which.Names.Contains(c.Names[0])));

      addedCommands.Clear();
      addedCommands = null;

      DebugConsole.Commands.Insert(0, vanillaShowperfCommand);
      vanillaShowperfCommand = null;
    }

    public static void permitCommands(Identifier command, ref bool __result)
    {
      if (addedCommands.Any(c => c.Names.Contains(command.Value))) __result = true;
    }
  }
}