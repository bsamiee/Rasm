# [ARTIFACTS_REBUILD_BRIEF]

Authoritative campaign input for the ground-up rebuild of `libs/python/artifacts/.planning/`. Every workflow phase (api-completion, the three specific build-outs, the general capstone) reads this file as binding scope. Temporary — deleted at campaign end. This brief fixes telos, structure, package roster, the media capability-detection contract, the AEC owned-vocabulary set, the seam-unification target, the categorical-best mandate, and the C#-side boundary. It does not restate `docs/stacks/python/` doctrine, the authoring standard, or the section-divider grammar — those govern unchanged.

## [01]-[TELOS]

`artifacts` is a **publication/print-production engine that is simultaneously the foundation of a high-end AEC documentation engine** — one unified body, not two arms. A full professional pub/print engine (color-managed, separations-aware, PDF/X-correct, typographically complete, layered-export-clean, metadata/provenance-sealed) is **necessary but not sufficient** for world-class AEC documentation; the AEC documentation plane (sheet sets, drawing standards, dimensions, annotation, schedules, specifications, ISO 19650 delivery) sits ON TOP of that pub/print substrate and composes it. Every owner is graded against both telos planes at once: a chart owner is graded as a journal-grade publication primitive AND as a drawing-sheet figure source; a typography owner is graded as a book-typesetting engine AND as an ISO 3098 drawing-annotation engine. The capability bar is the union, never the lesser plane.

Capability coverage is the sole density measure. The prior "~450 forced-LOC look" is **dropped** — design-doc code fences are as large as the owned concern requires (doctrine: "coverage over size", fences are "3-4x denser than ordinary code"). A small page modeling a rich concept is under-built; a large well-collapsed page can still be capability-sparse. Grade owners against their full domain + both `.api` tiers, never against a line budget.

## [02]-[CATEGORICAL_BEST_MANDATE]

- **Categorical-best per concern, zero overlaps.** Each bounded concern (PDF read/render/create/forms, PSD author, vector boolean/offset, color/CMYK/separations, descriptive metadata, font merge/freeze/shape, diagram layout/routing, subtitle model, DXF) is owned by exactly one categorical-best package. Where a newer/stronger package supersedes an older admitted one, the older is **replaced, not stacked beside** — superseded packages are flagged per-page during rebuild and removed in the final `pyproject` reconciliation.
- **Replace-older, not accrete.** A second package admitted for a concern an existing package already owns is a defect unless it is strictly stronger on a distinct axis (then the weaker is removed). No two packages own the same concern.
- **Mine to full capability.** Every admitted package is internalized into its canonical owner at the deepest primitive it reaches; a thin one-shot use of a deep package is surface sprawl. Stack BOTH `.api` tiers: the shared/universal `libs/python/.api/*.md` rails (expression/msgspec/pydantic/pydantic-settings/beartype/structlog/stamina/anyio/opentelemetry/numpy) layered ON TOP OF the folder-specific `libs/python/artifacts/.api/*.md` domain packages — never the folder set alone.

## [03]-[PACKAGE_ROSTER]

Admit ALL of the following (all copyleft/OSS/free — paid/gated/proprietary rejected: no aspose, no Pantone-licensed data). One `.api` stub per new pip package already created under `libs/python/artifacts/.api/`. License/gating decisions are locked; the resolver (`uv lock`) is the ground truth for cp315-wheel availability — gate `; python_version<'3.15'` only where no cp315 wheel exists, and remove the gate when one lands.

**PDF** — `pdf_oxide` (MIT/Apache, Rust abi3->3.15; extract+render+create+forms). Rationalize against the existing `pdfplumber`/`pypdf`/`pypdfium2`/`pymupdf` stack with parity evidence per concern; PyMuPDF (AGPL) stays. Superseded older PDF arms flagged per-page for final removal.

**PSD/layered raster** — `PhotoshopAPI` (BSD; native PSD/PSB writer; gate `python_version<'3.15'` or source-build), `psd-tools` (MIT, abi3+pure-py; read/inspect + pixel author), `imagecodecs` (BSD, abi3 incl 3.15; PackBits/ZIP channel codec). These own `export/layered` PSD/PSB; the existing `psdtags`+`tifffile` layered-TIFF arm is retained only for the TIFF container, PSD authority moves to PhotoshopAPI/psd-tools.

**Vector** — `skia-pathops` (BSD, abi3->3.15; boolean/offset/stroke-to-outline). Compose existing `shapely` harder for planar set ops (gate cp315 only if the resolver shows no cp315 wheel). Owns `graphic/vector` boolean/offset/stroke-to-outline.

