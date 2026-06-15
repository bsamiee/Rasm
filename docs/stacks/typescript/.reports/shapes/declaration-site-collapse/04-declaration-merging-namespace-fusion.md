# Declaration-merging namespace fusion

[FUSION_THESIS]:
- The merge is the one lever that grows an owner's surface with ZERO new symbols: every other collapse in the chain fuses what one declaration emits, while the merge fuses what THREE declarations emit onto one name across two namespaces, so the symbol count an owner exposes is invariant under its surface growth — a richer static set, a wider instance type, and the original codec all resolve through the single name a consumer already imports. The factory declaration is the only symbol; the value-side `namespace X` and the type-side `interface X` are augmentations of a name that already exists, not new bindings, so the merge is the surface-growth move whose cost in the export surface is nothing.
- The fusion is the structural alternative to the companion object: the naive growth declares the owner, then a parallel `const XStatics = { seed, of, ... }` beside it for the value-side helpers and a parallel `interface XShape` beside it for the type-side extras — two new symbols a consumer must import and correlate, two names for one concept. The merge collapses both back onto the owner's name, so `X.seed`, `X.of`, and the widened `X` instance type all resolve to the one owner in one hop, the companion object and the mirror interface being the two parallel symbols the merge deletes.
- The merge is namespace-additive, never namespace-overwriting: a `namespace X` member sharing a name with a generated class static does not replace it — the namespace member merges into the static side and the merged static side is then checked to still extend the factory's base static side, so a name collision surfaces as a static-side incompatibility (the merged `make: number` failing to extend the inherited `make` function signature), not a silent override. The static surface a namespace may add is therefore the complement of the surface the factory already generated, and the empty slots differ per owner family.
- The two augmentation directions split on what the owner declaration could already carry: an instance member expressible as a getter or method belongs in the class body and never in a merged `interface` — the `interface X` merge is reserved for the type-only widening a value-bearing body cannot express (a phantom-typed property, a structurally-merged external instance shape, a declaration the runtime must not initialize), so a getter smuggled into the merged interface is the body-member misrouted to the type namespace, and a phantom field forced into the body is a value where only a type belongs.

[TWO_NAMESPACE_IDENTITY]:
- The class name is the one identifier that pre-occupies both namespaces, which is why the merge attaches to a class and not to a `const`-bound owner: `class X extends Schema.Class<X>('X')({ ... }) {}` binds `X` as a constructor value AND as the instance type in the same statement, so `namespace X` finds an existing value-side binding to augment and `interface X` finds an existing type-side binding to widen — a `const X = Schema.Struct({ ... })` binds only the value namespace, so a following `namespace X` is a fresh value binding (a redeclaration error against the const) and an `interface X` has no class instance type to merge into, collapsing the merge to companion-object territory.
- The value-side merge and the type-side merge are independent and either may be absent: an owner that only grows statics carries `class X` plus `namespace X` and no `interface X`, an owner that only widens its instance type carries `class X` plus `interface X` and no `namespace X`, and the rich owner carries all three — the three declarations are one fused owner precisely because they share a name across the two namespaces, not because they are written together.
- The merge keeps the static-side accretion typed against the owner: a `namespace X { export const of = (...): X => ... }` returns the owner's own instance type because `X` in return position resolves to the type namespace while `X` in `new X(...)` resolves to the value namespace — one name carrying both meanings inside the merged block is the fusion in miniature, the static factory naming its own result type without a separate alias.

```typescript
import { Schema } from 'effect'
class Span extends Schema.Class<Span>('Span')({
    lo: Schema.Number,
    hi: Schema.Number.pipe(Schema.greaterThanOrEqualTo(0)),
}) {
    get width(): number {
        return this.hi - this.lo
    }
}
namespace Span {
    export const unit = new Span({ lo: 0, hi: 1 })
    export const of = (lo: number, hi: number): Span => new Span({ lo, hi })
}
const grown = Span.of(2, 5)
```

