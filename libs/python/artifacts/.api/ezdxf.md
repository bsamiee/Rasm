# [PY_ARTIFACTS_API_EZDXF]

`ezdxf` is the single categorical-best owner of DXF read/write/render for the artifacts CAD-export rail: a `Drawing` document graph (header vars, symbol tables, blocks, modelspace + paperspace layouts, the objects dictionary) authored through one `GraphicsFactory.add_*` builder family, the `ezdxf.path.Path` command-segment algebra that is the lossless bridge between DXF curve entities and the SVG path geometry the `svgelements`/`graphic/vector` owners already speak, a `ezdxf.math` kernel (`Vec3`/`Matrix44`/`BSpline`/`OCS`/`UCS`/`BoundingBox`) the geometry seam reads, the `addons.drawing` `Frontend`+backend family that rasterizes/vectorizes any DXF to Matplotlib/SVG/PDF/JSON without a foreign renderer, and the boundary surfaces (`recover` salvage, `xref` block linking, `select` spatial query, `transform` affine, `query`/`groupby` selection, `bbox` extents, `addons.geo` GeoJSON, `addons.Importer` cross-document copy). The `export/dxf` owner composes `ezdxf.new`/`readfile`/`recover.readfile`, the `GfxAttribs` attribute value object, `ezdxf.path.make_path`/`render_*`, and the `Frontend`/`SVGBackend`/`qsave` render drivers; it never re-implements the DXF tag grammar, the OCS/WCS transform, the B-spline evaluator, or the DXF-entity-to-SVG path conversion ezdxf already owns, and it never re-authors the IFC semantic model (`csharp:Rasm.Bim`) or the placement/scaling the composition owners hold. Pure-Python wheel (`py3-none-any`), so no cp-gate applies on any interpreter.

## [01]-[PACKAGE_SURFACE]

- package: `ezdxf`
- import: `ezdxf`
- license: MIT
- owner: `artifacts`
- rail: cad-export
- installed: `1.4.4`
- build floor: pure-Python (`ezdxf-1.4.4-py3-none-any.whl`); no abi3/cp-gate — installs on every interpreter incl. 3.15
- runtime deps: `fonttools`, `numpy`, `pyparsing`, `typing-extensions` (the `numpy` dep is the same array substrate `libs/python/.api/numpy.md` owns; `fonttools` is the same font surface `fonttools.md` owns — ezdxf composes both, no second copy)
- optional render deps: `matplotlib` (raster/PDF via `qsave`), `PyMuPDF` (`PyMuPdfBackend`); both already admitted folder packages (`matplotlib.md`, `pymupdf.md`)
- entry points: none (library only)
- capability: DXF R12->R2018 read/write/recover, the full graphic-entity builder family (line/arc/circle/ellipse/spline/lwpolyline/polyline/mesh/hatch/mpolygon/text/mtext/leader/multileader/all dimension kinds/image/wipeout/3d-solid/surface), the `Path` command-segment algebra with curve flattening + DXF<->SVG<->hatch conversion, the `ezdxf.math` geometry kernel (vectors/matrices/splines/OCS/UCS/construction primitives/bounding boxes), the `addons.drawing` render frontend over Matplotlib/SVG/PDF/JSON/GeoJSON backends with a full property/policy resolution stack, symbol-table + layer + linetype + textstyle + dimstyle authoring, block + external-reference (`xref`) management, spatial `select` (rtree-backed window/fence/point query), `transform` affine ops, `query`/`groupby` entity selection, `bbox` extents, GeoJSON `addons.geo` round-trip, cross-document `addons.Importer` copy, `text2path` glyph-outline conversion, and the `r12writer` streaming fast-writer

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and layout roots
- rail: cad-export

The `Drawing` is the single document owner; `modelspace()`/`paperspace(name)` return `Layout` views that ARE the `GraphicsFactory` (every `add_*` builder lives on the layout, not the doc). A `BlockLayout` is the reusable-geometry container an `Insert` references. One owner, never a parallel reader/writer pair.

| [INDEX] | [TYPE]        | [KIND]        | [ROLE]                                                                    |
| :-----: | :------------ | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `Drawing`     | document root | the whole DXF model; `modelspace`/`paperspace`/`blocks`/`tables`/`saveas` |
|  [02]   | `Modelspace`  | layout        | the model layout; a `GraphicsFactory` — every `add_*` builder + `query`   |
|  [03]   | `Paperspace`  | layout        | a print layout; same builder family + `page_setup` + `add_viewport`       |
|  [04]   | `BlockLayout` | block         | a reusable block definition the `Insert` entity references                |
|  [05]   | `EntityDB`    | store         | handle->entity database; `doc.entitydb` resolves any `#handle`            |
|  [06]   | `GfxAttribs`  | value object  | the shared graphic-attribute bundle (`layer`/`color`/`rgb`/`linetype`/…)  |

[PUBLIC_TYPE_SCOPE]: graphic-entity vocabulary
- rail: cad-export

Every drawable derives `DXFGraphic`; each carries a typed `.dxf` namespace (the DXF group-code attributes) plus entity-specific methods. The builder family on the layout is the only admitted construction path — never hand-assembled tags. This is the closed entity grammar the `export/dxf` owner emits and the render `Frontend` consumes.

