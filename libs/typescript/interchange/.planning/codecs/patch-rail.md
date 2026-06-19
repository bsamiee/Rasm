# [INTERCHANGE_PATCH_RAIL]

The recorded-intent partial-update face of the wire boundary: the codec that admits a recorded mutation against an already-admitted value rather than a whole value. `PatchRail` is the partial-update sibling of `codecs/decode-rail#DECODE_RAIL` — `DecodeRail` decodes bytes into a typed whole value, `PatchRail` decodes a recorded `JsonPatchDocument`/`FieldMask` and replays it against an already-admitted clone, the two co-located in `codecs/` as the whole-value and partial-update faces of one decode discipline. The rail mines `rfc6902` end-to-end: the six-verb `Operation` union decodes at the `PatchOpWire` `Schema.Union` boundary so an unrecognized `op` faults as `quarantine/drift-terminal#QUARANTINE` drift before any mutation lands, never as a post-apply `InvalidOperationError` slot; `applyPatch` runs error-accumulating against a structural clone and its one-nullable-slot-per-op result folds into ONE `faults/fault-family#FAULT_FAMILY` `FaultDetail.HopFault` keyed by the failing operation index; `createTests` builds the optimistic-concurrency `test`-op prefix that self-guards a replayed or stale apply; and the proto `FieldMask` sparse-update lowers through `fieldMaskLower` so the binary partial-update and the json-stj patch reach the identical apply owner. The `JsonPointer` RFC-6901 token-path brand the `path`/`from` slots carry is the `refinement/schema-refinement#REFINEMENT` row, so a raw string never enters a pointer slot. The patched value re-enters `quarantine/drift-terminal#QUARANTINE` admission as a whole value — the mutation is recorded intent off the wire, never a re-derived diff at the consumer and never a straight binding to a render surface, mirroring the C# producer law at `csharp:Rasm.AppHost/configuration/configuration-and-options#TS_PROJECTION` that a patch applies to a value projection and re-binds through the section's own admission.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                          |
| :-----: | :------------- | :----------------------------------------------------------------------------- |
|   [2]   | PATCH_RAIL     | the six-arm `PatchOpWire` union, the `applyPatch` fold, the `createTests` guard, the `fieldMaskLower` |
|   [3]   | TS_PROJECTION  | the C# `JsonPatchDocument` operation wire and the proto `FieldMask` shapes the rail decodes |

## [2]-[PATCH_RAIL]

