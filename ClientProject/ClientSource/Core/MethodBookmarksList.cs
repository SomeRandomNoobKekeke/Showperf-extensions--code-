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
  public string DirPath => Path.Combine("Data", "Mods", ModInfo.Name);
  public string FilePath => Path.Combine(DirPath, "Method Bookmarks.json");


  public void Toggle(MethodBase mb)
  {
    if (Contains(mb)) Remove(mb); else Add(mb);
  }


  public void Load() => Fill(_Load());
  public void LoadSilent() => FillSilent(_Load());

  private IEnumerable<MethodBase> _Load()
  {
    if (!File.Exists(FilePath)) return [];

    List<string> methodNames = [];
    try
    {
      methodNames = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(FilePath)) ?? [];
    }
    catch (Exception e)
    {
      Logger.Default.Warning($"couldn't load method bookmarks file: {e.Message}");
    }

    return methodNames.Select(MethodBaseParser.Parse).Where(mb => mb != null)!;
  }

  public void Save()
  {
    if (!Directory.Exists(DirPath)) Directory.CreateDirectory(DirPath);
    File.WriteAllText(
      FilePath,
      JsonSerializer.Serialize(
        this.Select(MethodBaseParser.Serialize),
        new JsonSerializerOptions()
        {
          WriteIndented = true,
        }
      )
    );
  }

  public MethodBookmarksList()
  {
    LoadSilent();
    Changed += NotifyMethodBookmarksListeners;
    Changed += Save;
  }
}