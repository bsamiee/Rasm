# [TYPESCRIPT_TEST_STRATEGY]

One page owns the verification law for the TS lib branch — the algebraic property-testing posture every Effect module is held to, the browser-mode runner for the atom-bound view surfaces, and the ephemeral-container harness that proves the node-tier durable cluster against real engines. Testing is a whole concern no domain page owns: it spans every page's spec posture, the per-file budget and coverage floor, the mutation kill-ratio gate, and the layer-aware Effect bridge. The page authors three clusters along the verification altitude — unit-and-property, browser-and-end-to-end, mutation-and-harness — and crosses no wire contract; every spec consumes the owner symbols the domain pages declare and never re-mints a shape.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]            | [OWNS]                                                          |
| :-----: | :------------------- | :-------------------------------------------------------------- |
|   [1]   | UNIT_AND_PROPERTY    | the algebraic PBT spine, the LOC budget, and the coverage floor |
|   [2]   | BROWSER_AND_E2E      | the DOM-mode runner and the deep-link end-to-end flow proof     |
|   [3]   | MUTATION_AND_HARNESS | the mutation kill-ratio gate and the durable-cluster harness    |

## [2]-[UNIT_AND_PROPERTY]

- Owner: `UnitProperty`, the spec spine over the Effect-aware runner and the property generator; every pure transform and every fold algebra is graded by algebraic law, never by hand-enumerated example.
- Cases: pure domain logic — the decode-rail folds, the store key-discriminated folds, the quarantine terminal, the receipt skew-band fold — proves identity, inverse, idempotence, commutativity, associativity, and determinism laws as the external oracle, so a passing spec cannot re-derive the implementation; the Effect bridge auto-provides the test clock and test services so a `Schedule`-driven reconnect or a staleness-marker transition proves deterministically against virtual time; a `Schema.Class` boundary shape passes directly into the property runner as its arbitrary so the encode-decode round-trip law stays synchronized with the wire shape it grades; the algebraic property packs two-to-four laws sharing one arbitrary shape into one property so coverage is density, not file count.
- Entry: one spec per source module per category; the per-file budget is a hard cap and the per-file coverage floor is statements, branches, and functions measured independently; the spec sections order constants then helpers then layer then mocks then algebraic then edge-cases, and the edge-case section appears only when the property set leaves a branch below the floor.
- Packages: the Effect-aware vitest bridge for layer-shared suites and test services, the vitest runner core, the property-based generator for algebraic laws and model-based command sequences, and the v8 coverage provider for the per-file floor.
- Growth: a new pure owner lands as one spec graded by its applicable law set; a new law on an existing owner packs into the property already covering its arbitrary; a new coverage gap lands as one edge-case arm, never a second spec file.
- Boundary: a spec that re-derives expected values from the source it grades is the named circular-test defect the mutation gate deletes; example-only assertions where an algebraic law exists are the named defect; the unit spine holds no real infrastructure — a boundary touching a real engine routes to the harness cluster.

```ts contract
type SpecBudget = {
  readonly maxFileLoc: 175;
  readonly coverageFloorPct: 95;
  readonly coverageDimensions: ReadonlyArray<"statements" | "branches" | "functions">;
  readonly oneSpecPerModulePerCategory: true;
};

type TestCategory =
  | "unit-pbt"
  | "integration"
  | "contract"
  | "system"
  | "e2e";

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

type EffectSpecRunner = {
  readonly effect: "auto-provides TestClock and TestServices";
  readonly prop: "Schema or arbitrary quantified property";
  readonly scoped: "Scope-managed acquire-release coverage";
  readonly layer: "single materialization per suite";
};
```

## [3]-[BROWSER_AND_E2E]

- Owner: `BrowserE2E`, the DOM-mode spec surface for the atom-bound view layer and the end-to-end flow proof for the deep-link and command-intent routing; the only verification altitude where DOM behavior is load-bearing.
- Cases: the browser-mode runner instantiates the real React renderer over the headless browser so an `AtomBinding` leaf subscriber, a virtualized table, and the `GeoSeriesSurface` map composition prove their subscription against a real DOM rather than a JSDOM shim; the end-to-end driver walks the deep-link and intent-routing flow — a `DeepLinkBinding` query-string key resolves through `IntentRegistry` into `CommandGateway` and the resulting availability fold gates the affordance — so the survives-a-reload claim is proven as a real navigation, not asserted; the auth login-logout affordance and the session-status leaf prove their re-auth typed-fault path through the same browser flow.
- Entry: the browser-mode runner shares the vitest project surface with the unit spine so one runner configuration carries both the node-mode pure specs and the browser-mode view specs as inline projects; the end-to-end flow specs are driven by the playwright agent pipeline and are never hand-authored as assertions.
- Packages: the playwright-backed browser provider for the vitest runner, the playwright end-to-end driver for the navigation flows, and the React atom binding under test.
- Growth: a new leaf surface lands as one browser-mode spec over its store subscription; a new deep-link flow lands as one end-to-end navigation through the registry and gateway.
- Boundary: the browser cluster proves DOM-load-bearing behavior only; a pure fold reachable without a DOM routes to the unit spine; the end-to-end flows never assert against mocked transports — they drive the real composition root the runtime-host page owns.

## [4]-[MUTATION_AND_HARNESS]

- Owner: `MutationHarness`, the mutation kill-ratio gate over the unit spine and the ephemeral-container harness that proves the node-tier durable cluster against real engines.
- Cases: the mutation runner injects operator-swap, conditional-negation, and statement-deletion mutants over the source and fails the build below the break threshold, so a circular spec that cannot kill a mutant is surfaced as a defect; the typescript checker eliminates compile-error mutants before run and the related-test scoping runs only the specs touching each mutant; the container harness stands up an ephemeral Postgres and the Redis backplane so `WorkflowOwner`, `ActivityOwner`, `ClusterEngine`, and `RunnerBackplane` prove their exactly-once and durable-replay semantics against the real engines the node-tier page wires, never against a mock; the message-storage and runner-storage rows prove durable replay across a simulated runner restart inside the container lifecycle.
- Entry: the mutation gate runs after the semantic gate over the unit spine with the typescript checker enabled and dynamic concurrency; the kill-ratio carries a break threshold that fails the build, a low threshold that triggers investigation, and a high threshold as the contract-driven target; the container harness is a node-tier integration category and is bound to the durable cluster owners, never to a browser surface.
- Packages: the mutation runner core with its vitest runner plugin and the typescript checker plugin, and the ephemeral-container library for the Postgres and Redis backplane the durable tier consumes.
- Growth: a new durable owner lands as one container-harness integration spec against the real engine; a raised kill-ratio target tightens the existing thresholds, never a second mutation surface.
- Boundary: the mutation gate grades the unit spine, never the browser or end-to-end surfaces where DOM and navigation cost dominate; the container harness proves durable semantics against real engines only — a mock standing in for a real engine in the durable tier is the named defect; the harness crosses no wire contract and provisions nothing the node-tier page owns.

```ts contract
type MutationThresholds = {
  readonly break: 50;
  readonly low: 60;
  readonly high: 80;
  readonly checker: "typescript";
  readonly relatedTestScoping: true;
};

type DurableHarness = {
  readonly engines: ReadonlyArray<"postgres" | "redis-backplane">;
  readonly lifecycle: "ephemeral container per suite";
  readonly proves: ReadonlyArray<
    | "exactly-once-workflow"
    | "durable-replay"
    | "runner-restart-recovery"
  >;
};
```
