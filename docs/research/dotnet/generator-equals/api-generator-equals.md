# [GENERATOR_EQUALS_API]

Generator.Equals is a .NET source generator for structural equality. An equatable type is a `partial` class, struct, record, or record struct marked with `EquatableAttribute`. The generator emits `Equals`, `GetHashCode`, the equality operators that the declaration form admits, and a nested `EqualityComparer` class at compile time. The generated code uses no reflection.

Member attributes select the comparison and the hashing of one member. The nested comparer also exposes an `Inequalities` method, which reports each differing member as an `Inequality` that carries the path to that member.

Every public type is in the namespace `Generator.Equals`. The analyzer reports diagnostics with identifiers of the form `GE0##`.

## [01]-[ATTRIBUTES]

[TYPE_SCOPE]: every attribute in `Generator.Equals`. `[TARGET]` states the `AttributeUsage` targets. No attribute sets `AllowMultiple`, so a member accepts each attribute once. Each member attribute also sets `Inherited = true`, which changes nothing here: the generator reads declared attributes and walks the override chain itself.

| [INDEX] | [SYMBOL]                     | [TARGET]        | [CAPABILITY]                           |
| :-----: | :--------------------------- | :-------------- | :------------------------------------- |
|  [01]   | `EquatableAttribute`         | class, struct   | equality generation and its options    |
|  [02]   | `DefaultEqualityAttribute`   | property, field | default comparison and explicit opt-in |
|  [03]   | `IgnoreEqualityAttribute`    | property, field | member exclusion                       |
|  [04]   | `OrderedEqualityAttribute`   | property, field | sequence comparison, order sensitive   |
|  [05]   | `UnorderedEqualityAttribute` | property, field | multiset or dictionary comparison      |
|  [06]   | `SetEqualityAttribute`       | property, field | set comparison, duplicates collapsed   |
|  [07]   | `ReferenceEqualityAttribute` | property, field | reference identity                     |
|  [08]   | `StringEqualityAttribute`    | property, field | selected string comparison             |
|  [09]   | `PrecisionEqualityAttribute` | property, field | numeric tolerance                      |
|  [10]   | `CustomEqualityAttribute`    | property, field | caller-supplied comparer               |

`AttributeTargets` names no `Parameter` target, so a positional record parameter rejects a bare member attribute. The `property:` target, such as `[property: OrderedEquality]`, puts the attribute on the property the compiler synthesizes.

`AttributeTargets.Class` admits a record and `AttributeTargets.Struct` admits a record struct. `EquatableAttribute` names no `Interface` target.

Each attribute class carries `[Conditional("GENERATOR_EQUALS")]`. Unless the project defines that symbol, the compiler omits the attribute usage from the emitted metadata, and runtime reflection cannot read it. Generation is unaffected, because the generator reads the compilation model rather than metadata.

## [02]-[EQUATABLE_OPTIONS]

[TYPE_SCOPE]: the settable properties of `EquatableAttribute`. Each is a named argument. `EquatableAttribute` declares no constructor other than the implicit parameterless one.

| [INDEX] | [PROPERTY]                       | [TYPE] | [DEFAULT] | [EFFECT]                             |
| :-----: | :------------------------------- | :----- | :-------: | :----------------------------------- |
|  [01]   | `Explicit`                       | `bool` |  `false`  | drop the type's unattributed members |
|  [02]   | `IgnoreInheritedMembers`         | `bool` |  `false`  | drop every base-type member          |
|  [03]   | `GenerateClassEqualityOperators` | `bool` |  `true`   | emit `==` and `!=` for a class       |

- `Explicit = true` reduces an unattributed property or field declared on the type to `[IgnoreEquality]`. Every member that carries an equality attribute other than `[IgnoreEquality]`, `[DefaultEquality]` included, stays compared. Members collected from a non-equatable base type are unaffected.
- `IgnoreInheritedMembers = true` drops the inherited members and replaces the `base.Equals` and `base.GetHashCode` calls with a type-identity check. The generated `Inequalities` then skips the base chain.
- `GenerateClassEqualityOperators` applies to a class only. A record or a struct reports `GE011`.
- `GenerateClassEqualityOperators = false` on a derived class alone does not restore reference equality, because C# operator resolution reads base-type operators. The opt-out takes effect only when every `[Equatable]` class in the chain sets it. When an ancestor declares `==` by hand, it cannot take effect at all.

