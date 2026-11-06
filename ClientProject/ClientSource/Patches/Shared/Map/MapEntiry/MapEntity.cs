using ShowperfExtensions;
using System.Diagnostics;
using Barotrauma.Items.Components;
using static Barotrauma.MapEntity;
using CursedUI;
namespace ShowperfPatches;

[ShowperfPatch("Deep MapEntity.UpdateAll()")]
public class MapEntity_UpdateAll_Deep
{
  public record ItemCaptureModes
  {
    private ItemCaptureModes(string name) => Name = name;

    public string Name { get; }

    public static List<string> AllNames => [ByPrefab.Name, ById.Name, All.Name, No.Name,];

    public static ItemCaptureModes All { get; } = new("all");
    public static ItemCaptureModes No { get; } = new("no");
    public static ItemCaptureModes ByPrefab { get; } = new("by prefab");
    public static ItemCaptureModes ById { get; } = new("by id");

    public static ItemCaptureModes FromString(string raw)
    {
      if (raw == All.Name) return All;
      if (raw == ByPrefab.Name) return ByPrefab;
      if (raw == ById.Name) return ById;
      return No;
    }
  }

  public static CUIComponent SettingsPage
  {
    get
    {
      var settings = new CUIDefault.VerticalBackPanel();

      settings["item capture mode"] = new CUIDefault.ButtonGroupField()
      {
        Text = "Capture Items:",
        GroupName = "item capture mode",
        Options = ItemCaptureModes.AllNames,
        SelectedOption = ItemCaptureMode.Name,
        OnSelected = (name) => ItemCaptureMode = ItemCaptureModes.FromString(name),
      };

      settings["CaptureOtherStuff"] = new CUIDefault.BoolField("Capture Other Stuff")
      {
        State = CaptureOtherStuff,
        OnToggle = (state) => CaptureOtherStuff = state,
      };

      return settings;
    }
  }

  public static ItemCaptureModes ItemCaptureMode { get; set; } = ItemCaptureModes.ByPrefab;
  public static bool CaptureOtherStuff { get; set; } = false;


  [PatchMethod("MapEntity.UpdateAll")]
  public static bool UpdateAll(float deltaTime, Camera cam)
  {
    Stopwatch sw = new Stopwatch();
    Stopwatch sw2 = new Stopwatch();
    sw.Restart();



    using (var watch = DisposableStopwatch.StartNew())
    {
      foreach (Hull hull in Hull.HullList)
      {
        hull.Update(deltaTime, cam);
      }
      Hull.UpdateCheats(deltaTime, cam);

      watch.Capture($"Update Hulls");
    }








    sw2.Restart();
    foreach (Structure structure in Structure.WallList)
    {
      structure.Update(deltaTime, cam);
    }
    sw2.Stop();
    Capture.Ticks($"Update Structures", sw2.ElapsedTicks);

    sw2.Restart();
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

    sw2.Stop();
    Capture.Ticks($"Update Gaps", sw2.ElapsedTicks);

    sw.Stop();
    GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw.ElapsedTicks);

    sw.Restart();

    sw2.Restart();
    Powered.UpdatePower(deltaTime);
    sw2.Stop();
    Capture.Ticks($"Update Power", sw2.ElapsedTicks);

    Item.UpdatePendingConditionUpdates(deltaTime);
    Item lastUpdatedItem = null;
    try
    {
      if (ItemCaptureMode == ItemCaptureModes.ById)
      {
        foreach (Item item in Item.ItemList)
        {
          lastUpdatedItem = item;


          sw2.Restart();
          item.Update(deltaTime, cam);
          sw2.Stop();
          Capture.Ticks($"{item}", sw2.ElapsedTicks);
        }
      }
      else if (ItemCaptureMode == ItemCaptureModes.ByPrefab)
      {
        foreach (Item item in Item.ItemList)
        {
          lastUpdatedItem = item;

          sw2.Restart();
          item.Update(deltaTime, cam);
          sw2.Stop();
          Capture.Ticks($"{item.Prefab.Name}", sw2.ElapsedTicks);
        }
      }
      else
      {
        foreach (Item item in Item.ItemList)
        {
          lastUpdatedItem = item;
          item.Update(deltaTime, cam);
        }
      }
    }
    catch (InvalidOperationException e)
    {
      GameAnalyticsManager.AddErrorEventOnce("MapEntity.UpdateAll:ItemUpdateInvalidOperation", GameAnalyticsManager.ErrorSeverity.Critical, "Error while updating item " + (((lastUpdatedItem != null) ? lastUpdatedItem.Name : null) ?? "null") + ": " + e.Message);
      throw new InvalidOperationException("Error while updating item " + (((lastUpdatedItem != null) ? lastUpdatedItem.Name : null) ?? "null"), e);
    }
    MapEntity.UpdateAllProjSpecific(deltaTime);
    sw.Stop();


    if (ItemCaptureMode == ItemCaptureModes.All)
    {
      Capture.Ticks("Update:MapEntity:Items", sw.ElapsedTicks);
    }

    GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw.ElapsedTicks);
    sw.Restart();
    EntitySpawner spawner = Entity.Spawner;
    if (spawner == null)
    {
      return false;
    }
    spawner.Update(true);

    return false;
  }
}