# [RASM_API_GENERATOR_EQUALS]

`Generator.Equals` derives structural equality and member-level difference receipts for attributed partial C# types at compile time, without reflection or IL injection. Attribute policy and the generated `EqualityComparer` bind every admitted member to one equality rail with path-aware difference projection.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Generator.Equals`
- package: `Generator.Equals`
- asset: analyzer-only package at `analyzers/dotnet/cs/Generator.Equals.dll`; Assay resolves zero public runtime types for this package key
- runtime surface: transitive `Generator.Equals.Runtime`, assembly `Generator.Equals.Runtime`, namespace `Generator.Equals`
- owners: `Rasm.Element`, `Rasm.Bim`, `Rasm.Persistence`, `Rasm.Fabrication`
- requirement: .NET Standard 2.0-compatible target and C# `LangVersion` 9.0 or higher
- rail: equality

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `EquatableAttribute` marks classes and structs; every configuration attribute carries `[Conditional("GENERATOR_EQUALS")]`, so the compiler reads its syntax and elides it from consumer IL.
- rail: equality

| [INDEX] | [SYMBOL]                     | [SCOPE] | [CAPABILITY]                  |
| :-----: | :--------------------------- | :------ | :---------------------------- |
|  [01]   | `EquatableAttribute`         | type    | structural-equality admission |
|  [02]   | `DefaultEqualityAttribute`   | member  | default-comparer selection    |
|  [03]   | `IgnoreEqualityAttribute`    | member  | equality exclusion            |
|  [04]   | `OrderedEqualityAttribute`   | member  | ordered sequence equality     |
|  [05]   | `UnorderedEqualityAttribute` | member  | multiplicity-aware equality   |
|  [06]   | `SetEqualityAttribute`       | member  | set equality                  |
|  [07]   | `ReferenceEqualityAttribute` | member  | reference identity            |
|  [08]   | `StringEqualityAttribute`    | member  | selected string comparison    |
|  [09]   | `PrecisionEqualityAttribute` | member  | numeric tolerance             |
|  [10]   | `CustomEqualityAttribute`    | member  | custom-comparer selection     |

[ATTRIBUTE_POLICY]:
- `EquatableAttribute.Explicit` includes only attributed members.
- `EquatableAttribute.IgnoreInheritedMembers` excludes ancestor members.
- `DefaultEqualityAttribute` opts fields and `Explicit`-mode members into `DefaultEqualityComparer<T>.Default`.
- `IgnoreEqualityAttribute` excludes a member from `Equals`, `GetHashCode`, and `Inequalities`.
- `OrderedEqualityAttribute` applies `SequenceEqual` to `IEnumerable<T>`.
- `UnorderedEqualityAttribute` applies order-independent, multiplicity-aware comparison and recognizes dictionaries.
- `SetEqualityAttribute` applies set equality and contributes zero to hashing.
- `ReferenceEqualityAttribute` requires a reference-type member and hashes its identity through `RuntimeHelpers.GetHashCode`.
- `StringEqualityAttribute` selects `StringComparer.<ComparisonType>`.
- `PrecisionEqualityAttribute` applies `Math.Abs(a - b) < precision` to admitted numeric types and contributes nothing to `GetHashCode`.
- `CustomEqualityAttribute` selects an `IEqualityComparer<T>` through a named static member or a parameterless comparer constructor.

[PUBLIC_TYPE_SCOPE]: every runtime helper in `Generator.Equals` implements `IEqualityComparer<TInput>` and exposes `Default`; collection helpers also accept comparer constructors, and `ReferenceEqualityComparer<T>` requires `where T : class`.
- rail: equality

