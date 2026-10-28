using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace ShowperfExtensions;

public partial class FunnyDictOfWeakSets<TKey, TValue> : IDictionary<TKey, HashSet<TValue>>
  where TKey : notnull
  where TValue : class
{
  public HashSet<TValue> this[TKey key]
  {
    get
    {
      if (dict.TryGetValue(key, out var set))
      {
        var shaked = dict[key] = Shake(set);
        return shaked.Select(JustGetTarget).ToHashSet();
      }

      return [];
    }
    set
    {
      dict[key] = value.Select(v => new WeakReference<TValue>(v)).ToHashSet();
    }
  }

  public ICollection<TKey> Keys => dict.Keys;
  public ICollection<HashSet<TValue>> Values => dict.Values.Select(Dereference).ToArray();
  public int Count => dict.Count;
  public bool IsReadOnly => false;

  public void Add(TKey key, HashSet<TValue> value) => this[key] = value;
  public void Add(KeyValuePair<TKey, HashSet<TValue>> item)
    => this[item.Key] = item.Value;

  public void Clear() => dict.Clear();


  public bool Contains(KeyValuePair<TKey, HashSet<TValue>> item) => false;

  public bool ContainsKey(TKey key) => dict.ContainsKey(key);

  public void CopyTo(KeyValuePair<TKey, HashSet<TValue>>[] array, int arrayIndex)
  {
    throw new NotImplementedException("too lazy");
  }

  public IEnumerable<KeyValuePair<TKey, HashSet<TValue>>> Enumerate()
  {
    foreach (TKey key in dict.Keys)
    {
      yield return new(key, Dereference(dict[key]));
    }
  }

  public IEnumerator<KeyValuePair<TKey, HashSet<TValue>>> GetEnumerator()
    => Enumerate().GetEnumerator();

  public bool Remove(TKey key) => dict.Remove(key);
  public bool Remove(KeyValuePair<TKey, HashSet<TValue>> item) => dict.Remove(item.Key);

  public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out HashSet<TValue> value)
  {
    value = this[key];
    return true;
  }

  IEnumerator IEnumerable.GetEnumerator() => Enumerate().GetEnumerator();
}