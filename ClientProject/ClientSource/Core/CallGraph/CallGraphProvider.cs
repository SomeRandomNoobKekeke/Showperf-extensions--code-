using System.Reflection;
using HarmonyLib;
using System.Text;
using CUILibs;
using System.IO;
using Barotrauma;

namespace ShowperfExtensions;

/// <summary>
/// This thing is responsible for providing you actual call graph for this version of the game
/// </summary>
public class CallGraphProvider
{

  private CallGraph _Current; public CallGraph Current
  {
    get => _Current ??= LoadOrExtractCallGraph();
  }

  private CallGraphBuilder CallGraphBuilder = Plugin.Get<CallGraphBuilder>();

  public string DirWhereCallGraphsAreStored => Path.Combine(ModInfo.Dir, "Data", "Call Graphs");
  public string CurrentCallGraphPath => Path.Combine(DirWhereCallGraphsAreStored, $"{GameVersion.CurrentVersion}.json");


  private CallGraph LoadOrExtractCallGraph()
  {
    if (File.Exists(CurrentCallGraphPath))
    {
      return LoadFrom(CurrentCallGraphPath);
    }

    CallGraph callGraph = CallGraphBuilder.ExtractFullCallGraph();
    SaveTo(CurrentCallGraphPath, callGraph);

    return callGraph;
  }

  public ICallGraphSerializer Serializer { get; set; } = new MetadataCallGraphSerializer();

  public CallGraph LoadFrom(string path)
  {
    if (!File.Exists(path)) return new CallGraph();
    return Serializer.Parse(File.ReadAllText(path));
  }

  public void SaveTo(string path, CallGraph graph)
  {
    if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
    File.WriteAllText(path, Serializer.Serialize(graph));
  }
}