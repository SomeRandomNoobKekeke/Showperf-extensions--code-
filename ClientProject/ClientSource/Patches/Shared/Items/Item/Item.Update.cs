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

  public static string ToText(this WhatToCaptureOptions value) => value switch
  {
    WhatToCaptureOptions.All => "All",
    WhatToCaptureOptions.ItemComponents => "Item Components",
    _ => "All",
  };

  public static WhatToCaptureOptions FromText(this string value) => value switch
  {
    "All" => WhatToCaptureOptions.All,
    "Item Components" => WhatToCaptureOptions.ItemComponents,
    _ => WhatToCaptureOptions.All,
  };



  public static CUIComponent SettingsPage
  {
    get
    {
      var settings = new CUIDefault.VerticalBackPanel();

      settings["capture mode"] = new CUIDefault.ButtonGroupField()
      {
        Text = "Capture:",
        GroupName = "item capture mode",
        Options = Enum.GetValues<WhatToCaptureOptions>().Select(ToText).ToList(),
        SelectedOption = ToText(WhatToCapture),
        OnSelected = (name) => WhatToCapture = FromText(name),
      };

      settings["per item prefab"] = new CUIDefault.BoolField("Per item prefab")
      {
        State = CapturePerItemPrefab,
        OnToggle = (state) => CapturePerItemPrefab = state,
      };

      return settings;
    }
  }


  public static WhatToCaptureOptions WhatToCapture { get; set; } = WhatToCaptureOptions.All;
  public static bool CapturePerItemPrefab { get; set; }


  [PatchMethod("Item.Update", "1.15.28.0")]
  public static bool Item_Update_Replace(Item __instance, float deltaTime, Camera cam)
  {
    if (CapturePerItemPrefab)
    {
      return Item_Update_Replace_PerItemPrefab(__instance, deltaTime, cam);
    }
    else
    {
      return Item_Update_Replace_ForAllItems(__instance, deltaTime, cam);
    }
  }
}