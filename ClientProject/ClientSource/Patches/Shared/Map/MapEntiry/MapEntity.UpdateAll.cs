using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;
namespace ShowperfPatches;

[ShowperfPatch("Deep MapEntity.UpdateAll()")]
public class MapEntity_UpdateAll_Deep
{
  public record WhatToCaptureOptions(string name)
  {
    public string Name { get; } = name;

    public static List<string> AllNames => [All.Name, ItemsByPrefab.Name, ItemsById.Name,];

    public static WhatToCaptureOptions All { get; } = new("all");
    public static WhatToCaptureOptions ItemsByPrefab { get; } = new("items by prefab");
    public static WhatToCaptureOptions ItemsById { get; } = new("items by id");

    public static WhatToCaptureOptions FromString(string raw)
    {
      if (raw == ItemsByPrefab.Name) return ItemsByPrefab;
      if (raw == ItemsById.Name) return ItemsById;
      return All;
    }
  }

  public static CUIComponent SettingsPage
  {
    get
    {
      var settings = new CUIDefault.VerticalBackPanel();

      settings["capture mode"] = new CUIDefault.ButtonGroupField()
      {
        Text = "Capture:",
        GroupName = "map entity capture mode",
        Options = WhatToCaptureOptions.AllNames,
        SelectedOption = WhatToCapture.Name,
        OnSelected = (name) => WhatToCapture = WhatToCaptureOptions.FromString(name),
      };

      return settings;
    }
  }

  public static WhatToCaptureOptions WhatToCapture { get; set; } = WhatToCaptureOptions.All;



  [PatchMethod("MapEntity.UpdateAll", "1.15.28.0")]
  public static bool MapEntity_UpdateAll_Replace(float deltaTime, Camera cam)
  {
    Stopwatch sw = new Stopwatch();
    Stopwatch sw2 = new Stopwatch();

    DisposableStopwatch watch = new DisposableStopwatch()
    {
      ShouldCapture = WhatToCapture == WhatToCaptureOptions.All,
    };

    DisposableStopwatch itemWatch = new DisposableStopwatch();


    sw.Start();


    using (watch.Restart())
    {
      foreach (Hull hull in Hull.HullList)
      {
        hull.Update(deltaTime, cam);
      }
      Hull.UpdateCheats(deltaTime, cam);

      watch.Stop().Capture("Update hulls");
    }


    using (watch.Restart())
    {
      foreach (Structure structure in Structure.WallList)
      {
        structure.Update(deltaTime, cam);
      }

      watch.Stop().Capture("Update structures");
    }

    using (watch.Restart())
    {
      foreach (Gap gap in Gap.GapList)
      {
        gap.ResetWaterFlowThisFrame();
      }
      MapEntity.gapUpdateList.Clear();
      MapEntity.gapUpdateList.AddRange(Gap.GapList);
      for (int i = MapEntity.gapUpdateList.Count - 1; i > 0; i--)
      {
        int j = Rand.Int(i + 1, Rand.RandSync.Unsynced);
        List<Gap> list = MapEntity.gapUpdateList;
        int index = i;
        List<Gap> list2 = MapEntity.gapUpdateList;
        int index2 = j;
        Gap value = MapEntity.gapUpdateList[j];
        Gap value2 = MapEntity.gapUpdateList[i];
        list[index] = value;
        list2[index2] = value2;
      }
      foreach (Gap gap2 in MapEntity.gapUpdateList)
      {
        gap2.Update(deltaTime, cam);
      }

      watch.Stop().Capture("Update gaps");
    }



    sw.Stop();
    GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw.ElapsedTicks);
    sw.Restart();

    using (watch.Restart())
    {
      Powered.UpdatePower(deltaTime);

      watch.Stop().Capture("Powered.UpdatePower");
    }

    using (watch.Restart())
    {
      Item.UpdatePendingConditionUpdates(deltaTime);

      watch.Stop().Capture("Item.UpdatePendingConditionUpdates");
    }


    Item lastUpdatedItem = null;
    try
    {
      if (WhatToCapture == WhatToCaptureOptions.ItemsById)
      {
        foreach (Item item in Item.ItemList)
        {
          lastUpdatedItem = item;

          sw2.Restart();
          item.Update(deltaTime, cam);
          sw2.Stop();
          Capture.Ticks(item.ToString(), sw2.ElapsedTicks);
        }
      }

      if (WhatToCapture == WhatToCaptureOptions.ItemsByPrefab)
      {
        foreach (Item item in Item.ItemList)
        {
          lastUpdatedItem = item;

          sw2.Restart();
          item.Update(deltaTime, cam);
          sw2.Stop();
          Capture.Ticks(item.Prefab.Identifier, sw2.ElapsedTicks);
        }
      }

      if (WhatToCapture == WhatToCaptureOptions.All)
      {
        watch.Restart();
        foreach (Item item in Item.ItemList)
        {
          lastUpdatedItem = item;
          item.Update(deltaTime, cam);
        }
        watch.Stop().Capture("Update items");
      }
    }
    catch (InvalidOperationException e)
    {
      GameAnalyticsManager.AddErrorEventOnce("MapEntity.UpdateAll:ItemUpdateInvalidOperation", GameAnalyticsManager.ErrorSeverity.Critical, "Error while updating item " + (((lastUpdatedItem != null) ? lastUpdatedItem.Name : null) ?? "null") + ": " + e.Message);
      throw new InvalidOperationException("Error while updating item " + (((lastUpdatedItem != null) ? lastUpdatedItem.Name : null) ?? "null"), e);
    }

    using (watch.Restart())
    {
      MapEntity.UpdateAllProjSpecific(deltaTime);
      watch.Stop().Capture("UpdateAllProjSpecific");
    }

    sw.Stop();

    GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw.ElapsedTicks);
    sw.Restart();
    EntitySpawner spawner = Entity.Spawner;
    if (spawner == null)
    {
      return false;
    }

    using (watch.Restart())
    {
      spawner.Update(true);
      watch.Stop().Capture("spawner.Update");
    }

    return false;
  }
}