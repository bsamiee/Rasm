# [CORE_SCHEMA]

`Shape` is the branch's sole value-shape owner: refined cross-language primitives, recursive JSON, ordered vocabularies, absence posture, walk bounds, and ingress ceilings share one import and one derivation floor. Module: `core/src/value/schema.ts`.

## [01]-[SHAPE_OWNER]

- `Shape.vocabulary` snapshots an ordered tuple plus exact rows; every public projection derives once and rows stay private.
- `Shape.admitted` applies every default at one site and NAMES its provider — a fallback at a foreign edge fuses declared with absent.
- `Shape.Bound` is the one walk budget, parameterized on its unit; `fixpoint` names convergence, so no site spells `Infinity` or a bare maximum.
- `Shape.Ingress.bounded` enforces bytes and structural axes; streaming boundaries project `frames` into native limits.
- Coded foreign identity admits through the platform's own registry — canonical locale round-trip, circulating ISO-4217 currency — never free text.
- `Shape.Json` is the recursive JSON schema, suspended at its recursive edges; every primitive refinement and equivalence remains owner-derived.
- `Shape.Record` closes a REFINED key domain at its own node — a refused key fails the decode where bare `Schema.Record` drops it and reports success.
- An open key domain keeps bare `Schema.Record`; the closed form is for keys a refinement already narrows, never for a bag a peer may extend.

