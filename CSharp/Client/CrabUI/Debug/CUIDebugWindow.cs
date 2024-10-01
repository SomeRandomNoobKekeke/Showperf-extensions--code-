#define CUIDEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace CrabUI
{
  public class CUIDebugWindow : CUIFrame
  {
    public static CUIDebugWindow Main;


    public List<CUIDebugEventComponent> Events = new List<CUIDebugEventComponent>();
    public int target;



    public float dc;
    public Color cl;


    public void Capture(CUIDebugEvent e)
    {
      if (target > 200) return;

      if (Events.Count < target + 1)
      {
        CUIDebugEventComponent ec = new CUIDebugEventComponent(e);
        Events.Add(ec);
        this["content"].Append(ec);
      }
      else
      {
        Events[target].Value = e;
      }

      Events[target].BackgroundColor = cl;

      target++;
    }

    public void Flush()
    {
      target = 0;
      Events.ForEach(e => e.Flush());
      dc += 0.01f;
      if (dc > 1) dc = 0;
      cl = ToolBox.GradientLerp(dc,
        Color.Indigo,
        Color.MidnightBlue
      );
    }
    public CUIDebugWindow() : base()
    {
      Main = this;
      IgnoreDebug = true;
      this.ZIndex = 1000;
      this.Layout = new CUILayoutVerticalList(this);

      this["header"] = new CUIComponent()
      {
        Absolute = new CUINullRect(null, null, null, 30),
        IgnoreDebug = true,
      };

      this["content"] = new CUIVerticalList()
      {
        FillEmptySpace = new CUIBool2(false, true),
        BackgroundColor = new Color(0, 0, 32, 128),
        Scrollable = true,
        IgnoreDebug = true,
      };
    }

    public CUIDebugWindow(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}