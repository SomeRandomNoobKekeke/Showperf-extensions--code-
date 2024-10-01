#define CUIDEBUG

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
  public enum CUIDebugEventType
  {
    LayoutUpdate, UpdateDecor, ResizeToContent, CheckChildBoundaries
  }
  public class CUIDebugEvent
  {
    public CUIComponent Component;
    public CUIDebugEventType EventType;
    public string Info;
    public CUIDebugEvent(CUIComponent c, CUIDebugEventType t, string i)
    {
      Component = c;
      EventType = t;
      Info = i;
    }
  }


  public class CUIDebugEventComponent : CUIComponent
  {
    private CUIDebugEvent _value; public CUIDebugEvent Value
    {
      get => _value;
      set
      {
        _value = value;
        if (value == null)
        {
          Visible = false; IgnoreEvents = true;
          Text = $"";
        }
        else
        {
          Visible = true; IgnoreEvents = false;
          Text = $"{value.EventType} {value.Component} {value.Info}";
        }
      }
    }

    public void Flush() => Value = null;


    public string Text = "";


    protected override void Draw(SpriteBatch spriteBatch)
    {
      base.Draw(spriteBatch);

      GUIStyle.Font.Value.DrawString(spriteBatch, Text, Real.Position, Color.White, rotation: 0, origin: Vector2.Zero, 0.9f, se: SpriteEffects.None, layerDepth: 0.1f);
    }


    public CUIDebugEventComponent(CUIDebugEvent value = null) : base()
    {
      Value = value;
      IgnoreDebug = true;
      BackgroundColor = Color.Green;
      Absolute = new CUINullRect(null, null, null, 30);
    }
  }
}