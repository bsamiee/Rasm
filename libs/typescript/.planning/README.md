# [TYPESCRIPT_BRANCH]

The file router for the TypeScript branch planning corpus and the branch-level cross-cutting package registry — the packages shared across two or more folders, trimmed from the per-folder registries. The branch is the host-free web/edge platform of the workspace: five packages meeting only at the wire contracts and the offline/companion seams, consuming the C# wire as settled vocabulary and authoring no wire shape. The package topology and dependency direction are in `ARCHITECTURE.md`, the cross-package concert in `IDEAS.md`, the cross-package open work in `TASKLOG.md`.

## [1]-[FOLDER_ROUTER]

Each package carries its own four index docs at its root and its design pages under `<pkg>/.planning/<sub-domain>/<page>.md`; this router points at the package roots in dependency order.

- [interchange](../interchange/README.md) — the wire boundary and inbound dependency root: transport, codecs, refinement, artifacts, faults, quarantine, gateway, contracts.
- [projection](../projection/README.md) — the read-side fold-algebra owner: fold-core, feed-stores, standing-query, convergence, envelope, evidence, availability, clock-uncertainty.
- [ui](../ui/README.md) — the browser UI/UX/components library: binding, component-system, theming, observation, cartography, viewport, motion, overlay.
- [platform](../platform/README.md) — the browser AppHost-analog and SPA entry: runtime-composition, identity-session, runtime-config, observability, build-pipeline, local-persistence, routing, offline-cache, fault-capture, feature-flags, web-vitals.
- [services](../services/README.md) — the node tier and deploy-time IaC: durable-execution, persistence, hybrid-search, messaging, runtime-backplane, provisioning.

## [2]-[CROSS_CUTTING_PACKAGES]

The packages two or more folders draw on, registered once here and trimmed from the per-folder registries; folder-local packages stay in the folder README. Versions are centralized in the one workspace catalog; this registry carries no pin and no `.api/` link. The catalogue for each lives at the `.api/<package>.md` of every folder that uses it, never consolidated to a branch-level home.

- `effect` — the substrate of every folder: `Effect.Service`, `Layer`, `ManagedRuntime`, `Schema`, `Stream`, `SubscriptionRef`, `Match`, `Data.TaggedEnum`, `Schedule`, `Duration`, `HashMap`, `HashSet`.
- `@effect-atom/atom` — the reactive-store bridge target: `projection` exposes each `SubscriptionRef` store as a `Subscribable`, `ui` binds it through `Atom.subscriptionRef`/`Atom.searchParam`/`kvs`/`family`/`pull` at the render boundary.
- `@effect-atom/atom-react` — the React hook surface over the atom bridge, consumed by the `ui` binding spine and the `platform` SPA shell.
- `@effect/opentelemetry` — the OTLP telemetry edge: `platform` owns the WebSdk collector path, `services` the node SDK path, and `ui` reads it strictly as a collector reader, never an emitter.
- `@effect/platform` — the HTTP/KeyValueStore/Worker service tags: `@effect/platform-browser` carries the browser bindings for `platform`, `@effect/platform-node` the node bindings for `services`.
- `@effect/vitest` — the Effect-aware test runner binding the property spine to the fold and durable owners across `projection` and `services`.
- `fast-check` — the algebraic property-testing arbitrary spine driving the convergence, window-fold, and durable law harnesses.
- `@stryker-mutator/core` — the mutation kill-ratio gate over the law spines, one `stryker.config.mjs` per package.
- `isomorphic-dompurify` — the DOM-bound text sanitizer at the `interchange` quarantine terminal and the `ui` content surfaces.

## [3]-[ROOT_DEV_TOOLING]

A branch concern the folders flagged: the mutation gate and the descriptor codegen rail resolve their packages from the workspace catalog but require an honest landing in the root dev-tooling manifest before any folder transcribes. The `@stryker-mutator/core` plus `typescript-checker` and `vitest-runner` trio and the `@bufbuild/buf` descriptor CLI are present in the catalog yet absent from the root manifest; the `TASKLOG.md` carries the wiring task that closes this gap. The strictness floor, the folder-stratum module-boundary fence, the `projection/**` `@connectrpc/*` ban, the `ui/**` `../platform` ban, and the `./provisioning` subpath isolation are centralized at the monorepo root, never per-package config.
