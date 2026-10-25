using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;
using System.IO;


namespace ShowperfExtensions;

public partial class CallGraph
{
  public static ICallGraphSerializer Serializer => Plugin.CallGraphSerializer;

  public static CallGraph LoadFrom(string path)
  {
    if (!File.Exists(path)) return new CallGraph();
    return Serializer.Parse(File.ReadAllText(path));
  }

  public void SaveTo(string path)
  {
    File.WriteAllText(path, Serializer.Serialize(this));
  }

  public void SaveTo(string path, ICallGraphSerializer serializer)
  {
    File.WriteAllText(path, serializer.Serialize(this));
  }
}