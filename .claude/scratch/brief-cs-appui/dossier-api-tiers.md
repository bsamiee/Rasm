# DOSSIER — Rasm.AppUi — LANE: api-tiers

Read-only adversarial survey of BOTH `.api` catalog tiers + `Directory.Packages.props` + `Rasm.AppUi.csproj`, judged against the 36 owning `.planning` pages. Stance: hostile; catalogs presumed naive/illusory until disk proves mining. Pages read fully: manifests (2), governance (README/ARCHITECTURE/IDEAS/TASKLOG), catalogs read in full (loro, drafting-export, mapsui, nodeeditor, pdfsharp) + structural/consumption sweep of all 57 target + 30 shared; consuming pages read in full (Editing/collab, Render/drafting head, Render/capture PDF region, Render/evidence probe).

## [00]-[TIER_CENSUS]

- Target tier `libs/csharp/Rasm.AppUi/.api/`: 57 catalogs, 9314 loc (avg ~163). Uniform 3-section stub grammar `[01]-[PACKAGE_SURFACE]` / `[02]-[PUBLIC_TYPES]` / `[03]-[ENTRYPOINTS]`, richer ones add `[04]-[IMPLEMENTATION_LAW]`/`[ERROR_TAXONOMY]` + `[05]-[STACKING_AND_RAIL]`. Asset-only rows (harfbuzz-native, skia-native, hotavalonia) use `[PACKAGE_ASSETS]`/`[ASSET_ENTRYPOINTS]`.
- Shared tier `libs/csharp/.api/`: 30 catalogs (substrate). AppUi consumes LanguageExt.Core / Thinktecture.Runtime.Extensions(+Json) / NodaTime / System.IO.Hashing / UnitsNet / Verify / Unicolour.
- Manifest `[App UI]` group: ~66 pinned ids (Directory.Packages.props:320-386) + `[Bridge Runtime]` + substrate rows; `.csproj` binds them in 14 label groups. Central-only, hand-grouped — CPM law clean.
- INTRINSIC catalog craft is HIGH (8-9/10): real signatures, honest TFM/floor notes (e.g. loro netstandard2.0-only, mapsui `net9.0` top, ACadSharp `lib/net9.0`), full entrypoint + stacking + rail-law. Defects are RELATIONAL/ARCHITECTURAL, not craft: catalogs document consumers/seams the pages never realize, one over-claims a cross-package READ leg, one seam persists the wrong durable form, substrate catalogs duplicate the shared tier.

## [01]-[SCOPE_MANDATE_DISPOSITION]

### (1) LoroCs live-collaboration vs Persistence CrdtOpWire — DEFECT, campaign-defining
- `api-loro.md` (182 loc) is the single best-mined catalog in the tier — full container forest / value+diff+export unions / lifecycle / subscriptions / per-kind ops / error taxonomy / stacking. Charter honest: live merge authority, companion-only (`loro.dylib` firebreak), retires bespoke `NotebookCrdt` LWW.
- BUT the durable seam is WRONG. `api-loro.md:172` and `Editing/collab.md:101,105,117-120,145-150` persist Loro's OWN `Export(Snapshot)` bytes as the `XxHash128` content-keyed durable blob, with `collab.md:105` stating Persistence "replays the opaque delta blob without re-modeling the CRDT." That is exactly the "Rust-binding CRDT whose wire is its own" the Persistence brief's `CrdtOpWire` cross-runtime bit-parity law FORECLOSES (RASM-CS-PERSISTENCE-BRIEF [ROSTER_RECONCILIATION]). The design aligned at the WRONG layer: it made Persistence a dumb blob-store for Loro's proprietary wire, minting a SECOND durable CRDT representation.
- Correct charter: Loro is LIVE-only; `SyncRail` must TRANSCODE each `SubscribeLocalUpdate` delta into `CrdtOpWire` ops at the broadcast boundary (decode) and hydrate Loro from `CrdtOpWire` on cold-load (replay). `CollabSnapshot`-as-durable-blob (`collab.md:117-120`, `api-loro.md:172` stacking bullet 2) is the DELETED form. `ExportMode.Snapshot/Updates` remain a live/cold-load transport cache, never system-of-record.
- WATERFALL RIPPLE (demanded upstream edit, RASM-CS-PERSISTENCE-BRIEF): Persistence/Sync must expose a `CrdtOpWire` encode/decode seam AppUi transcodes Loro ops against — a capability-row owner-extension on the Sync/CrdtOpWire owner, consumer named = Rasm.AppUi Editing/collab. Not an opaque-blob passthrough.

