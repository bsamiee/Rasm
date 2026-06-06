# [RASM_GRASSHOPPER_AGENTS]

Scope: `libs/csharp/Rasm.Grasshopper/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; this file adds Grasshopper 2 boundary deltas.

## [1][SCOPE]

`Rasm.Grasshopper` is the canonical Grasshopper 2 boundary over `Rasm`. It captures host API capability, preserves native semantics, and exposes smaller, stronger component, data, and UI rails for downstream app code.

Downstream code declares intent, ports, outputs, component specs, and UI requests through the typed GH2 rails; it does not choreograph GH2 lifecycle, data access, tree paths, conversion, disposal, undo, repaint, or UI-thread sequencing.

## [2][READ_ORDER]

- When changing component/data flow, read `Components/` to find the owner for ports, outputs, conversion, diagnostics, and ownership transfer.
- When changing GH2 UI operation dispatch, scope resolution, or UI-thread behavior, read the `IUiOp<TResult>`, `GrasshopperUiIntent<T>`, `GhUi`, and `GrasshopperUi.Use` owners in `UI/`.
- When changing document mutation, undo, repaint, action commit, snapshot, placement, clipboard, or wire mutation behavior, read `DocumentMutation`, `DocumentMutationReceipt`, `DocumentMutationDelta`, and the shared mutation runner in `UI/`.
- When changing wire query, edit, route, overlay, diagnostics, or reflective GH2 wire internals, read `WireOp` and `WireRepositoryRail`.
- When changing kernel integration or analysis operation usage, read `Rasm/AGENTS.md`.
- When naming a GH2 host API fact, use local GH2 XML/decompile evidence before writing the claim.
- When authoring GH2 runtime scenarios, read `tests/csharp/AGENTS.md`, `tests/csharp/libs/AGENTS.md`, and `tools/rhino-bridge/AGENTS.md` first.

## [3][EXTENSION_GRAMMAR]

- New component capability: extend `ComponentSpec`, `SpecBuilder`, `OutputBinding`, `PortKind`, `Capability`, and ownership-transfer rails before adding one-off parameter, conversion, or diagnostic code.
- New GH2 UI request family: implement an `IUiOp<TResult>` case and surface it through `GrasshopperUiIntent<T>` and `GhUi`; do not add a separate public executor, thread-marshalling path, or caller-side GH2 operation switch.
- New document mutation: extend `DocumentMutation` and run it through `UiRail.RunDocumentMutation` so undo, repaint, action commit, `DocumentMutationReceipt`, `DocumentMutationDelta`, and snapshots stay one owner.
- New wire behavior: extend `WireOp`, `WireEdit`, `WireQuery`, `WireResult`, or `WireRepositoryRail`; do not add a second reflection capsule for GH2 wire internals.
- New canvas motion, haptics, display-link pacing, spring configuration, cosmetic animation, or accessibility-driven motion reduction: extend `UI/Motion.cs` owners; do not add per-feature timers, animation state, event polls, or paint hooks.
- New host fact: place exact signatures in source or architecture proof; keep this overlay to the action rule.
- New app behavior: app packages pass typed `ComponentSpec`, ports, outputs, `GrasshopperUiIntent<T>`, `DocumentMutation`, `WireOp`, and receipts; they do not choreograph GH2 lifecycle or expose raw native knobs.

## [4][BOUNDARY_RULES]

| [INDEX] | [CONCERN]        | [RULE]                                                                                                             |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------------------------- |
|   [1]   | Components       | Own `ComponentSpec`, ports, output binding, conversion, diagnostics, and ownership transfer                        |
|   [2]   | UI               | Own `IUiOp<TResult>`, intents, `DocumentMutation`, `WireOp`, undo, repaint, and snapshots                          |
|   [3]   | `Rasm`           | Own computation kernel and analysis semantics                                                                      |
|   [4]   | Bridge scenarios | Own successful GH2 runtime behavior                                                                                |
|   [5]   | App code         | Stays a thin caller of component and UI rails                                                                      |

## [5][REJECTIONS]

- No wrapper-only methods that rename GH2 calls.
- No second local operation model for analysis.
- No duplicate host access paths, UI frameworks, event polling replacements, managers, helpers, or compatibility shims.
- No public GH2 executor beside `GhUi` and the existing UI intent rail.
- No direct host-internal wire reflection outside `WireRepositoryRail`.
- No mutation path that bypasses the shared mutation runner and receipt/delta rail.
- No raw `Grasshopper2.*` in isolated bridge scenarios when the existing bridge-owned GH2 execution route is required.
- No public exposure of every native knob as parameters; model semantic operations and hide native detail behind typed policies.

## [6][STOP_RULES]

If static tests cannot execute native GH2 behavior, route to bridge scenarios instead of weakening the spec. If host API evidence is missing, record a proof gap in the owning architecture or source route before publishing the claim.