| [INDEX] | [SYMBOL]                                   | [INPUT]                     | [SEMANTICS]                  |
| :-----: | :----------------------------------------- | :-------------------------- | :--------------------------- |
|  [01]   | `DefaultEqualityComparer<T>`               | `T`                         | type-default equality        |
|  [02]   | `OrderedEqualityComparer<T>`               | `IEnumerable<T>`            | ordered sequence             |
|  [03]   | `UnorderedEqualityComparer<T>`             | `IEnumerable<T>`            | multiplicity-aware multiset  |
|  [04]   | `SetEqualityComparer<T>`                   | `IEnumerable<T>`            | mathematical set             |
|  [05]   | `DictionaryEqualityComparer<TKey, TValue>` | `IDictionary<TKey, TValue>` | order-independent dictionary |
|  [06]   | `ReferenceEqualityComparer<T>`             | `T`                         | reference identity           |

[COMPARER_POLICY]:
- `DefaultEqualityComparer<T>` routes sealed types through `EqualityComparer<T>.Default` and other types through `object.Equals`.
- `OrderedEqualityComparer<T>` applies `SequenceEqual`.
- `UnorderedEqualityComparer<T>` preserves multiplicity during equality and folds element hashes through order-independent XOR.
- `SetEqualityComparer<T>` takes the `ISet<T>.SetEquals` fast path under the default element comparer and returns zero from `GetHashCode`.
- `DictionaryEqualityComparer<TKey, TValue>` exposes `KeyEqualityComparer` and `ValueEqualityComparer`; its constructors accept independent key and value comparers. Dictionary lookup governs key matching, while the configured key comparer contributes to entry hashing.
- `ReferenceEqualityComparer<T>` hashes identity through `RuntimeHelpers.GetHashCode`.

[PUBLIC_TYPE_SCOPE]: `Inequality`, `MemberPath`, and `MemberPathSegment` are `readonly struct` values; `MemberPathSegmentKind` is their path-step vocabulary.
- rail: equality

| [INDEX] | [SYMBOL]                | [ROLE]                   |
| :-----: | :---------------------- | :----------------------- |
|  [01]   | `Inequality`            | differing-member receipt |
|  [02]   | `MemberPath`            | ordered segment path     |
|  [03]   | `MemberPathSegment`     | kinded path step         |
|  [04]   | `MemberPathSegmentKind` | path-step vocabulary     |

[`Inequality`]:

- Shape: `Path` is a `MemberPath`; `Left` and `Right` are `object?`
- Format: `ToString()` returns `"{Path}: {Left} → {Right}"`

[`MemberPath`]:

- Shape: `Segments` is `MemberPathSegment[]`
- Operations: `Append(MemberPathSegment segment)` and `Append(MemberPath other)` return extended paths without mutating either input
- Format: `ToString()` renders dotted and bracketed segments

[`MemberPathSegment`]:

- Shape: `Kind` plus `Value` (`object?`)
- Construction: a private constructor plus one static factory per kind

[`MemberPathSegmentKind`]:

- Values: `Property`, `Field`, `Index`, `Key`, `Added`, and `Removed`

[PUBLIC_TYPE_SCOPE]: an `[Equatable]` partial type exposes the compiled equality surface below; the generator emits or replaces the applicable members for its declaration kind. Nested-comparer parameters use `TArg`: `TSelf?` for reference types and `TSelf` for value types; `partial class` declarations implement `IEquatable<TSelf>` explicitly, while record and struct declarations expose typed equality publicly.
- rail: equality

