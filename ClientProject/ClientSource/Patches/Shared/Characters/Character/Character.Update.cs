using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using CUILibs;
using Microsoft.Xna.Framework;

namespace ShowperfPatches;

[ShowperfPatch("Deep Character.UpdateAll()")]
public static partial class Character_UpdateAll_Deep
{
  public enum CaptureByOptions { All, Species, Character }


  public static CUIComponent SettingsPage
  {
    get
    {
      var settings = new CUIDefault.VerticalBackPanel();

      settings["CaptureBy"] = new EnumButtonGroupField<CaptureByOptions>("Capture By:")
      {
        SelectedOption = CaptureBy,
        OnSelected = (option) => CaptureBy = option,
      };

      return settings;
    }
  }

  public static CaptureByOptions CaptureBy { get; set; } = CaptureByOptions.All;


  [PatchMethod("Character.Update", "1.15.30.0")]
  public static bool Character_Update_Replace(Character __instance, float deltaTime, Camera cam)
    => CaptureBy switch
    {
      CaptureByOptions.All => Character_Update_Replace_All(__instance, deltaTime, cam),
      CaptureByOptions.Species => Character_Update_Replace_BySpecies(__instance, deltaTime, cam),
      CaptureByOptions.Character => Character_Update_Replace_ByCharacter(__instance, deltaTime, cam),
      _ => true,
    };
}