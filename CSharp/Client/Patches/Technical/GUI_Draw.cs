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
    public static void GUI_Draw_Postfix(Camera cam, SpriteBatch spriteBatch)
    {
      if (!ShowperfCategory.None.IsActive)
      {
        mod.Window.Update();
        //mod.View.Update();
        mod.CUI.Step(spriteBatch);
      }
    }
  }
}

