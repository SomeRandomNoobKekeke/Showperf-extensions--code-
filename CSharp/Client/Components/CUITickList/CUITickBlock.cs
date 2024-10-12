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

        public Color TextColor = Color.White;
        public GUIFont Font = GUIStyle.MonospacedFont;
        public float TextScale = 0.8f;


        public int VisibleRangeStart;
        public int VisibleRangeEnd;


        public void Update()
        {
          VisibleRangeStart = (int)Math.Floor(-TickList.Scroll / StringHeight);
          VisibleRangeStart = Math.Max(0, Math.Min(VisibleRangeStart, TickList.Values.Count));

          VisibleRangeEnd = (int)Math.Ceiling((-TickList.Scroll + TickList.Real.Height) / StringHeight);
          VisibleRangeEnd = Math.Max(0, Math.Min(VisibleRangeEnd, TickList.Values.Count));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
          float y = 0;

          for (int i = VisibleRangeStart; i < VisibleRangeEnd; i++)
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
          UnCullable = true;
        }
      }
    }
  }
}