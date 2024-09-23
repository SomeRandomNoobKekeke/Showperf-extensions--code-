using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using CrabUI;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    public class CUITickList : CUIComponent
    {
      public CUIView View;

      public float StringHeight = 12f;

      public Color TextColor { get; set; } = Color.White;
      public GUIFont Font { get; set; } = GUIStyle.MonospacedFont;
      public float TextScale { get; set; } = 0.8f;

      public int ScrollSurround = 0;

      public void Update()
      {
        Absolute.Height = (View.Values.Count + 2) * StringHeight; // +2 just for gap
      }



      protected override void Draw(SpriteBatch spriteBatch)
      {
        float y = 0;

        int start = (int)Math.Floor(-View.Scroll / StringHeight) - ScrollSurround;
        int end = (int)Math.Ceiling((-View.Scroll + View.Real.Height) / StringHeight) + ScrollSurround;
        start = Math.Max(0, Math.Min(start, View.Values.Count));
        end = Math.Max(0, Math.Min(end, View.Values.Count));

        for (int i = start; i < end; i++)
        {
          Font.DrawString(
              spriteBatch,
              View.GetName(View.Values[i]),
              Real.Position + new Vector2(0, i * StringHeight),
              View.GetColor(View.Values[i]),
              rotation: 0,
              origin: Vector2.Zero,
              TextScale,
              spriteEffects: SpriteEffects.None,
              layerDepth: 0.1f
            );
        }

      }

      public CUITickList(CUIView view)
      {
        View = view;
        BackgroundColor = Color.Transparent;
        BorderColor = Color.Transparent;

        Dragable = true;
        OnDrag += (x, y) => View.Scroll = y;
      }
    }
  }
}