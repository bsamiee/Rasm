# [ROOT_02_INTEGRATION]

## [TRANSCRIPT]

Scope received: context / Root Integration research for Rasm `AGENTS.md`, with no active standards edits and no active `AGENTS.md` edits. Required output path: `.reports/agents-md-050626/track-source-scans/11-root-integration.md`.

Research sequence:

1. Loaded repo instruction chain in order: `CLAUDE.md`, then root `AGENTS.md`.
2. Loaded documentation-standard owners for this instruction-file task: `docs/standards/README.md`, `docs/standards/agents-md.md`, and `docs/standards/AGENTS.md`.
3. Loaded cross-stack and proof routing: `docs/usage.md`.
4. Loaded `docs/system-api-map` routing and enough leaves to understand BCL, package, build metadata, and replacement ownership: `README.md`, `bcl.md`, `packages.md`, `meta.md`, and `replacements.md`.
5. Loaded `docs/external-libs` routing and enough leaves to understand external-library first-class ownership: `README.md`, `csharp/language.md`, `languageext/rasm.md`, and `thinktecture/rasm.md`.
6. Inventoried nested overlays with `fd '^AGENTS(\\.override)?\\.md$' .`, then read every nested `AGENTS.md`: `docs/standards/AGENTS.md`, `libs/csharp/AGENTS.md`, every `libs/csharp/Rasm*/AGENTS.md`, `tests/csharp/AGENTS.md`, `tests/csharp/libs/AGENTS.md`, `tools/assay/AGENTS.md`, and `tools/rhino-bridge/AGENTS.md`.
7. Ran a targeted `rg` cross-check for future-standard, baseline, deprecation, compatibility, shim, wrapper, external-library, package, root, route, and duplication wording across the root files, docs route owners, and nested overlays.
8. Wrote only this `.reports/` report. No active standards file or active `AGENTS.md` file was edited.

## [FINDINGS]

1. Root already has the correct authority shape, but it should stay an integrator, not become a second `CLAUDE.md`.
   - `CLAUDE.md` owns universal skill routing, dependency policy, universal constraints, quality gates, plan discipline, and surface preference (`CLAUDE.md:12`, `CLAUDE.md:43`, `CLAUDE.md:58`, `CLAUDE.md:114`, `CLAUDE.md:134`, `CLAUDE.md:142`).
   - Root `AGENTS.md` explicitly says `CLAUDE.md` owns universal project policy, skill routing, and quality rails, while root is the instruction router and first-hop overlay map (`AGENTS.md:3`, `AGENTS.md:7`).
   - `docs/standards/agents-md.md` confirms root `AGENTS.md` is the repository instruction router, not a larger leaf overlay (`docs/standards/agents-md.md:68`, `docs/standards/agents-md.md:70`).

2. The future-forward rule belongs globally in root, with details delegated to owners.
   - `CLAUDE.md` sets the newest viable current versions as the operating baseline after docs or local-output verification (`CLAUDE.md:3`, `CLAUDE.md:35`).
   - Root already states the newest advanced viable language, platform, library, feature, and architecture baseline; existing code and pinned versions are evidence to replace, not constraints (`AGENTS.md:39`).
   - Root also says to work from a greenfield target inside a brownfield tree and preserve behavior through replacement/refactor instead of deprecation, shims, legacy aliases, or baseline caveats (`AGENTS.md:41`).
   - The `AGENTS.md` standard requires future-standard posture as a produced function and root-profile requirement (`docs/standards/agents-md.md:35`, `docs/standards/agents-md.md:74`).
   - The standards overlay applies the same doctrine to documentation: standards are future foundation, not the current project ceiling (`docs/standards/AGENTS.md:38`, `docs/standards/AGENTS.md:40`, `docs/standards/AGENTS.md:63`).

3. Root should enforce holistic internal replacement, not compatibility rails.
   - `CLAUDE.md` bans shims, adapters, legacy aliases, `[Obsolete]` wrappers, and backwards-compat surfaces when collapse improves the system (`CLAUDE.md:72`).
   - Root already prefers root-cause refactoring, caller updates, and obsolete-path removal over additive wrappers or compatibility shims (`AGENTS.md:37`).
   - Library-family guidance rejects compatibility aliases or transitional wrappers after the canonical owner exists (`libs/csharp/AGENTS.md:52`).
   - Rhino and GH overlays localize the same rule in concrete owner terms: no compatibility shims, stale public names, wrapper-only host renames, or parallel rails (`libs/csharp/Rasm.Rhino/AGENTS.md:50`, `libs/csharp/Rasm.Rhino/AGENTS.md:52`, `libs/csharp/Rasm.Rhino/AGENTS.md:55`, `libs/csharp/Rasm.Grasshopper/AGENTS.md:39`, `libs/csharp/Rasm.Grasshopper/AGENTS.md:41`).

