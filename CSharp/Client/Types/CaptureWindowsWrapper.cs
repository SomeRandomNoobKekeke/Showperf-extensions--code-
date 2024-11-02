using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;

namespace ShowPerfExtensions
{
  public partial class Plugin
  {
    public class CaptureWindowsWrapper
    {
      public CaptureWindow Draw = new CaptureWindow();
      public CaptureWindow Update = new CaptureWindow();
      public CaptureWindow GPUCalls = new CaptureWindow();

      private bool frozen; public bool Frozen
      {
        get => frozen;
        set
        {
          frozen = value;
          Draw.Frozen = value;
          Update.Frozen = value;
        }
      }
    }
  }
}