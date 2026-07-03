# [WIRE_PATCH]

`codec/patch.ts` is the RFC 6902 arm and the `EntityEdit` egress codec: the C#-authored `JsonPatchDocument` (`Rasm.Persistence/Version`) decodes once through a `Schema.Union` mirror of the six-operation vocabulary, applies over a defensive clone with the three engine faults folded onto the folder rail as values, and emits minimal patches whose optimistic-concurrency proof — `test` ops over the base pre-image — travels with the document. `rfc6902` stays the pure engine underneath; the Schema union is the boundary contract, and one `VoidableDiff` hook owns every domain reconciliation. This is the folder's one egress-authoring codec: the patch is TS-computed, C#-applied, and self-guarding across the version seam.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                   |
| :-----: | :---------------- | :---------------------------------------------------------------------------- |
|   [1]   | `OPERATION_UNION` | the six-op Schema mirror over the `op` literal discriminant                    |
|   [2]   | `APPLY`           | clone-fenced application; engine fault slots folded to `WireFault` reasons     |
|   [3]   | `EGRESS`          | minimal diff with the content-key hook, OCC `test` guards, the encode surface  |

## [2]-[OPERATION_UNION]

- Owner: the `_op` union — six struct rows discriminated on the `op` literal (a foreign field, never `_tag`), mirroring `rfc6902`'s `Operation` union member-for-member; `path`/`from` fields decode as RFC 6901 pointer strings whose escaping and traversal the engine's `Pointer` owns.
- Entry: `JsonPatch.FromJson` — the text-arrival decode fusing `JSON.parse` through `Schema.parseJson`; `JsonPatch.FromValue` — the already-parsed arrival; both land the same decoded document.
- Growth: the op set is closed by RFC 6902 — a seventh op is a standards event, not a growth axis; a new document envelope axis (a base version coordinate) is one field on the egress record in `[4]`.
- Law: the union is the boundary — `rfc6902`'s own types are `any`-typed at the edge, so no raw operation array crosses into `applyPatch` untyped and no decoded operation leaves this module as engine material; an op literal outside the closed six fails decode, and that failure is a drift signal surfaced through `contract/drift.ts`'s vocabulary, never a plain refusal.
- Law: `test` doubles as the OCC precondition — one row shape serves inbound preconditions and the egress guards `[4]` mints; one vocabulary, both directions.
- Law: destructiveness is the engine's own classifier — `isDestructive` binds directly; a local re-derivation over the op literal restates a shipped member.

```typescript
import { Schema } from "effect"

const _Pointer = Schema.String

const _op = Schema.Union(
  Schema.Struct({ op: Schema.Literal("add"), path: _Pointer, value: Schema.Unknown }),
  Schema.Struct({ op: Schema.Literal("remove"), path: _Pointer }),
  Schema.Struct({ op: Schema.Literal("replace"), path: _Pointer, value: Schema.Unknown }),
  Schema.Struct({ op: Schema.Literal("move"), from: _Pointer, path: _Pointer }),
  Schema.Struct({ op: Schema.Literal("copy"), from: _Pointer, path: _Pointer }),
  Schema.Struct({ op: Schema.Literal("test"), path: _Pointer, value: Schema.Unknown }),
)

const _document = Schema.Array(_op)
```

## [3]-[APPLY]

- Owner: the apply fold — clone before mutate, one result slot per op, the three engine fault classes triaged through a `Match.instanceOf` ladder onto `WireFault` reasons, the first non-null slot lifted onto the rail with its op index as evidence.
- Entry: `JsonPatch.apply(target, patch)` — returns the patched successor value; the input is never mutated because the engine applies over `clone(target)`.
- Receipt: a `stale` fault carries `{ actual, expected }` from the engine's `TestError` — the OCC drift report the rebase decision reads; a `conflict` carries the missing path; a `drift` carries the alien-op index.
- Growth: an apply policy axis (`implicitArrayCreation`) is one `_APPLY` options field threaded to every applier; never a per-call-site flag.
- Law: errors are values end to end — `applyPatch` returns one `(fault | null)` slot per op; the fold finds the first inhabited slot, the instance ladder maps `TestError -> stale`, `MissingError -> conflict`, residue -> `drift`, and the rail fails typed; imperative inspection or a re-`throw` of the returned `Error` instances is rejected.
- Law: the clone fence is the immutability law — the decoded value stays a value; the engine's in-place mutation dies inside this fold.
- Boundary: `stale`/`conflict` are non-quarantining reasons on `fault/quarantine.ts`'s policy table — a stale patch is a rebase decision, not a poison frame; the ladder is the sanctioned foreign-thrown triage form even though these instances return rather than throw.

