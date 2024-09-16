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

    public CUIComponent? parent; public CUIComponent? Parent
    {
      get => parent;
      set { parent = value; TreeChanged = true; Layout.Changed = true; }
    }

    public bool treeChanged = true; public bool TreeChanged
    {
      get => treeChanged;
      set { treeChanged = value; if (value && Parent != null) Parent.TreeChanged = true; }
    }


    public CUIComponent append(CUIComponent c)
    {
      c.Parent = this;
      Children.Add(c);
      return c;
    }
    public virtual CUIComponent Append(CUIComponent c) => append(c);
    public void removeChild(CUIComponent c)
    {
      if (!Children.Contains(c)) return;
      c.Parent = null;
      TreeChanged = true;
      Children.Remove(c);
    }
    public virtual void RemoveChild(CUIComponent c) => removeChild(c);

    public void removeAllChildren()
    {
      foreach (CUIComponent c in Children)
      {
        c.Parent = null;
      }
      Children.Clear();
      TreeChanged = true;
    }

    public virtual void RemoveAllChildren() => removeAllChildren();


    public virtual CUILayout Layout { get; set; }

    public void OnPropChanged()
    {
      Layout.Changed = true;
    }

    public virtual void UpdateOwnLayout()
    {
      //TODO unhardcode
      ResizeHandle = new CUIRect(Real.Right - 9, Real.Bottom - 9, 8, 8);
    }


    public CUINullRect Relative;
    public CUINullRect Absolute;
    public CUINullRect AbsoluteMin;
    public CUINullRect AbsoluteMax;
    public CUINullRect RelativeMin;
    public CUINullRect RelativeMax;

    public CUIRect Real;


    public bool MouseOver { get; set; }
    public bool MousePressed { get; set; }
    public bool PassMouseClicks { get; set; } = true;
    public bool PassDragAndDrop { get; set; } = true;
    public bool PassMouseScroll { get; set; } = true;

    // Without wrappers they will throw FieldAccessException
    public event Action<CUIMouse> OnMouseLeave; public void InvokeOnMouseLeave(CUIMouse m) => OnMouseLeave?.Invoke(m);
    public event Action<CUIMouse> OnMouseEnter; public void InvokeOnMouseEnter(CUIMouse m) => OnMouseEnter?.Invoke(m);
    public event Action<CUIMouse> OnMouseDown; public void InvokeOnMouseDown(CUIMouse m) => OnMouseDown?.Invoke(m);
    public event Action<CUIMouse> OnMouseUp; public void InvokeOnMouseUp(CUIMouse m) => OnMouseUp?.Invoke(m);
    public event Action<CUIMouse> OnDClick; public void InvokeOnDClick(CUIMouse m) => OnDClick?.Invoke(m);
    public event Action<float> OnScroll; public void InvokeOnScroll(float scroll) => OnScroll?.Invoke(scroll);
    public event Action<float, float> OnDrag; public void InvokeOnDrag(float x, float y) => OnDrag?.Invoke(x, y);

    public bool Dragable { get; set; }

    public CUIRect DragZone => Parent.Real;

    public void TryDragTo(Vector2 to)
    {
      if (Parent == null) return;

      float newX = to.X;
      float newY = to.Y;

      if (newX + Real.Width > DragZone.Width) newX = DragZone.Width - Real.Width;
      if (newY + Real.Height > DragZone.Height) newY = DragZone.Height - Real.Height;

      if (newX < 0) newX = 0;
      if (newY < 0) newY = 0;

      Absolute.Left = newX;
      Absolute.Top = newY;

      InvokeOnDrag(newX, newY);
    }

    public bool Resizible { get; set; }
    public CUIRect ResizeHandle { get; set; }

    public void TryToResize(Vector2 newSize)
    {
      Absolute.Width = Math.Max(ResizeHandle.Width + 2, newSize.X);
      Absolute.Height = Math.Max(ResizeHandle.Height + 2, newSize.Y);
    }

    public Color BackgroundColor = Color.Black * 0.5f;
    public Color BorderColor = Color.White * 0.5f;
    public float BorderThickness = 1f;
    public Vector2 padding = new Vector2(2, 2); public Vector2 Padding
    {
      get => padding;
      set { padding = value; OnPropChanged(); }
    }


    public bool Visible { get; set; } = true;
    public bool HideChildrenOutsideFrame { get; set; } = false;
    public bool DrawOnTop { get; set; }
    public virtual void Draw(SpriteBatch spriteBatch)
    {
      //if (!Visible) return;

      GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, BackgroundColor, isFilled: true);
      GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, BorderColor);

      if (Resizible)
      {
        GUI.DrawRectangle(spriteBatch, ResizeHandle.Position, ResizeHandle.Size, BorderColor, isFilled: true);
      }
    }

    public virtual void DrawFront(SpriteBatch spriteBatch) { }

    public void DrawFrontRecursive(SpriteBatch spriteBatch)
    {
      if (Visible) DrawFront(spriteBatch);
      Children.ForEach(c => c.DrawFrontRecursive(spriteBatch));
    }


    public void DrawRecursive(SpriteBatch spriteBatch)
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

    public virtual void Update() { }

    public CUIComponent()
    {
      ID = MaxID++;
      ComponentsById[ID] = this;

      Layout = new CUILayoutSimple(this);

      Absolute = new CUINullRect(this);
      AbsoluteMax = new CUINullRect(this);
      AbsoluteMin = new CUINullRect(this);

      Relative = new CUINullRect(this);
      RelativeMax = new CUINullRect(this);
      RelativeMin = new CUINullRect(this);

      // OnMouseUp += (CUIMouse m) => Mod.log($"OnMouseUp {this}");
      // OnMouseDown += (CUIMouse m) => Mod.log($"OnMouseDown {this}");
      // OnMouseEnter += (CUIMouse m) => Mod.log($"OnMouseEnter {this}");
      // OnMouseLeave += (CUIMouse m) => Mod.log($"OnMouseLeave {this}");
    }

    public CUIComponent(float x, float y, float w, float h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }

    public static void RunRecursiveOn(CUIComponent component, Action<CUIComponent> action, int depth = 0)
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