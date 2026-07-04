export const meta = {
  name: 'build-ts',
  description: 'Execute the approved TypeScript branch re-architecture: scaffold swap (6 new folders, .api moves, old 13-folder deletion, one commit), then wave-ordered fable builders authoring the ruled page roster per folder, a hostile critique pass then a red-team pass per folder, then branch index docs, tests-plane alignment, and the repo-wide drift sweep. Every agent fixes what it notices with whole-repository authority; the ruling is embedded as data rows.',
  whenToUse: 'Launch once after the partition ruling is approved. Ephemeral - delete after the campaign lands.',
  phases: [
    { title: 'Scaffold' },
    { title: 'Build' },
    { title: 'Terminal' },
  ],
}

// --- [CONSTANTS] --------------------------------------------------------------------------

const EVIDENCE = '.cache/design-ts/census-a.md (83 capability rows: kernel/state/host/security/telemetry/wire), ' +
  '.cache/design-ts/census-b.md (96 rows: work/store/ai/edge/browser/ui/iac), .cache/design-ts/effect.md ' +
  '(ecosystem gaps + engine inventory), .cache/design-ts/backend.md (8 guarantee lanes, engine rows, verified versions), ' +
  '.cache/design-ts/seams.md (cross-language seam inventory + tests-plane ripple), .cache/design-ts/api.md ' +
  '(128-catalog inventory), .cache/design-ts/fragments/ (platform-core/node/bun module surfaces + PG extension classes)'

// --- [INPUTS] -----------------------------------------------------------------------------

const ROOT = 'libs/typescript'

// --- [MODELS] -----------------------------------------------------------------------------

const SCAFFOLD_OUT = {
  type: 'object',
  properties: {
    preSwap: { type: 'string' },
    commit: { type: 'string' },
    summary: { type: 'string' },
  },
  required: ['preSwap', 'commit', 'summary'],
}

const BUILD_OUT = {
  type: 'object',
  properties: {
    pages: { type: 'array', items: { type: 'string' } },
    amendments: { type: 'array', items: { type: 'string' } },
    kills: { type: 'array', items: { type: 'string' } },
    residuals: { type: 'array', items: { type: 'string' } },
    summary: { type: 'string' },
  },
  required: ['pages', 'amendments', 'kills', 'residuals', 'summary'],
}

const FIX_OUT = {
  type: 'object',
  properties: {
    fixes: { type: 'array', items: { type: 'string' } },
    residuals: { type: 'array', items: { type: 'string' } },
    verdict: { type: 'string' },
  },
  required: ['fixes', 'residuals', 'verdict'],
}

const TERMINAL_OUT = {
  type: 'object',
  properties: {
    fixes: { type: 'array', items: { type: 'string' } },
    summary: { type: 'string' },
  },
  required: ['fixes', 'summary'],
}

// --- [DOCTRINE] ---------------------------------------------------------------------------

const LAW =
  'CAMPAIGN LAW. The libs/typescript branch is rebuilt as 6 capability domains in 5 waves: core(W0) -> security(W1) -> ' +
  'data(W2) -> runtime(W3) -> ui(W4) -> iac(W4). Every page is ONE dense polymorphic source-file owner at the ' +
  'docs/stacks/typescript bar - 400-700 LOC median fence mass, transcription-complete signature fences with bodies where ' +
  'the body is the law, ZERO comments inside fences, card fields earned. Page grammar is libs/.planning/README.md [03]; ' +
  'banned-hedge and zero-provenance law applies to every line. Names: 1-word camelCase pages, 1-word sub-folders. ' +
  'Variation is rows/cases/policy values on one owner - never sibling shapes, never name-suffix families, never ' +
  'boolean knobs. Backend rows are semantic guarantees with engines as rows. The C#-minted wire decodes through ONE ' +
  'keyed-decode codec engine over registry rows in core/interchange - never per-shape codec pages. External members are ' +
  'written only at spellings the folder .api catalogs (or the branch tier libs/typescript/.api/) verify; an unverifiable ' +
  'member is a RESEARCH item, never settled code. RULED PINS: BM25 row = vchord_bm25 (pairs with VectorChord; ' +
  'pg_search rejected: AGPL + pre-1.0 + corruption issue; pg_textsearch = watch row). NATS JetStream ADMITTED as the ' +
  'fanout/replay engine row (Jepsen fsync hardening noted; never the system of record). ioredis DROPPED (Effect-native ' +
  'cache tier owns stampede/TTL/dedup; Valkey only if a cross-process cache is ever admitted). pg_uuidv7 PRUNED ' +
  '(PG18-native uuidv7). MinIO is dead (self-host row: pgsty/minio, Ceph RGW; managed: R2/Tigris; NEVER Garage - no CAS). ' +
  'Frozen packages @effect/rpc-http and @effect/cluster-node/-browser/-workflow are never admitted. AiPlan is removed ' +
  'upstream - provider fallback is Effect.withExecutionPlan over Model layers. Cluster is leaderless (RunnerStorage ' +
  'advisory locks; no ShardManager); runner entries are NodeClusterHttp/NodeClusterSocket + Bun peers.'

const BUILDER_LAW =
  'YOU ARE A BUILDER. Before writing: compose the FULL docs/stacks/typescript doctrine (every page), the planning ' +
  'authoring standard libs/.planning/README.md, and your folder .api catalogs. The census register rows for the old ' +
  'pages your scope absorbs are DISPOSITION-BINDING: every capability row lands in one of your pages woven root-up ' +
  '(never tacked on) or is returned in kills with the ruling; silence is a defect. The old corpus is deleted from disk - ' +
  'recover any old page verbatim via git show <preSwap>:<oldPath> when the census row needs depth. Consume ' +
  'earlier-wave pages on disk as settled law - reuse their vocabulary, never re-mint. Author the EXACT page set ' +
  'assigned; a roster amendment (merge/split/rename, max 1 page delta) is allowed only with a returned justification. ' +
  'Weave the unexploited-capability signals from the census and the effect/backend dossiers into the owning pages - ' +
  'an admitted capability with no exploiting owner is a defect. Every new-admission package your pages compose needs a ' +
  'folder .api catalog; author a real one (verified members, integration-shaped) if missing. YOU FIX WHAT YOU NOTICE: ' +
  'your write authority is the whole repository - a defect in an earlier folder page, a stale cross-language seam ' +
  'mirror in libs/csharp, a wrong manifest row, a drifted doc: repair it in place NOW, never report it for someone ' +
  'else. Deferral exists ONLY for a fact owned by a folder later in the wave order. Return pages written, amendments, ' +
  'kills, residuals (strictly later-wave-owned facts, nothing else), summary max 8 lines.'

