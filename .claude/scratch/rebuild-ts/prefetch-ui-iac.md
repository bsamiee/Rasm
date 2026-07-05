# PREFETCH DOSSIER — UI DEPTH + IAC DEPTH (TS rebuild, Effect-native)

Verified 2026-07-04 against npm registry, GitHub releases/source, Context7, and current project docs. Verdict vocabulary: `admit-substrate` (central manifest shared dep) / `admit-folder` (folder dep or provision-as-typed-target) / `mine-design-only` (internalize capability, no dep) / `reject`.

## [A]-[UI_DEPTH]

### [A1] Motion (post-Framer rename)

| name | version | license | maintenance | VERDICT | justification |
|---|---|---|---|---|---|
| `motion` | 12.42.2 (2026-07-01) | MIT | sub-weekly patch cadence (12.42.0 Jun 24 -> 12.42.2 Jul 1) | admit-folder | React-19-native (`peerDependencies: react ^18 || ^19`, optional), hybrid rAF/WAAPI engine with compositor offload; single canonical animation owner for the UI folder |
| `framer-motion` | 12.42.2 (lockstep alias) | MIT | rename target, not independently developed | reject | superseded by `motion`; never carry both |
| `motion-plus-react` | 1.5.4 | MIT (membership-gated) | active (2026 AI Kit, `AnimateActivity`, `<AnimateView>`, Split Text) | reject | paid membership dep; core `motion` + native View Transitions cover the folder's needs |

- Entry-point split: `motion/react` (full hybrid, ~17kb), `motion/react-mini` (WAAPI-only, ~2.3kb), `motion` vanilla (`animate`, `scroll`, `stagger` — drives DOM/Three/canvas), `motion-v` (Vue, irrelevant).
- Verified exports from `motion/react`: `motion`, `AnimatePresence`, `useSpring`, `useInView`, `useVelocity`, `useMotionValueEvent`, `useAnimate` (also in `motion/react-mini`). Verified vanilla: `animate`, `scroll`, `stagger` (used as `delay: stagger(0.1)`). `layout`/`layoutId` are props on `motion.*` components; `AnimateSharedLayout` is long-removed — `layoutId` alone drives shared-layout across unrelated trees.
- Pattern-inferred (sibling-hook path, not literal-fetched — re-verify at implementation): `useScroll`, `useTransform`, `useMotionValue`, `LayoutGroup`, `MotionConfig` from `motion/react`.
- `animateView()` is now open-source in core `motion` (promoted out of Motion+): typed spring-physics layer over `document.startViewTransition` with interruption handling and automatic `view-transition-name` management. The React `<AnimateView>` component remains paid — do not depend on it.

### [A2] View Transitions API

| surface | status (2026-07) | VERDICT | justification |
|---|---|---|---|
| `document.startViewTransition` (same-doc) | Baseline: Chrome/Edge 111+, Safari 18+, Firefox 144+ (~88.6% usage) | mine-design-only | cross-engine now; consume through `motion` `animateView()` — no separate typed owner |
| `@view-transition { navigation: auto }` (cross-doc/MPA) | Chrome/Edge 126+, Safari 18.2+, Firefox unsupported through 155 (~81.3%) | mine-design-only | pure-CSS opt-in, silent Firefox no-op; progressive enhancement only |
| React `<ViewTransition>` / `unstable_ViewTransition` + `unstable_addTransitionType` | Canary/experimental only — NOT in React 19.2.7 stable | reject | unshippable on stable; `animateView()` already covers the same problem today |

Design law: native API is the target; `animateView()` is the ergonomic layer. React's component (props `name`, `enter`/`exit`/`update`/`share`/`default`, `onEnter`... callbacks; fires only inside `useTransition`/`Suspense`/`useDeferredValue`) is watch-list material for a future React minor, not a 2026 dependency.

### [A3] Scroll-driven animations

| surface | status (2026-07) | VERDICT | justification |
|---|---|---|---|
| CSS `animation-timeline` / `scroll()` / `view()` | Chrome/Edge 115+, Safari 26+ (NOT 18 — common misclaim), Firefox 155+ unshipped (stable is 152); ~82.6%, not Baseline | admit-folder | thin typed CSS-class/data-attr owner gated on `@supports (animation-timeline: scroll())`; decorative parallax/reveal where Firefox no-op is acceptable |
| JS `ScrollTimeline`/`ViewTimeline` | same matrix | reject | `motion` `useScroll`/`useTransform` gives full four-engine parity today; a second scroll engine is split-brain |
| `scroll-timeline-polyfill` (flackr) | maintained, Apache-2.0 | reject | closes a gap `motion` already closes with better React ergonomics and zero polyfill weight |

