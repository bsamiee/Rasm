# Rasm.Rhino Agent Instructions

Scope: `libs/csharp/Rasm.Rhino/` only. Root `AGENTS.md` and `CLAUDE.md` own universal policy; this file adds subtree deltas only.

## Purpose

`Rasm.Rhino` is the canonical RhinoWIP boundary library for command, UI, camera, viewport, document, and future Rhino concern categories.

Each category folder owns one full Rhino concern. Capture native API capability deeply, then expose a smaller, stronger, value-adding OOP boundary with FP/ROP internals. Downstream code should read like product logic, not RhinoCommon sequencing, option plumbing, or ceremony.

## Design Contract

- Build category owners, not wrapper facades. A folder like `Camera/` owns camera/view/named-view/capture capability through one coherent public owner and typed operation rail.
- Preserve full native capability without method spam. Internalize native calls, ordering, failure semantics, redraw/commit behavior, document receipts, and resource lifetimes.
- Add real abstraction value. Encode common intelligence, coherent defaults, validation, batching, projection, target resolution, and native consistency rules inside the boundary.
- Keep public surfaces dense and semantic. Prefer one polymorphic operation algebra over parallel helpers, grab-bag option records, native call mirrors, or convenience renames.
- Keep internals functional. Use `Fin<T>`, `Validation`, `Option`, `Seq`, `TraverseM`, folds, discriminants, and table-driven dispatch instead of imperative branching or scattered state.

## Category Folders

- `Commands/` owns staged command execution, command input, document mutation receipts, selection, options, prompt flow, and command-visible transactions.
- `UI/` owns Eto/Rhino UI integration: dialogs, panels, overlays, mouse callbacks, progress, gumball-style interaction, and UI-thread protection.
- `Camera/` owns viewport target resolution, live camera state, camera mutation, projections, frustums, named views, capture, and sequencing.
- `Blocks/` owns block definitions, instances, live/archive linked state, placement, replacement, graph/audit, events, cleanup, preview/export, and block management. It consumes `Construction` output and preserves `Commands/Selection`, `Commands/Document`, and `Exchange` ownership boundaries.
- `Exchange/` owns file import/export, archive metadata, publish targets, sheet/detail editing, and format registration.
- `Construction/` — implementation pending; see `Construction/ROADMAP.md` only. Do not treat as production boundary until code lands.
- Future folders should follow the same pattern: one concern, one public owner, one operation rail, compact state records, and native API truth preserved internally.

## API Ownership Rules

- Verify risky Rhino behavior against RhinoWIP `RhinoCommon.xml`, decompile evidence when XML is absent, and `uv run python -m tools.quality api doctor`.
- Use `uv run python -m tools.quality ...` for API, static, test, and bridge rails in this checkout. Treat older `scripts/rhino.sh` guidance as stale unless the script exists locally.
- For new category roadmaps, verify every named Rhino member through local XML/decompile evidence before presenting it as available. List false, obsolete, internal, and missing APIs directly in the roadmap.
- Member existence is not value equivalence. A member that compiles can still return a different quantity than the one it is claimed to replace — `File3dm.Views` counts model views, not layout page views. Verify return SEMANTICS by decompiling the backing native call or table, never by confirming the member resolves.
- Treat RhinoWIP 9 as target. Do not rely on older public examples unless current local API evidence confirms semantics.
- Keep `Domain` and `Analysis` as quality references for C# shape, ROP rails, polymorphism, and density unless scope explicitly expands.
- Reuse `LanguageExt` and `Thinktecture` deliberately when they collapse surface area, strengthen types, or remove repeated logic.
- Use `docs/system-api-map` for BCL, `System.*`, package/reference, and C# meta policy. Use local RhinoWIP XML/decompile first for Rhino math/API claims, `docs/external-libs/mathnet` for numerical algorithms, and LanguageExt/Thinktecture docs for rails and generated type collapse. `System.Drawing.Common` is RhinoWIP-hosted UI/raster boundary surface only, not a universal NuGet dependency.

## Surface Rules

- Do not add wrapper-only methods that rename RhinoCommon calls.
- Do not expose every native knob as public parameters. Model semantic operations and keep native detail internal.
- Do not split one concern across owner classes, managers, helpers, compatibility shims, or parallel rails.
- Do not hardcode project policy as invisible constants. Prefer caller-provided values, named policies, native defaults, or explicit default records. Values like `96` DPI belong in a documented capture policy or caller input, not buried in logic.
- Do not remove functionality to reduce LOC. Collapse repeated ownership and preserve capability through denser operations.

## Implementation Rules