**Color/print** — `colour-cxf` (BSD; CxF3 spot/spectral exchange). Deepen existing `colour-science` (CAM16/spectral/CMYK conversion) + `coloraide` (interpolation/gamut). DEVICE-LINK / named-color / N-channel / ICC proof transforms route through Pillow `ImageCms` (the lcms2/Little-CMS surface Pillow already bundles), NOT a standalone binding: `littlecms` was rejected — the 0.1b3 binding imports the removed `imp` module and cannot run on Python 3.12+. TAC/separations/PDF-X PREFLIGHT routes through the veraPDF (JRE) + ghostscript subprocess oracles in [03] system tools, NOT a Python preflight library: `lint-pdf` was rejected — it is a FastAPI/Celery/Redis/Postgres service (drags `psycopg2-binary`, no cp315 wheel), overlaps veraPDF, and adds no capability the categorical-best set lacks. These complete the CMYK/separations/PDF-X plane.

**Metadata** — `pyexiftool` (BSD wrapper + exiftool binary; richest cross-format read/write; **REPLACES the abandoned `exif`**), `puremagic` (MIT pure-py; default format sniffer — promote over `python-magic` for the default path, libmagic retained only where it is strictly stronger), `pyexiv2` (GPL acceptable; optional in-process EXIF/IPTC/XMP). Keep `c2pa-python`/`pyhanko`/`pikepdf`/`python-xmp-toolkit`/`iptcinfo3`.

**Typography** — `opentype-feature-freezer` (Apache pure-py; freeze/RIBBI), `vharfbuzz` (MIT pure-py; shaping QA/buffer-diff), `PyICU` (MIT; gate cp315; locale line-break/bidi/collation). Tier-1 gains are free: `fonttools` `merge`/`STAT` and explicit `uharfbuzz` script/lang/direction in `typography/shape`+`typography/font`.

**Diagramming** — `pyelk` (EPL pure-py; ELK layered+orthogonal+ports+nesting — **vendor + pin** via git source, not a plain PyPI name if unavailable), `fast-sugiyama` (MIT Rust/PyO3; replaces `grandalf` placement, keep `grandalf` fallback until parity proven; verify cp315), `kiwisolver` (BSD; **promote** the transitive dep to direct; Cassowary constraint layout), `ziafont` (MIT; text->SVG-path), `ziamath` (MIT; math->SVG), `schemdraw` (MIT; native-SVG schematic), `drawpyo` (MIT; draw.io/diagrams.net export). `grandalf` flagged for removal once `fast-sugiyama` parity lands.

**AEC/CAD** — `ezdxf` (DXF read/write/render) — owns the new `export/dxf` page.

**Media** — `pysubs2` (MIT pure-py; subtitle parse/convert/retime/restyle model). `soundfile` REJECTED (redundant with PyAV; cffi 2.0 has no cp315 wheel). PyAV stays and is source-built (see [05]).

**System tools (parameterized subprocess boundaries; discovery-env -> configured-path -> bundled-fallback resolution order)** — `ghostscript`/`exiftool`/`qpdf`/`poppler` (machine-provided), `veraPDF` (JRE; PDF/A+PDF/UA validation oracle), `libavoid-server` (C++; orthogonal connector routing for diagramming). Each is a reproducible parameterized boundary, local for now; actual JRE/C++ provisioning is a Forge follow-up, never a design-page blocker.

**REMOVALS** — `exif` dropped up front (replaced by `pyexiftool`). All other supersession removals (`grandalf` once fast-sugiyama parity lands; PDF/magic/PSD overlaps once parity proven) are flagged per-page during rebuild and applied in the final `pyproject` reconciliation — never blindly up front.

## [04]-[NEW_STRUCTURE]

Full structural authority: split/add/remove pages, new folders (minimum 2 files each), move/tear-apart, remove superseded functionality. Discovery owns the final page set so per-file work never collides; new pages are authored by their owning agent; merged/removed pages are deleted after their content is absorbed. Update `core/receipt`, `core/plan`, `ARCHITECTURE.md` `[01]-[DOMAIN_MAP]`+`[02]-[SEAMS]`, and `README.md` `[01]-[ROUTER]`+`[02]-[DOMAIN_PACKAGES]` for every structural and roster change.

**NEW top-level AEC-documentation-plane domains** (mostly OWNED ISO/NCS vocabularies + geometry, not packages):

`drawing/` — the drawing-production plane:
- `standard.md` — ISO 128 line types/weights, hatch patterns, ISO 5455 drawing scales, the ISO 13567 + NCS/AIA layer-name codec, discipline/sheet-type/status codes, ISO 3098 text heights, dimension-style families. The shared owned-vocabulary substrate the other drawing pages compose.
- `dimension.md` — ISO 129-1 linear/aligned/angular/radial/diameter/ordinate/chain/baseline dimensioning + tolerance authoring, emitted onto the drawing as positioned annotation geometry.
- `annotate.md` — ISO 128-2 leaders/keynotes/flag-notes/text annotation + revision clouds.
- `symbol.md` — section/elevation/detail markers, grid bubbles, match lines, in-field north arrow + graphic scale, revision triangle.
- `detail.md` — detail callouts + a content-keyed detail library + cross-references between sheets.
- `schedule.md` — AEC schedule templates + legends, lowering into `visualization/table`.