const REVIEW_READ =
  'READ FIRST, AT SOURCE, BEFORE ANY EDIT: enumerate docs/stacks/typescript/ with a real ls (never memory) and read ' +
  'the README plus EVERY page it routes IN FULL in atlas order. Then STATE the complete README [02]-[DOCTRINE] law ' +
  'set by name - all sixteen across FLOW, SHAPE, DERIVATION, MATERIAL, and INTEGRATION - each with a one-line reading ' +
  'of how it bears on this folder; a law you cannot state is a law you have not read, and no edit lands before this. ' +
  'Then ls BOTH .api tiers in full - libs/typescript/.api/ AND the folder .api/ - and read every catalog the folder ' +
  'composes. ULTRA-STACKING IS LAW: an admitted capability the concept admits that no page exploits is a defect YOU ' +
  'close by growing the owning page; a cited member you cannot verify against the catalog or the node_modules ' +
  'declarations is a phantom YOU delete. Your write authority is the whole repository: a defect outside this folder ' +
  'is yours to fix now; deferral exists only for a fact owned by a folder later in the wave order.'

const CRITIQUE_LAW =
  'YOU ARE THE CRITIQUE - the mechanical line-by-line doctrinal-conformance and capability-completeness audit; every ' +
  'hit is a fix made NOW, never a note; the named dimensions are a FLOOR you hunt past. ' +
  '- COLLAPSE_SCAN: run the README [03] table on every fence - any signal triggers the move, three or more make it ' +
  'mandatory. ' +
  '- OWNER_CHOOSER (shapes.md [01]): re-derive every shape from its discriminants (fallibility, payload arity, wire ' +
  'crossing); kill every parallel interface or type alias beside a Schema owner, every DTO, every one-field wrapper, ' +
  'every standalone brand export. The SHAPE BUDGET is one fifth of naive TypeScript and LOOSE TYPE/CONST SPAM IS THE ' +
  'PRIME TARGET: a hand-written union a table derives, a const-plus-type restatement, a shape minted to name an ' +
  'intermediate step, a low-value object a richer owner absorbs - each is deleted into derivation (typeof, keyof ' +
  'typeof, indexed access, Schema.Schema.Type, pick/omit/extend) or folded into the owning family as a case, row, ' +
  'field, or variant. ' +
  '- KNOB_TEST + MODAL_ARITY: delete each parameter - where the value reconstructs it, collapse to a policy value or ' +
  'input-shape discriminant; one entrypoint owns every modality through input-shape discrimination, Function.dual ' +
  'twins, and overload sets over discriminated input - never suffix families, never boolean knobs. ' +
  '- INJECTION: capability travels the requirement channel - Effect.Service owners, Context.Tag ports, Layer graphs, ' +
  'Context.Reference ambients; a module-level live instance, an ambient import, or a parameter-drilled dependency is ' +
  'rewired now. ' +
  '- RAILS + ASPECTS (rails-and-effects.md + surfaces-and-dispatch.md, both owners): typed error channel, closed ' +
  'tagged fault families, accumulate-vs-abort fixed once at the boundary, Schedule policy values, telemetry and ' +
  'resilience attached at the definition seam. ' +
  '- STRINGY/FRAGILE: raw string plumbing dies - vocabulary literals derive from as-const anchors, brands ride Schema ' +
  'fields, structured strings ride TemplateLiteralParser, keys are keyof-typeof proven; zero any, zero unsafe as, ' +
  'zero non-null assertion. ' +
  '- DISPOSITION + SEAMS: every census row for this scope landed or explicitly killed by the builders - fix the gaps; ' +
  'cross-language rows mirror the C#-side ARCHITECTURE.md fences on disk verbatim, both ends. ' +
  '- CAPABILITY + ILLUSION: the body implements what names and prose promise; ANTICIPATE COMPLEX USAGE - every owner ' +
  'is sized for five-times demand and the hard modality (batch, stream, concurrent, multi-tenant), never the naive ' +
  'single-shot case. Fix everything; return the fix log, residuals (later-wave-owned only), verdict max 5 lines.'

const REDTEAM_LAW =
  'YOU ARE THE RED-TEAM - the terminal and most aggressive pass; assume the builders AND the critique missed things; ' +
  'the folder ends objectively DENSER and MORE CAPABLE than the critique left it. ' +
  '(A) COUNTERFACTUAL on every core owner, algebra, and dispatch surface: does a denser tagged family with exhaustive ' +
  'Match, a Schema class family with derived variants and projections, a vocabulary table with lookup dispatch, a ' +
  'parameterized generator over the enumerated space, or a deeper Effect-ecosystem primitive (Stream, Layer algebra, ' +
  'RequestResolver, STM, Cluster, Workflow, VariantSchema) collapse the whole fence? A fundamentally stronger design ' +
  'is BUILT, never defended against. ' +
  '(B) ANTICIPATORY_COLLAPSE: compute the diff of the next feature - the next engine row, provider, wire shape, lane, ' +
  'or modality lands as ONE row with every consumer untouched or loudly broken at Match.exhaustive. ' +
  '(C) LONG-TAIL: empty, singular, plural, batch, stream, malformed, concurrent, cancelled, partial-failure, ' +
  'version-skew; backpressure and interruption correct; ingress AND egress parameterized. ' +
  '(D) BOUNDARY/STRATA: wave order holds, no downward dependency, seams repaired on BOTH ends, entry surfaces stay ' +
  'few and deep - a consumer imports a small set of deep owners, never a spray of modules. ' +
  '(E) SPRAWL + PHANTOMS: hand-re-derived package capability, flat code below the operator depth the admitted ' +
  'packages reach, any/unsafe-as/non-null smuggled anywhere, phantom members, thin wrappers, barrel indirection - ' +
  'deleted or rebuilt on the real surface. ' +
  '(F) FULL COLD RE-REVIEW of every critique dimension by name - COLLAPSE_SCAN, OWNER_CHOOSER, KNOB_TEST + ' +
  'MODAL_ARITY, INJECTION, RAILS + ASPECTS, STRINGY, shape budget, both-tier .api ultra-stacking, prose hygiene - and ' +
  'verify on disk that the critique fix-log genuinely landed. Fix everything; return the fix log, residuals ' +
  '(later-wave-owned only), verdict max 5 lines.'

