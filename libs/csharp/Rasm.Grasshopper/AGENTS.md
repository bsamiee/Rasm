# [RASM_GRASSHOPPER_AGENTS]

Scope: `libs/csharp/Rasm.Grasshopper/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; this file adds Grasshopper 2 boundary deltas.

## [1][SCOPE]

`Rasm.Grasshopper` is the canonical Grasshopper 2 boundary over `Rasm`. It captures host API capability, preserves native semantics, and exposes smaller, stronger component, data, and UI rails for downstream app code.

Downstream code should declare intent, ports, outputs, component specs, and UI requests without hand-rolling GH2 lifecycle, data access, tree paths, conversion, disposal, undo, repaint, or UI-thread sequencing.

## [2][READ_ORDER]

- When changing component/data flow, read `Components/` to find the owner for ports, outputs, conversion, diagnostics, and ownership transfer.
- When changing canvas, editor, document, wire, layout, event, paint, or motion behavior, read `UI/` to find the typed UI rail.
- When changing kernel integration or analysis operation usage, read `Rasm/AGENTS.md`.
- When naming a GH2 host API fact, use local GH2 XML/decompile evidence before writing the claim.
- When authoring GH2 runtime scenarios, read bridge scenario guidance first.

## [3][EXTENSION_GRAMMAR]

- New component capability: extend component specs, port catalogs, output binding, and ownership-transfer rails before adding one-off component code.
- New GH2 UI request family: extend the UI operation algebra and typed intent factory before adding callers.
- New mutation behavior: route through the mutation rail with undo, repaint, action commit, and snapshot behavior.
- New host fact: place exact signatures in source or architecture proof; keep this overlay to the action rule.
- New app behavior: keep App code a thin caller of component and UI rails.

## [4][BOUNDARY_RULES]

| [INDEX] | [CONCERN]        | [RULE]                                                                                                             |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------------------------- |
|   [1]   | Components       | Own plugin/component infrastructure, ports, output binding, shape conversion, diagnostics, and ownership transfer  |
|   [2]   | UI               | Own canvas, document, editor, input, dialog, menu, toolbar, event, layout, paint, wire, undo, and repaint policies |
|   [3]   | `Rasm`           | Own computation kernel and analysis semantics                                                                      |
|   [4]   | Bridge scenarios | Own successful GH2 runtime behavior                                                                                |
|   [5]   | App code         | Stays a thin caller of component and UI rails                                                                      |

## [5][REJECTIONS]

- No wrapper-only methods that rename GH2 calls.
- No second local operation model for analysis.
- No duplicate host access paths, UI frameworks, event polling replacements, managers, helpers, or compatibility shims.
- No raw `Grasshopper2.*` in isolated bridge scenarios when the bridge-owned wrapper route is required.
- No public exposure of every native knob as parameters; model semantic operations and hide native detail behind typed policies.

## [6][STOP_RULES]

If static tests cannot execute native GH2 behavior, route to bridge scenarios instead of weakening the spec. If host API evidence is missing, record a proof gap in the owning architecture or source route before publishing the claim.
