using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using CUILibs;

namespace ShowperfPatches;

[ShowperfPatch("Deep Item.Update()")]
public static partial class Item_Update_Deep
{
  public enum WhatToCaptureOptions { All, ItemComponents }
  public enum ByWhatOptions { ForAllItems, ByPrefab, ByID }


  public static CUIComponent SettingsPage
  {
    get
    {
      var settings = new CUIDefault.VerticalBackPanel();

      settings["capture what"] = new EnumButtonGroupField<WhatToCaptureOptions>("What to capture:")
      {
        SelectedOption = WhatToCapture,
        OnSelected = (option) => WhatToCapture = option,
      };

      settings["by what"] = new EnumButtonGroupField<ByWhatOptions>("By what:")
      {
        SelectedOption = ByWhat,
        OnSelected = (option) => ByWhat = option,
      };

      return settings;
    }
  }


  public static WhatToCaptureOptions WhatToCapture { get; set; } = WhatToCaptureOptions.All;
  public static ByWhatOptions ByWhat { get; set; } = ByWhatOptions.ForAllItems;


  [PatchMethod("Item.Update", "1.15.30.0")]
  public static bool Item_Update_Replace(Item __instance, float deltaTime, Camera cam)
  => ByWhat switch
  {
    ByWhatOptions.ForAllItems => Item_Update_Replace_ForAllItems(__instance, deltaTime, cam),
    ByWhatOptions.ByPrefab => Item_Update_Replace_ByItemPrefab(__instance, deltaTime, cam),
    ByWhatOptions.ByID => Item_Update_Replace_ByID(__instance, deltaTime, cam),
    _ => true,
  };

}