| [INDEX] | [TYPE]                                  | [KIND]    | [ROLE]                                                               |
| :-----: | :-------------------------------------- | :-------- | :------------------------------------------------------------------- |
|  [01]   | `DXFGraphic`                            | base      | transformable drawable base; `.dxf` attribs, `translate`/`transform` |
|  [02]   | `Line`                                  | linear    | straight segment (`dxf.start`/`dxf.end`)                             |
|  [03]   | `Arc` / `Circle`                        | curve     | circular arc / circle (`dxf.center`/`dxf.radius`)                    |
|  [04]   | `Ellipse`                               | curve     | elliptical arc (`dxf.major_axis`/`dxf.ratio`)                        |
|  [05]   | `Spline`                                | curve     | NURBS spline (`fit_points`/`control_points`/`knots`/`weights`)       |
|  [06]   | `LWPolyline`                            | polyline  | lightweight 2D polyline with per-vertex bulge/width                  |
|  [07]   | `Polyline`                              | polyline  | 2D/3D heavy polyline, polymesh, polyface mesh                        |
|  [08]   | `Hatch` / `MPolygon`                    | fill      | solid/gradient/pattern fill (`Hatch`) or filled polygon (`MPolygon`) |
|  [09]   | `Text` / `MText`                        | text      | single-line / multi-line formatted text                              |
|  [10]   | `Leader` / `MultiLeader`                | annot     | leader line / leader-with-content (block or mtext)                   |
|  [11]   | `Dimension`                             | dimension | linear/aligned/angular/radial/diameter/ordinate/arc dimension        |
|  [12]   | `Insert`                                | reference | block reference (placement of a `BlockLayout`) + `ATTRIB` children   |
|  [13]   | `Mesh` / `Body` / `Surface` / `Solid3d` | 3d        | mesh builder / ACIS body / surface / 3D solid                        |
|  [14]   | `Image` / `Wipeout`                     | raster    | raster-image reference / masking wipeout                             |
|  [15]   | `Point` / `Ray` / `XLine`               | primitive | point; infinite / semi-infinite line                                 |
|  [16]   | `Solid` / `Trace` / `Face3d`            | primitive | 2D filled tri/quad; 3D face                                          |

[PUBLIC_TYPE_SCOPE]: symbol-table entries
- rail: cad-export

The named resources entities reference. The `Drawing` exposes one collection per table (`doc.layers`/`doc.linetypes`/…); each `.add(name, dxfattribs=)` mints an entry. These are the drawing-standard substrate the `drawing/standard` AEC vocabulary lowers onto (ISO 128 line types -> `Linetype`, ISO 3098 text heights -> `Textstyle`, the layer-name codec -> `Layer`).

| [INDEX] | [TYPE]                   | [TABLE]                  | [ROLE]                                                          |
| :-----: | :----------------------- | :----------------------- | :-------------------------------------------------------------- |
|  [01]   | `Layer`                  | `doc.layers`             | layer (name/color/linetype/lineweight/on/off/lock/freeze/plot)  |
|  [02]   | `Linetype`               | `doc.linetypes`          | dash/dot/complex linetype definition (ISO 128 line-type target) |
|  [03]   | `Textstyle`              | `doc.styles`             | text style (font/height/width — ISO 3098 lettering target)      |
|  [04]   | `DimStyle`               | `doc.dimstyles`          | dimension style family (ISO 129-1 dimension-style target)       |
|  [05]   | `BlockRecord`            | `doc.block_records`      | block-definition table record                                   |
|  [06]   | `UCSTableEntry` / `View` | `doc.ucs`/`views`        | named coordinate systems / views                                |
|  [07]   | `Viewport` / `AppID`     | `doc.viewports`/`appids` | vports; registered app ids                                      |

[PUBLIC_TYPE_SCOPE]: geometry kernel (`ezdxf.math`)
- rail: cad-export

The pure geometry value objects ezdxf computes against — the same `Vec3`/`Matrix44` shapes the `geometry/*` and the C# `Rasm` geometry seam exchange at the wire. `Vec3.list(...)`/`v.xyz` round-trips to plain tuples a `numpy` array or a `msgspec` struct records; never re-derive an affine or a B-spline evaluator.

| [INDEX] | [TYPE]                                     | [KIND]       | [ROLE]                                                                  |
| :-----: | :----------------------------------------- | :----------- | :---------------------------------------------------------------------- |
|  [01]   | `Vec3` / `Vec2`                            | vector       | 3D/2D vector; `cross`/`dot`/`normalize`/`lerp`/`rotate`/`angle_between` |
|  [02]   | `Matrix44`                                 | affine       | `translate`/`scale`/`*_rotate`/`chain`/`inverse`/`transform_vertices`   |
|  [03]   | `BSpline`                                  | spline       | NURBS curve; `point`/`flattening`/`derivative`/`insert_knot`/`measure`  |
|  [04]   | `Bezier4P` / `Bezier3P` / `Bezier`         | bezier       | cubic/quadratic/arbitrary Bezier curve evaluators                       |
|  [05]   | `OCS` / `UCS`                              | coord sys    | object/user coordinate system <-> WCS transform                         |
|  [06]   | `BoundingBox` / `BoundingBox2d`            | extents      | 3D/2D AABB; `extend`/`union`/`inside`/`intersection`/`size`/`center`    |
|  [07]   | `ConstructionArc` / `ConstructionCircle`   | construction | analytic arc / circle                                                   |
|  [08]   | `ConstructionEllipse` / `ConstructionLine` | construction | analytic ellipse / line                                                 |
|  [09]   | `ConstructionRay` / `ConstructionPolyline` | construction | analytic primitives for intersection/tangent/offset math                |
|  [10]   | `Plane` / `Shape2d`                        | helper       | plane algebra; 2D shape transform                                       |
|  [11]   | `EulerSpiral` / `ApproxParamT`             | helper       | clothoid; arc-length reparam                                            |

[PUBLIC_TYPE_SCOPE]: render frontend + backends (`ezdxf.addons.drawing`)
- rail: cad-export

The in-process render stack — no foreign DXF renderer, no CLI. `Frontend(ctx, backend, config)` walks any layout and drives the chosen `BackendInterface`; the `RenderContext` resolves entity DXF attributes into concrete pen properties through the full layer/linetype/lineweight/color/transparency stack; `config.Configuration` + the policy enums govern lineweight/hatch/text/proxy/background/color behavior. This is how `export/dxf` produces an SVG/PDF/PNG preview and how `visualization/diagram` rasterizes a DXF figure into the document.

| [INDEX] | [TYPE]                                               | [KIND]     | [ROLE]                                                              |
| :-----: | :--------------------------------------------------- | :--------- | :------------------------------------------------------------------ |
|  [01]   | `Frontend`                                           | driver     | walks a layout, resolves properties, calls the backend per entity   |
|  [02]   | `RenderContext`                                      | resolver   | layer/linetype/lineweight/color/font/visibility resolution stack    |
|  [03]   | `Configuration`                                      | policy     | render policy bundle over the closed policy enums                   |
|  [04]   | `MatplotlibBackend`                                  | backend    | Matplotlib `Axes` target -> raster/PDF (`qsave` convenience)        |
|  [05]   | `SVGBackend`                                         | backend    | native SVG string target (`get_string`) — no Matplotlib needed      |
|  [06]   | `PyMuPdfBackend`                                     | backend    | PDF/PNG/PSD target via PyMuPDF (`get_pdf_bytes`/`get_pixmap_bytes`) |
|  [07]   | `CustomJSONBackend` / `GeoJSONBackend`               | backend    | structured JSON / GeoJSON geometry export                           |
|  [08]   | `layout.Page` / `layout.Settings` / `layout.Margins` | page model | output page size/units/margins/fit-to-page for the SVG/PDF backends |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: document factory and IO
- rail: cad-export