## [03]-[MEMBER_ATTRIBUTE_FORMS]

[TYPE_SCOPE]: the constructors and properties of the member attributes. `OrderedEqualityAttribute`, `UnorderedEqualityAttribute`, and `SetEqualityAttribute` each declare the same three constructors and the same three properties: `Type? ComparerType`, `string ComparerMemberName`, and `StringComparison? StringComparison`.

| [INDEX] | [DECLARATION]                                                     | [CAPABILITY]             |
| :-----: | :---------------------------------------------------------------- | :----------------------- |
|  [01]   | `[DefaultEquality]`                                               | include under `Explicit` |
|  [02]   | `[IgnoreEquality]`                                                | exclude the member       |
|  [03]   | `[ReferenceEquality]`                                             | compare by reference     |
|  [04]   | `[OrderedEquality]`                                               | default element comparer |
|  [05]   | `[OrderedEquality(StringComparison stringComparison)]`            | element `StringComparer` |
|  [06]   | `[OrderedEquality(Type comparerType, string comparerMemberName)]` | named element comparer   |
|  [07]   | `[UnorderedEquality]`, with the same three constructors           | multiset or dictionary   |
|  [08]   | `[SetEquality]`, with the same three constructors                 | set comparison           |
|  [09]   | `[StringEquality(StringComparison comparisonType)]`               | one string comparison    |
|  [10]   | `[PrecisionEquality(double precision)]`                           | one numeric tolerance    |
|  [11]   | `[CustomEquality(Type equalityType, string fieldOrPropertyName)]` | whole-member comparer    |

`CustomEqualityAttribute` declares one constructor and names its properties `EqualityType` and `FieldOrPropertyName`. Its second parameter defaults to `"Default"`, as does `comparerMemberName` on the three collection attributes, so rows [06] and [11] each also accept the one-argument form.

`StringEqualityAttribute` exposes `ComparisonType`. `PrecisionEqualityAttribute` exposes `Precision`.

## [04]-[EMITTED_COMPARISONS]

[EXPRESSION_SCOPE]: the comparer the generator emits for one member `M`. `T` is the member type, `E` the collection element type, and `S` the `StringComparer` static member that matches the `StringComparison` value. The table elides the `global::Generator.Equals.` prefix that every emitted runtime comparer carries.

| [INDEX] | [ATTRIBUTE]                                     | [COMPARER]                                 | [HASH] |
| :-----: | :---------------------------------------------- | :----------------------------------------- | :----: |
|  [01]   | none outside `Explicit`, or `[DefaultEquality]` | `DefaultEqualityComparer<T>`               |  yes   |
|  [02]   | `[IgnoreEquality]`                              | none                                       |   no   |
|  [03]   | `[ReferenceEquality]`                           | `ReferenceEqualityComparer<T>`             |  yes   |
|  [04]   | `[OrderedEquality]`                             | `OrderedEqualityComparer<E>`               |  yes   |
|  [05]   | `[UnorderedEquality]`                           | `UnorderedEqualityComparer<E>`             |  yes   |
|  [06]   | `[UnorderedEquality]` on an `IDictionary`       | `DictionaryEqualityComparer<TKey, TValue>` |  yes   |
|  [07]   | `[SetEquality]`                                 | `SetEqualityComparer<E>`                   |  yes   |
|  [08]   | `[StringEquality]`                              | `global::System.StringComparer.S`          |  yes   |
|  [09]   | `[PrecisionEquality]`                           | `global::System.Math.Abs`                  |   no   |
|  [10]   | `[CustomEquality]`                              | the resolved comparer                      |  yes   |

