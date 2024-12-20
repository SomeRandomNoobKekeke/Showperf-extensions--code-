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

    public struct ParentInfo
    {
      public string Name;
      public string GenericName;
      public ParentInfo(string name, string genericName = null)
      {
        Name = name ?? "???";
        GenericName = genericName ?? Name;
      }
    }

    public static Dictionary<LightSource, ParentInfo> LightSource_Parent => Instance.lightSource_parent;
    public Dictionary<LightSource, ParentInfo> lightSource_parent = new Dictionary<LightSource, ParentInfo>();

    [ShowperfPatch]
    public class FindLightSourceParents
    {

      public static void Initialize()
      {
        harmony.Patch(
          original: typeof(LightComponent).GetConstructors()[0],
          postfix: ShowperfMethod(typeof(FindLightSourceParents).GetMethod("LightComponent_Constructor_Postfix"))
        );

        harmony.Patch(
          original: typeof(Structure).GetConstructors()[0],
          postfix: ShowperfMethod(typeof(FindLightSourceParents).GetMethod("Structure_Constructor_Postfix"))
        );

        harmony.Patch(
          original: typeof(LevelObject).GetConstructors()[0],
          postfix: ShowperfMethod(typeof(FindLightSourceParents).GetMethod("LevelObject_Constructor_Postfix"))
        );
        harmony.Patch(
          original: typeof(Limb).GetConstructors()[0],
          postfix: ShowperfMethod(typeof(FindLightSourceParents).GetMethod("Limb_Constructor_Postfix"))
        );
      }


      public static void Find()
      {
        try
        {
          LightSource_Parent.Clear();

          if (Level.Loaded != null)
          {
            foreach (LevelObject o in Level.Loaded.LevelObjectManager.objects)
            {
              if (o.LightSources != null)
              {
                foreach (LightSource l in o.LightSources)
                {
                  LightSource_Parent[l] = new ParentInfo(o.ToString());
                }
              }
            }
          }

          foreach (Item i in Item.ItemList)
          {
            foreach (LightComponent lc in i.GetComponents<LightComponent>())
            {
              if (lc.Light != null)
              {
                LightSource_Parent[lc.Light] = new ParentInfo(
                  i.ToString(),
                  i.Prefab?.Identifier.Value ?? i.ToString()
                );
              }
            }
          }

          foreach (Structure s in Structure.WallList)
          {
            foreach (LightSource l in s.Lights)
            {
              LightSource_Parent[l] = new ParentInfo(
                $"{s} ({s.ID})",
                s.ToString()
              );
            }
          }

          foreach (Character c in Character.CharacterList)
          {
            if (c.AnimController == null) continue;

            foreach (Limb l in c.AnimController.Limbs)
            {
              if (l.LightSource == null) continue;
              LightSource_Parent[l.LightSource] = new ParentInfo(
                $"{c} {l.Name}",
                l.Name
              );
            }
          }

          // foreach (LightSource l in GameMain.LightManager.Lights)
          // {
          //   if (!LightSource_Parent.ContainsKey(l))
          //   {
          //     l.LightSourceParams.Range = 1000;
          //     l.LightSourceParams.Color = Color.White;
          //     l.LightSourceParams.Flicker = 1;
          //     l.LightSourceParams.FlickerSpeed = 10;
          //     l.LightSourceParams.PulseAmount = 1;
          //     l.LightSourceParams.BlinkFrequency = 10;
          //   }
          // }
        }
        catch (Exception e) { error(e); }
      }


      public static void LightComponent_Constructor_Postfix(Item item, ContentXElement element, LightComponent __instance)
      {
        try
        {
          if (__instance.Light == null) return;

          LightSource_Parent[__instance.Light] = new ParentInfo(
            __instance.Item?.ToString(),
            __instance.Item?.Prefab?.Identifier.Value ?? __instance.Item?.ToString()
          );
        }
        catch (Exception e) { error(e); }
      }

      public static void Structure_Constructor_Postfix(Structure __instance)
      {
        try
        {
          foreach (LightSource l in __instance.Lights)
          {
            LightSource_Parent[l] = new ParentInfo(
                $"{__instance} ({__instance.ID})",
                __instance.ToString()
              );
          }
        }
        catch (Exception e) { error(e); }
      }

      public static void LevelObject_Constructor_Postfix(LevelObject __instance)
      {
        try
        {
          if (__instance.LightSources == null) return;

          foreach (LightSource l in __instance.LightSources)
          {
            LightSource_Parent[l] = new ParentInfo(__instance.ToString());
          }

        }
        catch (Exception e) { error(e); }
      }

      public static void Limb_Constructor_Postfix(Limb __instance)
      {
        try
        {
          if (__instance.LightSource == null) return;
          LightSource_Parent[__instance.LightSource] = new ParentInfo(
            $"{__instance.character} {__instance.Name}",
            __instance.Name
          );
        }
        catch (Exception e) { error(e); }
      }

    }
  }
}