`ezdxf.new` mints a fresh document at a target version with optional standard-resource `setup`; `readfile`/`read`/`readzip`/`decode_base64` are the polymorphic ingestion family (file path / text stream / zip member / base64 blob), each returning the same `Drawing`. For damaged third-party files, `recover.readfile` is the salvage path (the only correct loader for non-conforming DXF). `saveas`/`write`/`encode_base64` are the egress family; `audit()` validates structural integrity before save. One document owner — no per-version reader subtype.
- call: `ezdxf.readfile(filename, encoding=None, errors='surrogateescape')` / `ezdxf.read(stream)` / `ezdxf.readzip(zipfile, filename=None)`
- call: `Drawing.units` (property) / `ezdxf.units.conversion_factor(src, tgt)` — insert-units factor

| [INDEX] | [MEMBER]                                                 | [KIND]    | [ROLE]                                                        |
| :-----: | :------------------------------------------------------- | :-------- | :------------------------------------------------------------ |
|  [01]   | `ezdxf.new(dxfversion='AC1027', setup=False, units=6)`   | construct | new document; `setup=True` loads standard linetypes/dimstyles |
|  [02]   | `ezdxf.readfile(...)`                                    | read      | load a conforming DXF from a path                             |
|  [03]   | `ezdxf.read(...)` / `ezdxf.readzip(...)`                 | read      | load from a text stream / from a (zipped) DXF                 |
|  [04]   | `ezdxf.recover.readfile(...)` / `recover.read(stream)`   | salvage   | load a DAMAGED/non-conforming DXF, returns `(doc, auditor)`   |
|  [05]   | `Drawing.saveas(filename, encoding=None, fmt='asc')`     | write     | save to a path; `fmt='bin'` for binary DXF                    |
|  [06]   | `Drawing.write(stream, fmt='asc')` / `.encode_base64()`  | write     | save to a stream / encode to a base64 blob                    |
|  [07]   | `Drawing.audit()`                                        | validate  | structural audit -> `Auditor` (errors/fixes) before egress    |
|  [08]   | `Drawing.modelspace()` / `Drawing.paperspace(name=None)` | layout    | the model / a paper layout (the `GraphicsFactory`)            |
|  [09]   | `Drawing.layout(name)`                                   | layout    | a named layout (the `GraphicsFactory`)                        |
|  [10]   | `Drawing.units` / `ezdxf.units.conversion_factor(...)`   | units     | insert-units (`InsertUnits` enum) + conversion factor         |

[ENTRYPOINT_SCOPE]: graphic-entity builder family (`GraphicsFactory.add_*`)
- rail: cad-export

The single construction surface for every drawable, all living on the layout (`msp.add_*`). Every builder takes a final `dxfattribs=` mapping (or a `GfxAttribs` via `.asdict()`) carrying layer/color/linetype/lineweight — so attribute application is one uniform axis across the whole vocabulary, never a per-entity setter. This is the dense owner the `export/dxf`, `drawing/*` (dimensions/annotation/symbols), and `visualization/diagram` pages emit through. Every `add_*_dim` builder (the full ISO 129-1 family) returns a `DimStyleOverride` whose `.render()` generates the dimension geometry.

| [INDEX] | [MEMBER]                                                              | [KIND]    | [ROLE]                                               |
| :-----: | :-------------------------------------------------------------------- | :-------- | :--------------------------------------------------- |
|  [01]   | `add_line(start, end)`                                                | linear    | a `Line`                                             |
|  [02]   | `add_circle(center, radius, …)`                                       | curve     | a `Circle`                                           |
|  [03]   | `add_arc(center, radius, start_angle, end_angle, …)`                  | curve     | an `Arc`                                             |
|  [04]   | `add_ellipse(center, major_axis, ratio, start_param, end_param, …)`   | curve     | an `Ellipse`                                         |
|  [05]   | `add_spline(fit_points=None, degree=3)` / `add_open_spline`           | curve     | a NURBS `Spline` from fit points / open spline       |
|  [06]   | `add_rational_spline` / `add_cad_spline_control_frame`                | curve     | rational spline / spline from a control frame        |
|  [07]   | `add_lwpolyline(points, format='xyseb', close=False)`                 | polyline  | a 2D `LWPolyline` (`(x,y,start_w,end_w,bulge)` rows) |
|  [08]   | `add_polyline2d` / `add_polyline3d` / `add_polymesh` / `add_polyface` | polyline  | heavy 2D/3D polyline, polygon mesh, polyface mesh    |
|  [09]   | `add_hatch(color=7)`                                                  | fill      | a solid/gradient/pattern `Hatch`                     |
|  [10]   | `add_mpolygon(…)`                                                     | fill      | filled `MPolygon`                                    |
|  [11]   | `add_text(text, height=…)` / `add_mtext(text)`                        | text      | single-line `Text` / multi-line `MText`              |
|  [12]   | `add_blockref(name, insert)`                                          | reference | an `Insert` block reference                          |
|  [13]   | `add_auto_blockref`                                                   | reference | auto-attrib insert                                   |
|  [14]   | `add_attdef`                                                          | reference | attribute def                                        |
|  [15]   | `add_leader(vertices, …)`                                             | annot     | a `Leader`                                           |
|  [16]   | `add_multileader_mtext(style)`                                        | annot     | `MultiLeader` mtext builder (table below)            |
|  [17]   | `add_multileader_block(style)`                                        | annot     | `MultiLeader` block builder (table below)            |
|  [18]   | `add_linear_dim` / `add_aligned_dim` / `add_multi_point_linear_dim`   | dimension | linear / aligned / chained multi-point               |
|  [19]   | `add_angular_dim_2l` / `add_angular_dim_3p` / `add_angular_dim_cra`   | dimension | angular 2-line / 3-point / center-radius-angle       |
|  [20]   | `add_radius_dim` / `add_diameter_dim`                                 | dimension | radius / diameter                                    |
|  [21]   | `add_ordinate_dim` / `add_ordinate_x_dim` / `add_ordinate_y_dim`      | dimension | ordinate + x/y-feature                               |
|  [22]   | `add_arc_dim_3p` / `add_arc_dim_cra`                                  | dimension | arc 3-point / center-radius-angle                    |
|  [23]   | `add_mesh` / `add_3dface` / `add_body` / `add_3dsolid`                | 3d        | mesh / 3D face / ACIS body / 3D solid                |
|  [24]   | `add_surface`                                                         | 3d        | ACIS surface                                         |
|  [25]   | `add_extruded_surface`                                                | 3d        | extruded surface                                     |
|  [26]   | `add_revolved_surface`                                                | 3d        | revolved surface                                     |
|  [27]   | `add_swept_surface`                                                   | 3d        | swept surface                                        |
|  [28]   | `add_image(image_def, insert, size_in_pixels, …)`                     | raster    | raster-image reference                               |
|  [29]   | `add_wipeout(vertices)` / `add_underlay(…)`                           | raster    | masking wipeout / PDF/DWF/DGN underlay               |
|  [30]   | `add_point` / `add_ray` / `add_xline`                                 | primitive | point; semi-infinite ray / infinite line             |
|  [31]   | `add_solid` / `add_trace` / `add_shape`                               | primitive | 2D filled solid/trace; shape ref                     |
|  [32]   | `add_foreign_entity(entity, copy=True)`                               | adopt     | adopt an entity created elsewhere                    |
|  [33]   | `add_entity(entity)`                                                  | adopt     | add a constructed entity                             |

