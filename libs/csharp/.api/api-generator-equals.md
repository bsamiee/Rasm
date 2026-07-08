# [RASM_API_GENERATOR_EQUALS]

`Generator.Equals` is a compile-time C# source generator that implements value equality — `IEquatable<T>`, `Equals`/`GetHashCode`, `==`/`!=`, and a structured member-level diff — from attributes alone, with zero reflection or runtime IL. Marking a `partial` class/struct/record/record-struct with `[Equatable]` emits a nested `EqualityComparer` whose `Default` instance performs deep structural comparison, and whose `Inequalities(x, y)` enumerates exactly which members differ with full member paths into nested objects, collection indices, and dictionary keys. Per-member attributes select collection-aware comparers (`[OrderedEquality]`, `[UnorderedEquality]`, `[SetEquality]`), reference identity, string-culture, floating-point tolerance, ignore, or a custom comparer. It is the seam's node/edge structural-equality and structured-diff substrate: `ElementGraph` `Node`/`Relationship` cases get correct deep equality for free, and `Inequalities()` feeds the Persistence `StructuralMerge` 3-way graph diff. It composes with — never duplicates — the kernel `XxHash128` content-addressing rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Generator.Equals`
- package: `Generator.Equals`
- asset: analyzer-only package at `analyzers/dotnet/cs/Generator.Equals.dll`; Assay resolves zero public runtime types for this package key
- runtime surface: transitive `Generator.Equals.Runtime`, assembly `Generator.Equals.Runtime`, namespace `Generator.Equals`
- owners: `Rasm.Element`, `Rasm.Bim`, `Rasm.Persistence`
- requirement: consumer `LangVersion` ≥ (records); applies to classes, structs, records, record structs
- rail: equality

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: marker + per-member configuration attributes (`[Conditional("GENERATOR_EQUALS")]` — read from syntax at compile time, NOT emitted into consumer IL)
- rail: equality

| [INDEX] | [SYMBOL] | [TARGET] | [CAPABILITY] |
|:-----: |:-------------------------- |:--------------- |:-------------------------------------------------------------------------- |
| [01] | `EquatableAttribute` | class / struct | the type marker; `Explicit` (opt-in members only) + `IgnoreInheritedMembers` |
| [02] | `DefaultEqualityAttribute` | property / field | `EqualityComparer<T>.Default` member equality; required to opt a field in, or any member under `Explicit` |
| [03] | `IgnoreEqualityAttribute` | property / field | exclude the member from `Equals`, `GetHashCode`, and `Inequalities` |
| [04] | `OrderedEqualityAttribute` | property / field | sequence equality (`SequenceEqual`) over an `IEnumerable<T>` member |
| [05] | `UnorderedEqualityAttribute` | property / field | multiset equality (order-independent, multiplicity-aware); dictionary-aware |
| [06] | `SetEqualityAttribute` | property / field | set equality (`ISet<T>.SetEquals` fast path; hashing always 0) |
| [07] | `ReferenceEqualityAttribute` | property / field | reference identity (`RuntimeHelpers.GetHashCode`); member type must be a class |
| [08] | `StringEqualityAttribute` | property / field | `StringComparer.<ComparisonType>` culture/ordinal equality |
| [09] | `PrecisionEqualityAttribute` | property / field | tolerance equality `Math.Abs(a-b) < precision`; excluded from `GetHashCode` |
| [10] | `CustomEqualityAttribute` | property / field | a caller-supplied `IEqualityComparer<T>` selected by type + member name |

[PUBLIC_TYPE_SCOPE]: runtime comparer helpers (in `Generator.Equals`; the generated code binds these — each is callable directly)
- rail: equality

