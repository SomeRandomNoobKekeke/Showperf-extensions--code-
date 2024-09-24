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
    public static CUIMainComponent Main;

    public long DrawTime;
    public long UpdateTime;
    public double UpdateInterval = 1.0 / 300.0;
    public event Action OnUpdate;


    public CUIDragHandle GrabbedDragHandle;
    public CUIResizeHandle GrabbedResizeHandle;
    public CUISwipeHandle GrabbedSwipeHandle;
    public CUIComponent MouseOn;
    public CUIMouse Mouse = new CUIMouse();


    private Stopwatch sw;
    private Harmony harmony;
    private List<CUIComponent> Flat = new List<CUIComponent>();
    private List<CUIComponent> MouseOnList = new List<CUIComponent>();
    private Vector2 GrabbedOffset;



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

    //  https://youtu.be/xuFgUmYCS8E?feature=shared&t=72
    #region HandleMouse Start 

    public void OnDragEnd(CUIDragHandle h) { if (h == GrabbedDragHandle) GrabbedDragHandle = null; }
    public void OnResizeEnd(CUIResizeHandle h) { if (h == GrabbedResizeHandle) GrabbedResizeHandle = null; }
    public void OnSwipeEnd(CUISwipeHandle h) { if (h == GrabbedSwipeHandle) GrabbedSwipeHandle = null; }


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

      if (!Mouse.SomethingHappened) return;

      if (!Mouse.Held)
      {
        GrabbedDragHandle?.EndDrag();
        GrabbedResizeHandle?.EndResize();
      }

      if (Mouse.Moved)
      {
        GrabbedDragHandle?.DragTo(Mouse.Position);
        GrabbedResizeHandle?.Resize(Mouse.Position);
      }

      if (GrabbedResizeHandle != null || GrabbedDragHandle != null || GrabbedSwipeHandle != null) return;

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
      if (GrabbedResizeHandle != null) return;


      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        if (MouseOnList[i].DragHandle.Draggable)
        {
          GrabbedDragHandle = MouseOnList[i].DragHandle;
          GrabbedDragHandle.BeginDrag(Mouse.Position);
          break;
        }

        if (!MouseOnList[i].PassDragAndDrop) break;
      }
      if (GrabbedDragHandle != null) return;



      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        MouseOnList[i].MousePressed = Mouse.Held;

        if (Mouse.Down) MouseOnList[i].InvokeOnMouseDown(Mouse);
        if (Mouse.Up) MouseOnList[i].InvokeOnMouseUp(Mouse);
        if (Mouse.DoubleClick) MouseOnList[i].InvokeOnDClick(Mouse);
        if (Mouse.Scrolled) MouseOnList[i].InvokeOnScroll(Mouse.Scroll);

        if (!MouseOnList[i].PassMouseClicks) break;
      }
    }
    #endregion
    #region HandleMouse End
    #endregion
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