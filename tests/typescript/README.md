# [TYPESCRIPT_TESTING]

Authoring law for every TypeScript spec, kit member, and e2e suite. The cross-language proof law — oracle grades, the witness mandate, the banned-shape core — is [tests/README.md](../README.md). Anything in this tree — the kit package, spec placement, runner configuration, mutation policy — is rebuilt ground-up the moment a denser shape exists; no compatibility wrappers, no aliased old surfaces, no band-aids, and breaking existing specs is never a reason to preserve chaff.

## [01]-[TOPOLOGY]

Unit specs colocate beside their source in `libs/typescript` per the vitest idiom — spec placement is language-idiomatic, kit placement is not. `tests/typescript/` owns only the shared kit package (`_testkit`), the architecture suite home (`_architecture`), the playwright e2e home (`e2e`, stood up at TS buildout), and the dev-tool API catalog tier (`.api`). A unit spec placed under `tests/typescript/` instead of beside its source is misfiled; a shared harness placed beside source instead of in the kit is a duplication seed.

## [02]-[KIT_CONTRACT]

`@rasm/ts-testkit` at `tests/typescript/_testkit` is the one shared kit: a private, source-exporting pnpm workspace package that colocated specs import through the workspace graph, never through relative path reach-around. Its charter: corpus readers for `tests/contracts/` assets, witness-mandatory law combinators, Schema-derived arbitraries for kernel brands and decoded wire shapes, harness layers, and e2e drivers. Harness layers wrap the verification lanes as shared `Layer`s: the testcontainers pg-with-extensions row and its S3-compatible object-store sibling prove extension-dependent and presign law, while the pglite fast lane serves everything its no-server-extensions boundary admits. The kit body lands at TS buildout; until then the package is the resolution anchor and the extension point — a new shared test capability is a kit export, never a helper file beside a spec. The kit carries no runtime dependencies of its own; everything it composes resolves through the workspace catalog.

## [03]-[ARCHITECTURE_SUITE]

`tests/typescript/_architecture` is the gauge home — the branch-boundary suites the `@rasm/ts` exports map cannot express, the analog of `tests/csharp/_architecture`. Its charter: the edge-ledger import audit over the permitted-edge table owned by `libs/typescript/.planning/composition-system.md`, per-runtime subpath purity (node code unreachable from browser resolution), the external-admission and per-sub-folder package-admission audits, and the branch-wide migrator-import ban. The home stands now; suites land at TS buildout. A gauge verdict is a structural fact about the import graph — a law the compiler or exports map already enforces physically is never re-proven here.

## [04]-[API_CATALOGS]

`tests/typescript/.api/` is the dev-tool catalog tier: one API-evidence catalog per dev-plane package the kit and suites compose, under the same catalog law as the `libs/` tiers — a package is catalogued at exactly one tier, versions live only in `pnpm-workspace.yaml`, and a kit member or suite transcribes an external member only at a spelling its catalog verifies. Dev tooling is catalogued here, never in a `libs/typescript` tier; runtime packages are never catalogued here.

## [05]-[EFFECT_IDIOMS]

Specs are written against the installed vitest major — the `@effect/vitest` peer range is resolved explicitly as a peer rule in `pnpm-workspace.yaml`, never by silent tolerance:
- `it.effect` runs a law inside the Effect test environment with `TestClock` control; `it.live` opts into real time and real services when the law is about wall-clock behavior.
- `it.layer` shares an expensive `Layer` across a suite; layer construction inside each test body is a density failure.
- `it.effect.prop` is the one property surface: fast-check arbitraries feed Effect laws without losing `TestClock`. `@fast-check/vitest` is rejected for exactly that loss.
- `it.scoped` and direct `TestServices` reach-ins are banned; scoping rides the effect under test, service substitution rides layers.

Time-dependent laws advance `TestClock` deterministically instead of sleeping; a spec that awaits real time in the unit lane is an integration test mislabeled or a flake being incubated.

## [06]-[ORACLES_GENERATORS]

Schema, failure-tag, and union proofs ride these rails; typed error tags and exhaustive union arms are the TS spelling of the Grade C failure-category rail:
- Schema-first boundaries prove as decode/encode round-trip laws over generated arbitraries; a boundary without a round-trip law is unproven.
- Failure lanes prove by tag: assert the typed error case identity, never message substrings.
- Generators derive from the owning schema where one exists; hand-rolled arbitraries are for domains the schema cannot express, and they generate distinct payload values wherever a subject transports multiple inputs — equal placeholders hide swapped arguments.
- Union-shaped subjects prove exhaustively: one property or matrix per union that visits every arm, so an added arm fails loudly instead of passing silently.

## [07]-[SNAPSHOTS]

`toMatchFileSnapshot` owns file-backed goldens, and its primary consumer is the contracts corpus: decode a `tests/contracts/` asset, re-encode it, and snapshot-prove equivalence under the corpus law in [tests/contracts/README.md](../contracts/README.md). Inline snapshots carry small stable projections only.

## [08]-[BROWSER_E2E]

Browser-mode suites run real browser semantics through the vitest browser provider and stay in the unit/property lanes' runner; e2e suites live under `tests/typescript/e2e` on the playwright rail and own full user-flow proof. The split is capability-driven: browser-mode proves DOM-coupled units, e2e proves flows across pages and processes. Browser-mode suites are never mutated — the mutation runner does not support them, and a mutated browser suite is noise, not signal. E2e artifacts route with the rest of the TS estate under `.artifacts/typescript/`.

## [09]-[GATES]

The root `vitest.config.ts` is the runner authority: coverage and result artifacts route to `.artifacts/typescript/`, runner cache to `.cache/vitest`, and per-package projects return with the TS buildout. Nx target defaults in `nx.json` mirror those outputs for the project graph. Mutation runs ride StrykerJS from the root `stryker.config.json` — root residency keeps every invocation, flagged or bare, inside auto-discovery so sandbox state can never fall back to a root `.stryker-tmp/`; temp state rides `.cache/stryker/`, reports land in `.artifacts/typescript/stryker`, and the incremental file stays uncommitted.

## [10]-[DENSITY_AND_BANS]

A spec file is strong when one generated domain attacks decoding, projection, failure tags, and an independent oracle together. Before a second `it` that shares setup with an existing one, collapse into `it.effect.prop`, a case matrix, or an `it.layer` suite; a spec earns lines only through a new oracle, boundary, or product-bug guard.

[BANNED_SHAPES]:
- Real-time waits in the unit lane; `TestClock` owns time.
- Type-cast escapes in specs: an `any` or unchecked cast in a test hides exactly the boundary the test exists to prove.
