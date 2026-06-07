# [ASSAY_01_CONTEXT]

## [TRANSCRIPT]

This pass read the governing root instructions, the `coding-python` skill, the `tools/assay` local overlay, the `tools/assay` README, the active `agents-md.md` standard, and the local `docs/standards` overlay before writing this `.reports/` report. No active files were edited.

Source-read transcript:
- Loaded `CLAUDE.md` for repo policy: universal code should reuse canonical object shapes, avoid wrappers and helper proliferation, and collapse behavior into polymorphic surfaces; Python and Markdown route through `coding-python` and `docs/standards`.
- Loaded `tools/assay/AGENTS.md` and found the core local invariant already stated: Assay is one engine over data rows, with catalog rows, selected rails, shared checks, one envelope, and ordered aspect seams.
- Loaded `tools/assay/README.md` and found the operator-facing distinction between command surfaces and runtime backends. The referenced `fsspec / UPath` pattern appears there as an artifact-store shape, explicitly not cloud-native command marketing.
- Read `tools/assay/core/model.py`, `composition/catalog.py`, `composition/registry.py`, `composition/settings.py`, `core/aspect.py`, `core/engine.py`, selected rail modules, and automation files to verify how the pattern is implemented.
- Loaded `docs/standards/agents-md.md` and `docs/standards/AGENTS.md` to frame changes to universal `AGENTS.md` guidance without turning task notes or implementation details into active standards.
- Checked current git state for the target surfaces; `docs/standards/agents-md.md` was already modified, so this pass did not touch it.

## [FOLDER_CONTEXT]

`tools/assay/AGENTS.md` is compact and already unusually close to the desired rule shape. It defines the local overlay scope and routes public command truth to `README.md` at `tools/assay/AGENTS.md:3`, routes rail/verb/tool/wire changes through `core/model.py`, `composition/catalog.py`, and `composition/registry.py` at `tools/assay/AGENTS.md:9`, and routes execution/aspect/routing changes through `core/engine.py`, `core/aspect.py`, and `core/routing.py` at `tools/assay/AGENTS.md:10`.

The main architecture sentence is the owner map: one engine over data rows, programs as catalog rows, rails selecting rows, engine running checks, one envelope shape, and cross-cutting behavior through ordered aspect seams at `tools/assay/AGENTS.md:13-15`.

The README reinforces the same shape from the operator side. CLI argv resolves through registry bind rows, rails own settings/scope/routing/check construction/dispatch/fold, and the engine returns completed receipts or faults that emit through one envelope path at `tools/assay/README.md:57-79`. The command table is explicitly curated operator surface, not generated help, at `tools/assay/README.md:83-96`.

The README also names the internal integration pattern directly: runtime model libraries are load-bearing but not command surfaces at `tools/assay/README.md:278`. `fsspec / UPath` enables artifact-store shape and local file default, while normal CLI operation still assumes local or shared paths at `tools/assay/README.md:270-272`; later, the README repeats that `ArtifactStore` is fsspec-shaped and normal CLI use is local file storage at `tools/assay/README.md:345-348`.

## [INTERNALIZATION_PATTERN]

The pattern to codify is: when a new capability is really infrastructure, integrate it into the nearest core axis, row, aspect, envelope, fold, store, or tagged union. Do not expose it as a knob, helper module, wrapper type, parallel params object, or second output shape.

