# [RASM_API_GENERATOR_EQUALS]

`Generator.Equals` derives structural equality and member-level difference receipts for attributed `partial` C# types at compile time, reaching neither reflection nor IL injection. Member attributes bind each admitted member to one comparison and hashing policy, and the generated nested `EqualityComparer` projects every difference as a path-anchored receipt a structural merge reconciles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Generator.Equals`
- package: `Generator.Equals` (MIT, © Diego Frata)
- assembly: `Generator.Equals.Runtime` carries the attributes, comparers, and diff family; `Generator.Equals` ships the Roslyn incremental generator under `analyzers/dotnet/cs` and never binds at runtime
- namespace: `Generator.Equals`
- rail: equality

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: attributes selecting each member's comparison policy, the runtime comparer family implementing `IEqualityComparer<TInput>` under a static `Default`, and the receipt family `Inequalities` yields.

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
|  [17]   | `Inequality`                               | struct        | differing-member receipt           |
|  [18]   | `MemberPath`                               | struct        | ordered segment path               |
|  [19]   | `MemberPathSegment`                        | struct        | kinded path step                   |
|  [20]   | `MemberPathSegmentKind`                    | enum          | path-step vocabulary               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: declaring an equatable type, calling its generated comparer, and composing the runtime comparers directly.

`[OrderedEquality]`, `[UnorderedEquality]`, and `[SetEquality]` share the three ctor forms below; a `Type` argument resolves a static comparer member named `Default` unless the second argument names one.

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `[Equatable] partial record T(...)`                              | ctor     | admit every non-static member   |
|  [02]   | `[Equatable(Explicit = true)] partial class T`                   | ctor     | narrow admission to attributed  |
|  [03]   | `[Equatable(IgnoreInheritedMembers = true)] partial class T : B` | ctor     | drop ancestor members           |
|  [04]   | `[property: OrderedEquality] T[] Items`                          | ctor     | positional-record member policy |
|  [05]   | `[DefaultEquality] private int _field;`                          | ctor     | explicit member opt-in          |
|  [06]   | `[OrderedEquality(StringComparison)]`                            | ctor     | string-element comparison       |
|  [07]   | `[OrderedEquality(Type)]`                                        | ctor     | comparer-type element selection |
|  [08]   | `[OrderedEquality(Type, string)]`                                | ctor     | named comparer member           |
|  [09]   | `[StringEquality(StringComparison)]`                             | ctor     | member string comparison        |
|  [10]   | `[PrecisionEquality(double)]`                                    | ctor     | numeric tolerance band          |
|  [11]   | `[CustomEquality(Type, string)]`                                 | ctor     | custom member comparer          |
|  [12]   | `T.EqualityComparer.Default`                                     | property | canonical comparer entry        |
|  [13]   | `T.EqualityComparer.Default.Equals(T, T)`                        | instance | deep structural equality        |
|  [14]   | `T.EqualityComparer.Default.GetHashCode(T)`                      | instance | member-policy hash              |
|  [15]   | `T.EqualityComparer.Default.Inequalities(T, T, MemberPath)`      | instance | member diff under a base path   |
|  [16]   | `a.Equals(b)`                                                    | instance | typed equality                  |
|  [17]   | `a == b`                                                         | operator | value equality                  |
|  [18]   | `a != b`                                                         | operator | value inequality                |
|  [19]   | `T.GetHashCode()`                                                | instance | structural hash override        |
|  [20]   | `new OrderedEqualityComparer<T>(IEqualityComparer<T>)`           | ctor     | nested sequence comparer        |
|  [21]   | `new UnorderedEqualityComparer<T>(IEqualityComparer<T>)`         | ctor     | nested multiset comparer        |
|  [22]   | `new SetEqualityComparer<T>(IEqualityComparer<T>)`               | ctor     | nested set comparer             |
|  [23]   | `OrderedEqualityComparer<T>.EqualityComparer`                    | property | configured element comparer     |

[MemberPath]: `MemberPath(MemberPathSegment[])` `Append(MemberPathSegment)` `Append(MemberPath)` `Segments`
[MemberPathSegment]: `Property(string)` `Field(string)` `Index(int)` `Key(object)` `Added()` `Removed()` `Kind` `Value`
[Inequality]: `Path` `Left` `Right`