[ENTRYPOINT_SCOPE]: multileader builders, hatch pattern fill, and arrow blocks
- rail: cad-export

`add_multileader_mtext(style)` / `add_multileader_block(style)` return a fluent builder — `render.MultiLeaderMTextBuilder` (mtext content) / `render.MultiLeaderBlockBuilder` (block content) — whose `set_content` / `add_leader_line(ConnectionSide, vertices)` / `set_connection_types` / `build(insert)` compose the leader before it lands on the layout, `render.mleader.ConnectionSide` (`left`/`right`/`top`/`bottom`) selecting the dogleg attachment side the `drawing/annotate` keynote/leader owner drives. `Hatch.set_pattern_fill` applies an ISO 128-50 pattern the `ezdxf.tools.pattern` module supplies (`load()` the full pattern-definition dict, `scale_pattern(pattern, factor, angle)` the ISO-scale transform, `ISO_PATTERN` the 172-entry ISO hatch-name table the `drawing/standard` hatch table selects from), and `ezdxf.ARROWS` carries the arrow-block name constants the `drawing/dimension`/`drawing/annotate` terminators reference.

| [INDEX] | [MEMBER]                                                         | [KIND]    | [ROLE]                                                    |
| :-----: | :--------------------------------------------------------------- | :-------- | :-------------------------------------------------------- |
|  [01]   | `render.MultiLeaderMTextBuilder`                                 | builder   | fluent mtext-content multileader builder                  |
|  [02]   | `render.MultiLeaderBlockBuilder`                                 | builder   | fluent block-content multileader builder                  |
|  [03]   | `render.mleader.ConnectionSide` (`left`/`right`/`top`/`bottom`)  | enum      | leader dogleg attachment side passed to `add_leader_line` |
|  [04]   | `Hatch.set_pattern_fill(name, color=7, angle=0.0, scale=1.0, …)` | fill      | pattern hatch fill (ISO 128-50 from `drawing/standard`)   |
|  [05]   | `Hatch.set_solid_fill` / `set_gradient`                          | fill      | solid / gradient hatch fill                               |
|  [06]   | `tools.pattern.load(measurement=1, factor=None)` / `ISO_PATTERN` | pattern   | ISO/imperial hatch-pattern definition table               |
|  [07]   | `tools.pattern.scale_pattern(pattern, factor=1, angle=0)`        | pattern   | ISO-scale pattern transform                               |
|  [08]   | `ezdxf.ARROWS` (`closed_filled`/`architectural_tick`/`dot`/…)    | constants | arrow-block name constants for terminators                |

[ENTRYPOINT_SCOPE]: the `Path` geometry bridge (`ezdxf.path`)
- rail: cad-export

`make_path(entity)` lifts any curve entity (Arc/Circle/Ellipse/Spline/LWPolyline/Polyline/Hatch boundary) into one `Path` command-segment object — the lossless intermediate between DXF geometry and the SVG `d`-path the `svgelements`/`graphic/vector` owners speak. The `render_*` rows are the inverse (a `Path` back to DXF entities), the `from_hatch*`/`to_*` rows convert between fills and paths, and the curve operations (`flattening`/`fillet`/`chamfer`/`triangulate`/`fit_paths_into_box`) are the native pipeline. This is the seam that lets a `skia-pathops` boolean result or an `svgelements.Path` cross into DXF and back without re-parsing a `d` string.