// --- [OPERATIONS] -------------------------------------------------------------------------

const API_MOVES =
  'kernel/.api/hash-wasm.md -> core/.api/; state/.api/{effect-typeclass,electric-sql-d2ts,electric-sql-d2mini}.md -> ' +
  'core/.api/; wire/.api/* (6: bufbuild-protobuf, cbor-x, msgpack-msgpack, rfc6902, connectrpc-connect, ' +
  'connectrpc-connect-web) -> core/.api/; telemetry/.api/opentelemetry-semantic-conventions.md -> core/.api/; ' +
  'telemetry/.api/* (remaining 9 OTel) -> runtime/.api/; security/.api/* (9) -> security/.api/ (same names, new dir); ' +
  'store/.api/* (8) -> data/.api/; work/.api/* (7) -> runtime/.api/; ai/.api/* (7) -> runtime/.api/; edge/.api/* (4) -> ' +
  'runtime/.api/; browser/.api/* (4) -> runtime/.api/; ui/.api/* (47) -> ui/.api/; iac/.api/* (15) -> iac/.api/; ' +
  'the branch tier libs/typescript/.api/ (7 effect substrate) stays; drop every .gitkeep.'

const MANIFEST_REMAP =
  'tests/contracts/MANIFEST.md consumer-token re-map: kernel/identity/contentkey -> core/value/contentKey; ' +
  'kernel/clock/hlc -> core/value/clock; wire/frame -> core/interchange/frame; wire/fault/detail -> ' +
  'core/interchange/codec; wire/codec/crdt -> core/interchange/format (feeding core/state/merge); wire/codec/bim -> ' +
  'core/interchange/codec; wire/contract/descriptor + wire/contract/drift -> core/interchange/contract; ' +
  'browser/transport -> runtime/browser/fetch; store/object -> data/object/store; ui scene/glb -> ui/viewer/scene; ' +
  'state/crdt/merge -> core/state/merge; state/causal -> core/state/causal; testkit reader paths unchanged.'

