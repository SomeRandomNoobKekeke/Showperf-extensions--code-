using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using Barotrauma.Items.Components;


namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {

    //NOTE this is a shitty temporary solution
    public static bool MapEntity_UpdateAll_Replace(float deltaTime, Camera cam)
    {
      if (Capture.WholeSubmarineUpdate.IsActive)
      {
        return PerSub_MapEntity_UpdateAll(deltaTime, cam);
      }
      else
      {
        if (
          Capture.ItemUpdate.IsActive ||
          Capture.StructureUpdate.IsActive ||
          Capture.HullUpdate.IsActive ||
          Capture.GapUpdate.IsActive
        )
        {
          return Deep_MapEntity_UpdateAll(deltaTime, cam);
        }
      }

      return true;
    }

    public static bool Deep_MapEntity_UpdateAll(float deltaTime, Camera cam)
    {
      if (Capture.ItemUpdate.IsActive) Window.EnsureCategory(Capture.ItemUpdate.Category);
      if (Capture.StructureUpdate.IsActive) Window.EnsureCategory(Capture.StructureUpdate.Category);
      if (Capture.HullUpdate.IsActive) Window.EnsureCategory(Capture.HullUpdate.Category);
      if (Capture.GapUpdate.IsActive) Window.EnsureCategory(Capture.GapUpdate.Category);

      long ticks;
      Stopwatch sw1 = new Stopwatch();
      Stopwatch sw2 = new Stopwatch();

      MapEntity.mapEntityUpdateTick++;

      //#if CLIENT

      sw1.Start();

      //#endif
      if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
      {

        if (Capture.HullUpdate.IsActive)
        {
          foreach (Hull hull in Hull.HullList)
          {
            sw2.Restart();
            hull.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
            ticks = sw2.ElapsedTicks;
            if (Window.ShouldCapture(hull))
            {
              Window.AddTicks(new UpdateTicks(ticks, Capture.HullUpdate.Category, $"{hull}"));
            }
          }
        }
        else
        {
          foreach (Hull hull in Hull.HullList)
          {
            hull.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
          }
        }


        //#if CLIENT
        Hull.UpdateCheats(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
        //#endif

        if (Capture.StructureUpdate.IsActive)
        {
          foreach (Structure structure in Structure.WallList)
          {
            sw2.Restart();
            structure.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
            ticks = sw2.ElapsedTicks;
            if (Window.ShouldCapture(structure))
            {
              if (Capture.StructureUpdate.ByID)
              {
                Window.AddTicks(new UpdateTicks(ticks, Capture.StructureUpdate.Category, $"{structure}({structure.ID})"));
              }
              else
              {
                Window.AddTicks(new UpdateTicks(ticks, Capture.StructureUpdate.Category, $"{structure}"));
              }
            }
          }
        }
        else
        {
          foreach (Structure structure in Structure.WallList)
          {
            structure.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
          }
        }
      }

      //update gaps in random order, because otherwise in rooms with multiple gaps
      //the water/air will always tend to flow through the first gap in the list,
      //which may lead to weird behavior like water draining down only through
      //one gap in a room even if there are several
      if (Capture.GapUpdate.IsActive)
      {
        foreach (Gap gap in Gap.GapList.OrderBy(g => Rand.Int(int.MaxValue)))
        {
          sw2.Restart();
          gap.Update(deltaTime, cam);
          ticks = sw2.ElapsedTicks;

          if (Window.ShouldCapture(gap))
          {
            Window.AddTicks(new UpdateTicks(ticks, Capture.GapUpdate.Category, $"Gap({gap.ID})"));
          }
        }
      }
      else
      {
        foreach (Gap gap in Gap.GapList.OrderBy(g => Rand.Int(int.MaxValue)))
        {
          gap.Update(deltaTime, cam);
        }
      }

      if (MapEntity.mapEntityUpdateTick % MapEntity.PoweredUpdateInterval == 0)
      {
        Powered.UpdatePower(deltaTime * MapEntity.PoweredUpdateInterval);
      }

      //#if CLIENT
      sw1.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw1.ElapsedTicks);
      sw1.Restart();
      //#endif



      Item.UpdatePendingConditionUpdates(deltaTime);
      if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
      {
        Item lastUpdatedItem = null;

        try
        {
          if (Capture.ItemUpdate.IsActive)
          {
            foreach (Item item in Item.ItemList)
            {
              if (GameMain.LuaCs.Game.UpdatePriorityItems.Contains(item)) { continue; }
              lastUpdatedItem = item;

              sw2.Restart();
              item.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);


              ticks = sw2.ElapsedTicks;
              if (Window.ShouldCapture(item))
              {
                if (Capture.ItemUpdate.ByID)
                {
                  Window.AddTicks(new UpdateTicks(ticks, Capture.ItemUpdate.Category, $"{item}"));
                }
                else
                {
                  Window.AddTicks(new UpdateTicks(ticks, Capture.ItemUpdate.Category, item.Prefab.Identifier));
                }
              }
            }
          }
          else
          {
            foreach (Item item in Item.ItemList)
            {
              if (GameMain.LuaCs.Game.UpdatePriorityItems.Contains(item)) { continue; }
              lastUpdatedItem = item;

              item.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
            }
          }
        }
        catch (InvalidOperationException e)
        {
          GameAnalyticsManager.AddErrorEventOnce(
              "MapEntity.UpdateAll:ItemUpdateInvalidOperation",
              GameAnalyticsManager.ErrorSeverity.Critical,
              $"Error while updating item {lastUpdatedItem?.Name ?? "null"}: {e.Message}");
          throw new InvalidOperationException($"Error while updating item {lastUpdatedItem?.Name ?? "null"}", innerException: e);
        }
      }



      if (Capture.ItemUpdate.IsActive)
      {
        foreach (var item in GameMain.LuaCs.Game.UpdatePriorityItems)
        {
          if (item.Removed) continue;

          sw2.Restart();

          item.Update(deltaTime, cam);

          ticks = sw2.ElapsedTicks;
          if (Window.ShouldCapture(item))
          {
            if (Capture.ItemUpdate.ByID)
            {
              Window.AddTicks(new UpdateTicks(ticks, Capture.ItemUpdate.Category, $"{item}"));
            }
            else
            {
              Window.AddTicks(new UpdateTicks(ticks, Capture.ItemUpdate.Category, item.Prefab.Identifier));
            }
          }
        }
      }
      else
      {
        foreach (var item in GameMain.LuaCs.Game.UpdatePriorityItems)
        {
          if (item.Removed) continue;
          item.Update(deltaTime, cam);
        }
      }
      sw2.Stop();

      //#if CLIENT
      sw1.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw1.ElapsedTicks);
      //#endif

      if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
      {
        MapEntity.UpdateAllProjSpecific(deltaTime * MapEntity.MapEntityUpdateInterval);

        MapEntity.Spawner?.Update();
      }

      return false;
    }

    public static bool PerSub_MapEntity_UpdateAll(float deltaTime, Camera cam)
    {
      if (Capture.WholeSubmarineUpdate.IsActive) Window.EnsureCategory(Capture.WholeSubmarineUpdate.Category);

      long ticks;
      Stopwatch sw1 = new Stopwatch();
      Stopwatch sw2 = new Stopwatch();

      MapEntity.mapEntityUpdateTick++;

      //#if CLIENT
      sw1.Start();
      //#endif
      if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
      {
        foreach (Hull hull in Hull.HullList)
        {
          sw2.Restart();
          hull.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
          ticks = sw2.ElapsedTicks;
          Window.AddTicks(new UpdateTicks(ticks, Capture.WholeSubmarineUpdate.Category, $"{hull.Submarine?.ToString() ?? "Things in open water"}"));
        }

        //#if CLIENT
        Hull.UpdateCheats(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
        //#endif

        foreach (Structure structure in Structure.WallList)
        {
          sw2.Restart();
          structure.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);
          ticks = sw2.ElapsedTicks;

          Window.AddTicks(new UpdateTicks(ticks, Capture.WholeSubmarineUpdate.Category, $"{structure.Submarine?.ToString() ?? "Things in open water"}"));
        }
      }

      //update gaps in random order, because otherwise in rooms with multiple gaps
      //the water/air will always tend to flow through the first gap in the list,
      //which may lead to weird behavior like water draining down only through
      //one gap in a room even if there are several

      foreach (Gap gap in Gap.GapList.OrderBy(g => Rand.Int(int.MaxValue)))
      {
        sw2.Restart();
        gap.Update(deltaTime, cam);
        ticks = sw2.ElapsedTicks;

        Window.AddTicks(new UpdateTicks(ticks, Capture.WholeSubmarineUpdate.Category, $"{gap.Submarine?.ToString() ?? "Things in open water"}"));
      }


      if (MapEntity.mapEntityUpdateTick % MapEntity.PoweredUpdateInterval == 0)
      {
        Powered.UpdatePower(deltaTime * MapEntity.PoweredUpdateInterval);
      }

      //#if CLIENT
      sw1.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Misc", sw1.ElapsedTicks);
      sw1.Restart();
      //#endif

      Item.UpdatePendingConditionUpdates(deltaTime);
      if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
      {
        Item lastUpdatedItem = null;

        try
        {
          foreach (Item item in Item.ItemList)
          {
            if (GameMain.LuaCs.Game.UpdatePriorityItems.Contains(item)) { continue; }
            lastUpdatedItem = item;

            sw2.Restart();
            item.Update(deltaTime * MapEntity.MapEntityUpdateInterval, cam);

            ticks = sw2.ElapsedTicks;

            Window.AddTicks(new UpdateTicks(ticks, Capture.WholeSubmarineUpdate.Category, $"{item.Submarine?.ToString() ?? "Things in open water"}"));
          }
        }
        catch (InvalidOperationException e)
        {
          GameAnalyticsManager.AddErrorEventOnce(
              "MapEntity.UpdateAll:ItemUpdateInvalidOperation",
              GameAnalyticsManager.ErrorSeverity.Critical,
              $"Error while updating item {lastUpdatedItem?.Name ?? "null"}: {e.Message}");
          throw new InvalidOperationException($"Error while updating item {lastUpdatedItem?.Name ?? "null"}", innerException: e);
        }
      }

      foreach (var item in GameMain.LuaCs.Game.UpdatePriorityItems)
      {
        if (item.Removed) continue;

        sw2.Restart();

        item.Update(deltaTime, cam);

        ticks = sw2.ElapsedTicks;
        Window.AddTicks(new UpdateTicks(ticks, Capture.WholeSubmarineUpdate.Category, $"{item.Submarine?.ToString() ?? "Things in open water"}"));
      }

      sw2.Stop();

      //#if CLIENT
      sw1.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Update:MapEntity:Items", sw1.ElapsedTicks);
      //#endif

      if (MapEntity.mapEntityUpdateTick % MapEntity.MapEntityUpdateInterval == 0)
      {
        MapEntity.UpdateAllProjSpecific(deltaTime * MapEntity.MapEntityUpdateInterval);

        MapEntity.Spawner?.Update();
      }

      return false;
    }

  }
}