### (2) netDxf WRITE vs ACadSharp READ — CONCERN-MIXING DEFECT
- Mandate: netDxf DXF/DWG WRITE stays AppUi-owned; Rasm.Bim owns READ via ACadSharp; Fabrication consumes the read seam only.
- Manifest homes them correctly: `netDxf` in `[App UI]` (props:359); `ACadSharp` in `[BIM]` (props:122), NOT in `[App UI]`.
- BUT `api-drafting-export.md` gives AppUi ACadSharp full DWG/DXF READ+WRITE first-class — `DwgReader`/`DxfReader` (lines 76-79, 159-164), `[04]` topology (217) and `[RAIL_LAW]` (230) both state "ACadSharp owns DWG/DXF read AND write round-trip." netDxf likewise catalogued full Load+Save (174-179). And `.csproj:95` binds `ACadSharp` directly into AppUi's `Drafting Export` group.
- The READ leg is Rasm.Bim's. AppUi pulling the BIM-owned ACadSharp reader into its drafting rail is a duplicate-owner/concern-leak. Correct charter: `api-drafting-export.md` scopes ACadSharp/netDxf to the WRITE leg (`DxfWriter`/`DwgWriter`/`SvgWriter` + typed-entity construction); the READ surfaces (`DwgReader`/`DxfReader`/`DxfDocument.Load`) move to the Bim catalog; drop `ACadSharp` from `.csproj:95` (AppUi's DWG output either routes the Bim CAD-writer seam or is netDxf-DXF-only per the mandate's letter). `drafting.md:3` already says "AppUi mints no second CAD writer" — the catalog contradicts the page.

### (3) HiddenLineResult Viewport2D seam vs painter-sort — SATISFIED
- Seam declared BOTH directions: `ARCHITECTURE.md:75-76` (`Render/drafting ← Rasm.Fabrication/Posting [BOUNDARY] HiddenLineSeam BSP`; `Render ← Rasm.Fabrication/Posting/projection [RECEIPT] HiddenLineResult Viewport2D edge sets`). Consumed: `drafting.md:2-3` composes `#PROJECTION_HIDDEN_LINE` and "mints no second hidden-line kernel"; `drafting.md:418` `[HIDDEN_LINE_SEAM]`; `TASKLOG [APPUI_VIEWPORT_SILHOUETTE_CONSUMPTION]-[COMPLETE]` wires the `EdgeStyle.Silhouette`.
- Zero painter-sort remnant across `.planning/` (swept: no `painter.?sort`/`back-to-front sort`/`depth sort`). Mandate item DISPOSED. The DXF/DWG WRITE leg stays AppUi per (2).

### (4) AppHost counterpart seams — MOSTLY SETTLED, one stale row
- AppUi composes AppHost exports as settled vocabulary: `AppUiTelemetry.Contribute`/`TelemetryContributorPort` (collab.md:102,128-129), `ReceiptSinkPort`, determinism kernel for notebook replay (collab.md:216 keeps it DISTINCT from document time-travel — correct). No re-mint of host plumbing observed in the catalogs.
- FaultBand: catalogs/pages use local code bands (`CollabFault` 0x4B00, `ControlFault` 4200); confirm against the AppHost FaultBand registry pinned-mirror rows at author time (not a catalog defect, a reconcile note).

## [02]-[PER-CATALOG_VERDICTS] (defective + notable; unlisted catalogs are well-mined single-owner, verdict 8)