Evidence:
- Axes carry payload behavior. `Runner`, `Input`, `Language`, and `Mode` are enums with command prefixes, input flags, routing strategy, suffixes, streaming, and write flags embedded at `tools/assay/core/model.py:18-97`. This prevents separate token maps, routing tables, and flag wrappers.
- One row shape owns executable and in-process tools. `Tool` carries runner, command, input placement, language, claim, mode, parser, and thunk at `tools/assay/core/model.py:174-187`; `Check` then binds that row to concrete scope at `tools/assay/core/model.py:189-198`.
- One wire model owns reports. `Completed`, `Fault`, `Counts`, `Artifact`, `Match`, rail-specific `Detail` variants, `Report`, and `Envelope` are defined in one model file at `tools/assay/core/model.py:200-367`; `fold` and `envelope` project receipts into that wire at `tools/assay/core/model.py:480-528`.
- Catalog rows own tool selection. `TOOLS` encodes Python, TypeScript, C#, Bash, SQL, docs, code, API, bridge, and package entries as `Tool` rows at `tools/assay/composition/catalog.py:197-299`; `select` filters them by claim and language at `tools/assay/composition/catalog.py:302-313`.
- Registry rows own command binding. `REGISTRY` binds claim, verb, handler, params type, and help text at `tools/assay/composition/registry.py:117-146`; `build_app` folds those rows into the Cyclopts command tree at `tools/assay/composition/registry.py:580-595`.
- Aspects are ordered seams, not scattered decorators. `Slot` is the layer order at `tools/assay/core/aspect.py:45-52`; `assemble` validates order at `tools/assay/core/aspect.py:161-173`; `_RAIL_LAYERS` wires checked/logged/traced at `tools/assay/composition/registry.py:110-115`; engine spawn adds retry/tracing/checking at `tools/assay/core/engine.py:299-310`.
- The `fsspec` example is internalized under settings and history. `ArtifactFileSystem` defines only the structural subset of fsspec Assay needs at `tools/assay/composition/settings.py:60-99`; `AssaySettings.store` builds an `ArtifactStore` from `fsspec.filesystem` at `tools/assay/composition/settings.py:196-207`; `ArtifactStore.ensure`, `glob`, and `exists` provide the store operations at `tools/assay/composition/settings.py:221-248`; registry history uses the same store for envelope persistence at `tools/assay/composition/registry.py:343-365`.
- Remote execution follows the same pattern. `ASSAY_EXEC_TARGET` is a setting at `tools/assay/composition/settings.py:138-145`, validates to local or `ssh://` at `tools/assay/composition/settings.py:147-164`, and is consumed inside the shared process backend at `tools/assay/core/engine.py:141-167`, not exposed as a separate remote rail.
- Automation extends tagged unions and reuses registry/engine envelopes. `Trigger` and `Action` unions are declared at `tools/assay/automation/model.py:85-89`; rail actions dispatch through `REGISTRY` at `tools/assay/automation/engine.py:127-141`; program actions wrap their result into the same envelope concept at `tools/assay/automation/engine.py:144-151`; sequence behavior folds by `RailStatus` at `tools/assay/automation/engine.py:175-192`.

## [FINDINGS]

[1] `tools/assay/AGENTS.md` already states the correct local grammar, but it can make the `fsspec`/runtime-backend rule explicit. The current overlay says settings/store belongs to `composition/settings.py` at `tools/assay/AGENTS.md:46` and rejects wrappers at `tools/assay/AGENTS.md:53`; it does not yet say that runtime backends such as artifact storage and remote process execution must be internalized under settings, engine, store, and envelope history rather than exposed as new operator surfaces.

[2] The extension grammar is strong but could be more action-complete for storage, history, remote execution, and runtime backend adoption. It names Program, Language, Verb, Detail, Aspect, Automation, and In-process tool at `tools/assay/AGENTS.md:19-25`. A backend/storage line would prevent future agents from turning `fsspec`, UPath, SSH, tracing, or persistence into agent-facing flags and parallel command surfaces.

[3] The overlay's "no knobs" rule is present but general. `tools/assay/AGENTS.md:33` says improve tools through internal behavior, resilience, routing, and typed failure, not agent-facing knobs. The stronger codified version should name the replacement rails: axis payload, catalog row, registry bind, detail variant, aspect slot, store method, history fold, or envelope field.

[4] Universal `agents-md.md` already has the general form needed to absorb this pattern. It asks authors to identify the local owner rail, value, algebra, fold, table, receipt, or boundary before adding another one at `docs/standards/agents-md.md:14`; it defines owner contract as extension grammar and boundary conversions at `docs/standards/agents-md.md:36`; it says a useful rule names trigger, target, owner, extension action, and rejected substitute at `docs/standards/agents-md.md:81-89`.