### [A4] Container queries

| surface | status (2026-07) | VERDICT | justification |
|---|---|---|---|
| `@container` / `container-type` / `cqw cqh cqi cqb cqmin cqmax` | Baseline since 2023 (Chrome 105+, Firefox 110+, Safari 16+) | mine-design-only | zero-gap; Tailwind v4 core ships `@container`, `@sm:`/`@lg:`/`@[500px]:`, `@max-*`, named `@container/name` natively |
| `@container style(...)` | Chrome/Edge 111+ partial (custom-property conditions only), Safari 18+ partial, Firefox 151+ full | mine-design-only | usable now for custom-property-driven variants/theming; not yet a general style-predicate surface |
| `@tailwindcss/container-queries` plugin | superseded by Tailwind v4 core | reject | capability is native in v4; never add to manifest |

### [A5] React 19.x current capabilities

Stable floor: `react`/`react-dom` **19.2.7** (June 2026). Lines 19.0.7/19.1.8/19.2.7 concurrently patched; no 19.3/20. Security floor: CVE-2025-55182 (RSC RCE) patched only at 19.0.1/19.1.2/19.2.1+ on `react-server-dom-*` — verify any pin clears it.

`api | stability | since | capability`
- `useTransition` async Actions | stable | 19.0 | automatic pending/error/race handling around async handlers — kills hand-rolled `isLoading`/try-catch/race-guard triples
- `useActionState(action, initialState, permalink?)` -> `[state, dispatchAction, isPending]` | stable | 19.0 | reducer-shaped form/button action owner
- `useFormStatus()` -> `{pending, data, method, action}` | stable | 19.0 (`react-dom`) | reads enclosing form's Action pending state, zero prop-drilling
- `<form action={fn}>` / `formAction` | stable | 19.0 | function actions + auto-reset of uncontrolled inputs; kills manual `preventDefault` + `FormData` plumbing
- `use(promise)` / `use(context)` | stable | 19.0 | conditional/loop-legal (never inside try/catch; rejections -> Error Boundaries); context reads after early returns
- `useOptimistic(state, reducer?)` | stable | 19.0 | optimistic set must run inside an Action/transition to get auto re-sync
- `<Activity mode="visible"|"hidden">` | stable | 19.2 | keep-alive DOM+state with paused Effects; background pre-render + instant back-nav
- `useEffectEvent` | stable | 19.2 | non-reactive event logic out of Effects — removes the stale-closure vs over-firing tradeoff
- `cacheSignal` | stable, RSC-only | 19.2 | AbortSignal-tied lifetime for `cache()`-deduped fetches
- ref-as-prop (no `forwardRef`), ref cleanup fns, `<Context value>` as provider | stable | 19.0 | `forwardRef`/`.Provider` are legacy-only in new code
- metadata hoisting (`<title>/<meta>/<link>`) | stable | 19.0 | kills react-helmet-class deps
- `preload`/`preinit`/`preloadModule`/`preinitModule`/`preconnect`/`prefetchDNS` (`react-dom`) | stable | 19.0 | component-co-located resource priming
- `useDeferredValue(value, initialValue)` | stable | 19.0 | seeded first-render deferred value
- Web Streams SSR (`renderToReadableStream`, `prerender`, `resume`, `resumeAndPrerender`) + Suspense SSR reveal batching | stable | 19.2 | unified edge/Node streaming surface
- NOT shipped (do not design against): Fragment refs, Concurrent Stores, Automatic Effect Dependencies, `<ViewTransition>` stable.

| package | version | license | VERDICT | justification |
|---|---|---|---|---|
| `babel-plugin-react-compiler` | 1.0.0 (GA 2025-10-07) | MIT | admit-substrate | per-expression auto-memoization strictly finer than hand `useMemo`/`useCallback`; per-file `'use memo'` opt-in; Vite 6 path via `@vitejs/plugin-react@6`/rolldown |
| `eslint-plugin-react-hooks` | 7.1.1 | MIT | admit-substrate | owns Rules-of-Hooks AND compiler diagnostics (`set-state-in-effect`, purity) since v6; `recommended`/`recommended-latest` presets; ESLint v10 ready |
| `eslint-plugin-react-compiler` (standalone) | superseded | MIT | reject | folded into `eslint-plugin-react-hooks@6+`; remove if present |
| `@eslint-react/eslint-plugin` | 5.9.x | MIT | mine-design-only | cherry-pick rules (`no-leaked-intersection-observer`, immutability variants) into local lint config; never swap the official plugin |

