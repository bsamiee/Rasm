# [TYPESCRIPT_TEST_STRATEGY]

One page fixes the verification law across the five-domain flat TS lib branch — the algebraic property-testing posture every Effect module is held to, the browser-mode runner for the atom-bound `ui` surfaces and the `platform` SPA composition, and the ephemeral-container harness that proves the `services` cluster against real engines. Testing spans every domain's spec posture, the per-file budget and coverage floor, the mutation kill-ratio gate, and the layer-aware Effect bridge. The neutral and node strata (`interchange`, `projection`, `services`) each contribute a node-mode `project` row configured in the monorepo's CENTRALIZED root vitest setup (the existing vite/vitest setup); the two browser folders (`ui` + `platform`) share ONE browser-mode `project` named `browser` because the playwright provider is folder-agnostic and the browser publication is one runtime — a second runner config per browser folder is the named test-config defect. Each domain carries a per-domain `stryker.config.mjs`, never five divergent runner configs. The test-config contract shapes (`SpecBudget`, `TestCategory`, `AlgebraicLaw`, `MutationThresholds`, `DurableHarness.engines`, `WebProject.proves`) are runner-configuration vocabularies stated as inline literal unions, exempt from the domain `Schema.Literal`-plus-`Record` form because they never decode a wire value or carry a domain shape. This is a `CROSS_PACKAGE_LAWS`-tier page, not a domain page.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]            | [OWNS]                                                          |
| :-----: | :------------------- | :-------------------------------------------------------------- |
|   [1]   | UNIT_AND_PROPERTY    | the algebraic PBT spine, the LOC budget, and the coverage floor |
|   [2]   | BROWSER_AND_E2E      | the DOM-mode runner and the deep-link end-to-end flow proof     |
|   [3]   | MUTATION_AND_HARNESS | the mutation kill-ratio gate and the durable-cluster harness    |

## [2]-[UNIT_AND_PROPERTY]

- Owner: `UnitProperty`, the spec spine over the Effect-aware runner and the property generator; every pure transform and every fold algebra is graded by algebraic law, never by hand-enumerated example.
- Cases: pure domain logic — the `DecodeRail` codec folds, the `ArtifactFrameRail` offset-ordered frame stitch with its Crc32-verify and XxHash128 content-key derivation (proving associativity of the offset-ordered accumulation and determinism of the content key over reordered-then-sorted frames), the `QuarantineFold` tolerance terminal, the `HealthStore`/`ProgressStore`/`ConflictPresenceStore`/`AvailabilityStore` key-discriminated folds, the `ReceiptStore` union fold, and the `EvidenceFeed` HLC skew-band confidence-interval fold — proves identity, inverse, idempotence, commutativity, associativity, monotonicity, and determinism laws as the external oracle, so a passing spec cannot re-derive the implementation; the `it.prop` arbitrary is the boundary `Schema.Class` itself so the encode-decode round-trip law stays synchronized with the wire shape it grades and no second arbitrary is hand-minted; a `Schedule`-driven reconnect-and-backoff retry, a `RuntimeFeed` phase transition, and an `AuthSession` `SubscriptionRef` silent-refresh fold prove deterministically against `it.effect`'s auto-provided `TestClock` and `TestServices`; a fiber-interruption or scope-acquire-release law rides `it.scoped`; the algebraic property packs two-to-four laws sharing one arbitrary into one `it.prop` so coverage is density, not file count; a model-based command sequence over `fc.commands` + `fc.asyncModelRun` grades a stateful fold (`AvailabilityStore` gate transitions, `ProgressStore` monotone marks) against a reference model the spec carries.
- Entry: one spec per source module per category, `<module>.<category>.spec.ts` beside the module; the per-file budget is a hard `175` LOC cap and the per-file coverage floor is `95` percent on statements, branches, and functions measured independently; the spec sections order constants then helpers then layer then mocks then algebraic then edge-cases, and the edge-case section appears only when the property set leaves a branch below the floor; `it.layer` materializes a suite-shared `Layer` once so every `it.effect` in the block reuses one composition rather than re-provisioning services per case.
- Packages: the `@effect/vitest` bridge auto-providing `TestClock` and `TestServices` for layer-shared suites, the vitest runner core, `fast-check` owning `fc.property`/`fc.asyncProperty` and the `fc.commands`/`fc.asyncModelRun` model-based sequences, and the v8 coverage provider for the per-file floor.
- Growth: a new pure owner lands as one spec graded by its applicable law set; a new law on an existing owner packs into the `it.prop` already covering its arbitrary; a new coverage gap lands as one edge-case arm, never a second spec file.
- Boundary: a spec that re-derives expected values from the source it grades is the named circular-test defect the mutation gate deletes; an example-only assertion where an algebraic law exists is the named defect; the unit spine holds no real infrastructure — a `vi.fn` transport mock standing where a layer-provided test service exists is the named defect, and a boundary touching a real engine routes to the harness cluster.

