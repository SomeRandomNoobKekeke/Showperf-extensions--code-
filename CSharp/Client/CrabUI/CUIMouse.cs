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
  // PlayerInput is updated at 60 fps and desync with GUI.Draw
  // mb i should also limit updates to 60 fps
  public class CUIMouse
  {
    public static double DoubleClickInterval = 0.18;
    public static float ScrollSpeed = 5f;

    private double PrevMouseDownTiming;
    private int PrevScrollWheelValue;

    public MouseState CurrentState;
    private MouseState PrevState;
    private Vector2 PrevPosition;

    public bool Down;
    public bool DoubleClick;
    public bool Up;
    public bool Held;
    public float Scroll;
    public Vector2 Position;
    public Vector2 PositionDif;
    public bool Moved;

    public void Scan()
    {
      CurrentState = Mouse.GetState();

      Down = PrevState.LeftButton == ButtonState.Released && CurrentState.LeftButton == ButtonState.Pressed;
      Up = PrevState.LeftButton == ButtonState.Pressed && CurrentState.LeftButton == ButtonState.Released;
      Held = CurrentState.LeftButton == ButtonState.Pressed;

      PrevPosition = Position;
      Position = new Vector2(CurrentState.Position.X, CurrentState.Position.Y);
      PositionDif = Position - PrevPosition;
      Moved = PositionDif != Vector2.Zero;

      Scroll = (CurrentState.ScrollWheelValue - PrevScrollWheelValue) / ScrollSpeed;
      PrevScrollWheelValue = CurrentState.ScrollWheelValue;

      DoubleClick = false;
      //FIXME remove Timing.TotalTime, it may not be updated
      if (Down)
      {
        if (Timing.TotalTime - PrevMouseDownTiming < DoubleClickInterval)
        {
          DoubleClick = true;
        }

        PrevMouseDownTiming = Timing.TotalTime;
      }

      PrevState = CurrentState;
    }
  }
}