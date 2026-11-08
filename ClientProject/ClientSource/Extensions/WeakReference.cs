using System.Reflection;

namespace ShowperfExtensions.Extensions;

public static class WeakReference_Extensions
{
  public static bool IsAlive<T>(this WeakReference<T> wr) where T : class
    => wr.TryGetTarget(out T? _);


}