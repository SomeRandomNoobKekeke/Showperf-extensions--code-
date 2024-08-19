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

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    #region Bruh
    #endregion
    enum CaptureCategory
    {
      ItemsOnMainSub,
      ItemsOnOtherSubs,
    }

    public struct ItemUpdateTicks
    {
      public string ID;
      public long ticks;

      public ItemUpdateTicks(string ID, long ticks)
      {
        this.ID = ID;
        this.ticks = ticks;
      }

      public static ItemUpdateTicks operator +(ItemUpdateTicks a, ItemUpdateTicks b)
      {
        return new ItemUpdateTicks(a.ID, a.ticks + b.ticks);
      }
      public static ItemUpdateTicks operator +(ItemUpdateTicks a, long ticks)
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

      public void Add(Slice s)
      {
        foreach (int key in s.mainSub.Keys)
        {
          if (!mainSub.ContainsKey(key)) mainSub[key] = s.mainSub[key];
          else mainSub[key] += s.mainSub[key];
        }

        foreach (int key in s.otherSubs.Keys)
        {
          if (!otherSubs.ContainsKey(key)) otherSubs[key] = s.otherSubs[key];
          else otherSubs[key] += s.otherSubs[key];
        }
      }

      public void Substract(Slice s)
      {
        foreach (int key in s.mainSub.Keys)
        {
          if (!mainSub.ContainsKey(key)) mainSub[key] = -s.mainSub[key];
          else mainSub[key] -= s.mainSub[key];
        }

        foreach (int key in s.otherSubs.Keys)
        {
          if (!otherSubs.ContainsKey(key)) otherSubs[key] = -s.otherSubs[key];
          else otherSubs[key] -= s.otherSubs[key];
        }
      }
    }

    public class CaptureWindow : IDisposable
    {
      public double frameDuration;

      public int frames;

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

      private int fps;
      public int FPS
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
        if (accumulate) return;

        Slice lastSlice = partialSums.Dequeue();

        totalTicks.Add(firstSlice);
        totalTicks.Substract(lastSlice);

        lastSlice.Clear();
        firstSlice = lastSlice;
        partialSums.Enqueue(lastSlice);
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