| [INDEX] | [SYMBOL]                                                                                           | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | `EqualityComparer : IEqualityComparer<TSelf>`                                                      | nested comparer owner            |
|  [02]   | `EqualityComparer.Default`                                                                         | canonical entrypoint             |
|  [03]   | `bool EqualityComparer.Equals(TArg x, TArg y)`                                                     | deep structural equality         |
|  [04]   | `int EqualityComparer.GetHashCode(TSelf obj)`                                                      | member-policy hash               |
|  [05]   | `IEnumerable<Inequality> EqualityComparer.Inequalities(TArg x, TArg y, MemberPath path = default)` | member diff with optional prefix |
|  [06]   | `Equals(object?)`                                                                                  | object equality override         |
|  [07]   | `IEquatable<TSelf>.Equals(TArg)`                                                                   | typed equality                   |
|  [08]   | `GetHashCode()`                                                                                    | structural hash override         |
|  [09]   | `operator ==`                                                                                      | value-equality operator          |
|  [10]   | `operator !=`                                                                                      | value-inequality operator        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: a `partial` type carries `[Equatable]`; collection-policy attributes and `[CustomEquality]` select a named static comparer or a parameterless comparer type.
- rail: equality
- surface-root: the `partial` type carrying `[Equatable]`

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]               |
| :-----: | :--------------------------------------------------------------- | :------------------------- |
|  [01]   | `[Equatable] partial record T(...)`                              | default member equality    |
|  [02]   | `[Equatable(Explicit = true)] partial class T`                   | attributed-member equality |
|  [03]   | `[Equatable(IgnoreInheritedMembers = true)] partial class T : B` | declared-member equality   |
|  [04]   | `[property: OrderedEquality] T[] Items`                          | positional-member policy   |
|  [05]   | `[DefaultEquality] private int _field;`                          | field inclusion            |
|  [06]   | `[OrderedEquality(StringComparison.OrdinalIgnoreCase)]`          | string-element comparison  |
|  [07]   | `[OrderedEquality(typeof(MyComparer))]`                          | comparer-type selection    |
|  [08]   | `[OrderedEquality(typeof(C), nameof(C.Member))]`                 | named comparer selection   |
|  [09]   | `[CustomEquality(typeof(C))]`                                    | custom comparer selection  |
|  [10]   | `[CustomEquality(typeof(C), nameof(C.Member))]`                  | named custom comparer      |

[ENTRYPOINT_SCOPE]: comparing, hashing, and diffing through the generated comparer
- rail: equality

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]         |
| :-----: | :------------------------------------------------------------- | :------------------- |
|  [01]   | `T.EqualityComparer.Default.Equals(a, b)`                      | canonical comparison |
|  [02]   | `a == b`                                                       | equality operator    |
|  [03]   | `a != b`                                                       | inequality operator  |
|  [04]   | `a.Equals(b)`                                                  | instance equality    |
|  [05]   | `T.EqualityComparer.Default.GetHashCode(a)`                    | structural hash      |
|  [06]   | `T.EqualityComparer.Default.Inequalities(a, b)`                | member diff          |
|  [07]   | `T.EqualityComparer.Default.Inequalities(a, b, basePath)`      | prefixed member diff |
|  [08]   | `new MemberPath(new[] { MemberPathSegment.Property("Home") })` | composed base path   |

[ENTRYPOINT_SCOPE]: direct comparers and path segments compose equality without a generated owner.
- rail: equality

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------- | :-------------------------- |
|  [01]   | `OrderedEqualityComparer<T>.Default`                           | default ordered comparer    |
|  [02]   | `new OrderedEqualityComparer<T>(inner)`                        | ordered element comparer    |
|  [03]   | `UnorderedEqualityComparer<T>.Default`                         | default multiset comparer   |
|  [04]   | `new UnorderedEqualityComparer<T>(inner)`                      | multiset element comparer   |
|  [05]   | `SetEqualityComparer<T>.Default`                               | default set comparer        |
|  [06]   | `new SetEqualityComparer<T>(inner)`                            | set element comparer        |
|  [07]   | `DictionaryEqualityComparer<TKey, TValue>.Default`             | default dictionary comparer |
|  [08]   | `new DictionaryEqualityComparer<TKey, TValue>(keyCmp, valCmp)` | two-comparer constructor    |
|  [09]   | `DefaultEqualityComparer<T>.Default`                           | scalar default comparer     |
|  [10]   | `ReferenceEqualityComparer<T>.Default`                         | identity comparer           |
|  [11]   | `MemberPathSegment.Property(name)`                             | property segment            |
|  [12]   | `MemberPathSegment.Field(name)`                                | field segment               |
|  [13]   | `MemberPathSegment.Index(i)`                                   | index segment               |
|  [14]   | `MemberPathSegment.Key(k)`                                     | key segment                 |
|  [15]   | `MemberPathSegment.Added()`                                    | addition segment            |
|  [16]   | `MemberPathSegment.Removed()`                                  | removal segment             |
|  [17]   | `path.Append(segment)`                                         | segment append              |
|  [18]   | `path.Append(otherPath)`                                       | path append                 |

