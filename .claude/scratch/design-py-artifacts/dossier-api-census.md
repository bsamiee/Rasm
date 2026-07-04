# DOSSIER — lane: api-census (RASM-PY-ARTIFACTS)

Scope: both `.api` tiers (shared `libs/python/.api/` + artifacts `libs/python/artifacts/.api/`, 92 folder catalogs), root `pyproject.toml` artifacts rows, the `[PYPROJECT_RECONCILIATION]` eight-row dead-marker census, every `[04]-[PACKAGE_PRESSURE]` load-bearing member claim, E13, and every roster-closure row. Stance: hostile, disk-first. Verdicts: **HOLD** (anchor exact) / **DRIFT** (real defect, anchor moved) / **REFUTED** (disk falsifies).

Headline: the catalog corpus is HIGH-FIDELITY — every `.api:LINE` anchor the brief cites resolves exactly, every roster-closure member exists (zero phantoms across ~62 catalogs), all three absence claims (weasyprint/vtk/pyvista/pymupdf) confirm. The register is an accurate pointer, NOT inflated. **But one register premise is falsified on the repo's own target interpreter, and one concurrency-collapse census undercounts its blast radius by an order of magnitude.**

---

## [1] REFUTED — E13(b)/V16(b): `frozendict` IS a CPython 3.15 builtin; the imports do NOT raise

The single most consequential finding in this lane. E13 and V16(b) rest on the premise: *"`from builtins import frozendict` (59 import sites across 54 pages — `frozendict` is not a CPython builtin; every import raises) ... the data-E4 defect at artifacts scale."* **This premise is false on `requires-python = ">=3.15"`.**

On-disk proof (the repo's own target interpreter, `uv run python` == CPython **3.15.0b3**):
- `from builtins import frozendict` **SUCCEEDS** — returns `<class 'frozendict'>`, `__module__ == 'builtins'`, `__qualname__ == 'frozendict'`.
- Under `python -I -S` (isolated, no site, no `sitecustomize`/`.pth`) it STILL succeeds — rules out any environment injection. `'frozendict' in vars(builtins)` is `True`, sitting beside `frozenset`.
- MRO is `(frozendict, object)` (inherits directly from `object`, NOT a `dict` subclass); immutable (`TypeError: 'frozendict' object does not support item assignment`); hashable; the PyPI `frozendict` package is NOT installed (`uv pip show frozendict` → not found). It is a genuine interpreter builtin.

Spec corroboration: **PEP 814 "Add frozendict built-in type"** (Victor Stinner, Donghee Na — Status **Final**, Python-Version **3.15**, Resolution **2026-02-11**) and *What's New in Python 3.15* both state: *"A new immutable type, `frozendict`, is added to the `builtins` module ... it inherits directly from `object`."* (PEP 810 "Explicit lazy imports" also landed in 3.15 — the corpus's pervasive `lazy import <pkg>` form is 3.15-native and correct, not pseudo-code.)

Consequence — the DECISION MUST re-rule V16(b), not restate it:
- The **action** (migrate `Final[frozendict[...]]` → `Final[Map[...]]`; `rg` zero for `from builtins import frozendict`) can still stand, but ONLY on `[04]` SHARED-TIER-LAW grounds — the `expression.Map` (`Map.of_seq`) unified ADT/dispatch spine the runtime/data/compute campaigns forced on every sibling. It is a **consistency mandate**, not a bug fix.
- The **stated rationale is wrong**: a page writing `Final[frozendict[K, V]]` on 3.15 gets a real, working, immutable, hashable builtin table — nothing raises. V16(b)'s prescribed deleted-form prose (*"names the anti-pattern by the `Map`-vs-frozendict contrast"*) must be re-grounded as **"mutable-adjacent builtin table vs the `expression.Map` ADT-integrated rail (dispatch/fold/`Result` interop)"**, NEVER "broken import vs working import." The acceptance `rg` gate survives only under the re-grounded Map-consistency justification.
- Cross-track flag (out of artifacts lane, record only): the sibling runtime/data/compute campaigns cite the same "data-E4 defect / not a builtin" premise. Their Map migration is the right rail for the wrong stated reason; the artifacts DECISION should not inherit the falsified justification.

