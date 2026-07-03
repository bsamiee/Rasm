# [SERVICES_EMBEDDING]

The embedding-lifecycle owner — `EmbeddingLifecycle`, the re-embed scheduling, the embedding-profile MIGRATION, and the staleness-driven refresh ORCHESTRATION the `folder:search/rank#FUSED_RANK` semantic arm depends on but does not own. It admits ONLY the cross-profile concern the search arm lacks: `fused-rank` already owns the staleness column, the single re-embed `folder:persistence/work#WORK_AND_SIGNALS` `WorkQueue` row, and the `EmbeddingProfile { model, dimensions, efSearch }` shape — this owner adds the durable MIGRATION between profiles and the batched embed ORCHESTRATION that feeds the search arm fresh vectors. The batched embed rides the `@effect/ai` `EmbeddingModel.makeDataLoader` windowed data-loader and the `EmbeddingModel.make` per-call cache, the exact admitted surface no page drives today, with one entrypoint owning singular, batch, and stream by input shape. The re-embed runs as a `folder:execution/backplane#RUNNER_AND_SCHEDULING` durable job; profiles size ONLY the `pgvector` column, never the gated BM25 lexical index. The owner rides the one `PgClient` and crosses no .NET wire.

## [01]-[INDEX]

- [01]-[EMBEDDING_LIFECYCLE]: owns the `EmbeddingProfile` migration-row table, the `BatchEmbed` data-loader-plus-cache entrypoint, the durable re-embed schedule, and the boundary against `fused-rank`'s staleness column and the gated BM25 arm.

## [02]-[EMBEDDING_LIFECYCLE]

- Owner: `EmbeddingLifecycle`, the single cross-profile orchestration owner — `EmbeddingProfile` migration rows recording the dimension/model each profile sizes the `pgvector` column at, `BatchEmbed` the one embed entrypoint over the `@effect/ai` data-loader and cache, and the durable re-embed schedule that re-vectors a corpus when a profile migrates or a row goes stale. One owner over the embedding cross-profile concern, never a parallel migration service per model.
- Cases: `EmbeddingProfile` is one migration-row `Model.Class` — the profile name, the model, the `pgvector` dimensions, the HNSW `efSearch`, a `pending`/`embedding`/`active`/`retired` migration status, and the from/to profile pointers — so a model upgrade is one new profile row in `pending` and the migration drains the corpus from the old profile's vector column to the new, never an ad-hoc re-embed loop. `BatchEmbed` is one entrypoint owning singular/batch/stream by input shape: a single id rides `EmbeddingModel.embed`, a bounded set rides `EmbeddingModel.embedMany`, and the corpus-scale migration drains a `Stream` through the `EmbeddingModel.makeDataLoader` windowed data-loader (`{ window, maxBatchSize }`) so concurrent embed calls coalesce into one provider round-trip per window, with the `EmbeddingModel.make` per-call cache (`{ capacity, timeToLive }`) short-circuiting a repeated text; the discriminant is the input's shape, never a `batch`/`mode` flag beside it. The migration fold drains the `pending` profile's rows, embeds each through the data-loader against the new model, writes the new vector column, and flips the row's profile pointer; the staleness refresh re-embeds a row `fused-rank` flagged stale (the `embedding_model <> profile.model` column the search arm already owns) and writes the fresh vector back. The lifecycle feeds `fused-rank` the fresh embeddings and the profile dimension; the search arm reads the column, this owner writes it.
- Entry: the owner rides the one `folder:persistence/store#STORE_BOUNDARY` `PgClient` — the `EmbeddingProfile` migration rows and the staleness/profile columns are columns on the searched entity's `Model.Class` over the one `SqlClient`, tenant-scoped through the `folder:persistence/tenancy#TENANCY` GUC; the re-embed runs as a `folder:execution/backplane#RUNNER_AND_SCHEDULING` durable job (a shard-pinned schedule draining the `pending` migration and the stale rows), distinct from the single per-row re-embed `WorkQueue` row `fused-rank` already references — this owner orchestrates the corpus-scale migration the per-row row does not; the `@effect/ai` `EmbeddingModel` is the provider surface the `folder:execution/ai#AI_ACTIVITY` `AiProvider` axis selects, so the embed runs through the one provider boundary, never a second AI client.
- Wire: the owner crosses no .NET wire — the embeddings, the profile migration, and the staleness refresh are node-internal over the one `PgClient`; the embed call reaches the `@effect/ai` provider, itself a node owner, never a decoded wire contract.
- Packages: `@effect/ai` for `EmbeddingModel.makeDataLoader` (the windowed batched embed `{ window, maxBatchSize }`), `EmbeddingModel.make` (the per-call cache `{ capacity, timeToLive }`), and `embed`/`embedMany` for the singular/batch arms — the admitted surface no page drives today; `@effect/sql` and `@effect/sql-pg` for the profile/staleness columns over the one `PgClient`; `@effect/workflow` for the durable re-embed schedule; `effect` for the `Stream` migration drain and the input-shape dispatch.
- Growth: a new embedding model lands as one `EmbeddingProfile` migration row in `pending`, never a parallel migration service; a new staleness rule lands as one column predicate the refresh reads; a new embed modality is absorbed by the one `BatchEmbed` entrypoint's input-shape dispatch, never a sibling embed method; the migration schedule lands as one `runtime-backplane` durable job row.
- Boundary: the named defects — re-owning `fused-rank`'s staleness column or the single re-embed `WorkQueue` row instead of consuming them (this owner adds ONLY the profile migration and the corpus-scale batch orchestration); a parallel embed client beside the one `@effect/ai` `EmbeddingModel` provider; a `batch`/`mode` flag beside the embed value instead of input-shape dispatch through `makeDataLoader`; an eager per-call embed instead of the windowed data-loader coalesce plus the cache short-circuit; profiling the gated BM25 lexical index — the profile dimensions size ONLY the `pgvector` column, and the `pg_search`/ParadeDB/Tiger BM25 extension stays the `fused-rank` RESEARCH item gated on `folder:provisioning/contract#PROVISIONING` provisioning the BM25-bearing Postgres image, so this owner feeds the UNBLOCKED vector arm and never the gated lexical arm. This is a node-only surface, never browser-reachable.

