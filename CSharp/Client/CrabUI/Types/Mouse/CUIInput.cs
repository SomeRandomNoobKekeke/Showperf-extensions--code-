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
  public class CUIInput
  {
    public static double DoubleClickInterval = 0.2;
    public static float ScrollSpeed = 0.6f;

    private double PrevMouseDownTiming;
    private int PrevScrollWheelValue;

    public MouseState CurrentMouseState;
    private MouseState PrevMouseState;
    private Vector2 PrevMousePosition;

    public bool MouseDown;
    public bool DoubleClick;
    public bool MouseUp;
    public bool MouseHeld;
    public float Scroll;
    public bool Scrolled;
    public Vector2 MousePosition;
    public Vector2 MousePositionDif;
    public bool MouseMoved;

    public bool SomethingHappened;

    //HACK rethink, this is too hacky
    public bool ClickConsumed;

    public void Scan(double totalTime)
    {
      ClickConsumed = false;

      CurrentMouseState = Mouse.GetState();

      MouseDown = PrevMouseState.LeftButton == ButtonState.Released && CurrentMouseState.LeftButton == ButtonState.Pressed;
      MouseUp = PrevMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.LeftButton == ButtonState.Released;
      MouseHeld = CurrentMouseState.LeftButton == ButtonState.Pressed;

      PrevMousePosition = MousePosition;
      MousePosition = new Vector2(CurrentMouseState.Position.X, CurrentMouseState.Position.Y);
      MousePositionDif = MousePosition - PrevMousePosition;
      MouseMoved = MousePositionDif != Vector2.Zero;

      Scroll = (CurrentMouseState.ScrollWheelValue - PrevScrollWheelValue) * ScrollSpeed;
      PrevScrollWheelValue = CurrentMouseState.ScrollWheelValue;
      Scrolled = Scroll != 0;

      DoubleClick = false;

      if (MouseDown)
      {
        if (totalTime - PrevMouseDownTiming < DoubleClickInterval)
        {
          DoubleClick = true;
        }

        PrevMouseDownTiming = totalTime;
      }

      SomethingHappened = MouseUp || MouseDown || MouseMoved || Scrolled;

      PrevMouseState = CurrentMouseState;
    }
  }
}