const ROSTER = [
  {
    folder: 'core', wave: 'W0', stages: [['value+observe'], ['state'], ['interchange']],
    units: [
      {
        key: 'value+observe',
        pages: ['core/.planning/value/contentKey.md', 'core/.planning/value/identity.md', 'core/.planning/value/clock.md',
          'core/.planning/value/schema.md', 'core/.planning/value/quantity.md', 'core/.planning/value/fault.md',
          'core/.planning/observe/convention.md', 'core/.planning/observe/slo.md', 'core/.planning/observe/board.md'],
        charter: 'The contract floor. contentKey absorbs the mint PLUS the whole hash-wasm unexploited set (keyed digests, ' +
          'checksum family, IHasher save/load resumable streaming, binary 16-byte key row, width rows as seed data). ' +
          'clock = hlc + uncertainty as ONE clock owner (stamp algebra + window algebra + grade ladder). fault = ' +
          'classify + budget + degrade as ONE fault-policy owner (taxonomy, enricher port, Schedule compilation - ' +
          'resolve the Budget/IngressBudget name collision). schema = brands + ingress budgets. observe = semconv ' +
          'vocabulary spine, SLO burn algebra + derived alert specs as one owner, board model + pack library as one ' +
          'owner (DashboardModel values; iac applies them).',
      },
      {
        key: 'state',
        pages: ['core/.planning/state/fold.md', 'core/.planning/state/merge.md', 'core/.planning/state/causal.md',
          'core/.planning/state/commit.md', 'core/.planning/state/evidence.md', 'core/.planning/state/feed.md',
          'core/.planning/state/machine.md', 'core/.planning/state/presence.md'],
        charter: 'The host-free state algebra. fold = algebra + replay + window as ONE time-coordinate owner - and it ' +
          'ABSORBS d2ts Index.reconstructAt/compact (kill the hand-rolled Chunk<Slice> retained log), Diff.iterate ' +
          'fixpoint, join/groupBy incremental operators. merge = crdt merge + converge laws as one owner exposing the ' +
          'lawful Monoid (typeclass combineAll, Bounded tops). causal = order + vector lattice (delivery, stability ' +
          'frontier, retention handoff). commit = the commit-graph/Merkle anti-entropy owner split OUT of the old ' +
          'vector page. evidence = receipt + progress + availability vocabularies as one bounded family; feed = the ' +
          'timeline aggregator + DocumentRef. machine = state machines: vocabulary transition tables + the ' +
          '@effect/experimental Machine serializable actor (snapshot/restore) - the ruled Machine-vs-workflow altitude. ' +
          'presence = the presence CRDT freed from query/live. STM enters here: causal frontier advancement and ' +
          'multi-cell merge atomicity are STM transactions where the census flags Ref+Semaphore hand-rolls.',
      },
      {
        key: 'interchange',
        pages: ['core/.planning/interchange/codec.md', 'core/.planning/interchange/format.md',
          'core/.planning/interchange/contract.md', 'core/.planning/interchange/frame.md',
          'core/.planning/interchange/invoke.md'],
        charter: 'The ONE interchange plane replacing 30 wire pages. codec = the keyed-decode engine: one polymorphic ' +
          'registry keyed off the 43-family census (envelope/version/livewire/bcf/bim/appearance/credential/claim/' +
          'control/layout/flag/progress/geo/graph rows AS DATA), the shared parity-verify combinator, the divert+dedup ' +
          'stream feed, the sequence-gap Mealy - each spelled ONCE. format = the format engines: proto (bufbuild), ' +
          'snapshot canonical-CBOR (cbor-x), crdt MessagePack oplog (msgpack), jsonpatch RFC6902. contract = ' +
          'FileDescriptorSet descriptor + drift gate (Identical/Additive/Breaking) - author the two declared-but-unminted ' +
          'verdicts (WireTypeChanged/EnumValueAdded). frame = artifact/geometry/residency reassembly under kernel ingress ' +
          'budgets. invoke = CapabilityDescriptor vocabulary + the capability client (honor its own catalog law: ' +
          'fetch/interceptors via platform HttpClient). ChannelSchema duplex is the substrate. Seam rows mirror the ' +
          'C#-side fences verbatim (seams dossier section A).',
      },
    ],
  },
  {
    folder: 'security', wave: 'W1', stages: [['all']],
    units: [
      {
        key: 'all',
        pages: ['security/.planning/authn/oauth.md', 'security/.planning/authn/webauthn.md',
          'security/.planning/authn/credential.md', 'security/.planning/authn/session.md',
          'security/.planning/access/claim.md', 'security/.planning/access/tenant.md',
          'security/.planning/crypt/sign.md', 'security/.planning/crypt/secret.md', 'security/.planning/crypt/verify.md'],
        charter: 'Trust A-Z, internally complete. oauth widens to the FULL arctic surface: the 60+ provider roster as ' +
          'rows (google/github/microsoft/apple-with-pkcs8 + generic OAuth2Client for self-hosted issuers), refresh + ' +
          'revoke + expiry/scopes lifecycle, ExecutionPlan-free single dispatch. webauthn absorbs attestation trust ' +
          '(SettingsService/MetadataService FIDO MDS), conditional-UI autofill, WebAuthnAbortService, platform-probe ' +
          'gates. credential = otp (with timeStep replay floor + guardrails) + recovery + apikey as ONE digest-at-rest ' +
          'owner (the census-flagged shared mint+resolve idiom). session = token + cookie as one owner (rotation, ' +
          'reuse-detection, CSRF, framing). claim = claim + policy as one authz owner (RBAC-union-ReBAC fold); tenant = ' +
          'the tenancy contract (app.current_tenant RLS shape data enforces). sign = crypto + jwt + material as ONE ' +
          'jose/crypto owner (argon2id, HMAC, AES-GCM Shredder, JWKS with jwksCache + customFetch, JWE confidential ' +
          'profile, RFC-7638 thumbprints). secret = doppler lease custody. verify = the external-signature ingress row ' +
          '(oslojs verify-only asymmetric surface: partner webhooks, attestation chains).',
      },
    ],
  },
  {
    folder: 'data', wave: 'W2', stages: [['lane+journal'], ['read+object']],
    units: [
      {
        key: 'lane+journal',
        pages: ['data/.planning/lane/postgres.md', 'data/.planning/lane/sqlite.md', 'data/.planning/lane/olap.md',
          'data/.planning/lane/cache.md', 'data/.planning/lane/tenant.md', 'data/.planning/lane/capability.md',
          'data/.planning/journal/append.md', 'data/.planning/journal/evolve.md', 'data/.planning/journal/retain.md',
          'data/.planning/journal/fact.md'],
        charter: 'The guarantee-lane matrix + the journal spine, built on backend.md verbatim. postgres = the PG 18.4 ' +
          'spine (L1 first-party rows incl. uuidv7/virtual-generated/RETURNING old-new/temporal constraints/skip scan, ' +
          'SKIP-LOCKED primitive table with what each does NOT guarantee) + the ruled extension matrix (pgvector/' +
          'VectorChord, vchord_bm25, pgmq-semantics-mined-not-adopted, pg_cron, pg_ivm, pg_partman, pg_duckdb, ' +
          'pg_graphql, pg_jsonschema, pgaudit, postgis/h3, timescale with TSL+Citus-exclusion flags; additions ' +
          'pg_parquet + pg_incremental). sqlite = ONE lane, five profile rows (node better-sqlite3, bun, wasm-OPFS, ' +
          'libSQL/Turso edge-replica, D1) + the degradation table. olap = DuckDB node (@duckdb/node-api Neo) + wasm + ' +
          'ClickHouse at-scale + DuckLake; Arrow is the ONE columnar wire. cache = the Tier-0 Effect-native lane ' +
          '(Cache single-flight, RequestCache, PersistedCache, RcRef/RcMap pools) + the escalation law (Valkey row ' +
          'gated on cross-process need). tenant = StoreHandle LayerMap scopes + RLS GUC enforcement of security/access. ' +
          'capability = fail-closed probe matrix with RequestResolver-batched probes. append = journal + outbox + ' +
          'xmax=0 idempotency as ONE atomic owner; evolve = upcast folds + snapshot-as-projection; retain = retention ' +
          'frontier + crypto-shred (security Shredder) + DSAR export; fact = audit + meter fact streams as ONE ' +
          'polymorphic fact-journal owner (BigDecimal rating, retention classes) - the old telemetry pair collapsed.',
      },
      {
        key: 'read+object',
        pages: ['data/.planning/read/fold.md', 'data/.planning/read/live.md', 'data/.planning/read/query.md',
          'data/.planning/read/search.md', 'data/.planning/read/batch.md', 'data/.planning/object/store.md',
          'data/.planning/object/stream.md', 'data/.planning/object/file.md'],
        charter: 'The read side + the object plane. fold = inline/async/rebuild projection lanes binding core/state ' +
          'fold plans (checkpointed, LISTEN/NOTIFY-woken, SKIP LOCKED; pg_incremental exactly-once alignment noted). ' +
          'live = Reactivity-keyed reactive reads. query = the typed CRUD engine the census flags as unexploited ' +
          'corpus-wide: SqlSchema findAll/findOne/single/void + SqlResolver ordered/grouped/findById/void + ' +
          'VariantSchema Model field-families collapsing per-variant shapes. search = FTS + vchord_bm25 BM25 + ' +
          'pgvector/VectorChord + trigram fuzzy + hybrid rank fusion as one retrieval owner. batch = the general ' +
          'RequestResolver/dataLoader engine (window/maxBatchSize, persisted, withRequestCaching) serving S3 HEAD ' +
          'coalescing, embedding batches, probe batches. store = ContentKey-addressed conditional-put object storage ' +
          '(If-None-Match:* 412-noop algebra, If-Match CAS GC, presign, engine conformance table from L7). stream = ' +
          'the ONE resumable content-addressed rail (Web Streams BYOB -> FastCDC-wasm chunk stage -> hash-wasm ' +
          'incremental fold -> BLAKE3-Merkle sub-keys -> tus/@tus/s3-store offsets onto multipart -> conditional ' +
          'complete; Range+If-Range reads). file = filesystem management + sharp derivative pipelines over the same ' +
          'ContentKey identity.',
      },
    ],
  },
  {
    folder: 'runtime', wave: 'W3', stages: [['proc+net+otel'], ['serve', 'browser'], ['work+ai']],
    units: [
      {
        key: 'proc+net+otel',
        pages: ['runtime/.planning/proc/exec.md', 'runtime/.planning/proc/worker.md', 'runtime/.planning/proc/config.md',
          'runtime/.planning/proc/life.md', 'runtime/.planning/proc/flag.md', 'runtime/.planning/net/client.md',
          'runtime/.planning/net/channel.md', 'runtime/.planning/net/pubsub.md', 'runtime/.planning/otel/emit.md',
          'runtime/.planning/otel/vital.md', 'runtime/.planning/otel/crash.md'],
        charter: 'The process plane. exec = node/bun binding rows + runMain + subprocess (Command pipelines, Process ' +
          'handles). worker = pools + Transferable zero-copy + serialized runner protocols + backpressure/TTL sizing. ' +
          'config = provider chain + boot-validated Setting via Schema.Config as ONE config owner. life = cycle + ' +
          'health as ONE owner (ranked-registry budgeted fold spelled once, k8s probe routes, drain -> ' +
          'terminationGracePeriod). flag = rollout + verdict as ONE owner over the REAL @openfeature/server-sdk ' +
          '(hooks, events, object kind) with kernel-hash bucket parity. client = egress lane table (undici dispatcher ' +
          'tuning rows); channel = framed duplex (Ndjson/MsgPack over Socket) + full Sse codec (encoder + Retry-driven ' +
          'reconnect). pubsub = the fanout/replay engine row: NATS JetStream (nats-core + jetstream clients, dedup ' +
          'window, double-ack, replay-by-sequence/time, sync-interval hardening; never system of record) + the ' +
          'in-process PubSub/replay tier. emit = OTLP export + W3C context continuation as ONE otlp owner (lane ' +
          'dispatch, redaction as the one shared scrub owner); vital = browser RUM budgets; crash = Cause->fatal fold ' +
          '+ enricher round-trip + breadcrumb ring.',
      },
      {
        key: 'serve',
        pages: ['runtime/.planning/serve/api.md', 'runtime/.planning/serve/route.md', 'runtime/.planning/serve/live.md',
          'runtime/.planning/serve/problem.md', 'runtime/.planning/serve/cli.md'],
        charter: 'The one public front door. api = the HttpApi engine at full depth: spec-derived server + typed ' +
          'client + OpenAPI 3.1 + Scalar UI, HttpApiSecurity schemes threaded to security/ (the census gap - declarative ' +
          'auth into the emitted spec), HttpApiMiddleware provides/failure, multipart typed uploads, toWebHandler edge ' +
          'form. route = HttpLayerRouter layer-native routing + HttpMultiplex host/header dispatch + static/file ' +
          'serving with Etag. live = realtime serve: websocket upgrade, SSE mirror of net/channel, presence serving ' +
          'core/state/presence. problem = HttpServerRespondable as the outbound-fault law (each error renders itself; ' +
          'RFC 9457 projection) killing the central mapper. cli = @effect/cli command tree + printer Doc rendering + ' +
          'Schema-derived Pretty.',
      },
      {
        key: 'browser',
        pages: ['runtime/.planning/browser/boot.md', 'runtime/.planning/browser/shell.md',
          'runtime/.planning/browser/persist.md', 'runtime/.planning/browser/route.md',
          'runtime/.planning/browser/fetch.md'],
        charter: 'The browser runtime. boot = single-boot law + BrowserRuntime + ManagedRuntime handle + ' +
          'Clipboard/Geolocation/Permissions services. shell = PWA install + workbox worker lifecycle. persist = ' +
          'idb-keyval KV + OPFS lanes (wasm sqlite coordination noted at data/lane/sqlite). route = Navigation-API ' +
          'routing + guards + nuqs URL state. fetch = the transport pool + off-thread decode workers (ContentKey ' +
          'verify delegation) + XHR progress lane + BrowserSocket.',
      },
      {
        key: 'work+ai',
        pages: ['runtime/.planning/work/entity.md', 'runtime/.planning/work/flow.md', 'runtime/.planning/work/queue.md',
          'runtime/.planning/work/schedule.md', 'runtime/.planning/work/deliver.md', 'runtime/.planning/work/report.md',
          'runtime/.planning/ai/model.md', 'runtime/.planning/ai/embed.md', 'runtime/.planning/ai/tool.md',
          'runtime/.planning/ai/agent.md'],
        charter: 'Durable execution + the intelligence rail, on the L6 decomposition (80% native / 15% pg-composed / ' +
          '5% external). entity = cluster entities over RpcGroups, leaderless sharding (RunnerStorage advisory locks), ' +
          'singletons, MessageStorage durable mailbox on data SqlClient, K8s runner health. flow = workflows + ' +
          'activities + compensation sagas + DurableClock/DurableDeferred + WorkflowProxy. queue = DurableQueue + ' +
          'DurableRateLimiter native rows + the pg-composed outbox/priority/DLQ/replay lanes (ORDER BY + SKIP LOCKED ' +
          'on data journal; pgmq semantics mined, engine rejected). schedule = ClusterCron + misfire/catch-up windows. ' +
          'deliver = mail + webhook signed egress + relay as one delivery owner (retry/DLQ spelled ONCE - the census ' +
          'twice-owned defect). report = exceljs/jspdf/jszip/papaparse render engines at real depth (census: toy ' +
          'depth). model = provider rows + Effect.withExecutionPlan fallback (AiPlan is gone) + generateObject/' +
          'toolChoice/disableToolCallResolution. embed = EmbeddingModel.makeDataLoader + two-tier cache + persisted. ' +
          'tool = Toolkit + McpServer (stdio/http transports, resources, prompts, elicitation). agent = Machine-actor ' +
          'altitude (core/state/machine) + persisted Chat memory.',
      },
    ],
  },
  {
    folder: 'ui', wave: 'W4', stages: [['system+view'], ['viewer']],
    units: [
      {
        key: 'system+view',
        pages: ['ui/.planning/system/token.md', 'ui/.planning/system/primitive.md', 'ui/.planning/system/act.md',
          'ui/.planning/system/intl.md', 'ui/.planning/system/atom.md', 'ui/.planning/view/table.md',
          'ui/.planning/view/form.md', 'ui/.planning/view/overlay.md'],
        charter: 'Component capability. token = scale + theme as one owner (colorjs.io ramps). primitive = ' +
          'react-aria/radix composition + cva/tailwind-merge recipes as one owner. act = gesture + transition (the ' +
          'idle animation packages exploited: use-gesture, tw-animate). intl = format + message as one owner (the ' +
          'duplicated Intl caches collapsed). atom = the @effect-atom bridge (components consume the graph only ' +
          'through it). table = tanstack table + virtual as one data-grid owner. form = Schema.standardSchemaV1-driven ' +
          'forms. overlay = cmdk command palette + vaul drawers + floating-ui as one overlay owner.',
      },
      {
        key: 'viewer',
        pages: ['ui/.planning/viewer/scene.md', 'ui/.planning/viewer/geo.md', 'ui/.planning/viewer/mark.md',
          'ui/.planning/viewer/panel.md', 'ui/.planning/viewer/probe.md'],
        charter: 'The spatial tier (nested Nx project stays). scene = GLB residency by ContentKey + three/model-viewer ' +
          '+ OpenPBR appearance decode + deck.gl mesh-layers (the census-flagged idle package exploited). geo = ' +
          'maplibre + deck.gl geo/geoarrow layers + turf planar peer. mark = BCF topics/viewpoints + GlobalId ' +
          'selection sets. panel = the census-ruled three-way collapse: binding + control + layout as ONE ' +
          'wire-materializer over ControlIntent/LayoutConstraint (kiwi solver). probe = benchmark + receipt as one ' +
          'evidence owner. Seam rows mirror Rasm.AppUi/Rasm.Bim fences verbatim.',
      },
    ],
  },
  {
    folder: 'iac', wave: 'W4', stages: [['all']],
    units: [
      {
        key: 'all',
        pages: ['iac/.planning/program/spec.md', 'iac/.planning/program/automation.md', 'iac/.planning/program/provider.md',
          'iac/.planning/kube/workload.md', 'iac/.planning/kube/data.md', 'iac/.planning/kube/traffic.md',
          'iac/.planning/operate/secret.md', 'iac/.planning/operate/observe.md', 'iac/.planning/operate/policy.md'],
        charter: 'The deploy plane - Pulumi typed programs, never yaml. spec + automation = program shapes + the ' +
          'Automation API driver. provider = dispatch + surface as ONE owner (the census-flagged arm duplication ' +
          'killed; aws/gcp/cloudflare/docker rows at real depth, not bare signatures). kube/workload+data+traffic = ' +
          'the cluster estate; data carries the CNPG image with the RULED extension roster (vchord_bm25, VectorChord, ' +
          'pgvector, pgmq-not-adopted note, pg_cron, pg_ivm, pg_partman, pg_duckdb, postgis, pgaudit CNPG-managed, ' +
          'pg_parquet, pg_incremental) + the object-store row (pgsty/minio or R2/Tigris - MinIO references are stale, ' +
          'Garage banned) + NATS JetStream deployment with fsync hardening. secret = doppler provisioning + TLS. ' +
          'observe = stacks + grafana board apply (core/observe DashboardModel values) - drift folds INTO automation. ' +
          'policy = guard + drift as one policy owner. StackOutputs -> ShardingConfig stays the one runtime-crossing ' +
          'value (the work seam).',
      },
    ],
  },
]

