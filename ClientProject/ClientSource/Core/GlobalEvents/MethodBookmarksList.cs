using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;
using System.IO;
using CUILibs;
using CursedUI;
using System.Text.Json;

namespace ShowperfExtensions;

public partial class MethodBookmarksList : ReactiveHashSet<MethodBase>
{
  public FunnyDictOfWeakSets<MethodBase, ITrackMethodBookmarks> MethodBookmarksListeners { get; } = new();

  private void NotifyMethodBookmarksListeners()
  {
    foreach (var (method, listeners) in MethodBookmarksListeners)
    {
      foreach (ITrackMethodBookmarks listener in listeners)
      {
        listener.NotifyMethodBookmarksChanged();
      }
    }
  }
}