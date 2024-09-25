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
    public static Rectangle GameScreenRect => new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
    public static GUIButton dummyComponent = new GUIButton(new RectTransform(new Point(0, 0)));


    #region Tree
    public List<CUIComponent> Children = new List<CUIComponent>();

    private CUIComponent? parent; public CUIComponent? Parent
    {
      get => parent;
      set { parent = value; TreeChanged = true; OnPropChanged(); }
    }
    private bool treeChanged = true; public bool TreeChanged
    {
      get => treeChanged;
      set { treeChanged = value; if (value && Parent != null) Parent.TreeChanged = true; }
    }

    public int? ZIndex;

    // public int Depth;
    // public void DepthImpulse(int d)
    // {
    //   Depth = Math.Max(Depth, d);
    //   if (Parent != null) Parent.DepthImpulse(d + 1);
    // }

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

    // guh
    public bool IgnoreEvents;

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
    public bool ConsumeMouseClicks { get; set; }
    public bool ConsumeDragAndDrop { get; set; }
    public bool ConsumeSwipe { get; set; }
    public bool ConsumeMouseScroll { get; set; }


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

    protected Rectangle? ScissorRect;
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

        if (HideChildrenOutsideFrame)
        {
          Rectangle SRect = new Rectangle(
            (int)real.Left + 1,
            (int)real.Top + 1,
            (int)real.Width - 1,
            (int)real.Height - 1
          );

          if (Parent?.ScissorRect != null)
          {
            ScissorRect = Rectangle.Intersect(Parent.ScissorRect.Value, SRect);
          }
          else
          {
            ScissorRect = SRect;
          }
        }
        else ScissorRect = Parent?.ScissorRect;
      }
    }

    #endregion
    #region Graphic Props
    internal bool DecorChanged { get; set; }
    public bool BackgroundVisible = true;
    private Color backgroundColor = CUIColors.ComponentBackground; public Color BackgroundColor
    {
      get => backgroundColor;
      set { backgroundColor = value; BackgroundVisible = backgroundColor != Color.Transparent; }
    }

    public bool BorderVisible = true;
    private Color borderColor = CUIColors.ComponentBorder; public Color BorderColor
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

    protected static void RunRecursiveOn(CUIComponent component, Action<CUIComponent, int> action, int depth = 0)
    {
      action(component, depth);
      foreach (CUIComponent child in component.Children)
      {
        RunRecursiveOn(child, action, depth + 1);
      }
    }

    public override string ToString() => $"{base.ToString()}:{ID}";
    public void Info(object msg) => CUI.log($"{this} {msg}");
    public void PrintLayout() => Info($"{Real} {Anchor.Type} A:{Absolute} R:{Relative} AMin:{AbsoluteMin} RMin:{RelativeMin} AMax:{AbsoluteMax} RMax:{RelativeMax}");

  }
}