| [INDEX] | [SYMBOL] | [IMPLEMENTS] | [CAPABILITY] |
|:-----: |:---------------------------------------- |:------------------------------------ |:------------------------------------------------------------------ |
| [01] | `DefaultEqualityComparer<T>` | `IEqualityComparer<T>` | `Default`; sealed-`T` fast path to `EqualityComparer<T>.Default`, else `object.Equals` (deep-equality-safe) |
| [02] | `OrderedEqualityComparer<T>` | `IEqualityComparer<IEnumerable<T>>` | `Default` + element-comparer ctor; `SequenceEqual` with order |
| [03] | `UnorderedEqualityComparer<T>` | `IEqualityComparer<IEnumerable<T>>` | `Default` + element-comparer ctor; multiplicity-aware multiset compare |
| [04] | `SetEqualityComparer<T>` | `IEqualityComparer<IEnumerable<T>>` | `Default` + element-comparer ctor; `ISet<T>.SetEquals` fast path; `GetHashCode` returns 0 |
| [05] | `DictionaryEqualityComparer<TKey,TValue>` | `IEqualityComparer<IDictionary<TKey,TValue>>` | `Default` + key/value-comparer ctor; order-independent entry compare |
| [06] | `ReferenceEqualityComparer<T>` | `IEqualityComparer<T>` (`where T:class`) | `Default`; identity equality via `RuntimeHelpers.GetHashCode` |

[PUBLIC_TYPE_SCOPE]: structured-diff value family (the `Inequalities()` output model)
- rail: equality

| [INDEX] | [SYMBOL] | [SHAPE] | [CAPABILITY] |
|:-----: |:------------------------ |:----------------- |:---------------------------------------------------------------------- |
| [01] | `Inequality` | `readonly struct` | one differing member: `Path` (`MemberPath`), `Left`/`Right` (`object?`); `ToString` → `"{Path}: {Left} → {Right}"` |
| [02] | `MemberPath` | `readonly struct` | ordered `Segments` (`MemberPathSegment[]`); immutable `Append(segment)` / `Append(path)`; dotted/bracketed `ToString` |
| [03] | `MemberPathSegment` | `readonly struct` | one path step: `Kind` + `Value` (`object?`); private ctor, static factories per kind |
| [04] | `MemberPathSegmentKind` | `enum` | `Property`, `Field`, `Index`, `Key`, `Added`, `Removed` |

[PUBLIC_TYPE_SCOPE]: generated per-`[Equatable]`-type surface (emitted into the partial type as `<Type>.Generator.Equals.g.cs`)
- rail: equality

| [INDEX] | [SYMBOL] | [CAPABILITY] |
|:-----: |:-------------------------------------- |:-------------------------------------------------------------------------- |
| [01] | nested `EqualityComparer: IEqualityComparer<TSelf>` | the comparer type; static `Default` instance — the canonical entrypoint |
| [02] | `EqualityComparer.Equals(x, y)` / `GetHashCode(obj)` | deep structural equality + hash per the member attributes |
| [03] | `EqualityComparer.Inequalities(x, y)` + `(x, y, MemberPath path)` | `IEnumerable<Inequality>` of differing members, drilling into nested `[Equatable]` elements |
| [04] | `Equals(object?)` / `IEquatable<TSelf>.Equals(TSelf)` / `GetHashCode()` | type-level overrides (classes implement `IEquatable<T>` explicitly for deep equality) |
| [05] | `operator ==` / `operator !=` | value-equality operators routed through the generated comparer |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: authoring an equatable owner
- rail: equality
- surface-root: the `partial` type carrying `[Equatable]`

