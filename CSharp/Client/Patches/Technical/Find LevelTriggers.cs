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

    public static Dictionary<LevelTrigger, ParentInfo> LevelTrigger_Parent => Instance.levelTrigger_parent;
    public Dictionary<LevelTrigger, ParentInfo> levelTrigger_parent = new Dictionary<LevelTrigger, ParentInfo>();

    [ShowperfPatch]
    public class FindLevelTriggerParents
    {

      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(LevelObject).GetConstructors()[0],
          postfix: ShowperfMethod(typeof(FindLightSourceParents).GetMethod("LevelObject_Constructor_Postfix"))
        );
      }


      public static void Find()
      {
        try
        {
          LevelTrigger_Parent.Clear();

          if (Level.Loaded != null)
          {
            foreach (LevelObject o in Level.Loaded.LevelObjectManager.objects)
            {
              if (o.Triggers != null)
              {
                foreach (LevelTrigger lt in o.Triggers)
                {
                  LevelTrigger_Parent[lt] = new ParentInfo(o.ToString());
                }
              }
            }
          }

        }
        catch (Exception e) { error(e); }
      }




      public static void LevelObject_Constructor_Postfix(LevelObject __instance)
      {
        try
        {
          if (__instance.Triggers == null) return;

          foreach (LevelTrigger lt in __instance.Triggers)
          {
            LevelTrigger_Parent[lt] = new ParentInfo(__instance.ToString());
          }

        }
        catch (Exception e) { error(e); }
      }



    }
  }
}