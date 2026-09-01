# [RASM_API_GENERATOR_EQUALS]

`Generator.Equals` derives structural equality and member-level differences for attributed `partial` C# types at compile time, reaching neither reflection nor IL injection. Member attributes bind each admitted member to one comparison and hashing policy, and the generated nested `EqualityComparer` projects every difference as a path-anchored `Inequality` a structural merge reconciles.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: attributes selecting each member's comparison policy, the runtime comparer family implementing `IEqualityComparer<TInput>` under a static `Default`, and the difference family `Inequalities` yields.

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :----------------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `EquatableAttribute`                       | class         | structural-equality admission      |
|  [02]   | `DefaultEqualityAttribute`                 | class         | default-comparer opt-in            |
|  [03]   | `IgnoreEqualityAttribute`                  | class         | member exclusion                   |
|  [04]   | `OrderedEqualityAttribute`                 | class         | ordered sequence equality          |
|  [05]   | `UnorderedEqualityAttribute`               | class         | multiplicity-aware equality        |
|  [06]   | `SetEqualityAttribute`                     | class         | set equality                       |
|  [07]   | `ReferenceEqualityAttribute`               | class         | reference identity                 |
|  [08]   | `StringEqualityAttribute`                  | class         | selected string comparison         |
|  [09]   | `PrecisionEqualityAttribute`               | class         | numeric tolerance                  |
|  [10]   | `CustomEqualityAttribute`                  | class         | custom comparer selection          |
|  [11]   | `DefaultEqualityComparer<T>`               | class         | type-default scalar equality       |
|  [12]   | `OrderedEqualityComparer<T>`               | class         | `IEnumerable<T>` sequence equality |
|  [13]   | `UnorderedEqualityComparer<T>`             | class         | `IEnumerable<T>` multiset equality |
|  [14]   | `SetEqualityComparer<T>`                   | class         | `IEnumerable<T>` set equality      |
|  [15]   | `DictionaryEqualityComparer<TKey, TValue>` | class         | `IDictionary` entry equality       |
|  [16]   | `ReferenceEqualityComparer<T>`             | class         | identity equality for `T : class`  |
|  [17]   | `Inequality`                               | struct        | differing-member value             |
|  [18]   | `MemberPath`                               | struct        | ordered segment path               |
|  [19]   | `MemberPathSegment`                        | struct        | kinded path step                   |
|  [20]   | `MemberPathSegmentKind`                    | enum          | path-step vocabulary               |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: declaring an equatable type, calling its generated comparer, and composing the runtime comparers directly.

`[OrderedEquality]`, `[UnorderedEquality]`, and `[SetEquality]` share the three ctor forms below; a `Type` argument resolves a static comparer member named `Default` unless the second argument names one, and `[CustomEquality(Type)]` carries the same default — its second parameter defaults to `"Default"`.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `[Equatable] partial record T(...)`                                   | ctor     | admit every non-static member   |
|  [02]   | `[Equatable(Explicit = true)] partial class T`                        | ctor     | narrow admission to attributed  |
|  [03]   | `[Equatable(IgnoreInheritedMembers = true)] partial class T : B`      | ctor     | drop ancestor members           |
|  [04]   | `[property: OrderedEquality] T[] Items`                               | ctor     | positional-record member policy |
|  [05]   | `[DefaultEquality] private int _field;`                               | ctor     | explicit member opt-in          |
|  [06]   | `[OrderedEquality(StringComparison)]`                                 | ctor     | string-element comparison       |
|  [07]   | `[OrderedEquality(Type)]`                                             | ctor     | comparer-type element selection |
|  [08]   | `[OrderedEquality(Type, string)]`                                     | ctor     | named comparer member           |
|  [09]   | `[StringEquality(StringComparison)]`                                  | ctor     | member string comparison        |
|  [10]   | `[PrecisionEquality(double)]`                                         | ctor     | numeric tolerance band          |
|  [11]   | `[CustomEquality(Type, string)]`                                      | ctor     | custom member comparer          |
|  [12]   | `T.EqualityComparer.Default`                                          | property | canonical comparer entry        |
|  [13]   | `T.EqualityComparer.Default.Equals(T, T)`                             | instance | deep structural equality        |
|  [14]   | `T.EqualityComparer.Default.GetHashCode(T)`                           | instance | member-policy hash              |
|  [15]   | `T.EqualityComparer.Default.Inequalities(T, T, MemberPath = default)` | instance | member diff, base path optional |
|  [16]   | `a.Equals(b)`                                                         | instance | typed equality                  |
|  [17]   | `a == b`                                                              | operator | value equality                  |
|  [18]   | `a != b`                                                              | operator | value inequality                |
|  [19]   | `T.GetHashCode()`                                                     | instance | structural hash override        |
|  [20]   | `new OrderedEqualityComparer<T>(IEqualityComparer<T>)`                | ctor     | nested sequence comparer        |
|  [21]   | `new UnorderedEqualityComparer<T>(IEqualityComparer<T>)`              | ctor     | nested multiset comparer        |
|  [22]   | `new SetEqualityComparer<T>(IEqualityComparer<T>)`                    | ctor     | nested set comparer             |
|  [23]   | `OrderedEqualityComparer<T>.EqualityComparer`                         | property | configured element comparer     |

[MemberPath]: `MemberPath(MemberPathSegment[])` `Append(MemberPathSegment)` `Append(MemberPath)` `Segments`
[MemberPathSegment]: `Property(string)` `Field(string)` `Index(int)` `Key(object)` `Added()` `Removed()` `Kind` `Value`
[MemberPathSegmentKind]: consumers read the added/removed sentinels off `MemberPathSegment.Added().Kind`/`Removed().Kind` rather than transcribing enum case spellings — the factory-projected value survives a case rename where a transcription silently diverges.
[Inequality]: `Path` `Left` `Right`

- `Inequality.ToString()`: renders `{Path}: {Left} → {Right}`.
- `MemberPath.ToString()`: dots property and field segments and brackets index, key, and membership segments, so an inequality reads `Addresses["home"].Street` or `Layers[2]`.
- `DefaultEqualityComparer<T>`: routes a sealed `T` through `EqualityComparer<T>.Default` and every other `T` through `object.Equals`.
- `SetEqualityComparer<T>.Equals`: takes the `ISet<T>.SetEquals` fast path only under the default element comparer.
- `DictionaryEqualityComparer<TKey, TValue>`: takes independent key and value comparers readable back through `KeyEqualityComparer` and `ValueEqualityComparer`; `Equals` matches keys through the dictionary's own lookup while `KeyEqualityComparer` drives entry hashing.
- `UnorderedEqualityComparer<T>.Default`: types as `IEqualityComparer<IEnumerable<T>>` where every sibling `Default` types as its own comparer.
