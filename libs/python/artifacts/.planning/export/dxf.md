# [PY_ARTIFACTS_DXF]

The CAD-exchange editable hand-off owner authoring, salvaging, rendering, querying, and geometry-bridging DXF documents. `Dxf` is ONE owner over the closed-payload `DxfOp` `expression.tagged_union` — `New` authors a fresh document from a `DxfDocument` spec, `Read` ingests a conforming DXF, `Recover` salvages a damaged one, `Render` lowers a DXF figure into the composition/graphic plane over seven in-process backends, `Query` extracts an attribute/spatial sub-selection, and `Bridge` crosses the DXF↔SVG↔GeoJSON↔glyph geometry wire — each case its own typed payload, never a `StrEnum` keyed against an erased `dict[str, object]`, dispatched by one total `match` and folded ONCE into a `DxfComposed` evidence struct the `of`/`contribute` projections share. `ezdxf` (pure-Python `py3-none-any`, no cp-gate on any interpreter) is the sole categorical-best owner of the DXF R12→R2018 read/write/recover/render surface: this owner composes `ezdxf.new`, the polymorphic `readfile`/`read`/`readzip`/`decode_base64` ingestion family, `recover.read`/`readfile` salvage, the `GraphicsFactory.add_*` builder family under the uniform `GfxAttribs` attribute axis, the `xref.attach` external-reference surface, the `ezdxf.path.Path` command-segment algebra, the `ezdxf.addons.drawing` `Frontend`+backend render stack, the `doc.query`/`groupby`/`select` spatial/`bbox` read side, and the `addons.geo`/`addons.text2path` boundary surfaces — it re-implements no DXF tag grammar, no OCS/WCS transform, no B-spline evaluator, no entity-to-SVG conversion, and it never re-authors the IFC semantic model (`csharp:Rasm.Bim` holds that) nor the sheet placement/scale the `composition/sheet#SHEET` owner holds nor the SVG framing the `graphic/vector#VECTOR` owner holds.

The rail is the branch `RuntimeRail[ContentKey] = Result[ContentKey, BoundaryFault]` the `runtime/faults#FAULTS` owner legislates, minted ONCE at `async_boundary(f"dxf.{op.tag}", self._keyed, catch=_FAULTS)` over the real engine raise tuple `_FAULTS = (DXFError, RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)` — `DXFError` (the `ezdxf` error base, `Exception`-derived and NOT a stdlib subclass, so it is named explicitly exactly as the sibling `composition/imposition#IMPOSE` names `PdfImposeUserError`) admits `DXFStructureError`/`DXFVersionError`/`DXFValueError` from a malformed conforming read or a bad builder call, `RuntimeError` admits the `PyMuPdfBackend`/`pymupdf`/`matplotlib` native-render raise, `ValueError` a `matplotlib`/`msgspec.json`/EQL or a malformed entity-query raise, `KeyError` a `_DIM`/`_SPATIAL_TEST` table miss, `OSError` an engine font/resource/stream fault, and `BeartypeCallHintViolation` a non-positive render `dpi`/`scale`/flatten `distance` the `_GUARD`-contracted `_admit` scalar seam rejects — each discriminated into its own `BoundaryFault` case rather than the `Exception` catch-all the faults owner rejects. A transient `OSError` (a font/resource/stream load crossing the disk boundary) is re-armed by the `stamina.retry` aspect wrapping the worker seam BEFORE the boundary converts a persistent one, the cancellation class excluded from the retry set. The Auditor evidence is NOT a parallel `DxfFault` `Literal` the boundary never reads (the deleted illusion the sibling `composition/sheet#SHEET` `[RAIL_SETTLED]` warns against): a salvageable damaged file produces bytes and its `auditor.errors`/`auditor.fixes` counts ride the `Cad` receipt as evidence, a truly-unparseable input raises `DXFStructureError` the boundary converts, so `Recover` never double-rails and the salvage cleanliness is honest receipt evidence exactly as the `algorithms.md` `SolveReceipt` carries its residual. Cancellation is excluded from `_FAULTS` and re-raises as the structured signal.

`DxfEntity` is the closed-payload build vocabulary each drawable carries — one `expression.tagged_union` over `Line`/`Arc`/`Circle`/`Ellipse`/`Spline`/`LwPolyline`/`Hatch`/`Text`/`Mtext`/`Leader`/`Dimension`/`BlockRef`/`Point`/`Mesh`, every case bundling a shared `DxfAttribs` value object projected to the UNIFORM `dxfattribs=GfxAttribs(...).asdict()` payload the WHOLE `add_*` family takes, so layer/color/linetype application is one axis across the vocabulary, never a per-entity `set_layer`/`set_color` setter, and the `Dimension` case's `DimKind` sub-axis routes the seven ISO 129-1 dimension kinds through the `_DIM` `frozendict` to the matching `add_*_dim` builder. `TableEntry` is the closed symbol-table row family — `Layer`/`Linetype`/`Textstyle`/`Dimstyle` folded onto `doc.layers`/`doc.linetypes`/`doc.styles`/`doc.dimstyles` `.add(...)` — the ezdxf-shaped substrate the future `drawing/standard` AEC vocabularies (ISO 128 line types → `Linetype`, ISO 3098 text heights → `Textstyle`, ISO 129-1 styles → `Dimstyle`, the ISO 13567 layer codec → `Layer`) LOWER their computed rows onto; dxf owns the table-authoring shape, the ISO semantics stay their own owners. `Xref` is the external-reference row `xref.attach` binds so a `DxfDocument` composes an external drawing without copying its geometry. `DxfSource` is the closed polymorphic ingestion — `Blob`/`File`/`Zip`/`Base64` folding the `ezdxf.read`/`readfile`/`readzip`/`decode_base64` conforming family and the `recover.read`/`readfile` salvage family, never a per-source reader class. `Spatial` is the closed spatial-query family (`Window`/`Circle`/`Polygon`/`Fence`/`Point`) the `_SPATIAL_TEST` table routes to the rtree-backed `select.*` predicate.

`ezdxf` resolves on the runtime (pure-Python authoring, but the `PyMuPdfBackend`/`matplotlib` render backends are GIL-releasing native code), so every op defers `ezdxf` through a module-scope `lazy` import and the `_composed` fold offloads off the event loop through a `CapacityLimiter`-bounded `to_thread.run_sync` — `to_thread` is the lane for the WHOLE fold because the fold returns the `msgspec`-backed `DxfComposed` owner the subinterpreter `to_interpreter` arm cannot load the C-extension for, and the GIL-releasing PyMuPDF/Matplotlib render shares the address space with zero serialization, so a per-arm `to_interpreter` split is impossible rather than merely suboptimal (the sibling `composition/sheet#SHEET`/`graphic/vector#VECTOR` chooser). The `Render` arm lowers a rendered DXF figure into `composition/sheet#SHEET` (the `PyMuPdfBackend.get_pdf_bytes` one-page PDF the `show_pdf_page` seam places) and `graphic/vector#VECTOR` (the `SVGBackend.get_string` SVG the placement owner composites), adds `MatplotlibBackend` EPS/PS publication-vector output and `CustomJSONBackend`/`GeoJSONBackend` structured-geometry export; the `Bridge` arm crosses the `graphic/vector#VECTOR` `svg` egress at the vertex/`d`-string wire, `ezdxf.path.make_path(entity).flattening(distance)` → `Vec3.list` → `numpy.asarray` per drawable folded into the vector owner's `svg` framing, and the `addons.geo` GeoJSON wire and `addons.text2path` glyph-outline wire in the inverse. Every operation returns a `RuntimeRail[ContentKey]` and contributes ONE `core/receipt#RECEIPT` `ArtifactReceipt.Cad` case off the one `DxfComposed` fold — the `dxfversion`, the units, the artifact format, the `Counter(dxftype)` entity-census map, the `doc.layers`/`doc.blocks` roster counts, the `Auditor` error+fix counts, the `bbox.extents` model AABB, and the output byte length — the receipt owner's named flat-scalar mint, never a second render and never a parallel DXF-receipt rail.

## [01]-[INDEX]

- [01]-[DXF]: the CAD-exchange owner over the closed-payload `DxfOp` `tagged_union` (`New`/`Read`/`Recover`/`Render`/`Query`/`Bridge`) folded once into the `DxfComposed` evidence struct the `of`/`contribute` projections share, rail-typed `RuntimeRail[ContentKey]` over `async_boundary(catch=_FAULTS)`, offloaded through the `_GATE`-bounded `to_thread` band under a `stamina.retry` transient-`OSError` aspect; the `DxfEntity` closed build vocabulary under the uniform `DxfAttribs`→`GfxAttribs.asdict()` axis with the `DimKind`-keyed `_DIM` dimension family, the `TableEntry` symbol-table row family the `drawing/standard` ISO vocabularies lower onto, the `Xref` external-reference row, the `DxfSource` polymorphic ingestion over `ezdxf.read`/`readfile`/`readzip`/`decode_base64` + `recover.read`/`readfile`, the `Spatial` closed spatial-query family over the `_SPATIAL_TEST`-keyed `select.*` rtree predicates, the `ezdxf.path` `make_path`/`flattening`/`control_vertices`/`from_vertices`/`render_lines` + `addons.geo`/`addons.text2path` bridge at the `graphic/vector#VECTOR` wire, the `ezdxf.addons.drawing` `Frontend`+`SVGBackend`/`PyMuPdfBackend`/`MatplotlibBackend`/`CustomJSONBackend`/`GeoJSONBackend` render stack lowering into `composition/sheet#SHEET` and `graphic/vector#VECTOR`, the `doc.query`/`groupby`/`select`/`bbox` read side, and the `@_GUARD`-contracted `_admit` scalar seam; contributes ONE `core/receipt#RECEIPT` `ArtifactReceipt.Cad` case and one `core/plan#PLAN` `ArtifactWork` node.

## [02]-[DXF]

