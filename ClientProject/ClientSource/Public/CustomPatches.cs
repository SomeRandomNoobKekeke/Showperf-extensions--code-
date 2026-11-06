using Barotrauma;
using CursedUI;
using CUILibs;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace ShowperfExtensions;

#if FAKE 
public static class CustomPatches
{
  public const string SettingsPagePropName = "SettingsPage";

  public static void Add(
    string id,
    PatchInfoBase info,
    string desctiption = "",
    Func<CUIComponent>? settingsPageFactory = null
  ){}

  public static void AddAllPatchesInThisAssembly(){}
  public static void AddAllPatchesInAssembly(Assembly assembly){}
  public static void Add<T>() {}
  public static void Add(Type showperfPatchType){}
}
#endif


public static class CustomPatches
{




  private static BindingFlags StaticAny => BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

  public const string SettingsPagePropName = "SettingsPage";


  public static void Add(
    string id,
    PatchInfoBase info,
    string desctiption = "",
    Func<CUIComponent>? settingsPageFactory = null
  )
  {
    Plugin.Instance?.CustomPatchRepo.Add(
      new CustomPatchRepo.Entry(id, info)
      {
        Description = desctiption,
        SettingsPageFactory = settingsPageFactory,
      }
    );
  }

  public static void AddAllPatchesInThisAssembly()
    => AddAllPatchesInAssembly(Assembly.GetCallingAssembly());

  public static void AddAllPatchesInAssembly(Assembly assembly)
  {
    foreach (Type T in assembly.GetTypes())
    {
      if (T.GetCustomAttribute<ShowperfPatchAttribute>() != null)
      {
        Add(T);
      }
    }
  }

  public static void Add<T>() => Add(typeof(T));
  public static void Add(Type showperfPatchType)
  {
    ShowperfPatchAttribute? showperfPatchAttribute = showperfPatchType.GetCustomAttribute<ShowperfPatchAttribute>();

    string entryID = showperfPatchAttribute?.ID ?? showperfPatchType.Name;
    string description = showperfPatchAttribute?.Desctiption ?? "";

    List<PatchInfo> patches = [];

    foreach (MethodInfo mi in showperfPatchType.GetMethods(StaticAny))
    {
      PatchMethodAttribute? attribute = mi.GetCustomAttribute<PatchMethodAttribute>();
      if (attribute is null) continue;
      if (attribute.Target is null) continue;

      patches.Add(new PatchInfo($"{entryID}|{mi.Name}", attribute.Target, mi, attribute.PatchType));
    }

    Func<CUIComponent>? settingsPageFactory = null;
    if (showperfPatchType.GetProperty(SettingsPagePropName, StaticAny) != null)
    {
      PropertyInfo pi = showperfPatchType.GetProperty(SettingsPagePropName, StaticAny)!;
      if (pi.PropertyType.IsAssignableTo(typeof(CUIComponent)))
      {
        settingsPageFactory = () => (CUIComponent)pi.GetValue(null)!;
      }
    }

    Add(
      entryID,
      new MultiPatchInfo($"{entryID} patches", patches),
      description,
      settingsPageFactory
    );
  }
}