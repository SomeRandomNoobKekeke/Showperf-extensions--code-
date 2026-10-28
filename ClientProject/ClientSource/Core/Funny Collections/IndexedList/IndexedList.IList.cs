using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;
using System.Collections;

namespace ShowperfExtensions;

public partial class IndexedList<T> : IList
{
  object? IList.this[int index]
  {
    get => this[index];
    set => this[index] = (T)value;
  }

  bool IList.IsFixedSize => false;
  bool IList.IsReadOnly => IsReadOnly;
  int ICollection.Count => Count;
  bool ICollection.IsSynchronized => (_List as IList).IsSynchronized;
  object ICollection.SyncRoot => (_List as IList).SyncRoot;
  int IList.Add(object? value)
  {
    Add((T)value);
    return Count - 1;
  }
  void IList.Clear() => Clear();
  bool IList.Contains(object? value) => Contains((T)value);
  void ICollection.CopyTo(Array array, int index) => (_List as IList).CopyTo(array, index);
  int IList.IndexOf(object? value) => IndexOf((T)value);
  void IList.Insert(int index, object? value) => Insert(index, (T)value);
  void IList.Remove(object? value) => Remove((T)value);
  void IList.RemoveAt(int index) => RemoveAt(index);
}