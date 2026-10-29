using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;

namespace ShowperfExtensions;


public partial class ReactiveHashSet<T> : ISet<T>
{
  bool ISet<T>.Add(T item) => Add(item);
  void ISet<T>.ExceptWith(IEnumerable<T> other) => ExceptWith(other);
  void ISet<T>.SymmetricExceptWith(IEnumerable<T> other) => SymmetricExceptWith(other);
  void ISet<T>.UnionWith(IEnumerable<T> other) => UnionWith(other);
  void ISet<T>.IntersectWith(IEnumerable<T> other) => IntersectWith(other);
  bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other) => IsProperSubsetOf(other);
  bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other) => IsProperSupersetOf(other);
  bool ISet<T>.IsSubsetOf(IEnumerable<T> other) => IsSubsetOf(other);
  bool ISet<T>.IsSupersetOf(IEnumerable<T> other) => IsSupersetOf(other);
  bool ISet<T>.Overlaps(IEnumerable<T> other) => Overlaps(other);
  bool ISet<T>.SetEquals(IEnumerable<T> other) => SetEquals(other);


}