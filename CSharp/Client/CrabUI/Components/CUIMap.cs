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
        LineColor = lineColor ?? new Color(128, 128, 128);
        LineWidth = lineWidth;
        Start = start;
        End = end;
      }
    }

    private List<CUIMapLink> Connections = new List<CUIMapLink>();

    internal override CUINullRect ChildrenBoundaries => new CUINullRect(null, null, null, null);

    public void Connect(CUIComponent start, CUIComponent end, Color? color = null)
    {
      //TODO too sneaky
      if (color == null && (!start.Disabled || !end.Disabled)) color = new Color(0, 0, 255);
      Connections.Add(new CUIMapLink(start, end, color));
    }

    public override void Draw(SpriteBatch spriteBatch)
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
      UnCullable = true;
    }
  }

  public class CUIMap : CUIComponent
  {
    public static CUIMap Default = new CUIMap();
    public CUIMapContent Map;
    public CUIComponent Add(CUIComponent c) => Map.Append(c);
    public CUIComponent Add(string name, CUIComponent c)
    {
      if (name != null) Remember(c, name);
      return Map.Append(c);
    }


    public CUIComponent Connect(CUIComponent startComponent, CUIComponent endComponent, Color? color = null)
    {
      if (startComponent != null && endComponent != null)
      {
        Map.Connect(startComponent, endComponent, color);
      }
      return startComponent;
    }
    public CUIComponent Connect(CUIComponent startComponent, int end = -2, Color? color = null)
    {
      end = MathUtils.PositiveModulo(end, Map.Children.Count);
      CUIComponent endComponent = Map.Children.ElementAtOrDefault(end);
      return Connect(startComponent, endComponent, color);
    }
    public CUIComponent Connect(int start, int end, Color? color = null)
    {
      start = MathUtils.PositiveModulo(start, Map.Children.Count);
      end = MathUtils.PositiveModulo(end, Map.Children.Count);

      CUIComponent startComponent = Map.Children.ElementAtOrDefault(start);
      CUIComponent endComponent = Map.Children.ElementAtOrDefault(end);
      return Connect(startComponent, endComponent, color);
    }

    public CUIComponent ConnectTo(CUIComponent Host, params CUIComponent[] children)
    {
      foreach (CUIComponent child in children) { Connect(Host, child); }
      return Host;
    }

    public CUIMap() : base()
    {
      Swipeable = true;
      ConsumeMouseClicks = true;
      HideChildrenOutsideFrame = true;

      this.Append(Map = new CUIMapContent());
    }
  }

}