- `api-loro.md` — 9/10 craft, **4/10 as-seamed**. Best-mined catalog; durable-form seam wrong (see §1). Fix = stacking bullets 1-2 (line 171-172) re-point to CrdtOpWire transcode.
- `api-pdfsharp.md` — 9/10 craft, **2/10 as-consumed**. Fully mined (PdfDocument/XGraphics/Security/Signatures/AcroForms/UniversalAccessibility/MigraDoc flow DOM). ZERO page consumers. `[PDF_LAW]:155` + `[REPORT_LAW]:161` name `Render/drafting`+`Render/evidence` as consumers that DO NOT consume it — pages realize PDF via `SKDocument` (see §cross-cutting). Strongest catalog-vs-page split.
- `api-mapsui.md` — 9/10 craft, **2/10 as-consumed**. Excellent 5-assembly mining. ZERO page consumers; self-cites nonexistent consumers ("the Shell map surface composes", `:131` a nonexistent "PDFsharp page"). Unwired vs `ARCHITECTURE.md:67` Charts basemap seam. No owning page exists.
- `api-nodeeditor.md` — 9/10 craft, **2/10 as-consumed**. Deep (model/canvas/engine/export). ZERO page consumers; charter names `Shell/Editing parametric and dependency-graph surfaces` that have NO owning page. `:149` flags a latent dup `Avalonia.Controls.PanAndZoom` transitive vs admitted `PanAndZoom.dll` (handled honestly).
- `api-drafting-export.md` — 8/10 craft, **5/10 as-scoped**. Over-claims ACadSharp/netDxf READ (see §2). Multi-package catalog (ACadSharp+netDxf+OpenXml) — fine grouping.
- `api-unicolour.md` (target) — **DUPLICATE**. Byte-identical 480 loc to `libs/csharp/.api/api-unicolour.md` (shared substrate). Pure duplication; target copy adds zero AppUi overlay. Drop from target tier.
- `api-thinktecture-json.md` (target) — **DUPLICATE + zero-consumer**. Substrate, also in shared tier (differs slightly), zero AppUi-specific page consumption. Drop from target tier; shared owns it.
- `api-hashing.md` / `api-nodatime.md` / `api-unitsnet.md` (target) — substrate overlaps that DIFFER from shared (justify-or-collapse; near-duplicate 176-loc nodatime vs shared 180-loc is questionable overlay value).
- `api-avalonia-fluent.md` — zero per-page token hits; Fluent is the retained composition-level FLOOR beneath Semi (README:100-101), wired once — acceptable, but confirm it is not dead beneath the Semi active layer.
- Well-mined, correctly-consumed (verdict 8, sampled/consumption-confirmed): `api-avalonia.md` (30 pages), `api-skiasharp.md` (18), `api-reactive.md` (17), `api-unitsnet.md` (15), `api-avalonia-fonts.md` (13), `api-reactiveui.md` (11), `api-ursa.md` (9), `api-headless.md` (9), `api-silk-webgpu(-wgpu).md` (7), `api-dock.md`/`api-behaviors.md`/`api-propertygrid.md`/`api-harfbuzz-native.md` (6), `api-libmpv.md`/`api-drywetmidi.md`/`api-hidsharp.md`/`api-silk-*` (single-owner, correctly scoped).

## [03]-[CROSS-CUTTING]

### Unmined admitted capability + hand-rolled reimplementation (STRONGEST cluster)
- PDF/report export: THREE catalogs (`api-pdfsharp.md`, cross-refs in `api-nodeeditor.md:150,162`, `api-mapsui.md:131`) document PDFsharp+MigraDoc as the drafting/evidence/canvas PDF owner. The pages realize PDF/XPS via SkiaSharp `SKDocument.CreatePdf/CreateXps` (`capture.md:478`) over a BESPOKE `FlowBlock` `[Union]` (Text/Tile/Rule, capture.md:294-298) + `FlowFold` pagination (capture.md:324-335), which even hand-maps `FlowBlock.Text → OOXML Paragraph/Run` (capture.md:440). `drafting.md:258,262` PDF arm = `SKDocument`, "mints no second exporter." `evidence.md` realizes NONE of the signing/AES-256/tagged-PDF/AcroForms the `[PDF_LAW]:155`/`[REPORT_LAW]:161` claim. → The bespoke `FlowBlock`/`FlowFold` REIMPLEMENTS MigraDoc's auto-pagination flow DOM (`Document`/`Section`/`PdfDocumentRenderer`/`FormattedDocument`), and `SKDocument` lacks everything PDFsharp `Security`/`Signatures`/`AcroForms`/`UniversalAccessibility` own. Redundancy runs the WRONG way (PDFsharp is richer). INTEGRATION MANDATE, not removal.

