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
      public Dictionary<int, ItemUpdateTicks> mainSub;
      public Dictionary<int, ItemUpdateTicks> otherSubs;

      public Slice()
      {
        mainSub = new Dictionary<int, ItemUpdateTicks>();
        otherSubs = new Dictionary<int, ItemUpdateTicks>();
      }

      public void Clear()
      {
        mainSub.Clear();
        otherSubs.Clear();
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
      public double captureWindowSectionLength;
      public double captureWindowLength;
      public int captureWindowSections;

      public bool frozen = false;

      public Slice firstSlice;

      public Slice totalTicks = new Slice();
      public Queue<Slice> partialSums;


      public CaptureWindow(double length = 1, int sections = 10)
      {
        sections = Math.Max(1, sections);

        captureWindowSectionLength = length / sections;
        captureWindowSections = sections;
        captureWindowLength = length;

        partialSums = new Queue<Slice>(sections);

        for (int i = 0; i < captureWindowSections - 1; i++)
        {
          partialSums.Enqueue(new Slice());
        }

        firstSlice = new Slice();
        partialSums.Enqueue(firstSlice);
      }

      public void Rotate()
      {
        if (frozen) return;

        Slice lastSlice = partialSums.Dequeue();

        totalTicks.Add(firstSlice);
        totalTicks.Substract(lastSlice);

        lastSlice.Clear();
        firstSlice = lastSlice;
        partialSums.Enqueue(lastSlice);
      }

      public void Clear()
      {
        partialSums.Clear();
        totalTicks.Clear();

        for (int i = 0; i < captureWindowSections - 1; i++)
        {
          partialSums.Enqueue(new Slice());
        }

        firstSlice = new Slice();
        partialSums.Enqueue(firstSlice);
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