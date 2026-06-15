# Operation R Derived From Codecs Run: Each Method's Requirement Is The Union Of The Schemas It Actually Encodes And Decodes

[THE_OPERATION_R_IS_THE_SQLSCHEMA_TRIPLE_R_MINUS_THE_PROVIDED_CLIENT]:
- Each repository method is one `SqlSchema.{single,findOne,void}` call whose returned function is typed `(request: IA) => Effect<…, E | ParseError | …, R | IR | AR>` — the operation requirement is the THREE-WAY union of the `execute` effect's own `R`, the `Request` schema's encode requirement `IR`, and the `Result` schema's decode requirement `AR`. `insert` is `single({ Request: Model.insert, Result: Model, execute: sql`…returning *` })`, so its raw `R | IR | AR` is `SqlClient | S["insert"]["Context"] | S["Context"]`; the surfaced `S["Context"] | S["insert"]["Context"]` is that triple with `SqlClient` discharged at the enclosing `Effect.gen` scope where `const sql = yield* SqlClient` runs once. The per-op requirement is not a flat owner copy; it is the requirement-closure of the exact `Request`/`Result` pair the method binds.
- The `Request` schema is ALWAYS a write/lookup codec the method ENCODES (the payload going to the database), and the `Result` schema, when present, is ALWAYS the owner the method DECODES (the row coming back as `S["Type"]`). So `IR` is the encode-side requirement and `AR` the decode-side requirement, and the operation `R` is `encode-R ∪ decode-R` by construction — the requirement is two halves of one round trip, each half present only if that direction's codec runs. A method that encodes a payload but decodes no row carries `IR` and no `AR`; a method that decodes a row from a bare id carries `AR` and the id-leaf's `IR`.
- `S["insert"]["Context"]` and `S["update"]["Context"]` are themselves per-variant `Struct.Context` union folds over disjoint slot families, so `insert`'s `IR` and `update`'s `IR` differ exactly as their variant slot sets differ — a generative `insert`-slot service rides `insert`'s requirement and is structurally absent from `update`'s. The operation requirement inherits the variant-level disjointness rather than re-deriving it: the method binds `Model.insert` versus `Model.update`, and the union fold the variant already computed becomes the method's `IR` with no second traversal.

[DELETE_HAS_NO_RESULT_TERM_AND_THAT_ABSENCE_IS_THE_THEOREM]:
- `delete` binds `void({ Request: idSchema, execute: sql`delete …` })` — `void` is typed `<IR, II, IA, R, E>({ Request, execute }) => (request: IA) => Effect<void, E | ParseError, R | IR>` with NO `Result` type parameter and NO `AR` term. So `delete`'s requirement is `Schema.Schema.Context<S["fields"][Id]>` ALONE — it encodes the id through the id-leaf codec and discards every row, so the select-decode requirement `S["Context"]` is STRUCTURALLY ABSENT, not merely empty. This is the sharpest disproof of a flat-owner-R model: the same owner whose `findById` carries `S["Context"]` produces a `delete` that does not, because `delete` never instantiates a `Result` schema to fold a decode requirement from.
- `insertVoid` and `updateVoid` are the parallel proof on the WRITE side: each binds `void({ Request: Model.insert | Model.update, execute })` with no `Result`, so each carries `S["insert"]["Context"]` / `S["update"]["Context"]` (the payload still encodes) but DROPS the `S["Context"]` decode term that its row-returning sibling `insert`/`update` carries. The `…Void` suffix is not a knob on one method; it is a DIFFERENT `SqlSchema` constructor (`void` versus `single`) whose missing `Result` parameter deletes the `AR` union member — the requirement diff between `insert` and `insertVoid` is exactly `S["Context"]`, the decode requirement the void form never runs.
- `findById` binds `findOne({ Request: idSchema, Result: Model, execute })`, restoring the `AR = S["Context"]` term `delete` lacks, so `findById`'s requirement is `S["Context"] | Schema.Schema.Context<S["fields"][Id]>` — the owner select-decode UNIONED with the id-leaf's own encode requirement. A branded id whose brand carries no service contributes `never` to the id term; an id whose leaf codec is `Schema<A, I, Service>` surfaces `Service` on the read AND the delete (both encode the id) but on no write op (which encode the write variant, not the id). The id-leaf requirement is the one term shared by `findById` and `delete` and absent from every write method, because those two are the only methods whose `Request` is the id schema.

