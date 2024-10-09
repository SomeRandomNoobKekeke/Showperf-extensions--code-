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
  public partial class CUIComponent
  {
    #region Static --------------------------------------------------------

    public static int MaxID = 0;
    public static Dictionary<int, CUIComponent> ComponentsById = new Dictionary<int, CUIComponent>();
    public static Vector2 GameScreenSize => new Vector2(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
    public static Rectangle GameScreenRect => new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
    public static GUIButton dummyComponent = new GUIButton(new RectTransform(new Point(0, 0)));
    public static void RunRecursiveOn(CUIComponent component, Action<CUIComponent, int> action, int depth = 0)
    {
      action(component, depth);
      foreach (CUIComponent child in component.Children)
      {
        RunRecursiveOn(child, action, depth + 1);
      }
    }

    #endregion
    #region Virtual --------------------------------------------------------

    internal virtual CUINullRect ChildrenBoundaries => new CUINullRect(null, null, null, null);
    internal virtual CUINullRect ChildOffsetBounds => new CUINullRect(null, null, null, null);
    internal virtual void UpdatePseudoChildren()
    {
      LeftResizeHandle.Update();
      RightResizeHandle.Update();
    }
    internal virtual Vector2 AmIOkWithThisSize(Vector2 size) => size;
    internal virtual void ChildrenSizeCalculated() { }
    public virtual partial void ApplyState(CUIComponent state);
    protected virtual partial void Draw(SpriteBatch spriteBatch);
    protected virtual partial void DrawFront(SpriteBatch spriteBatch);

    #endregion
    #region Meta --------------------------------------------------------

    public int ID;
    public bool DebugHighlight;
    internal bool CulledOut;
    //HACK need a more robust solution
    protected bool ComponentInitialized;

    public override string ToString() => $"{this.GetType().Name}:{ID}:{AKA}";

    protected CUIRect BorderBox;
    protected Rectangle? ScissorRect;
    private CUIRect real; public CUIRect Real
    {
      get => real;
      set => SetReal(value);
    }

    internal void SetReal(CUIRect value, [CallerMemberName] string memberName = "")
    {
      real = value;
      CUIDebug.Capture(null, this, "SetReal", memberName, "real", real.ToString());

      BorderBox = new CUIRect(
        real.Left - BorderThickness,
        real.Top - BorderThickness,
        real.Width + BorderThickness * 2,
        real.Height + BorderThickness * 2
      );

      if (HideChildrenOutsideFrame)
      {
        //HACK Remove these + 1
        Rectangle SRect = new Rectangle(
          (int)real.Left + 1,
          (int)real.Top + 1,
          (int)real.Width - 2,
          (int)real.Height - 2
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

    #endregion
    #region Debug --------------------------------------------------------

    public bool debug; public bool Debug
    {
      get => debug;
      set
      {
        debug = value;
        foreach (CUIComponent c in Children) { c.Debug = value; }
      }
    }

    private bool ignoreDebug; public bool IgnoreDebug
    {
      get => ignoreDebug;
      set
      {
        ignoreDebug = value;
        foreach (CUIComponent c in Children) { c.IgnoreDebug = value; }
      }
    }

    public void Info(object msg, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      var fi = new FileInfo(source);

      CUI.log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Yellow * 0.5f);
      CUI.log($"{this} {msg ?? "null"}", Color.Yellow);
    }

    public void PrintLayout() => Info($"{Real} {Anchor.Type} Z:{ZIndex} A:{Absolute} R:{Relative} AMin:{AbsoluteMin} RMin:{RelativeMin} AMax:{AbsoluteMax} RMax:{RelativeMax}");

    #endregion
    #region Tree --------------------------------------------------------

    public List<CUIComponent> Children = new List<CUIComponent>();

    private CUIComponent? parent; public CUIComponent? Parent
    {
      get => parent;
      set { parent = value; TreeChanged = true; OnPropChanged(); }
    }
    //TODO do i need OnAbsolutePropChanged(); here?

    private bool treeChanged = true; public bool TreeChanged
    {
      get => treeChanged;
      set { treeChanged = value; if (value && Parent != null) Parent.TreeChanged = true; }
    }

    public CUIComponent Append(CUIComponent c, string name = null)
    {
      if (c == null) return c;

      c.Parent = this;
      PassPropsToChild(c);

      if (name != null)
      {
        NamedComponents[name] = c;
        c.AKA = name;
      }
      Children.Add(c);

      return c;
    }

    public void RemoveChild(CUIComponent c)
    {
      if (c == null || !Children.Contains(c)) return;

      if (c.AKA != null && NamedComponents.ContainsKey(c.AKA))
      {
        NamedComponents.Remove(c.AKA);
        //c.AKA = null;
      }
      c.Parent = null;
      TreeChanged = true;
      Children.Remove(c);
    }

    public void RemoveAllChildren()
    {
      foreach (CUIComponent c in Children) { c.Parent = null; }
      NamedComponents.Clear();
      Children.Clear();
      TreeChanged = true;
    }

    public bool ShouldPassPropsToChildren = true;
    private void PassPropsToChild(CUIComponent child)
    {
      if (!ShouldPassPropsToChildren) return;

      if (ZIndex.HasValue) child.ZIndex = ZIndex.Value + 1;
      if (IgnoreEvents) child.IgnoreEvents = true;
      if (!Visible) child.Visible = false;
    }

    #endregion
    #region Layout --------------------------------------------------------

    private CUILayout layout; public CUILayout Layout
    {
      get => layout;
      set { layout = value; layout.Host = this; }
    }

    private Vector2 childrenOffset; public Vector2 ChildrenOffset
    {
      get => childrenOffset;
      set => SetChildrenOffset(value);
    }
    internal void SetChildrenOffset(Vector2 value, [CallerMemberName] string memberName = "")
    {
      childrenOffset = value;
      // if (ComponentInitialized)
      // {
      //   CUIDebug.Capture(null, this, "SetChildrenOffset", memberName, "childrenOffset", childrenOffset.ToString());
      // }
      OnChildrenPropChanged();
    }

    internal void OnPropChanged([CallerMemberName] string memberName = "")
    {
      Layout.Changed = true;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "OnPropChanged", memberName, "Layout.Changed", "true");
      }
    }
    internal void OnDecorPropChanged([CallerMemberName] string memberName = "")
    {
      Layout.DecorChanged = true;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "OnDecorPropChanged", memberName, "Layout.DecorChanged", "true");
      }
    }
    internal void OnAbsolutePropChanged([CallerMemberName] string memberName = "")
    {
      Layout.AbsoluteChanged = true;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "OnAbsolutePropChanged", memberName, "Layout.AbsoluteChanged", "true");
      }
    }
    internal void OnChildrenPropChanged([CallerMemberName] string memberName = "")
    {
      foreach (CUIComponent child in Children)
      {
        child.Layout.Changed = true;
      }
    }

    #endregion
    #region Events --------------------------------------------------------

    public bool MouseOver { get; set; }
    public bool MousePressed { get; set; }
    public bool ConsumeMouseClicks { get; set; }
    public bool ConsumeDragAndDrop { get; set; }
    public bool ConsumeSwipe { get; set; }
    public bool ConsumeMouseScroll { get; set; }


    // Without wrappers they will throw FieldAccessException
    public event Action OnUpdate; internal void InvokeOnUpdate() => OnUpdate?.Invoke();
    public Action AddOnUpdate { set { OnUpdate += value; } }
    public event Action<CUIMouse> OnMouseLeave; internal void InvokeOnMouseLeave(CUIMouse m) => OnMouseLeave?.Invoke(m);
    public Action<CUIMouse> AddOnMouseLeave { set { OnMouseLeave += value; } }
    public event Action<CUIMouse> OnMouseEnter; internal void InvokeOnMouseEnter(CUIMouse m) => OnMouseEnter?.Invoke(m);
    public Action<CUIMouse> AddOnMouseEnter { set { OnMouseEnter += value; } }
    public event Action<CUIMouse> OnMouseDown; internal void InvokeOnMouseDown(CUIMouse m) => OnMouseDown?.Invoke(m);
    public Action<CUIMouse> AddOnMouseDown { set { OnMouseDown += value; } }
    public event Action<CUIMouse> OnMouseUp; internal void InvokeOnMouseUp(CUIMouse m) => OnMouseUp?.Invoke(m);
    public Action<CUIMouse> AddOnMouseUp { set { OnMouseUp += value; } }
    public event Action<CUIMouse> OnDClick; internal void InvokeOnDClick(CUIMouse m) => OnDClick?.Invoke(m);
    public Action<CUIMouse> AddOnDClick { set { OnDClick += value; } }
    public event Action<float> OnScroll; internal void InvokeOnScroll(float scroll) => OnScroll?.Invoke(scroll);
    public Action<float> AddOnScroll { set { OnScroll += value; } }
    public event Action<float, float> OnDrag; internal void InvokeOnDrag(float x, float y) => OnDrag?.Invoke(x, y);
    public Action<float, float> AddOnDrag { set { OnDrag += value; } }
    public event Action<float, float> OnSwipe; internal void InvokeOnSwipe(float x, float y) => OnSwipe?.Invoke(x, y);
    public Action<float, float> AddOnSwipe { set { OnSwipe += value; } }

    #endregion
    #region Handles --------------------------------------------------------

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
    #region Props --------------------------------------------------------

    //TODO This is potentially cursed
    public object Data;
    public bool UnCullable { get; set; } // >:(
    public bool HideChildrenOutsideFrame { get; set; } = false;
    public CUIAnchor Anchor = new CUIAnchor(CUIAnchorType.LeftTop);

    private int? zIndex; public int? ZIndex
    {
      get => zIndex;
      set
      {
        zIndex = value;
        OnPropChanged();
        foreach (var child in Children)
        {
          //TODO think, should i propagate null?
          if (zIndex.HasValue) child.ZIndex = zIndex.Value + 1;
        }
      }
    }
    private bool ignoreEvents; public bool IgnoreEvents
    {
      get => ignoreEvents;
      set { ignoreEvents = value; foreach (var child in Children) child.IgnoreEvents = value; }
    }

    private bool visible = true; public bool Visible
    {
      get => visible;
      set { visible = value; foreach (var child in Children) child.Visible = value; }
    }

    private CUIBool2 fillEmptySpace; public CUIBool2 FillEmptySpace
    {
      get => fillEmptySpace;
      set { fillEmptySpace = value; OnPropChanged(); }
    }

    private CUIBool2 fitContent; public CUIBool2 FitContent
    {
      get => fitContent;
      set { fitContent = value; OnPropChanged(); OnAbsolutePropChanged(); }
    }

    // Ugly, but otherwise it'll be undebugable
    #region Absolute Props
    private CUINullRect absolute; public CUINullRect Absolute
    {
      get => absolute;
      set => SetAbsolute(value);
    }
    internal void SetAbsolute(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      absolute = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetAbsolute", memberName, "Absolute", Absolute.ToString());
      }
      OnPropChanged(); OnAbsolutePropChanged();
    }

    private CUINullRect absoluteMin; public CUINullRect AbsoluteMin
    {
      get => absoluteMin;
      set => SetAbsoluteMin(value);
    }
    internal void SetAbsoluteMin(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      absoluteMin = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetAbsoluteMin", memberName, "AbsoluteMin", AbsoluteMin.ToString());
      }
      OnPropChanged(); OnAbsolutePropChanged();
    }
    private CUINullRect absoluteMax; public CUINullRect AbsoluteMax
    {
      get => absoluteMax;
      set => SetAbsoluteMax(value);
    }
    internal void SetAbsoluteMax(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      absoluteMax = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetAbsoluteMax", memberName, "AbsoluteMax", AbsoluteMax.ToString());
      }
      OnPropChanged(); OnAbsolutePropChanged();
    }

    #endregion
    #region Relative Props

    //TODO make sure i don't call Relative setters directly
    private CUINullRect relative; public CUINullRect Relative
    {
      get => relative;
      set => SetRelative(value);
    }
    internal void SetRelative(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      relative = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetRelative", memberName, "Relative", Relative.ToString());
      }
      OnPropChanged();
    }

    private CUINullRect relativeMin; public CUINullRect RelativeMin
    {
      get => relativeMin;
      set => SetRelativeMin(value);
    }
    internal void SetRelativeMin(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      relativeMin = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetRelativeMin", memberName, "RelativeMin", RelativeMin.ToString());
      }
      OnPropChanged();
    }

    private CUINullRect relativeMax; public CUINullRect RelativeMax
    {
      get => relativeMax;
      set => SetRelativeMax(value);
    }
    internal void SetRelativeMax(CUINullRect value, [CallerMemberName] string memberName = "")
    {
      relativeMax = value;
      if (ComponentInitialized)
      {
        CUIDebug.Capture(null, this, "SetRelativeMax", memberName, "RelativeMax", RelativeMax.ToString());
      }
      OnPropChanged();
    }

    #endregion
    #endregion
    #region Graphic Props --------------------------------------------------------

    protected bool BackgroundVisible;
    private Color backgroundColor; public Color BackgroundColor
    {
      get => backgroundColor;
      set { backgroundColor = value; BackgroundVisible = backgroundColor != Color.Transparent; }
    }

    protected bool BorderVisible;
    private Color borderColor; public Color BorderColor
    {
      get => borderColor;
      set { borderColor = value; BorderVisible = borderColor != Color.Transparent; }
    }

    public float BorderThickness = 1f;
    private Vector2 padding = new Vector2(2, 2); public Vector2 Padding
    {
      get => padding;
      set { padding = value; OnDecorPropChanged(); }
    }

    #endregion
    #region State --------------------------------------------------------

    public Dictionary<string, CUIComponent> States = new Dictionary<string, CUIComponent>();
    public CUIComponent Clone()
    {
      CUIComponent clone = new CUIComponent();
      clone.ApplyState(this);
      return clone;
    }
    public virtual partial void ApplyState(CUIComponent state)
    {
      if (state == null) return;

      ShouldPassPropsToChildren = state.ShouldPassPropsToChildren;
      zIndex = state.ZIndex; // TODO think how to uncurse this
      IgnoreEvents = state.IgnoreEvents;
      Visible = state.Visible;
      ChildrenOffset = state.ChildrenOffset;
      Draggable = state.Draggable;
      LeftResizeHandle.Visible = state.LeftResizeHandle.Visible;
      RightResizeHandle.Visible = state.RightResizeHandle.Visible;
      Swipeable = state.Swipeable;
      Anchor = state.Anchor;
      Absolute = state.Absolute;
      AbsoluteMax = state.AbsoluteMax;
      AbsoluteMin = state.AbsoluteMin;
      Relative = state.Relative;
      RelativeMax = state.RelativeMax;
      RelativeMin = state.RelativeMin;
      FillEmptySpace = state.FillEmptySpace;
      FitContent = state.FitContent;
      HideChildrenOutsideFrame = state.HideChildrenOutsideFrame;
      BackgroundColor = state.BackgroundColor;
      BorderColor = state.BorderColor;
      BorderThickness = state.BorderThickness;
      Padding = state.Padding;
    }

    #endregion
    #region AKA

    public string AKA;
    public Dictionary<string, CUIComponent> NamedComponents = new Dictionary<string, CUIComponent>();

    public CUIComponent Remember(CUIComponent c, string name) => NamedComponents[name] = c;
    public CUIComponent Remember(CUIComponent c) => NamedComponents[c.AKA ?? ""] = c;

    public CUIComponent this[string name]
    {
      get => NamedComponents.GetValueOrDefault(name);
      set => Append(value, name);
    }

    public T Get<T>(string name) where T : CUIComponent => NamedComponents.GetValueOrDefault(name) as T;

    #endregion
    #region Draw --------------------------------------------------------

    protected virtual partial void Draw(SpriteBatch spriteBatch)
    {
      if (BackgroundVisible) GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, BackgroundColor, isFilled: true);

      if (BorderVisible) GUI.DrawRectangle(spriteBatch, BorderBox.Position, BorderBox.Size, BorderColor, thickness: BorderThickness);

      LeftResizeHandle.Draw(spriteBatch);
      RightResizeHandle.Draw(spriteBatch);
    }

    protected virtual partial void DrawFront(SpriteBatch spriteBatch)
    {
      if (DebugHighlight)
      {
        GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, Color.Cyan * 0.5f, isFilled: true);
      }
    }


    #endregion
    #region Constructors --------------------------------------------------------
    public CUIComponent()
    {
      ID = MaxID++;
      ComponentsById[ID] = this;

      BackgroundColor = CUIPallete.Default.Primary.Off;
      BorderColor = CUIPallete.Default.Primary.Border;

      Layout = new CUILayoutSimple();

      DragHandle = new CUIDragHandle(this);
      SwipeHandle = new CUISwipeHandle(this);
      LeftResizeHandle = new CUIResizeHandle(this, CUIAnchorType.LeftBottom);
      RightResizeHandle = new CUIResizeHandle(this, CUIAnchorType.RightBottom);

      ComponentInitialized = true;
    }

    public CUIComponent(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
    #endregion
  }
}