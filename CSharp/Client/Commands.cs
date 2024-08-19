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

      // addedCommands.Add(new DebugConsole.Command("showperf_dump_items", "", (string[] args) =>
      // {
      //   using (StreamWriter writer = new StreamWriter("Showperf_dump_items.txt", false))
      //   {

      //   }
      // }));

      addedCommands.Add(new DebugConsole.Command("showperf", "showperf [category]", (string[] args) =>
      {
        if (args.Length == 0)
        {
          vanillaShowperfCommand.Execute(args);
          return;
        }

        if (Enum.TryParse<ShowperfCategories>(args[0], out ShowperfCategories c))
        {
          activeCategory = activeCategory == c ? ShowperfCategories.none : c;
        }

        if (activeCategory != ShowperfCategories.none) window.Reset();
      }, () => new string[][] { Enum.GetValues<ShowperfCategories>().Select(c => $"{c}").ToArray() }));


      addedCommands.Add(new DebugConsole.Command("showperf_accumulate", "toggles between average and sum", (string[] args) =>
      {
        window.Accumulate = !window.Accumulate;
        log($"window accumulate: {window.Accumulate}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_freeze", "", (string[] args) =>
      {
        view.frozen = !view.frozen;
        log($"view frozen: {view.frozen}");
      }));



      addedCommands.Add(new DebugConsole.Command("showperf_track", "toggles tracking of some ID", (string[] args) =>
      {
        if (args.Length > 0)
        {
          if (!view.tracked.Contains(args[0]))
          {
            if (view.tracked.Add(args[0])) log($"{args[0]} tracked");
            else log($"{args[0]} not found :(");
          }
          else
          {
            if (view.tracked.Remove(args[0])) log($"{args[0]} untracked");
            else log($"{args[0]} not found :(");
          }
        }
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_untrack", "", (string[] args) =>
      {
        if (args.Length > 0)
        {
          if (args[0] == "all")
          {
            view.tracked.Clear();
            return;
          }

          if (view.tracked.Remove(args[0])) log($"{args[0]} untracked");
          else log($"{args[0]} not found :(");

        }
      }, () => new string[][] { view.tracked.ToArray().Append("all").ToArray() }));

      addedCommands.Add(new DebugConsole.Command("showperf_duration", "", (string[] args) =>
      {
        if (args.Length > 0 && double.TryParse(args[0], out double d))
        {
          window.Duration = d;
        }

        log($"window.Duration: {window.Duration}");
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_fps", "", (string[] args) =>
      {
        if (args.Length > 0 && int.TryParse(args[0], out int fps))
        {
          window.FPS = fps;
        }

        log($"window.fps: {window.FPS}");
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