[STATIC_COLLISION_SURFACE]:
- The names a `namespace X` may add are the complement of the factory's generated static set, and that set is a per-family fact the author reads off the factory return: `Schema.Class` generates `make`, `fields`, `extend`, `transformOrFail`, `transformOrFailFrom`, `annotations`, `ast`, and `identifier` on the value side, so a merged `namespace` member named `make` merges then fails the static-side extends check while `of`/`unit`/`seed`/`floor` land in free slots — the collision is exact-name and type-driven, so the author names a smart constructor anything outside the eight generated statics and the merge is clean.
- `Data.Class` and `Data.TaggedClass(tag)` return a bare constructor type `new <A>(args) => ...` with NO generated statics, so the merged namespace of a `Data`-family owner has the entire static surface free — `class Marker extends Data.TaggedClass('Marker')<{ ... }>() {}` plus `namespace Marker { export const seed = new Marker({ ... }) }` collides with nothing, the codec-free interior-identity owner being the family with the cleanest merge slot precisely because it pays for no factory statics it would otherwise reserve.
- `Effect.Service` is the family with the most occupied static surface and an explicit fence: the generated value side carries `Default` (or `DefaultWithoutDependencies` when dependencies are declared), `make`, `use`, `key`, and — when `accessors: true` — one `Tag.Proxy` static per service method, so a merged `namespace` member colliding with `Default`, `make`, `key`, or any accessor name is a duplicate-identifier error, and the `Service.AllowedType` fence additionally forbids those same names AS FIELDS on the service shape, so the collision is walled at both the static-merge seam and the shape-definition seam.
- The accessor surface makes the service collision surface VALUE-DEPENDENT: with `accessors: true` the generated statics are a mapped type over the service's own method keys (`Tag.Proxy<Self, MakeService<Make>>`), so adding a service method named `seed` retroactively occupies the `seed` static slot a merged namespace might have wanted — the free static names of an accessor-bearing service are the complement of its method set, a surface that shrinks as the service grows, so a merged namespace static is safest named for a concept disjoint from any method (`Live`, `Test`, `Mock` layer seeds rather than operation-shaped names).

[TAG_PROXY_ACCESSOR_FUSION]:
- The accessor generation is itself a declaration-site fusion the merge composes with: `accessors: true` projects each service method into a same-named static whose `R` channel adds `Self`, so `class Cache extends Effect.Service<Cache>()('Cache', { accessors: true, sync: () => ({ read: (k: string) => 0 }) }) {}` yields a `Cache.read(k)` static returning `Effect<number, never, Cache>` — the static accessor surface grows from the make-record's shape with no second declaration, the same one-name-two-uses fusion the namespace merge performs, here driven by the factory rather than the author.
- The author's `namespace` merge and the factory's accessor projection are two writers into one static namespace, and they coexist only on disjoint names: the accessor projection owns every name in the service's method set, the merged namespace owns names outside it, so the layer seeds and test doubles an author attaches via `namespace Cache { export const Test = Layer.succeed(Cache, ...) }` ride beside the auto-generated `Cache.read`/`Cache.write` accessors without collision — one static surface, two contributing declarations, partitioned by the method-key boundary.
- The fence `Service.AllowedType` forbids the names `Default`, `make`, `key`, `of`, `use`, `context`, `pipe`, `_tag` as FIELDS of the service shape so the accessor projection can never overwrite a generated static with a method-derived one — a service method named `make` would project a `make` accessor static colliding with the generated `make`, so the shape fence rejects the field before the static collision can form, the fence the static-merge seam relies on to keep its complement well-defined.

[SELF_SEEDING_AND_TDZ]:
- A namespace static that seeds a canonical instance reads the owner's value-side binding eagerly when the namespace block evaluates — `namespace X { export const base = new X({ ... }) }` runs `new X(...)` at module-init — so the eager-heritage ordering law extends to the merge: the namespace block must follow the class declaration line, because the class value materializes at its eager heritage and the namespace seed reads that value, and a `namespace X` placed above its `class X` line seeds against an uninitialized binding and throws TDZ at the seed's evaluation, not at a type check.
- The merge is order-free in one namespace and order-bound in the other, and only the seed reveals it: a `namespace X` with NO value statics (a pure type-grouping block) merges regardless of source position because the type checker resolves names globally, but a `namespace X` whose member runs `new X(...)` is a runtime read that fails before any type check if it precedes the class value's construction — so the merge inherits the checker-symmetric/evaluation-asymmetric split exactly at the seed, and an owner whose namespace holds only type members tolerates an inverted order that the same owner with one seed instance does not.
- The seed instance is a value-after-value link ONLY if it restates a value the owner already produces — a canonical zero, identity, or sentinel instance is a genuine new value the owner does not otherwise carry and belongs as a namespace seed, while a seed that re-wraps a value the decode boundary already yields (`namespace X { export const fromInput = new X(decodedParts) }` beside a `Schema.decodeUnknown(X)` that produced `decodedParts`) is the instantiation-after-decode link, dead because the decode already minted the instance.
- The seed is a validating construction at the merge site: `new X({ ... })` inside the namespace runs the owner's field refinements, so a seed violating a `Schema.between`/`Schema.nonEmpty` filter is a construction throw at module-init — the canonical instance cannot be minted invalid, and where the seed's parts are upstream-proven the `disableValidation` option rides the namespace `new` exactly as it rides any other construction, the merge inheriting the producer's validation contract unchanged.