- Owner: `Dxf` the one CAD-exchange owner holding `op: DxfOp` and discriminating operation over the closed `DxfOp` `expression.tagged_union` whose every case carries its own typed payload, every arm folded ONCE into the `DxfComposed` evidence struct (`data` bytes, `kind` the `DxfArtifact` format discriminant, plus the `dxfversion`/`units`/`counts`/`layers`/`blocks`/`errors`/`fixes`/`extent` CAD evidence) the `of`/`contribute` projections share — no second render and no `@cache` memo standing in for the one fold. `DxfDocument` is the New-arm authoring spec (version/units/setup + the `TableEntry` symbol-table rows + the `Xref` external references + the `BlockDef` reusable-block definitions + the modelspace `DxfEntity` drawables + the `DxfFormat` egress encoding), admitting the whole document graph as one owner the `_authored` fold lowers onto `ezdxf.new` + the builder family. `DxfEntity` is the closed drawable vocabulary each modelspace/block entity carries, bundling a shared `DxfAttribs` value object whose `.gfx()` projects the ONE `GfxAttribs(layer=, color=, rgb=, linetype=, lineweight=, transparency=, ltscale=).asdict()` payload the WHOLE `add_*` family takes, so a layer/color/linetype change is one `DxfAttribs` field, never a per-entity setter; the `Dimension` case's `DimKind` sub-axis (`LINEAR`/`ALIGNED`/`ANGULAR`/`RADIUS`/`DIAMETER`/`ORDINATE`/`ARC`) keys the `_DIM` `frozendict` to the matching `add_*_dim` builder whose returned `DimStyleOverride.render()` generates the dimension geometry. `TableEntry` is the closed `Layer`/`Linetype`/`Textstyle`/`Dimstyle` symbol-table row family the `_table_entry` fold lowers onto `doc.layers`/`doc.linetypes`/`doc.styles`/`doc.dimstyles` `.add(...)`, the ezdxf-shaped substrate the `drawing/standard` ISO 128/3098/129-1/13567 vocabularies compute their rows into — a new AEC standard lowers as `TableEntry` rows, never a re-implemented DXF table writer. `Xref` is the external-reference value object (`block_name`/`filename`/`insert`/`scale`/`rotation`) the `_attach_xref` fold binds through `xref.attach`, so a `DxfDocument` composes an external drawing by reference. `DxfSource` is the closed `Blob`/`File`/`Zip`/`Base64` polymorphic ingestion the `_ingest` conforming fold routes to `ezdxf.read`/`readfile`/`readzip`/`decode_base64` and the `_recovered` salvage fold routes to `recover.read`/`readfile`, never a per-source parser. `PageSpec` is the render page model (`width`/`height` at zero auto-detecting from extents, `margin`, `dpi`, `fit_page`, `scale`, `dark`) projecting the `ezdxf.addons.drawing.layout.Page`/`Settings` and `config.Configuration(background_policy=)` render policy; `Selection` is the read-side query spec (the `doc.query` EQL string plus the optional `Spatial` spatial refinement). `Spatial` is the closed spatial-query family (`Window`/`Circle`/`Polygon`/`Fence`/`Point`) the `_spatial` fold routes to the rtree-backed `select.Window`/`Circle`/`Polygon` shape under a `SpatialTest` (`INSIDE`/`OUTSIDE`/`OVERLAP`) `_SPATIAL_TEST` row or the `select.bbox_crosses_fence`/`point_in_bbox` predicate, never a `get_by_layer`/`find` family. `ezdxf` owns `new`/`readfile`/`read`/`readzip`/`decode_base64`/`recover.read`/`readfile`/`saveas`/`write`/`encode_base64`/`audit`/`modelspace`/`blocks`/`layers`/`linetypes`/`styles`/`dimstyles`/`add_entity`, the `add_*` builder family + `GfxAttribs`/`colors.RGB`, `xref.attach`, `path.make_path`/`flattening`/`control_vertices`/`from_vertices`/`render_lines`, `math.Vec3`/`Matrix44`/`BoundingBox`, the `Frontend`/`RenderContext`/`Configuration`/`SVGBackend`/`PyMuPdfBackend`/`MatplotlibBackend`/`CustomJSONBackend`/`GeoJSONBackend` render stack, `query`/`groupby`/`select`/`bbox.extents`, and the `addons.geo`/`addons.text2path`/`addons.Importer` boundary surfaces; no DXF grammar, affine, spline evaluator, or entity-to-SVG conversion is re-implemented.
- Cases: `DxfOp` cases — `New(document)` (author a fresh DXF from a `DxfDocument` spec — `ezdxf.new(version, setup=, units=)` mints the document, `_table_entry` folds each `TableEntry` onto its symbol table, `_attach_xref` binds each `Xref`, `doc.blocks.new(name)` + `_build_entity` populates each `BlockDef`, `_build_entity` folds every modelspace `DxfEntity` onto `msp.add_*` with the uniform `DxfAttribs.gfx()` payload, `doc.audit()` structurally validates before egress, and `_serialize` encodes to the `DxfFormat` bytes — the Auditor error/fix counts riding the `Cad` receipt) · `Read(source)` (ingest a CONFORMING DXF — the `DxfSource` folds through `_ingest` to `ezdxf.readfile`/`read`/`readzip`/`decode_base64`, `doc.audit()` normalizes, `_serialize` re-emits the audit-clean canonical bytes, a malformed source raising the `DXFStructureError` the `_FAULTS` tuple admits) · `Recover(source)` (SALVAGE a damaged/non-conforming DXF — `recover.readfile`/`read` (the ONLY correct loader for non-conforming DXF) returns `(doc, auditor)`, `_serialize` emits the salvaged bytes, the `auditor.errors`/`auditor.fixes` counts riding the `Cad` receipt as honest salvage evidence, a truly-unparseable input raising the `DXFStructureError` the boundary converts — never a parallel `DxfFault.recovered` `Literal` the boundary double-rails) · `Render(source, backend, page)` (lower a DXF figure into the composition/graphic plane — `Frontend(RenderContext(doc), backend, Configuration(...)).draw_layout(msp, finalize=True)` walks the layout resolving every entity's layer/linetype/lineweight/color into pen properties, and the `DxfBackend` selects the sink: `SVGBackend.get_string(page, settings)` for the SVG `graphic/vector#VECTOR` composites, `PyMuPdfBackend.get_pdf_bytes(page, settings=)` for the one-page PDF `composition/sheet#SHEET` places, `PyMuPdfBackend.get_pixmap_bytes(page, dpi=)` for the raster preview, `MatplotlibBackend` + `Figure.savefig(fmt="eps"/"ps")` for the publication-vector EPS/PS the pub plane needs, `CustomJSONBackend`/`GeoJSONBackend.get_string()` for the structured geometry export — no foreign renderer) · `Query(source, selection)` (extract an attribute/spatial sub-selection — `doc.query(selection.eql)` runs the DXF entity-query language, the optional `_spatial` fold refines it through the rtree-backed `select.Window`/`Circle`/`Polygon` shape under a `SpatialTest` or the `bbox_crosses_fence`/`point_in_bbox` predicate, `addons.Importer` copies the matched entities into a fresh document `_serialize` emits, the `Counter(dxftype)` census and `bbox.extents` riding the receipt — never a `find`/`get_by_layer`/`filter` family) · `Bridge(spec)` (cross the DXF↔SVG↔GeoJSON↔glyph geometry wire — `ToSvg` folds `path.make_path(entity)` over the path-convertible drawables, `flattening(distance)` or `control_vertices()` sampling each into a `Vec3` sequence `numpy.asarray` records as the shared `(N, 3)` lane and `_polyline` emits as one `<path>` the `graphic/vector#VECTOR` `svg` owner frames once; `FromSvg` crosses `svgelements.Path` vertex rings through `path.from_vertices` + `path.render_lines` onto a fresh modelspace; `ToGeoJson`/`FromGeoJson` cross the `addons.geo.GeoProxy` georeferenced wire; `TextPaths` crosses `addons.text2path.make_paths_from_str` glyph outlines to DXF — the two path owners meeting at the vertex/`d`-string wire, neither re-implementing the other's geometry) — matched by one total `match`/`case` lowering to the one `DxfComposed` fold; never a per-version reader sibling, never a per-entity `_emit` method, never a per-backend render family.
- Auto: `_composed(op) -> DxfComposed` is the ONE total `match` over `DxfOp` both `of` and `contribute` read — no second render: the `New` arm calls `_authored(document)` (the `_table_entry`/`_attach_xref`/block/`_build_entity` fold + `doc.audit()`) then `_serialize`, the `Read` arm `_ingest`s and re-serializes the audit-clean form, the `Recover` arm `_recovered`s and reads the auditor counts, the `Render` arm `_admit`s the `dpi`/`scale`/`margin` scalars then drives `_rendered` over the seven-member `DxfBackend`, the `Query` arm folds `doc.query`/`_spatial`/`Importer` into a matched sub-document, and the `Bridge` arm crosses the five geometry wires — each arm reading `doc.dxfversion`, the `_counts` `Counter(dxftype)` census, the `_extent` `bbox.extents` AABB, and the `doc.layers`/`doc.blocks` roster into `DxfComposed`, any ezdxf/backend failure propagating as the provider raise the boundary admits. `_build_entity(layout, entity)` is the total `match` over `DxfEntity` folding each onto its `add_*` builder with the shared `attribs.gfx()` payload, the `Dimension` arm keying `_DIM[kind]` to the matching `add_*_dim` and calling `.render()` on the returned override; `_table_entry(doc, entry)` is the total `match` lowering each `TableEntry` onto its `doc.<table>.add`. The ezdxf `Drawing` is pure-Python and GC-safe (no native handle to bracket, unlike the sibling `pymupdf` documents), so `_serialize` writes it through one `io.StringIO`/`io.BytesIO`/`encode_base64` egress; the render backends open their own native `pymupdf` documents (or a GC-safe `matplotlib.figure.Figure`) internally, which the backend owns and closes. Each arm returns `DxfComposed(data, kind, dxfversion, units, counts, layers, blocks, errors, fixes, extent)`, so the body stays one `match`-shaped path — never an inline `try`/`except` ladder beside it, never a `@cache` memo standing in for the one fold, never a second `match` re-rendering for the receipt.
- Bridge: the `graphic/vector#VECTOR` geometry seam is FIVE crossings on one `BridgeSpec` family, none re-implementing the other owner's geometry — `ToSvg` folds `path.make_path(entity)` over the `_PATH_TYPES`-filtered drawables (a TOTAL `dxftype` predicate, so no per-element `TypeError` from a `Text`/`Insert` drives the loop), `Path.flattening(distance)` adaptively sampling each curve or `Path.control_vertices()` reading the exact NURBS frame keyed by `BridgeSample`, `Vec3.list(verts)` round-tripping to tuples `numpy.asarray(...)` records as one `(N, 3)` array the offset/simplify/hull numeric lane crosses, `_polyline` emitting each as one `<path d="M…">` and the whole fragment stream framed ONCE by the `graphic/vector#VECTOR` `svg(fragments, viewbox)` owner (imported one hop, never a hand-rolled `<svg>` and never the last-entity-only fold the naive form kept); `FromSvg` folds each `svgelements.Path` vertex ring through `path.from_vertices(ring)` into a `Path` and `path.render_lines(msp, paths)` back onto a fresh modelspace, so a `skia-pathops` boolean or an `svgelements.Path` transform crosses INTO DXF and a DXF NURBS curve crosses OUT at the shared `numpy` vertex array; `ToGeoJson`/`FromGeoJson` cross the `addons.geo.GeoProxy` georeferenced wire (`geo.proxy(entities).__geo_interface__` out over the `_GEO_TYPES`-filtered convertibles, `GeoProxy.parse(mapping).to_dxf_entities()` in), ezdxf holding only the DXF↔GeoJSON conversion and the geospatial owners the CRS authority; `TextPaths` crosses `addons.text2path.make_paths_from_str(text, FontFace(family=), size, m=Matrix44.translate(*insert))` glyph outlines (composing `fonttools`, never re-shaping) to DXF through the same `render_lines` egress.
- Receipt: each operation contributes ONE `core/receipt#RECEIPT` `ArtifactReceipt.Cad(key, dxfversion, units, artifact, bytes, layers, blocks, errors, fixes, counts)` off the one `DxfComposed` fold through the receipt owner's named flat-scalar mint — the `dxfversion` string, the `units` name, the `artifact` format (`"dxf"` for a DXF-producing arm, `"svg"`/`"pdf"`/`"png"`/`"eps"`/`"ps"`/`"json"`/`"geojson"` for a render or geometry-export arm, from `composed.kind`), the output byte count, the `doc.layers`/`doc.blocks` roster counts, the `Auditor` error+fix counts (zero for the always-clean `New`/`Read` normalize, non-zero for a salvaged `Recover`), and the `Counter(dxftype)` entity-census `frozendict` the receipt owner's `cad` arm flattens into per-type facts beside the scalar summary. `contribute` reads the SAME `_composed(op)` the `of` projection reads, mints the content key over `composed.data` through the same `ContentIdentity.of` `_keyed` uses, and yields `ArtifactReceipt.Cad(...).contribute()` — the receipt owner's `Cad` mint being the closed family's own constructor exactly as `Ok`/`Some` are `Result`/`Option`'s, the flat positional shape the `_facts` `cad` arm projects, never a per-kind `CadFacts` `Struct` re-wrapping the scalars the mint already takes and never a phantom `ArtifactReceipt.of(key, facts)` the receipt owner rejects. A failed production raises the ezdxf/backend fault the `async_boundary` converts to a `BoundaryFault` (the receipt owner's own `rejected` line projecting it) rather than a zero-byte placeholder hand-built in the projection.
- Growth: a new DXF version is one `DxfVersion` member — never a per-version reader; a new drawable is one `DxfEntity` case plus one `_build_entity` arm (the `assert_never` tail breaking the fold at type-check until the arm exists) — never a per-entity `_emit`; a new dimension kind is one `DimKind` member plus one `_DIM` row; a new symbol-table row (a `View`/`UCS`/`AppId`) is one `TableEntry` case plus one `_table_entry` arm; a new ingestion source is one `DxfSource` case plus one `_ingest`/`_recovered` arm; a new render backend or format is one `DxfBackend`+`DxfArtifact` member plus one `_rendered` arm — never a parallel renderer; a new egress encoding (a zipped bundle, the `r12writer` streaming fast-writer for millions of R12 entities) is one `DxfFormat` member plus one `_serialize` arm, the collapsed "Emit" axis a policy value not a parallel op; a new spatial refinement (a `select` bounding-shape, a `PlanarSearchIndex` reuse) is one `Spatial` case plus one `_spatial` arm; a new bridge direction (a DXF↔DXF `xref` binding, a `addons.geo` CRS transform, a `MTextEditor` fluent-content stream) is one `BridgeSpec` case plus one arm over the existing `ezdxf.addons` surface; a new query refinement (a `groupby` key function, the `EntityQuery.union`/`difference`/`intersection` set-algebra combining two selections) is one `Selection` field; a new receipt scalar is one slot on the `Cad` case tuple plus one `_facts` field; a new engine raise is one type in the `_FAULTS` module tuple. Zero new surface.
- Boundary: a hand-assembled DXF tag stream where the `add_*` builder family + `GfxAttribs` exist; a per-entity `set_layer`/`set_color` setter where the uniform `dxfattribs=` axis applies; a re-implemented affine/B-spline/OCS transform where `ezdxf.math` owns it; a hand-rolled `<svg>` wrapper or a per-entity-overwriting fold where the `graphic/vector#VECTOR` `svg` owner frames the whole `make_path`/`flattening` fragment stream; a re-parsed SVG `d`-string where `from_vertices`/`render_lines` bridge the geometry; a foreign DXF renderer where the `Frontend`+backend family renders; a `find`/`get_by_layer`/`filter` query family where `doc.query`/`groupby`/`select` discriminate; a per-element `try`/`except make_path` where the `_PATH_TYPES`/`_GEO_TYPES` total predicate filters; the conforming `readfile` on a damaged file where `recover.readfile` is the correct salvage path; a parallel `DxfFault` `Literal` the boundary never reads where the Auditor evidence rides the `Cad` receipt and the hard raise crosses `_FAULTS`; a per-version reader class, a per-backend render method, a per-kind receipt `Struct`, an inline `try`/`except` ladder beside the fold, a second full render for the receipt, and a `@cache` memo standing in for the one fold are the deleted forms. `ezdxf` owns DXF read/write/recover, the graphic-entity vocabulary, the `Path` geometry algebra, the `ezdxf.math` kernel, and the DXF render frontend; the IFC semantic model stays `csharp:Rasm.Bim`; the AEC standards vocabularies (ISO 128/129-1/3098/13567) stay the future `drawing/standard` owned-vocabulary owners that LOWER onto the `TableEntry` rows; SVG framing shared with the figure rail meets `graphic/vector#VECTOR` at the `svg`/`d`-string wire; PDF page assembly and sheet placement stay `composition/sheet#SHEET`/`composition/imposition#IMPOSE`; font shaping stays `typography` (`text2path` composes `fonttools`, it does not re-shape); the geospatial CRS authority stays the geospatial owner (`addons.geo` holds only the DXF↔GeoJSON conversion); identity minting the runtime owns.
- Packages: `ezdxf` (`new`/`readfile`/`read`/`readzip`/`decode_base64`/`recover`/`audit`/`write`/`encode_base64`; the `add_*` builder family + `GfxAttribs`/`colors.RGB`; `xref.attach`; `path.make_path`/`flattening`/`control_vertices`/`from_vertices`/`render_lines`; `math.Vec3`/`Matrix44`; `addons.drawing` `Frontend`/`RenderContext`/`Configuration`/`SVGBackend`/`PyMuPdfBackend`/`MatplotlibBackend`/`CustomJSONBackend`/`GeoJSONBackend`/`layout.Page`/`Settings`/`Margins`; `query`/`groupby`/`select.Window`/`Circle`/`Polygon`/`bbox_inside`/`bbox_outside`/`bbox_overlap`/`bbox_crosses_fence`/`point_in_bbox`/`bbox.extents`; `addons.Importer`/`addons.geo.GeoProxy`/`addons.text2path.make_paths_from_str`/`fonts.FontFace`); `matplotlib` (`figure.Figure` + `savefig` the EPS/PS publication-vector sink the `MatplotlibBackend` renders into, GC-safe with no `pyplot` global); `graphic/vector#VECTOR` (`svg` the SVG-framing egress the `ToSvg` bridge composes one hop); `numpy` (the `Vec3.list` → `asarray` `(N, 3)` vertex lane the flatten result crosses); `expression` (`tagged_union`/`tag`/`case` the `DxfOp`/`DxfEntity`/`TableEntry`/`DxfSource`/`BridgeSpec`/`Spatial` unions); `msgspec` (`Struct(frozen=True)` the value objects, `json.encode`/`decode` the GeoJSON wire); `beartype` (the `_GUARD` scalar contract over `_admit`); `anyio` (the `_GATE`-bounded `to_thread` render offload); `stamina` (the transient-`OSError` retry aspect over the worker seam); `builtins.frozendict` (the `_DIM`/`_UNITS`/`_SPATIAL_TEST` policy tables); `collections.Counter` (the dxftype census); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`); `core/receipt#RECEIPT` (`ArtifactReceipt.Cad`).

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import base64
import io
import zipfile
from collections import Counter
from collections.abc import Callable, Iterable, Iterator
from enum import IntEnum, StrEnum
from math import tau
from pathlib import Path
from typing import TYPE_CHECKING, Annotated, Literal, assert_never

