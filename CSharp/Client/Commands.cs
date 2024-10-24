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
            // if (!GameMain.ShowPerf) ActiveCategory = ShowperfCategory.None;
            return;
          }

          if (args[0].Equals("id", StringComparison.OrdinalIgnoreCase))
          {
            Capture.GlobalByID = !Capture.GlobalByID;
            log($"Capture.GlobalByID: {Capture.GlobalByID}");
            return;
          }

          if (Enum.TryParse<SubType>(args[0], out SubType sub))
          {
            Window.CaptureFrom = sub;
            log($"Window.CaptureFrom = {Window.CaptureFrom}");
            return;
          }

          CaptureState cs = Capture.GetByName(args[0]);
          if (cs != null)
          {
            cs.ToggleIsActive();
            log($"{cs.Category}.IsActive = {cs.IsActive}");
            return;
          }

        }, () => new string[][] {
            Capture.GetAllNames()
            .Concat(Enum.GetValues<SubType>().Select(c => c.ToString()))
            .Append("id").ToArray()
          }
        )
      );


      addedCommands.Add(new DebugConsole.Command("showperf_accumulate|s_a", "toggles between mean and sum", (string[] args) =>
      {
        Window.Mode = Window.Mode switch
        {
          CaptureWindowMode.Sum => CaptureWindowMode.Mean,
          CaptureWindowMode.Mean => CaptureWindowMode.Sum,
        };

        log($"Window.Mode = {Window.Mode}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_freeze|s_freeze|s_f", "", (string[] args) =>
      {
        Window.Frozen = !Window.Frozen;
        log($"Window.Frozen = {Window.Frozen}");
      }));


      addedCommands.Add(new DebugConsole.Command("showperf_track|s_track|s_t", "toggles tracking of some ID", (string[] args) =>
      {
        if (args.Length > 0)
        {
          if (!Showperf.TickList.Tracked.Contains(args[0]))
          {
            if (Showperf.TickList.Tracked.Add(args[0])) log($"{args[0]} Tracked");
            else log($"{args[0]} not found :(");
          }
          else
          {
            if (Showperf.TickList.Tracked.Remove(args[0])) log($"{args[0]} Untracked");
            else log($"{args[0]} not found :(");
          }
        }
      }, () => new string[][] { Showperf.TickList.Values.Select(t => t.Name).ToArray() }));

      addedCommands.Add(new DebugConsole.Command("showperf_untrack|s_untrack", "", (string[] args) =>
      {
        if (args.Length == 0 || args[0] == "all")
        {
          Showperf.TickList.Tracked.Clear();
          log("Untracked all");
          return;
        }

        if (args.Length > 0)
        {
          if (Showperf.TickList.Tracked.Remove(args[0])) log($"{args[0]} Untracked");
          else log($"{args[0]} not found :(");

        }
      }, () => new string[][] { Showperf.TickList.Tracked.ToArray().Append("all").ToArray() }));

      addedCommands.Add(new DebugConsole.Command("showperf_duration|s_duration", "", (string[] args) =>
      {
        if (args.Length > 0 && double.TryParse(args[0], out double d))
        {
          Window.Duration = d;
        }

        log($"Window.Duration = {Window.Duration}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_units|s_units", "toggles between ticks and ms", (string[] args) =>
      {
        Showperf.TickList.ShowInMs = !Showperf.TickList.ShowInMs;
        log($"ShowInMs = {Showperf.TickList.ShowInMs}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_fps|s_fps", "", (string[] args) =>
      {
        if (args.Length > 0 && int.TryParse(args[0], out int fps))
        {
          Window.FPS = fps;
        }

        log($"Window.FPS = {Window.FPS}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_frames|s_frames", "", (string[] args) =>
      {
        if (args.Length > 0 && int.TryParse(args[0], out int frames))
        {
          Window.Frames = frames;
        }

        log($"Window.Frames = {Window.Frames}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_exposure|s_exposure", "showperf_exposure size [graph]", (string[] args) =>
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

      if (vanillaShowperfCommand != null)
      {
        DebugConsole.Commands.Insert(0, vanillaShowperfCommand);
      }
      vanillaShowperfCommand = null;
    }

    public static void permitCommands(Identifier command, ref bool __result)
    {
      if (addedCommands.Any(c => c.Names.Contains(command.Value))) __result = true;
    }
  }
}