Counts, by contrast, all HOLD exactly on disk (clean-flag recount): 59 `from builtins import frozendict` sites across 54 pages; 69 `content_identity` spellings / 45 full `rasm.runtime.content_identity` paths across 44 pages; 54 `[RESEARCH]`-bearing pages. The *numbers* are precise; only the *premise attached to them* is refuted.

---

## [2] DRIFT — E13/[04]: the limiter/stamina collapse census names 3, the corpus carries 43 + 55

Anchors HOLD exactly: `composition/compose.md:153` `_GATE: CapacityLimiter = CapacityLimiter(4)`, `:159` `stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)`; `imposition.md:150` and `sheet.md:151` each `CapacityLimiter(4)`. But the `[04]` phrase *"the three folder-minted `CapacityLimiter(4)`s ... zero artifacts-minted limiters"* is a COVERAGE-thin-slice: the three literal-`4` composition mints are an **exemplar**, not the census. Corpus-wide there are **43 `CapacityLimiter(` mints** (the 3 literal-`4` + ~40 `CapacityLimiter(os.process_cpu_count() or N)` forms at `core/plan.md:84`, `delivery/register.md:170`, `drawing/{annotate:69,detail:62,dimension:74,schedule:86,symbol:58}`, and ~33 more) and **55 bare `stamina.*` refs**. The "zero artifacts-minted limiters" obligation (collapse onto the runtime `lanes.offload` isolation-modality axis) is correct in intent and correctly scoped by the *"and every direct `to_thread`/`to_process`"* clause, but a build agent reading "three" fixes only `composition/`. The DECISION must scope the limiter/retry collapse to the full 43-mint + 55-stamina surface, distinguishing the egregious literal-`4` mints from the `process_cpu_count()` pools (both collapse, per "zero artifacts-minted limiters").

---

## [3] Dead-marker census — all 8 rows HOLD

Every artifacts-tagged row carries `python_version<'3.15'` under `requires-python = ">=3.15"` (a never-installs marker). Census is COMPLETE and accurate — no artifacts row missed, none over-counted:

| Row | Anchor | Verdict | Resolution class (disk-confirmed) |
|---|---|---|---|
| `scikit-image` | pyproject:113 | HOLD | wheel-lag; disk comment is MORE specific than brief: *"Cython codegen fails on 3.15 (no cp315 wheel)"* (physically under `[SCIENTIFIC]`, `[ARTIFACTS]`-tagged) |
| `pyvista` | pyproject:190 | HOLD | pure-Python wheel, gated only through `vtk` |
| `vtk` | pyproject:201 | HOLD | wheel-lag (cp314 stands) |
| `usd-core` | pyproject:239 | HOLD | wheel-lag |
| `lets-plot` | pyproject:234 | HOLD | wheel-lag |
| `pyexiv2` | pyproject:223 | HOLD | structurally-dead + copyleft: `GPL-3.0-only` confirmed `.api/pyexiv2.md:13` |
| `PhotoshopAPI` | pyproject:243 | HOLD | structurally-dead: *"wheels-only, no cp315 wheel/sdist"* — no sdist confirmed |
| `PyICU` | pyproject:250 | HOLD | structurally-dead: *"sdist-only, needs ICU native"* |

Minor drift note: the brief frames the four wheel-lag rows uniformly as *"cp314 wheels stand, cp315 pending the interpreter's final release"* (auto-resolving). `scikit-image`'s disk comment ("Cython codegen fails on 3.15") is an active codegen failure, potentially NOT auto-resolving on final release. Marker verdict (never-installs under `>=3.15`) HOLDS regardless. The V8/V9/V10/V14 by-name blocker carries are self-consistent (brief lines 122/126/130/146).

---

## [4] `[04]` load-bearing member anchors — all HOLD, zero phantoms