| [INDEX] | [MEMBER]                                                                    | [KIND]     | [ROLE]                                    |
| :-----: | :-------------------------------------------------------------------------- | :--------- | :---------------------------------------- |
|  [01]   | `path.make_path(entity, segments=1, level=4)`                               | lift       | any curve entity -> one `Path`            |
|  [02]   | `Path.move_to(p)`                                                           | build      | `MoveTo` command                          |
|  [03]   | `Path.line_to(p)`                                                           | build      | `LineTo` command                          |
|  [04]   | `Path.curve3_to(loc, ctrl)`                                                 | build      | `Curve3To` command                        |
|  [05]   | `Path.curve4_to(loc, ctrl1, ctrl2)`                                         | build      | `Curve4To` command                        |
|  [06]   | `Path.close()`                                                              | build      | close the sub-path                        |
|  [07]   | `Path.flattening(distance, segments=16)`                                    | flatten    | adaptive curve->`Vec3` vertices           |
|  [08]   | `Path.approximate(segments=20)`                                             | sample     | uniform-sampled vertices                  |
|  [09]   | `Path.control_vertices()`                                                   | sample     | control-vertex list                       |
|  [10]   | `Path.commands()`                                                           | sample     | raw command list                          |
|  [11]   | `Path.transform(m)`                                                         | transform  | apply a `Matrix44`                        |
|  [12]   | `Path.to_wcs(ocs, elevation)`                                               | transform  | OCS->WCS lift                             |
|  [13]   | `Path.reversed()` / `Path.clone()`                                          | transform  | reverse direction / copy                  |
|  [14]   | `path.from_hatch(hatch)`                                                    | convert    | hatch boundaries -> paths                 |
|  [15]   | `path.from_hatch_boundary_path(...)`                                        | convert    | one boundary -> a `Path`                  |
|  [16]   | `path.from_vertices(verts, close=False)`                                    | convert    | a vertex ring -> a `Path`                 |
|  [17]   | `path.render_lines` / `render_splines_and_polylines` / `render_lwpolylines` | render     | a `Path` iterable -> layout DXF entities  |
|  [18]   | `render_polylines2d` / `render_polylines3d`                                 | render     | -> 2D / 3D polylines on a layout          |
|  [19]   | `render_hatches` / `render_mpolygons`                                       | render     | -> hatches / mpolygons on a layout        |
|  [20]   | `path.to_lwpolylines` / `to_polylines2d` / `to_polylines3d`                 | emit       | paths -> virtual DXF entities (no layout) |
|  [21]   | `to_splines_and_polylines` / `to_hatches` / `to_mpolygons`                  | emit       | paths -> virtual DXF entities (no layout) |
|  [22]   | `path.fillet(points, radius)`                                               | corner     | filleted corner path                      |
|  [23]   | `path.chamfer(points, length)`                                              | corner     | chamfered corner path                     |
|  [24]   | `path.polygonal_fillet(...)`                                                | corner     | polygonal filleted corner path            |
|  [25]   | `path.triangulate(paths, max_sagitta=0.01)`                                 | tessellate | constrained triangulation                 |
|  [26]   | `path.make_polygon_structure(paths)`                                        | tessellate | nested-polygon hole structure             |
|  [27]   | `path.winding_deconstruction(...)`                                          | tessellate | winding hole deconstruction               |
|  [28]   | `path.fit_paths_into_box(paths, size, uniform=True)`                        | fit        | scale-to-fit a path set into a box        |
|  [29]   | `path.bbox(paths, fast=False)` / `precise_bbox(...)`                        | fit        | extents / precise extents of a path set   |
|  [30]   | `path.rect` / `ngon` / `star` / `gear` / `helix` / `unit_circle` / `wedge`  | shapes     | parametric construction paths             |

[ENTRYPOINT_SCOPE]: render drivers (`ezdxf.addons.drawing`)
- rail: cad-export

The in-process DXF->image/SVG/PDF pipeline. `Frontend(RenderContext(doc), backend, config).draw_layout(msp, finalize=True)` is the one render call; the backend determines the output (`SVGBackend.get_string()` for SVG, `PyMuPdfBackend.get_pdf_bytes()` for PDF, `qsave(...)` for a Matplotlib raster/PDF in one shot). `Configuration(...).with_changes(...)` tunes the policy; the policy bundle carries the closed enums `LineweightPolicy`/`HatchPolicy`/`TextPolicy`/`ColorPolicy`/`BackgroundPolicy`/`ProxyGraphicPolicy`/`LinePolicy`. No foreign renderer is admitted — this is how `export/dxf` previews and how a DXF figure lowers into the document/composition plane.

- call: `matplotlib.qsave(layout, filename, *, bg=None, fg=None, dpi=300, backend='agg', config=None, size_inches=None)` — layout to a PNG/PDF/SVG file in one Matplotlib call
- call: `Frontend(ctx, out, config=Configuration(), bbox_cache=None)` -> `.draw_layout(layout, finalize=True, filter_func=None, ...)` / `.draw_entities(entities)`
- call: `RenderContext(doc, ctb=None, export_mode=False)` -> `.set_current_layout(layout)` / `.resolve_all(entity)` / `.resolve_color(...)` / `.resolve_lineweight(...)`
- call: `SVGBackend().get_string(page, settings=Settings())` / `.get_xml_root_element(...)`
- call: `PyMuPdfBackend().get_pdf_bytes(page, settings=...)` / `.get_pixmap_bytes(page, fmt='png', dpi=...)`
- call: `layout.Page(width, height, units=Units.mm, margins=Margins(...), max_width=0, max_height=0)` / `layout.Settings(fit_page=True, scale=1.0, page_alignment=…, ...)`

| [INDEX] | [MEMBER]                                             | [KIND]    | [ROLE]                                                      |
| :-----: | :--------------------------------------------------- | :-------- | :---------------------------------------------------------- |
|  [01]   | `Frontend(...)`                                      | construct | render driver binding a context, backend, policy            |
|  [02]   | `Frontend.draw_layout` / `draw_entities`             | render    | walk a layout / an explicit entity set into the backend     |
|  [03]   | `RenderContext(...)`                                 | construct | property-resolution context (layers/linetypes/colors/fonts) |
|  [04]   | `RenderContext.set_current_layout` / `resolve_all`   | resolve   | bind a layout / resolve full pen properties                 |
|  [05]   | `RenderContext.resolve_color` / `resolve_lineweight` | resolve   | resolve per-axis pen properties                             |
|  [06]   | `SVGBackend.get_string` / `get_xml_root_element`     | backend   | native SVG string / lxml element tree                       |
|  [07]   | `PyMuPdfBackend.get_pdf_bytes` / `get_pixmap_bytes`  | backend   | PDF bytes / a raster (PNG/PSD/PPM) bytes                    |
|  [08]   | `matplotlib.qsave(...)`                              | one-shot  | layout -> PNG/PDF/SVG file via Matplotlib (call above)      |
|  [09]   | `CustomJSONBackend().get_json_data()`                | backend   | structured JSON geometry of the layout                      |
|  [10]   | `GeoJSONBackend().get_json_data()`                   | backend   | GeoJSON geometry of the layout                              |
|  [11]   | `layout.Page(...)`                                   | page      | output page model the SVG/PDF backends fill                 |
|  [12]   | `layout.Settings(...)`                               | page      | page placement/scale settings for the backends              |
|  [13]   | `config.Configuration(...)`                          | policy    | render-policy bundle + closed policy enums (in the lead)    |

[ENTRYPOINT_SCOPE]: query, selection, transform, extents
- rail: cad-export