- Owner: `PatchRail`, the recorded-intent partial-update owner composing `rfc6902` to full capability — one `PatchOpWire` `Schema.Union` over the six op verbs decodes the wire patch, one `applyPatch` fold replays it against a clone and folds the error-accumulating result into one `FaultDetail.HopFault`, one `createTests` fold builds the optimistic-concurrency prefix, and one `fieldMaskLower` fold lowers the proto `FieldMask` sparse-update into the same `Operation[]` the json-stj patch carries. One owner carries decode, guard, lower, and apply; a parallel patch object per update modality is the named defect, and a state-diff re-derived at the consumer through `createPatch` is the rejected form because `createPatch` is the `rfc6902` development-time diff generator, never a contract decoder.
- Cases: the six op verbs are the `rfc6902` `Operation` union — `add`/`replace`/`test` carry `path` plus `value`, `move`/`copy` carry `from` plus `path`, `remove` carries `path` alone; the union decodes exhaustively over the `op` discriminant at the boundary so an unrecognized verb faults as drift, never reaching `applyPatch` as an `InvalidOperationError` slot. The two partial-update modalities reach one apply owner: the json-stj `JsonPatchDocument` decodes directly as the `Operation[]`, and the proto `FieldMask.paths` repeated dotted-path set lowers through `fieldMaskLower` into a `replace`-op per masked path resolving its value out of the sparse-update message, so the binary sparse-update and the recorded JSON patch share `applyPatch` rather than two parallel replay kernels. `createTests` reads the touched-path set of an incoming patch and emits the `test`-op prefix asserting the pre-patch state, so a patch prefixed with the guard self-aborts on a stale or replayed apply through a `TestError` rather than corrupting the clone.
- Entry: `applyRecordedIntent(value, patch)` is the one polymorphic partial-update entry — it structurally clones the admitted value (`rfc6902` `applyPatch` mutates the target in place, so the pre-patch value survives only off a clone), decodes the wire patch through `Schema.decodeUnknown(Schema.Array(PatchOpWire))` so an unknown `op` faults at the boundary, prepends the `createTests` guard prefix, runs `applyPatch` against the clone over the guard-prefixed `[...guard, ...patch]` op list, and folds the `(MissingError | TestError | InvalidOperationError | null)[]` result one-slot-per-op into one `FaultDetail.HopFault` keyed by the first non-null slot's array index resolved against the same applied op list — the failing op's `op` and `path` ride the evidence row, a guard-prefix slot reports `patch-test-mismatch` with `phase: "precondition"` and a patch slot reports `patch-apply-failed`/`patch-test-mismatch` with `phase: "apply"`, and the `index` evidence is the patch-relative op position. A clean run returns the mutated clone; a non-null slot faults rather than returning a half-mutated value, so a `test`-op mismatch faults before the mutating ops downstream of it observe the clone. The `FieldMask` modality enters the same entry through `fieldMaskLower` producing the `Operation[]` the fold then applies, so the entry is one surface keyed by the wire shape, never a `applyPatch`/`applyFieldMask` pair.
- Packages: `rfc6902` for `applyPatch` (error-accumulating, one nullable slot per op, mutates in place — VERIFIED `.api/rfc6902.md` line 56), `createTests` (the `TestOperation[]` guard builder — VERIFIED line 59), `Pointer.fromJSON` (the RFC-6901 token-path parser the `JsonPointer` brand re-uses for the escape-form validation — VERIFIED lines 78, 105), and the six-verb `Operation` union (VERIFIED lines 20-27); `@bufbuild/protobuf/wkt` for `FieldMask` (the `paths: string[]` repeated dotted-path set — VERIFIED `.api/bufbuild-protobuf.md` line 70); `effect` for `Schema.Union`/`Schema.Literal`/`Schema.decodeUnknown` at the six-arm boundary decode and `Schema.Array` over the patch.
- Growth: a new partial-update modality lands as one lower fold producing the shared `Operation[]`, never a parallel apply kernel; a new op verb is impossible — the RFC-6902 verb set is the closed package-owned six, so a seventh `op` is a wire drift the `PatchOpWire` union faults at the boundary, not a new arm; a new fault discriminant for a patch failure lands as one `HopReason` literal on the `faults/fault-family#FAULT_FAMILY` closed vocabulary, never a code-keyed `HopFault`.
- Boundary: the rail decodes recorded intent and re-derives nothing — `createPatch` is the `rfc6902` development-time diff generator (VERIFIED line 58) and a contract patch decoded off the wire is never a re-derived diff at the consumer; `applyPatch` mutates in place (VERIFIED line 56) so the rail clones before apply and a `test`-op mismatch faults rather than corrupts; an unknown `op` faults at the `Schema.Union` boundary as drift (VERIFIED `.api/rfc6902.md` line 110), never as a post-apply `InvalidOperationError` slot after a partial mutation; the patched clone re-enters `quarantine/drift-terminal#QUARANTINE` admission as a whole value, never a straight binding to a render surface; the `path`/`from` slots carry the `refinement/schema-refinement#REFINEMENT` `JsonPointer` brand so a raw unescaped string never enters a pointer slot; operation order is load-bearing (`move`/`copy` evaluate `from` against the post-prior-operation state — VERIFIED line 102) so the rail never reorders the decoded `Operation[]`.