// --- [SCAFFOLD_PROMPT]

const scaffoldPrompt =
  'Execute the approved TypeScript scaffold swap as ONE commit. ' + LAW + ' ACTIONS, in order: ' +
  '(1) Record the current HEAD hash as preSwap. ' +
  '(2) Create the six folder skeletons under ' + ROOT + ': core, security, data, runtime, ui, iac - each with .planning/ ' +
  'and .api/ dirs (ui keeps a nested ui/viewer/ dir for the second Nx project). ' +
  '(3) Move the .api catalogs with git mv per this routing: ' + API_MOVES + ' ' +
  '(4) Delete the thirteen old folders (git rm -r): kernel state host security-old telemetry wire work store ai edge ' +
  'browser ui-old iac-old - NOTE security/ui/iac are REPLACED: move their .api content FIRST into the new skeleton, ' +
  'then remove every old .planning page, old README/ARCHITECTURE/IDEAS/TASKLOG, and old project.json under them. ' +
  '(5) Rewrite ' + ROOT + '/package.json exports: one subpath per new folder (./core ./security ./data ./runtime ./ui ' +
  './ui/viewer ./iac -> ./<folder>/src/index.ts), server/browser/wasm conditional trios on core, security, data, ' +
  'runtime; DELETE the #vocab imports map entry (wire is dissolved; ui imports @rasm/ts/core). ' +
  '(6) Write one project.json per new folder + ui/viewer nested, preserving the tag-triple contract: scope:<folder>, ' +
  'runtime: (core neutral, security neutral, data neutral, runtime neutral, ui browser, iac node), plane: (iac deploy, ' +
  'all others runtime); ui/viewer sourceRoot ' + ROOT + '/ui/viewer/src. ' +
  '(7) pnpm-workspace.yaml catalog: relabel the folder comment blocks to the new six; REMOVE ioredis; ADD under the ' +
  'owning labels (newest stable versions, verify via npm): @effect/sql-libsql, @effect/sql-d1, @effect/sql-clickhouse, ' +
  '@duckdb/node-api, @duckdb/duckdb-wasm, @openfeature/server-sdk, @tus/server, @tus/s3-store, tus-js-client, ' +
  '@aws-sdk/lib-storage, @nats-io/nats-core, @nats-io/jetstream. Keep catalogMode strict and the two packages rows ' +
  'untouched (the admission suite asserts them). ' +
  '(8) ' + MANIFEST_REMAP + ' ' +
  '(9) Do NOT touch tests/typescript suites (deferred ripple; boundaries.spec re-target lands in a later leg) and do ' +
  'NOT touch docs/stacks. ' +
  '(10) Stage everything and commit with message "ts scaffold swap: 13 deployment-plane folders -> 6 capability ' +
  'domains (core/security/data/runtime/ui/iac); .api estate re-homed; ruled package deltas; wire dissolved into ' +
  'core/interchange" plus the Claude-Session trailer. Return {preSwap, commit, summary}.'

