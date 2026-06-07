# [STACKS_CSHARP_AGENTS]

Scope: `docs/stacks/csharp/**`. Parent instructions and `docs/standards` own general documentation behavior; this file adds the local rules for maintaining the C# stack decision atlas.

## [1][READ_BEHAVIOR]

- When changing syntax, expression style, or language baseline, read `language.md` and extend the active syntax rule, replacement, or rejection.
- When changing result flow, effects, schedules, traversal, immutable collections, or boundary state, read `rails-and-effects.md` and extend the rail or effect owner.
- When changing generated value objects, smart enums, unions, factories, equality, validation, or dispatch, read `domain-shapes.md` and extend the generated-shape owner.
- When changing dense solve, iterative sparse solve, eigen, statistics, symbolics gates, or numeric receipts, read `numeric-algorithms.md` and extend the algorithm owner.
- When changing direct sparse solve, CSparse storage, ordering, factorization, factor cache, or residual policy, read `sparse-factorization.md` and extend the sparse factorization owner.
- When changing package graph, globals, host references, tools, analyzers, or package admission, read `platform/build-and-packages.md` and the relevant manifest.
- When changing BCL or `System.*` replacement guidance, read `platform/system-apis.md` and route package adoption to `platform/build-and-packages.md`.
- When changing test-tool behavior, proof-rail selection, or test package guidance, read `testing/README.md` and the concern page that owns the proof rail.
- When changing C# stack concept sequencing, future concept-page admission, or reusable implementation-policy milestones, read `ROADMAP.md` and extend its milestone or task records instead of copying sequence state into this overlay or concept pages.
- When changing cross-stack precedence, read `docs/usage/`; when changing host SDK semantics, read `docs/hosts/` where present.

## [2][TARGETS]

C# 14.0 on `net10.0` is the target language surface. This folder owns C# coding policy for the repository; package-backed capability is first-class when it is source-backed by direct reference, global injection, conditioned build route, host bundle, tool graph, central package graph support attached to a declared closure, tool surface, host route, or an accepted adoption gate.

Do not weaken target guidance with migration caveats, version anxiety, compatibility aliases, deprecation windows, partial-adoption prose, or stale route names. State the target rule, route current graph facts to manifests and `platform/build-and-packages.md`, or mark a proof gap.

## [3][OWNER_CONTRACT]

Concept pages own coding decisions. Extend the owning page by adding or correcting a rule, row, gate, rejection, receipt, bridge, or boundary record. Do not add package-facade pages, summary pages, API catalogs, or parallel external-library folders when a concept page can own the decision.

Package versions, project references, global usings, local tools, host references, analyzers, adoption gates, and package-state records stay in `platform/build-and-packages.md`. BCL replacement decisions stay in `platform/system-apis.md`. Proof-tool selection stays in `testing/**`.

## [4][DEPENDENCY_BOUNDARY]

LanguageExt owns rails, effects, schedules, traversal, immutable collection flow, and boundary state. Thinktecture owns generated domain shapes. MathNet owns dense and iterative numeric algorithms. CSparse owns direct sparse factorization. Test libraries route through `testing/`.

Internalize new library capability into the existing concept owner before exposing a public doc route. Treat external packages as normal implementation material, not secondary adapters; reject provider-branded wrappers, package-shaped docs, consumer-count caveats, and copied upstream manuals unless the existing owner cannot answer a distinct reader action.

## [5][PRESERVATION]

Before replacing, moving, or deleting a C# stack page, inventory load-bearing package IDs, owner routes, gates, qualifiers, API facts that change decisions, and rejection rules. Preserve them in the replacement, route them to the owning page, or delete them only when current proof shows they no longer change action.

Archived or generated source material is evidence only. Strip source history, report process, agent orchestration, and provenance narration before promoting a durable rule into the active docs.

## [6][ROUTE_AWAY]

Route README orientation to `README.md`, package graph truth to `platform/build-and-packages.md`, BCL replacement truth to `platform/system-apis.md`, host runtime semantics to `docs/hosts/`, cross-stack precedence to `docs/usage/`, command syntax to tool owners, and ordinary documentation form to `docs/standards`.

Use these boundaries while maintaining the folder; do not copy them into concept pages as lead sentences, validation bullets, or generic "routes to" prose. A concept page states its own coding decision and omits non-owned facts unless the link changes the reader's implementation choice.

Keep concept pages free of package-version tables, broad upstream API inventories, generic validation ladders, local command catalogs, source-link collections, generated catalog bodies, and compatibility stubs.

## [7][REJECTIONS]

- No compatibility routes for removed package-shaped, map-shaped, or tool-leaf docs.
- No package-shaped documentation when a concept page owns the coding decision.
- No link farms, copied provider manuals, broad member catalogs, or source/provenance sections.
- No defensive version wording in active docs.
- No route-only README unless the boundary has no useful chooser beyond routes.
- No long prose-table cells when grouped records carry the decision better.
- No test-tool leaf file when the concern belongs in a proof-rail page.