```ts contract
// --- [TYPES] -------------------------------------------------------------------------
type PatchOp = "add" | "remove" | "replace" | "move" | "copy" | "test";

// --- [MODELS] ------------------------------------------------------------------------
const PatchOpWire = Schema.Union(
  Schema.Struct({ op: Schema.Literal("add"), path: JsonPointer, value: Schema.Unknown }),
  Schema.Struct({ op: Schema.Literal("remove"), path: JsonPointer }),
  Schema.Struct({ op: Schema.Literal("replace"), path: JsonPointer, value: Schema.Unknown }),
  Schema.Struct({ op: Schema.Literal("move"), from: JsonPointer, path: JsonPointer }),
  Schema.Struct({ op: Schema.Literal("copy"), from: JsonPointer, path: JsonPointer }),
  Schema.Struct({ op: Schema.Literal("test"), path: JsonPointer, value: Schema.Unknown }),
);
type PatchOpWire = Schema.Schema.Type<typeof PatchOpWire>;

const PatchWire = Schema.Array(PatchOpWire);
type PatchWire = Schema.Schema.Type<typeof PatchWire>;

// --- [OPERATIONS] --------------------------------------------------------------------
const decodePatch = (raw: unknown): Effect.Effect<PatchWire, ParseResult.ParseError> =>
  Schema.decodeUnknown(PatchWire)(raw); // an unknown op faults here as drift, never as a post-apply InvalidOperationError slot

const createTests = (value: unknown, patch: PatchWire): ReadonlyArray<TestOperation> =>
  rfc6902.createTests(value, patch as ReadonlyArray<Operation>); // the optimistic-concurrency test-op prefix asserting the pre-patch state of touched paths

const slotFault = (
  results: ReadonlyArray<MissingError | TestError | InvalidOperationError | null>,
  applied: ReadonlyArray<Operation>, // the full [...guard, ...patch] op list applyPatch ran against, so the slot index resolves the exact failing op
  guardCount: number, // the guard-prefix length; an index below it is an optimistic-concurrency precondition failure, not a mutating-op failure
): Option.Option<FaultDetail> =>
  pipe(
    Array.findFirstIndex(results, (slot) => slot !== null), // the error-accumulating result is one nullable slot per op; the first non-null slot is the failing operation
    Option.map((index) => {
      const failing = applied[index];
      return FaultDetail.HopFault({
        reason: index < guardCount || results[index] instanceof TestError ? "patch-test-mismatch" : "patch-apply-failed",
        evidence: {
          index: String(index < guardCount ? index : index - guardCount), // report the patch-relative op index; a guard failure reports its prefix index
          phase: index < guardCount ? "precondition" : "apply",
          op: failing.op,
          path: failing.path,
          detail: (results[index] as MissingError | TestError | InvalidOperationError).message,
        },
      });
    }),
  );

const applyRecordedIntent = (value: unknown, raw: unknown): Effect.Effect<unknown, FaultDetail | ParseResult.ParseError> =>
  decodePatch(raw).pipe(
    Effect.flatMap((patch) =>
      Effect.sync(() => {
        const clone = structuredClone(value); // applyPatch mutates in place; the pre-patch value survives only off a clone
        const guard = createTests(clone, patch);
        const applied = [...guard, ...patch] as ReadonlyArray<Operation>; // the guard-prefixed op list, the exact array applyPatch indexes its result slots against
        const results = rfc6902.applyPatch(clone, applied); // test-op prefix self-guards a replayed or stale apply
        return { clone, fault: slotFault(results, applied, guard.length) };
      })),
    Effect.flatMap(({ clone, fault }) =>
      Option.match(fault, { onNone: () => Effect.succeed(clone), onSome: Effect.fail })), // a non-null slot faults rather than returning a half-mutated clone
  );
```

The `FieldMask` modality reaches `applyRecordedIntent` through the same `Operation[]` the json-stj patch carries — `fieldMaskLower` reads the `FieldMask.paths` repeated dotted-path set, resolves each masked path's value out of the sparse-update message through the RFC-6901 `Pointer`, and lowers it into a `replace`-op so the binary sparse-update and the recorded JSON patch share the one apply owner rather than two parallel replay kernels; an empty mask faults as `field-mask-empty` so a no-op sparse update never silently passes.

