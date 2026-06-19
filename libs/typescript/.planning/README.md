# [TYPESCRIPT_BRANCH]

This file routes the TypeScript branch planning corpus and registers the branch-level cross-cutting packages — those shared across two or more folders and trimmed from per-folder registries. The branch is the host-free web/edge platform of the workspace: five packages meeting only at wire contracts and offline/companion seams, consuming the C# wire as settled vocabulary and authoring no wire shape. Package topology and dependency direction are in `ARCHITECTURE.md`, the cross-package concert in `IDEAS.md`, and cross-package open work in `TASKLOG.md`.

## [01]-[FOLDER_ROUTER]

- [01]-[INTERCHANGE](../interchange/README.md): wire boundary and inbound dependency root — Transport, Codec, Contract, Ingress.
- [02]-[PROJECTION](../projection/README.md): read-side fold-algebra owner — fold, query, convergence, causality, evidence.
- [03]-[UI](../ui/README.md): browser UI/UX/components library — binding, interaction, theming, overlay, render.
- [04]-[PLATFORM](../platform/README.md): browser AppHost-analog and SPA entry — Runtime, Config, Transport, Session, Observability, Shell.
- [05]-[SERVICES](../services/README.md): node tier and deploy-time IaC — persistence, search, execution, messaging, security, agent, provisioning.

## [02]-[SUBSTRATE_PACKAGES]

The cross-domain TypeScript foundation every folder builds on: the Effect runtime, the reactive bridge, observability, the React view core, the sanitizer, content identity, the wire-codegen toolchain, and the test stack. Versions are centralized in the one workspace catalog; this registry carries no pin. Each substrate package carries one catalogue at the branch `libs/typescript/.api/<slug>.md`, authored once and never duplicated into a folder `.api/`; a consuming folder names it in the folder README `## [3]-[SUBSTRATE_PACKAGES]` section.

[RUNTIME_CORE]:
- `effect` — the substrate of every folder: `Effect.Service`, `Layer`, `ManagedRuntime`, `Schema`, `Stream`, `SubscriptionRef`, `Match`, `Data.TaggedEnum`, `Schedule`, `Duration`, `HashMap`, `HashSet`.
- `@effect/platform` — the HTTP/KeyValueStore/Worker service tags: `@effect/platform-browser` carries the browser bindings for `platform`, `@effect/platform-node` the node bindings for `services`.

[REACTIVE_BRIDGE]:
- `@effect-atom/atom` — the reactive-store bridge target: `projection` exposes each `SubscriptionRef` store as a `Subscribable`, `ui` binds it through `Atom.subscriptionRef`/`Atom.searchParam`/`kvs`/`family`/`pull` at the render boundary.
- `@effect-atom/atom-react` — the React hook surface over the atom bridge, consumed by the `ui` binding spine and the `platform` SPA shell.

[OBSERVABILITY]:
- `@effect/opentelemetry` — the OTLP telemetry edge: `platform` owns the WebSdk collector path, `services` the node SDK path, and `ui` reads it strictly as a collector reader, never an emitter.

[VIEW_CORE]:
- `react` — the React 19 core render substrate: `ui` binds the component spine and hook set, `platform` binds the SPA shell render bridge for the error-boundary integration.
- `react-dom` — the browser DOM renderer: `platform` owns the `createRoot` SPA mount and resource-hint surface, `ui` consumes portals and the form-status hooks.

[SECURITY_SUBSTRATE]:
- `isomorphic-dompurify` — the DOM-bound text sanitizer at the `interchange` quarantine terminal and the `ui` content surfaces.

[IDENTITY]:
- `hash-wasm` — the cross-libs content-identity substrate: `interchange` owns the canonical content-address derivation surface; other folders consume the derived address, never the hash primitive directly.

[WIRE_CODEGEN]:
- `@bufbuild/protobuf` — the runtime protobuf message library: TypeScript folders deserialize the emitted descriptor and typed message shapes the C# wire authors.
- `@bufbuild/buf` — the cross-libs `FileDescriptorSet` ownership contract: the descriptor CLI compiles the proto set the C# wire authors, and the TypeScript folders consume the emitted descriptor as settled wire vocabulary rather than authoring a shape.

