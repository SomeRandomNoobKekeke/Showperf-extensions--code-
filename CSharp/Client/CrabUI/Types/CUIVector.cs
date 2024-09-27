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

  public struct CUIBool2
  {
    public bool X;
    public bool Y;

    public CUIBool2(bool x = false, bool y = false)
    {
      X = x;
      Y = y;
    }
  }
}