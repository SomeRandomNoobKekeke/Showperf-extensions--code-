using System.Reflection;
using Barotrauma;
using CUILibs;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using System.Text;

namespace ShowperfExtensions;

public static class MethodBaseParser
{
  public static BindingFlags Pls = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

  public static string StaticConstuctorName = "StaticConstructor";
  public static string ConstuctorName = "Constructor";

  public static Assembly[] AssembliesToScan = [
    typeof(GameMain).Assembly,
    typeof(Color).Assembly,
    typeof(Fixture).Assembly,
  ];

  public static string Serialize(MethodBase mb)
  {
    string SerializeParameterInfo(ParameterInfo pi)
    {
      string SerializeType(Type T)
      {
        if (T.IsByRef) return SerializeType(T.GetElementType());

        if (Nullable.GetUnderlyingType(T) != null)
        {
          return $"{Nullable.GetUnderlyingType(T).Name}?";
        }

        if (T.IsGenericType)
        {
          Type[] genArgs = T.GetGenericArguments();
          return $"{T.Name.Substring(0, T.Name.Length - 2)}<{string.Join(", ", genArgs.Select(t => SerializeType(t)))}>";
        }

        return T.Name;
      }

      if (pi.IsOut) return $"out {SerializeType(pi.ParameterType)}";
      if (pi.IsIn) return $"in {SerializeType(pi.ParameterType)}";
      if (pi.ParameterType.IsByRef) return $"ref {SerializeType(pi.ParameterType)}";

      return SerializeType(pi.ParameterType);
    }

    if (mb is null) return "";

    string methodName = mb is ConstructorInfo ?
    mb.IsConstructor ? ConstuctorName : StaticConstuctorName :
    mb.Name;


    StringBuilder sb = new();
    sb.Append($"{mb.DeclaringType.FullName}.{methodName}");
    if (mb is MethodInfo mi)
    {
      if (mi.ContainsGenericParameters)
      {
        sb.Append($"<{string.Join(',', mi.GetGenericArguments().Select(T => T.Name))}>");
      }
    }
    sb.Append($"({string.Join(", ", mb.GetParameters().Select(SerializeParameterInfo))})");

    return sb.ToString();
  }

  public static MethodBase? Parse(string raw)
  {
    int endOfName = raw.IndexOfAny(['(', '<']);
    int startOfArgs = raw.IndexOf('(');

    // bool hasGenericArgs = endOfName != startOfArgs;
    // Akshually i don't care because i compare by serializing back xd


    string[] methodNameParts = raw.Substring(0, endOfName).Split('.');
    string args = raw.Substring(endOfName, raw.Length - endOfName);

    if (methodNameParts.Length < 2) return null;

    string typeName = string.Join('.', methodNameParts.SkipLast(1));
    string methodName = methodNameParts.Last();

    Type T = null;
    foreach (Assembly assembly in AssembliesToScan)
    {
      T = assembly.GetType(typeName);
      if (T is not null) break;
    }

    if (T is null) return null;

    if (methodName == ConstuctorName)
    {
      MethodBase[] constructor = T.GetConstructors(Pls);

      foreach (MethodBase mb in constructor)
      {
        if (Serialize(mb) == raw) return mb;
      }

      return null;
    }

    if (methodName == StaticConstuctorName)
    {
      return T.GetConstructor(BindingFlags.Static | BindingFlags.NonPublic, []);
    }

    MethodInfo[] methods = T.GetMethods(Pls).Where(mi => mi.Name == methodName).ToArray();

    if (methods.Length == 0) return null;
    if (methods.Length == 1) return methods[0];

    foreach (MethodInfo mi in methods)
    {
      if (Serialize(mi) == raw) return mi;
    }

    return null;
  }
}