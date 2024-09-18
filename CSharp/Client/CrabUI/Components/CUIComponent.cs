using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

    public static Vector2 GameScreenSize => new Vector2(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
    public static GUIButton dummyComponent = new GUIButton(new RectTransform(new Point(0, 0)));

    public List<CUIComponent> Children = new List<CUIComponent>();

    private CUIComponent? parent; public CUIComponent? Parent
    {
      get => parent;
      set { parent = value; TreeChanged = true; Layout.Changed = true; }
    }

    public virtual CUILayout Layout { get; set; }

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
        InvokeOnChildAdded(c);
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
      InvokeOnChildRemoved(c);
    }

    public virtual void RemoveAllChildren() => removeAllChildren();
    protected void removeAllChildren()
    {
      foreach (CUIComponent c in Children)
      {
        c.Parent = null;
      }
      Children.Clear();
      TreeChanged = true;
      InvokeOnAllChildrenRemoved();
    }

    public event Action<CUIComponent> OnChildAdded; protected void InvokeOnChildAdded(CUIComponent c) => OnChildAdded?.Invoke(c);
    public event Action<CUIComponent> OnChildRemoved; protected void InvokeOnChildRemoved(CUIComponent c) => OnChildRemoved?.Invoke(c);
    public event Action OnAllChildrenRemoved; protected void InvokeOnAllChildrenRemoved() => OnAllChildrenRemoved?.Invoke();





    internal virtual void UpdatePseudoChildren()
    {
      //TODO unhardcode
      ResizeHandle = new CUIRect(Real.Right - 9, Real.Bottom - 9, 9, 9);
    }
    // protected virtual void UpdateStateBeforeLayout() { }
    // protected virtual void UpdateStateAfterLayout() { }

    internal virtual Vector2 AmIOkWithThisSize(Vector2 size) => size;
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

    private Vector2 childrenOffset; public Vector2 ChildrenOffset
    {
      get => childrenOffset;
      set
      {
        childrenOffset = value;
        OnChildrenPropChanged();
      }
    }

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
    protected CUIRect BorderBox;

    public bool MouseOver { get; set; }
    public bool MousePressed { get; set; }
    public bool PassMouseClicks { get; set; } = true;
    public bool PassDragAndDrop { get; set; } = true;
    public bool PassMouseScroll { get; set; } = true;

    // Without wrappers they will throw FieldAccessException
    public event Action<CUIMouse> OnMouseLeave; protected void InvokeOnMouseLeave(CUIMouse m) => OnMouseLeave?.Invoke(m);
    public event Action<CUIMouse> OnMouseEnter; protected void InvokeOnMouseEnter(CUIMouse m) => OnMouseEnter?.Invoke(m);
    public event Action<CUIMouse> OnMouseDown; protected void InvokeOnMouseDown(CUIMouse m) => OnMouseDown?.Invoke(m);
    public event Action<CUIMouse> OnMouseUp; protected void InvokeOnMouseUp(CUIMouse m) => OnMouseUp?.Invoke(m);
    public event Action<CUIMouse> OnDClick; protected void InvokeOnDClick(CUIMouse m) => OnDClick?.Invoke(m);
    public event Action<float> OnScroll; protected void InvokeOnScroll(float scroll) => OnScroll?.Invoke(scroll);
    public event Action<float, float> OnDrag; protected void InvokeOnDrag(float x, float y) => OnDrag?.Invoke(x, y);

    public bool Dragable { get; set; }

    // probably should be a NullRect
    protected virtual CUIRect DragZone => Parent.Real;

    protected void TryDragTo(Vector2 to)
    {
      if (Parent == null) return;

      float newX = to.X;
      float newY = to.Y;

      if (newX + Real.Width > DragZone.Width) newX = DragZone.Width - Real.Width;
      if (newY + Real.Height > DragZone.Height) newY = DragZone.Height - Real.Height;

      if (newX < DragZone.Left) newX = DragZone.Left;
      if (newY < DragZone.Top) newY = DragZone.Top;

      Absolute.Left = newX;
      Absolute.Top = newY;

      InvokeOnDrag(newX, newY);
    }

    public bool Resizible { get; set; }
    protected CUIRect ResizeHandle { get; set; }

    protected void TryToResize(Vector2 newSize)
    {
      Absolute.Width = Math.Max(ResizeHandle.Width, newSize.X);
      Absolute.Height = Math.Max(ResizeHandle.Height, newSize.Y);
    }


    internal bool DecorChanged { get; set; }
    public Color BackgroundColor = Color.Black * 0.5f;
    public Color BorderColor = Color.White * 0.5f;
    public float BorderThickness = 1f;
    private Vector2 padding = new Vector2(2, 2); public Vector2 Padding
    {
      get => padding;
      set { padding = value; DecorChanged = true; }
    }


    public bool Visible { get; set; } = true;
    public bool HideChildrenOutsideFrame { get; set; } = false;
    protected virtual void Draw(SpriteBatch spriteBatch)
    {
      GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, BackgroundColor, isFilled: true);
      GUI.DrawRectangle(spriteBatch, BorderBox.Position, BorderBox.Size, BorderColor, thickness: BorderThickness);

      if (Resizible)
      {
        GUI.DrawRectangle(spriteBatch, ResizeHandle.Position, ResizeHandle.Size, BorderColor, isFilled: true);
      }
    }

    protected virtual void DrawFront(SpriteBatch spriteBatch) { }

    protected void DrawFrontRecursive(SpriteBatch spriteBatch)
    {
      if (Visible) DrawFront(spriteBatch);
      Children.ForEach(c => c.DrawFrontRecursive(spriteBatch));
    }

    protected void DrawRecursive(SpriteBatch spriteBatch)
    {
      if (Debug) log(this);

      if (Visible) Draw(spriteBatch);

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

      // OnMouseUp += (CUIMouse m) => Mod.log($"OnMouseUp {this}");
      // OnMouseDown += (CUIMouse m) => Mod.log($"OnMouseDown {this}");
      // OnMouseEnter += (CUIMouse m) => Mod.log($"OnMouseEnter {this}");
      // OnMouseLeave += (CUIMouse m) => Mod.log($"OnMouseLeave {this}");
    }

    public CUIComponent(float? x, float? y, float? w, float? h) : this()
    {
      Relative.Set(x, y, w, h);
    }

    protected static void RunRecursiveOn(CUIComponent component, Action<CUIComponent> action, int depth = 0)
    {
      action(component);
      foreach (CUIComponent child in component.Children)
      {
        RunRecursiveOn(child, action, depth + 1);
      }
    }

    public override string ToString() => $"{base.ToString()}:{ID} {Real} A:{Absolute} R:{Relative}";

    public static void log(object msg, Color? cl = null)
    {
      cl ??= Color.Cyan;
      LuaCsLogger.LogMessage($"{msg ?? "null"}", cl * 0.8f, cl);
    }
  }
}