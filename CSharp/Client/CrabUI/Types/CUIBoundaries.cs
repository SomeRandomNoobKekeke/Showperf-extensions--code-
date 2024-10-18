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
  //TODO should be 2 different structs i think
  public struct CUIBoundaries
  {
    public float? MinX;
    public float? MaxX;
    public float? MinY;
    public float? MaxY;

    //TODO minZ is hardcoded in CUI3DOffset, untangle this crap
    // unusable for now
    public float? MinZ;
    public float? MaxZ;

    public CUIRect Check(float x, float y, float w, float h)
    {
      CUIRect r = new CUIRect(x, y, w, h);

      if (MaxX.HasValue && x + w > MaxX.Value) x = MaxX.Value - w;
      if (MaxY.HasValue && y + h > MaxY.Value) y = MaxY.Value - h;
      if (MinX.HasValue && x < MinX.Value) x = MinX.Value;
      if (MinY.HasValue && y < MinY.Value) y = MinY.Value;

      CUIRect r2 = new CUIRect(x, y, w, h);

      return r2;
    }


    public CUI3DOffset Check(CUI3DOffset offset) => Check(offset.X, offset.Y, offset.Z);
    public CUI3DOffset Check(float x, float y, float z)
    {
      if (MaxX.HasValue && x > MaxX.Value) x = MaxX.Value;
      if (MaxY.HasValue && y > MaxY.Value) y = MaxY.Value;
      if (MaxZ.HasValue && z > MaxZ.Value) z = MaxZ.Value;

      if (MinX.HasValue && x < MinX.Value) x = MinX.Value;
      if (MinY.HasValue && y < MinY.Value) y = MinY.Value;
      if (MinZ.HasValue && z < MinZ.Value) z = MinZ.Value;

      return new CUI3DOffset(x, y, z);
    }


    public CUIBoundaries(
      float? minX = null, float? maxX = null,
      float? minY = null, float? maxY = null,
      float? minZ = null, float? maxZ = null
    )
    {
      MinX = minX;
      MaxX = maxX;
      MinY = minY;
      MaxY = maxY;
      MinZ = minZ;
      MaxZ = maxZ;
    }

    public override string ToString() => $"[{MinX},{MaxX},{MinY},{MaxY},{MinZ},{MaxZ}]";
  }
}