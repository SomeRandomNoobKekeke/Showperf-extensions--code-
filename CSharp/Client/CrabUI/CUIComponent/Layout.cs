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
    public Vector2 Size { get; set; }
    public Vector2 position; public Vector2 Position
    {
      get => position;
      set { position = value; RealPosition = (Parent?.RealPosition ?? Vector2.Zero) + value; }
    }

    public Vector2 RealPosition { get; set; }
    public Rectangle BorderBox => new Rectangle(
      (int)RealPosition.X, (int)RealPosition.Y,
      (int)Size.X, (int)Size.Y
    );

    public float Left
    {
      get => Position.X;
      set { Position = new Vector2(value, Position.Y); }
    }

    public float Top
    {
      get => Position.Y;
      set { Position = new Vector2(Position.X, value); }
    }

    public float Width
    {
      get => Size.X;
      set { Size = new Vector2(value, Size.Y); }
    }
    public float Height
    {
      get => Size.Y;
      set { Size = new Vector2(Size.X, value); }
    }


    public float? relativeLeft; public float? RelativeLeft
    {
      get => relativeLeft;
      set { relativeLeft = value; NeedsLayoutUpdate = true; }
    }

    public float? relativeTop; public float? RelativeTop
    {
      get => relativeTop;
      set { relativeTop = value; NeedsLayoutUpdate = true; }
    }

    public float? relativeWidth; public float? RelativeWidth
    {
      get => relativeWidth;
      set { relativeWidth = value; NeedsLayoutUpdate = true; }
    }

    public float? relativeHeight; public float? RelativeHeight
    {
      get => relativeHeight;
      set { relativeHeight = value; NeedsLayoutUpdate = true; }
    }


    public Vector2 RelativeSize
    {
      get => new Vector2(RelativeWidth ?? 0, RelativeHeight ?? 0);
      set { RelativeWidth = value.X; RelativeHeight = value.Y; NeedsLayoutUpdate = true; }
    }

    public Vector2 RelativePosition
    {
      get => new Vector2(RelativeLeft ?? 0, RelativeTop ?? 0);
      set { RelativeLeft = value.X; RelativeTop = value.Y; NeedsLayoutUpdate = true; }
    }

    public virtual void ApplyRelSize(bool applyW = true, bool applyH = true)
    {
      float w = Size.X;
      float h = Size.Y;

      if (RelativeWidth.HasValue && applyW) w = (Parent?.Size.X ?? GameScreenSize.X) * RelativeWidth.Value;
      if (RelativeHeight.HasValue && applyH) h = (Parent?.Size.Y ?? GameScreenSize.Y) * RelativeHeight.Value;

      Size = new Vector2(w, h);
    }

    public virtual void ApplyRelPosition(bool applyX = true, bool applyY = true)
    {
      float x = Position.X;
      float y = Position.Y;

      if (RelativeLeft.HasValue && applyX) x = (Parent?.Size.X ?? GameScreenSize.X) * RelativeLeft.Value;
      if (RelativeTop.HasValue & applyY) y = (Parent?.Size.Y ?? GameScreenSize.Y) * RelativeTop.Value;

      Position = new Vector2(x, y);
    }


    public bool needsLayoutUpdate = true; public bool NeedsLayoutUpdate
    {
      get => needsLayoutUpdate;
      set
      {
        needsLayoutUpdate = value;
        if (value && Parent != null) Parent.NeedsLayoutUpdate = true;
      }
    }

    public bool NeedsContentSizeUpdate { get; set; } = false;
    public bool NeedsRelSizeUpdate { get; set; } = true;


    public virtual void ApplyParentSizeRestrictions(CUIComponent c) { }
    public virtual Vector2 CalculateContentSize() { NeedsContentSizeUpdate = false; return Vector2.Zero; }
    public virtual void UpdateLayout() { Children.ForEach(c => c.ApplyRelPosition()); }


    public string LayoutToString()
    {
      return $"{ID}:{Name}| {Position} {Size} | {RelativePosition} {RelativeSize}";
    }
  }
}