### Zero-consumption inventory (admitted, catalogued, never demanded by a page)
| catalog | loc | manifest home | disposition |
| :-- | :-- | :-- | :-- |
| `api-pdfsharp.md` (PDFsharp + MigraDoc) | 162 | `[App UI]` props:362-363 | INTEGRATE: drafting/evidence/canvas PDF owner; collapse `FlowBlock`/`SKDocument.CreatePdf` into MigraDoc+XGraphics |
| `api-mapsui.md` (+ transitive Mapsui/Tiling/Nts/Rendering.Skia) | 139 | `[App UI]` props:355 | INTEGRATE: author `Charts/basemap.md` (or Shell geo surface) realizing `MapControl`/`Map`; wires ARCHITECTURE:67 |
| `api-nodeeditor.md` (NodeEditorAvalonia + .Model) | 150 | `[App UI]` props:360 | INTEGRATE: author `Editing/graph.md` realizing `IDrawingNode`/`DrawingNodeEditor` on ReactiveUI; LoroTree is the CRDT data seam |
Absence of consumers is NEVER a REMOVE reason (brief); all three are INTEGRATION mandates before any removal. No REMOVE candidate has a redundancy proof.

### Cross-tier catalog duplication (folder-architecture)
- Both tiers carry: `api-hashing.md`, `api-nodatime.md`, `api-thinktecture-json.md`, `api-unicolour.md`, `api-unitsnet.md`. `api-unicolour.md` byte-identical (480 loc). Doctrine routes shared substrate to `libs/csharp/.api/`; package tier carries domain + folder-specific overlays. Drop unicolour + thinktecture-json (zero AppUi overlay value); justify-or-collapse the other three.

### Dead / phantom cross-references
- `api-mapsui.md:131` cites a "PDFsharp page" that does not exist. NodeEditor `:150,162` and pdfsharp `:162` cross-cite each other's PDF export as a live convergence that no page realizes. These are dead carriers pointing at the unbuilt PDF integration.

### Unwired declared seams (ARCHITECTURE.md [02])
- `Charts ← Rasm.Bim/Semantics/geospatial` basemap (line 67) — declared, no Mapsui page realizes it.
- The "parametric and dependency-graph" surface referenced by README:146-153, `api-loro.md:176` (LoroTree), `api-nodeeditor.md` — no owning Editing page; the Editing/ codemap (ARCHITECTURE:35-44) has inspector/tables/notebook/livedata/forms/history/media/issues/tour, NO graph/node page. Folder-architecture gap.
- Stale/loose seam rows: `Render/glb → typescript:ui/viewer` (line 63) and `Render/query ← Rasm.Bim/Model` (line 66) name source nodes (`glb`, `query`) absent from the Render codemap; re-point against the four landed sibling briefs.

### Hardcoding-vs-generator (positive controls; the tier mostly generates well)
- `drafting.md:31-57` `SheetSize` `[SmartEnum]` is a proper seed-DATA roster (ISO/ANSI/JIS rows carry mm + standard) — correct generator shape, not enumerated instances.
- Counter-example is the PDF path: `FlowBlock`/`FlowFold` is a hand-rolled pagination ENGINE where MigraDoc's `PdfDocumentRenderer` is the parameterized owner — the one place a bespoke mechanism stands in for admitted capability.

### Concern-mixing
- `api-drafting-export.md` mixes AppUi WRITE with Bim-owned READ (§2).
- PDF concern is split three ways in the catalogs (drafting sheet-PDF, evidence report-PDF, node-editor canvas-PDF) but realized as one `SKDocument`/`FlowBlock` owner in `capture.md` — the catalogs describe a fragmentation the pages already collapsed onto `SKDocument`; the correct collapse is onto MigraDoc/PdfSharp, keeping `capture.md` the single PDF owner (drafting/evidence/canvas compose it).

