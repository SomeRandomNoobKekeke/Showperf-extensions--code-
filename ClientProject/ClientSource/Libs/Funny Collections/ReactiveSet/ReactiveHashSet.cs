using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;

namespace ShowperfExtensions;


public partial class ReactiveHashSet<T>
{
  public ReactiveHashSet() { }
  public ReactiveHashSet(IEnumerable<T> items) => FillSilent(items);

  private HashSet<T> _Set = [];

  public event Action? Changed;
  public event Action? ItemAdded;
  public event Action? ItemRemoved;

  public int Count => _Set.Count;

  public void Fill(IEnumerable<T> items)
  {
    _Set = items.ToHashSet();
    Changed?.Invoke();
  }

  public bool Add(T item)
  {
    bool ok = _Set.Add(item);
    if (ok)
    {
      ItemAdded?.Invoke();
      Changed?.Invoke();
    }
    return ok;
  }

  public bool Remove(T item)
  {
    bool ok = _Set.Remove(item);
    if (ok)
    {
      ItemRemoved?.Invoke();
      Changed?.Invoke();
    }
    return ok;
  }

  public void Clear()
  {
    if (_Set.Count == 0) return;
    _Set.Clear();
    Changed?.Invoke();
  }

  public bool Contains(T item) => _Set.Contains(item);
  public void CopyTo(T[] array, int arrayIndex) => _Set.CopyTo(array, arrayIndex);

  public IEnumerator<T> GetEnumerator() => _Set.GetEnumerator();

  public void ExceptWith(IEnumerable<T> other)
  {
    _Set.ExceptWith(other);
    Changed?.Invoke();
  }

  public void SymmetricExceptWith(IEnumerable<T> other)
  {
    _Set.SymmetricExceptWith(other);
    Changed?.Invoke();
  }

  public void UnionWith(IEnumerable<T> other)
  {
    _Set.UnionWith(other);
    Changed?.Invoke();
  }

  public void IntersectWith(IEnumerable<T> other) => _Set.IntersectWith(other);
  public bool IsProperSubsetOf(IEnumerable<T> other) => _Set.IsProperSubsetOf(other);
  public bool IsProperSupersetOf(IEnumerable<T> other) => _Set.IsProperSupersetOf(other);
  public bool IsSubsetOf(IEnumerable<T> other) => _Set.IsSubsetOf(other);
  public bool IsSupersetOf(IEnumerable<T> other) => _Set.IsSupersetOf(other);
  public bool Overlaps(IEnumerable<T> other) => _Set.Overlaps(other);
  public bool SetEquals(IEnumerable<T> other) => _Set.SetEquals(other);

}