Every cited `.api:LINE` resolves to the claimed member on disk. This catalog set is precise; the register's pointers are trustworthy.

| Claim | Anchor(s) | Verdict |
|---|---|---|
| pikepdf read-side `is_separation`/`is_device_n`; NO Separation/DeviceN authoring | `.api/pikepdf.md:124` (+ `:149,:166` reject authoring) | HOLD — `.is_device_n`/`.is_separation` on `PdfImage` color surface, read-only |
| pikepdf `mediabox`/`trimbox`/`bleedbox` page boxes | `.api/pikepdf.md:111,149` | HOLD (`Page.mediabox/cropbox/trimbox/bleedbox/artbox`) |
| pikepdf `add_field` (documented, uninvoked) | `.api/pikepdf.md:30,135` | HOLD (`AcroForm.add_field`, `Pdf.acroform.add_field`) |
| ocrmypdf `run_hocr_pipeline`→`run_hocr_to_ocr_pdf_pipeline` two-phase; `pdfinfo.PdfInfo`/`PageInfo.has_text`; `pdfa.speculative_pdfa_conversion`+`file_claims_pdfa` | `.api/ocrmypdf.md:117-127,134-152` | HOLD exactly (`:125,:126,:138-140,:142,:143`) |
| rustworkx `TopologicalSorter`/`transitive_reduction`/`ancestors`/`descendants`/`PyDAG`/`DAGWouldCycle` | `.api/rustworkx.md:98-111,152-154` | HOLD (`:104-111,:134,:152-153`) |
| tifffile GeoTIFF/`aszarr`/`memmap`/`TiffSequence`/`validate_jhove` | `.api/tifffile.md:17,81-85` | HOLD (validate_jhove `:84`, TiffSequence `:81`; minor: `memmap :79`/`aszarr :78` sit just above the 81-85 window but all named at `:17`) |
| pyphen `DataInt.data` `(change,index,cut)` growth seam; no emit arm applies it | `.api/pyphen.md:79` | HOLD verbatim |
| altair typed `theme.ThemeConfig`/`*Kwds`, `@theme.register` factory | `.api/altair.md:36,120` | HOLD; statistical/geo members (`transform_regression`/`loess`/`density`/`mark_geoshape`/`topo_feature`) present |
| vl-convert `register_font_directory`, `get_local_tz`+`format_locale`/`time_format_locale`, `svg_to_png/jpeg/pdf` | `.api/vl-convert-python.md:88-97,104` | HOLD (`:88,:90-91,:97,:104`) |
| drawpyo `load_diagram`→`get_by_id`→`File.write` round-trip; `PageSize` ISO; TOML-validated style vocabs | `.api/drawpyo.md:169-176,187,120-142` | HOLD |
| opentype-feature-freezer `FREEZE`/`RemapByOTL` arm | `.api/opentype-feature-freezer.md:96-99` | HOLD |
| iptcinfo3 SUPERSEDED, zero live `metadata.py` consumer | `.api/iptcinfo3.md:75` | HOLD verbatim |

Absence claims (catalog-authorship-part-of-verdict) — all confirmed absent:
- `.api/weasyprint.md`: zero hits for `string-set`/`target-counter`/`margin-box`/`@page`/running content → V12 catalog authorship needed. HOLD.
- `.api/vtk.md` + `.api/pyvista.md`: zero hits for `Silhouette`/`FeatureEdges`/`GL2PS` → E11/V9 catalog authorship needed. HOLD.
- `.api/pymupdf.md`: `mediabox` present (`:227` `DocumentWriter.begin_page(mediabox)`), `trimbox`/`bleedbox`/`cropbox` absent → V7 pymupdf-side catalog authorship needed. HOLD.

---

## [5] Roster-closure table — all rows HOLD, zero phantoms

Phantom sweep across every roster-closure catalog: every cited member appears (hit counts all non-zero). No orphan admission is a removal candidate — each has a live integration TARGET (section [7]).

