using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Text;
namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    #region Bruh
    #endregion
    public enum CaptureCategory
    {
      ItemsOnMainSub,
      ItemsOnOtherSubs,
      Characters,
      ItemsDrawing,
      LevelObjectsDrawing,
      OtherLevelStuff,
      ItemComponents,
    }

    public struct ItemUpdateTicks
    {
      public string ID;
      public double ticks;

      public ItemUpdateTicks(string ID, double ticks)
      {
        this.ID = ID;
        this.ticks = ticks;
      }

      public static ItemUpdateTicks operator +(ItemUpdateTicks a, ItemUpdateTicks b)
      {
        return new ItemUpdateTicks(a.ID, a.ticks + b.ticks);
      }
      public static ItemUpdateTicks operator +(ItemUpdateTicks a, double ticks)
      {
        return new ItemUpdateTicks(a.ID, a.ticks + ticks);
      }

      public static ItemUpdateTicks operator -(ItemUpdateTicks a, ItemUpdateTicks b)
      {
        return new ItemUpdateTicks(a.ID, a.ticks - b.ticks);
      }

      public static ItemUpdateTicks operator -(ItemUpdateTicks a)
      {
        return new ItemUpdateTicks(a.ID, -a.ticks);
      }

      public static ItemUpdateTicks operator *(ItemUpdateTicks a, double mult)
      {
        return new ItemUpdateTicks(a.ID, a.ticks * mult);
      }

      public static ItemUpdateTicks operator /(ItemUpdateTicks a, double div)
      {
        return new ItemUpdateTicks(a.ID, a.ticks / div);
      }
    }



    public class Slice
    {
      public Dictionary<CaptureCategory, Dictionary<int, ItemUpdateTicks>> categories;

      public Slice()
      {
        categories = new Dictionary<CaptureCategory, Dictionary<int, ItemUpdateTicks>>();
      }

      public void Clear()
      {
        categories.Clear();
      }

      public Dictionary<int, ItemUpdateTicks> this[CaptureCategory cat]
      {
        get => categories[cat];
        set => categories[cat] = value;
      }

      public void Add(Slice s)
      {
        foreach (CaptureCategory cat in s.categories.Keys)
        {
          if (!categories.ContainsKey(cat)) categories[cat] = new Dictionary<int, ItemUpdateTicks>();

          foreach (int id in s.categories[cat].Keys)
          {
            if (!categories[cat].ContainsKey(id)) categories[cat][id] = s.categories[cat][id];
            else categories[cat][id] += s.categories[cat][id];
          }
        }
      }

      public void Substract(Slice s)
      {
        foreach (CaptureCategory cat in s.categories.Keys)
        {
          if (!categories.ContainsKey(cat)) categories[cat] = new Dictionary<int, ItemUpdateTicks>();

          foreach (int id in s.categories[cat].Keys)
          {
            if (!categories[cat].ContainsKey(id)) categories[cat][id] = -s.categories[cat][id];
            else categories[cat][id] -= s.categories[cat][id];
          }
        }
      }
    }

    public class CaptureWindow : IDisposable
    {
      public double frameDuration;

      public int frames;
      public int Frames
      {
        get { return frames; }
        set
        {
          frames = Math.Max(1, value);
          fps = frames / duration;
          frameDuration = duration / frames;
          Reset();
        }
      }
      private double duration;
      public double Duration
      {
        get { return duration; }
        set
        {
          duration = Math.Max(0.1, value);
          frames = Math.Max(1, (int)Math.Ceiling(duration / frameDuration));
          Reset();
        }
      }

      private double fps;
      public double FPS
      {
        get { return fps; }
        set
        {
          fps = Math.Min(Math.Max(1, value), 60);
          frameDuration = 1.0 / fps;
          frames = Math.Max(1, (int)Math.Ceiling(duration / frameDuration));
          Reset();
        }
      }



      public bool accumulate = false;
      public bool Accumulate
      {
        get => accumulate;
        set
        {
          accumulate = value;
          Reset();
        }
      }
      public Slice firstSlice;

      public Slice totalTicks = new Slice();
      public Queue<Slice> partialSums;


      public CaptureWindow(double duration = 3, int fps = 30)
      {
        this.duration = duration;
        this.fps = fps;

        frameDuration = 1.0 / fps;
        frames = Math.Max(1, (int)Math.Ceiling(duration / frameDuration));

        Reset();
      }


      public void Rotate()
      {
        if (Accumulate)
        {
          totalTicks.Add(firstSlice);
          firstSlice.Clear();
        }
        else
        {
          if (Frames == 1)
          {
            totalTicks.Clear();
            totalTicks.Add(firstSlice);
            firstSlice.Clear();

          }
          else
          {
            Slice lastSlice = partialSums.Dequeue();

            totalTicks.Add(firstSlice);
            totalTicks.Substract(lastSlice);

            lastSlice.Clear();
            firstSlice = lastSlice;
            partialSums.Enqueue(lastSlice);
          }
        }

        //debugCapacity();
      }

      public void Reset()
      {
        partialSums ??= new Queue<Slice>(frames);
        partialSums.Clear();
        totalTicks.Clear();

        for (int i = 0; i < frames - 1; i++)
        {
          partialSums.Enqueue(new Slice());
        }

        firstSlice = new Slice();
        partialSums.Enqueue(firstSlice);

        info($"Reset| fps:{fps} duration:{duration} partialSums.Count: {partialSums.Count}");
      }

      public void ensureCategory(CaptureCategory cat)
      {
        if (!firstSlice.categories.ContainsKey(cat)) firstSlice[cat] = new Dictionary<int, ItemUpdateTicks>();
      }

      public void tryAddTicks(Identifier id, CaptureCategory cat, double ticks) => tryAddTicks(id.HashCode, id.Value, cat, ticks);

      public void tryAddTicks(string id, CaptureCategory cat, double ticks) => tryAddTicks(id.GetHashCode(), id, cat, ticks);


      public void tryAddTicks(int id, string name, CaptureCategory cat, double ticks)
      {
        try
        {
          if (firstSlice[cat].ContainsKey(id))
          {
            firstSlice[cat][id] += ticks;
          }
          else
          {
            firstSlice[cat][id] = new ItemUpdateTicks(name, ticks);
          }
        }
        catch (KeyNotFoundException e)
        {
          ensureCategory(cat);
          err(e.Message);
        }
        catch (Exception e)
        {
          err(e.Message);
        }
      }

      public void debugCapacity()
      {
        if (debug)
        {
          StringBuilder sb = new StringBuilder();
          string s = "";
          foreach (Slice slice in partialSums)
          {

            int sum = 0;
            foreach (var cat in slice.categories)
            {
              sum += cat.Value.Count;
            }

            sb.Append($"{sum}|");
          }

          log(sb.ToString());
        }
      }


      public void Dispose()
      {
        foreach (var s in partialSums) s.Clear();
        partialSums.Clear();
        totalTicks.Clear();

        firstSlice = null;
        partialSums = null;
        totalTicks = null;
      }
    }


  }
}