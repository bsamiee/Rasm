# [CORE_SCHEMA]

The decode-once law's type floor: one `Refined` vocabulary owner carries every cross-language branded primitive — Guid-v7, `OrdinalKey`, `JsonPointer`, BCP-47 `Locale` — and one `Ingress` owner carries the decode-budget ceilings plus the budget-gated schema combinator every untrusted seam composes. A raw `string` or `number` never occupies an identity slot past a decode seam: every wire shape, journal event, config read, and message catalog composes these schemas at its own field record, so each brand has exactly one edit site and every folder reads one nominal identity per concept. The module is `core/src/value/schema.ts`; a new cross-language primitive is one interior anchor plus one owner row — never a per-folder re-declaration, never a standalone sibling export.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                  | [PUBLIC]  |
| :-----: | :--------------- | :-------------------------------------------------------- | :-------- |
|  [01]   | `REFINED_FLOOR`  | the branded primitive family and its merged type hub      | `Refined` |
|  [02]   | `INGRESS_CEILING` | decode-budget ceilings and the budget-gated schema seam   | `Ingress` |

## [2]-[REFINED_FLOOR]

[REFINED_FLOOR]:
- Owner: `Refined`, the assembled brand vocabulary — interior `_`-anchors carry the refinements, the exported owner assembles them under a stated annotation, and the merged `declare namespace Refined` hub serves every branded type off the single import, so `Refined.Guid` is the schema in value position and the branded type in type position.
- Law: `Refined.Guid` admits UUID version 7 only, lowercase only — version nibble `7`, variant nibble `[89ab]` — one spelling per identity; an uppercase or mixed-case inbound guid is wire skew repaired at the admitting seam's transform, never a widening here.
- Law: `Refined.OrdinalKey` is the C# smart-enum ordinal crossing — a non-negative int32 (`0..2147483647`); smart-enum member semantics stay C#-owned, and the floor carries only the ordinal identity decoded rows key on.
- Law: `Refined.JsonPointer` is the one RFC 6901 path identity — `""` admits the whole-document pointer, each segment admits `[^/~]`, `~0`, `~1` only — so a patch rail resolves a branded pointer and an unescaped raw `/`-joined string is unspellable in an identity slot.
- Law: `Refined.Locale` admits canonical BCP-47 only — the filter proves `Intl.getCanonicalLocales(tag)` returns exactly the input, so one spelling exists per locale and the ui intl catalogs key on it with zero normalize passes; canonicalizing a non-canonical inbound tag is a transform at the admitting seam.
- Law: derived surfaces stay truthful from the refinements alone — the pattern and range filters flow into `Arbitrary.make` and `JSONSchema.make`; the floor-brand arbitraries live in `tests/typescript/_testkit`, never on a `@rasm/ts` subpath.
- Exemption: `_canonical` is a marked kernel — `Intl.getCanonicalLocales` throws `RangeError` on a malformed tag, and the `try`/`catch` folds that platform throw into the filter's `false` before anything leaves the kernel; the implementer carries the `// BOUNDARY ADAPTER` mark on its first line.
- Growth: a new cross-language primitive is one interior anchor, one owner property, and one hub type line; a primitive needed by exactly one folder-local shape stays a field refinement inside that owner and never lands here.
- Boundary: `ContentKey`, `Hlc`, `AppKey`, and `TenantKey` are richer identities owned by their own `value` pages; this floor carries only the primitives whose whole algebra is admission.
- Packages: `effect` (`Schema`).

```typescript
import { ParseResult, Schema } from "effect"

const _GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-7[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/
const _POINTER = /^(?:\/(?:[^/~]|~0|~1)*)*$/

const _canonical = (tag: string): boolean => {
  try {
    const canon = Intl.getCanonicalLocales(tag)
    return canon.length === 1 && canon[0] === tag
  } catch {
    return false
  }
}

const _Guid = Schema.String.pipe(Schema.pattern(_GUID), Schema.brand("Guid"))
const _JsonPointer = Schema.String.pipe(Schema.pattern(_POINTER), Schema.brand("JsonPointer"))
const _Locale = Schema.String.pipe(Schema.maxLength(35), Schema.filter(_canonical), Schema.brand("Locale"))
const _OrdinalKey = Schema.Int.pipe(Schema.between(0, 2147483647), Schema.brand("OrdinalKey"))

declare namespace Refined {
  type Guid = typeof _Guid.Type
  type JsonPointer = typeof _JsonPointer.Type
  type Locale = typeof _Locale.Type
  type OrdinalKey = typeof _OrdinalKey.Type
}

const Refined: {
  readonly Guid: typeof _Guid
  readonly JsonPointer: typeof _JsonPointer
  readonly Locale: typeof _Locale
  readonly OrdinalKey: typeof _OrdinalKey
} = { Guid: _Guid, JsonPointer: _JsonPointer, Locale: _Locale, OrdinalKey: _OrdinalKey }
```