`specification/` — the specification plane:
- `section.md` — CSI SectionFormat 3-part (General/Products/Execution) + PageFormat, authored INTO the `document/model` `DocumentNode` tree.
- `classify.md` — MasterFormat/UniFormat/OmniClass classification tables + a drawing<->spec cross-reference resolver.

`delivery/` — the ISO 19650 delivery plane:
- `register.md` — ISO 19650 drawing register / sheet index / container metadata / suitability + revision codes.
- `transmittal.md` — transmittal records + plan-set assembly close, composing `composition/imposition` + `package/archive` + `exchange/credential|conformance`.

**MEDIA restructure** (`media/`, 7 pages, replacing `video.md`+`audio.md`; delete `video.md` after content absorbs into `container.md`; existing media quality is the FLOOR, substantially expand):
- `container.md` — rebuilt from `video.md`: the container spine family, mux/demux capsule, read+write, BSF remux, hardware VideoToolbox, HDR/color, HLS/DASH/fMP4 via `io_open`.
- `filtergraph.md` — NEW: a closed `FilterNode` owner over the verified filter vocabulary (the capability-detection routing core in [05]).
- `audio.md` — rebuilt: decode/encode/resample/layout/mix, composing `filtergraph`.
- `timeline.md` — NEW: Trim/Concat/Segment/xfade non-linear editing over the container/filtergraph spine.
- `subtitle.md` — NEW: `pysubs2` parse/convert/retime/restyle + passthrough-mux + RGBA-overlay burn-in.
- `analysis.md` — NEW: waveform / spectrogram (composing `libs/python/compute` scipy.signal) / loudness (ebur128) / silence / scene-detection / thumbnail.
- `synthesis.md` — NEW: numpy/scipy waveform generation -> encode.

**EXTENSIONS** (existing pages grown in place, not new domains):
- `composition/sheet.md` — SheetSet/register, `Viewport` with scale, the exact ISO 5457 Table 2 field counts, ISO 7200 mandatory-field audit, sheet-number assembly.
- `export/dxf.md` — NEW page under `export/`: `ezdxf` DXF read/write/render.
- `visualization/table.md` — consume schedule/QTO facts from the C# `Rasm.Bim` graph via the `data/tabular` seam (the QTO/schedule rows arrive as a tabular frame; the table owner renders, it never re-authors the IFC model).

## [05]-[MEDIA_CAPABILITY_DETECTION]

PyAV in-process only — **no ffmpeg CLI subprocess for media** (the system-tool subprocess boundaries in [03] are for PDF/metadata/preflight, not media). The media design is correct whether PyAV is a limited wheel or source-built against `ffmpeg-full`.

**Capability-detection filter routing.** At the boundary, probe the linked ffmpeg's available filter set through PyAV's filter registry; route each logical media operation to its native filter when the build exposes it, else to a verified in-process substitute. The `FilterNode` owner (`filtergraph.md`) carries one closed family of logical operations, each with a native-filter arm and a substitute arm selected by the runtime probe — never two parallel pages, never a hardcoded assumption that a filter exists.

Absent-filter -> verified-substitute map:
- text burn-in (`drawtext`, needs libfreetype) -> render text to an RGBA buffer via `typography` + `graphic`, composite via `overlay` filter or numpy alpha-composite.
- subtitle burn-in (`subtitles`/`ass`, needs libass) -> render subtitle frames via `pysubs2` + the text path, RGBA-overlay composite.
- color grade (`eq`: brightness/contrast/gamma/saturation) -> `colorbalance` + `curves`.
- denoise (`hqdn3d`/`atadenoise` absent) -> `nlmeans`.

**Provision a source-built PyAV** against the machine `ffmpeg-full` (`pip install av --no-binary av`) to unlock libass/libfreetype/native filters; the capability-detection design is the floor regardless of which build lands.

## [06]-[AEC_OWNED_VOCABULARIES]

The AEC documentation plane is built from OWNED standards vocabularies + geometry, not from packages. These are closed-family owners (StrEnum/Literal/tagged-union/frozendict tables), authored to the exact published cardinalities, never approximated:
- **ISO 128** — line types and line-weight groups; **ISO 128-2** — leaders/annotation conventions.
- **ISO 129-1** — linear/aligned/angular/radial/diameter/ordinate/chain/baseline dimensioning + tolerancing.
- **ISO 5455** — drawing scales (reduction/enlargement/full ratios).
- **ISO 5457** — drawing sheet sizes, frame, and the Table 2 field set (exact field counts are load-bearing).
- **ISO 7200** — title-block mandatory + optional data fields (the mandatory-field audit set is load-bearing).
- **ISO 13567** — CAD layer-name structure; composed with **NCS** and **AIA CAD Layer Guidelines** discipline/major/minor + status codes into one layer-name codec.
- **ISO 3098** — lettering / text heights for drawing annotation.
- **NCS** discipline / sheet-type / status code sets.
- **CSI MasterFormat / UniFormat / OmniClass** — specification + classification tables, with the drawing<->spec cross-reference resolver.
- **ISO 19650** — information-container metadata, suitability codes, revision codes, drawing register / sheet index structure.

