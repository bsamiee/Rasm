# [TS_API_CATALOGUE]

TypeScript-branch API catalogue pages capture reflection-grounded external package surface for
the libraries the TS planning pages consume. Each `api-<scope>.md` page records the package
surface, public types, entrypoints, and tier/admission law before any planning owner names that
API. Surfaces are reflected from the installed `node_modules` `.d.ts` declarations via
`uv run python -m tools.assay api query --key <package>` (with `--symbol` / `--full` for
member-level detail); a package absent from the install preserves its page with un-reflected
members recorded as gaps, never invented. Versions live only in `pnpm-workspace.yaml` and the
lockfile; these pages carry the reflected version as evidence.

## [1]-[PACKAGE_PAGES]

[EFFECT_CORE]:
- scope: the `effect` core runtime as its own per-library page (three-channel computation, Layer/Context DAG, ManagedRuntime root, Stream folds, Schema, Metric/Tracer, Data/Option/Either/Exit/Cause/Config value algebra); the abstract platform surface, the schema-driven RPC engine, the distributed-cluster substrate, and the durable-workflow engine each own their own per-library page
- packages: `effect`, `@effect/workflow`
- pages:
  - [api-effect.md](api-effect.md)
  - [api-effect-core.md](api-effect-core.md)
  - [api-effect-workflow.md](api-effect-workflow.md)

[PERSISTENCE_CORE]:
- scope: the dialect-agnostic `@effect/sql` SQL toolkit as its own per-library page — the `SqlClient` query service + callable template tag, the `Statement` template DSL / `Fragment` algebra / dialect `Compiler`, the `SqlConnection` driver contract, the `SqlError`/`ResultLengthMismatch` rail, the `SqlSchema`/`SqlResolver` schema-bridged query layers, the `Model` schema-class family with `makeRepository`/`makeDataLoaders`, the generic `Migrator` runner + loaders, the `SqlStream` pause/resume adapter, and the durable `SqlEventJournal`/`SqlEventLogServer`/`SqlPersistedQueue` subsystems; portable persistence logic and the `[PERSISTENCE]` (sql-pg) dialect depend on the abstract symbols owned here
- packages: `@effect/sql` (peers `effect`, `@effect/experimental`, `@effect/platform`)
- pages:
  - [api-effect-sql.md](api-effect-sql.md)

[CLUSTER]:
- scope: the distributed sharding/entity/durable-cluster substrate — the `Entity` addressable-actor vocabulary + `EntityResource`/`EntityProxy`/`EntityProxyServer` derivation, the `Sharding` runtime service + `ShardingConfig` and `ShardingRegistrationEvent`, the `Envelope`/`Message`/`Reply`/`DeliverAt` message plane, the `Snowflake`/`MachineId`/`ShardId`/`EntityId`/`EntityType` identity vocabulary, the `EntityAddress`/`RunnerAddress`/`SingletonAddress` addresses, the closed `ClusterError` tagged-error rail, the `ClusterSchema` annotation references, the `MessageStorage`/`RunnerStorage` persistence services with `SqlMessageStorage`/`SqlRunnerStorage` SQL drivers, the `Runner`/`Runners`/`RunnerHealth`/`RunnerServer` runner mesh, the `HttpRunner`/`SocketRunner`/`SingleRunner`/`TestRunner` transport assemblies, the `Singleton`/`ClusterCron` cluster-scoped work, the `ClusterWorkflowEngine` durable-workflow binding, the `K8sHttpClient` Kubernetes face, and the `ClusterMetrics` gauges; backs `@rasm/node-durable` `ClusterEngine` (`ClusterWorkflowEngine.layer` over `Sharding | MessageStorage`), `RunnerBackplane`, `ScheduledWork`, and `SqlBoundary`. Authoritative over the superseded thin `@effect/cluster` overview in `api-effect-core.md` section [6]
- packages: `@effect/cluster` (peers `effect`, `@effect/rpc`, `@effect/platform`, `@effect/sql`, `@effect/workflow`)
- pages:
  - [api-effect-cluster.md](api-effect-cluster.md)

