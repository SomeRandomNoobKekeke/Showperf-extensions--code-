using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;
using System.IO;


namespace ShowperfExtensions;

public class CallGraphSaver(ICallGraphSerializer serializer)
{
  public ICallGraphSerializer Serializer { get; set; } = serializer;

  public CallGraph LoadFrom(string path)
  {
    if (!File.Exists(path)) return new CallGraph();
    return Serializer.Parse(File.ReadAllText(path));
  }

  public void SaveTo(string path, CallGraph graph)
  {
    File.WriteAllText(path, Serializer.Serialize(graph));
  }
}