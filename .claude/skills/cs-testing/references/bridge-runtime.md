# [H1][BRIDGE_RUNTIME]
>**Dictum:** *Native behavior is proven in the host that owns it.*

<br>

## [1][OWNERSHIP]

| [INDEX] | [BEHAVIOR] | [OWNER] |
| :-----: | ---------- | ------- |
| [1] | Pure managed constructors, unions, SmartEnums, math | xUnit/CsCheck |
| [2] | RhinoDoc, viewport, command input, UI thread | Bridge scenario |
| [3] | GH document, canvas, wires, editor events | Bridge scenario |
| [4] | Bridge protocol/client/plugin behavior | Bridge scenarios and tool checks |

## [2][RULES]

- Static specs may classify bridge-owned behavior, but must not pretend to execute it.
- Pair new bridge scenarios with owning source files through `bridge check <source.cs> <scenario.verify.csx>` or run them through `scripts/rhino.sh verify <path-or-glob>`.
- Place library-owned scenarios under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/`; `scripts/rhino.sh verify` maps that convention to `libs/csharp/<Project>/<Project>.csproj`.
- Keep scenarios source-only: no `#r`, no `#load`, and no absolute build-output paths.
- Host/package collisions are evidence. Investigate loaded Rhino assemblies before weakening scenarios.
- Keep bridge output as JSON evidence plus captures under `.artifacts/rhino/verify` or `.artifacts/rhino/bridge/check`.
- On macOS, Rasm.Vectors static specs should treat successful `Plane`, `Curve`, `Surface`, `Mesh`, native polyline closure/conversion, vector unitization/tiny checks, point-cloud topology, and geometry materialization as bridge-owned unless a current run proves the path is pure managed.
- Static specs still own managed input guards around those payloads: null/default validation, unsupported outputs, category shape, and no exception-shaped failures.
- Do not replace a bridge-owned success case with a weaker xUnit shape assertion. Add a scenario or record the exact executable gap.

## [3][RECLASSIFICATION]

When a RhinoWIP update or product change makes a previously bridge-owned API genuinely pure-managed, reclassify it back to static via the following audit:

1. Run `bash scripts/rhino.sh bridge check <source.cs> <scenario.verify.csx>` and confirm the scenario passes WITHOUT loading any RhinoCommon/Grasshopper host-resolved type at the relevant call path.
2. Run the same behavior as a static xUnit spec; confirm the assertion succeeds in a clean managed process (no RhinoWIP bridge running).
3. If both pass, move the law into the static spec, delete the scenario (or shrink it to the still-bridge-owned remainder), and update the `[1][OWNERSHIP]` table above plus `tests/csharp/AGENTS.md` if the change affects multiple owners.
4. Record the RhinoWIP version that enabled the reclassification in the spec or commit message — reclassifications are reversible if a later update reintroduces the host dependency.

Reclassifications are conservative by default: when in doubt, the behavior stays bridge-owned. The cost of a false-positive reclassification (silent CI failure on next RhinoWIP regression) outweighs the cost of an over-strict bridge classification.