```typescript
import { Model } from '@effect/sql';
import { Context, Effect, Schema } from 'effect';

class Vault extends Context.Tag('Vault')<Vault, { readonly seal: (n: number) => Effect.Effect<number> }>() {}

const Keyed = Schema.transformOrFail(Schema.Number, Schema.Number, {
    strict: true,
    decode: (n) => Effect.succeed(n),
    encode: (n) => Effect.flatMap(Vault, (v) => v.seal(n)),
});

class Owner extends Model.Class<Owner>('Owner')({
    id: Model.Field({ select: Keyed, update: Keyed, json: Schema.Number }),
    label: Schema.NonEmptyTrimmedString,
}) {}

declare const repo: Effect.Effect.Success<ReturnType<typeof Model.makeRepository<typeof Owner, 'id'>>>;
type FindByIdR = Effect.Effect.Context<ReturnType<typeof repo.findById>>;
type DeleteR = Effect.Effect.Context<ReturnType<typeof repo.delete>>;
type UpdateR = Effect.Effect.Context<ReturnType<typeof repo.update>>;
```

- `id` carries `Vault` in `select`/`update` (its `encode` reads the service) and a context-free `Schema.Number` in `json` — so `Owner["Context"]` (the select fold) is `Vault`, and `Owner["update"]["Context"]` is `Vault`. `FindByIdR` is `Vault` (the `S["Context"]` select-decode term carries it), `UpdateR` is `Vault` (the `S["Context"]` decode term carries it, the `S["update"]["Context"]` encode term also), and `DeleteR` is `Schema.Schema.Context<Owner["fields"]["id"]>` — the id-leaf's own context — which is ALSO `Vault` only because the id leaf itself is `Keyed`. Move the service off the id leaf and onto a sibling `label`-style column: `delete`'s requirement drops to `never` while `findById`/`update` retain `Vault` through `S["Context"]`, the per-op requirement recomputed from which codecs each method binds, never restated. Reject a hand-written `type DeleteR = Vault`: `delete` binds no `Result`, so `Vault` reaches it only through the id leaf, and asserting the owner requirement onto it forks the source the next id-leaf edit silently desyncs.

[THE_ERROR_CHANNEL_IS_ALWAYS_NEVER_BECAUSE_EVERY_OP_PIPES_ORDIE]:
- Each method is `…Schema(payload).pipe(Effect.orDie, Effect.withSpan(…))`, and `Effect.orDie` is typed `<A, E, R>(self: Effect<A, E, R>) => Effect<A, never, R>` — it converts the ENTIRE typed error channel into a defect, leaving `R` untouched. The pre-`orDie` error alphabet is real and rich: `single` raises `SqlError | ParseError | Cause.NoSuchElementException`, `findOne` and `void` raise `SqlError | ParseError`. `orDie` collapses all of it to `never`, so the surfaced `Effect<…, never, …>` is the SAME requirement union with a defect-converted failure — the requirement channel is the only channel the variant codecs widen, the error channel they do not.
- The trade is deliberate and asymmetric: the SAME codec contributes its decode requirement to `R` (visible, providable) AND its `ParseError` to the defect channel (invisible, untyped). A malformed row failing `Result: Model` decode, a generative `Overrideable` whose `generate` returns `Effect.fail(new ParseResult.Forbidden(…))`, and a `NonEmptyTrimmedString` rejecting an empty wire value all surface as the SAME defect rather than as members of a typed `E` — the operation declares `never` because `orDie` ran, not because the codecs are infallible. A caller wanting a typed parse-failure rail re-introduces it by `Effect.catchAllDefect` over the operation or by not using the derived method at all, never by reading a typed `E` the surface refuses to expose.
- `NoSuchElementException` is the row-returning write's hidden failure: `single` (the `insert`/`update` constructor) raises it when the `returning *` clause yields no row, so an insert against a table whose trigger suppresses the return, or an update whose `where idColumn = …` matches nothing, is a DEFECT, not a typed empty result. `findById` uses `findOne` (returning `Option.Option<S["Type"]>`), so a missing row is `Option.none()` — a value, not a defect — while `update` uses `single`, so a missing target row is a defect. The empty-row semantics diverge by which `SqlSchema` constructor the method binds: `findOne` models absence as `Option`, `single` models it as `NoSuchElementException` that `orDie` then makes a defect.

```typescript
import { Model } from '@effect/sql';
import { Cause, Effect, ParseResult, Schema } from 'effect';

class Owner extends Model.Class<Owner>('Owner')({
    id: Model.Generated(Schema.Number),
    label: Schema.NonEmptyTrimmedString,
}) {}

const program = Effect.gen(function* () {
    const repo = yield* Model.makeRepository(Owner, { tableName: '<table>', spanPrefix: '<span>', idColumn: 'id' });
    const typedUpdate = (payload: (typeof Owner)['update']['Type']) =>
        repo.update(payload).pipe(
            Effect.catchAllDefect((defect) =>
                ParseResult.isParseError(defect)
                    ? Effect.fail({ _tag: 'DecodeFailed' as const, issue: defect.issue })
                    : Cause.isNoSuchElementException(defect)
                      ? Effect.fail({ _tag: 'RowMissing' as const })
                      : Effect.die(defect),
            ),
        );
    return typedUpdate;
});
```

