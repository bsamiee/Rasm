# [H1][BRIDGE_RUNTIME]
>**Dictum:** *Native behavior is proven in the host that owns it.*

<br>

## [1][OWNERSHIP]

| [INDEX] | [BEHAVIOR] | [OWNER] |
| :-----: | ---------- | ------- |
| [1] | Pure managed constructors, unions, SmartEnums, math | xUnit/CsCheck |
| [2] | RhinoDoc, viewport, command input, UI thread | Bridge scenario |
| [3] | GH document, canvas, wires, editor events | Bridge scenario |
| [4] | Bridge protocol/client/plugin behavior | Bridge scripts/tests |

## [2][RULES]

- Static specs may classify bridge-owned behavior, but must not pretend to execute it.
- Pair new bridge scenarios with owning source files through `bridge check <source.cs> <scenario.verify.csx>`.
- Host/package collisions are evidence. Investigate loaded Rhino assemblies before weakening scenarios.
- Keep bridge output as JSON evidence plus captures under `.artifacts/verify`.
- On macOS, Rasm.Vectors static specs should treat successful `Curve`, `Surface`, `Mesh`, `PlaneSurface`, `Point3d.IsValid`, `Vector3d.IsTiny`, and `Polyline.IsValid` native materialization as bridge-owned unless a current run proves the path is pure managed.
- Static specs still own managed input guards around those payloads: null/default validation, unsupported outputs, category shape, and no exception-shaped failures.
- Do not replace a bridge-owned success case with a weaker xUnit shape assertion. Add a scenario or record the exact executable gap.