4. External-lib-first is already universal in `CLAUDE.md`, but root needs only a route-and-precedence bridge.
   - `CLAUDE.md` names approved dependencies as primary implementation surface and requires direct native API integration without thin wrappers (`CLAUDE.md:45`, `CLAUDE.md:46`, `CLAUDE.md:47`, `CLAUDE.md:48`, `CLAUDE.md:49`, `CLAUDE.md:50`, `CLAUDE.md:51`).
   - `docs/usage.md` gives the cross-stack owner ladder: RhinoCommon, GH2, MathNet, BCL/System, LanguageExt, Thinktecture, platform libraries, then composition root (`docs/usage.md:5`, `docs/usage.md:7`, `docs/usage.md:17`).
   - `docs/usage.md` rejects API wrapping without domain value, package future-intent pins, first-consumer candidates as runtime proof, public docs over local XML, and current early implementation symbols as doctrine (`docs/usage.md:46`, `docs/usage.md:48`, `docs/usage.md:49`, `docs/usage.md:50`, `docs/usage.md:53`, `docs/usage.md:54`).
   - `docs/external-libs/README.md` scopes approved non-`System.*` APIs, routes BCL/package/host policy to `docs/system-api-map`, and says to use approved library APIs directly without wrapping, renaming, or mirroring (`docs/external-libs/README.md:3`, `docs/external-libs/README.md:24`, `docs/external-libs/README.md:26`).
   - `docs/system-api-map/README.md` owns BCL, package state, host references, and build metadata; it routes product-library APIs back to `docs/external-libs` (`docs/system-api-map/README.md:3`, `docs/system-api-map/README.md:14`, `docs/system-api-map/README.md:24`).

5. Polymorphic minimal surface should remain root-level doctrine, while local overlays name the exact extension rail.
   - `CLAUDE.md` requires polymorphic, agnostic, universal code and canonical names (`CLAUDE.md:5`, `CLAUDE.md:10`).
   - `CLAUDE.md` bans schema/type proliferation, wrapper files, parallel concerns, and file-splitting for LOC; it requires collapse into polymorphic surfaces, discriminants, reusable projections, and co-located domain logic (`CLAUDE.md:65`, `CLAUDE.md:66`, `CLAUDE.md:67`, `CLAUDE.md:68`, `CLAUDE.md:70`, `CLAUDE.md:71`, `CLAUDE.md:78`, `CLAUDE.md:82`).
   - Root repeats the generalized mechanism: collapse repeated case families into operation algebras, smart enums, unions, folds, projection carriers, typed receipts, or source-owned tables (`AGENTS.md:43`).
   - Leaf overlays already translate this into actionable local grammar: extend component specs and UI operation algebras in GH2 (`libs/csharp/Rasm.Grasshopper/AGENTS.md:21`, `libs/csharp/Rasm.Grasshopper/AGENTS.md:22`), extend category owners and case-row rails in Rhino (`libs/csharp/Rasm.Rhino/AGENTS.md:21`, `libs/csharp/Rasm.Rhino/AGENTS.md:24`), extend store lifecycle/query rails in Persistence (`libs/csharp/Rasm.Persistence/AGENTS.md:21`), and add one catalog row / tagged case in Assay (`tools/assay/AGENTS.md:19`, `tools/assay/AGENTS.md:24`, `tools/assay/AGENTS.md:25`).

6. Root should not copy nested owner bodies.
   - Root read order already routes C# libraries, tests, docs, assay, bridge, usage, system API, external libs, host libs, and testing libs to their owners (`AGENTS.md:13`, `AGENTS.md:23`).
   - `docs/standards/agents-md.md` says leaf overlays should keep only stable, local, non-derivable, action-changing guidance (`docs/standards/agents-md.md:18`, `docs/standards/agents-md.md:20`) and that root must reject copied nested-owner bodies (`docs/standards/agents-md.md:79`).
   - Nested overlays consistently say root owns universal policy while the leaf adds only local deltas (`libs/csharp/AGENTS.md:3`, `tests/csharp/AGENTS.md:3`, `tools/rhino-bridge/AGENTS.md:3`, `tools/assay/AGENTS.md:3`).