```ts contract
type SpecBudget = {
  readonly maxFileLoc: 175;
  readonly coverageFloorPct: 95;
  readonly coverageDimensions: ReadonlyArray<"statements" | "branches" | "functions">;
  readonly oneSpecPerModulePerCategory: true;
  readonly fileSuffix: "<module>.<category>.spec.ts";
};

type TestCategory = "unit-pbt" | "integration" | "contract" | "system" | "e2e";

type AlgebraicLaw =
  | "identity"
  | "inverse"
  | "idempotent"
  | "commutative"
  | "associative"
  | "homomorphism"
  | "annihilation"
  | "monotonicity"
  | "reflexive"
  | "symmetric"
  | "transitive"
  | "determinism"
  | "preservation"
  | "fiber-interruption"
  | "resource-cleanup";

interface UnitProperty {
  readonly effect: <A, E>(name: string, self: () => Effect.Effect<A, E, TestServices.TestServices>) => void;
  readonly scoped: <A, E>(name: string, self: () => Effect.Effect<A, E, Scope.Scope | TestServices.TestServices>) => void;
  readonly prop: <const Arbs extends ReadonlyArray<Schema.Schema.Any | FC.Arbitrary<unknown>>, A, E>(
    name: string,
    arbitraries: Arbs,
    self: (args: { [K in keyof Arbs]: Arbs[K] extends FC.Arbitrary<infer T> ? T : Schema.Schema.Type<Arbs[K]> }) => Effect.Effect<A, E, TestServices.TestServices>,
  ) => void;
  readonly layer: <R, E>(layer: Layer.Layer<R, E>) => (name: string, body: (it: UnitProperty) => void) => void;
  readonly command: <Model, Real>(commands: ReadonlyArray<FC.Arbitrary<FC.AsyncCommand<Model, Real>>>) => FC.Arbitrary<Iterable<FC.AsyncCommand<Model, Real>>>;
}
```

## [3]-[BROWSER_AND_E2E]

- Owner: `BrowserE2E`, the DOM-mode spec surface for the `ui` atom-bound layer and the `platform` SPA composition, and the end-to-end flow proof for the deep-link and command-intent routing; the only verification altitude where DOM behavior is load-bearing. The one `browser` project covers both `ui/**` and `platform/**` specs.
- Cases: the playwright-backed browser provider instantiates the real React renderer over the headless browser so an `AtomBinding` leaf subscriber, a virtualized table, the role-based component-system surfaces, and the `GeoSeriesSurface` maplibre-plus-deck.gl map composition prove their subscription against a real DOM rather than a JSDOM shim; the end-to-end driver walks the deep-link and intent-routing flow — a `DeepLinkBinding` query-string key resolves through `IntentRegistry` into `CommandGateway` and the `AvailabilityStore` fold gates the affordance — so the survives-a-reload claim is proven as a real navigation, not asserted; the `AuthSession` login-logout affordance and the session-status leaf prove their re-auth path by folding an expired-or-rejected token to the `FaultDetailRail` typed fault through the same browser flow; offline persistence proves a redial restores from the `LocalPersistence` last-good store rather than a cold boot.
- Entry: the browser-mode runner shares the monorepo's centralized root vitest setup with the unit spine so one workspace config carries both the node-mode pure specs (the `interchange`/`projection`/`services` project rows) and the browser-mode specs of the two browser folders as ONE `browser` project whose `include` globs both `ui/**` and `platform/**`, keyed by environment; the `browser` project sets the playwright provider with a headless chromium instance; the end-to-end navigation flows are driven by the playwright test driver against the real `platform` `CompositionRoot` and are never hand-authored as transport assertions.
- Packages: the playwright-backed browser provider for the vitest runner, the playwright end-to-end test driver for the navigation flows, the React atom binding under test, and the vitest runner core sharing the project surface.
- Growth: a new leaf surface lands as one browser-mode spec over its store subscription; a new deep-link flow lands as one end-to-end navigation through the registry and gateway, never a second runner configuration.
- Boundary: the browser cluster proves DOM-load-bearing behavior only; a pure fold reachable without a DOM routes to the unit spine; the end-to-end flows never assert against mocked transports — they drive the real `CompositionRoot` the `platform` host-runtime page owns, and the `services` node-tier owners are unreachable from this surface because the `./web` browser entry excludes them by the folder-scoped `browser`/`node` import strata.

