#define SHOWPERF

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
  public class CUIMainComponent : CUIComponent
  {
    public long DrawTime;
    public long UpdateTime;
    public double UpdateInterval = 1.0 / 300.0;
    public event Action OnUpdate;

    private Stopwatch sw;
    private static CUIMainComponent Main;
    private Harmony harmony;
    private List<CUIComponent> Flat = new List<CUIComponent>();
    private CUIMouse Mouse = new CUIMouse();
    private CUIComponent MouseOn;
    private List<CUIComponent> MouseOnList = new List<CUIComponent>();
    private CUIComponent Grabbed;
    private Vector2 GrabbedOffset;
    private CUIResizeHandle GrabbedResizeHandle;


    private void RunStraigth(Action<CUIComponent> a) { for (int i = 0; i < Flat.Count; i++) a(Flat[i]); }
    private void RunReverse(Action<CUIComponent> a) { for (int i = Flat.Count - 1; i >= 0; i--) a(Flat[i]); }

    private void FlattenTree()
    {
      Flat.Clear();
      RunRecursiveOn(this, c => Flat.Add(c));
    }

    private double LastUpdateTime;
    public void Update(double totalTime)
    {
      sw.Restart();
      if (totalTime - LastUpdateTime >= UpdateInterval)
      {
        if (TreeChanged)
        {
          FlattenTree();
          TreeChanged = false;
        }

        HandleMouse();

        //RunStraigth(c => c.UpdateStateBeforeLayout());
        RunStraigth(c => c.Layout.Update());
        //RunStraigth(c => c.UpdateStateAfterLayout());

        OnUpdate?.Invoke();

        LastUpdateTime = totalTime;
      }

      CUI.EnsureCategory();
      CUI.Capture(sw.ElapsedTicks, "CUI.Update");
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
      sw.Restart();

      foreach (CUIComponent child in this.Children) { child.DrawRecursive(spriteBatch); }
      foreach (CUIComponent child in this.Children) { child.DrawFrontRecursive(spriteBatch); }

      CUI.EnsureCategory();
      CUI.Capture(sw.ElapsedTicks, "CUI.Draw");
    }


    private void HandleMouse()
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

      Mouse.Scan();

      //TODO why am i sure that grabbed have parent?
      if (!Mouse.Held)
      {
        Grabbed = null;
        if (GrabbedResizeHandle != null)
        {
          GrabbedResizeHandle.EndResize();
          GrabbedResizeHandle = null;
        }
      }

      if (Grabbed != null && Mouse.Moved)
      {
        Grabbed.TryDragTo(Mouse.Position - Grabbed.Parent.Real.Position - GrabbedOffset);
      }

      if (GrabbedResizeHandle != null && Mouse.Moved)
      {
        GrabbedResizeHandle.Resize(Mouse.Position);
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
        //skip Main component
        foreach (CUIComponent child in this.Children)
        {
          CheckIfContainsMouse(child);
        }
      }

      CurrentMouseOn = MouseOnList.LastOrDefault();

      //if (CurrentMouseOn != null) GUI.MouseOn = dummyComponent;

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
        if (GrabbedResizeHandle == null)
        {
          for (int i = MouseOnList.Count - 1; i >= 0; i--)
          {
            if (MouseOnList[i].RightResizeHandle.IsHit(Mouse.Position))
            {
              GrabbedResizeHandle = MouseOnList[i].RightResizeHandle;
              GrabbedResizeHandle.BeginResize(Mouse.Position);
              break;
            }

            if (MouseOnList[i].LeftResizeHandle.IsHit(Mouse.Position))
            {
              GrabbedResizeHandle = MouseOnList[i].LeftResizeHandle;
              GrabbedResizeHandle.BeginResize(Mouse.Position);
              break;
            }
          }
        }

        // Init Drag and drop?
        if (GrabbedResizeHandle == null && Grabbed == null)
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

      if (GrabbedResizeHandle == null && Grabbed == null)
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

    private void patchAll()
    {
      harmony.Patch(
        original: typeof(GUI).GetMethod("Draw", AccessTools.all),
        prefix: new HarmonyMethod(typeof(CUIMainComponent).GetMethod("CUIDraw", AccessTools.all))
      );

      harmony.Patch(
        original: typeof(GameMain).GetMethod("Update", AccessTools.all),
        postfix: new HarmonyMethod(typeof(CUIMainComponent).GetMethod("CUIUpdate", AccessTools.all))
      );

      harmony.Patch(
        original: typeof(GUI).GetMethod("UpdateMouseOn", AccessTools.all),
        postfix: new HarmonyMethod(typeof(CUIMainComponent).GetMethod("CUIBlockClicks", AccessTools.all))
      );

      harmony.Patch(
        original: typeof(Camera).GetMethod("MoveCamera", AccessTools.all),
        prefix: new HarmonyMethod(typeof(CUIMainComponent).GetMethod("CUIBlockScroll", AccessTools.all))
      );
    }

    private static void CUIUpdate(GameTime gameTime)
    {
      try { Main.Update(gameTime.TotalGameTime.TotalSeconds); }
      catch (Exception e) { CUI.log($"CUI: {e}", Color.Yellow); }
    }

    private static void CUIDraw(SpriteBatch spriteBatch)
    {
      try { Main.Draw(spriteBatch); }
      catch (Exception e) { CUI.log($"CUI: {e}", Color.Yellow); }
    }

    private static void CUIBlockClicks(ref GUIComponent __result)
    {
      if (GUI.MouseOn == null && Main.MouseOn != null) GUI.MouseOn = dummyComponent;
    }

    private static void CUIBlockScroll(float deltaTime, ref bool allowMove, ref bool allowZoom, bool allowInput, bool? followSub)
    {
      if (GUI.MouseOn == dummyComponent) allowZoom = false;
    }

    public void Load(Action<CUIMainComponent> initFunc)
    {
      RemoveAllChildren();
      initFunc(this);
    }

    public CUIMainComponent() : base()
    {
      Real = new CUIRect(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
      Visible = false;
      sw = new Stopwatch();

      harmony = new Harmony("crabui");

      if (Main == null)
      {
        Main = this;
        patchAll();
      }
    }
  }
}