```typescript
import { Data } from 'effect'
class Vec extends Data.Class<{ readonly x: number; readonly y: number }> {
    get norm(): number {
        return Math.hypot(this.x, this.y)
    }
}
namespace Vec {
    export const zero = new Vec({ x: 0, y: 0 })
    export const of = (x: number, y: number): Vec => new Vec({ x, y })
}
const shifted = Vec.of(Vec.zero.x + 1, 2)
```

[INTERFACE_TYPE_SIDE_WIDENING]:
- The `interface X` merge widens the instance type the class already declares, and it earns its place only where a class body cannot express the member: a getter or method goes in the body (it is a value), so the merged interface carries the type-only widening — a phantom-branded property (`readonly [BrandSlot]: 'X'`), the structural shape of an external instance the runtime mixes in, or a member declared for the checker that the class must not initialize at runtime — and a body-expressible member routed into the merged interface is the misrouting the body-member rule rejects.
- The interface merge widens the type without touching the codec: a `Schema.Class` carries its decoded type as `Schema.Schema.Type<typeof X>`, and an `interface X` merge adds members to the instance type `X` that the SCHEMA does not know about, so the widened instance type and the codec's decoded type diverge — the interface merge is admitted only for a member the codec genuinely should not see (a computed-at-runtime field, a host-injected slot), and a member that should round-trip through decode belongs in the schema fields, never in a merged interface that the decoder cannot populate.
- The interface merge is the type-side counterpart of the namespace's value-side seed: the namespace adds value statics the factory left free, the interface adds type members the field set left unstated, and both attach to the one name across its two namespaces — an owner needing both a host-injected runtime slot (interface) and a canonical seed (namespace) carries `class X` plus `interface X` plus `namespace X`, three declarations fusing into one owner whose value statics, instance type, and codec all resolve through the single name.

[CODEC_FREE_FAMILY_HAS_NO_MERGE_TARGET]:
- `Data.taggedEnum<E>()` returns a value RECORD of tag-keyed constructors plus `$is`/`$match`, bound by destructuring — there is no class name occupying the two namespaces, so the static-augmentation merge has no target and the codec-free variant family grows its surface through the destructuring binding, not through namespace fusion: a new variant lands as a tag in the `Data.TaggedEnum` alias plus a key in the destructured record, never as a merged namespace static.
- The split is the merge lever's boundary: declaration merging is a CLASS-FAMILY collapse (`Schema.Class`, `Data.Class`, `Data.TaggedClass`, `Effect.Service`, `Effect.Tag` all bind a class name into both namespaces and admit the merge), while the destructured-constructor family (`Data.taggedEnum`) is a VALUE-RECORD collapse whose growth axis is the record's key set — so an owner that needs author-attached statics (seeds, smart constructors, sibling layers) must be a class-family owner, and reaching for `Data.taggedEnum` and then a parallel `const EnumStatics = { ... }` beside it is the companion object the class-family merge would have absorbed.
- The selection law fuses with the boundary law: a variant family that must admit from `unknown` is a `Schema.Union` of `Schema.TaggedClass` owners — each a mergeable class — while a codec-free interior family is a `Data.taggedEnum` record with no merge target, so the same decision that chooses codec-vs-no-codec chooses mergeable-class-vs-destructured-record, and an interior family that later needs an author-attached seed is re-shaped to a class owner, the merge requirement retroactively forcing the family form.

[DUAL_EXPORT_UNDER_VERBATIM]:
- Under `verbatimModuleSyntax` the value `export { X }` carries the class across both namespaces — the value-side constructor with its merged statics AND the type-side instance type with its merged interface members travel through one specifier, so a class-family owner exports value, type, statics, and widened instance type in one `export { X }` with no `export type { X }` beside it, and a `type X = typeof X` minted only to re-export the type is the redundant-type-export link the dual-namespace identity deletes.
- The `export type { X }` is FORCED, not optional, the moment the exported name is genuinely type-only: an extracted `Schema.Schema.Type<typeof X>` alias, a self-referential opaque instance interface, or a foreign type re-exported through the module each occupy only the type namespace, so omitting `export type` on them is a `verbatimModuleSyntax` error — the dual-namespace identity fuses value and type into one specifier ONLY for the name that occupies both namespaces, and a type-only name is the minimal pair the fusion cannot collapse, its `export type` the second point the value export cannot carry.
- The inbound seam mirrors the outbound: a foreign type imported to annotate or widen the owner — the one `Order.Order<A>` a `SortedSet` seed in the namespace requires, an external instance shape an `interface X` merge structurally absorbs — enters through `import type { ... }` and erases, so an owner whose namespace seed constructs a `SortedSet` pairs the `class X`/`namespace X` declaration with a type-only import of `Order`, the import carrying the type the shape-derived owner cannot itself produce and the `import type` marker keeping the erasure honest.
- The dual export keeps resolution one-hop with no barrel: each owner names its value-and-type export `export { X }` and each genuinely type-only derivation `export type { ... }` at the file end, so a consumer resolves the owner, its statics, and its widened type through the single imported name in one hop — a wildcard re-export or a barrel that forwards the name is the forwarding-helper layer `ONE_HOP_RESOLUTION` deletes, the explicit dual export being the spelling that carries both namespaces without an intermediary.

