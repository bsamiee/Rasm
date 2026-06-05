# [RASM_APPUI_AGENTS]

Scope: `libs/csharp/Rasm.AppUi/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; local README, `_ARCHITECTURE.md`, and `ROADMAP.md` own UI platform state, package facts, and implementation sequence.

## [1][READ_ORDER]

- Before production work, read `README.md`, `_ARCHITECTURE.md`, and `ROADMAP.md` to decide package state, embedding proof, and sequence.
- Before changing runtime boot, inbound observables, scheduler handoff, or diagnostics contracts, read `Rasm.AppHost/AGENTS.md`.
- Before changing host UI dispatch, panels, repaint, undo, canvas, or editor behavior, read `Rasm.Rhino/AGENTS.md` and `Rasm.Grasshopper/AGENTS.md`.
- Before package or host-reference changes, read `docs/system-api-map` and `docs/host-libraries.md`.

## [2][OWNER_CONTRACT]

`Rasm.AppUi` is the unified product UI rail. It composes UI packages behind product concepts, owns product UI intent and diagnostic receipts, and delegates final host mutation to Rhino, Grasshopper, AppHost, and Persistence owner rails.

Keep one typed app-surface operation rail for shell, screen, command, live-view, chart, visual, and diagnostic concerns. Package types stay internal unless exposing them carries real product semantics.

## [3][EXTENSION_GRAMMAR]

- New product UI case: extend the app-surface operation rail and typed intent model before adding a screen-specific helper.
- Host embedding behavior: update local architecture proof before expanding rendering, native focus, or panel lifecycle.
- Runtime observation: extend scheduler handoff and typed observable contracts before adding direct host calls.
- Diagnostic or chart behavior: add product-level receipt or projection data; keep package-specific types internal.

## [4][BOUNDARY_RULES]

| [INDEX] | [CONCERN]     | [RULE]                                                                                     |
| :-----: | :------------ | :----------------------------------------------------------------------------------------- |
|   [1]   | UI scheduler  | Construct at the host UI boundary before runtime boot                                      |
|   [2]   | Rhino UI      | Rhino panels, dispatch, repaint, undo, and lifecycle stay in `Rasm.Rhino/UI`               |
|   [3]   | GH2 UI        | GH2 canvas, editor, document, subscriptions, and paint hooks stay in `Rasm.Grasshopper/UI` |
|   [4]   | Runtime       | AppHost schedules, drains, and correlates                                                  |
|   [5]   | Durable state | Persistence owns migrations, snapshots, support artifacts                                  |
|   [6]   | Package facts | Architecture and central manifests own exact package state                                 |

## [5][REJECTIONS]

- No AppUi-owned Rhino/GH2 dispatch, repaint, undo, or lifecycle rail.
- No package-forwarding facade for UI libraries.
- No floating host window path where embedded host surfaces are the invariant.
- No mixed UI paradigms in one screen stack.
- No package versions in documentation text; version truth lives in central manifests.
- No deprecated or replacement package claim without current package-route proof.