- Rows [01], [03], and [06] always emit `.Default`. Rows [04], [05], and [07] emit `.Default` when the attribute carries no argument, and `new OrderedEqualityComparer<E>(elementComparer)` or its sibling when it carries one.
- `[PrecisionEquality]` emits `Math.Abs(x.M - y.M) < precision`, a strict `<`. On a `Nullable<T>` member it emits `(x.M == y.M || (x.M.HasValue && y.M.HasValue && Math.Abs(x.M.Value - y.M.Value) < precision))`, parenthesized as a whole. The literal carries the `f` suffix for `float` and the `m` suffix for `decimal`.
- `StringComparison` maps to the matching `StringComparer` static property: `CurrentCulture`, `CurrentCultureIgnoreCase`, `InvariantCulture`, `InvariantCultureIgnoreCase`, `Ordinal`, `OrdinalIgnoreCase`. A value outside these six makes the generator throw under `[StringEquality]`. On the three collection attributes it falls back to the default element comparer instead.
- A named comparer type resolves to `ComparerType.MemberName` when the type itself declares a static member of that name, and to `new ComparerType()` when it does not. The lookup reads declared members only, so an inherited static member does not match.
- `[OrderedEquality]` and `[SetEquality]` read the element type from `IEnumerable<T>`. `[UnorderedEquality]` reads `IDictionary<TKey, TValue>` first and falls back to `IEnumerable<T>`, so it reads an element type only when the member is not a dictionary.
- On a dictionary member, `[UnorderedEquality]` always emits `DictionaryEqualityComparer<TKey, TValue>.Default` and discards its own comparer arguments. The generator emits that comparer for no other attribute.

## [05]-[GENERATED_MEMBERS]

[MEMBER_SCOPE]: the members the generator adds to the partial declaration of a type `T`. The generator writes one file per type. The file name is the `global::`-qualified type name, then `.Generator.Equals.g.cs`, with `<`, `>`, `,`, and `:` each replaced by `_`, so the name starts `global__`. Each file carries `#nullable enable`, `#pragma warning disable CS0612,CS0618`, and `#pragma warning disable CS0436`.

| [INDEX] | [DECLARATION]                                       | [CLASS] | [STRUCT] | [RECORD] | [RECORD_STRUCT] |
| :-----: | :-------------------------------------------------- | :-----: | :------: | :------: | :-------------: |
|  [01]   | `: global::System.IEquatable<T>` on the partial     |   yes   |   yes    |    no    |       no        |
|  [02]   | `public static bool operator ==(T? left, T? right)` |   opt   |    no    |    no    |       no        |
|  [03]   | `public static bool operator !=(T? left, T? right)` |   opt   |    no    |    no    |       no        |
|  [04]   | `public static bool operator ==(T left, T right)`   |   no    |   yes    |    no    |       no        |
|  [05]   | `public static bool operator !=(T left, T right)`   |   no    |   yes    |    no    |       no        |
|  [06]   | `public override bool Equals(object? obj)`          |   yes   |   yes    |    no    |       no        |
|  [07]   | `bool global::System.IEquatable<T>.Equals(T? obj)`  |   yes   |    no    |    no    |       no        |
|  [08]   | `private bool Equals(T? other)` on a sealed class   |   yes   |    no    |    no    |       no        |
|  [09]   | `protected bool Equals(T? other)` otherwise         |   yes   |    no    |    no    |       no        |
|  [10]   | `public bool Equals(T other)`                       |   no    |   yes    |    no    |       yes       |
|  [11]   | `public bool Equals(T? other)` on a sealed record   |   no    |    no    |   yes    |       no        |
|  [12]   | `public virtual bool Equals(T? other)` otherwise    |   no    |    no    |   yes    |       no        |
|  [13]   | `public override int GetHashCode()`                 |   yes   |   yes    |   yes    |       yes       |
|  [14]   | nested sealed `EqualityComparer` class              |   yes   |   yes    |   yes    |       yes       |

`opt` means the generator emits the member unless `GenerateClassEqualityOperators` is `false`. Nothing else changes when it is `false`.

- Every generated member carries `[GeneratedCode("Generator.Equals", "1.0.0.0")]`, except the nested comparer's `Default`, `Equals`, and `GetHashCode`.
- A record and a record struct already carry `IEquatable<T>`, `==`, `!=`, `Equals(object?)`, and `PrintMembers` from the compiler. A record also carries `EqualityContract`. The generator supplies the typed `Equals` that those compiler-emitted members call, and replaces the compiler's `GetHashCode` with its own override.
- On a class, the typed `Equals(T? other)` overload is not public. A caller outside the type that writes `a.Equals(b)` on a class binds to `Equals(object?)`. The typed overload is reachable through `IEquatable<T>` or through `T.EqualityComparer.Default.Equals(a, b)`. On a record the typed overload is public and wins overload resolution.
- On a class, the comparison logic lives in the type's own `Equals` and `GetHashCode`, and the nested comparer delegates to them. On a struct the direction reverses: the type's members delegate to the nested comparer, which holds the logic. A record and a record struct follow the class direction.
- The class `Equals` chains to `base.Equals` when an ancestor is equatable, and otherwise compares `other.GetType() == this.GetType()`. The record `Equals` chains whenever a base record exists, without that ancestor test, and otherwise compares `EqualityContract`. `IgnoreInheritedMembers = true` suppresses both chains.
- `GetHashCode` adds `base.GetHashCode()` under the same condition, and otherwise adds `this.GetType()` for a class or `this.EqualityContract` for a record.
- A nested type and a generic type are both supported. The generator emits the containing `partial` declarations around the type.
- The generator accepts a `ref struct` declaration, but the code it emits for one does not compile: the `Equals(object?)` pattern match and the `Inequalities` iterator are both invalid on a ref struct. No diagnostic reports the case.