typst `Compiler`/`compile_with_warnings`/`query`/`eval` (23); python-docx `Document`/`add_*`/`Styles` (137); odfpy `OpenDocument`/`load`/`save`/`Element` (183); ruamel-yaml `CommentedMap`/`CommentedSeq`/`YAML` (62); tomlkit `TOMLDocument`/`dumps`/`parse` (72); msoffcrypto `OfficeFile`/`load_key`/`OOXMLFile` (36); python-calamine `CalamineWorkbook`/`to_python`/`iter_rows` (26); openpyxl `load_workbook`/`styles` (27); xlsxwriter `constant_memory` (20); jinja2 `SandboxedEnvironment` (4); papermill `execute_notebook` (12); nbclient `NotebookClient`+`CellExecutionError`/`CellTimeoutError`/`DeadKernelError` (40); nbconvert `get_exporter`/`PDFExporter` (37); jupytext `read`/`write` (41); matplotlib `Figure`/`colormaps`/`ColormapRegistry` (49); vegafusion `pre_transform_spec`/`pre_transform_extract` (15); lxml `_Element`/`XPath`/`XSLT`/`iterparse`/`XMLSyntaxError` (90); segno `QRCode`/`make_micro` (37); python-barcode `get_barcode_class` (10); zxing-cpp `read_barcodes`/`create_barcode` (45); scikit-image `AffineTransform`/`CircleModel` + thirteen submodules (`:3`); blackrenderer `BlackRendererFont`/`PaintFormat`/`Surface` (48); pdfimpose `AbstractImpositor`/`impose`/`Margins` (46); psdtags `TiffImageSourceData`/`PsdLayers`/`PsdChannel`/`fromtiff` (35); imagecodecs `*_encode`/`*_decode`/`available` (69); c2pa `Builder`/`get_validation_state`/`C2paSigningAlg` (46); av `InputContainer`/`OutputContainer`/`filter.Graph`/`from_ndarray` (47); pysubs2 `SSAFile`/`SSAEvent`/`make_time` (90); py7zr `writeall`/`extractall`/`test` (14); stream-zip `MemberFile`/`Method` (32); stream-unzip AES/`stream_unzip` (99); detools `create_patch`/`apply_patch`/`patch_info` (76); colour-cxf `read_cxf`/`write_cxf`/`cxf3.CxF` (30); great-tables `tab_options`/`write_raw_html`/`with_locale` (7); kiwisolver `Solver`/`Variable`/`Constraint` (129); coloraide `mix`/`harmony`/`contrast`/`mask` (52); colour-science `write_LUT`/`SpectralShape`/`CAM16` (10); ezdxf `path`/`paperspace`/`DIMSTYLE`/`ATTRIB` (10); pyicu `Transliterator`/`StringSearch`/`BidiTransform` (25); uniseg `words`/`sentences`/`line_break` (39); pyelk `layoutOptions` (9); simpleidml `IDMLPackage` (88); schemdraw `Kmap`/`Timing`/`BitField` (17); drawsvg `Pattern`/`Drawing`/`Group`/`Raw` (46).

License census (the removal-default survival conditions): pyexiv2 `GPL-3.0-only` (`.api/pyexiv2.md:13`); grandalf `GPLv2 | EPLv1` dual + Status **SUPERSEDED-PENDING-REMOVAL** *"until fast-sugiyama parity lands"* (`.api/grandalf.md:12,163`); fast-sugiyama MIT + `_fast_sugiyama.abi3.so` *"runs unmodified on cp315, NO cp-gate"* (`.api/fast-sugiyama.md:12,14`); pdf-oxide `MIT OR Apache-2.0` + `cp38-abi3` *"forward-compatible across all CPython including cp315"* (`.api/pdf-oxide.md:13-15`). All HOLD.

pvlib admission (additive, V15 solar): correctly ABSENT from `pyproject.toml` and NO `.api/pvlib.md` on either tier — this is the admission the DECISION MAKES, not a landed row. HOLD (pending-admission state exactly as brief frames it). Feed facts (BSD-3, NREL, pure-Python wheel, `solarposition` family) are the admission's proof burden; `.api/pvlib.md` is authored WITH the verdict.

