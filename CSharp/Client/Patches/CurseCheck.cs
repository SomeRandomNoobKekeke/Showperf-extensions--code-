using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using CrabUI;
using Microsoft.Xna.Framework;
namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
  {
    /// <summary>
    /// Attempt to fix the mod
    /// </summary>
    public static void DispelCurse()
    {
      if (Showperf == null)
      {
        if (Instance != null)
        {
          log($"Showperf GUI just died for no reason\nAttempting restart", new Color(100, 0, 0));
          CUI.Main?.RemoveAllChildren();
          Instance.CreateGUI();
        }
        else
        {
          log($"Showperf mod is dead\nAttempting clean up", new Color(100, 0, 0));
          harmony.UnpatchAll(harmony.Id);
          StaticRemoveCommands();
          CUI.Dispose();
        }
      }
    }

  }
}