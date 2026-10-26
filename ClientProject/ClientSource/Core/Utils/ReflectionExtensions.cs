using System.Reflection;

namespace ShowperfExtensions.Extensions;

public static class Reflection_Extensions
{
  public static string GetFullName(this Type T)
  {
    List<string> parts = new List<string>() { T.Name };
    Type declaringType = T.DeclaringType;
    while (declaringType != null)
    {
      parts.Add(declaringType.Name);
      declaringType = declaringType.DeclaringType;
    }
    parts.Reverse();

    return string.Join('.', parts);
  }

  public static string GetFullMethodName(this MethodBase mi)
  {
    List<string> parts = new List<string>() { mi.Name };
    Type declaringType = mi.DeclaringType;
    while (declaringType != null)
    {
      parts.Add(declaringType.Name);
      declaringType = declaringType.DeclaringType;
    }
    parts.Reverse();

    return $"{string.Join('.', parts)}({string.Join(',', mi.GetParameters().Select(pi => pi.ParameterType))})";
  }

  public static IEnumerable<Type> GetTypeChainTo(this Type T, Type rootType)
  {
    yield return T;

    while (T.BaseType != null && T.BaseType.IsAssignableTo(rootType))
    {
      T = T.BaseType;
      yield return T;
    }
  }
}