const builderPrompt = (folder, unit, preSwap) =>
  LAW + ' ' + BUILDER_LAW + ' ' +
  'SCOPE: folder ' + ROOT + '/' + folder + ', unit ' + unit.key + '. AUTHOR EXACTLY these pages: ' +
  unit.pages.map((page) => ROOT + '/' + page).join(', ') + '. CHARTER: ' + unit.charter + ' ' +
  'EVIDENCE on disk: ' + EVIDENCE + '. The pre-swap commit for git show recovery of old pages: ' + preSwap + '. ' +
  'Earlier waves are settled on disk under ' + ROOT + ' - compose their vocabulary. Doctrine, planning law, and your ' +
  'folder .api catalogs are read IN FULL before the first fence.'

const critiquePrompt = (folder, unitReports) =>
  LAW + ' ' + REVIEW_READ + ' ' + CRITIQUE_LAW + ' ' +
  'SCOPE: the whole folder ' + ROOT + '/' + folder + ' as just built. Builder reports (amendments/kills/residuals to ' +
  'verify, never trust): ' + JSON.stringify(unitReports) + '. EVIDENCE: ' + EVIDENCE + '.'

const redteamPrompt = (folder, crit) =>
  LAW + ' ' + REVIEW_READ + ' ' + REDTEAM_LAW + ' ' +
  'SCOPE: the whole folder ' + ROOT + '/' + folder + ' as the critique left it. CRITIQUE RESULT (verify its fixes on ' +
  'disk, never trust the log): ' + JSON.stringify(crit || {}) + '. EVIDENCE: ' + EVIDENCE + '.'

