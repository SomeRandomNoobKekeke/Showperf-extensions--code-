using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public enum CaptureCategory
    {
      ItemsUpdate, Characters, ItemsDrawing, LevelObjectsDrawing,
      OtherLevelStuff, ItemComponents,
    }

  }
}