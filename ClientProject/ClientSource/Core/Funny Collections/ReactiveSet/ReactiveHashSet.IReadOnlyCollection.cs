using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;

namespace ShowperfExtensions;


public partial class ReactiveHashSet<T> : IReadOnlyCollection<T>
{
  int IReadOnlyCollection<T>.Count => Count;
}