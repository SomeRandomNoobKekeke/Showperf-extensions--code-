using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using HarmonyLib;

namespace CrabUI
{
  public partial class CUIMainComponent : CUIComponent
  {
    public Stopwatch sw;
    public static CUIMainComponent main;
    public Harmony harmony;
    public long DrawTime;
    public long UpdateTime;
    public List<CUIComponent> Flat = new List<CUIComponent>();

    public CUIMouse Mouse = new CUIMouse();

    public CUIComponent MouseOn;
    public List<CUIComponent> MouseOnList = new List<CUIComponent>();
    public CUIComponent Grabbed;
    public Vector2 GrabbedOffset;

    public CUIComponent ResizingComponent;

    public double LastUpdateTiming;
    public double UpdateInterval = 1.0 / 300.0;


    public void RunStraigth(Action<CUIComponent> a) { for (int i = 0; i < Flat.Count; i++) a(Flat[i]); }
    public void RunReverse(Action<CUIComponent> a) { for (int i = Flat.Count - 1; i >= 0; i--) a(Flat[i]); }

    public void FlattenTree()
    {
      Flat.Clear();
      RunRecursiveOn(this, c => Flat.Add(c));
    }

    public void Step(SpriteBatch spriteBatch)
    {
      RunStraigth(c => c.Update()); // virtual update 

      sw.Restart();
      if (Timing.TotalTime - LastUpdateTiming > UpdateInterval)
      {
        Mouse.Scan();

        if (TreeChanged)
        {
          FlattenTree();
          TreeChanged = false;
        }

        if (Grabbed != null)
        {
          Grabbed.TryDragTo(Mouse.Position - Grabbed.Parent.Real.Position - GrabbedOffset);
          if (!Mouse.Held) Grabbed = null;
        }

        if (ResizingComponent != null)
        {
          ResizingComponent.TryToResize(Mouse.Position - ResizingComponent.Real.Position);
          if (!Mouse.Held) ResizingComponent = null;
        }

        RunStraigth(c => c.Layout.Update());

        HandleMouse();

        LastUpdateTiming = Timing.TotalTime;
      }
      else
      {
        if (MouseOn != null) GUI.MouseOn = dummyComponent;
      }

      UpdateTime = sw.ElapsedTicks;

      sw.Restart();
      DrawRecursive(spriteBatch);
      DrawFrontRecursive(spriteBatch);
      DrawTime = sw.ElapsedTicks;
    }

    public void HandleMouse()
    {
      void CheckIfContainsMouse(CUIComponent c)
      {
        bool mouseInRect = c.Real.Contains(Mouse.Position);

        if (mouseInRect) MouseOnList.Add(c);

        if (!c.HideChildrenOutsideFrame || mouseInRect)
        {
          foreach (CUIComponent child in c.Children)
          {
            CheckIfContainsMouse(child);
          }
        }
      }

      // just deep clear of prev mouse pressed state
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        MouseOnList[i].MousePressed = false;
      }

      CUIComponent CurrentMouseOn = null;

      MouseOnList.Clear();

      if (GUI.MouseOn == null || GUI.MouseOn == dummyComponent)
      {
        foreach (CUIComponent child in this.Children)
        {
          CheckIfContainsMouse(child);
        }
      }

      CurrentMouseOn = MouseOnList.LastOrDefault();

      if (CurrentMouseOn != null) GUI.MouseOn = dummyComponent;

      if (CurrentMouseOn != MouseOn)
      {
        if (MouseOn != null)
        {
          MouseOn.InvokeOnMouseLeave(Mouse);
          MouseOn.MouseOver = false;
          //MouseOn.MousePressed = false;
        }

        if (CurrentMouseOn != null)
        {
          CurrentMouseOn.InvokeOnMouseEnter(Mouse);
          CurrentMouseOn.MouseOver = true;
        }

        MouseOn = CurrentMouseOn;
      }


      //TODO optimize
      if (Mouse.Down)
      {
        // Click on resize handle?
        if (ResizingComponent == null)
        {
          for (int i = MouseOnList.Count - 1; i >= 0; i--)
          {
            if (MouseOnList[i].Resizible && MouseOnList[i].ResizeHandle.Contains(Mouse.Position))
            {
              ResizingComponent = MouseOnList[i];
              break;
            }
          }
        }

        // Init Drag and drop?
        if (ResizingComponent == null && Grabbed == null)
        {
          for (int i = MouseOnList.Count - 1; i >= 0; i--)
          {
            if (MouseOnList[i].Dragable)
            {
              Grabbed = MouseOnList[i];
              GrabbedOffset = Mouse.Position - MouseOnList[i].Real.Position;
              break;
            }

            if (!MouseOnList[i].PassDragAndDrop) break;
          }
        }
      }

      if (ResizingComponent == null && Grabbed == null)
      {
        for (int i = MouseOnList.Count - 1; i >= 0; i--)
        {
          MouseOnList[i].MousePressed = Mouse.Held;
        }
      }

      if (Mouse.Down)
      {
        for (int i = MouseOnList.Count - 1; i >= 0; i--)
        {
          MouseOnList[i].InvokeOnMouseDown(Mouse);
          if (!MouseOnList[i].PassMouseClicks) break;
        }
      }

      if (Mouse.Up)
      {
        for (int i = MouseOnList.Count - 1; i >= 0; i--)
        {
          MouseOnList[i].InvokeOnMouseUp(Mouse);
          if (!MouseOnList[i].PassMouseClicks) break;
        }
      }

      if (Mouse.DoubleClick)
      {
        for (int i = MouseOnList.Count - 1; i >= 0; i--)
        {
          MouseOnList[i].InvokeOnDClick(Mouse);
          if (!MouseOnList[i].PassMouseClicks) break;
        }
      }

      if (Mouse.Scroll != 0)
      {
        for (int i = MouseOnList.Count - 1; i >= 0; i--)
        {
          MouseOnList[i].InvokeOnScroll(Mouse.Scroll);
          if (!MouseOnList[i].PassMouseScroll) break;
        }
      }
    }

    public void Load(CUIComponent c)
    {
      RemoveAllChildren();
      Append(c);
    }

    public CUIMainComponent() : base()
    {
      Real = new CUIRect(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
      Visible = false;
      sw = new Stopwatch();

      harmony = new Harmony("crabui");

      if (main == null)
      {
        main = this;

        patchAll();
      }
    }

    public void patchAll()
    {
      harmony.Patch(
        original: typeof(GUI).GetMethod("Draw", AccessTools.all),
        prefix: new HarmonyMethod(typeof(CUIMainComponent).GetMethod("CUIStep"))
      );

      harmony.Patch(
        original: typeof(Camera).GetMethod("MoveCamera", AccessTools.all),
        prefix: new HarmonyMethod(typeof(CUIMainComponent).GetMethod("CUIBlockScroll"))
      );
    }

    public static void CUIStep(SpriteBatch spriteBatch)
    {
      try
      {
        main.Step(spriteBatch);
        //‖color:Yellow‖CUI:‖end‖
      }
      catch (Exception e) { log($"CUI: {e}", Color.Yellow); }
    }

    public static void CUIBlockScroll(float deltaTime, ref bool allowMove, ref bool allowZoom, bool allowInput, bool? followSub)
    {
      if (GUI.MouseOn == dummyComponent) allowZoom = false;
    }

  }
}