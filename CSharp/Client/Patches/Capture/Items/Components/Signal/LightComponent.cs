using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


using Barotrauma;
using HarmonyLib;

using Microsoft.Xna.Framework;
using System;
using Barotrauma.Networking;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework.Graphics;
using Barotrauma.Lights;
using Barotrauma.Items.Components;



namespace ShowPerfExtensions
{
  public partial class Plugin
  {

    public Dictionary<LightSource, LightComponent> LightSource_LightComponent = new Dictionary<LightSource, LightComponent>();

    [ShowperfPatch]
    public class LightComponentPatch
    {

      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(LightComponent).GetConstructors()[0],
          postfix: ShowperfMethod(typeof(LightComponentPatch).GetMethod("LightComponent_Constructor_Postfix"))
        );

      }

      public static void LightComponent_Constructor_Postfix(Item item, ContentXElement element, LightComponent __instance)
      {
        try
        {
          Mod.LightSource_LightComponent[__instance.Light] = __instance;
        }
        catch (Exception e) { error(e); }
      }

    }
  }
}