## [3]-[INGRESS_CEILING]

[INGRESS_CEILING]:
- Owner: `Ingress`, the untrusted-ingress ceiling vocabulary — `floor` is the default budget row, and `bounded(schema, budget?)` composes the depth gate in front of any schema, so a JSON-bomb fails decode as a typed `ParseError` before it exhausts the runtime and the budget is recoverable from the seam declaration.
- Law: `Ingress` prices decode admission only — the resilience retry vocabulary is `fault#RETRY_BUDGET`'s `Budget`, and the two names never share a concept: one bounds what a seam admits, the other bounds how a rail re-drives.
- Law: `bounded` is a schema combinator, never a decode wrapper — `Schema.compose` over a `Schema.transformOrFail` gate keeps the result a first-class schema that joins unions, feeds `Schema.decodeUnknown`, and derives like any owner; the configured decode built from it is a module-scope value at the consuming seam, one admission policy per seam.
- Law: the three ceilings split by enforcement site — `maxDepth` is enforced here by the gate; `maxFrames` and `maxAssembledBytes` are stream ceilings the interchange frame rail enforces with `Stream.take` and a running-byte filter, consuming these rows as values so the whole ingress budget has one declaration.
- Law: depth counts object nesting with the root's children at 1, and the probe short-circuits at first breach — the gate answers "too deep", never "how deep", so a wide shallow payload costs one sweep and a deep bomb costs `maxDepth` levels.
- Law: budgets gate untrusted ingress only — a same-origin trusted lane composes its schema bare, and a budget re-applied to an already-admitted value is the interior re-validation defect.
- Exemption: `_deeper` is a marked kernel — an iterative frontier over rebound arrays, licensed by the fixed JS call-stack ceiling adversarial depth overflows; the record-view pin on the traversed object is the kernel's sanctioned cast, no mutable reference escapes, and the implementer carries the `// BOUNDARY ADAPTER` mark on its first line.
- Growth: a new ceiling is one `Shape` field plus one enforcement row at its owning seam; a per-surface budget is a caller-composed `Shape` value, never a second owner.
- Boundary: which seams are untrusted, and the quarantine intake a breached decode diverts to, are interchange decisions; this owner fixes the ceilings and the gate.
- Packages: `effect` (`Schema`, `ParseResult`).

```typescript
declare namespace Ingress {
  type Shape = {
    readonly maxDepth: number
    readonly maxFrames: number
    readonly maxAssembledBytes: number
  }
}

const _FLOOR: Ingress.Shape = { maxDepth: 64, maxFrames: 4096, maxAssembledBytes: 268435456 }

const _deeper = (root: unknown, ceiling: number): boolean => {
  let frontier: ReadonlyArray<readonly [unknown, number]> = [[root, 0]]
  while (frontier.length > 0) {
    const next: Array<readonly [unknown, number]> = []
    for (const [value, depth] of frontier) {
      if (typeof value !== "object" || value === null) continue
      if (depth + 1 > ceiling) return true
      for (const child of Object.values(value as { readonly [key: string]: unknown })) next.push([child, depth + 1])
    }
    frontier = next
  }
  return false
}

const _gate = (budget: Ingress.Shape): Schema.Schema<unknown, unknown> =>
  Schema.transformOrFail(Schema.Unknown, Schema.Unknown, {
    strict: true,
    decode: (raw, _options, ast) =>
      _deeper(raw, budget.maxDepth)
        ? ParseResult.fail(new ParseResult.Type(ast, raw, `depth>${budget.maxDepth}`))
        : ParseResult.succeed(raw),
    encode: (value) => ParseResult.succeed(value),
  })

const Ingress: {
  readonly floor: Ingress.Shape
  readonly bounded: <A, I, R>(schema: Schema.Schema<A, I, R>, budget?: Ingress.Shape) => Schema.Schema<A, unknown, R>
} = {
  floor: _FLOOR,
  bounded: (schema, budget = _FLOOR) => Schema.compose(_gate(budget), schema, { strict: false }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Ingress, Refined }
```
