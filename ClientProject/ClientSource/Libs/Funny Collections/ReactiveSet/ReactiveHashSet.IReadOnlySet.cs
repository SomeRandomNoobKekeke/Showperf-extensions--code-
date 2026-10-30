using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;

namespace ShowperfExtensions;


public partial class ReactiveHashSet<T> : IReadOnlySet<T>
{
  bool IReadOnlySet<T>.Contains(T item) => Contains(item);
  bool IReadOnlySet<T>.IsProperSubsetOf(IEnumerable<T> other) => IsProperSubsetOf(other);
  bool IReadOnlySet<T>.IsProperSupersetOf(IEnumerable<T> other) => IsProperSupersetOf(other);
  bool IReadOnlySet<T>.IsSubsetOf(IEnumerable<T> other) => IsSubsetOf(other);
  bool IReadOnlySet<T>.IsSupersetOf(IEnumerable<T> other) => IsSupersetOf(other);
  bool IReadOnlySet<T>.Overlaps(IEnumerable<T> other) => Overlaps(other);
  bool IReadOnlySet<T>.SetEquals(IEnumerable<T> other) => SetEquals(other);
}