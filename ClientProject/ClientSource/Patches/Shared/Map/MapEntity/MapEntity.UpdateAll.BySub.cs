using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;
using HarmonyLib;
namespace ShowperfPatches;

public partial class MapEntity_UpdateAll_Deep
{


  public static bool MapEntity_UpdateAll_Replace_BySub(float deltaTime, Camera cam)
  {
    string SubName(MapEntity entity) => entity.Submarine is null ?
      "Open Water" : entity.Submarine.Info.Name;

    Stopwatch sw = new Stopwatch();
    sw.Start();

    DisposableStopwatch watch = new();


    foreach (Hull hull in Hull.HullList)
    {
      using (watch.Restart())
      {
        hull.Update(deltaTime, cam);
      }
      watch.Capture($"{SubName(hull)}  -  Hulls");
    }
    Hull.UpdateCheats(deltaTime, cam);


    foreach (Structure structure in Structure.WallList)
    {
      using (watch.Restart())
      {
        structure.Update(deltaTime, cam);
      }
      watch.Capture($"{SubName(structure)}  -  Structures");
    }


    foreach (Gap gap in Gap.GapList)
    {
      using (watch.Restart())
      {
        gap.ResetWaterFlowThisFrame();
      }
      watch.Capture($"{SubName(gap)}  -  Gaps");
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
      using (watch.Restart())
      {
        gap2.Update(deltaTime, cam);
      }
      watch.Capture($"{SubName(gap2)}  -  Gaps");
    }

    sw.Stop();
    GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw.ElapsedTicks);
    sw.Restart();


    // using (watch.Restart())
    // {
    Powered.UpdatePower(deltaTime);
    // }
    // watch.Capture("UpdatePower");



    Item.UpdatePendingConditionUpdates(deltaTime);
    Item lastUpdatedItem = null;
    try
    {
      foreach (Item item in Item.ItemList)
      {
        lastUpdatedItem = item;

        using (watch.Restart())
        {
          item.Update(deltaTime, cam);
        }
        watch.Capture($"{SubName(item)}  -  Items");
      }
    }
    catch (InvalidOperationException e)
    {
      GameAnalyticsManager.AddErrorEventOnce("MapEntity.UpdateAll:ItemUpdateInvalidOperation", GameAnalyticsManager.ErrorSeverity.Critical, "Error while updating item " + (((lastUpdatedItem != null) ? lastUpdatedItem.Name : null) ?? "null") + ": " + e.Message);
      throw new InvalidOperationException("Error while updating item " + (((lastUpdatedItem != null) ? lastUpdatedItem.Name : null) ?? "null"), e);
    }

    // using (watch.Restart())
    // {
    MapEntity.UpdateAllProjSpecific(deltaTime);
    // }
    // watch.Capture("UpdateAllProjSpecific");

    sw.Stop();
    GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw.ElapsedTicks);
    sw.Restart();

    EntitySpawner spawner = Entity.Spawner;
    if (spawner == null)
    {
      return false;
    }

    // using (watch.Restart())
    // {
    spawner.Update(true);
    // }
    // watch.Capture("EntitySpawner");

    return false;
  }
}