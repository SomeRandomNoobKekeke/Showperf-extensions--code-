using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;
using System.Collections;

namespace ShowperfExtensions;

public partial class WeakDict<TKey, TValue> : IDictionary<TKey, TValue>
  where TKey : notnull
  where TValue : class
{
  private Dictionary<TKey, WeakReference<TValue>> dict = [];

  public TValue this[TKey key]
  {
    get
    {
      if (dict[key].TryGetTarget(out TValue value))
      {
        return value;
      }

      dict.Remove(key);
      return null;
    }
    set => dict[key] = new WeakReference<TValue>(value);
  }

  public ICollection<TKey> Keys
  {
    get
    {
      List<TKey> aliveKeys = [];
      List<TKey> deadKeys = [];

      foreach (var (key, wr) in dict)
      {
        if (wr.IsAlive())
        {
          aliveKeys.Add(key);
        }
        else
        {
          deadKeys.Add(key);
        }
      }

      foreach (TKey key in deadKeys) dict.Remove(key);

      return aliveKeys;
    }
  }

  public ICollection<TValue> Values
  {
    get
    {
      List<TValue> aliveValues = [];
      List<TKey> deadKeys = [];

      foreach (var (key, wr) in dict)
      {
        if (wr.TryGetTarget(out TValue value))
        {
          aliveValues.Add(value);
        }
        else
        {
          deadKeys.Add(key);
        }
      }

      foreach (TKey key in deadKeys) dict.Remove(key);

      return aliveValues;
    }
  }

  public int Count => dict.Count;
  public bool IsReadOnly => false;

  public void Add(TKey key, TValue value) => this[key] = value;
  public void Add(KeyValuePair<TKey, TValue> item) => this[item.Key] = item.Value;
  public void Clear() => dict.Clear();
  public bool Contains(KeyValuePair<TKey, TValue> item)
    => dict.ContainsKey(item.Key) && dict[item.Key].IsAlive();
  public bool ContainsKey(TKey key) =>
    dict.ContainsKey(key) && dict[key].IsAlive();

  public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
  {
    throw new NotImplementedException();
  }

  public IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
  {
    List<KeyValuePair<TKey, TValue>> alivePairs = [];
    List<TKey> deadKeys = [];

    foreach (var (key, wr) in dict)
    {
      if (wr.TryGetTarget(out TValue value))
      {
        alivePairs.Add(new KeyValuePair<TKey, TValue>(key, value));
      }
      else
      {
        deadKeys.Add(key);
      }
    }

    foreach (TKey key in deadKeys) dict.Remove(key);

    return alivePairs;
  }

  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Enumerate().GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => Enumerate().GetEnumerator();

  public bool Remove(TKey key) => dict.Remove(key);
  public bool Remove(KeyValuePair<TKey, TValue> item) => dict.Remove(item.Key);

  public bool TryGetValue(TKey key, out TValue value)
  {
    if (dict.TryGetValue(key, out var wr))
    {
      if (wr.TryGetTarget(out value))
      {
        return true;
      }
      else
      {
        dict.Remove(key);
        return false;
      }
    }
    else
    {
      value = null;
      return false;
    }
  }

}