The read-side surfaces. `doc.query("LINE[layer=='WALLS']")` and `groupby` are the entity-selection family (one polymorphic query string, never a `find`/`filter`/`get_by` proliferation); `select` is the spatial query (rtree-backed window/fence/point against a `BoundingBox`); `transform` applies affines in place or as copies; `bbox.extents` computes the model extents the composition sheet sizing needs.

| [INDEX] | [MEMBER]                                                           | [KIND]    | [ROLE]                                               |
| :-----: | :----------------------------------------------------------------- | :-------- | :--------------------------------------------------- |
|  [01]   | `Drawing.query(query='*')` / `Layout.query(query='*')`             | select    | DXF entity-query-language selection -> `EntityQuery` |
|  [02]   | `EntityQuery.query(q)` / `.filter(fn)`                             | refine    | chain a query / predicate-filter                     |
|  [03]   | `EntityQuery.groupby(dxfattrib='', key=None)`                      | refine    | group a selection                                    |
|  [04]   | `EntityQuery.union` / `.difference` / `.intersection`              | refine    | set-algebra over selections                          |
|  [05]   | `EntityQuery.layer(...)` / `.color(...)`                           | filter    | attribute-filter builders -> a refined `EntityQuery` |
|  [06]   | `EntityQuery.linetype(...)` / `.transparency(...)`                 | filter    | more attribute-filter builders                       |
|  [07]   | `groupby(entities, dxfattrib='', key=None)`                        | group     | group entities by a DXF attribute or key -> `dict`   |
|  [08]   | `select.bbox_inside(shape, entities)`                              | spatial   | window selection vs a `SelectionShape`               |
|  [09]   | `select.bbox_outside` / `bbox_overlap`                             | spatial   | crossing selection vs a `SelectionShape`             |
|  [10]   | `select.bbox_crosses_fence` / `point_in_bbox`                      | spatial   | fence / point selection                              |
|  [11]   | `select.PlanarSearchIndex(entities)`                               | spatial   | an rtree spatial index                               |
|  [12]   | `select.Window(p1, p2)` / `Circle(center, r)` / `Polygon(verts)`   | spatial   | the selection-shape vocabulary                       |
|  [13]   | `transform.inplace(entities, m)` / `transform.copies(entities, m)` | transform | apply a `Matrix44` in place / as copies              |
|  [14]   | `transform.translate` / `scale` / `axis_rotate` / `z_rotate`       | transform | convenience translate / scale / rotate               |
|  [15]   | `bbox.extents(entities, *, fast=False, cache=None)`                | extents   | model extents `BoundingBox` (cache-accelerated)      |
|  [16]   | `bbox.multi_recursive(...)` / `bbox.Cache()`                       | extents   | recursive extents / the extents cache                |

[ENTRYPOINT_SCOPE]: attributes, blocks, xref, import, geo, text-to-path, fast-write
- rail: cad-export

The boundary surfaces. `GfxAttribs` is the one attribute value object (the uniform `dxfattribs=` payload across the whole builder family); `xref` links external drawings as references or copies their content; `addons.Importer` copies entities/blocks/tables across documents; `addons.geo` round-trips DXF<->GeoJSON (the geospatial seam); `text2path` converts text to glyph-outline paths/hatches; `r12writer` is the streaming fast-writer for huge R12 output.

- call: `GfxAttribs(*, layer='0', color=256, rgb=None, linetype='ByLayer', lineweight=-1, transparency=None, ltscale=1.0)` — the graphic-attribute value object; `.asdict()` feeds any `dxfattribs=`
- call: `xref.attach(doc, *, block_name, filename, insert=(0,0,0), scale=1.0, rotation=0.0)` / `xref.define(doc, block_name, filename)`
- call: `xref.load_modelspace(sdoc, tdoc, filter_fn=None, conflict_policy=ConflictPolicy.KEEP)`
- call: `addons.Importer(source, target)` -> `.import_modelspace()` / `.import_entities(...)`

| [INDEX] | [MEMBER]                                                                          | [KIND]     | [ROLE]                                  |
| :-----: | :-------------------------------------------------------------------------------- | :--------- | :-------------------------------------- |
|  [01]   | `GfxAttribs(...)`                                                                 | attribs    | `dxfattribs=` value object (call above) |
|  [02]   | `GfxAttribs.from_entity(e)` / `.from_dict(d)`                                     | attribs    | attribs from an entity / a dict         |
|  [03]   | `GfxAttribs.load_from_header(doc)` / `.write_to_header(doc)`                      | attribs    | round-trip header attribute defaults    |
|  [04]   | `colors.RGB(r, g, b)` / `RGB.from_hex(s)` / `.to_hex()` / `colors.RGBA(...)`      | color      | true-color value objects                |
|  [05]   | `colors.aci2rgb(i)` / `int2rgb` / `rgb2int`                                       | color      | ACI<->RGB conversion                    |
|  [06]   | `doc.blocks.new(name)` / `doc.blocks.new_anonymous_block()`                       | block      | define a named / anonymous block        |
|  [07]   | `block.add_attdef(...)`                                                           | block      | populate a block via the `add_*` family |
|  [08]   | `xref.attach(...)` / `xref.define(...)`                                           | xref       | attach/define an external DXF reference |
|  [09]   | `xref.load_modelspace(...)`                                                       | xref       | copy modelspace across docs             |
|  [10]   | `xref.write_block(entities, origin=(0,0,0))` / `xref.embed(xref, ...)`            | xref       | extract to a new doc / bind an xref     |
|  [11]   | `addons.Importer(...)` -> `.import_modelspace()` / `.import_entities(...)`        | import     | cross-document entity copy (mapped)     |
|  [12]   | `Importer.import_blocks(...)` / `.import_tables(...)` / `.finalize()`             | import     | copy blocks/tables; finalize            |
|  [13]   | `addons.geo.proxy(entity)` / `GeoProxy.parse(geo_mapping)` / `.to_dxf_entities()` | geo        | DXF entity <-> GeoJSON round-trip       |
|  [14]   | `geo.gps_to_world_mercator`                                                       | geo        | WGS84 -> Mercator transform             |
|  [15]   | `addons.text2path.make_paths_from_str(s, font, ...)` / `make_path_from_entity(e)` | text2path  | text/entity -> outline `Path`s          |
|  [16]   | `text2path.make_hatches_from_str(...)` / `virtual_entities(...)`                  | text2path  | text -> `Hatch`es / virtual entities    |
|  [17]   | `addons.r12writer.r12writer(stream, fixed_tables=False, fmt='asc')`               | fast-write | streaming R12 writer, no in-memory doc  |
|  [18]   | `R12FastStreamWriter.add_line` / `add_circle` / `add_text` / `...`                | fast-write | the streaming entity writers            |
|  [19]   | `tools.text.MTextEditor()` -> `.append` / `.color` / `.font` / `.height`          | mtext      | fluent inline-formatted MText builder   |
|  [20]   | `MTextEditor.bullet_list` / `.stack` / `.underline`                               | mtext      | list / stacked-fraction / underline     |