- `Inequality.ToString()`: renders `{Path}: {Left} → {Right}`.
- `MemberPath.ToString()`: dots property and field segments and brackets index, key, and membership segments, so a receipt reads `Addresses["home"].Street` or `Layers[2]`.
- `DefaultEqualityComparer<T>`: routes a sealed `T` through `EqualityComparer<T>.Default` and every other `T` through `object.Equals`.
- `SetEqualityComparer<T>.Equals`: takes the `ISet<T>.SetEquals` fast path only under the default element comparer.
- `DictionaryEqualityComparer<TKey, TValue>`: takes independent key and value comparers readable back through `KeyEqualityComparer` and `ValueEqualityComparer`; `Equals` matches keys through the dictionary's own lookup while `KeyEqualityComparer` drives entry hashing.
- `UnorderedEqualityComparer<T>.Default`: types as `IEqualityComparer<IEnumerable<T>>` where every sibling `Default` types as its own comparer.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `EqualsGenerator` matches `Generator.Equals.EquatableAttribute` through `ForAttributeWithMetadataName`, transforms class, record, struct, and record-struct declarations, and emits one `<FullName>.Generator.Equals.g.cs` partial per type.
- Every attribute carries `[Conditional("GENERATOR_EQUALS")]`, so the compiler reads its syntax and elides it from consumer IL.
- `Explicit` narrows admission to `[DefaultEquality]`-marked members; `[IgnoreEquality]` drops a member from `Equals`, `GetHashCode`, and `Inequalities` under every mode.
- With `IgnoreInheritedMembers` unset the generator walks the ancestor chain: the nearest ancestor carrying `[Equatable]` or a hand-written nested `EqualityComparer : IEqualityComparer<T>` takes a `base.Equals` delegation, and every ancestor above an unowned base folds its properties into this type's comparison. An overriding property inherits its base member's equality attribute.
- Hashing diverges from equality by design: `[PrecisionEquality]` members leave `GetHashCode` entirely and `[SetEquality]` members contribute the set comparer's constant zero.
- `[OrderedEquality]`, `[UnorderedEquality]`, and `[SetEquality]` require an `IEnumerable<T>` member; an `IDictionary<TKey, TValue>` member under `[UnorderedEquality]` routes to `DictionaryEqualityComparer<TKey, TValue>`.
- A `partial class` implements `IEquatable<TSelf>` explicitly while record and struct declarations expose typed equality publicly; the nested `EqualityComparer` is `sealed` and shadows an equatable base's comparer with `new`.

[STACKING]:
- `System.IO.Hashing`(`.api/api-hashing.md`): `GetHashCode` stays process-salted in-memory state and `Inequalities` stays a diff rail, so cross-runtime content identity rides the seed-zero `XxHash128` digest over canonical bytes; both rails read the same canonical member set.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): a key-projected `[ValueObject<T>]`, `[SmartEnum<TKey>]`, or record-root `[Union]` already owns its equality, so `[Equatable]` binds the class-root union and the multi-member record whose collection members declare bag, sequence, or set semantics.
- `Riok.Mapperly`(`.api/api-mapperly.md`): a mapped DTO or wire record carries `[Equatable]`, so Mapperly transcribes the shape while the generated comparer decides identity and localizes the member diff.
- within-library: element-comparer ctors nest generated comparers — `new OrderedEqualityComparer<Layer>(Layer.EqualityComparer.Default)` composes deep equality over `[Equatable]` elements — and the same comparers key LINQ `Distinct`/`GroupBy`, `HashSet`, and `ImmutableDictionary` outside generated code at the exact sequence, multiset, set, and dictionary semantics the generated members apply.

[LOCAL_ADMISSION]:
- A graph node or record needing structural equality is `partial` and carries `[Equatable]`; equality reads through `T.EqualityComparer.Default`.
- Every value-bearing collection member declares `[OrderedEquality]`, `[UnorderedEquality]`, or `[SetEquality]`; an unattributed collection compares by reference.
- Member-granular change detection flows through `Inequalities`, whose `MemberPath` anchors the exact member a structural merge or version diff reconciles and whose terminal `MemberPathSegmentKind` carries the change shape.

[RAIL_LAW]:
- Package: `Generator.Equals`
- Owns: compile-time structural value equality, the collection-aware comparer family, and the member-level structured diff.
- Accept: multi-member class-root and record owners whose equality is structural over collection members; direct comparer reuse for LINQ and BCL keying; member-granular change detection feeding a three-way merge.
- Reject: a hand-written `Equals`/`GetHashCode` pair on an equatable owner, and content addressing routed through `GetHashCode` or `Inequalities`.