```typescript
import { Schema } from 'effect'
class Tier extends Schema.Class<Tier>('Tier')({
    rank: Schema.Literal('low', 'mid', 'high'),
    cap: Schema.Number.pipe(Schema.positive()),
}) {
    get headroom(): number {
        return this.cap - this.rank.length
    }
}
namespace Tier {
    export const floor = new Tier({ rank: 'low', cap: 1 })
}
type TierWire = Schema.Schema.Encoded<typeof Tier>
export { Tier }
export type { TierWire }
```

[MERGE_REACH_LEDGER]:
- value-side `namespace X` merge: grows the static surface with author-attached seeds, smart constructors, and sibling instances; targets a class-family owner only; the added names are the complement of the factory's generated statics; the block follows the class line because seeds read the eagerly-built value.
- type-side `interface X` merge: widens the instance type with members the class body cannot express — phantom-branded slots, host-injected fields, externally-structured shapes; order-free at the type level; reserved for type-only members the codec must not see.
- `Schema.Class` free static names: everything but `make`, `fields`, `extend`, `transformOrFail`, `transformOrFailFrom`, `annotations`, `ast`, `identifier`.
- `Data.Class`/`Data.TaggedClass` free static names: the entire static surface — no factory statics reserved.
- `Effect.Service` free static names: everything but `Default`/`DefaultWithoutDependencies`, `make`, `use`, `key`, and every `accessors: true` method-name; the `Service.AllowedType` fence walls those names at the shape seam too.
- `Effect.Tag` free static names: everything but the `Tag.Proxy` accessor projection of the tag's service shape.
- `Data.taggedEnum`: NO merge target — codec-free value record bound by destructuring; statics grow through the record key set, never a namespace.
- dual export: `export { X }` carries both namespaces for a class-family owner; `export type { ... }` is forced for a genuinely type-only derivation; `import type { ... }` admits a foreign type a namespace seed or interface merge consumes.

[MERGE_ANTIPATTERNS]:
- A companion object `const XStatics = { seed, of }` declared beside a class-family owner is the value-side merge written as a second symbol: the namespace merge folds those statics onto the owner's name, so the parallel const is the companion object the two-namespace identity deletes, and a consumer importing both `X` and `XStatics` is the two-hop resolution the merge collapses to one.
- A mirror `interface XShape` declared beside a class to restate its instance shape is the type-side merge written as a second symbol — the class already IS the instance type, so a parallel interface restating its fields is the type-beside-owner link, and a genuine type-only widening belongs in a MERGED `interface X` sharing the name, never a separately-named mirror.
- A `namespace X` placed above its `class X` line is the order-inverted seed defect: the eager heritage has not built the class value, so a namespace seed `new X(...)` reads a TDZ binding and throws at module-init; the namespace always follows the class at the value level.
- A merged namespace member named for a generated factory static (`namespace X { export const make = ... }` over a `Schema.Class`, or `export const Default = ...` over an `Effect.Service`) is the static-side-incompatibility collision: the member merges into the static side and the merged side then fails to extend the factory's base, so a name colliding with `make`/`fields`/`Default`/an accessor name is a compile error on the class declaration, and the repair is a name outside the factory's generated set.
- A getter or method declared in a merged `interface X` rather than the class body is the body-member misrouted to the type namespace: an instance member that is a value belongs in the body where it carries an implementation, and the interface merge is reserved for the type-only member the body cannot express; routing a computable getter through the interface leaves it un-implemented at runtime.
- A `type X = typeof X` minted to re-export a class-family owner under `verbatimModuleSyntax` is the redundant-type-export link: the value `export { X }` already carries the type across the second namespace, so the alias and its `export type` are dead — but the inverse, omitting `export type` on a genuinely type-only derivation, is the missing-marker error, the two failures bounding the dual export from both sides.
- A `Data.taggedEnum` record with a parallel `const EnumStatics = { ... }` beside it for author-attached seeds is the merge reached for on the wrong family: the destructured record has no class name to merge into, so the companion object is unavoidable there — the repair is re-shaping the family to `Schema.Union` of `Schema.TaggedClass` owners (mergeable classes) when author statics are needed, the family form following the merge requirement.
