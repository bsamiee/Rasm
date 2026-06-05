# [RHINO_BRIDGE_AGENTS]

Scope: `tools/rhino-bridge/` only. Root policy owns universal quality selection; this file is the bridge-operator delta. `README.md` owns architecture, command catalog, output contract, environment variables, failure reading, and update rules.

## [1][READ_ORDER]

- When changing operator behavior, protocol expectations, command text, or failure interpretation, read `README.md`.
- When choosing static, test, bridge, package, or publish rails, read `CLAUDE.md` quality policy.
- When authoring scenarios, read `tests/csharp/AGENTS.md` and `testing-cs` bridge guidance.
- When changing bridge wire behavior, read protocol, client, plugin, and quality wrapper code first.

## [2][WHEN_TO_INVOKE]

Use bridge commands only when static managed gates cannot answer runtime host behavior:
- Library `*.verify.csx` scenarios under mirrored test paths.
- Real diagnostics on Rhino/GH projects or source paired with scenarios.
- Host runtime facts that require current RhinoWIP/GH2 assemblies.
- Undocumented host or package member reads through the API rail.

Do not invoke bridge rails for synthetic unit tests, coverage probes, normal managed compile cleanup, Rhino settings automation, or long-running UI-thread experiments that the host cannot cancel safely.

## [3][BRIDGE_INVARIANTS]

- Bridge check and verify rebuild the owning project and surface stale lockfiles as lock-drift faults; remediate lock drift through restore, not code edits.
- Grasshopper-aware scenarios must drive GH2 through the bridge-owned wrapper route so isolated resolver state does not split host singletons.
- Bridge-hot assemblies must use key types that the isolated resolver can warm up reliably.
- Scenarios are source-only. The bridge owns reference projection, host assemblies, and injected usings; `#r`, `#load`, and absolute build-output paths are hard errors.

## [4][SCENARIO_KIT]

Author scenarios through the testkit scenario harness. Emit facts through the provided fact bag and rely on bridge markers as the structured wire contract.

Use marker scanning for returned envelopes and evidence facts. Do not parse individual legacy `key=value` lines; human-readable duplicates are noise, not the contract.

## [5][ARTIFACTS]

Bridge verification writes ephemeral JSON reports, shadow references, summaries, and captures under the bridge artifact route. Treat those paths as run-local evidence; never persist references into them as durable documentation truth.

## [6][REJECTIONS]

- No bridge use for managed static cleanup that static rails can prove.
- No scenario body references, loads, or absolute artifact paths.
- No raw GH2 host types in isolated scenarios when the wrapper route is required.
- No parsing of human-readable bridge output when protocol markers exist.
- No durable documentation link to a run-retained artifact path.
- No package, deploy, publish, or secret-consuming operation without the quality policy route that owns it.

## [7][STOP_RULES]

If bridge bootstrap, host assembly identity, staged reference, lock drift, or LanguageExt loading fails, fix setup or report the proof gap before changing scenario semantics. If package, deploy, or publish behavior is in scope, route to the operator README and root quality policy before invoking it.