## [06]-[NESTED_COMPARER]

[COMPARER_SCOPE]: the `EqualityComparer` class the generator nests inside every equatable type `T`.

| [INDEX] | [SURFACE]                                                                            | [KIND]   | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `public sealed class EqualityComparer : IEqualityComparer<T>`                        | class    | generated equality                   |
|  [02]   | `public new sealed class EqualityComparer` on a derived type                         | class    | hide the base comparer               |
|  [03]   | `public static EqualityComparer Default { get; }`                                    | property | shared instance                      |
|  [04]   | `public bool Equals(T? x, T? y)` on a class or record                                | method   | structural equality                  |
|  [05]   | `public bool Equals(T x, T y)` on a struct or record struct                          | method   | structural equality                  |
|  [06]   | `public int GetHashCode(T obj)`                                                      | method   | structural hash                      |
|  [07]   | `public IEnumerable<Inequality> Inequalities(T? x, T? y, MemberPath path = default)` | method   | class and record differences         |
|  [08]   | `public IEnumerable<Inequality> Inequalities(T x, T y, MemberPath path = default)`   | method   | struct and record struct differences |

- The `new` modifier appears when an ancestor carries `[Equatable]` or already declares a sealed nested `EqualityComparer` for itself, so the derived comparer hides the inherited one. A struct and a record struct never receive it.
- `Inequalities` is an iterator method. Nothing runs until the caller enumerates it, and the enumeration reports every difference rather than stopping at the first.
- On a class or record, `Inequalities` yields nothing for two references to the same instance. When exactly one side is null it yields one `Inequality` carrying the incoming `path` and the two objects, then stops. The struct form has no such guard.
- When an ancestor is equatable and `IgnoreInheritedMembers` is `false`, the method first yields every inequality from the immediate base type's `EqualityComparer.Default.Inequalities(x, y, path)`, with the same `path`. A member declared on a non-equatable ancestor between the type and its equatable ancestor is not reported.
- A member that is neither a collection nor a dictionary yields one `Inequality` whose path appends `MemberPathSegment.Property(name)`, or `Field(name)` for a field. Such a member is never walked into, even when its own type is equatable.
- A collection or dictionary member appends its own `Property(name)` or `Field(name)` segment first, then appends one per-element segment to that.
- An `[OrderedEquality]` member copies both sides into a list and walks to the longer length, appending `MemberPathSegment.Index(i)`. A null side becomes an empty list, so it reports one inequality per element of the other side; a value-type collection is copied without a null check. When the element type itself carries `[Equatable]`, an in-range pair recurses into that element's own `Inequalities`; otherwise the walk compares the boxed elements. A position present on one side only yields `null` for the missing side.
- A `[SetEquality]` member, and an `[UnorderedEquality]` member whose type is not a dictionary, builds a `HashSet` of the element type per side and reports the two set differences: `MemberPathSegment.Removed()` with the element on the left and `null` on the right, then `MemberPathSegment.Added()` with `null` on the left and the element on the right. The report carries no element multiplicity and walks into no element.
- An `[UnorderedEquality]` member whose type is a dictionary appends `MemberPathSegment.Key(key)`. A key missing on one side yields `null` for that side. A null dictionary on one side reports every entry of the other side under its own key, and a value-type dictionary member emits no null branch. When the value type itself carries `[Equatable]`, a matched pair recurses into that value's own `Inequalities`.
- A collection or dictionary member yields nothing when its configured comparer already reports the two sides equal.
- A struct or record struct that declares no property and no field at all yields nothing. The generator emits that `yield break` only for an empty member list, so a struct whose every member is excluded instead produces a body with no statement, which does not compile.

## [07]-[MEMBER_SELECTION]