### [A6] Data-viz beyond deck.gl (genuinely-new capability only)

| npm-name | version | license | maintenance | VERDICT | justification |
|---|---|---|---|---|---|
| `@visx/scale` `@visx/shape` `@visx/axis` `@visx/group` `@visx/responsive` (family) | 4.0.0 | MIT | pushed 2026-06-22; v4 = React 19 support | admit-folder | unstyled accessible SVG/DOM chart primitives — crisp vector text, CSS/ARIA/focus/DOM events; deck.gl/three are GPU-canvas and structurally cannot serve bespoke accessible analytic charts |
| `@observablehq/plot` | 0.6.17 | ISC | pushed 2026-05-16, active | admit-folder | grammar-of-graphics (marks/facets/transforms) for exploratory-statistical charting; zero deck.gl/three overlap; pre-1.0 versioning but de facto stable (Observable Framework ships on it) |
| `d3` | 7.9.0 | ISC | pushed 2026-05-28, 20 open issues | admit-substrate | transitive scale/interpolation/geo substrate under visx + Plot; not a standalone charting surface |
| `@perspective-dev/client` + `@perspective-dev/viewer` (+`-datagrid`, `-d3fc` at 4.4.1) | 4.5.1 (2026-05-31) | Apache-2.0 | live scope after org migration | admit-folder | WASM+Arrow streaming pivot/aggregation engine + `<perspective-viewer>` grid — millions-of-rows live tabular analytics; capability class deck.gl does not own. HARD CORRECTION: `@finos/perspective` (3.8.0, 2025-09-03) is the dead scope — never reference it |
| `uplot` | 1.6.32 | MIT | pushed 2026-04-22 | admit-folder | canvas time-series at extreme point counts (~50KB, 60fps at 100k+ points) with classic axis/legend/zoom UX — telemetry/sensor/simulation dashboards; deck.gl paths are geospatial, not time-series UX |
| `apache-arrow` | 21.1.0 | Apache-2.0 | daily activity | admit-substrate | zero-copy columnar wire format; required by Perspective, feeds deck.gl/uPlot without JSON overhead |
| `echarts` | 6.1.0 | Apache-2.0 | active | reject | kitchen-sink option-object API; ground fully covered by Plot+visx+uPlot+deck.gl; inferior own-WebGL vs admitted stack |
| `plotly.js` | 3.7.0 | MIT | active | reject | ~3MB, Python/R-parity trace API, bundles own d3 fork + regl; no new capability |
| `@nivo/*` | 0.99.0 | MIT | healthy | reject (mine layouts only) | pre-styled React+d3 redundant with visx; circle-packing/sankey/chord layouts are reference material only |
| `recharts` | 3.9.2 | MIT | extremely active | reject | SVG composable charts redundant with visx/Plot; weaker large-data ceiling |
| `victory` | 37.3.6 | MIT | stalest surveyed (2025-12-19) | reject | same niche, weakest cadence, no differentiator |

### [A7] WebGPU compute-in-UI

Browser state: Chromium stable since 2023; Safari 26 line (Tahoe/iOS/iPadOS/visionOS 26) ships WebGPU + HDR canvas, 26.2 adds WebXR+WebGPU; Firefox 141+ (Windows) / 145+ (macOS ARM64) but disabled-by-default, Linux later 2026. Ruling: shippable with a mandatory WebGL2 fallback — which three.js and luma.gl already automate.

| npm-name | version | license | maintenance | VERDICT | justification |
|---|---|---|---|---|---|
| `typegpu` (+ `unplugin-typegpu` 0.11.6 build transform) | 0.11.9 | MIT | pushed 2026-07-03, very active | admit-folder | typed GPGPU: `tgpu` default export, `d` data namespace, `tgpu.computeFn`, `tgpu.fn`, `tgpu.resolve`, `root.createBuffer/createMutable/createUniform/createComputePipeline` — TS-in WGSL-out for standalone data-parallel kernels outside a scene graph; pre-1.0, pin exact + re-verify exports at implementation |
| `@webgpu/types` | 0.1.71 | BSD-3-Clause | pushed 2026-06-24 | admit-substrate | `lib.dom.d.ts` still lacks WebGPU types (open gap, Apr 2026) — required ambient augmentation for any direct WebGPU/TypeGPU/TSL authoring |
| `three/webgpu` (in `three`) | three 0.185.1 | MIT | pushed 2026-07-04 | admit-substrate | `WebGPURenderer` via `import * as THREE from 'three/webgpu'`, auto WebGL2 fallback; transitively admitted via `three` |
| `three/tsl` (in `three`) | three 0.185.1 (TSL stable at r184) | MIT | same | admit-substrate | verified: `Fn`, `instancedArray`, `instanceIndex`, `storage`, `textureStore`, `deltaTime`, `uniform`, `Loop`, `If`, `workgroupBarrier`, `subgroupAdd`; dispatch = `Fn(...)().compute(count)` + `renderer.compute(node)`; 1M+-particle compute vs ~50k WebGL ceiling — compute owner for anything living in a three scene |
| `@luma.gl/webgpu` | 9.3.6 (lockstep with deck.gl 9.3.6) | MIT | pushed 2026-07-04 | admit-substrate, flagged experimental | `webgpuAdapter` export; wire via `luma.createDevice()` or `new Deck({ deviceProps: { type: 'webgpu', adapters: [webgpuAdapter] } })`; layers ported incrementally — WebGL2 stays deck.gl's complete default; track, never design-primary yet |

