using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;

namespace ShowperfExtensions;

public partial class FunnyDictOfWeakSets<TKey, TValue>
{
  private HashSet<WeakReference<TValue>> Shake(HashSet<WeakReference<TValue>> set)
    => set.Where(wr => wr.IsAlive()).ToHashSet();

  private TValue JustGetTarget(WeakReference<TValue> wr)
  {
    wr.TryGetTarget(out TValue? target);
    return target!;
  }

  private HashSet<TValue> Dereference(HashSet<WeakReference<TValue>> weakSet)
  {
    HashSet<TValue> realSet = [];
    foreach (var wr in weakSet)
    {
      if (wr.TryGetTarget(out TValue value))
      {
        realSet.Add(value);
      }
    }

    return realSet;
  }
}