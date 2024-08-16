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
    public static void addCommands()
    {
      addedCommands ??= new List<DebugConsole.Command>();

      // addedCommands.Add(new DebugConsole.Command("showperf_dump_items", "", (string[] args) =>
      // {
      //   using (StreamWriter writer = new StreamWriter("Showperf_dump_items.txt", false))
      //   {

      //   }
      // }));

      addedCommands.Add(new DebugConsole.Command("showperf_items", "", (string[] args) =>
      {
        DrawItemUpdateTimes = !DrawItemUpdateTimes;

        if (DrawItemUpdateTimes) window.Clear();
      }));

      addedCommands.Add(new DebugConsole.Command("showperf_freeze", "", (string[] args) =>
      {
        window.frozen = !window.frozen;
      }));

      addedCommands.ForEach(c => DebugConsole.Commands.Add(c));
    }



    public static void removeCommands()
    {
      addedCommands.ForEach(c => DebugConsole.Commands.RemoveAll(which => which.Names.Contains(c.Names[0])));

      addedCommands.Clear();
      addedCommands = null;
    }
  }
}