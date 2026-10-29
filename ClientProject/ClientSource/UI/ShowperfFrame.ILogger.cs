using HarmonyLib;
using System.Collections.Immutable;
using CursedUI;
using Microsoft.Xna.Framework;
using System.Reflection;
using CUILibs;

namespace ShowperfExtensions;

public partial class ShowperfFrame : CUIDefault.Frame
{
  public class Logger_Part(ShowperfFrame self) : ILogger
  {
    public ShowperfFrame Self { get; } = self;

    public void Error(object msg)
    {
      Self.FooterTextBlock.TextColor = Color.Red;
      Self.FooterText = $"{msg}";
    }

    public void Log(object msg)
    {
      Self.FooterTextBlock.TextColor = Color.White;
      Self.FooterText = $"{msg}";
    }

    public void Warning(object msg)
    {
      Self.FooterTextBlock.TextColor = Color.Yellow;
      Self.FooterText = $"{msg}";
    }
  }


}