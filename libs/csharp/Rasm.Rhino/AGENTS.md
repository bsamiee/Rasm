# Rasm.Rhino Agent Instructions

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
- Future folders should follow the same pattern: one concern, one public owner, one operation rail, compact state records, and native API truth preserved internally.

## API Ownership Rules

- Verify risky Rhino behavior against RhinoWIP `RhinoCommon.xml`, decompile evidence when XML is absent, and `scripts/rhino.sh api doctor`.
- Treat RhinoWIP 9 as target. Do not rely on older public examples unless current local API evidence confirms semantics.
- Keep `Domain` and `Analysis` as quality references for C# shape, ROP rails, polymorphism, and density unless scope explicitly expands.
- Reuse `LanguageExt` and `Thinktecture` deliberately when they collapse surface area, strengthen types, or remove repeated logic.

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
- Batch mutation rails when possible. Apply redraw, commit, disposal, and UI-thread protection at the boundary edge, not in every leaf call.
- Prefer variable-driven algorithms over fixed values. When a default is useful, make it explicit, named, overridable, and tied to Rhino/native semantics.

## Exchange Folder Boundary Guidance

- **Capability vocabulary is a bitmask**: `FileCapability` is `[Flags]` (`Import | Export | Archive | Publish`). `FilePhase.Allows(capability)` performs a single bitwise check — `Requires == None || (Requires & capability) != None`. No nested enum, no dispatch switch. Compose freely (`FileCapability.Import | FileCapability.Publish` for PDF/SVG hybrid formats).
- **Raster encoding owns its codec metadata**: `FileRasterEncoding` is `[SmartEnum<int>]` carrying `(Format, Image, Compression)`. `FilePublishTarget.Raster(Target, Encoding, Settings)` accepts a `FileRasterSettings` record (`JpegQuality`, `TiffCompression`, `PngDepth`, `ExifDpi`). `SaveBitmap` reads `encoding.Parameters(settings)` and threads codec params (TIFF LZW/CCITT, PNG colour depth, JPEG quality) through `System.Drawing.Common` `EncoderParameter[]` automatically, with proper disposal.
- **Link validation offload**: `FileOp.Do(new FileExchange.ArchiveValidate(...))` performs `File.Exists` checks across all linked block archives, render texture files, and file references via PLINQ. The operation parallelizes across worker threads but executes synchronously from the calling thread. UI-thread callers MUST wrap the dispatch in `Task.Run` to avoid stutter on archives with hundreds of linked resources. Headless and batch processes can invoke directly.
- **PDF page captures rasterize**: `FilePublishTarget.Pdf.Annotate` callback runs INSIDE `op.Catch` per page; throws are converted to `Fin.Fail` per page. The annotation callback receives `(FilePdf, int pageIndex, Op op)` — use `pdf.DrawText` / `DrawLine` / `DrawBitmap` for title-block stamps.
- **Sheet/detail receipts are typed**: `FileExchange.SheetEdit` operations populate the correct `DocumentReceipt` slot — `Created` for new sheets/details, `Deleted` for removals, `AttributeChanged` for rename/reorder/activate/layer-override/clipping-override/refresh-links. Downstream code can pattern-match on receipt fields for selective undo or audit.
- **Sheet name collision is checked at create**: `FileSheetEdit.Create` fails with `InvalidInput` if a page with the same `PageName` exists (case-insensitive). Callers must remove or rename the existing page first.
- **Sheet reorder via `PageNumber`**: `FileSheetEdit.Reorder(Seq<string>)` rebinds each named page's `PageNumber` to its position in the supplied sequence and triggers `Views.Redraw`. RhinoCommon does not expose `ViewTable.Reorder` / `MoveTo`; rebinding `PageNumber` is the only public surface that preserves page identity.
- **Linked block refresh**: `FileSheetEdit.RefreshLinks(Option<Seq<string>>)` calls `InstanceDefinitionTable.RefreshLinkedBlock(definition)` for each matching definition (all linked blocks if `Archives` is `None`; otherwise the archive-path subset). Synchronous; wrap in `Task.Run` for UI-thread callers with large block counts.
- **Header/footer interpolation**: `FileSheetDecor.HeaderText` / `FooterText` accept `{key}` tokens resolved against `RhinoDoc.Strings`. Built-in tokens: `{page}` (page name), `{index}` (1-based page index), `{total}` (total page count). Unknown keys pass through verbatim.

## Known RhinoCommon Limitations

These capabilities are missing from RhinoCommon (verified against RhinoWIP 9). Future work that wants them must either wait for an SDK addition or invoke a non-public path:

- **`ViewTable.Reorder` / `MoveTo` for sheet sequence** — no public API to reorder `RhinoPageView` instances. `FileSheetEdit.Reorder` rebinds `PageNumber` instead; this preserves page identity but does not directly drive the layout panel ordering (Rhino may re-sort on next file load).
- **`RhinoPageView` page-size mutation** — `PageWidth` / `PageHeight` setters are absent post-creation; resizing a sheet requires duplicate + delete (loses any non-geometry properties tied to the original).
- **Async file save with progress + cancellation** — `File3dm.WriteWithLog` and `RhinoDoc.WriteFile` are synchronous. No `IProgress<int>` or `CancellationToken` parameter; no native worker-thread API. Callers needing UI feedback must wrap in `Task.Run` and accept all-or-nothing semantics.
- **3dm streaming/lazy reader** — `File3dm.ReadWithLog` loads the full archive (optionally filtered by `TableTypeFilter` / `ObjectTypeFilter`). `BinaryArchiveReader` internals are not surfaced for chunk-by-chunk consumption.
- **Plug-in data write** — `File3dm.PlugInData` is read-only at the public surface (`File3dmPlugInDataTable` exposes only enumerable access). Custom plug-in payloads cannot be patched into an existing archive; downstream tools must re-serialize through `RhinoDoc.WriteFile` after re-attaching plug-in user data on the live document.
- **PDF metadata / encryption / bookmarks** — `FilePdf` exposes `Create` / `AddPage` / `Write` / `DrawText` / `DrawLine` / `DrawBitmap` / `LayersAsOptionalContentGroups` only. Title/author/subject/keywords/encryption/permissions/outline must be applied via a downstream PDF library if needed.
- **Worksession (`.rws`) enumeration** — `RhinoDoc.Worksession` is not exposed as a managed type; iteration over linked models, add/remove, and persistence are unavailable.
- **`File3dm.Audit` / `IsValid`** — deprecated since v6; both return success unconditionally and provide no validation. Use `File3dm.ReadWithLog` log output and `FileResourceGraph.ValidateLinks()` for integrity checks instead.
- **`DetailViewObject.SetScale(double, UnitSystem, ...)` overload is obsolete since v9**; use the `LengthUnit` overload only.
- **`ViewCaptureSettings.TransparentBackground`** does not exist as an instance property. `Rhino.Display.ViewCapture.TransparentBackground` is a static, process-wide property; toggling it from a publish pipeline is not thread-safe across concurrent captures. Transparent-output workflows must use `RenderSettings.TransparentBackground` (model-level) or capture via render pipeline.
- **No detail layer-override enumeration** — `Layer.SetPerViewportColor`/`SetPerViewportVisible` write per-viewport overrides keyed by the detail's viewport ID, but RhinoCommon exposes no enumeration API for inspecting which layers have overrides on a given detail. Round-tripping detail layer state requires walking every layer and probing per viewport.
