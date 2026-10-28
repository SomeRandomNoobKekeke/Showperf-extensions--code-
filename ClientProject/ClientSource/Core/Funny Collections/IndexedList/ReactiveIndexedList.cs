using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;
using System.Collections;

namespace ShowperfExtensions;

/// <summary>
/// It's copypaste from CursedUI.ReactiveList but it uses IndexedList
/// </summary>
public class ReactiveIndexedList<T> : IList<T>, IList
{
  public ReactiveIndexedList() { }
  public ReactiveIndexedList(IEnumerable<T> initValues) => FillSilent(initValues);

  public event Action? Changed;
  public event Action<T>? Added;
  public event Action<T>? Removed;

  protected IndexedList<T> _List = new();

  public void Fill(IEnumerable<T> values)
  {
    _List = [.. values];
    Changed?.Invoke();
  }


  public T this[int index]
  {
    get => _List[index];
    set
    {
      _List[index] = value;
      Changed?.Invoke();
      Added?.Invoke(value);
    }
  }

  object? IList.this[int index]
  {
    get => _List[index];
    set
    {
      _List[index] = (T)value;
      Changed?.Invoke();
      Added?.Invoke((T)value);
    }
  }

  public int Count => _List.Count;
  public bool IsReadOnly => ((IList)_List).IsReadOnly;
  public bool IsFixedSize => ((IList)_List).IsFixedSize;
  public bool IsSynchronized => ((IList)_List).IsSynchronized;
  public object SyncRoot => ((IList)_List).SyncRoot;

  public void Add(T item)
  {
    _List.Add(item);
    Changed?.Invoke();
    Added?.Invoke(item);
  }

  public int Add(object? value)
  {
    _List.Add((T)value);
    Changed?.Invoke();
    Added?.Invoke((T)value);
    return _List.Count - 1;
  }

  public void Clear()
  {
    _List.Clear();
    Changed?.Invoke();
  }

  public bool Contains(T item) => _List.Contains(item);

  public bool Contains(object? value) => _List.Contains((T)value);


  public void CopyTo(T[] array, int arrayIndex) => _List.CopyTo(array, arrayIndex);
  public void CopyTo(Array array, int index) => ((IList)_List).CopyTo(array, index);

  public IEnumerator<T> GetEnumerator() => _List.GetEnumerator();

  public int IndexOf(T item) => _List.IndexOf(item);

  public int IndexOf(object? value) => _List.IndexOf((T)value);

  public void Insert(int index, T item)
  {
    _List.Insert(index, item);
    Changed?.Invoke();
    Added?.Invoke(item);
  }

  public void Insert(int index, object? value)
  {
    _List.Insert(index, (T)value);
    Changed?.Invoke();
    Added?.Invoke((T)value);
  }

  public bool Remove(T item)
  {
    bool result = _List.Remove(item);
    Changed?.Invoke();
    Removed?.Invoke(item);
    return result;
  }

  public void Remove(object? value)
  {
    _List.Remove((T)value);
    Changed?.Invoke();
    Removed?.Invoke((T)value);
  }

  public void RemoveAt(int index)
  {
    T item = _List[index];
    _List.RemoveAt(index);
    Changed?.Invoke();
    Removed?.Invoke(item);
  }

  IEnumerator IEnumerable.GetEnumerator() => _List.GetEnumerator();



  #region Silent
  public void FillSilent(IEnumerable<T> values) => _List = [.. values];
  public void AddSilent(T item) => _List.Add(item);
  public void ClearSilent() => _List.Clear();
  public void InsertSilent(int index, T item) => _List.Insert(index, item);
  public bool RemoveSilent(T item) => _List.Remove(item);
  public void RemoveAtSilent(int index) => _List.RemoveAt(index);
  #endregion
}