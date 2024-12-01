using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.IO;


namespace ShowPerfExtensions
{
  partial class Plugin : IAssemblyPlugin
  {

    // it's working correctly, but it's only 1 ns faster than LuaCsPerformanceCounter.MemoryUsage
    // so i'll use that for simplicity
    public class MemoryWatch : IDisposable
    {
      private object process;
      private Type ProcessType;
      private PropertyInfo PrivateMemorySize64;
      private MethodInfo ProcessDispose;
      private MethodInfo ProcessRefresh;
      private bool ok;

      private float last;
      private float diff;
      private bool recording;


      public float Total
      {
        get
        {
          if (!ok) return 0;
          ProcessRefresh.Invoke(process, new object[] { });
          float mem = Convert.ToSingle(PrivateMemorySize64.GetValue(process));
          return MathF.Round(mem / (1024 * 1024), 2);
        }
      }

      public void Start()
      {
        recording = true;
        last = Total;
      }

      public void Stop()
      {
        recording = false;
        diff = Total - last;
      }

      public float Allocated
      {
        get
        {
          if (recording) Stop();
          return diff;
        }
      }

      public MemoryWatch()
      {
        if (File.Exists("System.Diagnostics.Process.dll"))
        {
          try
          {
            Assembly asm = Assembly.LoadFrom("System.Diagnostics.Process.dll");
            ProcessType = asm.GetType("System.Diagnostics.Process");
            PrivateMemorySize64 = ProcessType.GetProperty("PrivateMemorySize64", AccessTools.all);
            ProcessDispose = ProcessType.GetMethod("Dispose", AccessTools.all, new Type[] { });
            ProcessRefresh = ProcessType.GetMethod("Refresh", AccessTools.all);
            process = ProcessType.GetMethod("GetCurrentProcess", AccessTools.all).Invoke(null, new object[] { });
            ok = true;
          }
          catch (Exception e)
          {
            error(e);
          }
        }
      }

      public void Dispose()
      {
        ProcessDispose?.Invoke(process, new object[] { });
      }


    }
  }
}