[EFFECT_PLATFORM]:
- scope: the abstract platform surface the browser/node driver tiers implement and the neutral interior composes — the `HttpClient` polymorphic dispatch surface + `FetchHttpClient.layer` browser binding, `HttpClientRequest`/`HttpClientResponse`/`HttpClientError` and the request-support value algebra (`HttpMethod`/`Headers`/`HttpBody`/`UrlParams`/`Cookies`), the `KeyValueStore` persistence face (`LocalPersistence`), the `Worker`/`WorkerPool`/`WorkerRunner`/`Transferable`/`WorkerError` pool backing (`DecodeWorkerPool`), the `MsgPack`/`Ndjson`/`ChannelSchema` schema-channel encoders, the `Socket` WebSocket transport, the `Error` (`PlatformError`) fault family, the `Runtime` node-entry boundary, and `PlatformConfigProvider`; the HTTP-server/router/api, filesystem/path, command/terminal, and OpenAPI modules ship but no planning owner consumes them
- packages: `@effect/platform` (peer `effect`; concrete drivers on the browser/node tier pages)
- pages:
  - [api-effect-platform.md](api-effect-platform.md)

[EFFECT_RPC]:
- scope: the schema-driven, transport-agnostic RPC engine backing `@rasm/node-durable` `InternalRpc` and the `WorkflowProxy.toRpcGroup`/`WorkflowProxyServer` projection; twelve modules — procedure atom (`Rpc`), aggregate (`RpcGroup`), typed client (`RpcClient`) + transport `Protocol` factories, server runner (`RpcServer`) + HTTP entrypoints, wire-message algebra (`RpcMessage`), middleware tag family (`RpcMiddleware`), stream schema (`RpcSchema`), serialization layers (`RpcSerialization`), client error (`RpcClientError`), in-memory test client (`RpcTest`), worker bootstrap (`RpcWorker`); the internal TS-to-TS RPC surface is distinct from the `.NET` protobuf/msgpack wire on `api-transport-wire.md`
- packages: `@effect/rpc` (peers `effect`, `@effect/platform`, `msgpackr`)
- pages:
  - [api-effect-rpc.md](api-effect-rpc.md)

[PERSISTENCE]:
- scope: node-tier Postgres dialect over `@effect/sql` — `PgClient` service, pool/layer constructors, Postgres statement compiler, `PgMigrator` runner + re-exported `Migrator` loaders; specializes the abstract `SqlClient`/`Statement.Compiler`/`Migrator` symbols owned by `[PERSISTENCE_CORE]` ([api-effect-sql.md](api-effect-sql.md))
- packages: `@effect/sql-pg`
- pages:
  - [api-effect-sql-pg.md](api-effect-sql-pg.md)

[DURABLE_SUBSTRATE]:
- scope: persistence substrate and offline/durable surfaces the `node-durable` and `host` planning pages consume — `Persistence` backing/result stores + KVS/memory layers, `PersistedCache` and `RequestResolver.persisted`/`dataLoader` memoization rails, `PersistedQueue` durable work queue, `RateLimiter` (fixed-window/token-bucket) + store, `Reactivity` key-scoped invalidation, the `Event`/`EventGroup`/`EventJournal`/`EventLog` local-log family with `EventLogRemote`/`EventLogServer`/`EventLogEncryption` sync, the `Machine` actor state machine, `Sse`, `DevTools` tracing layers, and the `VariantSchema` multi-variant builder
- packages: `@effect/experimental` (peer `@effect/platform`, `effect`)
- pages:
  - [api-effect-experimental.md](api-effect-experimental.md)

[STATE_CELL]:
- scope: the framework-agnostic (`neutral`-tier) reactive-atom state engine the TS state/view planning owners consume as a cell bridge — `Atom` reactive-cell model + full constructor/combinator surface (`make`/`fn`/`fnSync`/`pull`/`subscriptionRef`/`subscribable`/`kvs`/`searchParam`/`family` + `AtomRuntime`/`RuntimeFactory` layer-scoped sub-atom builders), the three-state `Result<A,E>` (`Initial`/`Success`/`Failure`) with its `match`/`builder` folds and `Schema`/`Encoded` serialization, the `Registry` node store + `AtomRegistry` `Context.TagClass`/`layer`, the registry-free `AtomRef`/`Collection` mutable refs, the `AtomRpc`/`AtomHttpApi` client-as-atoms tag projections over `@effect/rpc` `RpcGroup` and `@effect/platform` `HttpApi`, and the `Hydration` SSR dehydrate/hydrate family; the React binding `@effect-atom/atom-react` is a separate view-surface package
- packages: `@effect-atom/atom` (peers `effect`, `@effect/experimental`, `@effect/platform`, `@effect/rpc`)
- pages:
  - [api-effect-atom.md](api-effect-atom.md)