7. Root should not carry package tables, API member catalogs, command catalogs, tool workflow details, or proof transcripts.
   - Root explicitly rejects command catalogs, subtree-local implementation facts, copied provider manuals, fallback tutorials, package version prose, roadmap state, generated contract bodies, bridge transcripts, and C# proof claims for docs-only instruction edits (`AGENTS.md:77`, `AGENTS.md:82`).
   - `docs/standards/agents-md.md` rejects generic validation sections, command catalogs whose owner is a tool README, provider tutorials, current-baseline caveats, and metadata fields (`docs/standards/agents-md.md:60`, `docs/standards/agents-md.md:67`).
   - `tools/rhino-bridge/AGENTS.md` delegates architecture, command catalog, output contract, environment variables, failure reading, and update rules to its README (`tools/rhino-bridge/AGENTS.md:3`).
   - `tools/assay/AGENTS.md` delegates command surface, operator workflow, and public tool reference to its README (`tools/assay/AGENTS.md:3`).
   - `docs/system-api-map/packages.md` owns package state (`docs/system-api-map/packages.md:1`, `docs/system-api-map/packages.md:19`); root should route there, not summarize it.

## [RIPPLE_MODEL]

Use a five-layer ripple.

1. `CLAUDE.md`: universal operating policy.
   - Owns newest verified toolchain posture, skill routing, dependency policy, universal anti-shim and polymorphic constraints, quality gate taxonomy, and plan discipline (`CLAUDE.md:3`, `CLAUDE.md:25`, `CLAUDE.md:45`, `CLAUDE.md:72`, `CLAUDE.md:114`, `CLAUDE.md:134`).

2. Root `AGENTS.md`: repository integration router.
   - Binds `CLAUDE.md` to Rasm route ownership.
   - States the future-forward interpretation once for the repo: plans, docs, and implementation target the newest objectively stronger verified standard; current source and manifests are proof inputs, not baseline ceilings.
   - Requires root-started agents to discover nested overlays before subtree edits (`AGENTS.md:9`, `AGENTS.md:11`).
   - Routes cross-stack, BCL/package, external library, host SDK, testing, docs, quality, and bridge concerns to their owners (`AGENTS.md:51`, `AGENTS.md:69`).

3. Parent overlays: family-level translation.
   - `libs/csharp/AGENTS.md` translates universal engineering posture into library-family extension grammar and project routing (`libs/csharp/AGENTS.md:19`, `libs/csharp/AGENTS.md:40`).
   - `tests/csharp/AGENTS.md` translates test doctrine into adversarial laws, oracle rules, and bridge boundary routing (`tests/csharp/AGENTS.md:16`, `tests/csharp/AGENTS.md:35`).
   - `docs/standards/AGENTS.md` translates docs doctrine into active-corpus read rules, owner routing, and close checks (`docs/standards/AGENTS.md:11`, `docs/standards/AGENTS.md:19`, `docs/standards/AGENTS.md:67`).

4. Leaf overlays: local extension grammar and stop rules.
   - Leaf files should not restate "newest standard, no shims, no wrappers" as slogans. They should name the local owner and replacement action, exactly as `docs/standards/agents-md.md` requires (`docs/standards/agents-md.md:81`, `docs/standards/agents-md.md:91`).
   - Examples already present: GH2 UI operation algebra (`libs/csharp/Rasm.Grasshopper/AGENTS.md:22`), Rhino `WatchBus` / `CaptureRecipe` / case-row rail (`libs/csharp/Rasm.Rhino/AGENTS.md:22`, `libs/csharp/Rasm.Rhino/AGENTS.md:24`), Persistence lifecycle/query rail (`libs/csharp/Rasm.Persistence/AGENTS.md:21`), Assay catalog row / registry bind / tagged detail variant (`tools/assay/AGENTS.md:19`, `tools/assay/AGENTS.md:22`, `tools/assay/AGENTS.md:24`).

5. Route-owner docs and READMEs: volatile detail and proof.
   - `docs/usage.md`: cross-stack owner ladder and proof order (`docs/usage.md:5`, `docs/usage.md:56`).
   - `docs/system-api-map`: BCL, package state, host references, build metadata, replacements (`docs/system-api-map/README.md:5`, `docs/system-api-map/README.md:28`).
   - `docs/external-libs`: approved non-`System.*` library APIs and library-specific repo posture (`docs/external-libs/README.md:1`, `docs/external-libs/README.md:24`).
   - Tool READMEs: commands, operator workflows, output contracts, and user-facing tool references (`tools/rhino-bridge/AGENTS.md:3`, `tools/assay/AGENTS.md:3`).

The ripple rule: root states the repo-wide invariant once; parent overlays translate by family; leaf overlays name local owners; route-owner docs carry API/package/tool/proof detail. Any repeated rule appearing in three or more siblings should move upward to the nearest parent, but only if it remains action-changing there (`docs/standards/agents-md.md:133`, `docs/standards/agents-md.md:142`).