## [07]-[SEAM_UNIFICATION]

The entry/receipt seam is the campaign's structural correctness target — today it is fragmented; unify it:
- **One production entry** — `core/plan` `ArtifactPipeline` is the single content-keyed production entrypoint every domain (existing eleven + the three new AEC domains + the seven media pages) routes through. No parallel per-domain entry, no second pipeline owner.
- **One receipt family** — `core/receipt` `ArtifactReceipt` is the single kind-discriminated tagged union; every producer contributes exactly one case through the runtime `ReceiptContributor` port. The new AEC domains contribute new `ArtifactReceipt` cases (drawing/schedule/spec/delivery evidence) AS CASES on the existing family, never parallel receipt rails. The seven media pages all contribute the existing `ArtifactReceipt.Media` case (or a single extended media case), never seven sibling receipt shapes.
- **One color source / one managed egress** — `graphic/color/derive` stays the one upstream palette source; `graphic/color/managed` the one ICC/LUT/CCTF egress, now extended for CMYK/separations/PDF-X via `colour-cxf` (CxF3 exchange) + Pillow `ImageCms` (lcms2 device-link/proof) + `colour-science` (CMYK/spectral math), with veraPDF + ghostscript as the preflight oracles.
- New seams (drawing->document, drawing->composition/sheet, specification->document/model, delivery->composition/imposition+package/archive+exchange, visualization/table<-csharp:Rasm.Bim, export/dxf->composition) are recorded in `ARCHITECTURE.md` `[02]-[SEAMS]` as aligned wire seams, never as coupling to a sibling owner's interior.

## [08]-[SPECIFIC_WF_FOCUS_SETS]

The parameterized build-out workflow runs three times; each run's FOCUS files get the granular per-file treatment (implement -> critique -> redteam per file), while every OTHER folder page gets a general cold pass (per-file implement + batched critique/redteam) so the whole folder improves on every run.

1. **AEC documentation plane** — focus: `drawing/{standard,dimension,annotate,symbol,detail,schedule}` (6), `specification/{section,classify}` (2), `delivery/{register,transmittal}` (2), `export/dxf` (1); plus in-place extension of `composition/sheet` and `visualization/table`.
2. **Temporal media** — focus: `media/{container,filtergraph,audio,timeline,subtitle,analysis,synthesis}` (7).
3. **Visual authoring & diagramming** — focus: `visualization/diagram/{layout,draw,glyphset}` and new diagram pages (general node-link / ER / flowchart / Sankey / section-callout) over `pyelk`/orthogonal-routing(`libavoid`)/`kiwisolver`/`ziafont`/`ziamath`/`schemdraw`/`drawpyo`; `graphic/vector` (`skia-pathops` boolean/offset/stroke-to-outline; fix the `EdgeRoute.ORTHOGONAL` -> `route_with_lines` bug); `graphic/marks/*`; `export/layered` (`PhotoshopAPI`/`psd-tools`/`imagecodecs` PSD); typography orchestration (`typography/shape` font-fallback/script-itemization/vertical/kashida/text-on-path; `typography/font` merge/STAT/freeze).

Remaining depth (color/CMYK/separations/PDF-X, metadata/provenance, PDF/`pdf_oxide`, chart grammar, AcroForm/forms, entry/receipt-seam unification) is owned by the general-WF capstone deep pass plus the per-specific-WF general cold passes.

## [09]-[OUT_OF_SCOPE_CSHARP_BOUNDARY]

These are C#-side concerns; the Python `artifacts` corpus composes them at the wire, never re-authors them:
- **IFC authoring** — `Rasm.Bim` is the sole IFC semantic authority (the `BimModel`/`BimWire`/`ElementSet`/`IfcSemanticModel` graph). `visualization/table` consumes QTO/schedule rows from it via `data/tabular`; it never re-implements IFC.
- **BCF semantics** — lean `Rasm.Bim`; Python may serialize `.bcfzip` only if later split out, not now.
- **GD&T feature-control-frames** — `Rasm.Fabrication`.
- **3D/mesh/columnar interchange** — `geometry/mesh` and the runtime columnar lane; `scene/export` holds the boundary (scene-file serialization, not raw mesh codec).
- **Speckle** — out of scope.