## [B]-[IAC_DEPTH]

### [B1] Pulumi Automation API (`@pulumi/pulumi/automation`)

| npm-name | version | license | maintenance | VERDICT | justification |
|---|---|---|---|---|---|
| `@pulumi/pulumi` | 3.250.0 (2026-07-02) | Apache-2.0 | lockstep CLI/SDK, Node >=22 required | admit-substrate | sole owner of the Automation API — no separate package |
| `@pulumi/pulumiservice` | 1.3.0 (2026-06-25) | Apache-2.0 | active 1.x | admit-folder | Deployment Settings / Webhooks / Schedules (drift, TTL, review stacks) as IaC resources instead of hand-rolled REST |

Source-verified member spellings (read from `sdk/nodejs/automation/*.ts`, not inferred):
- `LocalWorkspace.createStack | selectStack | createOrSelectStack(args: InlineProgramArgs | LocalProgramArgs, opts?: LocalWorkspaceOptions): Promise<Stack>` — overloaded on both arg shapes.
- `InlineProgramArgs { stackName, projectName, program: PulumiFn }`; `LocalProgramArgs { stackName, workDir }`; `PulumiFn = () => Promise<Record<string, any> | void>` (inline mode runs an in-process gRPC `LanguageServer`, CLI called with `--client=127.0.0.1:<port>`).
- `Stack.up(opts?: UpOptions)`, `.preview`, `.refresh`, `.destroy`; NEW: `Stack.previewRefresh(opts?: RefreshOptions)` and `Stack.previewDestroy(opts?: DestroyOptions)` — `RefreshOptions.previewOnly`/`DestroyOptions.previewOnly` are `@deprecated` in their favor.
- Options extend `GlobalOpts` and carry `onOutput?`, `onError?`, `onEvent?: (event: EngineEvent) => void`, and `signal?: AbortSignal` (abort-cancellation is current, post-blog-era). Event delivery on CLI >3.205.0 is a local gRPC `EventsServer` on `127.0.0.1:0`; file-tail is the legacy fallback.
- Config: `setConfig(key, value: ConfigValue, path?)`, `setAllConfig(config: ConfigMap, path?)`, `setAllConfigJson`, `getConfig`, `getAllConfig`, `removeConfig`, `removeAllConfig`, `refreshConfig`.
- State: `exportStack(): Promise<Deployment>` / `importStack(state: Deployment)` where `Deployment = { version: number; deployment: any }` — an opaque state envelope, NOT the Deployments product (naming collision).
- Also: `Stack.rename`, `cancel`, `history`, `info`, `orgGetDefault/orgSetDefault`, `getTag/setTag/removeTag/listTags`.
- `EngineEvent` discriminated union: `{ sequence, timestamp, cancelEvent?, stdoutEvent?, diagnosticEvent?, preludeEvent?, summaryEvent?, resourcePreEvent?, resOutputsEvent?, resOpFailedEvent?, policyEvent?, startDebuggingEvent? }` — exactly one populated.
- CORRECTIONS (assumed names that DO NOT EXIST): `stack.getOutputs` -> real is `stack.outputs(): Promise<OutputMap>`; `stack.importResources` -> real is `stack.import(options: ImportOptions)` with `ImportOptions.resources: ImportResource[]` (+ `converter`, `generateCode`, `protect`); `WorkspaceOptions` -> real types are `LocalWorkspaceOptions` / `RemoteWorkspaceOptions`; `StackSettings` has NO `environment` field (only `secretsProvider`, `encryptedKey`, `encryptionSalt`, `config`).
- Remote execution is first-class: `RemoteWorkspace.createStack/selectStack/createOrSelectStack(args: RemoteGitProgramArgs, opts?: RemoteWorkspaceOptions): Promise<RemoteStack>`; `RemoteGitProgramArgs { stackName, url?, projectPath?, branch?, commitHash?, auth?: RemoteGitAuthArgs }`; `RemoteWorkspaceOptions { envVars?, preRunCommands?, skipInstallDependencies?, inheritSettings?, executorImage?: ExecutorImage }`. Git-source only — no inline-program remote mode. `RemoteStack` mirrors `Stack` (`up/preview/refresh/destroy/outputs/history/cancel/exportStack/importStack`) dispatched onto Deployments compute via `--remote`.