import msgspec
import numpy as np
import stamina
from anyio import CapacityLimiter, to_thread
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from builtins import frozendict
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.graphic.vector import svg as svg_frame  # the graphic/vector#VECTOR SVG-framing egress the ToSvg bridge composes one hop

# `DXFError` is the `ezdxf` error base, `Exception`-derived (NOT a stdlib subclass), so it is named
# eagerly for the `_FAULTS` tuple exactly as the sibling `composition/imposition#IMPOSE` names
# `PdfImposeUserError`; the heavy `ezdxf` surface stays lazy behind the module boundary.
from ezdxf import DXFError

lazy import ezdxf
lazy from ezdxf import bbox, colors, recover, select, xref
lazy from ezdxf import path as dxfpath
lazy from ezdxf.addons import Importer
lazy from ezdxf.addons import geo as dxfgeo
lazy from ezdxf.addons import text2path
lazy from ezdxf.addons.drawing import Frontend, RenderContext
lazy from ezdxf.addons.drawing import config as dwgconfig
lazy from ezdxf.addons.drawing import layout as dwglayout
lazy from ezdxf.addons.drawing.json import CustomJSONBackend, GeoJSONBackend
lazy from ezdxf.addons.drawing.matplotlib import MatplotlibBackend
lazy from ezdxf.addons.drawing.pymupdf import PyMuPdfBackend
lazy from ezdxf.addons.drawing.svg import SVGBackend
lazy from ezdxf.fonts.fonts import FontFace
lazy from ezdxf.gfxattribs import GfxAttribs
lazy from ezdxf.math import Matrix44, Vec3
lazy from matplotlib.figure import Figure

