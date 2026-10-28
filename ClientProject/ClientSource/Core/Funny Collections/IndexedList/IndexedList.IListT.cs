using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;
using System.Collections;

namespace ShowperfExtensions;

public partial class IndexedList<T> : IList<T>
{
  public T this[int index]
  {
    get => _List[index];
    set
    {
      Forget(_List[index]);
      _List[index] = value;
      Remember(value);
    }
  }

  public int Count => _List.Count;
  public bool IsReadOnly => false;

  public void Add(T item)
  {
    _List.Add(item);
    Remember(item);
  }

  public void Clear()
  {
    _List.Clear();
    Appearances.Clear();
    AppearanceOfNull = 0;
  }

  public bool Contains(T item)
  {
    if (item is null) return AppearanceOfNull > 0;
    return Appearances.ContainsKey(item);
  }

  public void CopyTo(T[] array, int arrayIndex)
  {
    _List.CopyTo(array, arrayIndex);
  }

  public IEnumerator<T> GetEnumerator()
  {
    return _List.GetEnumerator();
  }

  public int IndexOf(T item)
  {
    return _List.IndexOf(item);
  }

  public void Insert(int index, T item)
  {
    _List.Insert(index, item);
    Remember(item);
  }

  public bool Remove(T item)
  {
    bool ok = _List.Remove(item);
    if (ok) Forget(item);
    return ok;
  }

  public void RemoveAt(int index)
  {
    Forget(_List[index]);
    _List.RemoveAt(index);
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return _List.GetEnumerator();
  }
}