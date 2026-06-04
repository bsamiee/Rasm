# [H1][BRIDGE_RUNTIME]

## [1][OWNERSHIP]

| [INDEX] | [BEHAVIOR] | [OWNER] |
| :-----: | ---------- | ------- |
| [1] | Pure managed constructors, unions, SmartEnums, math | xUnit/CsCheck |
| [2] | RhinoDoc, viewport, command input, UI thread | Bridge scenario |
| [3] | GH document, canvas, wires, editor events | Bridge scenario |
| [4] | Bridge protocol/client/plugin behavior | Bridge scenarios and tool checks |

## [2][RULES]

- Static specs may classify bridge-owned behavior, but must not pretend to execute it.
- Pair new bridge scenarios with owning source files through `uv run python -m tools.quality bridge check <target> [<scenario.verify.csx>]` or run them through `uv run python -m tools.quality bridge verify <path-or-glob>`.
- Place library-owned scenarios under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/`; `uv run python -m tools.quality bridge verify` maps that convention to `libs/csharp/<Project>/<Project>.csproj`.
- Keep scenarios source-only: no `#r`, no `#load`, and no absolute build-output paths.
- Host/package collisions are evidence. Investigate loaded Rhino assemblies before weakening scenarios.
- Keep bridge output as JSON evidence plus captures under `.artifacts/rhino/verify` or `.artifacts/rhino/bridge/check`.
- On macOS, Rasm.Vectors static specs should treat successful `Plane`, `Curve`, `Surface`, `Mesh`, native polyline closure/conversion, vector unitization/tiny checks, point-cloud topology, and geometry materialization as bridge-owned unless a current run proves the path is pure managed.
- Static specs still own managed input guards around those payloads: null/default validation, unsupported outputs, category shape, and no exception-shaped failures.
- Do not replace a bridge-owned success case with a weaker xUnit shape assertion. Add a scenario or record the exact executable gap.

## [3][RECLASSIFICATION]

When a RhinoWIP update or product change makes a previously bridge-owned API genuinely pure-managed, reclassify it back to static via the following audit:

1. Run `uv run python -m tools.quality bridge check <target> [<scenario.verify.csx>]` and confirm the scenario passes WITHOUT loading any RhinoCommon/Grasshopper host-resolved type at the relevant call path.
2. Run the same behavior as a static xUnit spec; confirm the assertion succeeds in a clean managed process (no RhinoWIP bridge running).
3. If both pass, move the law into the static spec, delete the scenario (or shrink it to the still-bridge-owned remainder), and update the `[1][OWNERSHIP]` table above plus `tests/csharp/AGENTS.md` if the change affects multiple owners.
4. Record the RhinoWIP version that enabled the reclassification in the spec or commit message — reclassifications are reversible if a later update reintroduces the host dependency.

Reclassifications are conservative by default: when in doubt, the behavior stays bridge-owned. The cost of a false-positive reclassification (silent CI failure on next RhinoWIP regression) outweighs the cost of an over-strict bridge classification.

## [4][SCENARIO_GROUPING]

Every `uv run python -m tools.quality bridge verify <scenario>` invocation pays a 3-8s Rhino handshake (assembly probing, RhinoCode resolution, document init). N independent `.verify.csx` files cost N × handshake; grouping K related scenarios into one file costs 1 × handshake + K × scenario body.

| [PATTERN] | [EXAMPLE] |
| --------- | --------- |
| One scenario per concern | `vectors-mesh-topology.verify.csx` (Euler check only) |
| Thematic group | `vectors-mesh-topology-and-validity.verify.csx` (Euler + naked-edge + closed-watertight + non-manifold detection) |

Group when scenarios:
- Share the same fixture geometry (one cube/tetrahedron loaded once, reused for 4-6 assertions).
- Cover related production code paths (`Mesh.IsValid` + `Mesh.IsClosed` + `Mesh.IsSolid` are all native predicates on the same Mesh).
- Have similar performance cost (do not group a 50ms predicate with a 30s eigendecomposition).

Do NOT group when:
- A scenario's failure must be triagable without parsing N other assertions in the same evidence file.
- The fixture setup for one scenario is incompatible with another (different RhinoDoc unit systems, conflicting active views).
- The grouped file would exceed 200 LOC — split into thematic subgroups.

Evidence channel: grouped assertions populate a shared `FactBag` via `facts.Add(string key, object value);` inside the `Scenario.Run` body. On scope exit the harness emits one `facts={json}` plain line plus one `rasm.rhino-bridge.evidence=facts={json}` marker carrying the whole dictionary. When a grouped scenario fails, the JSON evidence under `.artifacts/rhino/verify/<scenario>.json` shows the predicate that threw plus the full fact dictionary collected before the throw.

## [5][SCENARIO_LOC_GUIDANCE]

| [TYPE] | [TARGET_LOC] | [NOTES] |
| ------ | ------------ | ------- |
| Single-concern scenario | 30-80 | Fixture setup + 1-2 assertions + `facts.Add` lines. |
| Thematic group | 80-200 | 4-8 assertions over shared fixture, all wrapped in one `Scenario.Run`. |
| Hard cap per file | 250 | Above 250, split into thematic subgroups. |

Use `Rasm.TestKit.Scenarios.*` helpers (`Scenario.Run`, `FactBag`, `Capture`, `DocumentScope`, `Probe`) to keep scenario boilerplate compact. Adding to `Scenarios/*` is permitted when the helper has 2+ scenario consumers across different specs.
