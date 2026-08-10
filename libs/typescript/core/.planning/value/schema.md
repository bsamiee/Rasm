# [CORE_SCHEMA]

`Shape` is the branch's sole value-shape owner: refined cross-language primitives, recursive JSON, exact ordered vocabularies, and untrusted-ingress ceilings share one import and one derivation floor. Module: `core/src/value/schema.ts`.

## [01]-[SHAPE_OWNER]

- `Shape.vocabulary` snapshots an ordered tuple plus exact rows; every public projection derives once and rows stay private.
- `Shape.Ingress.bounded` enforces bytes and structural axes; streaming boundaries project `frames` into native limits.
- `Shape.Json` is the recursive JSON schema, suspended at its recursive edges; every primitive refinement and equivalence remains owner-derived.

```typescript
import { Option, Order, ParseResult, Schema } from "effect"

const _GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-7[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/
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
  if (rank.size !== heldKinds.length) throw new TypeError("<vocabulary:duplicate>")
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

const _Ceiling = Schema.Int.pipe(Schema.positive(), Schema.brand("IngressCeiling"))
const _Ingress = Schema.Struct({
  bytes: _Ceiling,
  members: _Ceiling,
  frames: _Ceiling,
  depth: _Ceiling,
  collection: _Ceiling,
})
const _ingress = Schema.decodeSync(_Ingress)({ bytes: 268435456, members: 4096, frames: 4096, depth: 64, collection: 1024 })

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
    if (path.has(value)) return Option.some({ reason: "cycle", limit: ceiling.depth })
    if (seen.has(value)) continue
    seen.add(value)
    if (depth > ceiling.depth) return Option.some({ reason: "depth", limit: ceiling.depth })
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
  vocabulary: _vocabulary,
  Refined: {
    Guid: _Guid,
    JsonPointer: _JsonPointer,
    Locale: _Locale,
    OrdinalKey: _OrdinalKey,
    alike: {
      Guid: Schema.equivalence(_Guid),
      JsonPointer: Schema.equivalence(_JsonPointer),
      Locale: Schema.equivalence(_Locale),
      OrdinalKey: Schema.equivalence(_OrdinalKey),
    },
  },
  Ingress: {
    Ceiling: _Ceiling,
    Schema: _Ingress,
    floor: _ingress,
    bounded: <A, I, R>(schema: Schema.Schema<A, I, R>, ceiling: Shape.Ingress = _ingress): Schema.Schema<A, unknown, R> =>
      Schema.compose(_gate(ceiling), schema, { strict: false }),
  },
} as const

declare namespace Shape {
  type Ingress = typeof _Ingress.Type
  type IngressIssue = { readonly reason: "bytes" | "collection" | "cycle" | "depth" | "members"; readonly limit: typeof _Ceiling.Type }
  type Json = null | boolean | number | string | ReadonlyArray<Json> | { readonly [key: string]: Json }
  namespace Refined {
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
