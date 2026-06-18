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

The packages two or more folders draw on, registered once here and trimmed from the per-folder registries; folder-local packages stay in the folder README. Versions are centralized in the one workspace catalog; this registry carries no pin. Each cross-cutting package carries one catalogue at the branch `libs/typescript/.api/<slug>.md`, authored once and never duplicated into a folder `.api/`; a consuming folder names it in the folder README `[CROSS_CUTTING]` section.

- `effect` — the substrate of every folder: `Effect.Service`, `Layer`, `ManagedRuntime`, `Schema`, `Stream`, `SubscriptionRef`, `Match`, `Data.TaggedEnum`, `Schedule`, `Duration`, `HashMap`, `HashSet`.
- `@effect-atom/atom` — the reactive-store bridge target: `projection` exposes each `SubscriptionRef` store as a `Subscribable`, `ui` binds it through `Atom.subscriptionRef`/`Atom.searchParam`/`kvs`/`family`/`pull` at the render boundary.
- `@effect-atom/atom-react` — the React hook surface over the atom bridge, consumed by the `ui` binding spine and the `platform` SPA shell.
- `@effect/opentelemetry` — the OTLP telemetry edge: `platform` owns the WebSdk collector path, `services` the node SDK path, and `ui` reads it strictly as a collector reader, never an emitter.
- `@effect/platform` — the HTTP/KeyValueStore/Worker service tags: `@effect/platform-browser` carries the browser bindings for `platform`, `@effect/platform-node` the node bindings for `services`.
- `@effect/vitest` — the Effect-aware test runner binding the property spine to the fold and durable owners across `projection` and `services`.
- `fast-check` — the algebraic property-testing arbitrary spine driving the convergence, window-fold, and durable law harnesses.
- `@stryker-mutator/core` — the mutation kill-ratio gate over the law spines, one `stryker.config.mjs` per package.
- `isomorphic-dompurify` — the DOM-bound text sanitizer at the `interchange` quarantine terminal and the `ui` content surfaces.

## [3]-[API_CATALOGUE_FORM]

The branch-uniform shape of every `<pkg>/.api/<package>.md` catalogue, applying the [1]-[DOC_SET] one-catalogue-per-external-library mandate so a cold grade verifies a fence against exactly one file named for the package it spells. The catalogue is the verified-spelling resource a planning pass reads to transcribe an external member; an unverified member stays a RESEARCH item, never settled fence code.

- One file per external library, named for the package: `effect.md`, `effect-platform.md`, `@connectrpc/connect` at `connectrpc-connect.md`, `@bufbuild/protobuf` at `bufbuild-protobuf.md`. A scoped package collapses its slash to a dash; a closely-versioned provider family that one charter consumes as a set (the `effect-ai-*` provider layers) stays one file per published package.
- A concept-named file carrying several distinct published packages is the rejected form. The `transport-wire.md` set (`@connectrpc/connect`, `@connectrpc/connect-web`, `@bufbuild/protobuf`, `@bufbuild/buf`, `msgpackr`), the `infra-data.md` set (`@pulumi/*`, `ioredis`, `@aws-sdk/client-s3`, `maplibre-gl`, `deck.gl`), and the `ui-stack.md` set (`react`, `react-dom`, `react-aria-components`, `@radix-ui/react-*`, `@tanstack/react-table`, `@tanstack/react-virtual`, `vite`, `@vitejs/plugin-react`, `vite-plugin-pwa`) each split into one catalogue per package; the splitting task is in `TASKLOG.md`.
- A package two or more folders use is a cross-cutting package: it carries one catalogue at the branch `libs/typescript/.api/<slug>.md` (see [2]), never duplicated into a folder `.api/`, and each consuming folder names it in its README `[CROSS_CUTTING]` section. A folder-local package keeps its catalogue in the folder `.api/`. The `@effect-atom/atom` core and its `@effect-atom/atom-react` React binding are both branch cross-cutting, so both live at the branch `.api/`; a folder consuming the React binding names both in `[CROSS_CUTTING]`.
- A catalogue cross-reference names its real target: a sibling catalogue by its package filename (`effect.md`, `effect-atom.md`, `effect-platform.md`), and a design-page owner-symbol by `sub-domain/page#CLUSTER`. A stale `api-<name>.md` cross-reference scheme, an absent target page, or a defunct sub-domain or package name is the rejected form a cold grade fails — the catalogue carries no source-trace, tool-invocation, reflection-process, or bundle-fence-tool narration, only the verified surface and its real-named anchors.

## [4]-[ROOT_DEV_TOOLING]

A branch concern the folders flagged: the mutation gate and the descriptor codegen rail resolve their packages from the workspace catalog but require an honest landing in the root dev-tooling manifest before any folder transcribes. The `@stryker-mutator/core` plus `typescript-checker` and `vitest-runner` trio and the `@bufbuild/buf` descriptor CLI are present in the catalog yet absent from the root manifest; the `TASKLOG.md` carries the wiring task that closes this gap. The strictness floor, the folder-stratum module-boundary fence, the `projection/**` `@connectrpc/*` ban, the `ui/**` `../platform` ban, and the `./provisioning` subpath isolation are centralized at the monorepo root, never per-package config.
