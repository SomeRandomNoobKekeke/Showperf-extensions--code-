using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;

namespace ShowperfExtensions;

public partial class FunnyDictOfWeakSets<TKey, TValue>
  where TKey : notnull
  where TValue : class
{
  private Dictionary<TKey, HashSet<WeakReference<TValue>>> dict = [];


  public void Remove(TKey key, TValue value)
  {
    if (dict.TryGetValue(key, out var set))
    {
      dict[key] = [.. set.Where(wr => wr.TryGetTarget(out TValue? v) && v != value)];
    }
  }

  public void Add(TKey key, TValue value)
  {
    dict.TryAdd(key, []);
    dict[key] = Shake(dict[key]);
    dict[key].Add(new WeakReference<TValue>(value));
  }
}