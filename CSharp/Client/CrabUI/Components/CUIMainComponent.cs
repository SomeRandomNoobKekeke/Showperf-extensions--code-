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

namespace CrabUI
{
  public class CUIMainComponent : CUIComponent
  {
    public static CUIMainComponent Main;

    public long DrawTime;
    public long UpdateTime;
    public double UpdateInterval = 1.0 / 300.0;
    public event Action OnTreeChanged;
    public Action AddOnTreeChanged { set { OnTreeChanged += value; } }

    public CUIDragHandle GrabbedDragHandle;
    public CUIResizeHandle GrabbedResizeHandle;
    public CUISwipeHandle GrabbedSwipeHandle;
    public CUIComponent MouseOn;

    private Stopwatch sw;

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

      //RunRecursiveOn(this, (component, depth) => Flat.Add(component));
    }

    #region Update

    private int OKurwa = 0;
    private double LastUpdateTime;
    public void Update(double totalTime)
    {
      if (totalTime - LastUpdateTime >= UpdateInterval)
      {
        sw.Restart();

        if (OKurwa++ == 0) CUIDebug.Flush();
        if (OKurwa == 1) OKurwa = 0;

        if (TreeChanged)
        {
          OnTreeChanged?.Invoke();

          FlattenTree();
          TreeChanged = false;
        }


        CUIDebug.Capture(null, this, "Update", "", "HandleMouse", "");
        HandleMouse(totalTime);

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

        sw.Stop();
        CUIDebug.EnsureCategory();
        CUIDebug.CaptureTicks(sw.ElapsedTicks, "CUI.Update");


        RunStraigth(c => c.InvokeOnUpdate());

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
    public new void Draw(SpriteBatch spriteBatch)
    {
      sw.Restart();

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

      sw.Stop();
      CUIDebug.EnsureCategory();
      CUIDebug.CaptureTicks(sw.ElapsedTicks, "CUI.Draw");
    }
    #endregion
    // https://youtu.be/xuFgUmYCS8E?feature=shared&t=72
    #region HandleMouse Start 

    public void OnDragEnd(CUIDragHandle h) { if (h == GrabbedDragHandle) GrabbedDragHandle = null; }
    public void OnResizeEnd(CUIResizeHandle h) { if (h == GrabbedResizeHandle) GrabbedResizeHandle = null; }
    public void OnSwipeEnd(CUISwipeHandle h) { if (h == GrabbedSwipeHandle) GrabbedSwipeHandle = null; }


    private void HandleMouse(double totalTime)
    {
      CUI.Input.Scan(totalTime);

      if (!CUI.Input.SomethingHappened) return;

      if (!CUI.Input.MouseHeld)
      {
        GrabbedDragHandle?.EndDrag();
        GrabbedResizeHandle?.EndResize();
        GrabbedSwipeHandle?.EndSwipe();
      }

      if (CUI.Input.MouseMoved)
      {
        GrabbedDragHandle?.DragTo(CUI.Input.MousePosition);
        GrabbedResizeHandle?.Resize(CUI.Input.MousePosition);
        GrabbedSwipeHandle?.Swipe(CUI.Input);
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
          bool ok = !c.IgnoreEvents && c.Real.Contains(CUI.Input.MousePosition);

          if (c.Parent != null && c.Parent.ScissorRect.HasValue &&
              !c.Parent.ScissorRect.Value.Contains(CUI.Input.CurrentMouseState.Position))
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
        MouseOn?.InvokeOnMouseLeave(CUI.Input);
        CurrentMouseOn?.InvokeOnMouseEnter(CUI.Input);

        MouseOn = CurrentMouseOn;
      }

      // Resize
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        if (MouseOnList[i].RightResizeHandle.ShouldStart(CUI.Input))
        {
          GrabbedResizeHandle = MouseOnList[i].RightResizeHandle;
          GrabbedResizeHandle.BeginResize(CUI.Input.MousePosition);
          break;
        }

        if (MouseOnList[i].LeftResizeHandle.ShouldStart(CUI.Input))
        {
          GrabbedResizeHandle = MouseOnList[i].LeftResizeHandle;
          GrabbedResizeHandle.BeginResize(CUI.Input.MousePosition);
          break;
        }
      }
      if (GrabbedResizeHandle != null) return;

      //Scroll
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        if (CUI.Input.Scrolled) MouseOnList[i].InvokeOnScroll(CUI.Input.Scroll);

        if (MouseOnList[i].ConsumeMouseScroll) break;
      }

      //Clicks
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        //TODO mb this should be applied  separately
        MouseOnList[i].MousePressed = CUI.Input.MouseHeld;
        MouseOnList[i].MouseOver = true;

        if (CUI.Input.MouseDown) MouseOnList[i].InvokeOnMouseDown(CUI.Input);
        if (CUI.Input.MouseUp) MouseOnList[i].InvokeOnMouseUp(CUI.Input);
        if (CUI.Input.DoubleClick) MouseOnList[i].InvokeOnDClick(CUI.Input);

        if (MouseOnList[i].ConsumeMouseClicks || CUI.Input.ClickConsumed) break;
      }
      if (CUI.Input.ClickConsumed) return;

      // Swipe
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        if (MouseOnList[i].SwipeHandle.ShouldStart(CUI.Input))
        {
          GrabbedSwipeHandle = MouseOnList[i].SwipeHandle;
          GrabbedSwipeHandle.BeginSwipe(CUI.Input.MousePosition);
          break;
        }

        if (MouseOnList[i].ConsumeSwipe) break;
      }
      if (GrabbedSwipeHandle != null) return;

      // Drag
      for (int i = MouseOnList.Count - 1; i >= 0; i--)
      {
        if (MouseOnList[i].DragHandle.ShouldStart(CUI.Input))
        {
          GrabbedDragHandle = MouseOnList[i].DragHandle;
          GrabbedDragHandle.BeginDrag(CUI.Input.MousePosition);
          break;
        }

        if (MouseOnList[i].ConsumeDragAndDrop) break;
      }
      if (GrabbedDragHandle != null) return;


    }
    #endregion
    #region HandleMouse End
    #endregion


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

      Main = this;
      CUI.Initialize();
    }
  }
}