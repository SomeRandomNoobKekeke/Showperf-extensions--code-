using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public partial class CUIComponent
  {
    public static int MaxID = 0;
    public static Dictionary<int, CUIComponent> ComponentsById = new Dictionary<int, CUIComponent>();
    public int ID;
    public bool Debug;
    public Stopwatch sw = new Stopwatch();

    public static Vector2 GameScreenSize => new Vector2(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
    public static GUIButton dummyComponent = new GUIButton(new RectTransform(new Point(0, 0)));


    #region Tree
    public List<CUIComponent> Children = new List<CUIComponent>();

    private CUIComponent? parent; public CUIComponent? Parent
    {
      get => parent;
      set { parent = value; TreeChanged = true; Layout.Changed = true; }
    }
    private bool treeChanged = true; public bool TreeChanged
    {
      get => treeChanged;
      set { treeChanged = value; if (value && Parent != null) Parent.TreeChanged = true; }
    }

    public virtual CUIComponent Append(CUIComponent c) => append(c);
    protected CUIComponent append(CUIComponent c)
    {
      if (c != null)
      {
        c.Parent = this;
        Children.Add(c);
      }
      return c;
    }

    public virtual void RemoveChild(CUIComponent c) => removeChild(c);
    protected void removeChild(CUIComponent c)
    {
      if (c == null || !Children.Contains(c)) return;
      c.Parent = null;
      TreeChanged = true;
      Children.Remove(c);
    }

    public virtual void RemoveAllChildren() => removeAllChildren();
    protected void removeAllChildren()
    {
      foreach (CUIComponent c in Children) { c.Parent = null; }
      Children.Clear();
      TreeChanged = true;
    }
    #endregion
    #region Layout
    public virtual CUILayout Layout { get; set; }

    internal virtual CUINullRect ChildrenBoundaries => new CUINullRect(null, null, null, null);
    private Vector2 childrenOffset; public Vector2 ChildrenOffset
    {
      get => childrenOffset;
      set
      {
        childrenOffset = value;
        OnChildrenPropChanged();
      }
    }
    internal virtual void UpdatePseudoChildren()
    {
      LeftResizeHandle.Update();
      RightResizeHandle.Update();
    }
    // protected virtual void UpdateStateBeforeLayout() { }
    // protected virtual void UpdateStateAfterLayout() { }

    internal virtual Vector2 AmIOkWithThisSize(Vector2 size) => size;

    #endregion
    #region Events

    internal virtual void ChildrenSizeCalculated() { }

    internal void OnPropChanged()
    {
      Layout.Changed = true;
    }

    internal void OnChildrenPropChanged()
    {
      foreach (CUIComponent child in Children)
      {
        child.Layout.Changed = true;
      }
    }

    public bool MouseOver { get; set; }
    public bool MousePressed { get; set; }
    public bool PassMouseClicks { get; set; } = true;
    public bool PassDragAndDrop { get; set; } = true;
    public bool PassSwipe { get; set; } = true;


    // Without wrappers they will throw FieldAccessException
    public event Action<CUIMouse> OnMouseLeave; internal void InvokeOnMouseLeave(CUIMouse m) => OnMouseLeave?.Invoke(m);
    public event Action<CUIMouse> OnMouseEnter; internal void InvokeOnMouseEnter(CUIMouse m) => OnMouseEnter?.Invoke(m);
    public event Action<CUIMouse> OnMouseDown; internal void InvokeOnMouseDown(CUIMouse m) => OnMouseDown?.Invoke(m);
    public event Action<CUIMouse> OnMouseUp; internal void InvokeOnMouseUp(CUIMouse m) => OnMouseUp?.Invoke(m);
    public event Action<CUIMouse> OnDClick; internal void InvokeOnDClick(CUIMouse m) => OnDClick?.Invoke(m);
    public event Action<float> OnScroll; internal void InvokeOnScroll(float scroll) => OnScroll?.Invoke(scroll);
    public event Action<float, float> OnDrag; internal void InvokeOnDrag(float x, float y) => OnDrag?.Invoke(x, y);
    public event Action<float, float> OnSwipe; internal void InvokeOnSwipe(float x, float y) => OnSwipe?.Invoke(x, y);


    //TODO rethink
    //protected virtual CUINullRect DragZone => new CUINullRect(null, null, null, null);

    public CUIDragHandle DragHandle;
    public bool Draggable
    {
      get => DragHandle.Draggable;
      set => DragHandle.Draggable = value;
    }
    public CUIResizeHandle LeftResizeHandle;
    public CUIResizeHandle RightResizeHandle;

    public bool Resizible
    {
      get => LeftResizeHandle.Visible || RightResizeHandle.Visible;
      set { LeftResizeHandle.Visible = value; RightResizeHandle.Visible = value; }
    }

    public CUISwipeHandle SwipeHandle;
    public bool Swipeable
    {
      get => SwipeHandle.Swipeable;
      set => SwipeHandle.Swipeable = value;
    }

    #endregion
    #region Props
    public CUIAnchor Anchor = new CUIAnchor(CUIAnchorType.LeftTop);

    private CUINullRect absolute; public CUINullRect Absolute
    {
      get => absolute;
      set { absolute = value; absolute.Host = this; }
    }

    private CUINullRect absoluteMin; public CUINullRect AbsoluteMin
    {
      get => absoluteMin;
      set { absoluteMin = value; absoluteMin.Host = this; }
    }

    private CUINullRect absoluteMax; public CUINullRect AbsoluteMax
    {
      get => absoluteMax;
      set { absoluteMax = value; absoluteMax.Host = this; }
    }

    private CUINullRect relative; public CUINullRect Relative
    {
      get => relative;
      set { relative = value; relative.Host = this; }
    }

    private CUINullRect relativeMin; public CUINullRect RelativeMin
    {
      get => relativeMin;
      set { relativeMin = value; relativeMin.Host = this; }
    }

    private CUINullRect relativeMax; public CUINullRect RelativeMax
    {
      get => relativeMax;
      set { relativeMax = value; relativeMax.Host = this; }
    }


    private bool fillEmptySpace; public bool FillEmptySpace
    {
      get => fillEmptySpace;
      set { fillEmptySpace = value; OnPropChanged(); }
    }

    protected CUIRect BorderBox;
    private CUIRect real; public virtual CUIRect Real
    {
      get => real;
      set
      {
        real = value;
        BorderBox = new CUIRect(
          real.Left - BorderThickness,
          real.Top - BorderThickness,
          real.Width + BorderThickness * 2,
          real.Height + BorderThickness * 2
        );
      }
    }

    #endregion
    #region Graphic Props
    internal bool DecorChanged { get; set; }
    public bool BackgroundVisible = true;
    private Color backgroundColor = Color.Black * 0.5f; public Color BackgroundColor
    {
      get => backgroundColor;
      set { backgroundColor = value; BackgroundVisible = backgroundColor != Color.Transparent; }
    }

    public bool BorderVisible = true;
    private Color borderColor = Color.White * 0.5f; public Color BorderColor
    {
      get => borderColor;
      set { borderColor = value; BorderVisible = borderColor != Color.Transparent; }
    }

    public float BorderThickness = 1f;
    private Vector2 padding = new Vector2(2, 2); public Vector2 Padding
    {
      get => padding;
      set { padding = value; DecorChanged = true; }
    }

    public bool Visible { get; set; } = true;
    public bool HideChildrenOutsideFrame { get; set; } = false;

    #endregion
    #region Methods
    protected virtual void Draw(SpriteBatch spriteBatch)
    {
      if (BackgroundVisible) GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, BackgroundColor, isFilled: true);

      if (BorderVisible) GUI.DrawRectangle(spriteBatch, BorderBox.Position, BorderBox.Size, BorderColor, thickness: BorderThickness);

      LeftResizeHandle.Draw(spriteBatch);
      RightResizeHandle.Draw(spriteBatch);
    }

    protected virtual void DrawFront(SpriteBatch spriteBatch) { }

    protected void DrawFrontRecursive(SpriteBatch spriteBatch)
    {
      if (Visible) DrawFront(spriteBatch);
      Children.ForEach(c => c.DrawFrontRecursive(spriteBatch));
    }

    protected void DrawRecursive(SpriteBatch spriteBatch)
    {
      // if (Debug) CUI.log(this);
      if (!Visible) return;

      sw.Restart();
      Draw(spriteBatch);
      //CUI.Capture(sw.ElapsedTicks, $"CUI.Draw:{base.ToString()}");
      sw.Stop();

      Rectangle prevScissorRect = spriteBatch.GraphicsDevice.ScissorRectangle;

      if (HideChildrenOutsideFrame)
      {
        spriteBatch.End();
        spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(prevScissorRect, Real.Box);
        spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: GUI.SamplerState, rasterizerState: GameMain.ScissorTestEnable);
      }

      Children.ForEach(c => c.DrawRecursive(spriteBatch));

      if (HideChildrenOutsideFrame)
      {
        spriteBatch.End();
        spriteBatch.GraphicsDevice.ScissorRectangle = prevScissorRect;
        spriteBatch.Begin(SpriteSortMode.Deferred, samplerState: GUI.SamplerState, rasterizerState: GameMain.ScissorTestEnable);
      }
    }
    #endregion
    #region Constructors
    public CUIComponent()
    {
      ID = MaxID++;
      ComponentsById[ID] = this;

      Layout = new CUILayoutSimple(this);

      Absolute = new CUINullRect();
      AbsoluteMin = new CUINullRect();
      AbsoluteMax = new CUINullRect();
      Relative = new CUINullRect();
      RelativeMin = new CUINullRect();
      RelativeMax = new CUINullRect();

      DragHandle = new CUIDragHandle(this);
      SwipeHandle = new CUISwipeHandle(this);
      LeftResizeHandle = new CUIResizeHandle(this, CUIAnchorType.LeftBottom);
      RightResizeHandle = new CUIResizeHandle(this, CUIAnchorType.RightBottom);
    }

    public CUIComponent(float? x, float? y, float? w, float? h) : this()
    {
      Relative.Set(x, y, w, h);
    }
    #endregion

    protected static void RunRecursiveOn(CUIComponent component, Action<CUIComponent> action, int depth = 0)
    {
      action(component);
      foreach (CUIComponent child in component.Children)
      {
        RunRecursiveOn(child, action, depth + 1);
      }
    }

    public string Type() => base.ToString();

    public override string ToString() => $"{base.ToString()}:{ID} {Real} {Anchor.Type} A:{Absolute} R:{Relative} AMin:{AbsoluteMin} RMin:{RelativeMin} AMax:{AbsoluteMax} RMax:{RelativeMax}";
  }
}