---

## [6] Cross-cutting findings

**pyexiv2 in-process arm is LIVE and is the exact form the brief defaults to removal.** `exchange/metadata.md:46` does `lazy import pyexiv2` (*"OPTIONAL in-process Exiv2 unified arm; cp315-absent → the exiftool arm stands"*). This is GPL-3.0 composed **in-process** — precisely the copyleft-in-process posture `[PYPROJECT_RECONCILIATION]` says defaults to REMOVAL, surviving only *"re-sited behind the process boundary with consumed capability `pyexiftool` cannot reach."* The catalog frames it as *"the GPLv3 classifier the brief admits as acceptable for the OPTIONAL metadata path"* (`.api/pyexiv2.md:13`) — a direct tension with the brief's removal-default. The DECISION must rule: either (a) re-site the pyexiv2 arm out-of-process behind the boundary (`pyexiftool` already owns the out-of-process carrier), or (b) drop the in-process arm and stand on `pyexiftool` alone. The census substantiates the removal-default's premise: `iptcinfo3`/`python-xmp-toolkit`(`libxmp`) are PROSE-MENTIONED in `metadata.md` but NOT imported (only `pyexiv2` is imported at `:46`) — **zero live consumers confirmed** for both removal-default raster-metadata packages.

**zlib-ng data-strike re-entry is well-founded (integration, not removal).** `zlib-ng` has THREE live artifacts consumers (`package/archive`, `package/codec`, `scene/export`) — the artifacts compression band IS its natural home per the brief. But disk state is pre-reconciliation: `pyproject.toml:40` still tags it `[DATA]` and NO `libs/python/artifacts/.api/zlib-ng.md` exists (only shared-tier `libs/python/.api/zlib-ng.md`). Re-entry obligation for the DECISION: re-tag `[DATA]`→`[ARTIFACTS]`, confirm the shared-tier catalog serves (or author the folder overlay), and land the `package/codec` consuming fence beside the live `lz4`/`brotli`/`zstandard` band. The shared catalog exists, so the re-entry composes a real owner, never a hand-roll.

**puremagic-default / python-magic-fallback confirmed** (`pyproject.toml:222,193`): puremagic in 2 pages (`exchange/detect`, `graphic/raster/io`), python-magic in 1 (`exchange/detect`, fallback). HOLD.

**Shared-tier stacking present where load-bearing:** 39/92 artifacts catalogs explicitly reference `libs/python/.api/*` (the substantive folds — pyphen, rustworkx, opentype-feature-freezer all stack `expression`/`msgspec`/`beartype`/`numpy`/`anyio`/`stamina`/`structlog`). The `[04]` "both tiers stack; folder tier alone is a defect" law is honored in the folds that matter; pure-vocabulary leaf catalogs correctly don't force it. No cross-tier stacking defect found in this lane.