### [B2] Pulumi ESC

| name | version | license | maintenance | VERDICT | justification |
|---|---|---|---|---|---|
| `@pulumi/esc-sdk` | 0.14.0 (2026-06-15) | Apache-2.0 | lockstep with `pulumi/esc-sdk` tags | admit-substrate | canonical programmatic ESC client for runtime secret/config reads outside IaC |
| `esc` CLI (Go binary, not npm) | v0.25.0 (2026-06-01) | Apache-2.0 | 48+ releases | admit-substrate (tool, no manifest row) | `esc run <env> -- <cmd>` / `esc open` credential injection; versions independently from the bundled `pulumi env ...` subcommands (CLI 3.250.0) — two binaries, one backend |

- GA since Sep 2024; still shipping majors: BYOK/custom-KMS (Nov 2025), SSRF-protection hardening (Apr 2026 — breaking; `PULUMI_DISABLE_ESC_SSRF_PROTECTION` escape hatch for same-host providers).
- SDK members: `Configuration`, `EscApi`, `createEnvironment`, `updateEnvironment(..., envDef: EnvironmentDefinition)`, `openAndReadEnvironment`, `listEnvironments`. Arity varies by project scoping (org+env legacy vs org+project+env current) — confirm the overload against the target org before sealing signatures.
- Dynamic providers current: `fn::open::aws-login` (OIDC), `azure-login`/`azure-secrets`, `gcp-login`, `snowflake-login`, Vault, 1Password, Infisical, Doppler; plus Rotated Secrets (AWS IAM, Postgres/MySQL incl. private-VPC Lambda connectors) and ESC Connect (custom HTTPS adapters).
- Stack wiring: `environment:` key (string or array) in `Pulumi.<stack>.yaml`; ESC-internal `imports:` composes environments. Automation-API integration is imperative-only: `stack.addEnvironments(...envs)` (CLI >=3.95.0), `stack.listEnvironments()` (>=3.99.0), `stack.removeEnvironment(env)` — there is no typed `StackSettings` path.

### [B3] Deployments / orchestration

- Triggers current: push-to-deploy, click-to-deploy, REST API (`api.pulumi.com` create-deployment with per-request overrides), scheduled/drift/TTL, review stacks (per-PR ephemeral, template-toggle, destroyed on close), deployment webhooks. All Cloud-only; configure via `@pulumi/pulumiservice` or REST, never hand-rolled.
- Drift: `pulumi refresh` is never automatic on OSS/local; Pulumi Cloud adds scheduled drift detection AND remediation. Programmatic detect = `stack.previewRefresh()`; remediate = `stack.refresh()` / `up` with refresh.
- Division of labor: Automation API is an alternative execution backend (`RemoteWorkspace`) for Deployments compute, not the configuration surface for triggers/TTL/review-stack policy.

### [B4] K8s typed-program targets (PG estate + ingress stack)

