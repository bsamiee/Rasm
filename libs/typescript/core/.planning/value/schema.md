# [CORE_SCHEMA]

The decode-once law's type floor: one `Refined` vocabulary owner carries every cross-language branded primitive — Guid-v7, `OrdinalKey`, `JsonPointer`, BCP-47 `Locale` — and one `Ingress` owner carries the decode-budget ceilings plus the budget-gated schema combinator every untrusted seam composes. A raw `string` or `number` never occupies an identity slot past a decode seam: every wire shape, journal event, config read, and message catalog composes these schemas at its own field record, so each brand has exactly one edit site and every folder reads one nominal identity per concept. The module is `core/src/value/schema.ts`; a new cross-language primitive is one interior anchor plus one owner row — never a per-folder re-declaration, never a standalone sibling export.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                  | [PUBLIC]  |
| :-----: | :---------------- | :------------------------------------------------------ | :-------- |
|  [01]   | `REFINED_FLOOR`   | the branded primitive family and its merged type hub    | `Refined` |
|  [02]   | `INGRESS_CEILING` | decode-budget ceilings and the budget-gated schema seam | `Ingress` |

## [02]-[REFINED_FLOOR]

[REFINED_FLOOR]:
- Owner: `Refined`, the assembled brand vocabulary — interior `_`-anchors carry the refinements, the exported owner assembles them under a stated annotation, and the merged `declare namespace Refined` hub serves every branded type off the single import, so `Refined.Guid` is the schema in value position and the branded type in type position.
- Law: `Refined.Guid` admits UUID version 7 only, lowercase only — version nibble `7`, variant nibble `[89ab]` — one spelling per identity; an uppercase or mixed-case inbound guid is wire skew repaired at the admitting seam's transform, never a widening here.
- Law: `Refined.OrdinalKey` is the C# smart-enum ordinal crossing — a non-negative int32 (`0..2147483647`); smart-enum member semantics stay C#-owned, and the floor carries only the ordinal identity decoded rows key on.
- Law: `Refined.JsonPointer` is the one RFC 6901 path identity — `""` admits the whole-document pointer, each segment admits `[^/~]`, `~0`, `~1` only — so a patch rail resolves a branded pointer and an unescaped raw `/`-joined string is unspellable in an identity slot.
- Law: `Refined.Locale` admits canonical BCP-47 only — the filter proves `Intl.getCanonicalLocales(tag)` returns exactly the input, so one spelling exists per locale and the ui intl catalogs key on it with zero normalize passes; canonicalizing a non-canonical inbound tag is a transform at the admitting seam.
- Law: derived surfaces stay truthful from the refinements alone — the pattern and range filters flow into `Arbitrary.make` and `JSONSchema.make`, while `Refined.alike` carries the `Schema.equivalence` projections every keyed collection and dedup fold shares; the floor-brand arbitraries live in `tests/typescript/_testkit`, never on a `@rasm/ts` subpath.
- Exemption: `_canonical` is a marked kernel — `Intl.getCanonicalLocales` throws `RangeError` on a malformed tag, and the `try`/`catch` folds that platform throw into the filter's `false` before anything leaves the kernel; the implementer carries the `// BOUNDARY ADAPTER` mark on its first line.
- Growth: a new cross-language primitive is one interior anchor, one owner property, and one hub type line; a primitive needed by exactly one folder-local shape stays a field refinement inside that owner and never lands here.
- Boundary: `ContentKey`, `Hlc`, `AppKey`, and `TenantKey` are richer identities owned by their own `value` pages; this floor carries only the primitives whose whole algebra is admission.
- Packages: `effect` (`Schema`).

```typescript
import { Equivalence, ParseResult, pipe, Schema } from "effect"

const _GUID = /^[0-9a-f]{8}-[0-9a-f]{4}-7[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/
const _POINTER = /^(?:\/(?:[^/~]|~0|~1)*)*$/

const _canonical = (tag: string): boolean => {
  // BOUNDARY ADAPTER: Intl.getCanonicalLocales throws RangeError on a malformed tag; the platform throw folds to false before anything leaves
  try {
    const canon = Intl.getCanonicalLocales(tag)
    return canon.length === 1 && canon[0] === tag
  } catch {
    return false
  }
}

const _Guid = Schema.String.pipe(Schema.pattern(_GUID), Schema.brand("Guid")).annotations({ identifier: "Refined.Guid" })
const _JsonPointer = Schema.String.pipe(Schema.pattern(_POINTER), Schema.brand("JsonPointer")).annotations({ identifier: "Refined.JsonPointer" })
const _Locale = Schema.String.pipe(Schema.maxLength(35), Schema.filter(_canonical), Schema.brand("Locale")).annotations({ identifier: "Refined.Locale" })
const _OrdinalKey = Schema.Int.pipe(Schema.between(0, 2147483647), Schema.brand("OrdinalKey")).annotations({ identifier: "Refined.OrdinalKey" })

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
  readonly alike: {
    readonly Guid: Equivalence.Equivalence<Refined.Guid>
    readonly JsonPointer: Equivalence.Equivalence<Refined.JsonPointer>
    readonly Locale: Equivalence.Equivalence<Refined.Locale>
    readonly OrdinalKey: Equivalence.Equivalence<Refined.OrdinalKey>
  }
} = {
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
}
```