[SELECTION_SCOPE]: the rules that decide which members the generator compares, and the order in which it reads their attributes.

An instance property that is not an indexer is compared. An instance field is compared when a name can reference it, which excludes the compiler-generated backing field of an auto-property. A static member is never compared, and a `const` field is static. In a record, a member named `EqualityContract` is excluded by that name. Accessibility does not narrow the set: a private field and a private property are compared like public ones.

The generator reads the first matching attribute in this order and ignores the rest.

| [INDEX] | [ATTRIBUTE]                  |
| :-----: | :--------------------------- |
|  [01]   | `IgnoreEqualityAttribute`    |
|  [02]   | `UnorderedEqualityAttribute` |
|  [03]   | `OrderedEqualityAttribute`   |
|  [04]   | `ReferenceEqualityAttribute` |
|  [05]   | `SetEqualityAttribute`       |
|  [06]   | `StringEqualityAttribute`    |
|  [07]   | `PrecisionEqualityAttribute` |
|  [08]   | `CustomEqualityAttribute`    |

- A member that matches none of the eight uses `DefaultEqualityComparer<T>`, or drops out when `Explicit` is `true` and the member carries no `[DefaultEquality]`.
- Attribute lookup on a property walks the `OverriddenProperty` chain, so an overriding property inherits the equality attribute of the property it overrides. A field has no such chain.
- Each attribute in the table is probed in turn, and each probe walks that chain. An attribute re-declared on the override therefore wins only when it ranks at or above the inherited one.
- When `IgnoreInheritedMembers` is `false`, an overriding property is dropped from the derived type's own members if any type in its `OverriddenProperty` chain is equatable, because that ancestor's comparer already compares it.
- A base type counts as equatable when it carries `[Equatable]` or when it declares a sealed nested type named `EqualityComparer` whose directly declared interfaces include one named `IEqualityComparer` with that type as its single type argument. The match reads the interface name only, not its namespace. The second rule lets an equatable base type in a referenced assembly chain correctly, because the attribute itself does not survive into metadata.
- When no ancestor is equatable and `IgnoreInheritedMembers` is `false`, the generator collects the members of every base type up to `object` and compares them directly. It collects those members without `Explicit` mode. An ancestor property whose name matches an `override` property on the derived type is skipped, so the derived declaration supplies it. That match reads the name only, and applies to properties only.
- The generator recognizes the attribute name `Equatable` or `EquatableAttribute` in the namespace `Generator.Equals`.

## [08]-[RUNTIME_COMPARERS]

[TYPE_SCOPE]: the public comparer classes in `Generator.Equals`. The three collection comparers and the dictionary comparer compare a collection, so their `IEqualityComparer<>` argument is the collection type, not the element type.

| [INDEX] | [SYMBOL]                                       | [IMPLEMENTS]                                   |
| :-----: | :--------------------------------------------- | :--------------------------------------------- |
|  [01]   | `DefaultEqualityComparer<T>`                   | `IEqualityComparer<T>`                         |
|  [02]   | `ReferenceEqualityComparer<T> where T : class` | `IEqualityComparer<T>`                         |
|  [03]   | `OrderedEqualityComparer<T>`                   | `IEqualityComparer<IEnumerable<T>>`            |
|  [04]   | `UnorderedEqualityComparer<T>`                 | `IEqualityComparer<IEnumerable<T>>`            |
|  [05]   | `SetEqualityComparer<T>`                       | `IEqualityComparer<IEnumerable<T>>`            |
|  [06]   | `DictionaryEqualityComparer<TKey, TValue>`     | `IEqualityComparer<IDictionary<TKey, TValue>>` |