// --- [COMPOSITION] ------------------------------------------------------------------------

// --- [SCAFFOLD]

phase('Scaffold')
const scaffold = await agent(scaffoldPrompt, {
  label: 'scaffold', phase: 'Scaffold', model: 'fable', effort: 'high', schema: SCAFFOLD_OUT,
})
if (!scaffold) throw new Error('scaffold swap did not land')
log('swap committed: ' + scaffold.commit + ' (preSwap ' + scaffold.preSwap + ')')

// --- [BUILD]

phase('Build')
const WAVE_GROUPS = [['core'], ['security'], ['data'], ['runtime'], ['ui', 'iac']]
const folderResults = []
let carried = []

const runFolder = async (entry, carriedIn) => {
  log(entry.wave + ' ' + entry.folder + ': ' + entry.stages.length + ' stage(s), then critique, then red-team')
  const carriedNote = carriedIn.length > 0
    ? ' RESIDUALS carried from earlier folders that name your scope - land or explicitly return them: ' + JSON.stringify(carriedIn)
    : ''
  const reports = []
  for (const stageKeys of entry.stages) {
    const stage = stageKeys.map((key) => entry.units.find((unit) => unit.key === key)).filter(Boolean)
    const stageOut = (await parallel(stage.map((unit) => () =>
      agent(builderPrompt(entry.folder, unit, scaffold.preSwap) + carriedNote, {
        label: 'build:' + entry.folder + ':' + unit.key, phase: 'Build', model: 'fable', effort: 'xhigh', schema: BUILD_OUT,
      }).then((report) => (report ? { unit: unit.key, ...report } : null))))).filter(Boolean)
    reports.push(...stageOut)
  }
  const crit = await agent(critiquePrompt(entry.folder, reports), {
    label: 'crit:' + entry.folder, phase: 'Build', model: 'fable', effort: 'xhigh', schema: FIX_OUT,
  })
  const rt = await agent(redteamPrompt(entry.folder, crit), {
    label: 'rt:' + entry.folder, phase: 'Build', model: 'fable', effort: 'max', schema: FIX_OUT,
  })
  return { entry, reports, crit, rt }
}

for (const group of WAVE_GROUPS) {
  const entries = group.map((name) => ROSTER.find((row) => row.folder === name)).filter(Boolean)
  const outs = (await parallel(entries.map((entry) => () => runFolder(entry, carried)))).filter(Boolean)
  carried = outs.flatMap((out) => [
    ...out.reports.flatMap((report) => report.residuals || []),
    ...((out.crit && out.crit.residuals) || []),
    ...((out.rt && out.rt.residuals) || []),
  ])
  for (const out of outs) {
    folderResults.push({
      folder: out.entry.folder,
      pages: out.reports.flatMap((report) => report.pages || []).length,
      kills: out.reports.flatMap((report) => report.kills || []),
      fixes: (out.crit ? out.crit.fixes.length : 0) + (out.rt ? out.rt.fixes.length : 0),
      verdict: out.rt ? out.rt.verdict : out.crit ? out.crit.verdict : 'reviews dropped',
    })
  }
}