```ts contract
const dottedToPointer = (dotted: string): string =>
  `/${dotted.split(".").map((token) => token.replace(/~/g, "~0").replace(/\//g, "~1")).join("/")}`; // RFC-6901 escape: ~ -> ~0, / -> ~1 (VERIFIED .api/rfc6902.md line 105) so a FieldMask dotted path crosses to a token path

const fieldMaskLower = (mask: FieldMask, message: Record<string, unknown>): Effect.Effect<PatchWire, FaultDetail | ParseResult.ParseError> =>
  mask.paths.length === 0
    ? Effect.fail(FaultDetail.HopFault({ reason: "field-mask-empty", evidence: {} }))
    : Schema.decodeUnknown(PatchWire)( // re-admit through the same six-arm boundary so a lowered op carries the JsonPointer brand
        mask.paths.map((dotted) => {
          const pointer = dottedToPointer(dotted);
          return { op: "replace", path: pointer, value: Pointer.fromJSON(pointer).get(message) }; // each masked path resolves its value out of the sparse-update message
        }),
      );

const applyFieldMask = (value: unknown, mask: FieldMask, message: Record<string, unknown>): Effect.Effect<unknown, FaultDetail | ParseResult.ParseError> =>
  fieldMaskLower(mask, message).pipe(Effect.flatMap((patch) => applyRecordedIntent(value, patch))); // one apply owner, never a parallel applyFieldMask replay kernel
```

## [3]-[TS_PROJECTION]

- Owner: the recorded-intent partial-update wire shapes the rail decodes — the RFC-6902 `JsonPatchDocument` operation model sourced from `csharp:Rasm.AppHost/configuration/configuration-and-options#TS_PROJECTION` (the `PatchSection` route's `application/json-patch+json` document carrying the package-owned `op`/`path`/`from`/`value` operation model and the `Test`-op precondition assertion), and the proto `FieldMask` sparse-update sourced from `@bufbuild/protobuf/wkt`'s `google.protobuf.FieldMask` (`paths: string[]`). The C# producer is the single mint of the patch operation vocabulary — the `Microsoft.AspNetCore.JsonPatch` `Operations.Operation` model with the six-verb `op` set and the whole-document `Test`-op precondition law — transcribed here as the `PatchOpWire` union, never re-authored as a branch-side state-diff endpoint. The `FieldMask` is the proto well-known type both branches own through `@bufbuild/protobuf/wkt`, so the binary sparse-update needs no branch-side mint either.
- Cases: the `JsonPatchDocument` crosses as the RFC-6902 array body — each element an `op`-discriminated object whose `path`/`from` are RFC-6901 `/`-joined token paths and whose `value` is the verb's payload, the same shape the C# `JsonPatchDocument.ApplyTo(JsonObject, logErrorAction)` reads against the live value projection; the `Test`-op precondition that fails the whole patch before any mutation lands is the `createTests`/`test`-op self-guard the rail prepends. The proto `FieldMask` crosses as `paths: string[]` of dotted field paths (`a.b.c`) per the protobuf field-mask convention, lowered to RFC-6901 token paths through `dottedToPointer` before the shared apply. The C# write-back disposition vocabulary (`WriteBackWire`) and the binding-status wire are NOT this page's projection — they are decoded at `codecs/decode-rail#BCF_LIVE_WIRE_DECODE` as whole values; this page owns only the partial-update operation shapes that mutate an already-admitted value, the recorded-intent face of the same `Rasm.AppHost` producer.
- Packages: `effect` `Schema.Struct`/`Schema.Union`/`Schema.Literal`/`Schema.Array` for the operation-union surface; `@bufbuild/protobuf/wkt` `FieldMask`/`FieldMaskSchema` for the proto sparse-update (the `FieldMaskSchema` descriptor backs `fromBinary`/`fromJson` of the mask — VERIFIED `.api/bufbuild-protobuf.md` line 71); `rfc6902` `Pointer.fromJSON` for the RFC-6901 path resolution.
- Boundary: the operation model is the C# `Microsoft.AspNetCore.JsonPatch` package's, transcribed not re-spelled — a branch-side `op` enum, a Newtonsoft-shaped operation, or a hand-rolled RFC-6902 dispatch is the deleted form, the same deletion the C# producer carries (a hand-rolled RFC-6902 operation dispatch and a Newtonsoft `JsonPatchDocument` are the producer's named deleted forms); the `FieldMask` is the proto well-known type, never a branch-minted mask record; the patch `value` slot is `Schema.Unknown` because the operation model is type-agnostic over its target — the value re-admits through the patched whole value's own decode at `quarantine/drift-terminal#QUARANTINE`, not through a per-op value schema, so the rail never re-mints the target model per op-path; the `path`/`from` carry the `refinement/schema-refinement#REFINEMENT` `JsonPointer` brand, so the wire string is admitted as an RFC-6901 token path at decode rather than trusted raw.

```ts contract
// --- [MODELS] ------------------------------------------------------------------------
// The JsonPatchDocument operation wire IS the [2]-[PATCH_RAIL] PatchOpWire union — the
// C# Microsoft.AspNetCore.JsonPatch Operations.Operation model (op/path/from/value)
// transcribed verbatim as the six-arm Schema.Union, never a second operation shape on
// this page. The FieldMask sparse-update is the proto well-known type, decoded through
// the @bufbuild/protobuf/wkt descriptor and lowered to the SAME union.

const FieldMaskWire = Schema.Struct({
  paths: Schema.Array(Schema.String), // google.protobuf.FieldMask: repeated dotted-path set (VERIFIED .api/bufbuild-protobuf.md line 70)
});
type FieldMaskWire = Schema.Schema.Type<typeof FieldMaskWire>;

// --- [OPERATIONS] --------------------------------------------------------------------
const decodeJsonPatch = (bytes: Uint8Array): Effect.Effect<PatchWire, ParseResult.ParseError> =>
  Schema.decodeUnknown(PatchWire)(JSON.parse(new TextDecoder().decode(bytes))); // the application/json-patch+json body decoded as the six-arm union

const decodeFieldMask = (bytes: Uint8Array): Effect.Effect<FieldMask, ParseResult.ParseError> =>
  Schema.decodeUnknown(FieldMaskWire)(JSON.parse(new TextDecoder().decode(bytes))).pipe(
    Effect.map((wire) => create(FieldMaskSchema, { paths: [...wire.paths] })), // the proto FieldMask reconstructed through its own descriptor, never a hand-rolled mask record
  );
```