[TEST_SUBSTRATE]:
- `@effect/vitest` — the Effect-aware test runner binding the property spine to the fold and durable owners across `projection` and `services`.
- `fast-check` — the algebraic property-testing arbitrary spine driving the convergence, window-fold, and durable law harnesses.
- `@stryker-mutator/core` — the mutation kill-ratio gate over the law spines, one `stryker.config.mjs` per package; its `@stryker-mutator/typescript-checker` and `@stryker-mutator/vitest-runner` plugins are co-admitted at this one branch registration and never re-listed in a folder roster.

## [03]-[API_CATALOGUE_FORM]

The branch-uniform shape of every `<pkg>/.api/<package>.md` catalogue, applying the [1]-[DOC_SET] one-catalogue-per-external-library mandate so a cold grade verifies a fence against exactly one file named for the package it spells. The catalogue is the verified-spelling resource a planning pass reads to transcribe an external member; an unverified member stays a RESEARCH item, never settled fence code.

- One file per external library, named for the package: `effect.md`, `effect-platform.md`, `@connectrpc/connect` at `connectrpc-connect.md`, `@bufbuild/protobuf` at `bufbuild-protobuf.md`. A scoped package collapses its slash to a dash; a closely-versioned provider family that one charter consumes as a set (the `effect-ai-*` provider layers) stays one file per published package.
- A concept-named file carrying several distinct published packages is the rejected form. The transport-wire packages (`@connectrpc/connect`, `@connectrpc/connect-web`, `@bufbuild/protobuf`, `@bufbuild/buf`), the infra-data packages (`@pulumi/*`, `ioredis`, `@aws-sdk/client-s3`, `maplibre-gl`, `deck.gl`), and the UI-stack packages (`react`, `react-dom`, `react-aria-components`, `@radix-ui/react-*`, `@tanstack/react-table`, `@tanstack/react-virtual`, `vite`, `@vitejs/plugin-react`, `vite-plugin-pwa`) each carry one catalogue per package, named for the package and never collapsed into a shared concept file.
- A package two or more folders use is a cross-cutting package: it carries one catalogue at the branch `libs/typescript/.api/<slug>.md` (see [2]), never duplicated into a folder `.api/`, and each consuming folder names it in its README `[SUBSTRATE_PACKAGES]` section. A folder-local package keeps its catalogue in the folder `.api/`. The `@effect-atom/atom` core and its `@effect-atom/atom-react` React binding are both branch substrate, so both live at the branch `.api/`; a folder consuming the React binding names both in `[SUBSTRATE_PACKAGES]`.
- A catalogue cross-reference names its real target: a sibling catalogue by its package filename (`effect.md`, `effect-atom.md`, `effect-platform.md`), and a design-page owner-symbol by `sub-domain/page#CLUSTER`. A stale `api-<name>.md` cross-reference scheme, an absent target page, or a defunct sub-domain or package name is the rejected form a cold grade fails — the catalogue carries no source-trace, tool-invocation, reflection-process, or bundle-fence-tool narration, only the verified surface and its real-named anchors.

## [04]-[ROOT_DEV_TOOLING]

A branch concern the folders flagged: the mutation gate and the descriptor codegen rail resolve their packages from the workspace catalog but require an honest landing in the root dev-tooling manifest before any folder transcribes. The `@stryker-mutator/core` plus `typescript-checker` and `vitest-runner` trio and the `@bufbuild/buf` descriptor CLI are present in the catalog yet absent from the root manifest; the `TASKLOG.md` carries the wiring task that closes this gap. The strictness floor, the folder-stratum module-boundary fence, the `projection/**` `@connectrpc/*` ban, the `ui/**` `../platform` ban, and the `./provisioning` subpath isolation are centralized at the monorepo root, never per-package config.
