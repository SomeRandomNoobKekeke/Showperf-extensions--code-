using System.Reflection;
using Barotrauma;
using CUILibs;
using Microsoft.Xna.Framework;

namespace ShowperfExtensions;

public class MethodBaseParserTest : UTestPack
{
  public static BindingFlags Pls = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

  public List<UTest> Serialize()
  {
    return [
      new UTest(
        MethodBaseParser.Serialize(typeof(GameMain).GetMethod("Update", Pls)!),
        "Barotrauma.GameMain.Update(GameTime)"
      ),
      new UTest(
        MethodBaseParser.Serialize(typeof(Character).GetMethod("AddDamage", Pls, [
          typeof(Vector2),
          typeof(IEnumerable<Affliction>),
          typeof(float),
          typeof(bool),
          typeof(Vector2),
          typeof(Character),
          typeof(float),
        ])!),
        "Barotrauma.Character.AddDamage(Vector2, IEnumerable<Affliction>, Single, Boolean, Vector2?, Character, Single)"
      ),
      new UTest(
        MethodBaseParser.Serialize(typeof(Character).GetMethod("AddDamage", Pls, [
          typeof(Vector2),
          typeof(IEnumerable<Affliction>),
          typeof(float),
          typeof(bool),
          typeof(Vector2),
          typeof(Limb).MakeByRefType(),
          typeof(Character),
          typeof(float),
        ])!),
        "Barotrauma.Character.AddDamage(Vector2, IEnumerable<Affliction>, Single, Boolean, Vector2, out Limb, Character, Single)"
      ),
      new UTest(
        MethodBaseParser.Serialize(typeof(Attack).GetConstructor(Pls, [
          typeof(float),
          typeof(float),
          typeof(float),
          typeof(float),
          typeof(float),
          typeof(float),
        ])!),
        "Barotrauma.Attack.Constructor(Single, Single, Single, Single, Single, Single)"
      ),
      new UTest(
        MethodBaseParser.Serialize(typeof(Color).GetMethod("Lerp")!),
        "Microsoft.Xna.Framework.Color.Lerp(Color, Color, Single)"
      ),
      new UTest(
        MethodBaseParser.Serialize(typeof(Barotrauma.Items.Components.ItemContainer.ContainedItem).GetMethod("ToString")!),
        "Barotrauma.Items.Components.ItemContainer+ContainedItem.ToString()"
      ),


    ];
  }

  public List<UTest> SerializeGenericMethod()
  {
    return [
      new UTest(
        MethodBaseParser.Serialize(typeof(CharacterHealth).GetMethod("GetAffliction", 0, [
          typeof(Identifier),typeof(bool)
        ])!),
        "Barotrauma.CharacterHealth.GetAffliction(Identifier, Boolean)"
      ),
      new UTest(
        MethodBaseParser.Serialize(typeof(CharacterHealth).GetMethod("GetAffliction", 1, [
          typeof(Identifier),typeof(bool)
        ])!),
        "Barotrauma.CharacterHealth.GetAffliction<T>(Identifier, Boolean)"
      ),
    ];
  }

  public List<UTest> Parse()
  {
    return [
      new UTest(
        MethodBaseParser.Parse("Barotrauma.GameMain.Update(GameTime)")!,
        typeof(GameMain).GetMethod("Update", Pls)!
      ),
      new UTest(
        MethodBaseParser.Parse("Microsoft.Xna.Framework.Color.Lerp(Color, Color, Single)")!,
        typeof(Color).GetMethod("Lerp")!
      ),
      new UTest(
        MethodBaseParser.Parse("Barotrauma.Character.AddDamage(Vector2, IEnumerable<Affliction>, Single, Boolean, Vector2, out Limb, Character, Single)")!,
        typeof(Character).GetMethod("AddDamage", Pls, [
          typeof(Vector2),
          typeof(IEnumerable<Affliction>),
          typeof(float),
          typeof(bool),
          typeof(Vector2),
          typeof(Limb).MakeByRefType(),
          typeof(Character),
          typeof(float),
        ])!
      ),
      new UTest(
        MethodBaseParser.Parse("Barotrauma.Character.AddDamage(Vector2, IEnumerable<Affliction>, Single, Boolean, Vector2?, Character, Single)")!,
        typeof(Character).GetMethod("AddDamage", Pls, [
          typeof(Vector2),
          typeof(IEnumerable<Affliction>),
          typeof(float),
          typeof(bool),
          typeof(Vector2),
          typeof(Character),
          typeof(float),
        ])!
      ),
      new UTest(
        MethodBaseParser.Parse("Barotrauma.Attack.Constructor(Single, Single, Single, Single, Single, Single)")!,
        typeof(Attack).GetConstructor(Pls, [
          typeof(float),
          typeof(float),
          typeof(float),
          typeof(float),
          typeof(float),
          typeof(float),
        ])!
      ),
    ];
  }

  public List<UTest> ParsingGenericMethod()
  {
    return [
      new UTest(
        MethodBaseParser.Parse("Barotrauma.CharacterHealth.GetAffliction(Identifier, Boolean)")!,
        typeof(CharacterHealth).GetMethod("GetAffliction", 0, [
          typeof(Identifier),typeof(bool)
        ])!
      ),
      new UTest(
        MethodBaseParser.Parse("Barotrauma.CharacterHealth.GetAffliction<T>(Identifier, Boolean)")!,
        typeof(CharacterHealth).GetMethod("GetAffliction", 1, [
          typeof(Identifier),typeof(bool)
        ])!
      ),
    ];
  }

}