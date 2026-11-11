using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;
using HarmonyLib;
namespace ShowperfPatches;

[ShowperfPatch("Deep MapEntity.UpdateAll()")]
public partial class MapEntity_UpdateAll_Deep
{
  public enum CaptureModeOptions { ByEntityType, BySub, WholeSub }

  public static CUIComponent SettingsPage
  {
    get
    {
      var settings = new CUIDefault.VerticalBackPanel();

      settings["mode"] = new EnumButtonGroupField<CaptureModeOptions>("Mode:")
      {
        Options = ["By Entity Type", "By Sub", "Whole Sub"],
        SelectedOption = CaptureMode,
        OnSelected = (mode) => CaptureMode = mode,
      };

      return settings;
    }
  }

  public static CaptureModeOptions CaptureMode { get; set; } = CaptureModeOptions.ByEntityType;


  [PatchMethod("MapEntity.UpdateAll", "1.15.30.0")]
  public static bool MapEntity_UpdateAll_Replace(float deltaTime, Camera cam)
    => CaptureMode switch
    {
      CaptureModeOptions.ByEntityType => MapEntity_UpdateAll_Replace_ByEntity(deltaTime, cam),
      CaptureModeOptions.BySub => MapEntity_UpdateAll_Replace_BySub(deltaTime, cam),
      CaptureModeOptions.WholeSub => MapEntity_UpdateAll_Replace_WholeSub(deltaTime, cam),
      _ => true,
    };

}