## [04]-[IMPLEMENTATION_LAW]

- import: `import ezdxf` at boundary scope; the geometry kernel is `from ezdxf import math as ezmath` (aliased so it never shadows the stdlib `math`), the path bridge `from ezdxf import path as dxfpath`, the render stack `from ezdxf.addons.drawing import Frontend, RenderContext`, the chosen backend `from ezdxf.addons.drawing.svg import SVGBackend` (or `.matplotlib`/`.pymupdf`/`.json`). The version constant is `ezdxf.__version__` / `ezdxf.VERSION`; the DXF-version literals are `ezdxf.DXF2018` etc.
- document axis: `ezdxf.new` is the single construction factory (target version + optional `setup`), and `readfile`/`read`/`readzip`/`decode_base64` are the polymorphic ingestion family discriminating on source shape — never a per-version reader type. A damaged third-party file routes to `recover.readfile` (the ONLY correct loader for non-conforming DXF; it returns `(doc, auditor)` so the boundary inspects salvage errors), never the conforming `readfile`. `audit()` runs before `saveas` so a structurally invalid model never reaches disk.
- layout axis: the `Modelspace`/`Paperspace`/`BlockLayout` IS the `GraphicsFactory` — every drawable is minted by an `add_*` builder on the layout, never by constructing an entity class and inserting tags. A reusable symbol is one `doc.blocks.new(name)` block populated by the same builder family and placed by `add_blockref` (n placements of one definition), never a per-placement geometry copy.
- attribute axis: `GfxAttribs(layer=, color=, rgb=, linetype=, lineweight=, transparency=, ltscale=)` is the one attribute value object across the WHOLE builder family — every `add_*` takes the same `dxfattribs=GfxAttribs(...).asdict()` payload, so layer/color/linetype application is one uniform axis, never a per-entity `set_layer`/`set_color` setter. True color is `colors.RGB`, indexed color is the ACI int (1-255, 256=ByLayer, 0=ByBlock) the `colors` module names (`colors.RED` etc.).
- path axis: `path.make_path(entity)` is the single polymorphic lift of any curve entity into one `Path` command-segment object (`MoveTo`/`LineTo`/`Curve3To`/`Curve4To`), the lossless bridge to the SVG `d`-path grammar; the inverse is the `render_*` family (a `Path` iterable back onto a layout) and the `to_*` family (virtual entities). Curve flattening (`Path.flattening(distance)`), filleting/chamfering, triangulation, and scale-to-fit (`fit_paths_into_box`) are the native pipeline — flatten/fit through the `Path` owner, never re-sample a coordinate list by hand. A `skia-pathops`/`svgelements.Path` result crosses in through `from_vertices`/`make_path` and back through `render_lines`, never a re-parsed string.
- geometry axis: `ezdxf.math` is the geometry kernel — `Vec3`/`Vec2` for vectors (`cross`/`dot`/`normalize`/`lerp`/`rotate`), `Matrix44` for affines (`translate`/`scale`/`*_rotate`/`chain`/`inverse`/`transform_vertices`), `BSpline`/`Bezier4P` for curves, `OCS`/`UCS` for coordinate-system transforms, `BoundingBox` for extents, and the `Construction*` analytic primitives for intersection/tangent/offset math. `Vec3.list(...)`/`v.xyz` round-trips to plain tuples for a `numpy` array or a `msgspec` struct — never re-derive an affine, a spline evaluator, or an OCS transform.
- render axis: `Frontend(RenderContext(doc), backend, config).draw_layout(layout, finalize=True)` is the one render call; the backend selects the output (`SVGBackend.get_string()`, `PyMuPdfBackend.get_pdf_bytes()`/`get_pixmap_bytes()`, `qsave(...)` for Matplotlib), and `Configuration(...).with_changes(...)` plus the closed policy enums (`LineweightPolicy`/`HatchPolicy`/`TextPolicy`/`ColorPolicy`/`BackgroundPolicy`) govern behavior. The `layout.Page`/`Settings`/`Margins` model places content at a real page size/units. No foreign DXF renderer is admitted — ezdxf renders its own format.
- query axis: `doc.query("LINE CIRCLE[layer=='WALLS']")` is the entity-query-language selection (the single polymorphic query string), refined by `EntityQuery.query`/`filter`/`groupby` and the set-algebra (`union`/`difference`/`intersection`) — never a `get`/`get_many`/`find_by_layer` family. Spatial selection is `select` (rtree-backed `Window`/`Circle`/`Polygon`/fence against a `PlanarSearchIndex`); attribute grouping is `groupby`; affine application is `transform.inplace`/`copies`; model extents are `bbox.extents`.
- evidence: each DXF op captures the dxfversion, the entity count by type, the model `BoundingBox` extents (`bbox.extents`), the layer/block roster, the `audit()` error+fix counts, the units, and the output byte length as a CAD-export receipt; a render op additionally captures the backend, the page size/units, and the rendered byte length.
- boundary: ezdxf owns DXF read/write/recover, the graphic-entity vocabulary, the `Path` geometry algebra, the `ezdxf.math` kernel, and the DXF render frontend. The IFC semantic model stays `csharp:Rasm.Bim` (ezdxf never authors IFC). SVG geometry shared with the figure rail meets `svgelements`/`graphic/vector` at the `Path`/`d`-string wire; raster output beyond the bundled backends routes to `pyvips`/`pillow`; PDF page assembly stays `pymupdf`/`pikepdf`; font shaping stays `fonttools`/`uharfbuzz` (text2path composes `fonttools`, it does not re-shape); the AEC standards vocabularies (ISO 128/129-1/3098/13567 line types, dimensions, lettering, layer names) stay the `drawing/*` owned-vocabulary owners that LOWER onto ezdxf tables/entities; sheet placement and scale stay the `composition/sheet` owner; live UI stays outside this package.

