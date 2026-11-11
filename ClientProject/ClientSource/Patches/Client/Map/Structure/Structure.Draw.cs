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

[ShowperfPatch("Structure draw time")]
public static partial class StructureDrawTime
{

  public static CUIComponent SettingsPage
  {
    get
    {
      var settings = new CUIDefault.VerticalBackPanel();

      settings["by id"] = new CUIDefault.ButtonGroupField()
      {
        Text = "Capture:",
        GroupName = "structure draw capture mode",
        Options = ["By Prefab", "By ID"],
        SelectedOption = ById ? "By ID" : "By Prefab",
        OnSelected = (name) => ById = name == "By ID",
      };

      return settings;
    }
  }

  public static bool ById { get; set; }


  [PatchMethod("Structure.Draw(SpriteBatch, Boolean, Boolean)", "1.15.30.0")]
  public static bool Structure_Draw_Replace(Structure __instance, SpriteBatch spriteBatch, bool editing, bool back = true)
  {
    Structure _ = __instance;

    if (_.Prefab.Sprite == null) { return false; }

    if (editing)
    {
      if (!SubEditorScreen.IsLayerVisible(_)) { return false; }
      if (!_.HasBody && !Structure.ShowStructures) { return false; }
      if (_.HasBody && !Structure.ShowWalls) { return false; }
    }

    Stopwatch sw = Stopwatch.StartNew();
    _.Draw(spriteBatch, editing, back, null);
    sw.Stop();

    if (ById)
    {
      Capture.Ticks(_.ToString(), sw);
    }
    else
    {
      Capture.Ticks(_.Prefab.Identifier, sw);
    }

    return false;
  }

  public static void Draw(Item __instance, SpriteBatch spriteBatch, bool editing, bool back = true)
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

  }
}