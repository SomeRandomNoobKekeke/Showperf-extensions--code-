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
  public class CUIProp<T>
  {
    public T Value;

    private Action<T> onSet;
    public Action<T> OnSet { set => onSet += value; }

    public void Set(T value)
    {
      Value = value;
      onSet?.Invoke(value);
    }

    public CUIProp(T value)
    {
      Value = value;
    }

    public override string ToString() => Value.ToString();
  }
}