## [03]-[INGRESS_CEILING]

[INGRESS_CEILING]:
- Owner: `Ingress`, the untrusted-ingress ceiling vocabulary — `floor` is the default budget row, and `bounded(schema, budget?)` composes the depth gate in front of any schema, so a JSON-bomb fails decode as a typed `ParseError` before it exhausts the runtime and the budget is recoverable from the seam declaration.
- Law: `Ingress` prices decode admission only — the resilience retry vocabulary is `fault#RETRY_BUDGET`'s `Budget`, and the two names never share a concept: one bounds what a seam admits, the other bounds how a rail re-drives.
- Law: `bounded` is a schema combinator, never a decode wrapper — `Schema.compose` over a `Schema.transformOrFail` gate keeps the result a first-class schema that joins unions, feeds `Schema.decodeUnknown`, and derives like any owner; the configured decode built from it is a module-scope value at the consuming seam, one admission policy per seam.
- Law: the three ceilings split by enforcement site — `maxDepth` is enforced here by the gate; `maxFrames` and `maxAssembledBytes` are stream ceilings the interchange frame rail enforces with `Stream.take` and a running-byte filter, consuming these rows as values so the whole ingress budget has one declaration; per-dialect breadth and length ceilings (array, map, string, ext sizes) are the byte-engine configuration rows at the interchange format owner — dialect structure the shape-agnostic gate cannot see — priced beside these rows, never a second budget concept.
- Law: depth counts object nesting with the root at `0` and its children at `1`, and the probe short-circuits at first breach — the gate answers "too deep", never "how deep", so a wide shallow payload costs one sweep and a deep bomb costs `maxDepth + 1` admitted object levels.
- Law: the walk is bounded by the identity graph, never the unfolding tree — a repeat sighting on the live path is a self-referential value no wire dialect can spell, refused as the same typed `ParseError` (`cycle`); a repeat sighting off the path is sharing already priced at first sighting, so a DAG-shaped payload never multiplies the sweep and no input shape can hold the gate open.
- Law: budgets gate untrusted ingress only — a same-origin trusted lane composes its schema bare, and a budget re-applied to an already-admitted value is the interior re-validation defect.
- Exemption: `_probe` is a marked kernel — an iterative depth-first walk over an explicit stack with a live-path set and a visited set, licensed by the fixed JS call-stack ceiling adversarial depth overflows; the record-view pin on the traversed object is the kernel's sanctioned cast, no mutable reference escapes, and the mark rides its first line.
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
  type Verdict = "clear" | "cycle" | "depth"
}

const _FLOOR: Ingress.Shape = { maxDepth: 64, maxFrames: 4096, maxAssembledBytes: 268435456 }

const _probe = (root: unknown, ceiling: number): Ingress.Verdict => {
  // BOUNDARY ADAPTER: iterative identity-graph walk — the live-path set names cycles, the visited set prices sharing once; no mutable reference escapes
  const path = new Set<object>()
  const seen = new Set<object>()
  const stack: Array<readonly [unknown, number, boolean]> = [[root, 0, false]]
  while (stack.length > 0) {
    const entry = stack.pop()
    if (entry === undefined) break
    const [value, depth, leaving] = entry
    if (typeof value !== "object" || value === null) continue
    if (leaving) {
      path.delete(value)
      continue
    }
    if (path.has(value)) return "cycle"
    if (seen.has(value)) continue
    if (depth > ceiling) return "depth"
    path.add(value)
    seen.add(value)
    stack.push([value, depth, true])
    for (const child of Object.values(value as { readonly [key: string]: unknown })) stack.push([child, depth + 1, false])
  }
  return "clear"
}

const _gate = (budget: Ingress.Shape): Schema.Schema<unknown, unknown> =>
  Schema.transformOrFail(Schema.Unknown, Schema.Unknown, {
    strict: true,
    decode: (raw, _options, ast) =>
      pipe(_probe(raw, budget.maxDepth), (verdict) =>
        verdict === "clear"
          ? ParseResult.succeed(raw)
          : ParseResult.fail(new ParseResult.Type(ast, raw, verdict === "depth" ? `depth>${budget.maxDepth}` : "cycle"))),
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

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