| name | version | license | maintenance | VERDICT | justification |
|---|---|---|---|---|---|
| CloudNativePG operator | v1.30.0 (2026-06-29; N-2 support: 1.29.x/1.28.x) | Apache-2.0 | CNCF Sandbox, monthly cadence | admit-folder (typed target) | sole declarative PG control plane; CRDs at `postgresql.cnpg.io/v1`: `Cluster`, `Pooler`, `Database`, `Publication`, `Subscription`, `ScheduledBackup`, `Backup`, `ImageCatalog`, `ClusterImageCatalog`, new `DatabaseRole` (1.30) |
| Barman Cloud CNPG-I plugin (`plugin-barman-cloud`) | v0.13.0 (2026-06-10) | Apache-2.0 | active dedicated repo | admit-folder (typed target) | mandatory companion: in-tree `barmanObjectStore` is deprecated, removal now slated 1.31 — author exclusively `spec.plugins[].name: barman-cloud.cloudnative-pg.io` + `ObjectStore` CRD |
| cert-manager | v1.20.3 (2026-06-25; 1.21 due 2026-07-08) | Apache-2.0 | very active | admit-folder (typed target) | `Certificate`/`Issuer`/`ClusterIssuer`/`CertificateRequest` (`cert-manager.io/v1`), `Order`/`Challenge` (`acme.cert-manager.io/v1`); ACME via the `gatewayHTTPRoute` solver (`config.enableGatewayAPI=true`) — never a parallel Ingress solver |
| external-dns | v0.21.0 (2026-04-06; chart 1.21.1) | Apache-2.0 | active (k8s-sigs) | admit-folder (typed target) | native Gateway sources `gateway-httproute/-grpcroute/-tlsroute/-tcproute/-udproute` + `--gateway-namespace`/`--gateway-label-filter`; `DNSEndpoint` CRD as `--source=crd` sink; 0.21 removed ibmcloud/tencentcloud/ultradns in-tree and dropped `--txt-new-format-only` (new TXT format is the only format) |
| Gateway API | v1.6.0 (2026-06-29) | Apache-2.0 | very active (SIG-Network) | admit-folder (typed target) | ALL core resources GA/Standard as `v1`: `GatewayClass`, `Gateway`, `HTTPRoute`, `GRPCRoute`, `TLSRoute`, `TCPRoute`, `UDPRoute` (1.6 GA), `ReferenceGrant`, `BackendTLSPolicy`, `ListenerSet`; experimental channel holds only CORS filter, external-auth filter, `XBackend` (alpha), session persistence — never mix channels on one CRD |
| `@pulumi/kubernetes` | 4.32.0 (2026-06-08) | Apache-2.0 | flagship, auto-generated per k8s release | admit-substrate | sole runtime SDK for every native object, `CustomResource`, `Provider`; ships native `helm.v4.Chart` |
| `crd2pulumi` | v1.6.2 (2026-05-06; Go CLI, not npm) | Apache-2.0 | maintained, single-purpose; possible future absorption into pulumi-kubernetes as parameterized provider | admit-folder (dev-time tool, no manifest row) | generate typed TS SDKs for CNPG/cert-manager/external-dns/Gateway API CRDs, commit generated code, regenerate on operator bumps; raw `CustomResource<any>` forfeits compile-time shape checking exactly where the estate is PG-heavy |

CNPG capability notes for the design pages: declarative HA/failover/switchover, `Lease`-based primary election (1.30), operator-to-instance mTLS (1.30); `Pooler` PgBouncer with `spec.pgbouncer.imageCatalogRef`; `Publication`/`Subscription` logical replication; replica clusters / distributed topology via `externalClusters` + `.spec.replica` (cross-cluster failover explicitly out of scope — the IaC folder owns that orchestration); major-version upgrade is declarative offline `pg_upgrade` (not zero-downtime); `cluster` references on dependent CRDs are CEL-validated immutable (1.29.2+/1.30) — generators must treat them as create-only; PodMonitor generation for Prometheus/Grafana.

### [B5] Multi-tenant provisioning — design law + tools

| name | version | license | maintenance | VERDICT | justification |
|---|---|---|---|---|---|
| Capsule | v0.13.6 (2026-06-17) | Apache-2.0 | CNCF Sandbox, weekly cadence, Clastix-backed | admit-folder (typed target) | policy-based `Tenant` CRD over vanilla namespaces (RBAC/NetworkPolicy/ResourceQuota propagation) — the soft-tenancy default |
| vcluster (OSS core) | v0.35 (Platform 4.10) | Apache-2.0 core; commercial Platform tiers | weekly cadence, Loft Labs; K8s-conformance-certified (NOT a CNCF project — governance differs from Capsule/CNPG) | admit-folder (typed target) | virtual control-plane-per-tenant: real API server/controller-manager/storage as pods — the 2026 default for hard multi-tenancy before physical clusters; `vNode`/Private Nodes add data-plane isolation |
| Kiosk | archived | Apache-2.0 | dead | reject | superseded by vcluster/Platform |
| HNC | v1.1.0 final (2023), repo retired | Apache-2.0 | dead (`kubernetes-retired`) | reject | Capsule's `Tenant`/`GlobalTenantResource` owns the namespace-tree case |