```ts contract
type WebProject = {
  readonly name: "browser";
  readonly provider: "playwright";
  readonly instances: ReadonlyArray<{ readonly browser: "chromium"; readonly headless: true }>;
  readonly proves: ReadonlyArray<"atom-subscription" | "virtualized-table" | "geo-composition" | "auth-leaf" | "offline-restore">;
};

type DeepLinkFlow = {
  readonly entry: "DeepLinkBinding";
  readonly route: ReadonlyArray<"IntentRegistry" | "CommandGateway" | "AvailabilityStore">;
  readonly proves: "survives-reload-as-real-navigation";
  readonly faultPath: "FaultDetailRail re-auth fold, never silent redirect";
};
```

## [4]-[MUTATION_AND_HARNESS]

- Owner: `MutationHarness`, the mutation kill-ratio gate over the unit spine and the ephemeral-container harness that proves the `services` durable cluster against real engines.
- Cases: the stryker runner injects operator-swap, conditional-negation, and statement-deletion mutants over the source and fails the build below the break threshold, so a circular spec that cannot kill a mutant is surfaced as a defect; the typescript checker plugin eliminates compile-error mutants before run so a mutant that fails the strictness floor never spends a test run; the vitest runner plugin scopes each mutant to the related specs touching it; the container harness stands up an ephemeral Postgres and the Redis backplane so `WorkflowOwner`, `ActivityOwner`, `ClusterEngine`, `RunnerBackplane`, and `SqlBoundary` prove their exactly-once, durable-replay, and RLS-scoped semantics against the real engines `services` wires, never against a mock — the SQL message store and SQL runner store row over the Postgres adapter and the multi-node backplane over the Redis client; the message-storage and runner-storage rows prove durable replay across a simulated runner restart inside the container lifecycle, a `ScheduledWork` cluster singleton proves its exactly-one-runner pin across the same restart, and the hybrid-search owner proves its fused weighted-rank against the real `pg_trgm`/HNSW extensions provisioned in the container.
- Entry: the mutation gate runs after the semantic gate over each domain's unit spine through that domain's `stryker.config.mjs` with the typescript checker plugin enabled and the vitest runner plugin scoping related tests under dynamic concurrency; the kill-ratio carries a break threshold that fails the build, a low threshold that triggers investigation, and a high threshold as the contract-driven target; the container harness is a `services` integration category bound to the durable cluster owners through the real engines, one ephemeral container set acquired and released per suite through an `Effect.acquireRelease` scope so the lifecycle is fiber-scoped, never a browser surface.
- Packages: `@stryker-mutator/core` with its `@stryker-mutator/vitest-runner` plugin and its `@stryker-mutator/typescript-checker` plugin, `testcontainers` for the Postgres and Redis engines the durable tier consumes, and the `@effect/vitest` bridge providing the scoped lifecycle for the harness suites.
- Growth: a new durable owner lands as one container-harness integration spec against the real engine; a raised kill-ratio target tightens the existing thresholds, never a second mutation surface; a new engine lands as one container row on the harness, never a parallel harness.
- Boundary: the mutation gate grades each domain unit spine, never the browser or end-to-end surfaces where DOM and navigation cost dominate; the container harness proves durable and persistence semantics against real engines only — a mock standing in for a real engine in the services tier is the named defect; the harness crosses no wire contract, provisions nothing the `services` `provisioning` lifecycle owns, and consumes the durable owners as settled vocabulary.

```ts contract
type MutationThresholds = {
  readonly break: 50;
  readonly low: 60;
  readonly high: 80;
  readonly checker: "typescript";
  readonly testRunner: "vitest";
  readonly relatedTestScoping: true;
  readonly concurrency: "dynamic";
};

interface DurableHarness {
  readonly engines: ReadonlyArray<"postgres" | "redis-backplane">;
  readonly lifecycle: Effect.Effect<{ readonly postgres: StartedTestContainer; readonly redis: StartedTestContainer }, never, Scope.Scope>;
  readonly proves: ReadonlyArray<
    | "exactly-once-workflow"
    | "durable-replay"
    | "runner-restart-recovery"
    | "singleton-pin"
    | "rls-tenant-scope"
    | "hybrid-search-rank"
  >;
}
```
