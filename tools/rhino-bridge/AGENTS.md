# [RHINO_BRIDGE_AGENTS]

Scope: `tools/rhino-bridge/` only. Root policy owns universal quality selection; this file is the bridge-operator delta. `README.md` owns architecture, command catalog, output contract, environment variables, failure reading, and update rules.

## [1][READ_ORDER]

- When changing operator behavior, protocol expectations, command text, or failure interpretation, read `README.md`.
- When choosing static, test, bridge, package, or publish rails, read `CLAUDE.md` for rail selection and `tools/quality/README.md` for command behavior.
- When authoring scenarios, read `tests/csharp/AGENTS.md` and `testing-cs` bridge guidance.
- When changing bridge wire behavior, read `protocol/BridgeWire.cs`, `client/ClientVerb.cs`, `client/Program.cs`, `plugin/Rhino/Transport.cs`, `plugin/Rhino/CodeEngine.cs`, `tools/assay/rails/bridge.py`, and `tools/assay/composition/settings.py` first.

## [2][WHEN_TO_INVOKE]

Use bridge commands only when static managed gates cannot answer runtime host behavior:
- Library `*.verify.csx` scenarios under mirrored test paths.
- Real diagnostics on Rhino/GH projects or source paired with scenarios.
- Host runtime facts that require current RhinoWIP/GH2 assemblies.
- Undocumented host or package member reads through the API rail.

The bridge is not a synthetic unit-test framework; it is the runtime proof rail for source-owned host behavior that static managed gates cannot execute.

Do not invoke bridge rails for synthetic unit tests, coverage probes, static-managed assertions, normal managed compile cleanup, Rhino settings automation, or long-running UI-thread experiments that the host cannot cancel safely.

## [3][BRIDGE_INVARIANTS]

- Bridge check and verify rebuild the owning project and surface stale lockfiles as lock-drift faults; remediate lock drift through restore, not code edits.
- Grasshopper-aware scenarios must drive GH2 through the existing bridge-owned GH2 execution route so isolated resolver state does not split host singletons.
- Bridge-hot assembly warmup must use the existing isolated-warmup owner in `protocol/BridgeWire.cs`; if the needed type is not covered there, report a proof gap instead of adding scenario-local warmup code.
- Scenarios are source-only. The bridge owns reference projection, host assemblies, and injected usings; `#r`, `#load`, and absolute build-output paths are hard errors.

## [4][SCENARIO_KIT]

Author scenarios through the testkit scenario harness. Emit evidence through `Scenario.Run` and `FactBag`; parse the prefixed `rasm.rhino-bridge.evidence=facts={json}` marker, not human-readable duplicate lines or run-local artifact paths.

Use marker scanning for returned envelopes and evidence facts. Do not parse individual legacy `key=value` lines; human-readable duplicates are noise, not the contract.

## [5][ARTIFACTS]

When bridge evidence includes JSON reports, shadow references, summaries, captures, or artifact paths, treat them as run-local evidence and route exact artifact shape or path claims to `README.md`; never persist run-local paths as durable documentation truth.

## [6][REJECTIONS]

- No bridge use for managed static cleanup that static rails can prove.
- No artificial bridge probes without a real production source, host API, assembly freshness, document/canvas, native geometry, capture, resolver, package, or protocol fact to prove.
- No scenario body references, loads, or absolute artifact paths.
- No raw GH2 host types in isolated scenarios when the existing bridge-owned GH2 execution route is required.
- No parsing of human-readable bridge output when protocol markers exist.
- No claim that a bridge run proved behavior unless the scenario emitted structured facts through the marker contract.
- No durable documentation link to a run-retained artifact path.
- No package, deploy, publish, or secret-consuming operation without the quality policy route that owns it.

## [7][STOP_RULES]

If bridge bootstrap, host assembly identity, staged reference, lock drift, marker parsing, fact-envelope shape, or LanguageExt loading fails, fix setup or report the proof gap before changing scenario semantics. If package, deploy, or publish behavior is in scope, route to the operator README and root quality policy before invoking it.