```ts contract
import type { PgClient } from "@effect/sql-pg"
import type { SqlError } from "@effect/sql"
import { EmbeddingModel } from "@effect/ai"
import type { AiError } from "@effect/ai"
import { Model, SqlClient } from "@effect/sql"
import { Data, Duration, Effect, Layer, Schema as S, Stream } from "effect"

// --- [MODELS] --------------------------------------------------------------------------

class EmbeddingProfile extends Model.Class<EmbeddingProfile>("EmbeddingProfile")({
  profile: S.String,
  model: S.String,
  dimensions: S.Number.pipe(S.int(), S.positive()),
  efSearch: S.Number.pipe(S.int(), S.positive()),
  status: S.Literal("pending", "embedding", "active", "retired"),
  fromProfile: Model.FieldOption(S.String),
  createdAt: Model.DateTimeInsert,
}) {}

// --- [TYPES] ---------------------------------------------------------------------------

type EmbedInput = Data.TaggedEnum<{
  readonly One:    { readonly id: string; readonly text: string }
  readonly Many:   { readonly rows: ReadonlyArray<{ readonly id: string; readonly text: string }> }
  readonly Corpus: { readonly profile: string; readonly source: Stream.Stream<{ readonly id: string; readonly text: string }, SqlError.SqlError> }
}>
const EmbedInput = Data.taggedEnum<EmbedInput>()

// --- [ERRORS] --------------------------------------------------------------------------

class LifecycleFault extends S.TaggedError<LifecycleFault>()("LifecycleFault", {
  profile: S.String,
  stage: S.Literal("embed", "migrate", "persist"),
  cause: S.Unknown,
}) {}

// --- [SERVICES] ------------------------------------------------------------------------

class EmbeddingLifecycle extends Effect.Service<EmbeddingLifecycle>()("services/EmbeddingLifecycle", {
  accessors: true,
  effect: Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const model = yield* EmbeddingModel.EmbeddingModel
    const loader = yield* EmbeddingModel.makeDataLoader({
      embedMany: (input) => model.embedMany(input).pipe(Effect.map((vectors) => vectors.map((embeddings, index) => ({ index, embeddings })))),
      window: Duration.millis(50),
      maxBatchSize: 256,
    })

    const persist = (profile: string, id: string, vector: ReadonlyArray<number>): Effect.Effect<void, LifecycleFault> =>
      sql`UPDATE searchable SET embedding = ${vector}::vector, embedding_model = (SELECT model FROM embedding_profile WHERE profile = ${profile}) WHERE id = ${id}`.pipe(
        Effect.asVoid,
        Effect.mapError((cause) => new LifecycleFault({ profile, stage: "persist", cause })),
      )

    const embedOne = (profile: string, id: string, text: string): Effect.Effect<void, LifecycleFault> =>
      loader.embed(text).pipe(
        Effect.mapError((cause: AiError.AiError) => new LifecycleFault({ profile, stage: "embed", cause })),
        Effect.flatMap((vector) => persist(profile, id, vector)),
      )

    const embed = (input: EmbedInput): Effect.Effect<number, LifecycleFault | SqlError.SqlError> =>
      EmbedInput.$match(input, {
        One:    ({ id, text }) => embedOne("active", id, text).pipe(Effect.as(1)),
        Many:   ({ rows })     => Effect.forEach(rows, ({ id, text }) => embedOne("active", id, text), { concurrency: "inherit" }).pipe(Effect.as(rows.length)),
        Corpus: ({ profile, source }) =>
          source.pipe(
            Stream.mapEffect(({ id, text }) => embedOne(profile, id, text), { concurrency: "unbounded" }),
            Stream.runCount,
          ),
      })

    const migrate = (from: string, to: string): Effect.Effect<number, LifecycleFault | SqlError.SqlError> =>
      sql`UPDATE embedding_profile SET status = 'embedding' WHERE profile = ${to}`.pipe(
        Effect.flatMap(() =>
          embed(EmbedInput.Corpus({
            profile: to,
            source: sql<{ id: string; text: string }>`SELECT id, label AS text FROM searchable WHERE embedding_model = (SELECT model FROM embedding_profile WHERE profile = ${from})`.pipe(Stream.fromIterableEffect),
          })),
        ),
        Effect.tap(() => sql`UPDATE embedding_profile SET status = 'active' WHERE profile = ${to}`),
        Effect.tap(() => sql`UPDATE embedding_profile SET status = 'retired' WHERE profile = ${from}`),
      )

    const refreshStale = (profile: string): Effect.Effect<number, LifecycleFault | SqlError.SqlError> =>
      embed(EmbedInput.Corpus({
        profile,
        source: sql<{ id: string; text: string }>`SELECT s.id, s.label AS text FROM searchable s, embedding_profile p WHERE p.profile = ${profile} AND p.status = 'active' AND s.embedding_model <> p.model`.pipe(Stream.fromIterableEffect),
      }))

    return { embed, migrate, refreshStale } as const
  }),
}) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const EmbeddingLifecycleLayer: Layer.Layer<EmbeddingLifecycle, never, SqlClient.SqlClient | PgClient.PgClient | EmbeddingModel.EmbeddingModel> =
  EmbeddingLifecycle.Default
```