- `typedUpdate` is the only path to a typed failure off a derived operation: the method's `E` is `never`, so the parse failure and the missing-row failure live in the defect channel, and `Effect.catchAllDefect` is the seam that re-types them — `ParseResult.isParseError` recovers the swallowed decode/encode `ParseError`, `Cause.isNoSuchElementException` recovers `single`'s empty-`returning` defect, every other defect re-died. Reject wrapping the operation in `try/catch` or reading a `.error` field: `orDie` moved the failure to the `Cause` defect track, so the typed rail is rebuilt by `catchAllDefect` over the `never`-failing method, the owner-derived surface trading a typed parse failure for a defect by design.

[THE_OUTER_CONSTRUCTOR_EFFECT_IS_THE_ONLY_PLACE_SQLCLIENT_IS_REQUIRED]:
- `makeRepository(Model, options)` is itself `Effect<{ …methods }, never, SqlClient>` — the constructor effect requires `SqlClient` (yielded once inside `Effect.gen`) and CANNOT fail (`never`), and the methods it returns no longer require `SqlClient` because the `sql` handle is closed over. So `SqlClient` is provided exactly once, at the layer that runs the constructor, and every method's `R` is the residual `S["Context"] | S[variant]["Context"]` after that discharge — the requirement partition is "ambient infrastructure provided at construction" versus "codec services provided per call site", and the codec services are the only requirement that survives onto the methods.
- `makeDataLoaders` is `Effect<{ …methods }, never, SqlClient | Scope>` — the constructor additionally requires `Scope` because each loader is a `SqlResolver.{ordered,findById,void}` wrapped in a `dataLoader` whose request-coalescing window owns a scoped daemon. The `Scope` rides the CONSTRUCTOR, not the methods, and the `window: DurationInput` plus `maxBatchSize?` are batching policy values consumed at construction — not call-site knobs — so a single owner declaration yields a batched surface whose lifetime attaches to the scope the constructor runs in, never to any individual method invocation.
- The dataloader methods are each `Effect<S["Type"]>` / `Effect<void>` — `R = never`, `E = never`, the fully-discharged form. The `R = never` is GUARANTEED by the `AnyNoContext` bound (every variant codec is `Schema<any, any, never>`, so no `IR`/`AR` term can be non-`never`) and the `SqlClient | Scope` discharge at construction; the `E = never` is the same `Effect.orDie` over each `resolver.makeExecute(loader)` call. The batched surface is the context-free, defect-erased terminal form of the same per-op codec-run derivation, the requirement collapsed to `never` because the bound forbids any codec service AND the constructor discharges the infrastructure.

[THE_IDCOLUMN_KEY_INTERSECTION_PARTITIONS_MARKERS_BY_LOOKUP_QUALIFICATION]:
- `Id` is constrained `(keyof S["Type"]) & (keyof S["update"]["Type"]) & (keyof S["fields"])` at the type parameter of BOTH `makeRepository` and `makeDataLoaders` — a three-way key intersection, not a `keyof S["Type"]` alone. A column qualifies to anchor lookup only when it survives the SELECT projection's key set (it must be readable to decode the returned row) AND the UPDATE projection's key set (it must be writable to address the row in an `update … where idColumn = …`) AND appear in the raw field map (its leaf codec must exist to encode the lookup value). The intersection is a membership predicate evaluated across two variant projections at once, the marker's `schemas` config deciding eligibility.
- The intersection partitions the marker zoo by lookup qualification with no flag: `Model.Generated` (`{ select, update, json }`) survives `keyof Type ∩ keyof update["Type"]` and qualifies; `Model.GeneratedByApp` (`{ select, insert, update, json }`) qualifies; `Model.Sensitive` (`{ select, insert, update }`, json-erased) still keeps `update` and qualifies; a bare `Schema.X` (all six) qualifies. A column shaped to SKIP `update` — a `FieldOnly("select", "insert")` insert-once marker, or any `Model.Field` whose `schemas` omits the `update` key — is STRUCTURALLY barred from anchoring lookup, its absent `update` key resolving the intersection to drop it, the disqualification a fact of the missing variant slot rather than a runtime check at the call. The qualifier is `update`-membership, not json-membership: a json-erased column still anchors lookup, while a json-present-but-update-absent column cannot.
- The id leaf the intersection selects is the EXACT schema two operations bind: `findById` and `delete` both run `SqlSchema.{findOne,void}({ Request: Model.fields[idColumn], … })`, so `Schema.Schema.Type<S["fields"][Id]>` is their parameter type and `Schema.Schema.Context<S["fields"][Id]>` is the id-leaf requirement term in their `R`. The constraint and the per-op requirement are two reads of one fact: the `Id` intersection picks WHICH leaf anchors lookup, and the same leaf's `Context` becomes the requirement contribution of every id-keyed method — the eligibility predicate and the requirement derivation both read `S["fields"][Id]`.
- In the dataloader path the id leaf additionally anchors the BATCH KEY: `SqlResolver.findById` reads `ResultId(request) { return request[idColumn] }` to coalesce concurrent lookups by id and `SqlResolver.void` for `delete` batches `sql.in(idColumn, ids)`, so the same `idColumn` the intersection qualifies for addressing also partitions the request-coalescing window. A non-updatable column barred from anchoring lookup is therefore barred from being a batch key as well — one intersection constraint governs the lookup address, the per-op id requirement, and the batch-coalescing key in a single declaration.

