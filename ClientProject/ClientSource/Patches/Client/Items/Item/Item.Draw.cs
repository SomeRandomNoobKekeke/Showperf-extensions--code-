using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using CUILibs;
using Microsoft.Xna.Framework.Graphics;

namespace ShowperfPatches;

[ShowperfPatch("Item draw time")]
public static partial class ItemDrawTime
{

  public static CUIComponent SettingsPage
  {
    get
    {
      var settings = new CUIDefault.VerticalBackPanel();

      settings["by id"] = new CUIDefault.ButtonGroupField()
      {
        Text = "Capture:",
        GroupName = "item draw capture mode",
        Options = ["By Prefab", "By ID"],
        SelectedOption = ById ? "By ID" : "By Prefab",
        OnSelected = (name) => ById = name == "By ID",
      };

      return settings;
    }
  }

  public static bool ById { get; set; }

  [PatchMethod("Item.Draw(SpriteBatch, Boolean, Boolean, Color?, Single?, Single?)", "",
    HarmonyLib.HarmonyPatchType.Prefix
  )]
  public static void Item_Draw_Prefix(Item __instance, out Stopwatch __state)
  {
    __state = Stopwatch.StartNew();
  }

  [PatchMethod("Item.Draw(SpriteBatch, Boolean, Boolean, Color?, Single?, Single?)", "",
  HarmonyLib.HarmonyPatchType.Postfix
)]
  public static void Item_Draw_Postfix(Item __instance, Stopwatch __state)
  {
    __state.Stop();

    if (ById)
    {
      Capture.Ticks(__instance.ToString(), __state);
    }
    else
    {
      Capture.Ticks(__instance.Prefab.Identifier, __state);
    }
  }
}