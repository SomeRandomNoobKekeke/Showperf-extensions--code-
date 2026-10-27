using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public static class Capture
{
  public static CaptureWindow? Update => Plugin.CaptureCore?.Update;
}