if TYPE_CHECKING:
    from ezdxf.document import Drawing
    from ezdxf.entities import DXFGraphic
    from ezdxf.layouts import BlockLayout, Modelspace
    from ezdxf.query import EntityQuery

    from rasm.runtime.receipts import Receipt


# --- [TYPES] ----------------------------------------------------------------------------
type Point2 = tuple[float, float]
type Point3 = tuple[float, float, float]
type Extent = tuple[float, float, float, float, float, float]  # (min x, y, z, max x, y, z) bbox AABB
type Attribs = dict[str, object]  # the `GfxAttribs.asdict()` `dxfattribs=` payload
type Builder = Callable[["Modelspace | BlockLayout", tuple[Point3, ...], str, Attribs], object]  # a `_DIM` dimension builder
type Positive = Annotated[float, Is[lambda value: value > 0.0]]
type NonNegative = Annotated[float, Is[lambda value: value >= 0.0]]
type PositiveInt = Annotated[int, Is[lambda value: value > 0]]

# real ezdxf/backend raise tuple: `DXFError` admits `DXFStructureError`/`DXFVersionError`/`DXFValueError`
# (malformed read, bad builder call), `RuntimeError` the `PyMuPdfBackend`/`pymupdf`/`matplotlib` native render,
# `ValueError` a `matplotlib`/`msgspec.json`/EQL raise, `KeyError` a `_DIM`/`_SPATIAL_TEST` miss, `OSError` a
# font/resource/stream fault, and `BeartypeCallHintViolation` the `_GUARD`-contracted `_admit` non-positive reject.
_FAULTS: tuple[type[BaseException], ...] = (DXFError, RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)
# the native-offload bounded slot: the GIL-releasing PyMuPDF/Matplotlib render crosses one `to_thread` band
# off the event loop; forced for the whole fold because it returns the `msgspec`-backed `DxfComposed` the
# `to_interpreter` arm cannot load the C-extension for, so the shared-address thread is the only lane.
_GATE: CapacityLimiter = CapacityLimiter(4)


class DxfVersion(StrEnum):  # the `ezdxf.new(dxfversion=)` target; a new version is one member, never a reader subtype
    R12 = "AC1009"
    R2000 = "AC1015"
    R2004 = "AC1018"
    R2007 = "AC1021"
    R2010 = "AC1024"
    R2013 = "AC1027"
    R2018 = "AC1032"


class DxfUnits(IntEnum):  # the `ezdxf.units` InsertUnits `doc.units` value (drawing insert-units)
    UNITLESS = 0
    INCH = 1
    FOOT = 2
    MILLIMETER = 4
    CENTIMETER = 5
    METER = 6
    KILOMETER = 7


class DxfFormat(StrEnum):  # the egress encoding — the collapsed "Emit" axis as a policy value, not a parallel op
    ASC = "asc"  # ascii DXF (`doc.write` text stream)
    BIN = "bin"  # binary DXF (`doc.write(fmt="bin")`)
    BASE64 = "b64"  # `doc.encode_base64()` blob


class DxfBackend(StrEnum):  # the `Render` sink — seven in-process backends, no foreign renderer
    SVG = "svg"  # SVGBackend.get_string — the graphic/vector#VECTOR composite
    PDF = "pdf"  # PyMuPdfBackend.get_pdf_bytes — the composition/sheet#SHEET placement
    PNG = "png"  # PyMuPdfBackend.get_pixmap_bytes — the raster preview
    EPS = "eps"  # MatplotlibBackend + Figure.savefig — the publication-vector output
    PS = "ps"  # MatplotlibBackend + Figure.savefig — the PostScript publication output
    JSON = "json"  # CustomJSONBackend.get_string — the structured geometry export
    GEOJSON = "geojson"  # GeoJSONBackend.get_string — the georeferenced geometry export


class DxfArtifact(StrEnum):  # the `DxfComposed.kind` format discriminant the `Cad` receipt carries
    DXF = "dxf"
    SVG = "svg"
    PDF = "pdf"
    PNG = "png"
    EPS = "eps"
    PS = "ps"
    JSON = "json"
    GEOJSON = "geojson"


class DimKind(StrEnum):  # the ISO 129-1 dimension family the `_DIM` table routes to `add_*_dim`
    LINEAR = "linear"
    ALIGNED = "aligned"
    ANGULAR = "angular"
    RADIUS = "radius"
    DIAMETER = "diameter"
    ORDINATE = "ordinate"
    ARC = "arc"


class SpatialTest(StrEnum):  # the area-shape membership test the `_SPATIAL_TEST` table keys to a `select` predicate
    INSIDE = "inside"  # select.bbox_inside — the shape fully contains the entity bbox
    OUTSIDE = "outside"  # select.bbox_outside — the entity bbox lies fully outside the shape
    OVERLAP = "overlap"  # select.bbox_overlap — the entity bbox intersects the shape


class BridgeSample(StrEnum):  # the `ToSvg` curve-sampling mode over the `ezdxf.path.Path`
    FLATTEN = "flatten"  # Path.flattening(distance) — adaptive polyline for a toolpath/offset consumer
    CONTROL = "control"  # Path.control_vertices() — the exact NURBS control frame


