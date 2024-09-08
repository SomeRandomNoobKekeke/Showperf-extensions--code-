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

    // those should be events but for some reason events cause FieldAccessException 
    public Action OnMouseLeave;
    public Action OnMouseEnter;
    public Action OnMouseDown;
    public Action OnMouseClick;

    public bool MouseOver;
    public bool MousePressed;




    // public void PassMouseEvent(bool up = true)
    // {
    //   if (BorderBox.Contains(PlayerInput.MousePosition))
    //   {
    //     // trigger MouseOver(true)

    //     Children.ForEach(c => PassMouseEvent(true));
    //   }
    // }
  }
}