using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;

namespace ShowPerfExtensions
{
  public partial class Plugin : IAssemblyPlugin
  {
    public class ShowperfPatchAttribute : System.Attribute { }

    public static HarmonyMethod ShowperfMethod(MethodInfo? mi)
    {
      return new HarmonyMethod(mi)
      {
        priority = Priority.High,
        //before = ["that.other.harmony.user"]
      };
    }

    public void PatchAll()
    {
      PatchCapture();
      //PatchTechnical();
    }

    public void PatchCapture()
    {
      Assembly CallingAssembly = Assembly.GetCallingAssembly();

      foreach (Type type in CallingAssembly.GetTypes())
      {
        if (Attribute.IsDefined(type, typeof(ShowperfPatchAttribute)))
        {
          MethodInfo init = type.GetMethod("Initialize", AccessTools.all);
          init?.Invoke(null, new object[] { });
        }
      }
    }

    public void PatchTechnical()
    {
      harmony.Patch(
        original: typeof(LuaGame).GetMethod("IsCustomCommandPermitted"),
        postfix: ShowperfMethod(typeof(Plugin).GetMethod("permitCommands"))
      );
    }

  }
}