```typescript
import { Model } from '@effect/sql';
import { Schema } from 'effect';

const RowId = Schema.Number.pipe(Schema.brand('RowId'));

class Owner extends Model.Class<Owner>('Owner')({
    id: Model.Generated(RowId),
    onceOnly: Model.FieldOnly('select', 'insert')(Schema.Number),
    label: Schema.NonEmptyTrimmedString,
}) {}

const repo = Model.makeRepository(Owner, { tableName: '<table>', spanPrefix: '<span>', idColumn: 'id' });
type EligibleId = 'id' & keyof (typeof Owner)['Type'] & keyof (typeof Owner)['update']['Type'] & keyof (typeof Owner)['fields'];
type RejectedId = 'onceOnly' & keyof (typeof Owner)['Type'] & keyof (typeof Owner)['update']['Type'] & keyof (typeof Owner)['fields'];
```

- `idColumn: 'id'` type-checks because `Model.Generated` keeps `id` in BOTH `select` and `update`, so `'id'` survives the three-way intersection — `EligibleId` resolves `'id'`. `idColumn: 'onceOnly'` is a compile error: `Model.FieldOnly('select', 'insert')` omits `update`, so `'onceOnly'` is absent from `keyof Owner["update"]["Type"]` and the intersection drops it — `RejectedId` resolves `never`, the type-level proof a non-updatable column cannot anchor lookup. Reject a `idColumn: 'onceOnly' as 'id'` cast: the constraint exists because `update`'s `where idColumn = …` clause must address the row, and an insert-once column has no `update` slot to address through; the marker's variant membership IS the lookup-qualification gate, the cast a defeat of the addressing invariant the constraint encodes.

[THE_DERIVED_SURFACE_IS_ONE_OWNER_FANNED_INTO_SIX_CODEC_BINDINGS]:
- The whole repository is one `Effect.gen` that reads ONE owner and binds six `SqlSchema` calls, each pairing a variant codec with a SQL template — `insert`↔`single(Model.insert, Model)`, `insertVoid`↔`void(Model.insert)`, `update`↔`single(Model.update, Model)`, `updateVoid`↔`void(Model.update)`, `findById`↔`findOne(idSchema, Model)`, `delete`↔`void(idSchema)`. No method authors its own decode/encode; each delegates to the variant the owner already carries, so the entire CRUD surface is a derivation off the variant family, the SQL template the only per-op authored fragment. A seventh operation lands as one more `SqlSchema` binding over a variant projection, not a new model.
- The requirement of each method is therefore a THEOREM off variant membership, recomputed at compile time when the owner changes: adding a service-requiring leaf to one variant reshapes exactly the methods whose bound codecs retain that leaf, every other method's `R` unchanged, and the affected call sites break loudly while the rest absorb it silently. The proof of the derivation being correct is the next column's diff — a `Model.Sensitive(serviceLeaf)` adds the service to `insert`/`update`/`findById` (the methods binding `select`/`insert`/`update` codecs that retain it) and to NEITHER `delete` (no `Result`) NOR the json-keyed surfaces, the per-op requirement union recomputed off the widened field map with no method respelled.
- The two surfaces partition by requirement closure exactly as the per-op derivation predicts: `makeRepository<S extends Model.Any>` admits a service-carrying owner and surfaces the residual requirement per method, while `makeDataLoaders<S extends Model.AnyNoContext>` rejects any owner whose variant codecs are not all `Schema<any, any, never>`. The SAME owner drives both only when every codec's per-variant fold resolves `never` — a generative slot reading a service is admissible to the unbatched path (its requirement surfaces on the write methods) and silently disqualifies the batched path at the type parameter, the bound reading the field map's requirement closure that the per-op union folds already computed.