**APPROACH-naivety (enumerated-roster-where-generator-belongs) is NOT an `.api`-tier defect** — it lives in the design pages (`_HATCH` eleven-name table, terminator renderers, etc., E4/V2/V5), out of the api-census lane. The catalogs themselves are generative-shaped (e.g. tifffile's closed `IntEnum` vocabularies, rustworkx's polymorphic bare-name dispatch, drawpyo's TOML-keyed bounded vocabularies) and correctly document the generator surfaces the pages fail to compose.

---

## [7] Integration homes for orphan-looking admissions (TARGETS, never removals)

Every admission the census flags as zero/one-consumer is a MISSING-OWNER gap the page-set closes. Disk-substantiated consumer counts across `.planning`:

| Admission | Live consumers now | Integration TARGET (the missing owner) |
|---|---|---|
| `schemdraw` | 1 (`drawing/symbol` — only the `Segment*`/`ElementCompound` geometry spine, `symbol.md:5`) | NEW `visualization/diagram/schematic.md` owns the 226-element catalog (Kmap/Timing/BitField), V10 |
| `drawsvg.Pattern` | 0 (the `export/dxf` "Pattern" hit is ezdxf, not drawsvg) | V2 pattern-plane SVG lowering (`graphic/vector/` or `graphic/pattern.md`) — `drawsvg` broadly consumed (8 pages), only `.Pattern` unmined |
| `tifffile` | 1 (`export/layered` sole importer) | `graphic/raster/io` gains GeoTIFF tags + `aszarr`/`memmap`/`TiffSequence` lazy-tiled access; `validate_jhove` joins `exchange/conformance` oracle family (V8) |
| `drawpyo` round-trip | 2 (`diagram/draw`, `diagram/glyphset` — `.drawio` template round-trip `load_diagram` unconsumed) | `visualization/diagram/draw` `.drawio` arm composes `load_diagram`→`get_by_id`→`File.write` |
| `ocrmypdf` in-package owners | 1 (`document/lens` composes only top-level `ocr()`) | `document/lens` pre-flight (`pdfinfo.PdfInfo.has_text` skip-OCR) + PDF/A egress (`pdfa.speculative_pdfa_conversion`/`file_claims_pdfa` → veraPDF oracle), V12 |
| `vl-convert` `register_font_directory` | 1 ref (`visualization/chart/export`, font-identity loop unwired) | wire the fonttools/uharfbuzz font-identity loop so chart glyphs match document shaped text |
| `rustworkx` DAG surface | `drawing/detail` composes it; `delivery/register`/`transmittal`/`specification/classify` order assembly with ZERO graph imports | those three cross-reference owners fold `PyDiGraph` through `transitive_reduction`/`TopologicalSorter`/`ancestors`/`descendants` (`.api/rustworkx.md:146`) |
| `pvlib` | 0 (not yet admitted) | NEW V15 solar-ephemeris owner; `.api/pvlib.md` authored at admission |
| `pyexiv2` | 1 (`exchange/metadata:46`, in-process — the rejected form) | re-site out-of-process behind the process boundary, OR drop for the `pyexiftool` carrier ([6]) |
| `zlib-ng` | 3 (`package/archive`/`codec`, `scene/export`) | `package/codec` compression band beside `lz4`/`brotli`/`zstandard`; re-tag `[DATA]`→`[ARTIFACTS]` ([6]) |

---

## [8] Charter deltas the DECISION must carry (api-census scope)

1. **Re-rule V16(b)** on `expression.Map` shared-tier-consistency grounds; strike the "not a builtin / every import raises" rationale as REFUTED by CPython 3.15 (PEP 814); re-ground the deleted-form prose as the `Map`-ADT-vs-builtin-table contrast. The `rg`-zero acceptance gate stands only under the re-grounded justification.
2. **Scope the limiter/retry collapse** to the full 43 `CapacityLimiter(` mints + 55 `stamina.*` refs corpus-wide (the three literal-`4` composition mints are the exemplar, not the census) onto the runtime `lanes.offload`/`guarded` axis.
3. **Rule the pyexiv2 in-process tension** at `metadata.md:46` (re-site behind boundary or drop); **rule the zlib-ng re-entry** (`[DATA]`→`[ARTIFACTS]` re-tag + `package/codec` fence + catalog siting).
4. **Author the three absent catalog surfaces WITH the verdict** — weasyprint `@page`/`string-set`/`target-counter` (V12), vtk/pyvista `Silhouette`/`FeatureEdges`/`GL2PS` (V9/E11), pymupdf `trimbox`/`bleedbox` (V7), pikepdf Separation/DeviceN authoring surface (V4) — all confirmed absent, all catalog-authorship-is-part-of-verdict.
5. **Admit pvlib** with `.api/pvlib.md` (`solarposition` member set) in the same motion as the V15 solar owner; the owned closed-form kernel stands as fallback if declined.

Every other register row assigned to this lane HOLDS with the anchor exact.
