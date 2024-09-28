using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public class CUIMapContent : CUIComponent
  {
    private class CUIMapLink
    {
      public CUIComponent Start;
      public CUIComponent End;
      public float LineWidth;
      public Color LineColor;

      public CUIMapLink(CUIComponent start, CUIComponent end, Color? lineColor = null, float lineWidth = 2f)
      {
        LineColor = lineColor ?? Color.White;
        LineWidth = lineWidth;
        Start = start;
        End = end;
      }
    }

    private List<CUIMapLink> Connections = new List<CUIMapLink>();

    internal override CUINullRect ChildrenBoundaries => new CUINullRect(null, null, null, null);

    public void Connect(CUIComponent start, CUIComponent end, Color? color = null)
    {
      Connections.Add(new CUIMapLink(start, end, color));
    }

    protected override void Draw(SpriteBatch spriteBatch)
    {
      base.Draw(spriteBatch);

      foreach (CUIMapLink link in Connections)
      {
        GUI.DrawLine(spriteBatch, link.Start.Real.Center, link.End.Real.Center, link.LineColor, width: link.LineWidth);
      }
    }

    public CUIMapContent() : base()
    {
      BackgroundColor = Color.Transparent;
      BorderColor = Color.Transparent;
    }

    public CUIMapContent(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }

  public class CUIMap : CUIComponent
  {
    public CUIMapContent Map;

    public CUIComponent Add(CUIComponent c) => Map.Append(c);
    public void Connect(CUIComponent start, CUIComponent end, Color? color = null)
    {
      Map.Connect(start, end, color);
    }

    public CUIMap() : base()
    {
      Swipeable = true;
      ConsumeMouseClicks = true;
      OnDClick += (m) => ChildrenOffset = Vector2.Zero;
      BorderColor = Color.Transparent;
      BackgroundColor = CUIColors.ComponentBackground;

      this.Append(Map = new CUIMapContent());

    }

    public CUIMap(float? x, float? y, float? w, float? h) : this()
    {
      Relative = new CUINullRect(x, y, w, h);
    }
  }

}