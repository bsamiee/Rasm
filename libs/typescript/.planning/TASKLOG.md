# [TYPESCRIPT_BRANCH_TASKLOG]

The cross-package open and closed work for the TypeScript branch — the wiring, guards, and seams no single folder owns, distilled from the branch concert in `IDEAS.md`. Per-folder work lives in each folder's `TASKLOG.md`. Each task is a card whose leader carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the capability or wiring to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations.

## [1]-[OPEN]

[QUEUED] Wire the mutation gate and the descriptor codegen rail into the root dev-tooling manifest.
- Land `@stryker-mutator/core`, `@stryker-mutator/typescript-checker`, `@stryker-mutator/vitest-runner`, and `@bufbuild/buf` in the root dev-tooling manifest — they resolve from the workspace catalog but are absent from the root manifest the folders flagged — so the per-package mutation gate and the `interchange` descriptor `buf generate` rail resolve.
- Integrate the `@stryker-mutator/*` trio and `@bufbuild/buf`; the versions stay centralized in the workspace catalog, never pinned in any package.
- Wires the root dev-tooling only; each package carries one `stryker.config.mjs`, and `interchange` consumes `buf` at its codegen edge — no folder restates the version or the command.
- The honesty gap is catalog-present-but-root-absent; until it closes no folder transcribes its mutation spine or emits its `gen/` descriptors, so this task precedes every transcription card.

[QUEUED] Author the per-package manifests and the subpath publication surface.
- Author the minimal per-package manifest for each of the five folders so the `libs/typescript/*` workspace glob resolves each package and its subpath: `interchange` and `projection` under `.`, `ui` under `./ui`, `platform` under `./web`, `services` under `./node` plus `./provisioning`.
- Integrate no runtime package; the subpath `exports` and the centralized tsconfig solution references are build wiring.
- Wires the publication boundary that keeps the browser bundle free of the node/IaC closure and the node bundle free of the browser surface; the `./provisioning` subpath isolates the deploy-time `@pulumi/*` closure off the durable hot path.
- The manifests land at transcription time, not before; the subpath set is the publication contract the dependency direction in `ARCHITECTURE.md` rests on.

[QUEUED] Materialize the mechanical stratum and import guards through the centralized config.
- Confirm the centralized monorepo config materializes the folder-stratum module-boundary fence, the `projection/**` `@connectrpc/*` ban, the `ui/**` `../platform` ban, and the `./provisioning` subpath isolation at implementation start, so each cross-folder direction is a typecheck or lint failure rather than a prose review check.
- Integrate the centralized eslint module-boundary fence and the `tsgo` strictness floor (`isolatedDeclarations`, `erasableSyntaxOnly`, `exactOptionalPropertyTypes`, `verbatimModuleSyntax`, `noUncheckedIndexedAccess`, `customConditions`); no per-package config.
- Wires the dependency direction stated once in `ARCHITECTURE.md` into a mechanical guard at the four named surfaces.
- A transport import in the fold tier, a `platform` import in `ui`, or a `@pulumi/*` type on the hot path each fails the build at exactly one surface; the guard is config, never a runtime check.

[QUEUED] Wire the one-fold-one-binding read seam.
- Confirm each `projection`-derived `Subscribable` (the composite reactive views from the derived-query combinator) binds through the one `ui` `AtomBinding` with no recombination logic crossing into `ui`, so the read path is one fold algebra and one reactive spine.
- Integrate `effect` `SubscriptionRef.changes`/`Subscribable` on the `projection` side and `@effect-atom/atom`/`@effect-atom/atom-react` on the `ui` side.
- Wires `projection` fold-core and feed-stores to the `ui` binding spine; a view-state layer in `ui` or a raw-store recombination in a component is the named branch defect both sides forbid. From ONE_FOLD_ONE_BINDING.
- The derivations are incremental because they fold over base change streams, so the seam never grows per composite view; depends on the `projection` derived-query combinator landing first.

[QUEUED] Wire the content-addressed off-thread artifact seam.
- Confirm the `interchange` transferable-stream reassembly pipes to the `platform` `DecodeWorkerPool`, runs the Crc32-verify, stitch, and content-key digest off the main thread, and exposes one `ContentKey`-addressed blob the `ui` viewport renders and the `projection` evidence fold correlates on.
- Integrate `effect` `Stream`, the transferable `ReadableStream`/BYOB worker primitives in `@effect/platform`/`@effect/platform-browser`, and the resolved 128-bit wasm hash run inside the worker.
- Wires `interchange` artifacts to the `platform` worker pool and to the `ui` viewport and `projection` evidence correlation; a second hash mint anywhere on this path is the named cross-language drift defect. From CONTENT_ADDRESSED_OFF_THREAD.
- Depends on the `interchange` content-key parity gate (the 128-bit provider question and the upstream C# `XxHash128` fixtures); until parity is proven the off-thread digest is local-only, not trusted cross-runtime.

[QUEUED] Wire the native route-transition and per-route vital seam.
- Confirm the `platform` Navigation-API router and view-transition fold drive the `ui` `<ViewTransition>`/`<Activity>` wrappers around the guard-admitted location commit, and the `platform` web-vitals reset/flush reads the same one location cell for per-soft-navigation INP/CLS attribution.
- Integrate the native Navigation API, `document.startViewTransition`, the React `<ViewTransition>`/`<Activity>`/`useEffectEvent` surface, and `effect` `Stream.asyncScoped`/`Effect.acquireRelease` for the scoped ingress and transition resources.
- Wires `platform` routing and web-vitals to the `ui` motion surfaces over one location cell; the `<Activity>` preserves the `ui` viewport GL context and tab state across the swap, and a teardown/rebuild on route switch is the named defect. From NATIVE_TRANSITION_PAIR.
- The location cell is the single coupling seam; a parallel route tracker in web-vitals or a hand-authored FLIP choreography in `ui` is the named defect; the reduced-motion gate is one row.

[QUEUED] Wire the causal-ordering read off the C# HLC band — wire touchpoint for `libs/.planning` CAUSAL_TENANT_IDENTITY_WIRE.
- Confirm the `projection` convergence fold reads the HLC two-half causal stamp off the C# wire as an ordering input, promoting `SkewBand` `{ midpointMs, radiusMs }` into the concurrent-uncertain decision rather than forcing a spurious total order, and the `ui` evidence timeline renders the band — never minting a parallel clock.
- Integrate `effect` (the `projection` fold and `Subscribable` ordering surface); the HLC band is decoded from the upstream wire, no new transport package.
- Wires `projection` convergence/skew-ordering to the `ui` observation route over the C#-minted HLC stamp; this is the TS anchor the cross-`libs/` CAUSAL_TENANT_IDENTITY_WIRE seam consumes, never restated cross-language. From HONEST_CLOCK_UNCERTAINTY.
- The causal stamp is a C#-owned wire fact reproduced under the same parity gate as the content seed; a second clock or causal scheme on the TS side is the named cross-language drift defect; depends on the content/causal parity fixture landing.

## [2]-[CLOSED]

None.
