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
  try {
    const canonical = Intl.getCanonicalLocales(tag)
    return canonical.length === 1 && canonical[0] === tag
  } catch {
    return false
  }
}

const _circulating = (code: string): boolean => {
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
  const heldKinds = structuredClone(kinds)
  const rank = new Map<Kinds[number], number>(heldKinds.map((kind, index) => [kind, index] as const))
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

const _postureKinds = ["absent", "defaulted", "declared"] as const
const _postureRows = {
  absent: { asserted: false, held: false },
  defaulted: { asserted: false, held: true },
  declared: { asserted: true, held: true },
} as const
const _postures = _vocabulary(_postureKinds, _postureRows)

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

const _admitted = <A, I, R>(
  value: Schema.Schema<A, I, R>,
  fallback: { readonly source: Shape.PostureSource; readonly value: () => A },
) =>
  Schema.optionalWith(
    Schema.transform(value, _held(Schema.typeSchema(value)), {
      strict: true,
      decode: (held) => ({ _tag: "declared" as const, value: held }),
      encode: (posture) => posture.value,
    }),
    { default: (): Shape.Admitted<A> => ({ _tag: "defaulted", value: fallback.value(), source: fallback.source }) },
  )

const _record = <Key extends Schema.Schema.AnyNoContext, Value extends Schema.Schema.All>(
  key: Key,
  value: Value,
): Schema.Record$<Key, Value> =>
  Schema.Record({ key, value }).annotations({ parseOptions: { onExcessProperty: "error" } })

const _value = <A>(posture: Shape.Posture<A>): Option.Option<A> =>
  posture._tag === "absent" ? Option.none() : Option.some(posture.value)

const _source = <A>(posture: Shape.Posture<A>): Option.Option<Shape.PostureSource> =>
  posture._tag === "defaulted" ? Option.some(posture.source) : Option.none()

const _boundKinds = ["fanout", "fuel", "hops"] as const
const _boundRows = {
  fanout: { floor: 2 },
  fuel: { floor: 1 },
  hops: { floor: 1 },
} as const
const _bounds = _vocabulary(_boundKinds, _boundRows)

const _Ceiling = Schema.Int.pipe(Schema.positive(), Schema.brand("Ceiling"))

const _Spent = Schema.Struct({ unit: _bounds.schema, ceiling: _Ceiling, reached: Schema.Int.pipe(Schema.nonNegative()) })

const _finite = <const Unit extends Shape.BoundUnit>(unit: Unit) =>
  Schema.TaggedStruct("bounded", { unit: Schema.Literal(unit), ceiling: _Ceiling }).pipe(
    Schema.filter(({ ceiling }) => ceiling >= _bounds.at(unit).floor || `<bound:${unit}-floor>`),
  )

const _bound = <const Unit extends Shape.BoundUnit>(unit: Unit) =>
  Schema.Union(Schema.TaggedStruct("fixpoint", { unit: Schema.Literal(unit) }), _finite(unit))

const _bounded = <const Unit extends Shape.BoundUnit>(unit: Unit, ceiling: number): Shape.BoundFinite<Unit> =>
  Schema.decodeSync(_finite(unit))({ _tag: "bounded", unit, ceiling })

const _fixpoint = <const Unit extends Shape.BoundUnit>(unit: Unit): Shape.BoundFixpoint<Unit> => ({ _tag: "fixpoint", unit })

const _spent = <Unit extends Shape.BoundUnit>(bound: Shape.Bound<Unit>, reached: number): Option.Option<Shape.BoundSpent> =>
  bound._tag === "fixpoint" || reached <= bound.ceiling
    ? Option.none()
    : Option.some({ unit: bound.unit, ceiling: bound.ceiling, reached })

const _ceiling = <Unit extends Shape.BoundUnit>(bound: Shape.Bound<Unit>): Option.Option<Shape.BoundCeiling> =>
  bound._tag === "fixpoint" ? Option.none() : Option.some(bound.ceiling)

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
    ..._postures,
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { Shape }
```

## [02]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
