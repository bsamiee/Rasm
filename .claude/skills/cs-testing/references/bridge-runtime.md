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