## [04]-[INTEGRATION_STACKING]

[NODE_EDGE_EQUALITY]: the seam's structural-equality substrate.
- The `ElementGraph` `Node` class-root `[Union]` closes over `Object`, `Material`, `PropertySet`, `QuantitySet`, `Assessment`, `Appearance`, and `Coverage`; `Relationship` owns the edge union. Both carry `[Equatable]`, which owns equality across their complete member sets.
- Collection-valued members declare bag, sequence, or set semantics through `[UnorderedEquality]`, `[OrderedEquality]`, or `[SetEquality]`; these policies cover IFC property bags, material layer/profile/constituent sets, classification sets, and `Generic` edge attributes.
- Intermediate class-root payloads `MaterialComposition`, `MaterialPropertySet`, and `MaterialUsage` combine `[Union]` with `[Equatable]`.
- Intermediate payloads `MaterialLayer`, `MaterialConstituent`, `SectionProperties`, `ValueBag<V>`, `AssessmentPayload`, and `CoverageGrid` carry `[Equatable]`, so member paths descend one structural-equality hop per owner. `PropertyBag` and `QuantityBag` close over the `ValueBag<V>` equality owner.
- `PropertyValue` is the whole-value IFC Pset leaf at `Nodes[id].Bag.Values[name]`; merge replaces it wholesale. `MeasureValue` is the native-equality `readonly record struct` leaf at `Nodes[id].Properties[i].<column>`.
- Neither leaf carries `[Equatable]` or admits a deeper diff. A record-root `[Union]` retains Thinktecture-generated equality and carries no `[Equatable]`.
- Thinktecture `[Union]` and `[ValueObject<T>]` owners derive equality from their key projections; `[Equatable]` owns multi-member structural equality for class-root unions and records with collection members. One type selects one equality owner.

[STRUCTURED_DIFF]: `Inequalities` is the member-level diff feeding Persistence version control.
- `StructuralMerge` and `TimeTravel` consume `T.EqualityComparer.Default.Inequalities(before, after)` to isolate changed graph members.
- Each `Inequality` carries a `MemberPath`, such as `Addresses["home"].Street`, `Layers[2]`, or `Constituents[+]`, plus the old and new values in `Left` and `Right`.
- Nested `[Equatable]` collection elements and dictionary values descend to member granularity, so a changed `IfcMaterialLayer` reports `Layers[2].Thickness` instead of replacing the whole layer.
- `MemberPathSegmentKind` distinguishes ordered `Index`, keyed `Key`, and membership `Added`/`Removed` deltas. `basePath.Append(childPath)` nests a child diff beneath its parent context.

[CONTENT_KEY_BOUNDARY]: structural equality complements, never replaces, the kernel content hash.
- `NodeId` content keys and diff `ContentBytes` derive from the kernel seed-zero `XxHash128` over the canonical IEEE-754 and tolerance-quantized `Node.ToCanonicalBytes()` projection.
- Generator.Equals owns in-memory value equality and human-readable structured diffs; the kernel hash mints stable cross-runtime content addresses.
- Both rails consume the same canonical member set. `GetHashCode` remains process-salted in-memory state, and `Inequalities` remains a diff rail.

