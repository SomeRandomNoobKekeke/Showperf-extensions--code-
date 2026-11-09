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
public static partial class WhichItemIsDrawn
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


  [PatchMethod("Item.Draw(SpriteBatch, Boolean, Boolean)", "1.15.28.0")]
  public static bool Item_Draw_Replace(Item __instance, SpriteBatch spriteBatch, bool editing, bool back = true)
  {
    Stopwatch sw = Stopwatch.StartNew();
    __instance.Draw(spriteBatch, editing, back, overrideColor: null);
    sw.Stop();

    if (ById)
    {
      Capture.Ticks(__instance.ToString(), sw);
    }
    else
    {
      Capture.Ticks(__instance.Prefab.Identifier, sw);
    }

    return false;
  }
}