Pattern-selection law (encode as policy rows, never bespoke code paths):
- Namespace-per-tenant + Capsule: trusted multi-team tenancy, low-to-medium count, shared control plane acceptable; one `Tenant` CR per logical tenant, Pulumi-applied.
- vcluster: escalation the moment a tenant is untrusted/external, needs own CRDs/API versions/admission webhooks/cluster-scoped objects, or "my own cluster" self-service; dedicated physical clusters only for hardware/OS-boundary regulatory tenants.
- PG estate three-tier escalation, tenant-tier -> CRD shape: (1) shared-schema + RLS in one `Cluster` (highest density); (2) database-per-tenant logical — one `Cluster`, N `Database` CRDs (declarative `owner`/`extensions`/`schemas`; this is what the CRD exists for — never hand-rolled `CREATE DATABASE`); (3) dedicated `Cluster` per tenant for independent HA/backup/compliance (CNPG has no cross-namespace operator sharing — physical isolation = distinct `Cluster`, strictest = distinct operator instance).

### [B6] Crossplane verdict

| name | version | license | maintenance | VERDICT | justification |
|---|---|---|---|---|---|
| Crossplane | v2.3.3 (2026-06-22; v1 LTS 1.20.10) | Apache-2.0 | CNCF Graduated (2025-11-06), quarterly cadence | reject (runtime) / mine-design-only (pattern) | a second etcd-backed control plane with its own state, composition DSL, and reconcile loop — two desired-state owners over the same infrastructure is the split-brain the canonical-owner law forbids |
| Pulumi Kubernetes Operator | v2.7.0 (GA since 2.0) | Apache-2.0 | first-party, monthly | admit-substrate | in-cluster continuous reconciliation (`spec.refresh`, `spec.continueResyncOnCommitMatch`, `spec.resyncFrequencySeconds`) — the Pulumi-native reconcile loop, desired state as a real program |

Crossplane v2 facts (for the mined pattern only): namespaced XRs/MRs are the schema default (`scope: Namespaced`, `.m.` API-group suffix), Claims are legacy-compat headed for removal, `mode: Resources` deprecated for `mode: Pipeline` function pipelines, new primitives = Managed Resource Activation Policies (beta-grade, no GA date) and Operations (day-2 function-pipeline workflows). Pulumi-native equivalents that make it redundant: PKO resync loop = reconciliation; Cloud drift detect/remediate + `previewRefresh` = drift guarantee; a typed TS program strictly subsumes composition-function pipelines; ESC = `EnvironmentConfig` + dynamic OIDC creds. The one capability without a direct analog — tenant-submitted namespaced CR triggering provisioning — routes through PKO `Program`/`Stack` CRs, same ergonomics, one state owner.

### [B7] Provider roster beyond the admitted set

Admitted (not re-justified): `@pulumi/aws awsx gcp cloudflare kubernetes docker docker-build postgresql random tls command policy` + `@pulumi/doppler @pulumi/grafana`.

