using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using System.Xml;
using System.Xml.Linq;

namespace CrabUI
{
  public partial class CUIComponent
  {
    #region Debug --------------------------------------------------------

    public bool debug; public bool Debug
    {
      get => debug;
      set
      {
        debug = value;
        foreach (CUIComponent c in Children) { c.Debug = value; }
      }
    }

    private bool ignoreDebug; public bool IgnoreDebug
    {
      get => ignoreDebug;
      set
      {
        ignoreDebug = value;
        foreach (CUIComponent c in Children) { c.IgnoreDebug = value; }
      }
    }

    public void Info(object msg, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      var fi = new FileInfo(source);

      CUI.log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Yellow * 0.5f);
      CUI.log($"{this} {msg ?? "null"}", Color.Yellow);
    }

    public void PrintLayout([CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      Info(
        $"{Real} {Anchor} Z:{ZIndex} A:{Absolute} R:{Relative} AMin:{AbsoluteMin} RMin:{RelativeMin} AMax:{AbsoluteMax} RMax:{RelativeMax}",
        source,
        lineNumber
      );
    }

    #endregion
    #region AKA --------------------------------------------------------

    public string AKA;
    public Dictionary<string, CUIComponent> NamedComponents = new Dictionary<string, CUIComponent>();

    public CUIComponent Remember(CUIComponent c, string name)
    {
      NamedComponents[name] = c;
      c.AKA = name;
      return c;
    }
    public CUIComponent Remember(CUIComponent c) => NamedComponents[c.AKA ?? ""] = c;

    public CUIComponent this[string name]
    {
      get => NamedComponents.GetValueOrDefault(name);
      set => Append(value, name);
    }

    public T Get<T>(string name) where T : CUIComponent => NamedComponents.GetValueOrDefault(name) as T;

    #endregion
  }
}