[STATE_VIEW]:
- scope: the `browser`-tier React binding of the `[STATE_CELL]` atom engine — owns ALL React store subscriptions for the view/UI surface (the `[UI_STACK]` page defers state to it). The `index.d.ts` barrel re-exports the seven framework-neutral `@effect-atom/atom` core namespaces verbatim (`Atom`/`Registry`/`Result`/`AtomRef`/`AtomHttpApi`/`AtomRpc`/`Hydration` — full member surface on `api-effect-atom.md`, not re-transcribed) and adds its OWN React modules: the `Hooks` subscription family (`useAtom`/`useAtomValue`/`useAtomSet`/`useAtomSuspense`/`useAtomRefresh`/`useAtomMount`/`useAtomSubscribe`/`useAtomRef`/`useAtomRefProp`/`useAtomRefPropValue`/`useAtomInitialValues`, with `Mode` "value"/"promise"/"promiseExit" writer discrimination), `RegistryContext`/`RegistryProvider`/`scheduleTask`, the per-tree `ScopedAtom` factory, and the SSR `HydrationBoundary` component
- packages: `@effect-atom/atom-react` (peer `@effect-atom/atom`; `react`; transitive `effect`, `@effect/platform`, `@effect/rpc`, `@effect/experimental`)
- pages:
  - [api-effect-atom-atom-react.md](api-effect-atom-atom-react.md)

[BROWSER_TIER]:
- scope: browser-bundle platform bindings — runtime entry, HTTP/Socket/Worker/KeyValueStore drivers, DOM event streams, browser-API service capsules
- packages: `@effect/platform-browser`
- pages:
  - [api-effect-platform-browser.md](api-effect-platform-browser.md)

[NODE_TIER]:
- scope: node-bundle platform bindings — composite runtime context, main-process runner, HTTP client (agent + undici) / server, FileSystem/Path/CommandExecutor/Terminal/KeyValueStore service layers, TCP+WebSocket sockets, stream/sink bridge, worker pool, cluster sharding
- packages: `@effect/platform-node`
- pages:
  - [api-effect-platform-node.md](api-effect-platform-node.md)

[CLI]:
- scope: typed-command binding — `Command` tree with `Effect` handlers, `Options`/`Args`/`Prompt` parameter algebra, `CliConfig` parser policy, closed `ValidationError` rail, `HelpDoc`/`Usage` doc model, `ConfigFile` provider; executed via `Command.run` under host `Environment` (`FileSystem | Path | Terminal`)
- packages: `@effect/cli` (peers `@effect/platform`, `effect`, `@effect/printer-ansi`)
- pages:
  - [api-effect-cli.md](api-effect-cli.md)

[UI_STACK]:
- scope: React view layer, headless UI primitives, table/virtualization, build tooling
- packages: `react`, `react-dom`, `react-aria`, `react-aria-components`, `@radix-ui/*`, `@tanstack/react-table`, `@tanstack/react-virtual`, `vite`, `@vitejs/plugin-react`, `vite-plugin-pwa`
- pages:
  - [api-ui-stack.md](api-ui-stack.md)

[AI_CORE]:
- scope: the provider-agnostic `@effect/ai` core that every provider adapter binds to — the `LanguageModel`/`EmbeddingModel`/`Tokenizer`/`Chat`/`IdGenerator` `Context.Tag` services, the `Model`/`Model.ProviderName` adapter that tags a layer with a provider id, the `Prompt`/`Response`/`Tool`/`Toolkit` data algebra (paired decoded/`*Encoded` interfaces + `Schema` constants), the `AiError` tagged-error union, the `Telemetry` GenAI semantic-convention attributes, and the `McpServer`/`McpSchema` modules; the concrete provider client/model/tokenizer bindings are catalogued on the per-provider pages below
- packages: `@effect/ai` (peers `effect`, `@effect/platform`)
- pages:
  - [api-effect-ai.md](api-effect-ai.md)

