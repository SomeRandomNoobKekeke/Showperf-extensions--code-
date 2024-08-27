using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ShowPerfExtensions
{
  public partial class Mod : IAssemblyPlugin
  {
    // should be based on GUIComponent probably
    public partial class GUIList
    {
      public List<UpdateTicks> Values;

      public double Sum;
      public double Linearity;
      public double Slope;
      public double TopValue;

      // Not used for now.There could potentially be multiple lists displaying different things
      public CaptureCategory DisplayedCategory;

      public int showedItemsCount;
      public int ShowedItemsCount
      {
        get => showedItemsCount;
        set
        {
          showedItemsCount = value;
          ShowedRange = new Range(listOffset, listOffset + showedItemsCount);
        }
      }
      public int listOffset;
      public int ListOffset
      {
        get => listOffset;
        set
        {
          listOffset = value;
          ShowedRange = new Range(listOffset, listOffset + showedItemsCount);
        }
      }

      public Range ShowedRange = new Range();
      public double listShift;
      public double ListShift
      {
        get => listShift;
        set
        {
          listShift = value;
          listShift = Math.Min(Math.Max(0, value), Values.Count - showedItemsCount);
          ListOffset = (int)Math.Round(listShift);
        }
      }


      //--------------------------------------------------------

      public Vector2 DrawPosition = new Vector2(830, 50);
      public Vector2 HeaderDrawPosition;
      public float borderWidth = 1;
      public Vector2 Size = new Vector2(520, 600);
      public Vector2 padding = new Vector2(0, 2);
      public Vector2 FullSize;
      public Vector2 FullSizeWithBorders;

      public string caption = "";
      public string Caption
      {
        get => $"{caption} (in {View.UnitsName}/sec):";
        set => caption = value;
      }

      //--------------------------------------------------------
      public Vector2 textScale = new Vector2(0.8f, 0.8f);
      public Vector2 symbolSize;
      public Color backgroundColor = Color.Black * 0.8f;
      public Color BorderColor = Color.White * 0.5f;
      public float stringHeight = GUI.AdjustForTextScale(12);

      public GUIList(string caption = "")
      {
        Values = new List<UpdateTicks>();
        Caption = caption;

        symbolSize = GUIStyle.MonospacedFont.MeasureString("0") * textScale.Y * MonospacedFontRealSize;

        ShowedItemsCount = (int)Math.Floor(Size.Y / symbolSize.Y);

        HeaderDrawPosition = DrawPosition - new Vector2(0, symbolSize.Y * 3 + borderWidth + padding.Y);
        FullSize = Size + new Vector2(0, symbolSize.Y * 3 + borderWidth + padding.Y * 2);
        FullSizeWithBorders = FullSize + new Vector2(borderWidth * 2, borderWidth * 2);
      }

      public Color GetElementColor(UpdateTicks t)
      {
        Color cl = ShowperfGradient(t.Ticks / TopValue);

        return View.Tracked.Count != 0 && !View.Tracked.Contains(t.ID) ? Color.DarkSlateGray : cl;
      }

      public void Clear()
      {
        Values.Clear();
        Sum = 0;
        Linearity = 0;
        TopValue = 0;
        Slope = 0;
      }
    }
  }
}