[COLLECTION_COMPARER_REUSE]: the runtime comparers are first-class outside generated code.
- `OrderedEqualityComparer<T>`, `UnorderedEqualityComparer<T>`, `SetEqualityComparer<T>`, and `DictionaryEqualityComparer<TKey, TValue>` compose directly with LINQ `Distinct`/`GroupBy`, `HashSet`, and `ImmutableDictionary` keying.
- Direct use preserves the sequence, multiset, set, and dictionary semantics generated equality applies to graph membership.
- Element-comparer constructors nest generated comparers; `new OrderedEqualityComparer<Layer>(Layer.EqualityComparer.Default)` composes deep equality over `[Equatable]` elements.

## [05]-[IMPLEMENTATION_LAW]

[EQUALITY_TOPOLOGY]:
- namespace: `Generator.Equals` (one namespace for attributes, comparers, and the diff family)
- generation: the `EqualsGenerator` `IIncrementalGenerator` matches `Generator.Equals.EquatableAttribute` via `ForAttributeWithMetadataName`, transforms class/record/struct/record-struct declarations, and emits one `<FullName>.Generator.Equals.g.cs` partial per type; there is no post-initialization attribute injection — the attribute definitions live in `Generator.Equals.Runtime`.
- conditional attributes: all attributes are `[Conditional("GENERATOR_EQUALS")]`; their usages are read from syntax and elided from the consumer's IL, so they impose no runtime metadata cost.
- field policy: fields are excluded from comparison unless annotated `[DefaultEquality]` (or another member attribute); properties are included by default outside `Explicit` mode.
- inheritance: with `IgnoreInheritedMembers = false`, the generator walks the WHOLE ancestor chain and calls `base.Equals()` if any ancestor has `[Equatable]` or a manual `Equals(object)` override; otherwise it compares all inherited properties. An overriding property inherits the base member's equality attribute (no re-declaration needed).
- hashing exclusions: `[PrecisionEquality]` members are omitted from `GetHashCode`; `[SetEquality]` members contribute a constant zero because set hashing has no order-stable bucket. Equality and hashing intentionally diverge for these members.
- collection requirement: `[OrderedEquality]`/`[UnorderedEquality]`/`[SetEquality]` require the member to be `IEnumerable<T>` (dictionary for the dictionary path) with element-level equality; nest the element's own `[Equatable]` comparer for deep collection equality.

[LOCAL_ADMISSION]:
- A graph record that needs structural equality is `partial` and carries `[Equatable]`; equality is read through `T.EqualityComparer.Default`, never a hand-written `Equals`/`GetHashCode`.
- Collection members declare the intended semantics with `[OrderedEquality]`/`[UnorderedEquality]`/`[SetEquality]`; an unattributed collection compares by reference (a defect for value records) — annotate every value-bearing collection.
- Member-level diffs flow through `Inequalities()`; never diff by string-formatting two records and comparing text.
- Content addressing stays on the kernel `XxHash128` canonical-bytes rail; `GetHashCode` is for in-memory hashing only, never persisted or wire-compared.

[RAIL_LAW]:
- Package: `Generator.Equals` (+ runtime `Generator.Equals.Runtime`)
- Owns: compile-time structural value equality (`Equals`/`GetHashCode`/`==`/`IEquatable<T>`), the collection-aware comparer family (ordered/unordered/set/dictionary/reference/default), and the member-level structured diff (`Inequalities` → `Inequality`/`MemberPath`/`MemberPathSegment`).
- Accept: multi-member graph/record types whose equality is structural over many (often collection) members; ad-hoc reuse of the collection comparers; member-granular change detection for structural merge.
- Reject: hand-written `Equals`/`GetHashCode` on equatable owners; unattributed value-collection members; using `[Equatable]` where a Thinktecture key-projected `[ValueObject]`/`[Union]` already owns equality; routing content-addressing or wire equality through `GetHashCode`/`Inequalities` instead of the kernel `XxHash128` canonical-bytes rail.
