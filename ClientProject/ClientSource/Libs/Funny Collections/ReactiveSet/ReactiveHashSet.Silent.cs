using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;

namespace ShowperfExtensions;


public partial class ReactiveHashSet<T>
{
  public void FillSilent(IEnumerable<T> items) => _Set = items.ToHashSet();
  public bool AddSilent(T item) => _Set.Add(item);
  public bool RemoveSilent(T item) => _Set.Remove(item);
  public void ClearSilent() => _Set.Clear();
}