- `Equals` on all four collection comparers returns `true` when both arguments are the same reference, which covers two nulls, and `false` when exactly one argument is null.
- `DefaultEqualityComparer<T>.GetHashCode` takes a non-nullable `T`. Every other `GetHashCode` above takes a nullable argument and returns `0` for null, except `SetEqualityComparer<T>.GetHashCode`, which returns `0` for every input.
- `DefaultEqualityComparer<T>` routes a sealed `T` through `EqualityComparer<T>.Default`. It routes every other `T` through `object.Equals`, which dispatches on the runtime type, and hashes it as `obj?.GetHashCode() ?? 0`. Every struct is sealed, so a struct always takes the first branch.
- `ReferenceEqualityComparer<T>` compares by reference identity and hashes with `RuntimeHelpers.GetHashCode`.
- `OrderedEqualityComparer<T>.Equals` compares with `SequenceEqual` under the element comparer. `GetHashCode` combines the elements in order.
- `UnorderedEqualityComparer<T>.Equals` short-circuits on a count mismatch only when both sides implement `ICollection<T>`, then compares element multiplicity through a `Dictionary<T, int>`. `GetHashCode` masks each element hash with `& 0x7FFFFFFF` and folds the results with `XOR`, so it is order independent and skips null elements.
- `SetEqualityComparer<T>.Equals` takes the `ISet<T>.SetEquals` fast path only when the element comparer is the same instance as `DefaultEqualityComparer<T>.Default` and at least one argument is an `ISet<T>`. It tests the left argument first, so the left set's own comparer decides when both are sets. Otherwise it builds a `HashSet<T>` under the element comparer.
- `DictionaryEqualityComparer<TKey, TValue>.Equals` compares counts, then looks each left key up through the right dictionary's own `TryGetValue`, so the right dictionary's comparer decides key identity. `KeyEqualityComparer` drives hashing only, and `Equals` never reads it. `GetHashCode` masks each entry hash with `& 0x7FFFFFFF` and folds the results with `XOR`.
- `XOR` folding cancels equal hashes in pairs, so a duplicated element contributes nothing to the result.

## [09]-[COMPARER_SURFACE]

[SURFACE_SCOPE]: the static `Default`, the public constructors, and the configuration properties of the six runtime comparers. Each class also implements the `Equals` and `GetHashCode` of its interface, which section [08] describes.

| [INDEX] | [SURFACE]                                                                                          | [KIND]   |
| :-----: | :------------------------------------------------------------------------------------------------- | :------- |
|  [01]   | `DefaultEqualityComparer<T>.Default`                                                               | property |
|  [02]   | `ReferenceEqualityComparer<T>.Default`                                                             | property |
|  [03]   | `OrderedEqualityComparer<T>.Default`                                                               | property |
|  [04]   | `UnorderedEqualityComparer<T>.Default`                                                             | property |
|  [05]   | `SetEqualityComparer<T>.Default`                                                                   | property |
|  [06]   | `DictionaryEqualityComparer<TKey, TValue>.Default`                                                 | property |
|  [07]   | `new OrderedEqualityComparer<T>()`                                                                 | ctor     |
|  [08]   | `new OrderedEqualityComparer<T>(IEqualityComparer<T>)`                                             | ctor     |
|  [09]   | `new UnorderedEqualityComparer<T>()`                                                               | ctor     |
|  [10]   | `new UnorderedEqualityComparer<T>(IEqualityComparer<T>)`                                           | ctor     |
|  [11]   | `new SetEqualityComparer<T>()`                                                                     | ctor     |
|  [12]   | `new SetEqualityComparer<T>(IEqualityComparer<T>)`                                                 | ctor     |
|  [13]   | `new DictionaryEqualityComparer<TKey, TValue>()`                                                   | ctor     |
|  [14]   | `new DictionaryEqualityComparer<TKey, TValue>(IEqualityComparer<TKey>, IEqualityComparer<TValue>)` | ctor     |
|  [15]   | `OrderedEqualityComparer<T>.EqualityComparer`                                                      | property |
|  [16]   | `UnorderedEqualityComparer<T>.EqualityComparer`                                                    | property |
|  [17]   | `SetEqualityComparer<T>.EqualityComparer`                                                          | property |
|  [18]   | `DictionaryEqualityComparer<TKey, TValue>.KeyEqualityComparer`                                     | property |
|  [19]   | `DictionaryEqualityComparer<TKey, TValue>.ValueEqualityComparer`                                   | property |

- `DefaultEqualityComparer<T>` and `ReferenceEqualityComparer<T>` take no comparer argument. Each exposes only the implicit public parameterless constructor.
- Every parameterless constructor above uses `DefaultEqualityComparer<>.Default`, closed over the element, key, or value type.
- `UnorderedEqualityComparer<T>.Default` is declared as `IEqualityComparer<IEnumerable<T>>`. Every sibling `Default` is declared as its own comparer type, so only this one hides `EqualityComparer` behind the interface. Reaching that property needs a cast.

## [10]-[INEQUALITY_TYPES]

