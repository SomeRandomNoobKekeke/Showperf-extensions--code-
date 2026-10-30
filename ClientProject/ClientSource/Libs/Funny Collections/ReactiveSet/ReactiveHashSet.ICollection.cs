using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;
using System.Collections;

namespace ShowperfExtensions;


public partial class ReactiveHashSet<T> : ICollection<T>
{
  int ICollection<T>.Count => Count;
  bool ICollection<T>.IsReadOnly => false;
  void ICollection<T>.Add(T item) => Add(item);
  void ICollection<T>.Clear() => Clear();
  bool ICollection<T>.Contains(T item) => Contains(item);
  void ICollection<T>.CopyTo(T[] array, int arrayIndex) => CopyTo(array, arrayIndex);
  IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  bool ICollection<T>.Remove(T item) => Remove(item);
}