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

using System.Xml;
using System.Xml.Linq;

namespace CrabUI
{
  public partial class CUIComponent
  {
    #region Events --------------------------------------------------------

    public bool MouseOver { get; set; }
    public bool MousePressed { get; set; }
    [CUISerializable] public bool ConsumeMouseClicks { get; set; }
    [CUISerializable] public bool ConsumeDragAndDrop { get; set; }
    [CUISerializable] public bool ConsumeSwipe { get; set; }
    [CUISerializable] public bool ConsumeMouseScroll { get; set; }


    // Without wrappers they will throw FieldAccessException
    public event Action OnUpdate; internal void InvokeOnUpdate() => OnUpdate?.Invoke();
    public Action AddOnUpdate { set { OnUpdate += value; } }
    public event Action<CUIInput> OnMouseLeave; internal void InvokeOnMouseLeave(CUIInput e) => OnMouseLeave?.Invoke(e);
    public Action<CUIInput> AddOnMouseLeave { set { OnMouseLeave += value; } }
    public event Action<CUIInput> OnMouseEnter; internal void InvokeOnMouseEnter(CUIInput e) => OnMouseEnter?.Invoke(e);
    public Action<CUIInput> AddOnMouseEnter { set { OnMouseEnter += value; } }
    public event Action<CUIInput> OnMouseDown; internal void InvokeOnMouseDown(CUIInput e) => OnMouseDown?.Invoke(e);
    public Action<CUIInput> AddOnMouseDown { set { OnMouseDown += value; } }
    public event Action<CUIInput> OnMouseUp; internal void InvokeOnMouseUp(CUIInput e) => OnMouseUp?.Invoke(e);
    public Action<CUIInput> AddOnMouseUp { set { OnMouseUp += value; } }
    public event Action<CUIInput> OnDClick; internal void InvokeOnDClick(CUIInput e) => OnDClick?.Invoke(e);
    public Action<CUIInput> AddOnDClick { set { OnDClick += value; } }
    public event Action<CUIInput> OnScroll; internal void InvokeOnScroll(CUIInput e) => OnScroll?.Invoke(e);
    public Action<CUIInput> AddOnScroll { set { OnScroll += value; } }
    public event Action<float, float> OnDrag; internal void InvokeOnDrag(float x, float y) => OnDrag?.Invoke(x, y);
    public Action<float, float> AddOnDrag { set { OnDrag += value; } }
    public event Action<float, float> OnSwipe; internal void InvokeOnSwipe(float x, float y) => OnSwipe?.Invoke(x, y);
    public Action<float, float> AddOnSwipe { set { OnSwipe += value; } }

    public void Click() { OnMouseDown?.Invoke(CUI.Input); }

    #endregion
    #region Handles --------------------------------------------------------

    public CUIDragHandle DragHandle;
    [CUISerializable]
    public bool Draggable
    {
      get => DragHandle.Draggable;
      set => DragHandle.Draggable = value;
    }
    public CUIResizeHandle LeftResizeHandle;
    public CUIResizeHandle RightResizeHandle;
    [CUISerializable]
    public bool Resizible
    {
      get => LeftResizeHandle.Visible || RightResizeHandle.Visible;
      set { LeftResizeHandle.Visible = value; RightResizeHandle.Visible = value; }
    }

    public CUISwipeHandle SwipeHandle;
    [CUISerializable]
    public bool Swipeable
    {
      get => SwipeHandle.Swipeable;
      set => SwipeHandle.Swipeable = value;
    }

    #endregion
  }
}