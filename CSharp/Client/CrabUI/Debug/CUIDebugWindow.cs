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

    public CUIVerticalList EventsComponent;
    public CUIVerticalList DebugIDsComponent;
    public CUIPages Pages;

    public CUIMultiButton Header;

    public List<CUIDebugEventComponent> Events = new List<CUIDebugEventComponent>();
    public int target;


    public void Capture(CUIDebugEvent e)
    {
      if (EventsComponent == null) return;

      if (target > 1000) return;

      if (Events.Count < target + 1)
      {
        CUIDebugEventComponent ec = new CUIDebugEventComponent(e);
        Events.Add(ec);
        EventsComponent.Append(ec);
      }
      else
      {
        Events[target].Value = e;
      }

      target++;
    }

    public void Flush()
    {
      target = 0;
      Events.ForEach(e => e.Flush());
    }

    public void MakeIDList()
    {
      DebugIDsComponent.Visible = false;
      DebugIDsComponent.RemoveAllChildren();

      List<CUIComponent> l = new List<CUIComponent>();

      RunRecursiveOn(CUI.Main, (component, depth) => l.Add(component));

      foreach (CUIComponent c in l)
      {
        CUIToggleButton b = new CUIToggleButton(c.ToString());
        b.State = c.Debug;
        b.IgnoreDebug = true;
        b.TextAling = new CUIAnchor(CUIAnchorType.LeftTop);
        b.OnMouseDown += (m) =>
        {
          c.Debug = !c.Debug;
          Events.Clear();
          EventsComponent.RemoveAllChildren();
          MakeIDList();
        };

        b.OnMouseEnter += (m) =>
        {
          c.DebugHighlight = true;
        };

        b.OnMouseLeave += (m) =>
        {
          c.DebugHighlight = false;
        };


        DebugIDsComponent.Append(b);
      }
      DebugIDsComponent.Visible = true;
      l.Clear();
    }

    public CUIDebugWindow() : base()
    {
      Main = this;

      this.ZIndex = 1000;
      this.Layout = new CUILayoutVerticalList(this);

      this["handle"] = new CUIComponent()
      {
        Absolute = new CUINullRect(null, null, null, 20),
        IgnoreDebug = true,
      };

      Append(Header = new CUIMultiButton()
      {
        Absolute = new CUINullRect(null, null, null, 20),
        ConsumeDragAndDrop = false,
        IgnoreDebug = true,
      });
      Header.Add(new CUIButton("Debug events")
      {
        InactiveColor = new Color(0, 0, 0, 128),
        MousePressedColor = new Color(0, 255, 255, 64),
        IgnoreDebug = true,
      });
      Header.Add(new CUIButton("Debugged components")
      {
        InactiveColor = new Color(0, 0, 0, 128),
        MousePressedColor = new Color(0, 255, 255, 64),
        IgnoreDebug = true,
      });


      Append(Pages = new CUIPages()
      {
        FillEmptySpace = new CUIBool2(false, true),
        BackgroundColor = new Color(0, 0, 32, 128),
        IgnoreDebug = true,
      });

      EventsComponent = new CUIVerticalList(0, 0, 1, 1)
      {
        Scrollable = true,
        IgnoreDebug = true,
      };

      DebugIDsComponent = new CUIVerticalList(0, 0, 1, 1)
      {
        Scrollable = true,
        IgnoreDebug = true,
      };

      Header.OnSelect += (b, i) =>
      {
        if (i == 0) Pages.Open(EventsComponent);
        else Pages.Open(DebugIDsComponent);
      };
      Header.Select(0);

      IgnoreDebug = true;
    }

    public static void Open()
    {
      if (CUI.Main == null) return;

      CUIDebugWindow w = new CUIDebugWindow(0, 0.3f, 0.3f, 0.6f);
      CUI.Main.Append(w);
      CUI.Main.OnTreeChanged += () => w.MakeIDList();

    }

    public CUIDebugWindow(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }
}