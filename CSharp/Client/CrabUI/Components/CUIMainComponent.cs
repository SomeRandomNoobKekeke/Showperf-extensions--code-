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
    public event Action OnTreeChanged;

    public CUIDragHandle GrabbedDragHandle;
    public CUIResizeHandle GrabbedResizeHandle;
    public CUISwipeHandle GrabbedSwipeHandle;
    public CUIComponent MouseOn;
    public CUIMouse Mouse = new CUIMouse();


    private Stopwatch sw;
    private Harmony harmony;
    private List<CUIComponent> Flat = new List<CUIComponent>();
    private List<CUIComponent> Leaves = new List<CUIComponent>();
    private SortedList<int, List<CUIComponent>> Layers = new SortedList<int, List<CUIComponent>>();
    private List<CUIComponent> MouseOnList = new List<CUIComponent>();
    private Vector2 GrabbedOffset;


    internal override CUINullRect ChildrenBoundaries => new CUINullRect(0, 0, Real.Width, Real.Height);
    private void RunStraigth(Action<CUIComponent> a) { for (int i = 0; i < Flat.Count; i++) a(Flat[i]); }
    private void RunReverse(Action<CUIComponent> a) { for (int i = Flat.Count - 1; i >= 0; i--) a(Flat[i]); }

    private void FlattenTree()
    {
      Flat.Clear();
      Layers.Clear();
      // Leaves.Clear();

      RunRecursiveOn(this, (component, depth) =>
      {
        int d = component.ZIndex ?? depth;

        if (!Layers.ContainsKey(d)) Layers[d] = new List<CUIComponent>();
        Layers[d].Add(component);
      });

      foreach (var layer in Layers)
      {
        Flat.AddRange(layer.Value);
      }

      // RunRecursiveOn(this, (component, depth) => Flat.Add(component));
    }

    #region Update

    private double LastUpdateTime;
    public void Update(double totalTime)
    {

      if (totalTime - LastUpdateTime >= UpdateInterval)
      {
        CUIDebug.Flush();

        if (TreeChanged)
        {
          OnTreeChanged?.Invoke();

          FlattenTree();
          TreeChanged = false;
        }

        CUIDebug.Capture(null, this, "Update", "", "HandleMouse", "");
        HandleMouse();

        CUIDebug.Capture(null, this, "Update", "", "RunReverse", "");
        RunReverse(c =>
        {
          c.Layout.ResizeToContent();
        });

        CUIDebug.Capture(null, this, "Update", "", "RunStraigth", "");
        RunStraigth(c =>
        {
          c.Layout.Update();
          c.Layout.UpdateDecor();
        });



        //HACK BaroDev(wide)
        // RunStraigth(c =>
        // {
        //   c.Layout.Changed = false;
        //   c.Layout.DecorChanged = false;
        //   c.Layout.AbsoluteChanged = false;
        // });



        OnUpdate?.Invoke();

        LastUpdateTime = totalTime;
      }
    }

    #endregion
    #region Draw

    private void StopStart(SpriteBatch spriteBatch, Rectangle SRect)
    {
      spriteBatch.End();
      spriteBatch.GraphicsDevice.ScissorRectangle = SRect;
      spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: GUI.SamplerState, rasterizerState: GameMain.ScissorTestEnable);
    }

    //TODO lock flat, new components are blinking
    protected override void Draw(SpriteBatch spriteBatch)
    {
      Rectangle OriginalSRect = spriteBatch.GraphicsDevice.ScissorRectangle;
      Rectangle SRect = OriginalSRect;

      try
      {
        RunStraigth(c =>
        {
          if (!c.Visible || c.CulledOut) return;
          if (c.Parent != null && c.Parent.ScissorRect.HasValue && SRect != c.Parent.ScissorRect.Value)
          {
            SRect = c.Parent.ScissorRect.Value;
            StopStart(spriteBatch, SRect);
          }
          c.Draw(spriteBatch);
        });
      }
      finally
      {
        if (spriteBatch.GraphicsDevice.ScissorRectangle != OriginalSRect) StopStart(spriteBatch, OriginalSRect);
      }

      RunStraigth(c =>
      {
        if (!c.Visible || c.CulledOut) return;
        c.DrawFront(spriteBatch);
      });
    }
    #endregion
    // https://youtu.be/xuFgUmYCS8E?feature=shared&t=72
    #region HandleMouse Start 

    public void OnDragEnd(CUIDragHandle h) { if (h == GrabbedDragHandle) GrabbedDragHandle = null; }
    public void OnResizeEnd(CUIResizeHandle h) { if (h == GrabbedResizeHandle) GrabbedResizeHandle = null; }
    public void OnSwipeEnd(CUISwipeHandle h) { if (h == GrabbedSwipeHandle) GrabbedSwipeHandle = null; }


    private void HandleMouse()
    {
      Mouse.Scan();

      if (!Mouse.SomethingHappened) return;

      if (!Mouse.Held)
      {
        GrabbedDragHandle?.EndDrag();
        GrabbedResizeHandle?.EndResize();
        GrabbedSwipeHandle?.EndSwipe();
      }

      if (Mouse.Moved)
      {
        GrabbedDragHandle?.DragTo(Mouse.Position);
        GrabbedResizeHandle?.Resize(Mouse.Position);
        GrabbedSwipeHandle?.Swipe(Mouse);
      }

      //TODO think where should i put it?
      if (GrabbedResizeHandle != null || GrabbedDragHandle != null || GrabbedSwipeHandle != null) return;

      // just deep clear of prev mouse pressed state
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        MouseOnList[i].MousePressed = false;
        MouseOnList[i].MouseOver = false;
      }

      CUIComponent CurrentMouseOn = null;
      MouseOnList.Clear();

      // form MouseOnList
      if (GUI.MouseOn == null || GUI.MouseOn == dummyComponent)
      {
        RunStraigth(c =>
        {
          bool ok = !c.IgnoreEvents && c.Real.Contains(Mouse.Position);

          if (c.Parent != null && c.Parent.ScissorRect.HasValue &&
              !c.Parent.ScissorRect.Value.Contains(Mouse.CurrentState.Position))
          {
            ok = false;
          }

          if (ok) MouseOnList.Add(c);
        });
      }

      CurrentMouseOn = MouseOnList.LastOrDefault();

      //if (CurrentMouseOn != null) GUI.MouseOn = dummyComponent;

      //TODO those event are a bit useless, mb add similar bubbling events?
      //Enter / Leave
      if (CurrentMouseOn != MouseOn)
      {
        MouseOn?.InvokeOnMouseLeave(Mouse);
        CurrentMouseOn?.InvokeOnMouseEnter(Mouse);

        MouseOn = CurrentMouseOn;
      }

      // Resize
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        if (MouseOnList[i].RightResizeHandle.ShouldStart(Mouse))
        {
          GrabbedResizeHandle = MouseOnList[i].RightResizeHandle;
          GrabbedResizeHandle.BeginResize(Mouse.Position);
          break;
        }

        if (MouseOnList[i].LeftResizeHandle.ShouldStart(Mouse))
        {
          GrabbedResizeHandle = MouseOnList[i].LeftResizeHandle;
          GrabbedResizeHandle.BeginResize(Mouse.Position);
          break;
        }
      }
      if (GrabbedResizeHandle != null) return;

      //Scroll
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        if (Mouse.Scrolled) MouseOnList[i].InvokeOnScroll(Mouse.Scroll);

        if (MouseOnList[i].ConsumeMouseScroll) break;
      }

      //Clicks
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        //TODO mb this should be applied  separately
        MouseOnList[i].MousePressed = Mouse.Held;
        MouseOnList[i].MouseOver = true;

        if (Mouse.Down) MouseOnList[i].InvokeOnMouseDown(Mouse);
        if (Mouse.Up) MouseOnList[i].InvokeOnMouseUp(Mouse);
        if (Mouse.DoubleClick) MouseOnList[i].InvokeOnDClick(Mouse);

        if (MouseOnList[i].ConsumeMouseClicks) break;
      }
      if (Mouse.ClickConsumed) return;

      // Swipe
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        if (MouseOnList[i].SwipeHandle.ShouldStart(Mouse))
        {
          GrabbedSwipeHandle = MouseOnList[i].SwipeHandle;
          GrabbedSwipeHandle.BeginSwipe(Mouse.Position);
          break;
        }

        if (MouseOnList[i].ConsumeSwipe) break;
      }
      if (GrabbedSwipeHandle != null) return;

      // Drag
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        if (MouseOnList[i].DragHandle.ShouldStart(Mouse))
        {
          GrabbedDragHandle = MouseOnList[i].DragHandle;
          GrabbedDragHandle.BeginDrag(Mouse.Position);
          break;
        }

        if (MouseOnList[i].ConsumeDragAndDrop) break;
      }
      if (GrabbedDragHandle != null) return;


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
      if (GUI.MouseOn == null && Main.MouseOn != null && Main.MouseOn != Main) GUI.MouseOn = dummyComponent;
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
      //IgnoreEvents = true;
      ShouldPassPropsToChildren = false;
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