[TYPE_SCOPE]: the four public types that `Inequalities` yields and composes. `Inequality`, `MemberPath`, and `MemberPathSegment` are each a `readonly struct` that implements `IEquatable<T>`, `==`, and `!=`, and overrides `Equals(object?)`, `GetHashCode`, and `ToString`.

| [INDEX] | [SURFACE]                                                      | [KIND]   | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `new Inequality(MemberPath path, object? left, object? right)` | ctor     | one reported difference     |
|  [02]   | `Inequality.Path`                                              | property | `MemberPath` of the member  |
|  [03]   | `Inequality.Left`                                              | property | value on the left           |
|  [04]   | `Inequality.Right`                                             | property | value on the right          |
|  [05]   | `Inequality.ToString()`                                        | method   | rendered difference         |
|  [06]   | `new MemberPath(MemberPathSegment[] segments)`                 | ctor     | a path from its segments    |
|  [07]   | `MemberPath.Segments`                                          | property | the ordered segments        |
|  [08]   | `MemberPath.Append(MemberPathSegment)`                         | method   | one longer path             |
|  [09]   | `MemberPath.Append(MemberPath)`                                | method   | the concatenated path       |
|  [10]   | `MemberPath.ToString()`                                        | method   | rendered path               |
|  [11]   | `MemberPathSegment.Property(string name)`                      | method   | a property step             |
|  [12]   | `MemberPathSegment.Field(string name)`                         | method   | a field step                |
|  [13]   | `MemberPathSegment.Index(int index)`                           | method   | a sequence position         |
|  [14]   | `MemberPathSegment.Key(object key)`                            | method   | a dictionary key            |
|  [15]   | `MemberPathSegment.Added()`                                    | method   | present on the right only   |
|  [16]   | `MemberPathSegment.Removed()`                                  | method   | present on the left only    |
|  [17]   | `MemberPathSegment.Kind`                                       | property | the `MemberPathSegmentKind` |
|  [18]   | `MemberPathSegment.Value`                                      | property | the name, index, or key     |
|  [19]   | `MemberPathSegment.ToString()`                                 | method   | rendered segment            |

Rows [11] through [16] are `static`. `MemberPathSegment` declares no public constructor, so those six factories are the only way to build a populated segment. It remains a struct, so `default(MemberPathSegment)` gives `Kind` of `Property` and a null `Value`, and its `ToString` returns null.

- `Inequality.ToString` renders `{Path}: {Left} → {Right}`, with U+2192 as the arrow and one space on each side of it.
- `default(MemberPath).Segments` returns an empty array, so a `default` path is a usable empty path. `Segments` returns the backing array itself rather than a copy, so a caller that mutates it mutates the path.
- `Append(MemberPathSegment)` always allocates. `Append(MemberPath)` tests this path first and returns the other operand when this path is empty, so the other operand also wins when both are empty. It returns this path when only the other is empty.
- `MemberPath` compares its segments element by element, so two paths built from separate arrays compare equal. `MemberPathSegment` compares `Kind` and `Value`, so `Index(1)` and `Key(1)` differ.

## [11]-[PATH_SEGMENTS]

[SEGMENT_SCOPE]: the members of `MemberPathSegmentKind`, in declaration order, and the text each segment renders.

| [INDEX] | [KIND]     | [SEGMENT_TEXT]       |
| :-----: | :--------- | :------------------- |
|  [01]   | `Property` | the name             |
|  [02]   | `Field`    | the name             |
|  [03]   | `Index`    | `[2]`                |
|  [04]   | `Key`      | `["home"]` or `[42]` |
|  [05]   | `Added`    | `[+]`                |
|  [06]   | `Removed`  | `[-]`                |

- `MemberPath.ToString` writes a `.` before a `Property` or `Field` segment at any position after the first, and writes every other segment with no separator. An empty path renders as the empty string. A path therefore reads `Addresses["home"].Street`, `Layers[2]`, or `Tags[+]`.
- A `Key` segment quotes a `string` key inside the brackets and writes any other key unquoted.
- `MemberPathSegmentKind` declares exactly these six members.

## [12]-[DIAGNOSTICS]

[DIAGNOSTIC_SCOPE]: every diagnostic the analyzer reports. The category is `Generator.Equals`. Each is enabled by default and each accepts `dotnet_diagnostic.GE0##.severity` in an `.editorconfig` file. The analyzer skips generated code.

