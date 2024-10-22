using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;

namespace CrabUI
{
  public static partial class CUI
  {
    public static List<DebugConsole.Command> AddedCommands = new List<DebugConsole.Command>();
    public static void AddCommands()
    {
      AddedCommands ??= new List<DebugConsole.Command>();

      AddedCommands.Add(new DebugConsole.Command("cui_debug", "", (string[] args) =>
      {
        if (CUIDebugWindow.Main == null)
        {
          CUIDebugWindow.Open();
        }
        else
        {
          CUIDebugWindow.Close();
        }
      }));

      DebugConsole.Commands.InsertRange(0, AddedCommands);
    }

    public static void RemoveCommands()
    {
      AddedCommands?.ForEach(c => DebugConsole.Commands.RemoveAll(which => which.Names.Contains(c.Names[0])));

      AddedCommands?.Clear();
    }

    // public static void PermitCommands(Identifier command, ref bool __result)
    // {
    //   if (AddedCommands.Any(c => c.Names.Contains(command.Value))) __result = true;
    // }
  }
}