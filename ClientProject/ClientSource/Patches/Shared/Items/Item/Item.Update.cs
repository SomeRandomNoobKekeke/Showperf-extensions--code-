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
  public enum WhatToCaptureOptions { WholeItemUpdate, UpdateParts, ItemComponents }
  public enum ForWhatOptions { ForAllItems, ForPrefab, For1Item }


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

      settings["for what"] = new EnumButtonGroupField<ForWhatOptions>("For what:")
      {
        SelectedOption = ForWhat,
        OnSelected = (option) => ForWhat = option,
      };

      return settings;
    }
  }


  public static WhatToCaptureOptions WhatToCapture { get; set; } = WhatToCaptureOptions.WholeItemUpdate;
  public static ForWhatOptions ForWhat { get; set; } = ForWhatOptions.ForAllItems;


  [PatchMethod("Item.Update", "1.15.30.0")]
  public static bool Item_Update_Replace(Item __instance, float deltaTime, Camera cam)
  => WhatToCapture switch
  {
    WhatToCaptureOptions.WholeItemUpdate => Item_Update_Replace_WholeUpdate(__instance, deltaTime, cam),
    WhatToCaptureOptions.UpdateParts => Item_Update_Replace_UpdateParts(__instance, deltaTime, cam),
    WhatToCaptureOptions.ItemComponents => Item_Update_Replace_ItemComponents(__instance, deltaTime, cam),
    _ => true,
  };

}