```typescript
import { Option, Order, ParseResult, Schema } from "effect"

const _GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-7[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/
const _ISO4217 = /^[A-Z]{3}$/
const _POINTER = /^(?:\/(?:[^/~]|~0|~1)*)*$/

const _canonical = (tag: string): boolean => {
  // BOUNDARY ADAPTER: Intl rejects malformed language tags; the platform throw becomes failed admission.
  try {
    const canonical = Intl.getCanonicalLocales(tag)
    return canonical.length === 1 && canonical[0] === tag
  } catch {
    return false
  }
}

const _circulating = (code: string): boolean => {
  // BOUNDARY ADAPTER: ECMA-402 publishes the circulating ISO-4217 set in canonical upper case; a runtime shipping no
  // registry admits nothing rather than falling open, exactly as the locale adapter reads its own canonicalizer.
  try {
    return Intl.supportedValuesOf("currency").includes(code)
  } catch {
    return false
  }
}

const _Currency = Schema.String.pipe(Schema.pattern(_ISO4217), Schema.filter(_circulating), Schema.brand("Currency"))
  .annotations({ identifier: "Shape.Refined.Currency" })
const _Guid = Schema.String.pipe(Schema.pattern(_GUID), Schema.brand("Guid")).annotations({ identifier: "Shape.Refined.Guid" })
const _JsonPointer = Schema.String.pipe(Schema.pattern(_POINTER), Schema.brand("JsonPointer")).annotations({ identifier: "Shape.Refined.JsonPointer" })
const _Locale = Schema.String.pipe(Schema.maxLength(35), Schema.filter(_canonical), Schema.brand("Locale")).annotations({ identifier: "Shape.Refined.Locale" })
const _OrdinalKey = Schema.Int.pipe(Schema.between(0, 2147483647), Schema.brand("OrdinalKey")).annotations({ identifier: "Shape.Refined.OrdinalKey" })

const _Json: Schema.Schema<Shape.Json> = Schema.suspend(() =>
  Schema.Union(
    Schema.Null,
    Schema.Boolean,
    Schema.Number,
    Schema.String,
    Schema.Array(_Json),
    Schema.Record({ key: Schema.String, value: _Json }),
  ),
)

const _snapshotRow = <A>(row: A): A => {
  const snapshots = new WeakMap<object, unknown>()
  const snapshot = (value: unknown): unknown => {
    if (typeof value !== "object" || value === null) return value
    const prototype = Object.getPrototypeOf(value)
    if (!Array.isArray(value) && prototype !== Object.prototype && prototype !== null) return value
    const prior = snapshots.get(value)
    if (prior !== undefined) return prior
    const held: Array<unknown> | Record<PropertyKey, unknown> = Array.isArray(value) ? [] : Object.create(prototype)
    snapshots.set(value, held)
    Object.assign(
      held,
      Array.isArray(value)
        ? value.map(snapshot)
        : Object.fromEntries(Object.entries(value).map(([key, item]) => [key, snapshot(item)])),
    )
    return Object.freeze(held)
  }
  return snapshot(row) as A
}

type _Unique<
  Kinds extends readonly string[],
  Seen extends string = never,
> = Kinds extends readonly [infer Kind extends string, ...infer Tail extends readonly string[]]
  ? Kind extends Seen
    ? never
    : _Unique<Tail, Seen | Kind>
  : unknown

const _vocabulary = <
  const Kinds extends readonly [string, ...string[]],
  const Rows extends { readonly [Kind in Kinds[number]]: unknown },
>(kinds: Kinds & _Unique<Kinds>, rows: Shape.ExactRows<Kinds, Rows>): Shape.Vocabulary<Kinds, Rows> => {
  // BOUNDARY ADAPTER: snapshot and shallow row seals keep all published derivations on one immutable declaration.
  const heldKinds = structuredClone(kinds)
  const rank = new Map<Kinds[number], number>(heldKinds.map((kind, index) => [kind, index] as const))
  // Rank map keeps each kind's LAST index, so every earlier occurrence of a repeat fails this identity: one pass
  // censuses EVERY offending kind instead of aborting on whichever duplicate a size compare noticed first, and one
  // edit repairs the whole roster. Rows need no census — `ExactRows` closes them against the tuple at compile time,
  // so a duplicated kind is the only admission axis module evaluation can still refuse. Throw survives here because
  // module evaluation over a compile-time roster reaches no rail to fail onto.
  const repeated = [...new Set(heldKinds.filter((kind, index) => rank.get(kind) !== index))]
  if (repeated.length > 0) throw new TypeError(`<vocabulary:duplicate>${repeated.join(",")}`)
  const heldRows = Object.fromEntries(heldKinds.map((kind) => [kind, _snapshotRow(rows[kind])])) as unknown as Rows
  Object.freeze(heldKinds)
  Object.freeze(heldRows)
  const schema = Schema.Literal(...heldKinds)
  return Object.freeze({
    kinds: heldKinds,
    schema,
    order: Order.mapInput(Order.number, (kind: Kinds[number]) => rank.get(kind) ?? -1),
    is: Schema.is(schema),
    at: <Kind extends Kinds[number]>(kind: Kind): Rows[Kind] => heldRows[kind],
  })
}

// Absence posture. `{ default: () => value }` stays lawful only while no consumer distinguishes absent from
// defaulted; once one does — a rollout gate deciding canary exposure, a frozen cross-language preimage, an audit
// rail separating a subject's assertion from this deployment's fallback — that default has FUSED two states into one
// value nothing downstream can unfuse. Posture rows carry the two facts a consumer reads: `held` decides whether a
// value exists at all, `asserted` whether the value's own producer stated it. Kinds ascend by evidence, so
// `Order.greaterThanOrEqualTo(Shape.posture.order)(kind, "declared")` gates every preimage and receipt.
const _postureKinds = ["absent", "defaulted", "declared"] as const
const _postureRows = {
  absent: { asserted: false, held: false },
  defaulted: { asserted: false, held: true },
  declared: { asserted: true, held: true },
} as const
const _postures = _vocabulary(_postureKinds, _postureRows)

// Providers are NAMED rows, never free text: `owner` marks a value this declaration itself fixes, `deployment` one
// an operator supplied through a config row. Sources no row spells cannot enter the carrier.
const _sourceKinds = ["deployment", "owner"] as const
const _sourceRows = { deployment: {}, owner: {} } as const
const _sources = _vocabulary(_sourceKinds, _sourceRows)

const _held = <A, I, R>(value: Schema.Schema<A, I, R>) =>
  Schema.Union(
    Schema.TaggedStruct("declared", { value }),
    Schema.TaggedStruct("defaulted", { value, source: _sources.schema }),
  )

const _posture = <A, I, R>(value: Schema.Schema<A, I, R>) =>
  Schema.Union(Schema.TaggedStruct("absent", {}), _held(value))

// Field form: key present decodes `declared`, key absent decodes `defaulted` carrying the provider that supplied it,
// so the interior gains the discriminant while the ENCODED side keeps the plain optional key every peer already
// writes. Both held cases publish `value`, so a consumer of an admitted field reads `field.value` totally and pays no
// fold; only the three-case family — a field whose absence has no default — spends `Shape.posture.value`.
const _admitted = <A, I, R>(
  value: Schema.Schema<A, I, R>,
  fallback: { readonly source: Shape.PostureSource; readonly value: () => A },
) =>
  Schema.optionalWith(
    // Carrier rides the TYPE side: this field's own admission already ran in the `from` position, so the posture
    // wraps an admitted value rather than re-encoding it, and the encoded field stays the peer's plain optional key.
    Schema.transform(value, _held(Schema.typeSchema(value)), {
      strict: true,
      decode: (held) => ({ _tag: "declared" as const, value: held }),
      encode: (posture) => posture.value,
    }),
    { default: (): Shape.Admitted<A> => ({ _tag: "defaulted", value: fallback.value(), source: fallback.source }) },
  )

// `Schema.Record`'s default excess-property posture DROPS a key its own refinement refused and answers SUCCESS: a
// document carrying one bad key decodes to a map missing it, which reads exactly like the map a peer never sent — the
// silent admission failure a refined key exists to foreclose. `onExcessProperty: "error"` converts that refusal into
// a parse issue, and it seats at THIS node because the option does not inherit: annotating the enclosing struct
// leaves the record's own key domain open, and passing it at each `decodeUnknown` call makes closure a choice a
// caller forgets. One owner spells it once and every refined-key record on the branch inherits the posture.
// An OPEN key domain — a header bag, an attribute map a peer may extend, anything a later filter narrows — stays bare
// `Schema.Record`, because closing a key nothing refines refuses the very material the boundary exists to admit.
const _record = <Key extends Schema.Schema.AnyNoContext, Value extends Schema.Schema.All>(
  key: Key,
  value: Value,
): Schema.Record$<Key, Value> =>
  Schema.Record({ key, value }).annotations({ parseOptions: { onExcessProperty: "error" } })

const _value = <A>(posture: Shape.Posture<A>): Option.Option<A> =>
  posture._tag === "absent" ? Option.none() : Option.some(posture.value)

const _source = <A>(posture: Shape.Posture<A>): Option.Option<Shape.PostureSource> =>
  posture._tag === "defaulted" ? Option.some(posture.source) : Option.none()

// Walk bounds. Three quantities share one carrier and stay unconfusable because the UNIT is a type parameter: `fuel`
// counts microsteps a fold spends, `hops` counts levels a walk or a structure descends, `fanout` counts the width a
// tier branches. Collapsing them onto one scalar would erase exactly the guarantee each separate brand existed to
// give, so a `Bound<"fuel">` never unifies with a `Bound<"hops">`. Each unit's floor is the arithmetic its own meaning
// admits — a fan narrower than two is a chain, and a budget of zero steps is absence spelled as a number.
const _boundKinds = ["fanout", "fuel", "hops"] as const
const _boundRows = {
  fanout: { floor: 2 },
  fuel: { floor: 1 },
  hops: { floor: 1 },
} as const
const _bounds = _vocabulary(_boundKinds, _boundRows)

const _Ceiling = Schema.Int.pipe(Schema.positive(), Schema.brand("Ceiling"))

// Exhaustion evidence every bounded walk refuses with: unit that ran out, ceiling it was given, count it reached.
// One shape, so a folder embeds it in its own tagged fault instead of minting a private twin.
const _Spent = Schema.Struct({ unit: _bounds.schema, ceiling: _Ceiling, reached: Schema.Int.pipe(Schema.nonNegative()) })

const _finite = <const Unit extends Shape.BoundUnit>(unit: Unit) =>
  Schema.TaggedStruct("bounded", { unit: Schema.Literal(unit), ceiling: _Ceiling }).pipe(
    Schema.filter(({ ceiling }) => ceiling >= _bounds.at(unit).floor || `<bound:${unit}-floor>`),
  )

// `fixpoint` is the whole point of the union: a walk that runs to convergence DELEGATES termination to whatever owns
// it — an incremental-dataflow iterate, a trail-guarded closure — and says so as a value. The deleted spellings are an
// absent parameter, `Infinity`, and a sentinel maximum, each of which reads as a forgotten bound rather than a stated
// one. A surface that cannot admit convergence — a hostile-ingress gate, a Merkle tier — declares `finite` instead.
const _bound = <const Unit extends Shape.BoundUnit>(unit: Unit) =>
  Schema.Union(Schema.TaggedStruct("fixpoint", { unit: Schema.Literal(unit) }), _finite(unit))

const _bounded = <const Unit extends Shape.BoundUnit>(unit: Unit, ceiling: number): Shape.BoundFinite<Unit> =>
  Schema.decodeSync(_finite(unit))({ _tag: "bounded", unit, ceiling })

const _fixpoint = <const Unit extends Shape.BoundUnit>(unit: Unit): Shape.BoundFixpoint<Unit> => ({ _tag: "fixpoint", unit })

// One gate for every bounded walk, and `none` is the admitted answer: the refusal payload constructs on the refusing
// arm alone, so a per-node hostile-input walk allocates nothing while passing and a spent budget still hands its whole
// evidence row to the fault that reports it.
const _spent = <Unit extends Shape.BoundUnit>(bound: Shape.Bound<Unit>, reached: number): Option.Option<Shape.BoundSpent> =>
  bound._tag === "fixpoint" || reached <= bound.ceiling
    ? Option.none()
    : Option.some({ unit: bound.unit, ceiling: bound.ceiling, reached })

const _ceiling = <Unit extends Shape.BoundUnit>(bound: Shape.Bound<Unit>): Option.Option<Shape.BoundCeiling> =>
  bound._tag === "fixpoint" ? Option.none() : Option.some(bound.ceiling)

// `depth` counts the structural levels this page's own probe walks, so it takes the shared carrier under `hops`
// while the four byte-and-count axes take the shared ceiling brand; a second positive-integer brand beside
// `Shape.Bound.Ceiling` would be two names for one budget. Choosing the `finite` arm is
// deliberate: an ingress ceiling able to spell convergence is a hostile-depth hole wearing a sentinel.
const _Ingress = Schema.Struct({
  bytes: _Ceiling,
  members: _Ceiling,
  frames: _Ceiling,
  depth: _finite("hops"),
  collection: _Ceiling,
})
const _ingress = Schema.decodeSync(_Ingress)({
  bytes: 268435456,
  members: 4096,
  frames: 4096,
  depth: { _tag: "bounded", unit: "hops", ceiling: 64 },
  collection: 1024,
})

const _probe = (root: unknown, ceiling: Shape.Ingress): Option.Option<Shape.IngressIssue> => {
  // BOUNDARY ADAPTER: the iterative identity-graph walk bounds hostile depth without consuming the JS call stack.
  const path = new Set<object>()
  const seen = new Set<object>()
  const stack: Array<readonly [unknown, number, boolean]> = [[root, 0, false]]
  let bytes = 0
  while (stack.length > 0) {
    const entry = stack.pop()
    if (entry === undefined) return Option.none()
    const [value, depth, leaving] = entry
    if (typeof value !== "object" || value === null) continue
    if (leaving) {
      path.delete(value)
      continue
    }
    if (path.has(value)) return Option.some({ reason: "cycle", limit: ceiling.depth.ceiling })
    if (seen.has(value)) continue
    seen.add(value)
    // Shared gate decides the hop budget; five ingress reasons stay this owner's own AXES, so the walk keeps
    // spelling `depth` beside `bytes` and `cycle` rather than folding a structural axis into an exhaustion word.
    if (Option.isSome(_spent(ceiling.depth, depth))) return Option.some({ reason: "depth", limit: ceiling.depth.ceiling })
    const byteLength = ArrayBuffer.isView(value) || value instanceof ArrayBuffer
      ? value.byteLength
      : typeof SharedArrayBuffer !== "undefined" && value instanceof SharedArrayBuffer
        ? value.byteLength
        : undefined
    if (byteLength !== undefined) {
      bytes += byteLength
      if (bytes > ceiling.bytes) return Option.some({ reason: "bytes", limit: ceiling.bytes })
      continue
    }
    const children = Object.values(value)
    if (Array.isArray(value) && children.length > ceiling.collection)
      return Option.some({ reason: "collection", limit: ceiling.collection })
    if (!Array.isArray(value) && children.length > ceiling.members)
      return Option.some({ reason: "members", limit: ceiling.members })
    path.add(value)
    stack.push([value, depth, true])
    for (const child of children) stack.push([child, depth + 1, false])
  }
  return Option.none()
}

const _gate = (ceiling: Shape.Ingress): Schema.Schema<unknown, unknown> =>
  Schema.transformOrFail(Schema.Unknown, Schema.Unknown, {
    strict: true,
    decode: (raw, _options, ast) => {
      try {
        return Option.match(_probe(raw, ceiling), {
          onNone: () => ParseResult.succeed(raw),
          onSome: ({ reason, limit }) => ParseResult.fail(new ParseResult.Type(ast, raw, `<ingress:${reason}>${limit}`)),
        })
      } catch {
        return ParseResult.fail(new ParseResult.Type(ast, raw, "<ingress:inspection>"))
      }
    },
    encode: ParseResult.succeed,
  })

const Shape = {
  Json: _Json,
  Record: _record,
  vocabulary: _vocabulary,
  admitted: _admitted,
  Refined: {
    Currency: _Currency,
    Guid: _Guid,
    JsonPointer: _JsonPointer,
    Locale: _Locale,
    OrdinalKey: _OrdinalKey,
    alike: {
      Currency: Schema.equivalence(_Currency),
      Guid: Schema.equivalence(_Guid),
      JsonPointer: Schema.equivalence(_JsonPointer),
      Locale: Schema.equivalence(_Locale),
      OrdinalKey: Schema.equivalence(_OrdinalKey),
    },
  },
  posture: {
    ..._postures, // `schema` here is the KIND literal a receipt carries; `of` is the value-bearing family
    sources: _sources,
    of: _posture,
    source: _source,
    value: _value,
  },
  Bound: {
    ..._bounds,
    Ceiling: _Ceiling,
    Spent: _Spent,
    of: _bound,
    finite: _finite,
    bounded: _bounded,
    fixpoint: _fixpoint,
    ceiling: _ceiling,
    spent: _spent,
  },
  Ingress: {
    Schema: _Ingress,
    floor: _ingress,
    bounded: <A, I, R>(schema: Schema.Schema<A, I, R>, ceiling: Shape.Ingress = _ingress): Schema.Schema<A, unknown, R> =>
      Schema.compose(_gate(ceiling), schema, { strict: false }),
  },
} as const

declare namespace Shape {
  type Admitted<A> = Schema.Schema.Type<ReturnType<typeof _held<A, A, never>>>
  type Bound<Unit extends BoundUnit = BoundUnit> = Schema.Schema.Type<ReturnType<typeof _bound<Unit>>>
  type BoundCeiling = typeof _Ceiling.Type
  type BoundFinite<Unit extends BoundUnit = BoundUnit> = Schema.Schema.Type<ReturnType<typeof _finite<Unit>>>
  type BoundFixpoint<Unit extends BoundUnit = BoundUnit> = Exclude<Bound<Unit>, BoundFinite<Unit>>
  type BoundSpent = typeof _Spent.Type
  type BoundUnit = (typeof _boundKinds)[number]
  type Ingress = typeof _Ingress.Type
  type IngressIssue = { readonly reason: "bytes" | "collection" | "cycle" | "depth" | "members"; readonly limit: typeof _Ceiling.Type }
  type Json = null | boolean | number | string | ReadonlyArray<Json> | { readonly [key: string]: Json }
  type Posture<A> = Schema.Schema.Type<ReturnType<typeof _posture<A, A, never>>>
  type PostureKind = (typeof _postureKinds)[number]
  type PostureSource = (typeof _sourceKinds)[number]
  namespace Refined {
    type Currency = typeof _Currency.Type
    type Guid = typeof _Guid.Type
    type JsonPointer = typeof _JsonPointer.Type
    type Locale = typeof _Locale.Type
    type OrdinalKey = typeof _OrdinalKey.Type
  }
  type ExactRows<
    Kinds extends readonly [string, ...string[]],
    Rows extends { readonly [Kind in Kinds[number]]: unknown },
  > = Rows & { readonly [Kind in Exclude<keyof Rows, Kinds[number]>]: never }
  type Vocabulary<
    Kinds extends readonly [string, ...string[]],
    Rows extends { readonly [Kind in Kinds[number]]: unknown },
  > = {
    readonly kinds: Kinds
    readonly schema: Schema.Literal<Kinds>
    readonly order: Order.Order<Kinds[number]>
    readonly is: (input: unknown) => input is Kinds[number]
    readonly at: <Kind extends Kinds[number]>(kind: Kind) => Rows[Kind]
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Shape }
```

## [02]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