[STACKING]:
- `expression` Result/`beartype`: the `export/dxf` owner wraps `recover.readfile`/`audit()`/`saveas` in a `Result[Drawing, DxfFault]` rail — a salvage with auditor errors folds to a typed `DxfFault.recovered(errors, fixes)` case, an audit failure to `DxfFault.invalid(...)`, never a raised `DXFStructureError` crossing the domain; the boundary signature is `@beartype`-validated so a wrong-shaped `dxfattribs` is rejected at the seam, not deep in the tag writer.
- `msgspec`/`pydantic` discriminated receipt: the DXF facts (`doc.dxfversion`, the `groupby('dxftype')` count map, `bbox.extents(...)` as a `Vec3`-tuple AABB, the `doc.layers`/`doc.blocks` roster, the `Auditor` error+fix counts, the render backend + page size + byte length) populate one `ArtifactReceipt` CAD-export case — a `msgspec.Struct`/`pydantic` model recording typed scalars, contributed through the existing `core/receipt#RECEIPT` `ReceiptContributor` port as ONE more case on the tagged union, never a parallel DXF receipt rail.
- `numpy`: `Path.flattening(d)`/`make_path(e).control_vertices()` and `Vec3.list(verts)` produce vertex sequences that feed `numpy.asarray(...)` for the `graphic/vector`/`geometry` numeric lane (offset, simplify, convex hull) — the array substrate is the shared `libs/python/.api/numpy.md` owner ezdxf already depends on, so a path crosses into the numeric rail and the `render_lines` result crosses back, one array contract, no second copy.
- `svgelements` (folder `.api`): a QR/chart/figure `svgelements.Path` `d`-string crosses into DXF through `dxfpath.from_vertices([p for p in svg_path.as_points()])` + `render_lines(msp, [dxf_path])`, and a DXF curve crosses out through `make_path(entity).flattening(distance)` (or `.control_vertices()` for the exact NURBS frame) -> a `Vec3` vertex sequence the figure composer rebuilds as an `svgelements.Path` and transforms by `svgelements.Matrix` — the two path owners meet at the vertex/`d`-string wire, neither re-implementing the other's geometry (`ezdxf.path.Path` carries `move_to`/`line_to`/`curve3_to`/`curve4_to` and `flattening`/`control_vertices`, not the `svgelements.Path.as_points` method).
- `svg` render backend + `pymupdf`/`pyvips`: `SVGBackend.get_string(...)` produces an SVG the `graphic/vector` owner composites and `resvg-py`/`vl-convert` rasterizes, while `PyMuPdfBackend.get_pdf_bytes(...)` produces a one-page PDF the `composition/imposition`/`composition/sheet` owner places into a sheet set — so a DXF figure lowers into the document/composition plane through the existing raster/page owners, never a new renderer.
- `anyio` structured concurrency + `stamina`/`structlog`/OpenTelemetry: a batch DXF render or recover crosses one `anyio.to_thread.run_sync` worker seam (the GIL-releasing Matplotlib/PyMuPDF native render off the event loop) under a `CapacityLimiter`, retried by `stamina` on a transient IO fault, with the per-op evidence emitted as a `structlog` event inside an OpenTelemetry span — the same shared observability rails (`libs/python/.api/{anyio,stamina,structlog,opentelemetry-api}.md`) every artifacts owner stacks, ezdxf's render call slotting in as the worker body.
- `addons.geo` <-> `data/tabular`/geospatial seam: `addons.geo.proxy(entity)` -> a GeoJSON mapping and `GeoProxy.parse(...).to_dxf_entities()` round-trip DXF geometry against the geospatial frame, so a georeferenced site plan crosses the same GeoJSON wire the geospatial owners read, ezdxf holding only the DXF<->GeoJSON conversion, never the CRS authority.

## [05]-[LOCAL_ADMISSION]

- Package: `ezdxf`
- Owns: DXF R12->R2018 read/write/recover, the full graphic-entity builder vocabulary (line/arc/circle/ellipse/spline/polyline/hatch/text/mtext/leader/multileader/all dimension kinds/block-ref/mesh/solid/image/wipeout/primitives), the `ezdxf.path.Path` command-segment algebra with curve flattening + DXF<->SVG<->hatch conversion + tessellation, the `ezdxf.math` geometry kernel (vectors/matrices/NURBS/Bezier/OCS/UCS/bounding-box/construction primitives), the `addons.drawing` render frontend over Matplotlib/SVG/PDF/JSON/GeoJSON backends with full property + policy resolution, symbol-table/layer/linetype/textstyle/dimstyle authoring, block + xref management, spatial `select`, affine `transform`, `query`/`groupby` selection, `bbox` extents, GeoJSON `addons.geo`, cross-document `addons.Importer`, `text2path` outline conversion, and the `r12writer` streaming writer
- Accept: DXF ingestion and salvage, drawing/dimension/annotation/symbol/schedule geometry emission (the `drawing/*` AEC vocabularies lowering onto ezdxf tables + entities), DXF figure rendering into the document/composition plane (SVG/PDF/PNG preview), DXF<->SVG path exchange with the figure/vector rail, the geometry-kernel wire (`Vec3`/`Matrix44` to numpy + the C# `Rasm` geometry seam), and DXF<->GeoJSON round-trip for georeferenced plans
- Reject: a hand-assembled DXF tag stream where the `add_*` builder family + `GfxAttribs` exist; a per-entity attribute setter where the uniform `dxfattribs=` axis applies; a re-implemented affine/B-spline/OCS transform where `ezdxf.math` owns it; a re-parsed SVG `d`-string where `make_path`/`render_lines` bridge the geometry; a foreign DXF renderer where the `Frontend`+backend family renders; a `find`/`get_by_layer`/`filter` query family where `doc.query(...)`/`groupby` discriminate; the conforming `readfile` on a damaged file where `recover.readfile` is the correct salvage path; IFC semantic authoring the `csharp:Rasm.Bim` owner holds; the AEC standards vocabularies (ISO line types/dimensions/lettering/layer codec) the `drawing/*` owners hold; sheet placement/scale the `composition/sheet` owner holds; identity minting the runtime owns
