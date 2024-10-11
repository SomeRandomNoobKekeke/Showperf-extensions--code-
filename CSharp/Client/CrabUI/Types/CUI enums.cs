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
  public enum CUITextAling
  {
    Start,
    Center,
    End,
  }

  public enum CUIDirection
  {
    Straight,
    Reverse,
  }

  public enum CUIMouseEvent
  {
    Down, DClick
  }
}