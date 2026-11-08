using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;
using System.Collections;

namespace ShowperfExtensions;


/// <summary>
/// Weird collection, it resembles HashSet but doesn't create strong refs to items  
/// it's fast, but not internally protected from duplicates and can bloat
/// </summary>
public partial class WeakBag<T> where T : class
{
  private List<WeakReference<T>> _References = [];

  public HashSet<T> GetValues()
  {
    HashSet<T> values = [];
    List<WeakReference<T>> shaked = [];

    foreach (var wr in _References)
    {
      if (wr.TryGetTarget(out T? target))
      {
        values.Add(target);
        shaked.Add(wr);
      }
    }

    _References = shaked;
    return values;
  }

  public void Add(T value)
  {
    _References.Add(new WeakReference<T>(value));
  }

  public void Shake()
  {
    _References = _References.Where(wr => wr.IsAlive()).ToList();
  }

}