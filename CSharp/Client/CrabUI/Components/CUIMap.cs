using System;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace CrabUI
{
  public partial class CUIMap : CUIComponent
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

    public override CUIComponent Append(CUIComponent c)
    {
      c.Anchor.Type = CUIAnchorType.LeftCenter;
      return base.Append(c);
    }

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

    public CUIMap() : base()
    {
    }

    public CUIMap(float? x, float? y, float? w, float? h) : this()
    {
      Relative.Set(x, y, w, h);
    }
  }
}