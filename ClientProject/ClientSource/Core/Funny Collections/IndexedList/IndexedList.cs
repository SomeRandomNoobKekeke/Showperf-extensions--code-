using System.Reflection;
using Microsoft.Xna.Framework;
using Barotrauma.Extensions;
using Barotrauma;

namespace ShowperfExtensions;

/// <summary>
/// It's List<T> but Contains(T value) is O(1) at a cost of keeping indexed copy of data  
/// It's cringe don't use it
/// </summary>
public partial class IndexedList<T>
{
  private List<T> _List = [];
  private Dictionary<T, int> Appearances = [];
  private int AppearanceOfNull;
  private void Remember(T value)
  {
    if (value is null)
    {
      AppearanceOfNull++;
    }
    else
    {
      Appearances[value] = Appearances.TryGetValue(value, out int i) ? i + 1 : 1;
    }
  }

  private void Forget(T value)
  {
    if (value is null)
    {
      AppearanceOfNull = Math.Max(0, AppearanceOfNull - 1);
    }
    else
    {
      if (Appearances.TryGetValue(value, out int i))
      {
        if (i <= 1)
        {
          Appearances.Remove(value);
        }
        else
        {
          Appearances[value] = i - 1;
        }
      }
    }


  }
}