[5] Universal guidance could be sharpened by adding "internalization before exposure" as a reusable local-extension rule. `docs/standards/agents-md.md:52-58` already requires boundary/tool/trust/stop rules in confused domains, and `docs/standards/agents-md.md:93-110` routes command syntax and tool behavior away to READMEs/proof owners. It should additionally say that dependency or backend adoption belongs inside the existing owner algebra unless it changes reader action enough to be a documented surface.

[6] The active standard should avoid naming `fsspec` itself as universal policy. `docs/standards/agents-md.md:116-125` bans fragile exact facts unless they are local route targets or owner identifiers. The universal rule should cite the pattern generically; `tools/assay/AGENTS.md` can name `fsspec`, UPath, SSH, `ArtifactStore`, and `Envelope` because those are local owner identifiers and route targets.

## [RECOMMENDED_OVERLAY_CHANGES]

For `tools/assay/AGENTS.md`, add one rule to `## [3][EXTENSION_GRAMMAR]`:
- Runtime backend or storage behavior: extend `AssaySettings`, `ArtifactStore`, `ArtifactScope`, `engine` execution, history persistence, or the existing envelope/artifact rows before adding a CLI flag, command family, helper module, wrapper service, or parallel store type.

For `tools/assay/AGENTS.md`, strengthen `## [4][ENGINEERING_RULES]`:
- Runtime libraries are internal mechanisms unless they change the machine envelope or operator workflow. Integrate them through axis payloads, catalog rows, aspect slots, store methods, status folds, and detail variants; do not market them as new command surfaces.
- Backend capability must preserve local/default behavior through the same `Envelope`, `Report`, `Artifact`, `Match`, and history shapes. If the backend cannot share those shapes, stop and redesign the owner rail before adding a knob.

For `tools/assay/AGENTS.md`, add one rejection to `## [6][REJECTIONS]`:
- No backend-branded command, setting, params type, storage wrapper, or remote/cloud mode when `composition/settings.py`, `core/engine.py`, `composition/registry.py`, or the existing wire model can internalize the behavior.

For `tools/assay/AGENTS.md`, optionally add one read-order line:
- When changing artifact storage, retained history, run scopes, or backend defaults, read `composition/settings.py`, `composition/registry.py`, and `README.md` to decide whether the change is internal behavior or public operator workflow.

## [UNIVERSAL_PATTERNS_TO_CODIFY]

Add a compact "internalization before exposure" rule to `docs/standards/agents-md.md` under `## [7][LOCAL_EXTENSION_GRAMMAR]`:
- When a local dependency, backend, runtime, or integration adds capability, first identify the existing owner algebra: axis enum payload, row table, registry bind, tagged union case, receipt fold, aspect slot, boundary adapter, artifact/store shape, or envelope field. Extend that owner before adding flags, helper modules, wrapper services, parallel models, compatibility aliases, or command surfaces.

Add a companion rejection example near the existing accepted/rejected example:
- Accepted: When adding artifact storage backend behavior, extend the store/settings owner and preserve the existing artifact and envelope folds before documenting operator workflow.
- Rejected: Add a storage helper, cloud-mode flag, and backend-specific report.
- Reason: the accepted form internalizes capability into the existing owner rail; the rejected form exposes implementation as surface area.

Add an anti-fragility note, not a dependency catalog:
- Local overlays may name exact backend libraries only when the library is a local owner identifier, forbidden token, route target, or trust boundary. Universal standards describe the internalization rule without copying library names from one folder.

Add a route-away clarification:
- README files own public workflow and command semantics after a backend changes reader action. `AGENTS.md` owns only the local rule that the backend must be internalized through existing owners first.

## [CONFIDENCE]

High. The pattern is directly supported by `tools/assay/AGENTS.md`, the README's runtime-backend section, and source implementation across `core/model.py`, `composition/catalog.py`, `composition/registry.py`, `composition/settings.py`, `core/aspect.py`, `core/engine.py`, rails, and automation. I did not run Python gates because this was a read-only .reports/report task with no active source edits.