## [ROOT_WORDING_CANDIDATES]

Candidate replacement or consolidation for root `## [3][ENGINEERING_CONTRACT]`:

> Treat `CLAUDE.md` as the universal policy owner and this file as the Rasm integration router. Every plan, document, and implementation targets the newest verified language, platform, library, feature, tool, and architecture standard. Current source, manifests, older patterns, and partial adoption are proof inputs and replacement targets; they are never baseline ceilings that weaken the target.

> Prefer one holistic internal replacement over compatibility layers. When the repo owns both sides of an API, update callers, tests, scenarios, docs, and generated/source-truth surfaces with the canonical owner. Do not preserve legacy names through shims, `[Obsolete]`, deprecation windows, compatibility aliases, wrapper-only adapters, or stale public nouns when direct collapse improves correctness, capability, or surface area.

> Approved dependencies and host SDKs are implementation owners, not optional references. Before local machinery, read `docs/usage.md`, `docs/system-api-map`, `docs/external-libs`, and the nearest host overlay that owns the concern. Use native library APIs directly, isolate external naming only at boundary adapters with domain value, and keep package/version/API proof in the manifest or route-owner document.

> Keep one polymorphic surface per concern. Extend existing operation algebras, smart enums, unions, folds, projection carriers, typed receipt fields, source-owned tables, query algebras, or owner-local records before adding objects, files, modes, parameters, public entrypoints, helper functions, or parallel rails.

Candidate additions or sharpening for root `## [6][REJECTIONS]`:

> No root-level package tables, package versions, API member catalogs, host SDK member claims, generated contract bodies, runtime artifact paths, command catalogs, validation ladders, bridge transcripts, roadmap state, or subtree implementation maps. Route those facts to `docs/usage.md`, `docs/system-api-map`, `docs/external-libs`, `docs/testing-libs`, tool READMEs, source files, or nearest nested overlays.

> No compatibility prose that preserves old paths, aliases, deprecation windows, wrapper facades, partial adoption, or current-baseline caveats as policy. If a compatibility claim is still operationally required, route it to the owner with current proof; otherwise delete it.

Candidate root read-order sharpening:

> When a change touches a concern with both current implementation and target-standard claims, read the route owner for current proof and apply `CLAUDE.md` plus this engineering contract for target direction. Current proof decides whether a present-tense claim is true; it does not lower the future target.

Candidate root external-lib route wording:

> When a local implementation duplicates an approved library or host SDK concern, route first through `docs/usage.md` owner order, then `docs/system-api-map` for `System.*`/package/build policy or `docs/external-libs` for product-library APIs. Local code may add domain policy, proof, batching, typed failures, or boundary translation; it may not mirror or rename the library surface.

## [RISKS]

1. Over-amplification risk: root already contains the core future-forward rule (`AGENTS.md:37`, `AGENTS.md:41`). Adding too much wording can duplicate `CLAUDE.md` universal policy or `docs/standards/agents-md.md` root-profile guidance.

2. External-lib precedence risk: "external-lib-first" must not flatten the `docs/usage.md` owner ladder. RhinoCommon and GH2 outrank MathNet/BCL/LanguageExt/Thinktecture for their owned host semantics (`docs/usage.md:7`, `docs/usage.md:17`).

3. Boundary-adapter wording risk: `CLAUDE.md` bans shims/adapters as compatibility surfaces, but also permits boundary translation where an external contract requires different names (`CLAUDE.md:9`, `CLAUDE.md:72`). Root wording should reject compatibility adapters while preserving value-adding boundary adapters.

4. Package-fact drift risk: root should route package/package-version truth away because `Directory.Packages.props`, `Directory.Build.props`, `docs/system-api-map/packages.md`, and owner architecture files carry current proof (`docs/usage.md:56`, `docs/system-api-map/packages.md:3`, `docs/system-api-map/packages.md:19`).

5. Leaf-copy risk: repeating the full future-forward doctrine in every overlay would violate the local-delta rule and inflate project-doc budget pressure (`docs/standards/agents-md.md:20`, `docs/standards/agents-md.md:26`, `docs/standards/agents-md.md:79`).

6. `.reports/` artifact risk: this report is session research. It belongs in the explicitly requested `.reports/` path and should not be promoted into active instructions without a separate standards edit that routes each durable rule to its owner (`docs/standards/AGENTS.md:9`, `docs/standards/AGENTS.md:61`).

## [CONFIDENCE]

High.

The root-level rule already exists in current repo files, and the route-owner docs plus nested overlays consistently support the same ripple model. The main recommendation is not a new doctrine; it is wording consolidation: root should state the integration invariant once, then route API/package/tool/proof and local extension details to existing owners.