| [INDEX] | [SURFACE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------- |:------------------------------------------------ |
| [01] | `[Equatable] partial record T(...)` | deep value equality over every member |
| [02] | `[Equatable(Explicit = true)] partial class T` | only `[DefaultEquality]`/attributed members count |
| [03] | `[Equatable(IgnoreInheritedMembers = true)] partial class T: B` | skip `base.Equals()`; compare only declared members |
| [04] | `[property: OrderedEquality] T[] Items` (record positional) | per-member comparer on a positional record parameter |
| [05] | `[DefaultEquality] private int _field;` | opt a field into comparison (fields excluded by default) |
| [06] | `[OrderedEquality(StringComparison.OrdinalIgnoreCase)]` | culture/ordinal element comparison for a string collection |
| [07] | `[OrderedEquality(typeof(MyComparer))]` / `(typeof(C), nameof(C.Member))` | element comparer by type + static `Default`/named member |
| [08] | `[CustomEquality(typeof(C))]` / `(typeof(C), nameof(C.Member))` | whole-member comparer by static instance, named member, or parameterless ctor |

[ENTRYPOINT_SCOPE]: comparing, hashing, and diffing through the generated comparer
- rail: equality

| [INDEX] | [SURFACE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------- |:------------------------------------------------ |
| [01] | `T.EqualityComparer.Default.Equals(a, b)` | deep structural equality (the canonical compare) |
| [02] | `a == b` / `a != b` / `a.Equals(b)` | operator + `IEquatable<T>` routes to the same comparer |
| [03] | `T.EqualityComparer.Default.GetHashCode(a)` | structural hash (collection-aware, tolerance/set members excluded) |
| [04] | `T.EqualityComparer.Default.Inequalities(a, b)` | `IEnumerable<Inequality>` of exactly the differing members |
| [05] | `T.EqualityComparer.Default.Inequalities(a, b, basePath)` | same, with every reported `Path` prefixed by `basePath` |
| [06] | `new MemberPath(new[]{ MemberPathSegment.Property("Home") })` | construct a base path for composed parent-context diffs |

[ENTRYPOINT_SCOPE]: direct comparer + path construction (composing equality without a generated owner)
- rail: equality

| [INDEX] | [SURFACE] | [CAPABILITY] |
|:-----: |:---------------------------------------------------------------- |:-------------------------------------------- |
| [01] | `OrderedEqualityComparer<T>.Default` / `new OrderedEqualityComparer<T>(inner)` | sequence comparer, default or element-customized |
| [02] | `UnorderedEqualityComparer<T>.Default` / `new UnorderedEqualityComparer<T>(inner)` | multiset comparer (typed `IEqualityComparer<IEnumerable<T>>`) |
| [03] | `SetEqualityComparer<T>.Default` / `new SetEqualityComparer<T>(inner)` | set comparer (`SetEquals` fast path) |
| [04] | `DictionaryEqualityComparer<TKey,TValue>.Default` / `new DictionaryEqualityComparer<TKey,TValue>(keyCmp, valCmp)` | dictionary comparer with `KeyEqualityComparer`/`ValueEqualityComparer` |
| [05] | `DefaultEqualityComparer<T>.Default` / `ReferenceEqualityComparer<T>.Default` | scalar default / identity comparers |
| [06] | `MemberPathSegment.Property(name)` / `Field(name)` / `Index(i)` / `Key(k)` / `Added()` / `Removed()` | one path segment per kind |
| [07] | `path.Append(segment)` / `path.Append(otherPath)` | immutable path extension (copy-on-append) |

## [04]-[INTEGRATION_STACKING]

[NODE_EDGE_EQUALITY]: the seam's structural-equality substrate.
- The `ElementGraph` `Node` `[Union]` (cases `Object`/`Material`/`PropertySet`/`QuantitySet`/`Assessment`/`Appearance`/`Coverage`) and the `Relationship` edge `[Union]` are CLASS-root unions carrying `[Equatable]` (the `[GRAPH_FAMILY]` form): a class-root union surrenders Thinktecture's record-root generated equality, so `[Equatable]` is the sole equality owner — never stacked on a record-root union. The collection-valued members several cases carry (property bags, layer/profile/constituent sets, the classification set, the `Generic` edge attribute map) take `[UnorderedEquality]`/`[OrderedEquality]`/`[SetEquality]` to model the order-insensitive bag/set semantics IFC Psets and material composition require, and `[Equatable]` gives each case correct deep equality without a hand-written `Equals`/`GetHashCode`. This replaces the rejected hand-rolled per-case equality; one attribute owns the whole member set. The same `[Equatable]` rule propagates ONLY to the INTERMEDIATE payload owners the diff must descend THROUGH — `MaterialComposition`/`MaterialPropertySet` (class-root `[Union]`+`[Equatable]`), `MaterialLayer`/`MaterialConstituent`/`SectionProperties` (`[Equatable]` record-structs), the `ValueBag<V>` `[Equatable]` record the `PropertyBag`/`QuantityBag` aliases close over, `MaterialUsage` (class-root `[Union]`+`[Equatable]`), `AssessmentPayload`, `CoverageGrid` — so a member-path diff descends one structural-equality link per hop to the merge leaf rather than stopping at the node member. The drill then BOTTOMS at the atomic value-equality LEAVES `PropertyValue` (a RECORD-root `[Union]` whose Thinktecture record-generated value equality is correct — the leaf at `Nodes[id].Bag.Values[name]`, an IFC Pset value the merge replaces wholesale, never sub-merged) and `MeasureValue` (a native-equality `readonly record struct` — the leaf at `Nodes[id].Properties[i].<column>`): both carry NEITHER `[Equatable]` (it does redundantly re-derive the field compare the record already gives) NOR a deeper descent, because they ARE the merge leaves, not drill owners — adding `[Equatable]` to a leaf is the rejected redundant-ceremony form.
- Thinktecture `[Union]`/`[ValueObject]` owners generate their OWN equality from the key projection; Generator.Equals is reserved for the rich multi-member graph owners where equality is structural over many collection members, not key-derived. Choose the owner: a single-key value object → Thinktecture `[ValueObject<T>]`; a multi-member structural class-root union or record with collection members → `[Equatable]`. Never stack both on one type (a record-root `[Union]` keeps Thinktecture's equality and never also carries `[Equatable]`; a class-root `[Union]` carries `[Equatable]` alone).

[STRUCTURED_DIFF]: `Inequalities` is the member-level diff feeding Persistence version control.
- The Persistence `Version/merge.md` 3-way `StructuralMerge` and `TimeTravel` blame/bisect need to know exactly which members changed between two graph snapshots. `T.EqualityComparer.Default.Inequalities(before, after)` yields `Inequality` rows whose `MemberPath` (`Addresses["home"].Street`, `Layers[2]`, `Constituents[+]`) localizes the change, and whose `Left`/`Right` carry the old/new values. Nested `[Equatable]` elements drill down automatically (a changed `IfcMaterialLayer` reports `Layers[2].Thickness`, not the whole layer), so the merge operates at member granularity, not whole-node replacement.
- `MemberPathSegmentKind` distinguishes ordered (`Index`), keyed (`Key`), and set membership (`Added`/`Removed`) deltas — exactly the change kinds a structural graph merge must reconcile. `MemberPath.Append(basePath)` composes a child diff under a parent context so a per-node diff nests into a whole-graph change report.

[CONTENT_KEY_BOUNDARY]: structural equality complements, never replaces, the kernel content hash.
- `NodeId` content-keys and the diff `ContentBytes` derive from the kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes()` (the canonical IEEE-754/tolerance-quantized byte projection, §4-RT H7). Generator.Equals provides in-memory VALUE equality and the human-readable structured diff; the kernel hash provides the stable cross-runtime content address. They are aligned by sharing the same canonical member set, not by re-implementing each other — never hash through `GetHashCode` (process-salted, non-stable) nor content-address through `Inequalities`.

[COLLECTION_COMPARER_REUSE]: the runtime comparers are first-class outside generated code.
- `OrderedEqualityComparer<T>`/`UnorderedEqualityComparer<T>`/`SetEqualityComparer<T>`/`DictionaryEqualityComparer<TKey,TValue>` are usable directly anywhere an `IEqualityComparer<IEnumerable<T>>`/`IEqualityComparer<IDictionary<_,_>>` is needed (LINQ `Distinct`/`GroupBy`, `HashSet`, `ImmutableDictionary` keying), so the same multiset/set/sequence semantics used inside generated equality apply to ad-hoc graph-membership checks without a second comparer family. The element-comparer ctor nests comparers (`new OrderedEqualityComparer<Layer>(Layer.EqualityComparer.Default)`) to compose deep equality over a collection of `[Equatable]` elements.

## [05]-[IMPLEMENTATION_LAW]

[EQUALITY_TOPOLOGY]:
- namespace: `Generator.Equals` (one namespace for attributes, comparers, and the diff family)
- generation: the `EqualsGenerator` `IIncrementalGenerator` matches `Generator.Equals.EquatableAttribute` via `ForAttributeWithMetadataName`, transforms class/record/struct/record-struct declarations, and emits one `<FullName>.Generator.Equals.g.cs` partial per type; there is no post-initialization attribute injection — the attribute definitions live in `Generator.Equals.Runtime`.
- conditional attributes: all attributes are `[Conditional("GENERATOR_EQUALS")]`; their usages are read from syntax and elided from the consumer's IL, so they impose no runtime metadata cost.
- field policy: fields are excluded from comparison unless annotated `[DefaultEquality]` (or another member attribute); properties are included by default outside `Explicit` mode.
- inheritance: with `IgnoreInheritedMembers = false`, the generator walks the WHOLE ancestor chain and calls `base.Equals()` if any ancestor has `[Equatable]` or a manual `Equals(object)` override; otherwise it compares all inherited properties. An overriding property inherits the base member's equality attribute (no re-declaration needed).
- hashing exclusions: `[PrecisionEquality]` and `[SetEquality]` members contribute nothing to `GetHashCode` (no stable bucketing under tolerance; set hashing returns 0) — equality and hashing intentionally diverge for these members.
- collection requirement: `[OrderedEquality]`/`[UnorderedEquality]`/`[SetEquality]` require the member to be `IEnumerable<T>` (dictionary for the dictionary path) with element-level equality; nest the element's own `[Equatable]` comparer for deep collection equality.

[LOCAL_ADMISSION]:
- A graph record that needs structural equality is `partial` and carries `[Equatable]`; equality is read through `T.EqualityComparer.Default`, never a hand-written `Equals`/`GetHashCode`.
- Collection members declare the intended semantics with `[OrderedEquality]`/`[UnorderedEquality]`/`[SetEquality]`; an unattributed collection compares by reference (a defect for value records) — annotate every value-bearing collection.
- Member-level diffs flow through `Inequalities()`; never diff by string-formatting two records and comparing text.
- Content addressing stays on the kernel `XxHash128` canonical-bytes rail; `GetHashCode` is for in-memory hashing only, never persisted or wire-compared.

[RAIL_LAW]:
- Package: `Generator.Equals` (+ runtime `Generator.Equals.Runtime`)
- Owns: compile-time generated structural value equality (`Equals`/`GetHashCode`/`==`/`IEquatable<T>`), the collection-aware comparer family (ordered/unordered/set/dictionary/reference/default), and the member-level structured diff (`Inequalities` → `Inequality`/`MemberPath`/`MemberPathSegment`).
- Accept: multi-member graph/record types whose equality is structural over many (often collection) members; ad-hoc reuse of the collection comparers; member-granular change detection for structural merge.
- Reject: hand-written `Equals`/`GetHashCode` on equatable owners; unattributed value-collection members; using `[Equatable]` where a Thinktecture key-projected `[ValueObject]`/`[Union]` already owns equality; routing content-addressing or wire equality through `GetHashCode`/`Inequalities` instead of the kernel `XxHash128` canonical-bytes rail.
