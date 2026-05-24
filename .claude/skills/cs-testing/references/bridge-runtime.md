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
- Pair new bridge scenarios with owning source files through `bridge check-source`.
- Host/package collisions are evidence. Investigate loaded Rhino assemblies before weakening scenarios.
- Keep bridge output as JSON evidence plus captures under `.artifacts/verify`.
