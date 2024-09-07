using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public void Initialize()
    {

    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

    public void Dispose()
    {

    }
  }
}