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
    public CUIMultiButton PickIDButton;

    public List<CUIDebugEventComponent> Events = new List<CUIDebugEventComponent>();
    public int target;
    public bool Loop { get; set; } = true;



    public void Capture(CUIDebugEvent e)
    {
      if (EventsComponent == null) return;

      if (target > 1000) return;

      if (Events.Count < target + 1)
      {
        CUIDebugEventComponent ec = new CUIDebugEventComponent(e);
        Events.Add(ec);
        EventsComponent.Append(ec);

        //FIXME why it doesn't work?
        ec.OnMouseEnter += (m) => ec.Value.Target.DebugHighlight = true;
        ec.OnMouseLeave += (m) => ec.Value.Target.DebugHighlight = false;
      }
      else
      {
        Events[target].Value = e;
      }

      target++;
    }

    public void Flush()
    {
      if (Loop) target = 0;

      //TODO mb i should add two modes instead of just commenting it out
      //Events.ForEach(e => e.Flush());
    }

    public void MakeIDList()
    {
      DebugIDsComponent.Visible = false;
      DebugIDsComponent.RemoveAllChildren();

      List<CUIComponent> l = new List<CUIComponent>();

      RunRecursiveOn(CUI.Main, (component, depth) =>
      {
        CUI.log(component);
        l.Add(component);
      });

      foreach (CUIComponent c in l)
      {
        CUIToggleButton b = new CUIToggleButton(c.ToString());
        b.State = c.Debug;
        b.IgnoreDebug = true;
        b.TextAling = new CUIAnchor(CUIAnchorType.LeftTop);
        b.OnMouseDown += (m) =>
        {
          c.Debug = !c.Debug;
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
      };

      this["controls"] = new CUIComponent()
      {
        FitContent = new CUIBool2(false, true),
      };

      this["controls"]["loop"] = new CUIToggleButton("loop")
      {
        Relative = new CUINullRect(0, 0, 0.5f, null),
        AddOnStateChange = (state) =>
        {
          Loop = state;
          Events.Clear();
          EventsComponent.RemoveAllChildren();
        },
      };

      this["controls"].Append(PickIDButton = new CUIMultiButton()
      {
        Relative = new CUINullRect(0.5f, 0, 0.5f, null),
        ConsumeDragAndDrop = false,
      });
      PickIDButton.Add(new CUIButton("Debug events")
      {
        InactiveColor = new Color(0, 0, 0, 128),
        MousePressedColor = new Color(0, 255, 255, 64),
      });
      PickIDButton.Add(new CUIButton("Debugged components")
      {
        InactiveColor = new Color(0, 0, 0, 128),
        MousePressedColor = new Color(0, 255, 255, 64),
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

      PickIDButton.OnSelect += (b, i) =>
        {
          if (i == 0)
          {
            // Events.Clear();
            // EventsComponent.RemoveAllChildren();
            MakeIDList();
            Pages.Open(EventsComponent);
          }
          else Pages.Open(DebugIDsComponent);
        };
      PickIDButton.Select(0);

      this["controls"].Get<CUIToggleButton>("loop").State = true;

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