```typescript
import { applyPatch } from "rfc6902"
import { InvalidOperationError, MissingError, TestError } from "rfc6902/patch"
import { clone } from "rfc6902/util"
import { Array, Effect, Match, Option, pipe } from "effect"
import { WireFault } from "../fault/quarantine.ts"

const _APPLY = { implicitArrayCreation: false } as const

const _classified: (slot: MissingError | TestError | InvalidOperationError, at: number) => WireFault = (slot, at) =>
  pipe(
    Match.value(slot),
    Match.when(Match.instanceOf(TestError), (miss) =>
      new WireFault({
        family: "JsonPatchDocument",
        reason: "stale",
        detail: `<test:${at}>`,
        evidence: Option.some({ actual: miss.actual, expected: miss.expected }),
      }),
    ),
    Match.when(Match.instanceOf(MissingError), (miss) =>
      new WireFault({ family: "JsonPatchDocument", reason: "conflict", detail: String(miss.path), evidence: Option.none() }),
    ),
    Match.orElse(() =>
      new WireFault({ family: "JsonPatchDocument", reason: "drift", detail: `<alien-op:${at}>`, evidence: Option.none() }),
    ),
  )

const _applied = (target: unknown, patch: JsonPatch.Document): Effect.Effect<unknown, WireFault> =>
  Effect.suspend(() => {
    const successor = clone(target)
    const slots = applyPatch(successor, [...patch], _APPLY)
    return Option.match(
      pipe(
        slots,
        Array.map((slot, index) => [slot, index] as const),
        Array.findFirst((pair): pair is readonly [MissingError | TestError | InvalidOperationError, number] => pair[0] !== null),
      ),
      {
        onNone: () => Effect.succeed(successor),
        onSome: ([slot, index]) => Effect.fail(_classified(slot, index)),
      },
    )
  })
```

## [4]-[EGRESS]

- Owner: `JsonPatch` — the assembled owner: decode surfaces, the apply rail, the engine's `isDestructive` bound directly, and the egress pair — `diff` under the one content-key reconciliation hook, `guarded` composing pre-image `test` ops ahead of the mutation ops.
- Entry: `JsonPatch.diff(base, next)` — the minimal operation set; `JsonPatch.guarded(base, next)` — the OCC form: `createTests` captures the pre-image so the C# journal append verifies before applying; `JsonPatch.encode` re-serializes to the C# `JsonPatchDocument` array; `JsonPatch.key` mints the patch's content identity through the kernel delegate over the encoded bytes.
- Receipt: the encoded patch is content-addressable — its key is the `EntityEdit` coordinate the append receipt echoes back.
- Growth: a new domain reconciliation (id-stable object families, ordinal-anchored sequences) is one arm inside `_reconciled`, returning ops or `undefined` to fall through to the engine's `diffAny` — never a per-shape differ roster, never post-processing of engine output.
- Law: the hook is singular and its first arm is the content-address shortcut — two records carrying string `key` fields compare by key alone: equal keys emit zero ops because key equality IS content equality under the one mint, unequal keys emit one whole-row `replace`; deep-diffing content-addressed rows re-derives what the address already proves.
- Law: an egress crossing the version seam ships its own proof — `guarded` is the default egress; an unconditional `diff` crossing is legal only where the caller owns a separate CAS token and states it.
- Law: engine output is local computation, not foreign ingress — `createPatch`/`createTests` results type structurally as the decoded document with zero runtime re-decode; decode-once binds ingress alone.
- Boundary: the OCC verify runs C#-side at the journal append; the version-vector coordinate a guard references decodes at `codec/version.ts`; the identity mint is `kernel`'s, delegated, never re-implemented.

```typescript
import { createPatch, createTests, isDestructive, type VoidableDiff } from "rfc6902"
import { ContentKey } from "@rasm/ts/kernel"
import { type ParseResult, Predicate } from "effect"

const _reconciled: VoidableDiff = (input, output, pointer) =>
  Predicate.hasProperty(input, "key") && Predicate.isString(input.key) &&
  Predicate.hasProperty(output, "key") && Predicate.isString(output.key)
    ? input.key === output.key
      ? []
      : [{ op: "replace", path: pointer.toString(), value: output }]
    : undefined

declare namespace JsonPatch {
  type Operation = Schema.Schema.Type<typeof _op>
  type Document = ReadonlyArray<Operation>
  type Shape = {
    readonly FromJson: Schema.Schema<Document, string>
    readonly FromValue: typeof _document
    readonly destructive: (operation: Operation) => boolean
    readonly apply: (target: unknown, patch: Document) => Effect.Effect<unknown, WireFault>
    readonly diff: (base: unknown, next: unknown) => Document
    readonly guarded: (base: unknown, next: unknown) => Document
    readonly encode: (patch: Document) => Effect.Effect<string, ParseResult.ParseError>
    readonly key: (patch: Document) => Effect.Effect<ContentKey, ParseResult.ParseError>
  }
}

const JsonPatch: JsonPatch.Shape = {
  FromJson: Schema.parseJson(_document),
  FromValue: _document,
  destructive: isDestructive,
  apply: _applied,
  diff: (base, next) => createPatch(base, next, _reconciled),
  guarded: (base, next) => {
    const mutation = createPatch(base, next, _reconciled)
    return [...createTests(base, mutation), ...mutation]
  },
  encode: (patch) => Schema.encode(JsonPatch.FromJson)(patch),
  key: (patch) =>
    Schema.encode(JsonPatch.FromJson)(patch).pipe(
      Effect.flatMap((text) => ContentKey.mint(new TextEncoder().encode(text))),
    ),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { JsonPatch }
```
