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
  public class CUIResizeHandle
  {
    public CUIComponent Host;
    public CUIRect Real;
    public bool Grabbed;
    public bool Active;
    public void Update()
    {
      Real = new CUIRect(Real.Right - 9, Real.Bottom - 9, 9, 9);
    }
    public void Draw(SpriteBatch spriteBatch)
    {
      GUI.DrawRectangle(spriteBatch, Real.Position, Real.Size, Host.BorderColor, isFilled: true);
    }

    public CUIResizeHandle(CUIComponent host)
    {
      Host = host;
    }
  }
}