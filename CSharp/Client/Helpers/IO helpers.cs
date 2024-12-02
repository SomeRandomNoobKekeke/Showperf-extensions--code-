using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.IO;


namespace ShowPerfExtensions
{
  partial class Plugin : IAssemblyPlugin
  {

    public static string ShowperfLogPath = "Showperf.log";
    public void FindModFolder()
    {
      bool found = false;

      foreach (ContentPackage p in ContentPackageManager.EnabledPackages.All)
      {
        if (p.Name.Contains(ModName))
        {
          found = true;
          ModDir = Path.GetFullPath(p.Dir);
          ModVersion = p.ModVersion;
          break;
        }
      }

      if (!found) error($"Couldn't find {ModName} mod folder");
    }
  }
}