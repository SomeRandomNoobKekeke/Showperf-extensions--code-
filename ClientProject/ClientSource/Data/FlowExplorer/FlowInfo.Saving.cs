using System.Reflection;
using HarmonyLib;
using System.Text;
using System.Text.Json;
using CUILibs;
using System.IO;


namespace ShowperfExtensions;

public partial class FlowInfo
{
  public static FlowInfo LoadFrom(string path)
  {
    if (!File.Exists(path)) return new FlowInfo();
    return FlowInfo.Parse(File.ReadAllText(path));
  }

  public void SaveTo(string path)
  {
    File.WriteAllText(path, Serialize());
  }
}