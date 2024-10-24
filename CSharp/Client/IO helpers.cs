using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.Text.Json;
using System.IO;


namespace ShowPerfExtensions
{
  partial class Plugin : IAssemblyPlugin
  {
    public static void FindModFolder()
    {
      bool found = false;

      foreach (ContentPackage p in ContentPackageManager.EnabledPackages.All)
      {
        if (p.Name.Contains(ModName))
        {
          found = true;
          Mod.ModDir = Path.GetFullPath(p.Dir);
          Mod.ModVersion = p.ModVersion;
          break;
        }
      }

      if (!found) error($"Couldn't find {ModName} mod folder");
    }
  }
}