# --- [CONSTANTS] ------------------------------------------------------------------------
_ZERO_EXTENT: Extent = (0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
# the dxftypes `ezdxf.path.make_path` lifts into a `Path` and `addons.geo.proxy` converts to GeoJSON — a TOTAL
# predicate the bridge folds filter on so a non-convertible `Text`/`Insert`/`Dimension` never reaches the raise.
_PATH_TYPES: frozenset[str] = frozenset({"LINE", "ARC", "CIRCLE", "ELLIPSE", "SPLINE", "LWPOLYLINE", "POLYLINE", "HATCH"})
_GEO_TYPES: frozenset[str] = frozenset({"LINE", "ARC", "CIRCLE", "ELLIPSE", "SPLINE", "LWPOLYLINE", "POLYLINE", "POINT"})
_UNITS: frozendict[DxfUnits, str] = frozendict({unit: unit.name.lower() for unit in DxfUnits})
# the ISO 129-1 dimension family -> the matching `add_*_dim` builder; each returns a `DimStyleOverride`
# whose `.render()` generates the geometry, so a new kind is one member plus one row, never a per-kind method.
_DIM: frozendict[DimKind, Builder] = frozendict({
    DimKind.LINEAR: lambda msp, pts, ds, at: msp.add_linear_dim(base=pts[0], p1=pts[1], p2=pts[2], dimstyle=ds, dxfattribs=at),
    DimKind.ALIGNED: lambda msp, pts, ds, at: msp.add_aligned_dim(p1=pts[0], p2=pts[1], distance=pts[2][0], dimstyle=ds, dxfattribs=at),
    DimKind.ANGULAR: lambda msp, pts, ds, at: msp.add_angular_dim_2l(base=pts[0], line1=(pts[1], pts[2]), line2=(pts[3], pts[4]), dimstyle=ds, dxfattribs=at),
    DimKind.RADIUS: lambda msp, pts, ds, at: msp.add_radius_dim(center=pts[0], mpoint=pts[1], dimstyle=ds, dxfattribs=at),
    DimKind.DIAMETER: lambda msp, pts, ds, at: msp.add_diameter_dim(center=pts[0], mpoint=pts[1], dimstyle=ds, dxfattribs=at),
    DimKind.ORDINATE: lambda msp, pts, ds, at: msp.add_ordinate_x_dim(feature_location=pts[0], offset=pts[1], dimstyle=ds, dxfattribs=at),
    DimKind.ARC: lambda msp, pts, ds, at: msp.add_arc_dim_3p(base=pts[0], center=pts[1], p1=pts[2], p2=pts[3], dimstyle=ds, dxfattribs=at),
})
# the `Spatial` area-test row -> the rtree-backed `select` predicate; fence/point cross their own `select`
# functions (no `SelectionShape`), so a new area test is one `SpatialTest` member plus one row.
_SPATIAL_TEST: frozendict[SpatialTest, Callable[[object, "EntityQuery"], "Iterable[DXFGraphic]"]] = frozendict({
    SpatialTest.INSIDE: lambda shape, entities: select.bbox_inside(shape, entities),
    SpatialTest.OUTSIDE: lambda shape, entities: select.bbox_outside(shape, entities),
    SpatialTest.OVERLAP: lambda shape, entities: select.bbox_overlap(shape, entities),
})


# --- [MODELS] ---------------------------------------------------------------------------
class DxfAttribs(Struct, frozen=True):
    # the ONE graphic-attribute value object across the whole `add_*` family; `.gfx()` is the uniform
    # `dxfattribs=` payload, never a per-entity setter. The `drawing/standard` layer/linetype codecs
    # supply `layer`/`linetype` as data; the ISO semantics stay their owner's.
    layer: str = "0"
    color: int = 256  # ACI; 256=ByLayer, 0=ByBlock, 1-255 indexed
    rgb: tuple[int, int, int] | None = None  # true-color; None = ACI `color`
    linetype: str = "ByLayer"
    lineweight: int = -1  # -1=ByLayer, -3=default
    transparency: float | None = None
    ltscale: float = 1.0

    def gfx(self) -> Attribs:
        return GfxAttribs(
            layer=self.layer, color=self.color, rgb=colors.RGB(*self.rgb) if self.rgb is not None else None,
            linetype=self.linetype, lineweight=self.lineweight, transparency=self.transparency, ltscale=self.ltscale,
        ).asdict()


class Xref(Struct, frozen=True):  # an external DXF reference `xref.attach` binds into the authored document
    block_name: str
    filename: str
    insert: Point3 = (0.0, 0.0, 0.0)
    scale: float = 1.0
    rotation: float = 0.0


class PageSpec(Struct, frozen=True):  # the `ezdxf.addons.drawing.layout.Page`/`Settings`/`Configuration` render model
    width: float = 0.0  # 0 auto-detects from extents
    height: float = 0.0
    margin: float = 10.0
    dpi: int = 300
    fit_page: bool = True
    scale: float = 1.0
    dark: bool = False  # `BackgroundPolicy.BLACK` vs `WHITE`, a policy value not a hardcoded literal


class DxfComposed(Struct, frozen=True):  # the one evidence struct of/contribute read — no second render
    data: bytes
    kind: DxfArtifact = DxfArtifact.DXF  # the format the bytes ARE, riding the `Cad` receipt's `artifact` slot
    dxfversion: str = ""
    units: str = ""
    counts: frozendict[str, int] = frozendict()  # `Counter(dxftype)` entity-census map
    layers: int = 0
    blocks: int = 0
    errors: int = 0  # `Auditor.errors` count (salvage residual)
    fixes: int = 0  # `Auditor.fixes` count
    extent: Extent = _ZERO_EXTENT  # `bbox.extents` model AABB


@tagged_union(frozen=True)
class TableEntry:  # the symbol-table row family the `drawing/standard` ISO vocabularies lower onto
    tag: Literal["layer", "linetype", "textstyle", "dimstyle"] = tag()
    layer: tuple[str, int, str, int] = case()  # (name, color, linetype, lineweight) — ISO 13567 layer codec target
    linetype: tuple[str, tuple[float, ...], str] = case()  # (name, dash/gap pattern, description) — ISO 128 target
    textstyle: tuple[str, str, float, float] = case()  # (name, font, height, width) — ISO 3098 lettering target
    dimstyle: tuple[str, frozendict[str, float]] = case()  # (name, dxf-attr overrides) — ISO 129-1 style target

    @staticmethod
    def Layer(name: str, color: int = 7, linetype: str = "Continuous", lineweight: int = -3) -> "TableEntry":
        return TableEntry(layer=(name, color, linetype, lineweight))

    @staticmethod
    def Linetype(name: str, pattern: tuple[float, ...], description: str = "") -> "TableEntry":
        return TableEntry(linetype=(name, pattern, description))

    @staticmethod
    def Textstyle(name: str, font: str = "isocp.shx", height: float = 0.0, width: float = 1.0) -> "TableEntry":
        return TableEntry(textstyle=(name, font, height, width))

    @staticmethod
    def Dimstyle(name: str, overrides: frozendict[str, float] = frozendict()) -> "TableEntry":
        return TableEntry(dimstyle=(name, overrides))


@tagged_union(frozen=True)
class DxfEntity:  # the closed drawable vocabulary; each case bundles the shared `DxfAttribs` uniform axis
    tag: Literal[
        "line", "arc", "circle", "ellipse", "spline", "lwpolyline", "hatch",
        "text", "mtext", "leader", "dimension", "blockref", "point", "mesh",
    ] = tag()
    line: tuple[Point3, Point3, DxfAttribs] = case()  # (start, end, attribs)
    arc: tuple[Point3, float, float, float, DxfAttribs] = case()  # (center, radius, start_angle, end_angle, attribs)
    circle: tuple[Point3, float, DxfAttribs] = case()  # (center, radius, attribs)
    ellipse: tuple[Point3, Point3, float, float, float, DxfAttribs] = case()  # (center, major_axis, ratio, start, end, attribs)
    spline: tuple[tuple[Point3, ...], int, DxfAttribs] = case()  # (fit_points, degree, attribs)
    lwpolyline: tuple[tuple[tuple[float, ...], ...], bool, DxfAttribs] = case()  # (xyseb rows, close, attribs)
    hatch: tuple[tuple[tuple[Point3, ...], ...], int, DxfAttribs] = case()  # (boundary loops, ACI fill color, attribs)
    text: tuple[str, Point3, float, DxfAttribs] = case()  # (text, insert, height, attribs)
    mtext: tuple[str, Point3, float, DxfAttribs] = case()  # (text, insert, char_height, attribs)
    leader: tuple[tuple[Point3, ...], DxfAttribs] = case()  # (vertices, attribs)
    dimension: tuple[DimKind, tuple[Point3, ...], str, DxfAttribs] = case()  # (kind, defpoints, dimstyle, attribs)
    blockref: tuple[str, Point3, float, float, DxfAttribs] = case()  # (block name, insert, scale, rotation, attribs)
    point: tuple[Point3, DxfAttribs] = case()  # (location, attribs)
    mesh: tuple[tuple[Point3, ...], tuple[tuple[int, ...], ...], DxfAttribs] = case()  # (vertices, faces, attribs)

    @staticmethod
    def Line(start: Point3, end: Point3, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(line=(start, end, attribs))

    @staticmethod
    def Arc(center: Point3, radius: float, start_angle: float, end_angle: float, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(arc=(center, radius, start_angle, end_angle, attribs))

    @staticmethod
    def Circle(center: Point3, radius: float, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(circle=(center, radius, attribs))

    @staticmethod
    def Ellipse(center: Point3, major_axis: Point3, ratio: float, start: float = 0.0, end: float = tau, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(ellipse=(center, major_axis, ratio, start, end, attribs))

    @staticmethod
    def Spline(fit_points: tuple[Point3, ...], degree: int = 3, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(spline=(fit_points, degree, attribs))

    @staticmethod
    def LwPolyline(points: tuple[tuple[float, ...], ...], close: bool = False, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(lwpolyline=(points, close, attribs))

    @staticmethod
    def Hatch(loops: tuple[tuple[Point3, ...], ...], color: int = 7, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(hatch=(loops, color, attribs))

    @staticmethod
    def Text(text: str, insert: Point3, height: float = 2.5, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(text=(text, insert, height, attribs))

    @staticmethod
    def MText(text: str, insert: Point3, char_height: float = 2.5, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(mtext=(text, insert, char_height, attribs))

    @staticmethod
    def Leader(vertices: tuple[Point3, ...], attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(leader=(vertices, attribs))

    @staticmethod
    def Dimension(kind: DimKind, defpoints: tuple[Point3, ...], dimstyle: str = "Standard", attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(dimension=(kind, defpoints, dimstyle, attribs))

    @staticmethod
    def BlockRef(name: str, insert: Point3, scale: float = 1.0, rotation: float = 0.0, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(blockref=(name, insert, scale, rotation, attribs))

    @staticmethod
    def Point(location: Point3, attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(point=(location, attribs))

    @staticmethod
    def Mesh(vertices: tuple[Point3, ...], faces: tuple[tuple[int, ...], ...], attribs: DxfAttribs = DxfAttribs()) -> "DxfEntity":
        return DxfEntity(mesh=(vertices, faces, attribs))


class BlockDef(Struct, frozen=True):  # a reusable block definition `add_blockref` places n times
    name: str
    entities: tuple[DxfEntity, ...] = ()


class DxfDocument(Struct, frozen=True):  # the New-arm authoring spec — the whole document graph as one owner
    version: DxfVersion = DxfVersion.R2018
    units: DxfUnits = DxfUnits.MILLIMETER
    setup: bool = True  # load standard linetypes/styles/dimstyles
    tables: tuple[TableEntry, ...] = ()
    xrefs: tuple[Xref, ...] = ()
    blocks: tuple[BlockDef, ...] = ()
    entities: tuple[DxfEntity, ...] = ()
    fmt: DxfFormat = DxfFormat.ASC


@tagged_union(frozen=True)
class DxfSource:  # the polymorphic ingestion the `_ingest` conforming + `_recovered` salvage folds route
    tag: Literal["blob", "file", "zip", "base64"] = tag()
    blob: bytes = case()  # a DXF byte stream -> `ezdxf.read` / `recover.read`
    file: str = case()  # a filesystem path -> `ezdxf.readfile` / `recover.readfile`
    zip: tuple[str, str | None] = case()  # (zip path, member) -> `ezdxf.readzip`
    base64: str = case()  # a base64 DXF blob -> `ezdxf.decode_base64`

    @staticmethod
    def Blob(data: bytes) -> "DxfSource":
        return DxfSource(blob=data)

    @staticmethod
    def File(path: str) -> "DxfSource":
        return DxfSource(file=path)

    @staticmethod
    def Zip(path: str, member: str | None = None) -> "DxfSource":
        return DxfSource(zip=(path, member))

    @staticmethod
    def Base64(text: str) -> "DxfSource":
        return DxfSource(base64=text)


@tagged_union(frozen=True)
class Spatial:  # the rtree-backed spatial-query family the `_spatial` fold routes to `select.*`
    tag: Literal["window", "circle", "polygon", "fence", "point"] = tag()
    window: tuple[Point2, Point2, SpatialTest] = case()  # (corner, corner, area-test) -> select.Window
    circle: tuple[Point2, float, SpatialTest] = case()  # (center, radius, area-test) -> select.Circle
    polygon: tuple[tuple[Point2, ...], SpatialTest] = case()  # (vertices, area-test) -> select.Polygon
    fence: tuple[Point2, ...] = case()  # an open polyline -> select.bbox_crosses_fence
    point: Point2 = case()  # a hit-test point -> select.point_in_bbox

    @staticmethod
    def Window(low: Point2, high: Point2, test: SpatialTest = SpatialTest.INSIDE) -> "Spatial":
        return Spatial(window=(low, high, test))

    @staticmethod
    def Circle(center: Point2, radius: float, test: SpatialTest = SpatialTest.INSIDE) -> "Spatial":
        return Spatial(circle=(center, radius, test))

    @staticmethod
    def Polygon(vertices: tuple[Point2, ...], test: SpatialTest = SpatialTest.OVERLAP) -> "Spatial":
        return Spatial(polygon=(vertices, test))

    @staticmethod
    def Fence(vertices: tuple[Point2, ...]) -> "Spatial":
        return Spatial(fence=vertices)

    @staticmethod
    def Point(location: Point2) -> "Spatial":
        return Spatial(point=location)


class Selection(Struct, frozen=True):  # the `doc.query` EQL + optional `Spatial` read spec
    eql: str = "*"  # the DXF entity-query-language string, `'LINE CIRCLE[layer=="WALLS"]'`
    spatial: Spatial | None = None  # the optional rtree spatial refinement


@tagged_union(frozen=True)
class BridgeSpec:  # the DXF<->SVG<->GeoJSON<->glyph geometry wire at the `graphic/vector#VECTOR` seam
    tag: Literal["to_svg", "from_svg", "to_geojson", "from_geojson", "text_paths"] = tag()
    to_svg: tuple[DxfSource, float, BridgeSample] = case()  # (source, flatten distance, sample mode) -> framed SVG
    from_svg: tuple[tuple[tuple[Point2, ...], ...], DxfVersion, DxfAttribs] = case()  # (vertex rings, version, attribs) -> DXF
    to_geojson: DxfSource = case()  # entities -> GeoProxy.__geo_interface__ mapping bytes
    from_geojson: tuple[bytes, DxfVersion, DxfAttribs] = case()  # (GeoJSON bytes, version, attribs) -> DXF
    text_paths: tuple[str, str, float, Point3, DxfVersion, DxfAttribs] = case()  # (text, font family, size, insert, version, attribs) -> DXF outline

    @staticmethod
    def ToSvg(source: DxfSource, distance: float = 0.1, sample: BridgeSample = BridgeSample.FLATTEN) -> "BridgeSpec":
        return BridgeSpec(to_svg=(source, distance, sample))

    @staticmethod
    def FromSvg(rings: tuple[tuple[Point2, ...], ...], version: DxfVersion = DxfVersion.R2018, attribs: DxfAttribs = DxfAttribs()) -> "BridgeSpec":
        return BridgeSpec(from_svg=(rings, version, attribs))

    @staticmethod
    def ToGeoJson(source: DxfSource) -> "BridgeSpec":
        return BridgeSpec(to_geojson=source)

    @staticmethod
    def FromGeoJson(mapping: bytes, version: DxfVersion = DxfVersion.R2018, attribs: DxfAttribs = DxfAttribs()) -> "BridgeSpec":
        return BridgeSpec(from_geojson=(mapping, version, attribs))

    @staticmethod
    def TextPaths(text: str, font: str = "sans-serif", size: float = 10.0, insert: Point3 = (0.0, 0.0, 0.0), version: DxfVersion = DxfVersion.R2018, attribs: DxfAttribs = DxfAttribs()) -> "BridgeSpec":
        return BridgeSpec(text_paths=(text, font, size, insert, version, attribs))


@tagged_union(frozen=True)
class DxfOp:  # the closed request vocabulary lowered once into DxfComposed
    tag: Literal["new", "read", "recover", "render", "query", "bridge"] = tag()
    new: DxfDocument = case()
    read: DxfSource = case()
    recover: DxfSource = case()
    render: tuple[DxfSource, DxfBackend, PageSpec] = case()
    query: tuple[DxfSource, Selection] = case()
    bridge: BridgeSpec = case()

    @staticmethod
    def New(document: DxfDocument) -> "DxfOp":
        return DxfOp(new=document)

    @staticmethod
    def Read(source: DxfSource) -> "DxfOp":
        return DxfOp(read=source)

    @staticmethod
    def Recover(source: DxfSource) -> "DxfOp":
        return DxfOp(recover=source)

    @staticmethod
    def Render(source: DxfSource, backend: DxfBackend = DxfBackend.SVG, page: PageSpec = PageSpec()) -> "DxfOp":
        return DxfOp(render=(source, backend, page))

    @staticmethod
    def Query(source: DxfSource, selection: Selection = Selection()) -> "DxfOp":
        return DxfOp(query=(source, selection))

    @staticmethod
    def Bridge(spec: BridgeSpec) -> "DxfOp":
        return DxfOp(bridge=spec)


# --- [OPERATIONS] -----------------------------------------------------------------------
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


@_GUARD
def _admit(dpi: PositiveInt, scale: Positive, margin: NonNegative, distance: Positive, /) -> None:
    # the scalar admission seam beartype deep-checks: a non-positive `dpi`/`scale`/`distance` rails as the
    # `api` BoundaryFault BEFORE the render/bridge divide, the `Is`-refined aliases load-bearing where a
    # `@_GUARD` on `_composed(op: DxfOp)` would be dead (beartype never recurses into a case-tuple payload).
    return None


def _ingest(source: DxfSource, /) -> "Drawing":  # the conforming polymorphic ingestion family
    match source:
        case DxfSource(tag="blob", blob=data):
            return ezdxf.read(io.StringIO(data.decode(errors="surrogateescape")))
        case DxfSource(tag="file", file=path):
            return ezdxf.readfile(path)
        case DxfSource(tag="zip", zip=(path, member)):
            return ezdxf.readzip(path, member)
        case DxfSource(tag="base64", base64=text):
            return ezdxf.decode_base64(text.encode())
        case _ as unreachable:
            assert_never(unreachable)


def _binary(source: DxfSource, /) -> bytes:  # normalize any non-file source to a binary stream for salvage
    match source:
        case DxfSource(tag="blob", blob=data):
            return data
        case DxfSource(tag="base64", base64=text):
            return base64.b64decode(text)
        case DxfSource(tag="zip", zip=(path, member)):
            with zipfile.ZipFile(path) as archive:
                return archive.read(member or archive.namelist()[0])
        case DxfSource(tag="file", file=path):
            return Path(path).read_bytes()
        case _ as unreachable:
            assert_never(unreachable)


def _recovered(source: DxfSource, /) -> tuple["Drawing", object]:  # `recover.readfile`/`read` -> (doc, auditor)
    match source:
        case DxfSource(tag="file", file=path):
            return recover.readfile(path)
        case _:
            return recover.read(io.BytesIO(_binary(source)))


def _build_entity(layout: "Modelspace | BlockLayout", entity: DxfEntity, /) -> None:
    # Exemption: the ezdxf `Drawing` is a mutable builder, so each `add_*` is the platform-forced
    # construction statement — the domain shape is the closed `DxfEntity` match, the mutation is ezdxf's.
    match entity:
        case DxfEntity(tag="line", line=(start, end, at)):
            layout.add_line(start, end, dxfattribs=at.gfx())
        case DxfEntity(tag="arc", arc=(center, radius, lo, hi, at)):
            layout.add_arc(center, radius, lo, hi, dxfattribs=at.gfx())
        case DxfEntity(tag="circle", circle=(center, radius, at)):
            layout.add_circle(center, radius, dxfattribs=at.gfx())
        case DxfEntity(tag="ellipse", ellipse=(center, major, ratio, lo, hi, at)):
            layout.add_ellipse(center, major, ratio, lo, hi, dxfattribs=at.gfx())
        case DxfEntity(tag="spline", spline=(fit, degree, at)):
            layout.add_spline(fit_points=fit, degree=degree, dxfattribs=at.gfx())
        case DxfEntity(tag="lwpolyline", lwpolyline=(points, close, at)):
            layout.add_lwpolyline(points, format="xyseb", close=close, dxfattribs=at.gfx())
        case DxfEntity(tag="hatch", hatch=(loops, color, at)):
            fill = layout.add_hatch(color=color, dxfattribs=at.gfx())
            for loop in loops:
                fill.paths.add_polyline_path(loop, is_closed=True)
        case DxfEntity(tag="text", text=(body, insert, height, at)):
            layout.add_text(body, height=height, dxfattribs=at.gfx()).set_placement(insert)
        case DxfEntity(tag="mtext", mtext=(body, insert, height, at)):
            layout.add_mtext(body, dxfattribs={**at.gfx(), "char_height": height, "insert": insert})
        case DxfEntity(tag="leader", leader=(vertices, at)):
            layout.add_leader(vertices, dxfattribs=at.gfx())
        case DxfEntity(tag="dimension", dimension=(kind, defpoints, dimstyle, at)):
            _DIM[kind](layout, defpoints, dimstyle, at.gfx()).render()
        case DxfEntity(tag="blockref", blockref=(name, insert, scale, rotation, at)):
            layout.add_blockref(name, insert, dxfattribs={**at.gfx(), "xscale": scale, "yscale": scale, "rotation": rotation})
        case DxfEntity(tag="point", point=(location, at)):
            layout.add_point(location, dxfattribs=at.gfx())
        case DxfEntity(tag="mesh", mesh=(vertices, faces, at)):
            body = layout.add_mesh(dxfattribs=at.gfx())
            with body.edit_data() as data:
                data.vertices, data.faces = list(vertices), list(faces)
        case _ as unreachable:
            assert_never(unreachable)


def _table_entry(doc: "Drawing", entry: TableEntry, /) -> None:
    match entry:
        case TableEntry(tag="layer", layer=(name, color, linetype, lineweight)):
            doc.layers.add(name, color=color, linetype=linetype, lineweight=lineweight)
        case TableEntry(tag="linetype", linetype=(name, pattern, description)):
            doc.linetypes.add(name, pattern=list(pattern), description=description)
        case TableEntry(tag="textstyle", textstyle=(name, font, height, width)):
            doc.styles.add(name, font=font, dxfattribs={"height": height, "width": width})
        case TableEntry(tag="dimstyle", dimstyle=(name, overrides)):
            doc.dimstyles.add(name, dxfattribs=dict(overrides))
        case _ as unreachable:
            assert_never(unreachable)


def _attach_xref(doc: "Drawing", ref: Xref, /) -> None:  # bind one external DXF reference by name
    xref.attach(doc, block_name=ref.block_name, filename=ref.filename, insert=ref.insert, scale=ref.scale, rotation=ref.rotation)


def _authored(spec: DxfDocument, /) -> tuple["Drawing", object]:
    # Exemption: the `Drawing` is a mutable builder — the construction fold is ezdxf's platform seam.
    doc = ezdxf.new(spec.version.value, setup=spec.setup, units=int(spec.units))
    for entry in spec.tables:
        _table_entry(doc, entry)
    for ref in spec.xrefs:
        _attach_xref(doc, ref)
    for block in spec.blocks:
        layout = doc.blocks.new(block.name)
        for entity in block.entities:
            _build_entity(layout, entity)
    msp = doc.modelspace()
    for entity in spec.entities:
        _build_entity(msp, entity)
    return doc, doc.audit()


def _counts(doc: "Drawing", /) -> frozendict[str, int]:
    return frozendict(Counter(entity.dxftype() for entity in doc.modelspace()))


def _extent(doc: "Drawing", /) -> Extent:
    box = bbox.extents(doc.modelspace())
    return (*box.extmin.xyz, *box.extmax.xyz) if box.has_data else _ZERO_EXTENT


def _serialize(doc: "Drawing", fmt: DxfFormat, /) -> bytes:
    match fmt:
        case DxfFormat.ASC:
            sink = io.StringIO()
            doc.write(sink)
            return sink.getvalue().encode()
        case DxfFormat.BIN:
            binary = io.BytesIO()
            doc.write(binary, fmt="bin")
            return binary.getvalue()
        case DxfFormat.BASE64:
            return doc.encode_base64()
        case _ as unreachable:
            assert_never(unreachable)


def _dxf_composed(doc: "Drawing", auditor: object, fmt: DxfFormat, /) -> DxfComposed:
    data = _serialize(doc, fmt)
    return DxfComposed(
        data=data, kind=DxfArtifact.DXF, dxfversion=doc.dxfversion, units=_UNITS[DxfUnits(doc.units)],
        counts=_counts(doc), layers=len(doc.layers), blocks=len(doc.blocks),
        errors=len(getattr(auditor, "errors", ())), fixes=len(getattr(auditor, "fixes", ())), extent=_extent(doc),
    )


def _flattened(doc: "Drawing", distance: float, sample: BridgeSample, /) -> Iterator[np.ndarray]:
    # the lazy per-entity vertex fold: only `_PATH_TYPES` drawables cross `make_path` (a TOTAL predicate, so
    # no per-element `TypeError` from a `Text`/`Insert` drives the loop), `flattening` adaptively samples a
    # curve and `control_vertices` reads the exact NURBS frame; the `(N, 3)` numpy array is the shared
    # offset/simplify/hull lane the `render_lines` result crosses back through, materialized only at `svg`.
    for entity in doc.modelspace():
        if entity.dxftype() in _PATH_TYPES:
            outline = dxfpath.make_path(entity)
            verts = outline.control_vertices() if sample is BridgeSample.CONTROL else outline.flattening(distance)
            array = np.asarray(Vec3.list(verts))
            if array.size:
                yield array


def _polyline(vertices: np.ndarray, /) -> str:
    # one drawable's flattened polyline as an SVG `<path>` `d` over the numeric `(N, 3)` lane — trusted
    # geometry at the `graphic/vector#VECTOR` `path` altitude, the whole stream framed once by its `svg`.
    body = "M" + " L".join(f"{x:g},{y:g}" for x, y, _ in vertices)
    return f'<path fill="none" stroke="black" d="{body}"/>'


def _spatial(entities: "EntityQuery", spec: Spatial, /) -> "list[DXFGraphic]":
    match spec:
        case Spatial(tag="window", window=(low, high, test)):
            return list(_SPATIAL_TEST[test](select.Window(low, high), entities))
        case Spatial(tag="circle", circle=(center, radius, test)):
            return list(_SPATIAL_TEST[test](select.Circle(center, radius), entities))
        case Spatial(tag="polygon", polygon=(vertices, test)):
            return list(_SPATIAL_TEST[test](select.Polygon(vertices), entities))
        case Spatial(tag="fence", fence=vertices):
            return list(select.bbox_crosses_fence(vertices, entities))
        case Spatial(tag="point", point=location):
            return list(select.point_in_bbox(location, entities))
        case _ as unreachable:
            assert_never(unreachable)


def _rendered(source: DxfSource, backend: DxfBackend, page: PageSpec, /) -> DxfComposed:
    _admit(page.dpi, page.scale, page.margin, 1.0)
    doc = _ingest(source)
    context = RenderContext(doc)
    config = dwgconfig.Configuration(background_policy=dwgconfig.BackgroundPolicy.BLACK if page.dark else dwgconfig.BackgroundPolicy.WHITE)
    layout = dwglayout.Page(page.width, page.height, dwglayout.Units.mm, dwglayout.Margins.all(page.margin))
    settings = dwglayout.Settings(fit_page=page.fit_page, scale=page.scale)
    shared = {"dxfversion": doc.dxfversion, "units": _UNITS[DxfUnits(doc.units)],
              "counts": _counts(doc), "layers": len(doc.layers), "blocks": len(doc.blocks), "extent": _extent(doc)}
    match backend:
        case DxfBackend.SVG:
            sink = SVGBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return DxfComposed(data=sink.get_string(layout, settings=settings).encode(), kind=DxfArtifact.SVG, **shared)
        case DxfBackend.PDF:
            sink = PyMuPdfBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return DxfComposed(data=sink.get_pdf_bytes(layout, settings=settings), kind=DxfArtifact.PDF, **shared)
        case DxfBackend.PNG:
            sink = PyMuPdfBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return DxfComposed(data=sink.get_pixmap_bytes(layout, settings=settings, dpi=page.dpi), kind=DxfArtifact.PNG, **shared)
        case DxfBackend.EPS | DxfBackend.PS:
            figure = Figure()  # GC-safe (no `pyplot` global registry), so no bracket — the render sink savefig writes
            Frontend(context, MatplotlibBackend(figure.add_axes((0.0, 0.0, 1.0, 1.0))), config).draw_layout(doc.modelspace(), finalize=True)
            vector = io.BytesIO()
            figure.savefig(vector, format=backend.value, dpi=page.dpi)
            return DxfComposed(data=vector.getvalue(), kind=DxfArtifact(backend.value), **shared)
        case DxfBackend.JSON | DxfBackend.GEOJSON:
            sink = CustomJSONBackend() if backend is DxfBackend.JSON else GeoJSONBackend()
            Frontend(context, sink, config).draw_layout(doc.modelspace(), finalize=True)
            return DxfComposed(data=sink.get_string().encode(), kind=DxfArtifact(backend.value), **shared)
        case _ as unreachable:
            assert_never(unreachable)


def _queried(source: DxfSource, selection: Selection, /) -> DxfComposed:
    doc = _ingest(source)
    matched = doc.query(selection.eql)
    entities = _spatial(matched, selection.spatial) if selection.spatial is not None else list(matched)
    extract = ezdxf.new(doc.dxfversion, setup=True)
    importer = Importer(doc, extract)
    importer.import_entities(entities)
    importer.finalize()
    box = bbox.extents(entities)
    return DxfComposed(
        data=_serialize(extract, DxfFormat.ASC), kind=DxfArtifact.DXF, dxfversion=doc.dxfversion,
        units=_UNITS[DxfUnits(doc.units)], counts=frozendict(Counter(entity.dxftype() for entity in entities)),
        layers=len(extract.layers), blocks=len(extract.blocks),
        extent=(*box.extmin.xyz, *box.extmax.xyz) if box.has_data else _ZERO_EXTENT,
    )


def _bridged(spec: BridgeSpec, /) -> DxfComposed:
    match spec:
        case BridgeSpec(tag="to_svg", to_svg=(source, distance, sample)):
            _admit(1, 1.0, 0.0, distance)
            doc = _ingest(source)
            extent = _extent(doc)
            fragments = tuple(_polyline(verts) for verts in _flattened(doc, distance, sample))
            data = svg_frame(fragments, (extent[0], extent[1], extent[3], extent[4]))
            return DxfComposed(data=data, kind=DxfArtifact.SVG, dxfversion=doc.dxfversion,
                               units=_UNITS[DxfUnits(doc.units)], counts=_counts(doc), layers=len(doc.layers), extent=extent)
        case BridgeSpec(tag="from_svg", from_svg=(rings, version, attribs)):
            doc = ezdxf.new(version.value, setup=True)
            paths = [dxfpath.from_vertices(ring, close=False) for ring in rings if ring]
            dxfpath.render_lines(doc.modelspace(), paths, dxfattribs=attribs.gfx())
            return _dxf_composed(doc, doc.audit(), DxfFormat.ASC)
        case BridgeSpec(tag="to_geojson", to_geojson=source):
            doc = _ingest(source)
            proxy = dxfgeo.proxy(entity for entity in doc.modelspace() if entity.dxftype() in _GEO_TYPES)
            return DxfComposed(data=msgspec.json.encode(proxy.__geo_interface__), kind=DxfArtifact.GEOJSON,
                               dxfversion=doc.dxfversion, units=_UNITS[DxfUnits(doc.units)], counts=_counts(doc), layers=len(doc.layers), extent=_extent(doc))
        case BridgeSpec(tag="from_geojson", from_geojson=(mapping, version, attribs)):
            # Exemption: the `Drawing` mutable builder — the GeoProxy entity fold is ezdxf's construction seam.
            doc = ezdxf.new(version.value, setup=True)
            msp = doc.modelspace()
            for entity in dxfgeo.GeoProxy.parse(msgspec.json.decode(mapping)).to_dxf_entities(dxfattribs=attribs.gfx()):
                msp.add_entity(entity)
            return _dxf_composed(doc, doc.audit(), DxfFormat.ASC)
        case BridgeSpec(tag="text_paths", text_paths=(text, font, size, insert, version, attribs)):
            doc = ezdxf.new(version.value, setup=True)
            paths = text2path.make_paths_from_str(text, FontFace(family=font), size=size, m=Matrix44.translate(*insert))
            dxfpath.render_lines(doc.modelspace(), paths, dxfattribs=attribs.gfx())
            return _dxf_composed(doc, doc.audit(), DxfFormat.ASC)
        case _ as unreachable:
            assert_never(unreachable)


def _composed(op: DxfOp) -> DxfComposed:  # the one pure fold both `of` and `contribute` read
    match op:
        case DxfOp(tag="new", new=document):
            doc, auditor = _authored(document)
            return _dxf_composed(doc, auditor, document.fmt)
        case DxfOp(tag="read", read=source):
            doc = _ingest(source)
            return _dxf_composed(doc, doc.audit(), DxfFormat.ASC)
        case DxfOp(tag="recover", recover=source):
            doc, auditor = _recovered(source)
            return _dxf_composed(doc, auditor, DxfFormat.ASC)
        case DxfOp(tag="render", render=(source, backend, page)):
            return _rendered(source, backend, page)
        case DxfOp(tag="query", query=(source, selection)):
            return _queried(source, selection)
        case DxfOp(tag="bridge", bridge=spec):
            return _bridged(spec)
        case _ as unreachable:
            assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------
@stamina.retry(on=OSError, attempts=3)
async def _offloaded(op: DxfOp, /) -> DxfComposed:
    # the GIL-releasing render / disk-read fold crosses one `_GATE`-bounded `to_thread` band off the loop;
    # `stamina` re-arms only a transient `OSError` (a font/resource/stream load) before the boundary converts
    # a persistent one, the cancellation class excluded from the retry set (concurrency.md RETRY_BOUNDARY).
    return await to_thread.run_sync(_composed, op, limiter=_GATE)


class Dxf(Struct, frozen=True):
    op: DxfOp

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"dxf.{self.op.tag}", self._keyed, catch=_FAULTS)

    async def _keyed(self) -> ContentKey:
        composed = await _offloaded(self.op)
        return ContentIdentity.of(f"dxf-{self.op.tag}", composed.data)

    def contribute(self) -> "Iterable[Receipt]":
        # `contribute` re-enters the SAME deterministic `_composed` fold `of` reads (the sibling
        # `composition/sheet#SHEET` producer contract), mints the content key over its bytes, and yields the
        # one `Cad` case; no memo stands in for the fold, no parallel DXF-receipt rail.
        composed = _composed(self.op)
        key = ContentIdentity.of(f"dxf-{self.op.tag}", composed.data)
        receipt = ArtifactReceipt.Cad(
            key, composed.dxfversion, composed.units, composed.kind.value, len(composed.data),
            composed.layers, composed.blocks, composed.errors, composed.fixes, composed.counts,
        )
        yield from receipt.contribute()


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "BlockDef", "BridgeSample", "BridgeSpec", "DimKind", "Dxf", "DxfArtifact", "DxfAttribs", "DxfBackend",
    "DxfDocument", "DxfEntity", "DxfFormat", "DxfOp", "DxfSource", "DxfUnits", "DxfVersion",
    "PageSpec", "Selection", "Spatial", "SpatialTest", "TableEntry", "Xref",
]
```

## [03]-[RESEARCH]

- [OWNER_MODEL_SETTLED]: `Dxf` is a single-op producer (`Dxf(op)`, `of() -> RuntimeRail[ContentKey]`, `contribute()`) exactly as the sibling `composition/sheet#SHEET` and `composition/imposition#IMPOSE` are, NOT a modal `Vector.over` primitive — the discriminant is the `DxfOp` case, the receipt one `ArtifactReceipt.Cad` per content key, and batch fan-out is the `core/plan#PLAN` `ArtifactWork` concern, never an owner-level `Block[DxfResult]`. The `Emit` op the initial reading map named collapses into the `DxfFormat` egress policy value on the DXF-producing arms (POLICY_VALUES), because asc/binary/base64 encoding varies only a literal the value already carries — a parallel `Emit` op would be the boolean-flag-into-two-bodies defect. `contribute` re-runs `_composed` exactly as the sibling `composition/sheet#SHEET` `contribute` re-runs its own fold (the producer/receipt split is two lifecycle entry points on a frozen owner, and the corpus forbids a `@cache` memo bridging them), so the double-fold is the settled corpus producer contract, not a divergence. Justified on DOMAIN (a producer mints one content-keyed artifact per op) and the sibling settled producer shape.
- [RAIL_SETTLED]: the fault rail is the branch `RuntimeRail[ContentKey]` minted ONCE at `async_boundary(f"dxf.{op.tag}", self._keyed, catch=_FAULTS)` where `_FAULTS = (DXFError, RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)` — `DXFError` named eagerly because the `ezdxf` error hierarchy derives from `Exception` directly (verified `DXFStructureError.__mro__` ends `... DXFError, Exception, BaseException, object`), so no stdlib base covers it and the sibling `composition/imposition#IMPOSE` `PdfImposeUserError` pattern applies. There is NO parallel `DxfFault` `Literal` the boundary double-rails: a salvageable damaged file produces bytes and its `auditor.errors`/`auditor.fixes` counts ride the `Cad` receipt (verified `recover.read` returns `(doc, auditor)` with `.errors`/`.fixes` lists), a truly-unparseable input raises `DXFStructureError` the boundary converts. A non-convertible `Text`/`Insert` crossing `make_path`/`geo.proxy` raises `TypeError` (verified), which the `_PATH_TYPES`/`_GEO_TYPES` TOTAL predicate filters BEFORE the raise so the hot fold never traps per element (NO-EXCEPTION-HOTLOOP), rather than adding `TypeError` to `_FAULTS`. Justified on the sibling settled anti-pattern and the doctrine hot-loop law.
- [RETRY_SETTLED]: the worker seam carries a `stamina.retry(on=OSError, attempts=3)` aspect over `_offloaded` (verified `stamina.retry(*, on, attempts=10, timeout=45.0, ...)`), re-arming a TRANSIENT `OSError` — a font/resource/stream load crossing the disk boundary the `Read`/`Recover` ingest and the render backends incur — before `async_boundary` converts a persistent one, the cancellation class excluded from the transient set (concurrency.md RETRY_BOUNDARY). This is the reading-map PACKAGE gap the naive page ignored: DXF file IO (`readfile`/`recover.readfile` from paths, font resource loading during render) is transient-prone where the sibling `composition/sheet#SHEET` in-memory PDF authoring is not, so the aspect is justified for dxf specifically, not corpus noise. Justified on PACKAGE (`stamina.retry`) and DOMAIN (disk/resource IO transience).
- [OFFLOAD_SETTLED]: `to_thread` is the lane for the WHOLE `_composed` fold, not `to_interpreter`, because the render backends (`PyMuPdfBackend`, `MatplotlibBackend`) are GIL-releasing native code not isolate-safe for a subinterpreter (the pure-Python `ezdxf` authoring arms ride the same seam to keep one worker band), and the fold returns the `msgspec`-backed `DxfComposed` the `to_interpreter` arm cannot load the C-extension for — the sibling `composition/sheet#SHEET`/`graphic/vector#VECTOR` chooser one-for-one. The `matplotlib.figure.Figure` is constructed directly (NOT `pyplot.figure`), so no global `pyplot` registry holds it and it is GC-collected with no bracket, distinct from the sibling `pymupdf` documents that require a `with` — verified `Figure().savefig(BytesIO, format="eps"/"ps")` produces `%!PS-Adobe` bytes headless. Justified on the concurrency OFFLOAD_LANE law and the CAPSULE_OWNER GC-safe exemption.
- [BRIDGE_WIRE_SETTLED]: the `Bridge` arm is FIVE crossings, none re-implementing the sibling's geometry — `ToSvg` folds `make_path(entity).flattening(distance)`/`control_vertices()` over the `_PATH_TYPES` drawables (verified `make_path` supports `LINE`/`ARC`/`CIRCLE`/`ELLIPSE`/`SPLINE`/`LWPOLYLINE`/`HATCH`), `Vec3.list(...)` → `np.asarray` recording one `(N, 3)` array (confirmed shape), `_polyline` emitting each as one `<path d="M…">` and the WHOLE fragment stream framed once by the `graphic/vector#VECTOR` `svg(fragments, viewbox)` owner (imported one hop). This closes the naive form's TWO defects verified in the prior fence: the `to_svg` fold rebound `outline = make_path(entity)` per iteration keeping ONLY the last entity's path (an illusory fold the prose called "illustrative"), and it spliced the result into a hand-rolled `<svg>` f-string (a TEMPLATE_STRUCTURE_SITE injection surface) — both deleted, the whole document now folded and framed by the owner. `FromSvg` folds each vertex ring through `from_vertices`/`render_lines` (verified) so a `skia-pathops` boolean's multiple contours cross in; `ToGeoJson`/`FromGeoJson` cross `geo.proxy(...).__geo_interface__`/`GeoProxy.parse(...).to_dxf_entities()` (verified round-trip over a `GeometryCollection`), the `_GEO_TYPES` predicate filtering non-convertibles; `TextPaths` crosses `text2path.make_paths_from_str(text, FontFace(family=), size, m=Matrix44.translate(*insert))` (verified) composing `fonttools`. Justified on PACKAGE (`ezdxf.path`/`addons.geo`/`addons.text2path` + `numpy` + the `graphic/vector#VECTOR` `svg` owner) and the prior fence's illusory-fold defect.
- [RENDER_LOWERING_SETTLED]: the `Render` arm drives `Frontend(RenderContext(doc), backend, Configuration(...)).draw_layout(msp, finalize=True)` and reads the sink over a SEVEN-member `DxfBackend` (verified end to end): `SVGBackend.get_string(page, settings=)` lowers into `graphic/vector#VECTOR`, `PyMuPdfBackend.get_pdf_bytes`/`get_pixmap_bytes(dpi=)` into `composition/sheet#SHEET`, `MatplotlibBackend` + `Figure.savefig(format="eps"/"ps")` produces the publication-vector EPS/PS the SVG/PyMuPDF backends cannot (the reading-map underutilized item now adopted — matplotlib is the sole ezdxf backend reaching EPS/PS/PGF), and `CustomJSONBackend`/`GeoJSONBackend.get_string()` produce the structured geometry export (verified `get_string()` takes no `Page`). A new backend or format is one `DxfBackend`+`DxfArtifact` member plus one `_rendered` arm. Justified on PACKAGE (the in-process `Frontend`+backend stack, no foreign renderer) and the UNIFIED TELOS publication plane (EPS/PS).
- [QUERY_SPATIAL_SETTLED]: the `Query` arm refines `doc.query(eql)` with the `Spatial` closed family (`Window`/`Circle`/`Polygon` under a `SpatialTest` `_SPATIAL_TEST` row keying `select.bbox_inside`/`bbox_outside`/`bbox_overlap`, plus `Fence` → `bbox_crosses_fence` and `Point` → `point_in_bbox`) — verified `select.Window`/`Circle`/`Polygon` are the `SelectionShape` classes while `Fence`/`Point` are FUNCTIONS taking raw vertices/point (no shape class), so the family models the fence/point cases directly rather than as phantom shape classes. The census is `Counter(dxftype)` uniformly across every arm (the naive page's `group_by="layer"` field was DEAD — it drove no count and was deleted, a per-attribute `groupby(dxfattrib=)` census the documented growth axis when a consumer needs it). Justified on PACKAGE (the verified `select` surface) and the dead-field cleanup.
- [CROSS_FILE]: the new `ArtifactReceipt.Cad(key, dxfversion, units, artifact, bytes, layers, blocks, errors, fixes, counts)` case lands on `core/receipt.md` (the `cad` `ArtifactKind` token + case tuple + `Cad` mint + the `cad` or-pattern arm in `slot` + the special `_facts` arm flattening `counts` like `preview`/`verdict`) so `Dxf.contribute` type-checks against the real receipt owner — the receipt union had NO `cad` case, so the prior fence's `ArtifactReceipt.Cad(...)` was a phantom against the sibling owner (MODEL-COHERENCE); this rebuild adds the real case. The `export/dxf` codemap node and the `export/dxf → composition/sheet`, `export/dxf → graphic/vector`, `graphic/vector ↔ export/dxf`, and `export/dxf → geospatial` (GeoJSON) seams land on `ARCHITECTURE.md` `[01]`/`[02]`; the `[DXF]` router row lands on `README.md` `[01]` and the `ezdxf` roster row on `README.md` `[02]`. The `drawing/standard` ISO-vocabulary lowering onto the `TableEntry` rows is a future-page consumer contract, not authored here.