[AI_PROVIDER]:
- scope: Anthropic-backed `@effect/ai` provider — `AnthropicClient` HTTP service + `layer`/`layerConfig`, `AnthropicLanguageModel` model/layer family with `@effect/ai/Prompt`+`Response` provider-options/metadata augmentations, `AnthropicConfig` per-effect HTTP transform, `AnthropicTokenizer`, provider-defined `AnthropicTool` family, and the `Generated` OpenAPI codec module (request/response wire schemas + typed `Client` endpoint surface)
- packages: `@effect/ai-anthropic` (peers `@effect/ai`, `@effect/platform`, `effect`)
- pages:
  - [api-effect-ai-anthropic.md](api-effect-ai-anthropic.md)

[TRANSPORT_WIRE]:
- scope: gRPC-Web transport, protobuf descriptors/codegen, msgpack decode
- packages: `@connectrpc/connect`, `@connectrpc/connect-web`, `@bufbuild/protobuf`, `@bufbuild/buf`, `msgpackr`
- pages:
  - [api-transport-wire.md](api-transport-wire.md)

[INFRA_DATA]:
- scope: node-tier deploy/provisioning resources, Redis/S3 clients, geo/map rendering
- packages: `@pulumi/*`, `ioredis`, `@aws-sdk/client-s3`, `maplibre-gl`, `deck.gl`
- pages:
  - [api-infra-data.md](api-infra-data.md)

[AI_PROVIDERS]:
- scope: provider bindings onto the provider-agnostic `@effect/ai` `LanguageModel`/`EmbeddingModel`/`Tool`/`Tokenizer`/`Telemetry` contracts — per-provider transport client + streaming surface, language/embedding model layers, provider-defined tools, optional tokenizer/telemetry, and the OpenAPI-derived `Generated` REST/schema corpus; OpenAI is the full binding (49-member Responses-API streaming union, embedding+tokenizer+telemetry modules), Google is the narrower Gemini binding (`generateContent` stream, language-model + 4 provider tools only, no embedding/tokenizer/telemetry module), Amazon Bedrock is the Converse/ConverseStream binding (`AmazonBedrockClient` `converse`/`converseStream` service, language-model layer family with no tokenizer, Anthropic-on-Bedrock provider tools reusing `@effect/ai-anthropic/Generated`, the hand-written `AmazonBedrockSchema` Converse codec + full guardrail-trace corpus + 90-id `BedrockFoundationModelId` catalogue, and the `EventStreamEncoding` AWS event-stream channel parser)
- packages: `@effect/ai-openai`, `@effect/ai-google`, `@effect/ai-amazon-bedrock`, `@effect/ai-openrouter`
- pages:
  - [api-effect-ai-openai.md](api-effect-ai-openai.md)
  - [api-effect-ai-google.md](api-effect-ai-google.md)
  - [api-effect-ai-amazon-bedrock.md](api-effect-ai-amazon-bedrock.md)
  - [api-effect-ai-openrouter.md](api-effect-ai-openrouter.md)

[LINT_TOOLCHAIN]:
- scope: build-time ESLint flat-config plugin — `meta`/`rules`/`configs` plugin object, the `dprint` formatter-as-lint rule (11-message-ID `RuleModule`) and `no-import-from-barrel-package` rule, `configs/dprint` + `configs/disable-conflict-rules` stylistic-disable presets (~106 keys), `utils/eslint` `createRule`/`getParserServices` authoring primitives, `RegularExpression` whitespace/linebreak predicates, and the opaque `ConfigSchema` (`JSONSchema4`); a dev-tool import, not a runtime bundle dependency
- packages: `@effect/eslint-plugin` (rule typing via `@typescript-eslint/utils`)
- pages:
  - [api-effect-eslint-plugin.md](api-effect-eslint-plugin.md)