// --- [TERMINAL]

phase('Terminal')

const docs = await agent(
  LAW + ' BRANCH INDEX DOCS for the rebuilt branch - you WRITE, and you fix every defect you notice on the way. ' +
  'Author per the planning standard (libs/.planning/README.md [02]): per new folder - README.md (page router + ' +
  'domain-package registry + substrate section), ARCHITECTURE.md (codemap + [02]-[SEAMS] fence mirroring the C# ' +
  'endpoints verbatim + folder-specific sections), template IDEAS.md and TASKLOG.md (empty [OPEN]/[CLOSED] shells). ' +
  'Then the branch core: rewrite ' + ROOT + '/.planning/README.md (folder router + substrate registry), ' +
  ROOT + '/.planning/ARCHITECTURE.md (codemap, wave order, branch seam ledger, AND the permitted-edge table inline - ' +
  'composition-system.md is dead and is NOT re-authored), and rebuild dataflow-system.md against the new owners ' +
  '(content identity, interchange plane, journal spine, time/order, tenancy, cross-language invariants - same law, ' +
  'new homes). Update libs/.planning/planning-targets.md TYPESCRIPT rows. While authoring: land every carried ' +
  'later-wave fact at its owner page or record its explicit kill (' + JSON.stringify(carried) + '); verify every ' +
  'new-admission package (sql-libsql, sql-d1, sql-clickhouse, duckdb node+wasm, openfeature, tus family, lib-storage, ' +
  'nats-core, jetstream) has a real folder .api catalog and author any missing one with verified members. All prose ' +
  'obeys docs/standards/style-guide.md: declarative present-tense fact, zero meta framing, and never fragile ' +
  'count-based prose - name structure by its members and its law, never by how many there are. Folder build results ' +
  'for the routers: ' + JSON.stringify(folderResults) + '. Return {fixes, summary}.',
  { label: 'docs', phase: 'Terminal', model: 'fable', effort: 'max', schema: TERMINAL_OUT },
)

const [tests, sweep] = await parallel([
  () => agent(
    LAW + ' ALIGN THE TESTS PLANE to the rebuilt branch - nothing is deferred, you fix everything now, surgically, ' +
    'preserving the established idiom of each suite (this is a re-target, never a rebuild). The rebuilt state is on ' +
    'disk; the branch ' + ROOT + '/.planning/ARCHITECTURE.md carries the permitted-edge table inline. ' +
    '(a) tests/typescript/_architecture/src/boundaries.spec.ts: point the ledger parser at the inline edge table ' +
    '(composition-system.md is dead), update the hardcoded admission/crypto zone rows to the new folders (iac -> ' +
    '@pulumi family; data -> @effect/sql family + duckdb; security -> jose/arctic/@simplewebauthn/@oslojs; ui -> ' +
    'react family; runtime -> @effect/cluster/workflow/ai + nats + openfeature + tus), the tag-triple assertions, ' +
    'the @rasm/ts/<folder> zone grammar, and every folder literal in the falsification block. ' +
    '(b) admission.spec.ts: verify the two pnpm packages rows and catalogMode facts still hold, verify the per-folder ' +
    '.api tier walk resolves the new folder set, and keep the banned-module rows true to the journal law. ' +
    '(c) hygiene.spec.ts: verify the estate globs. (d) tests/typescript/_testkit corpus reader + ' +
    'tests/contracts/MANIFEST.md: verify the re-mapped consumer tokens parse and the reader stays folder-agnostic - ' +
    'fix any token the scaffold missed. (e) e2e: verify no import names an old subpath. Every comment or prose line ' +
    'you touch obeys docs/standards/style-guide.md. Return {fixes, summary}.',
    { label: 'tests', phase: 'Terminal', model: 'fable', effort: 'xhigh', schema: TERMINAL_OUT },
  ),
  () => agent(
    LAW + ' REPO-WIDE DRIFT SWEEP - the rebuilt branch is the state that always was; every stale reference is yours ' +
    'to fix NOW, zero deferral, zero meta framing. Hunt with rg across libs/csharp/**, libs/python/**, ' +
    'libs/.planning/**, docs/** (excluding docs/stacks - code doctrine is out of scope), and root-level *.md for: ' +
    'old TS owner tokens (kernel, state, host, telemetry, wire, work, store, ai, edge, browser spelled as ' +
    'libs/typescript paths, typescript:<folder> seam tokens, or @rasm/ts/<folder> subpaths), composition-system.md ' +
    'mentions, #vocab, and dead rulings surviving as live claims (MinIO as the object-store row, ioredis as admitted, ' +
    'pg_uuidv7 as a matrix row, AiPlan, ShardManager, NodeClusterRunnerSocket). Update every C#-side ARCHITECTURE.md ' +
    '[02]-[SEAMS] fence naming a typescript endpoint to the new owner tokens, mirroring the TS-side branch seam ' +
    'ledger on disk verbatim. Update libs/.planning/architecture.md per-language-role and wire sections to the ' +
    'rebuilt structure. PROSE LAW - docs/standards/style-guide.md in full: declarative present-tense fact, active ' +
    'voice, zero session or process narration, and never fragile enumeration prose (a folder roster is named by its ' +
    'members or its law, never by its count). Do not touch tests/ (owned by a sibling agent), .cache/, .claude/, ' +
    'docs/stacks/, or ' + ROOT + ' (already rebuilt). Return {fixes, summary}.',
    { label: 'sweep', phase: 'Terminal', model: 'fable', effort: 'xhigh', schema: TERMINAL_OUT },
  ),
])

return {
  commit: scaffold.commit,
  folders: folderResults,
  docs: docs ? docs.summary : 'dropped',
  tests: tests ? tests.summary : 'dropped',
  sweep: sweep ? sweep.summary : 'dropped',
}
