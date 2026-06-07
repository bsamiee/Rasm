# [RASM_APPUI_AGENTS]

Scope: `libs/csharp/Rasm.AppUi/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; local README, `ARCHITECTURE.md`, and `ROADMAP.md` own UI platform state, package facts, and implementation sequence.

## [1]-[READ_ORDER]

- Before production work, read `README.md`, `ARCHITECTURE.md`, and `ROADMAP.md` to decide package state, embedding proof, and sequence.
- Before changing runtime boot, inbound observables, scheduler handoff, or diagnostics contracts, read `Rasm.AppHost/AGENTS.md`.
- Before changing host UI dispatch, panels, repaint, undo, canvas, or editor behavior, read `Rasm.Rhino/AGENTS.md` and `Rasm.Grasshopper/AGENTS.md`.
- Before package or host-reference changes, read `docs/stacks/csharp/platform/` and `docs/usage/composition.md`.

## [2]-[OWNER_CONTRACT]

`Rasm.AppUi` is the unified product UI rail. It composes UI packages behind product concepts, owns product UI intent and diagnostic receipts, and delegates final host mutation to Rhino and Grasshopper owner rails. Runtime and durable-state side effects route through AppHost and Persistence owner rails.

Keep one typed app-surface operation rail for shell, screen, command, live-view, chart, visual, and diagnostic concerns.

Package-backed UI behavior: read `ARCHITECTURE.md` and central manifests, then internalize approved package capability into product shell, screen, command, live-view, chart, visual, diagnostic, scheduler, and receipt rails before exposing package-shaped APIs, toolkit settings, provider selectors, or compatibility aliases.

## [3]-[EXTENSION_GRAMMAR]

- New product UI case: extend typed product intent, `Screen<T>`, scheduler, command, live-view, chart, visual, diagnostic, or receipt rails before adding a screen-specific helper or package-shaped API.
- Host embedding behavior: extend rendering, native focus, panel lifecycle, scheduler, and disposal owners only after `ARCHITECTURE.md` or runtime proof covers the relevant host API gap.
- Runtime observation: extend scheduler handoff and typed observable contracts before adding direct host calls.
- Diagnostic or chart behavior: add product-level receipt or projection data; keep package-specific types internal.
- Native UI proof: route proof gaps through `ARCHITECTURE.md`, host-boundary overlays, central manifests, or runtime proof before changing product rails.

## [4]-[BOUNDARY_RULES]

| [INDEX] | [CONCERN]     | [RULE]                                                                                     |
| :-----: | :------------ | :----------------------------------------------------------------------------------------- |
|   [1]   | UI scheduler  | Construct at the host UI boundary before runtime boot                                      |
|   [2]   | Rhino UI      | Rhino panels, dispatch, repaint, undo, and lifecycle stay in `Rasm.Rhino/UI`               |
|   [3]   | GH2 UI        | GH2 canvas, editor, document, subscriptions, and paint hooks stay in `Rasm.Grasshopper/UI` |
|   [4]   | Runtime       | AppHost schedules, drains, and correlates                                                  |
|   [5]   | Durable state | Persistence owns migrations, snapshots, support artifacts                                  |
|   [6]   | Package facts | Architecture and central manifests own exact package state                                 |

## [5]-[REJECTIONS]

- No AppUi-owned Rhino/GH2 dispatch, repaint, undo, or lifecycle rail.
- No package-forwarding facade for UI libraries.
- No public toolkit settings bag, package option mirror, or floating-window fallback rail; encode behavior as typed product intent, `Screen<T>`, `CommandReceipt`, `DiagnosticReceipt`, or `RasmUiScheduler`.
- No floating host window path where embedded host surfaces are the invariant.
- No mixed UI paradigms in one screen stack.
- No package versions in documentation text; version truth lives in central manifests.
- No old package claim, replacement package claim, or package-route assertion without current package proof.

## [6]-[STOP_RULES]

If SkiaSharp native-major, Avalonia embedding, Rhino/GH2 panel-host API, focus/disposal ordering, software-rendering, or scheduler-handoff proof is missing, stop before expanding UI surface; route the gap to `ARCHITECTURE.md`, host-boundary overlays, central manifests, or runtime proof.