[OBSERVABILITY]:
- scope: OpenTelemetry export bridge for the Effect runtime — adapts the `effect/Tracer`/`Logger`/`Metric` channels onto OTLP either through the official `@opentelemetry/sdk-*` peers (`Resource` tag + `Tracer`/`Logger`/`Metrics` bridges + `NodeSdk`/`WebSdk` composites) or through the dependency-light direct-OTLP module family (`Otlp` + `OtlpTracer`/`OtlpLogger`/`OtlpMetrics` over `@effect/platform`'s `HttpClient`, with the `OtlpSerialization` JSON/protobuf seam and `OtlpResource` wire model)
- packages: `@effect/opentelemetry` (peers `@opentelemetry/api`, `@opentelemetry/resources`, `@opentelemetry/sdk-logs`, `@opentelemetry/sdk-metrics`, `@opentelemetry/sdk-trace-base`, `@opentelemetry/sdk-trace-node`, `@opentelemetry/sdk-trace-web`, `@opentelemetry/semantic-conventions`, `@effect/platform`, `effect`)
- pages:
  - [api-effect-opentelemetry.md](api-effect-opentelemetry.md)

[TESTING]:
- scope: dev/test-time Effect-aware Vitest binding — `export *` over the full `vitest` surface plus the Effect test runner (`it.effect`/`scoped`/`live`/`scopedLive`), the `Layer`-sharing harness (`layer`/`describeWrapped`/`makeMethods`, `MethodsNonLive` + `excludeTestServices`/`memoMap`), the fast-check property runner (`prop`/`Tester.prop`/`Tester.each` over `Vitest.Arbitraries`), the `Equal`-trait Vitest equality tester (`addEqualityTesters`), the flaky-retry combinator (`flakyTest`), and the closed `@effect/vitest/utils` assertion family (type-narrowing `assertNone`/`assertSome`/`assertLeft`/`assertRight`/`assertFailure`/`assertSuccess` over `Equal`/`Option`/`Either`/`Exit`/`Cause`)
- packages: `@effect/vitest` (peers `effect`, `vitest`)
- pages:
  - [api-effect-vitest.md](api-effect-vitest.md)

[EFFECT_TOOLING]:
- scope: dev-tooling surface with no value-import — the `@effect/language-service` TypeScript LSP plugin + `effect-language-service` CLI + `tsc` patcher; ships only bundled JS + `schema.json` (no `.d.ts`, no `exports`/`types`), so `assay api` reflects 0 types and the contract is the `tsconfig.json` plugin-options object (30 keys), the 76-rule diagnostic catalogue with per-rule default severities (`anyUnknownInErrorContext`…`unnecessaryPipeChain`), the `@effect-diagnostics`/`@effect-diagnostics-next-line`/`@effect-codegens`/`@effect-identifier` comment-directive language, and the 11-command CLI (`setup`/`config`/`patch`/`unpatch`/`check`/`diagnostics`/`quickfixes`/`codegen`/`overview`/`layerinfo`); configured (root `tsconfig.json`, last in `plugins[]`) and `tsc`-patched, never imported
- packages: `@effect/language-service` (devDependency)
- pages:
  - [api-effect-language-service.md](api-effect-language-service.md)

[DEV_TOOLING_TSGO]:
- scope: the TypeScript-Go delivery of the Effect Language Service — `@effect/tsgo` ships a single CLI binary (`effect-tsgo` bin, bundled `dist/effect-tsgo.js`) plus one platform-native Go binary, embedding a pinned upstream Microsoft `tsgo` commit patched with the `@effect/language-service` plugin (the `[EFFECT_TOOLING]` package above is the editor-plugin contract this binary delivers natively). No programmatic surface (`assay api` reflects 0 types: no `main`/`exports`/`types`, no `.d.ts`); the contract is the CLI subcommand grammar (`setup`/`config`/`patch`/`unpatch`/`get-exe-path` + global flags), the `@effect/language-service` tsconfig-plugin-options schema it writes during `setup`, and the diagnostic/refactor/completion/codegen/rename rule catalog over Effect V4/V3 code. Replaces (does not augment) upstream `tsgo`; companion `@typescript/native-preview` still required at alpha.
- packages: `@effect/tsgo` (dev tooling; platform binary from `@effect/tsgo-{win32,linux,darwin}-{x64,arm,arm64}` optional deps)
- pages:
  - [api-effect-tsgo.md](api-effect-tsgo.md)

## [2]-[CATALOGUE_LAW]

[PAGE_SCOPE]:
- API pages carry reflection-grounded external package API facts and tier/admission records.
- Planning pages carry owner boundaries and source-transcription law.
- README pages route catalogues without duplicating member tables.

[EVIDENCE_ROUTE]:
- Surface is captured through `uv run python -m tools.assay api query --key <package>`, with `--symbol` for member-level detail and `--full` for the complete declaration dump; the reflected `.d.ts` set under `node_modules/<package>/dist/dts` is authoritative over any published or remembered surface.
- A package absent from the active install preserves its page with un-reflected members recorded in a gaps section, never invented.

[TIER_LAW]:
- Pages record the Nx `@nx/enforce-module-boundaries` tag (`browser` / `node` / `neutral`) governing each package so planning owners cannot couple a browser surface into the node bundle or the reverse.