## [04]-[DOMAIN_GAPS] (roster candidates — named, not researched)
- ICC / CMYK print fidelity: Unicolour owns color science but no admitted package owns ICC-profile transforms for print-accurate CMYK PDF/plot output (drafting/evidence print deliverables). Candidate: a LittleCMS/ICC binding — verify OSS + net10 before admission.
- Vector text-diff / side-by-side merge view for collab review: Loro owns the CRDT; no admitted package renders a visual diff/merge surface for the review tour. Likely buildable on AvaloniaEdit + Loro `Diff`; gap is soft.
- Screen-reader/automation conformance harness: `Shell/accessibility.md` asserts a WCAG luminance gate; no admitted a11y test/automation package beyond Avalonia `AutomationProperties`. TASKLOG already dispositions eye-gaze/switch-access/STT as no-viable-net10 → out of scope; the conformance-test gap remains open but minor.

## [05]-[VERDICT_CANDIDATES] (campaign-defining, evidence-first)
1. **Loro durable-form inversion.** `Editing/collab.md:101,105,117-120,145-150` + `api-loro.md:171-172` persist Loro's own snapshot as the content-keyed durable blob — the exact Rust-wire-as-durable form the Persistence CrdtOpWire brief forecloses. Rule: Loro live-only; `SyncRail` transcodes deltas ↔ `CrdtOpWire` at the boundary; `CollabSnapshot`-as-durable is deleted. Demands an upstream Persistence/Sync CrdtOpWire encode/decode seam (waterfall-ripple).
2. **PDF/report integration mandate (collapse the bespoke `FlowBlock`).** `api-pdfsharp.md` (zero consumers) documents drafting/evidence/canvas PDF; pages realize `SKDocument.CreatePdf` + hand-rolled `FlowBlock`/`FlowFold` (capture.md:294-335,478) that reimplements MigraDoc auto-pagination and lacks PDFsharp signing/AES-256/tagged-PDF/AcroForms. Realize PDFsharp+MigraDoc as the single `capture.md` PDF owner; drafting/evidence/node-editor compose it.
3. **Mapsui integration mandate + unwired basemap seam.** `api-mapsui.md` fully mined, zero consumers; `ARCHITECTURE.md:67` basemap seam unrealized. Author a `Charts/basemap.md` (or Shell geo) page realizing `MapControl`/`Map`/`GeometryFeature` over Bim NTS; retire the "PDFsharp page" phantom (`:131`).
4. **NodeEditor integration mandate + missing Editing/graph page.** `api-nodeeditor.md` fully mined, zero consumers; the parametric/dependency-graph surface (README, LoroTree, catalog) has no owning page. Author `Editing/graph.md` realizing `IDrawingNode`/`DrawingNodeEditor` on ReactiveUI, LoroTree as the CRDT data seam — closes a genuine Editing/ folder-architecture gap.
5. **ACadSharp READ-leg concern leak.** `api-drafting-export.md:76-79,217,230` + `.csproj:95` give AppUi the Bim-owned ACadSharp DWG/DXF READ. Scope the catalog to the WRITE leg (`DxfWriter`/`DwgWriter`/`SvgWriter`); move readers to the Bim catalog; drop `ACadSharp` from `.csproj` — AppUi owns netDxf DXF WRITE per mandate (2).
6. **Cross-tier substrate duplication.** `api-unicolour.md` byte-identical across tiers; `api-thinktecture-json.md` duplicate + zero AppUi consumers; hashing/nodatime/unitsnet near-duplicates. Drop unicolour + thinktecture-json from the target tier; justify-or-collapse the rest — shared tier is the substrate owner.
7. **Catalog craft is high; defects are relational.** The 57-catalog tier is dense, verified-real, TFM-honest (9314 loc). The campaign's api-tiers work is NOT re-mining — it is RE-POINTING seams (Loro→CrdtOpWire, drafting READ→Bim), REALIZING three fully-mined zero-consumer packages into new pages, and DEDUPING substrate. Preserve the mining; fix the wiring.