| [INDEX] | [ID]  | [SEVERITY] | [TRIGGER]                                                               |
| :-----: | :---- | :--------- | :---------------------------------------------------------------------- |
|  [01]   | GE001 | warning    | a collection member carries no collection equality attribute            |
|  [02]   | GE002 | info       | a member has a complex type that lacks `[Equatable]`                    |
|  [03]   | GE003 | info       | a collection element type is complex and lacks `[Equatable]`            |
|  [04]   | GE004 | warning    | an equality attribute sits in a type that lacks `[Equatable]`           |
|  [05]   | GE005 | warning    | an `[Equatable]` type overrides `Equals` or `GetHashCode`               |
|  [06]   | GE006 | error      | an `[Equatable]` type is not declared `partial`                         |
|  [07]   | GE007 | error      | one member carries two conflicting equality attributes                  |
|  [08]   | GE008 | error      | `[StringEquality]` sits on a member that is not a `string`              |
|  [09]   | GE009 | error      | a collection equality attribute sits on a non-collection member         |
|  [10]   | GE010 | error      | `[PrecisionEquality]` sits on a type outside the supported set          |
|  [11]   | GE011 | warning    | `GenerateClassEqualityOperators` is set on a record or a struct         |
|  [12]   | GE012 | warning    | a base type's operators defeat `GenerateClassEqualityOperators = false` |

- `[PrecisionEquality]` supports `float`, `double`, `decimal`, `int`, `long`, `short`, `sbyte`, and the nullable form of each. Every other type reports `GE010`, whether or not it is numeric.
- GE001, GE002, and GE003 are not reported for a member that carries `[IgnoreEquality]` or `[ReferenceEquality]`, and not reported under `Explicit = true` for a member that lacks `[DefaultEquality]`.
- GE001 is also not reported when the member type carries `[Equatable]`, when the member type nests a sealed `EqualityComparer` implementing `IEqualityComparer<>` of itself, or when the member carries `[DefaultEquality]` outside `Explicit` mode.
- GE002 and GE003 are also not reported for a member that carries `[DefaultEquality]`. Only a class or a struct is ever complex. A primitive, a well-known `System` type, an interface, a collection, a type that implements `IEquatable<T>` in a referenced assembly, and a record or struct whose every member is itself deeply value-equatable all count as non-complex.
- GE004 supersedes GE007 through GE010: when the containing type lacks `[Equatable]`, the analyzer reports GE004 and stops.
- GE005 reads only a `public override bool Equals(object)` and a `public override int GetHashCode()`. It is not reported on a record.
- GE007 pairs `[IgnoreEquality]` with any other attribute, and pairs any two equality attributes other than `[DefaultEquality]`. It reads only the attributes declared on the member itself, not the ones inherited through an override.
- GE011 and GE012 are reported only when `GenerateClassEqualityOperators` is present as an explicit argument. GE012 additionally requires the value `false` and an ancestor that supplies applicable operators.

## [13]-[CODE_FIXES]

[FIX_SCOPE]: every code fix the analyzer package registers, and the diagnostics each one repairs. GE007 through GE012 have no code fix.

| [INDEX] | [CODE_FIX]                              | [FIXES]                           |
| :-----: | :-------------------------------------- | :-------------------------------- |
|  [01]   | `AddCollectionEqualityAttributeCodeFix` | GE001                             |
|  [02]   | `AddDefaultEqualityCodeFix`             | GE002, GE003                      |
|  [03]   | `AddEquatableAttributeCodeFix`          | GE004                             |
|  [04]   | `MakeTypePartialCodeFix`                | GE006                             |
|  [05]   | `SuppressWithAttributeCodeFix`          | GE001, GE002, GE003, GE004, GE005 |

- `AddCollectionEqualityAttributeCodeFix` offers all three collection attributes and marks one as recommended: `[SetEquality]` for a type implementing `ISet<T>`, `[UnorderedEquality]` for a type implementing `IDictionary<TKey, TValue>`, and `[OrderedEquality]` otherwise, a type implementing only `IReadOnlyDictionary<TKey, TValue>` included. It offers nothing on a field, because it requires a property declaration.
- `AddEquatableAttributeCodeFix` adds `partial` to the type as well when the declaration lacks it.
- `SuppressWithAttributeCodeFix` inserts `[System.Diagnostics.CodeAnalysis.SuppressMessage("Generator.Equals", "GE0##:<title>")]` on the enclosing member declaration.
- Every fix except `SuppressWithAttributeCodeFix` supports fix-all.
