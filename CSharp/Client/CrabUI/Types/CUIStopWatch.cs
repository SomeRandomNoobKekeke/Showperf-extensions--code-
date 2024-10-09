// using System;
// using System.Diagnostics;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;

// using Barotrauma;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Input;
// using Microsoft.Xna.Framework.Graphics;

// namespace CrabUI
// {
//   public static class CUIStopWatch
//   {
//     public static Dictionary<int, Stopwatch> Watches = new Dictionary<int, Stopwatch>();

//     public static void Start(int watch = 1)
//     {
//       if (!Watches.ContainsKey(watch)) Watches[watch] = new Stopwatch();
//       Watches[watch].Start();
//     }

//     public static void Stop(int watch = 1)
//     {
//       if (!Watches.ContainsKey(watch)) return;
//       Watches[watch].Stop();
//     }

//     public static void Restart(int watch = 1)
//     {
//       if (!Watches.ContainsKey(watch)) Watches[watch] = new Stopwatch();
//       Watches[watch].Restart();
//     }

//     public static void DumpAndRestart(string name, int watch = 1)
//     {
//       if (!Watches.ContainsKey(watch)) return;
//       CUI.Capture(CUIStopWatch.ElapsedTicks(watch), name);
//     }

//     public static long ElapsedMilliseconds(int watch = 1)
//     {
//       if (!Watches.ContainsKey(watch)) return 0;
//       return Watches[watch].ElapsedMilliseconds;
//     }

//     public static long ElapsedTicks(int watch = 1)
//     {
//       if (!Watches.ContainsKey(watch)) return 0;
//       return Watches[watch].ElapsedTicks;
//     }
//   }
// }