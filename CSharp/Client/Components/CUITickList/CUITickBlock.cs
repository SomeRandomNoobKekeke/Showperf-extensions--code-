using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
    public partial class CUITickList : CUIVerticalList
    {
      private class CUITickBlock : CUIComponent
      {
        public CUITickList TickList;

        public float StringHeight = 12f;

        public Color TextColor { get; set; } = Color.White;
        public GUIFont Font { get; set; } = GUIStyle.MonospacedFont;
        public float TextScale { get; set; } = 0.8f;

        public int ScrollSurround = 0;

        public void Update()
        {
          Absolute.Height = (TickList.Values.Count) * StringHeight;
        }

        protected override void Draw(SpriteBatch spriteBatch)
        {
          float y = 0;

          int start = (int)Math.Floor(-TickList.Scroll / StringHeight) - ScrollSurround;
          int end = (int)Math.Ceiling((-TickList.Scroll + TickList.Real.Height) / StringHeight) + ScrollSurround;
          start = Math.Max(0, Math.Min(start, TickList.Values.Count));
          end = Math.Max(0, Math.Min(end, TickList.Values.Count));

          for (int i = start; i < end; i++)
          {
            Font.DrawString(
                spriteBatch,
                TickList.GetName(TickList.Values[i]),
                Real.Position + new Vector2(Padding.X, i * StringHeight),
                TickList.GetColor(TickList.Values[i]),
                rotation: 0,
                origin: Vector2.Zero,
                TextScale,
                spriteEffects: SpriteEffects.None,
                layerDepth: 0.1f
              );
          }

        }

        public CUITickBlock(CUITickList tickList)
        {
          TickList = tickList;
          BackgroundColor = Color.Transparent;
          BorderColor = Color.Transparent;

          Draggable = true;
          OnDrag += (x, y) => TickList.Scroll = y;
        }
      }
    }
  }
}