| npm-name | version | license | maintenance | VERDICT | justification |
|---|---|---|---|---|---|
| `@pulumi/eks` | 4.2.0 (4.3.0 alpha-only) | Apache-2.0 | active, 107k wk dl | admit-substrate | the one real AWS gap: EKS control plane, IRSA/OIDC IAM wiring, managed node groups/Fargate — neither `awsx` (VPC/ECS/ALB) nor `kubernetes` (in-cluster only) reaches it |
| `@pulumi/cloudinit` | 1.6.0 | Apache-2.0 | active | admit-substrate | non-K8s charter leg: multi-part MIME cloud-init user-data for raw VM bootstrap |
| `@pulumiverse/acme` | 0.16.1 | Apache-2.0 | active community tier, 18.5k wk dl | admit-substrate | CA-trusted certs (Let's Encrypt/ZeroSSL) for non-cluster TLS — `@pulumi/tls` is self-signed-only and cert-manager is in-cluster-only; `@pulumi/acme` does not exist |
| `@pulumi/github` | 6.14.0 | Apache-2.0 | active, 143.8k wk dl | admit-folder | bootstrap axis source-control leg: repos, branch protection, environments, Actions secrets |
| `@pulumi/synced-folder` | 0.12.4 | Apache-2.0 | pushed 2025-08-13 | admit-folder | bucket-sync + CDN invalidation for static frontend distribution (S3/GCS/Blob), if that folder leg exists |
| `@pulumi/vault` | 7.10.0 | Apache-2.0 | active | mine-design-only | secret ENGINES (dynamic DB creds, PKI, transit) — distinct concern from Doppler/ESC static distribution; earn on a concrete dynamic-secrets/PKI feature |
| `@pulumi/keycloak` XOR `@pulumi/auth0` | 6.12.0 / 3.47.0 | Apache-2.0 | both active | mine-design-only | IdP provisioning; admit exactly one after the identity-architecture decision, never both |
| `@pulumi/minio` | 0.17.0 | Apache-2.0 | active | mine-design-only | self-hosted S3-compatible storage for an air-gapped tenant tier only |
| `@pulumi/tailscale` | 0.28.0 | Apache-2.0 | active | mine-design-only | mesh-VPN/zero-trust admin access; overlaps Cloudflare Access/Tunnel — pick one vendor when the tier is designed |
| `@pulumi/azure-native` | 3.19.0 | Apache-2.0 | very active | reject | no Azure tenant requirement; third hyperscaler is speculative bloat |
| `@pulumi/gke` | does not exist | — | — | reject | GKE ships natively inside admitted `@pulumi/gcp` |
| `@pulumi/digitalocean` / `@pulumi/hcloud` | 4.73.0 / 1.39.0 | Apache-2.0 | active | reject | no tenant requirement; redundant against aws/gcp |
| `@pulumi/kubernetes-cert-manager` | 0.2.0 (stale 2025-03-14) | Apache-2.0 | stale | reject | thin helm-install wrapper; `helm.v4.Chart` (confirmed in `@pulumi/kubernetes` 4.32.0) + crd2pulumi typed CRDs are denser |
| `@pulumi/kubernetes-helm` | retired (npm 404) | — | — | reject | absorbed into `@pulumi/kubernetes` helm.v3/v4 |
| `@pulumi/std` | 2.3.2 | Apache-2.0 | active | reject | remote-invoke ports of Terraform HCL functions — TS stdlib owns this natively |
| `@pulumi/time` | does not exist (`@pulumiverse/time` 0.1.1 only) | Apache-2.0 | community | reject | sleep/rotation expressible via `@pulumi/command` or a trivial dynamic provider |
| `@pulumi/local` / `@pulumi/external` / `@pulumi/null` | 0.1.6 / 0.2.0 / 0.2.0 | Apache-2.0 | stale/thin | reject | native `fs` / richer `@pulumi/command` / native dependency graph supersede all three |
| `@pulumi/mysql` | 3.3.0 | Apache-2.0 | active | reject | no MySQL requirement; `@pulumi/postgresql` owns relational provisioning |
| `@pulumiverse/harbor` | 3.10.21 (`@pulumi/harbor` does not exist) | Apache-2.0 | weak (4 stars) | reject | ECR/Artifact Registry reachable via admitted aws/gcp |
| `@pulumi/datadog` / `@pulumi/newrelic` | 5.7.0 / 5.73.0 | Apache-2.0 | active | reject | competing observability vendors against admitted `@pulumi/grafana` — one owner |
| `@pulumi/prometheus` | does not exist | — | — | reject | no management API to bridge; deploy as workload via `helm.v4.Chart`, query via Grafana |
| `@pulumi/ns1` | 3.10.0 | Apache-2.0 | near-zero adoption | reject | competing DNS vendor against admitted `@pulumi/cloudflare` |

## [C]-[CORRECTIONS_AND_FLAGS]

Hard corrections (assumed names that are wrong — never let these into design pages):
- `stack.getOutputs` -> `stack.outputs()`; `stack.importResources` -> `stack.import(options)`; `WorkspaceOptions` -> `LocalWorkspaceOptions`/`RemoteWorkspaceOptions`; `StackSettings.environment` typed field does not exist (imperative `addEnvironments`/`listEnvironments`/`removeEnvironment` only).
- `@finos/perspective` -> `@perspective-dev/client` + `@perspective-dev/viewer` (+`-datagrid`/`-d3fc`) at 4.5.x — the FINOS scope is dead at 3.8.0.
- Safari scroll-driven-animation support is Safari 26, not 18.
- React `<ViewTransition>`/`addTransitionType` are canary-only; nothing in 19.2.7 stable.
- In-tree CNPG barman backup is a dead end (removal slated 1.31) — plugin-barman-cloud only.
- `eslint-plugin-react-compiler` standalone is superseded by `eslint-plugin-react-hooks@6+`.

Re-verify at implementation time (pattern-inferred or version-volatile):
- `motion/react` exports `useScroll`/`useTransform`/`useMotionValue`/`LayoutGroup`/`MotionConfig` (sibling-pattern-inferred, not literal-fetched).
- `typegpu` 0.x and `@luma.gl/webgpu` are pre-1.0/experimental — pin exact, re-verify exports.
- `@pulumi/esc-sdk` environment-call arity (org+env vs org+project+env) varies with project scoping.
- cert-manager 1.21 due 2026-07-08; Gateway API `v1alpha2` `TCPRoute`/`UDPRoute` deprecated post-1.6 GA.
- Crossplane MRAP has no GA date — any MRAP-adjacent mined pattern is unstable.
