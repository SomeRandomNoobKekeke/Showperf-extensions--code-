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
      AddedCommands.Add(new DebugConsole.Command("showperf_frames|s_frames", "", Showperf_Frames));
      AddedCommands.Add(new DebugConsole.Command("showperf_fps|s_fps", "", Showperf_FPS));
      AddedCommands.Add(new DebugConsole.Command("showperf_highlight|s_highlight|s_h", "", Showperf_Highlight));
      AddedCommands.Add(new DebugConsole.Command("showperf_track|s_track|s_t", "", Showperf_Track));
      AddedCommands.Add(new DebugConsole.Command("showperf_ignore|s_ignore|s_i", "", Showperf_Ignore));
      AddedCommands.Add(new DebugConsole.Command("showperf_fakelag|s_fakelag", "showperf_fakelag ticks [draw|update]", Showperf_Fakelag, () => new string[][] { new string[0], new string[] { "update", "draw" } }));
      AddedCommands.Add(new DebugConsole.Command("showperf_exposure|s_exposure", "showperf_exposure size [graph]", Showperf_Exposure, () => new string[][] { new string[0], new string[] { "DrawTimeGraph", "UpdateTimeGraph" } }));


      AddedCommands.Add(new DebugConsole.Command("printcolors", "", (string[] args) =>
      {
        foreach (PropertyInfo prop in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public))
        {
          log(prop, (Color)prop.GetValue(null));
        }
      }));

      AddedCommands.Add(new DebugConsole.Command("memoryusage", "", (string[] args) => PrintMemoryUsage()));


      AddedCommands.Add(new DebugConsole.Command("showperf_freeze|s_freeze", "", (string[] args) =>
      {
        Capture.Frozen = !Capture.Frozen;
        log($"Capture.Frozen = {Capture.Frozen}");
      }));

      AddedCommands.Add(new DebugConsole.Command("showperf_dump|s_dump", "", Showperf_Dump));

      if (Debug)
      {
        AddedCommands.Add(new DebugConsole.Command("showperf_die|s_die", "", (string[] args) =>
        {
          showperf = null;
        }));

        AddedCommands.Add(new DebugConsole.Command("showperf_destroy|s_destroy", "", (string[] args) =>
        {
          Instance = null;
        }));
      }

      DebugConsole.Commands.AddRange(AddedCommands);
    }

    public static void Showperf_Frames(string[] args)
    {
      if (args.Length > 0 && int.TryParse(args[0], out int frames))
      {
        Capture.Frames = frames;
      }

      log($"Capture.Frames = {Capture.Frames}");
    }
    public static void Showperf_FPS(string[] args)
    {
      if (args.Length > 0 && double.TryParse(args[0], out double fps))
      {
        Showperf.FPS = fps;
      }

      log($"Showperf.FPS = {Showperf.FPS}");
    }

    public static void Showperf_Highlight(string[] args)
    {
      if (args.Length == 0)
      {
        Showperf.TickList.Highlighted.Clear();
        log("highlight cleared");
        return;
      }

      if (Showperf.TickList.ToggleHighlight(args[0]))
      {
        log($"{args[0]} unhighlighted");
      }
      else
      {
        log($"{args[0]} highlighted");
      }
    }

    public static void Showperf_Track(string[] args)
    {
      if (args.Length == 0)
      {
        Showperf.TickList.Tracked.Clear();
        log("all untracked");
        return;
      }

      if (Showperf.TickList.ToggleTracking(args[0]))
      {
        log($"{args[0]} untracked");
      }
      else
      {
        log($"{args[0]} tracked");
      }
    }

    public static void Showperf_Ignore(string[] args)
    {
      if (args.Length == 0)
      {
        Showperf.TickList.Ignored.Clear();
        log("all unignored");
        return;
      }

      if (Showperf.TickList.ToggleIgnore(args[0]))
      {
        log($"{args[0]} unignored");
      }
      else
      {
        log($"{args[0]} ignored");
      }
    }

    public static void Showperf_Fakelag(string[] args)
    {
      if (args.Length != 0)
      {
        long ticks = 0;
        if (long.TryParse(args[0], out ticks))
        {
          if (args.ElementAtOrDefault(1) != "update") Capture.DrawFakeLag = new TimeSpan(ticks);
          if (args.ElementAtOrDefault(1) != "draw") Capture.UpdateFakeLag = new TimeSpan(ticks);
        }
      }

      log($"Draw: {Capture.DrawFakeLag}");
      log($"Update: {Capture.UpdateFakeLag}");
    }

    public static void Showperf_Exposure(string[] args)
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
    }

    public static void Showperf_Dump(string[] args)
    {
      string path = args.ElementAtOrDefault(0) ?? ShowperfLogPath;

      try
      {
        using (StreamWriter writer = new StreamWriter(path, false))
        {
          foreach (UpdateTicksView t in Showperf.TickList.Values)
          {
            writer.WriteLine(t.Name);
          }
        }
        log($"Saved to {path}");
      }
      catch (Exception e)
      {
        log(e.Message);
      }
    }

    public void RemoveCommands()
    {
      AddedCommands.ForEach(c => DebugConsole.Commands.Remove(c));
      AddedCommands.Clear();
      AddedCommands = null;
    }

    public static void PermitCommands(Identifier command, ref bool __result)
    {
      if (Instance.AddedCommands.Any(c => c.Names.Contains(command.Value))) __result = true;
    }


    /// <summary>
    /// Apparently i need this, coz it's the only way to clear commands from static context
    /// </summary>
    public static void StaticRemoveCommands()
    {
      string[] AddedCommands = new string[] { "showperf_frames", "showperf_fps", "showperf_highlight", "showperf_track", "showperf_ignore", "showperf_fakelag", "showperf_exposure", "printcolors", "memoryusage", "showperf_freeze", "showperf_dump", "showperf_die", "showperf_destroy" };

      foreach (string name in AddedCommands)
      {
        DebugConsole.Commands.RemoveAll(c => c.Names.Contains(name));
      }
    }
  }
}