- Read existing folder files before editing. Extend canonical owners before creating new files or types.
- Preserve Rhino native coherence. For example, camera location, target, direction, up vector, projection, and detail commit behavior must stay aligned unless an operation explicitly represents a lower-level native apply.
- Convert native return shapes at the boundary: nullable to `Option`/`Fin`, bool failure to `Fin<Unit>`, resource lifetimes to scoped projection, document mutations to existing receipt vocabulary.
- Keep block and construction value internal: idempotency, batch receipts, dependency graphs, source/link policy, overload selection, framed bounds, typed output projection, and native diagnostics belong behind the category rail.
- Batch mutation rails when possible. Apply redraw, commit, disposal, and UI-thread protection at the boundary edge, not in every leaf call.
- Prefer variable-driven algorithms over fixed values. When a default is useful, make it explicit, named, overridable, and tied to Rhino/native semantics.

## UI Folder Boundary Guidance

- `UI/` owns Rhino/Eto thread dispatch, dialog and page semantics, panel lifecycle, RUI menu state, watch event projection, overlay conduits, retained canvas state, motion clocks, and drawing-resource lifetimes.
- Keep `RhinoUi.Use` as the sole UI dispatch edge. Route interactive work through `DispatchThread`; use `RhinoUi.Protect` for native callbacks and user delegates that run inside paint, Eto input, or RUI update events.
- Model native UI events as typed unions with native identities. Gate view events by `RhinoView.Document.RuntimeSerialNumber`; avoid optional bags that mix unrelated view, document, object, selection, replacement, attributes, and layer payloads.
- Treat `RuiUpdateUi.RegisterMenuItem` as process-lifetime native registration. Dedupe by `(file, menu, item)` and swap stored callbacks; no unregister rail exists in RhinoWIP.
- Split motion value and velocity algebra when projection clamps, wraps, or normalizes output. `DrawingColor` projects from signed velocity state; clamped UI values are not valid spring or decay velocities.
- Animate retained overlay state through `RasmOverlay<TState>.Transition` and a document redraw target. Do not push mutable sink ceremony to callers when the overlay already owns state.
- Return `Fin` from wake, toast, paint, and callback bridges where native failure or user exceptions can occur. Silent fire-and-forget paths hide broken UI state.
- Dispose per-paint Eto `Pen` and `Brush` instances. Set canvas focus and handled state intentionally; send cancellation decisions to Eto event args.

## Exchange Folder Boundary Guidance

### Operation rails

- `FileCapability` is `[Flags]` (`Import | Export | Archive | Publish`). `FilePhase.Allows(capability)` is one bitwise check: `Requires == None || (Requires & capability) != None`.
- `FileRasterEncoding` is `[SmartEnum<int>]` carrying `(Format, Image, Compression)`. `FilePublishTarget.Raster(Target, Encoding, Settings)` accepts `FileRasterSettings`.
- `FileOp.Do(new FileExchange.ArchiveValidate(...))` invokes `FileResourceGraph.Validate(source, scheduler)` through the injected `IoScheduler`.
- `FilePublishTarget.Pdf(Target, Prefix, Suffix, Annotate)` sequences prefix blank pages, captured sheet pages, then suffix blank pages.
- `PdfStamp` is a `[Union]` over raw `FilePdf` draw primitives; build `Annotate` with `Seq<PdfStamp>(...).Annotation()`.
- `FileSheetEdit` operations populate the correct `DocumentReceipt` slot.
- `FileScale` is a `[Union]` with GEOMETRY-commit persistence via `DetailGeometry.SetScale(...) && detail.CommitChanges()`.
- `SheetQuery` is the sole page-matching rail: conjunctive filter resolved once in `Sheets.cs`.
- `FileOp.Do(new FileExchange.NamedPosition(...))` wraps `RhinoDoc.NamedPositions`.
- Linked-block refresh is not an Exchange concern; `Blocks/` owns it as `BlockOp.RefreshLinks`.
- `FileFormat.Custom(...)` registers into process-static STM; dialog plug-in registration belongs to the consuming `.rhp`.

### Known RhinoCommon limitations

Verified against RhinoWIP 9. Future work waits for SDK additions or accepts non-public paths:

- No public `ViewTable.Reorder` / `MoveTo` for sheet sequence — `FileSheetEdit.Reorder` rebinds `PageNumber` instead.
- Async file save lacks progress/cancellation on the public API.
- No streaming/lazy `File3dm` reader at the public surface.
- `File3dm.PlugInData` is read-only at the public surface.
- `FilePdf` lacks metadata, encryption, and bookmarks; `SetCustomPages` replaces prior pages.
- Worksession (`.rws`) enumeration is unavailable on the managed surface.
- `File3dm.Audit` / `IsValid` are deprecated and non-informative.
- No in-memory page-view count on `File3dm` — use `File3dm.ReadPageViews(path)` for layout pages.
- Obsolete `DetailViewObject.SetScale(double, UnitSystem, ...)` — use the `LengthUnit` overload.
- No public Rhino 2D geometry primitive family (`Line2d`, etc.).
- Sheet raster transparency tradeoff is encoded via instance `ViewCapture` when `FileRasterSettings.Transparent = true`.
- No detail layer-override enumeration API.
