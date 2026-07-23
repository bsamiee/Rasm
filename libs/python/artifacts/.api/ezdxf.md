# [PY_ARTIFACTS_API_EZDXF]

`ezdxf` owns DXF read/write/render for the artifacts cad-export rail: a `Drawing` document graph authored through one `GraphicsFactory.add_*` builder family, the `ezdxf.path.Path` command-segment algebra bridging DXF curves to the SVG path the figure owners speak, the `ezdxf.math` kernel the geometry seam reads, and the `addons.drawing` `Frontend`+backend family rasterizing any DXF to SVG/PDF/PNG/JSON in-process. It re-implements neither the DXF grammar, OCS/WCS transform, B-spline evaluator, nor DXF-to-SVG conversion it owns, and authors neither the IFC model (`csharp:Rasm.Bim`) nor the placement the composition owners hold.

## [01]-[PACKAGE_SURFACE]

- package: `ezdxf` (MIT)
- import: `ezdxf`
- namespaces: `ezdxf`, `ezdxf.math`, `ezdxf.path`, `ezdxf.addons.drawing`, `ezdxf.addons`
- owner: `artifacts`
- rail: cad-export
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and layout roots

`Drawing` is the single document owner; `modelspace()`/`paperspace(name)` return `Layout` views that ARE the `GraphicsFactory` — every `add_*` builder lives on the layout, and a `BlockLayout` is the reusable-geometry container an `Insert` references, never a parallel reader/writer pair.

| [INDEX] | [TYPE]        | [KIND]        | [ROLE]                                                                    |
| :-----: | :------------ | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `Drawing`     | document root | the whole DXF model; `modelspace`/`paperspace`/`blocks`/`tables`/`saveas` |
|  [02]   | `Modelspace`  | layout        | the model layout; a `GraphicsFactory` — every `add_*` builder + `query`   |
|  [03]   | `Paperspace`  | layout        | a print layout; same builder family + `page_setup` + `add_viewport`       |
|  [04]   | `BlockLayout` | block         | a reusable block definition the `Insert` entity references                |
|  [05]   | `EntityDB`    | store         | handle->entity database; `doc.entitydb` resolves any `#handle`            |
|  [06]   | `GfxAttribs`  | value object  | the shared graphic-attribute bundle (`layer`/`color`/`rgb`/`linetype`/…)  |

[PUBLIC_TYPE_SCOPE]: graphic-entity vocabulary

Every drawable derives `DXFGraphic` with a typed `.dxf` group-code namespace; builder-family construction on the layout is the only admitted path — the closed entity grammar `export/dxf` emits and the render `Frontend` consumes, never hand-assembled tags.

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

Symbol tables carry the named resources entities reference; each `.add(name, dxfattribs=)` mints an entry, and the `drawing/standard` AEC vocabularies lower onto them (ISO 128 line types onto `Linetype`, ISO 3098 text heights onto `Textstyle`, the layer-name codec onto `Layer`).

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

Pure geometry value objects — the `Vec3`/`Matrix44` shapes the `geometry/*` and C# `Rasm` geometry seam exchange at the wire; `Vec3.list(...)`/`v.xyz` round-trips to plain tuples a `numpy` array or `msgspec` struct records, so no affine or B-spline evaluator is re-derived.

| [INDEX] | [TYPE]                                     | [KIND]       | [ROLE]                                                                  |
| :-----: | :----------------------------------------- | :----------- | :---------------------------------------------------------------------- |
|  [01]   | `Vec3` / `Vec2`                            | vector       | 3D/2D vector; `cross`/`dot`/`normalize`/`lerp`/`rotate`/`angle_between` |
|  [02]   | `Matrix44`                                 | affine       | `translate`/`scale`/`*_rotate`/`chain`/`inverse`/`transform_vertices`   |
|  [03]   | `BSpline`                                  | spline       | NURBS curve; `point`/`flattening`/`derivative`/`insert_knot`/`measure`  |
|  [04]   | `Bezier4P` / `Bezier3P` / `Bezier`         | bezier       | cubic/quadratic/arbitrary Bezier curve evaluators                       |
|  [05]   | `OCS` / `UCS`                              | coord sys    | object/user coordinate system <-> WCS transform                         |
|  [06]   | `BoundingBox` / `BoundingBox2d`            | extents      | 3D/2D AABB; `extend`/`union`/`inside`/`intersection`/`size`/`center`    |
|  [07]   | `BoundingBox.extmin`/`extmax`/`has_data`   | extents      | corner points + emptiness evidence the extents receipt reads            |
|  [08]   | `ConstructionArc` / `ConstructionCircle`   | construction | analytic arc / circle                                                   |
|  [09]   | `ConstructionEllipse` / `ConstructionLine` | construction | analytic ellipse / line                                                 |
|  [10]   | `ConstructionRay` / `ConstructionPolyline` | construction | analytic primitives for intersection/tangent/offset math                |
|  [11]   | `Plane` / `Shape2d`                        | helper       | plane algebra; 2D shape transform                                       |
|  [12]   | `EulerSpiral` / `ApproxParamT`             | helper       | clothoid; arc-length reparam                                            |

[PUBLIC_TYPE_SCOPE]: render frontend + backends (`ezdxf.addons.drawing`)

`Frontend(ctx, backend, config)` walks any layout and drives the chosen `BackendInterface`; `RenderContext` resolves entity attributes into concrete pen properties through the full layer/linetype/lineweight/color/transparency stack, and `Configuration` and the policy enums govern behavior — how `export/dxf` produces an SVG/PDF/PNG preview and `visualization/diagram` rasterizes a DXF figure into the document.

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

`ezdxf.new` mints a document at a target version with optional standard-resource `setup`; `readfile`/`read`/`readzip`/`decode_base64` are the polymorphic ingestion family, `recover.readfile` the salvage path for damaged non-conforming files, and `saveas`/`write`/`encode_base64` the egress family with `audit()` validating before save — one document owner, no per-version reader subtype.

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

One construction surface for every drawable, all on the layout (`msp.add_*`); every builder takes the uniform `dxfattribs=` payload (or a `GfxAttribs.asdict()`), and each `add_*_dim` returns a `DimStyleOverride` whose `.render()` generates the dimension geometry — the dense owner `export/dxf`, `drawing/*`, and `visualization/diagram` emit through.

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
|  [23]   | `add_mesh` / `add_3dface` / `add_body` / `add_region` / `add_3dsolid` | 3d        | mesh / 3D face / ACIS body / region / 3D solid       |
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

[ENTRYPOINT_SCOPE]: entity composition — multileader builders, hatch fills, text placement, mesh editing, and arrow blocks

`add_multileader_mtext(style)`/`add_multileader_block(style)` return a fluent `MultiLeaderMTextBuilder`/`MultiLeaderBlockBuilder`, `render.mleader.ConnectionSide` selecting the dogleg side the `drawing/annotate` leader owner drives. `Hatch.set_pattern_fill` applies an ISO 128-50 pattern from `tools.pattern` (the `ISO_PATTERN` table `drawing/standard` selects from), `ezdxf.ARROWS` carries the arrow-block terminators `drawing/dimension`/`drawing/annotate` reference, and `layout.new_entity("TOLERANCE", ...)` authors the ISO 1101 feature-control frame.

| [INDEX] | [MEMBER]                                           | [KIND]    | [ROLE]                                                          |
| :-----: | :------------------------------------------------- | :-------- | :-------------------------------------------------------------- |
|  [01]   | `render.MultiLeaderMTextBuilder`                   | builder   | fluent mtext-content multileader builder                        |
|  [02]   | `render.MultiLeaderBlockBuilder`                   | builder   | fluent block-content multileader builder                        |
|  [03]   | `render.mleader.ConnectionSide`                    | enum      | leader dogleg attachment side passed to `add_leader_line`       |
|  [04]   | `Hatch.set_pattern_fill(name, …)`                  | fill      | pattern hatch fill (ISO 128-50 from `drawing/standard`)         |
|  [05]   | `Hatch.set_solid_fill` / `set_gradient`            | fill      | solid / gradient hatch fill                                     |
|  [06]   | `tools.pattern.load(…)` / `ISO_PATTERN`            | pattern   | ISO/imperial hatch-pattern definition table                     |
|  [07]   | `tools.pattern.scale_pattern(…)`                   | pattern   | ISO-scale pattern transform                                     |
|  [08]   | `ezdxf.ARROWS`                                     | constants | arrow-block names for `dimblk`/`dimldrblk` terminators          |
|  [09]   | `layout.new_entity("TOLERANCE", dxfattribs={...})` | gd&t      | ISO 1101 feature-control frame authored via `content`           |
|  [10]   | `Insert.add_auto_attribs(values: dict[str, str])`  | reference | fill a placed blockref's `ATTRIB`s from its `add_attdef` tags   |
|  [11]   | `doc.mleader_styles.new(name)`                     | annot     | mint a named `MLeaderStyle` the multileader builders reference  |
|  [12]   | `MLeaderStyle.set_mtext_style(name)`               | annot     | bind the style's text style by name                             |
|  [13]   | `MLeaderStyle.set_leader_properties(…)`            | annot     | style-level `color=`/`linetype=`/`lineweight=`/`leader_type=`   |
|  [14]   | `Text.set_placement(p1, p2=None, align=…)`         | text      | place TEXT via `TextEntityAlignment`; `ALIGNED`/`FIT` take `p2` |
|  [15]   | `Hatch.paths.add_polyline_path(…)`                 | fill      | append one `(x, y[, bulge])` loop onto `BoundaryPaths`          |
|  [16]   | `Mesh.edit_data()`                                 | 3d        | context-managed `vertices`/`faces` mesh editing                 |
|  [17]   | `Polymesh.set_mesh_vertex(pos, point)`             | 3d        | polymesh grid-vertex population                                 |
|  [18]   | `Polyface.append_faces(faces)`                     | 3d        | polyface face population                                        |

[ENTRYPOINT_SCOPE]: the `Path` geometry bridge (`ezdxf.path`)

`make_path(entity)` lifts any curve entity into one `Path` command-segment object — the lossless intermediate between DXF geometry and the SVG `d`-path the `svgelements`/`graphic/vector` owners speak; the `render_*` rows are the inverse onto a layout, the `from_hatch*`/`to_*` rows convert between fills and paths, and the curve operations (`flattening`/`fillet`/`chamfer`/`triangulate`/`fit_paths_into_box`) are the native pipeline, so a `skia-pathops` boolean or an `svgelements.Path` crosses into DXF and back without re-parsing a `d` string.

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

`Frontend(RenderContext(doc), backend, config).draw_layout(msp, finalize=True)` is the one render call; the backend determines output (`SVGBackend.get_string()`, `PyMuPdfBackend.get_pdf_bytes()`, `qsave(...)` for a one-shot Matplotlib raster/PDF), and `Configuration(...).with_changes(...)` tunes the closed policy enums — no foreign renderer admitted, how `export/dxf` previews and a DXF figure lowers into the document/composition plane.

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

`doc.query("LINE[layer=='WALLS']")` and `groupby` are the entity-selection family (one polymorphic query string, never a `find`/`filter`/`get_by` proliferation); `select` is the rtree-backed spatial query, `transform` applies affines in place or as copies, and `bbox.extents` computes the model extents composition sheet sizing needs.

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

Boundary surfaces: `GfxAttribs` is the one attribute value object across the whole builder family; `xref` links or copies external drawings, `addons.Importer` copies entities/blocks/tables across documents, `addons.geo` round-trips DXF<->GeoJSON, `text2path` converts text to glyph-outline paths/hatches, and `r12writer` streams huge R12 output with no in-memory doc.

| [INDEX] | [MEMBER]                                                           | [KIND]     | [ROLE]                                                 |
| :-----: | :----------------------------------------------------------------- | :--------- | :----------------------------------------------------- |
|  [01]   | `GfxAttribs(...)`                                                  | attribs    | `dxfattribs=` value object (call above)                |
|  [02]   | `GfxAttribs.from_entity(e)` / `.from_dict(d)`                      | attribs    | attribs from an entity / a dict                        |
|  [03]   | `GfxAttribs.load_from_header(doc)` / `.write_to_header(doc)`       | attribs    | round-trip header attribute defaults                   |
|  [04]   | `colors.RGB` / `RGB.from_hex` / `.to_hex()` / `colors.RGBA`        | color      | true-color value objects                               |
|  [05]   | `colors.aci2rgb(i)` / `int2rgb` / `rgb2int`                        | color      | ACI<->RGB conversion                                   |
|  [06]   | `doc.blocks.new(name)` / `doc.blocks.new_anonymous_block()`        | block      | define a named / anonymous block                       |
|  [07]   | `block.add_attdef(...)`                                            | block      | populate a block via the `add_*` family                |
|  [08]   | `xref.attach(...)` / `xref.define(...)`                            | xref       | attach/define an external DXF reference                |
|  [09]   | `xref.load_modelspace(...)`                                        | xref       | copy modelspace across docs                            |
|  [10]   | `xref.write_block(entities, origin=(0,0,0))` / `xref.embed(...)`   | xref       | extract to a new doc / bind an xref                    |
|  [11]   | `addons.Importer(...)`                                             | import     | cross-document entity copy (mapped)                    |
|  [12]   | `Importer.import_blocks(...)` / `.import_tables(...)`              | import     | copy blocks/tables; finalize                           |
|  [13]   | `addons.geo.proxy(entity)` / `GeoProxy.parse(...)`                 | geo        | DXF <-> GeoJSON; `__geo_interface__` emits the mapping |
|  [14]   | `geo.gps_to_world_mercator`                                        | geo        | WGS84 -> Mercator transform                            |
|  [15]   | `make_paths_from_str(...)` / `make_path_from_entity(e)`            | text2path  | text/entity -> outline `Path`s                         |
|  [16]   | `text2path.make_hatches_from_str(...)` / `virtual_entities(...)`   | text2path  | text -> `Hatch`es / virtual entities                   |
|  [17]   | `r12writer.r12writer(stream, fixed_tables=False, fmt='asc')`       | fast-write | streaming R12 writer, no in-memory doc                 |
|  [18]   | `R12FastStreamWriter.add_line` / `add_circle` / `add_text` / `...` | fast-write | the streaming entity writers                           |
|  [19]   | `tools.text.MTextEditor()`                                         | mtext      | fluent inline-formatted MText builder (call above)     |
|  [20]   | `MTextEditor.bullet_list` / `.stack` / `.underline`                | mtext      | list / stacked-fraction / underline                    |
|  [21]   | `Drawing.add_image_def(filename, size_in_pixel, name=None)`        | raster def | image definition an `add_image` placement references   |
|  [22]   | `Drawing.add_underlay_def(filename, fmt='ext', name=None)`         | raster def | PDF/DWF/DGN underlay definition (`fmt` from extension) |
|  [23]   | `fonts.fonts.FontFace(filename, family, style, weight, width)`     | text2path  | the font descriptor `make_paths_from_str` takes        |
|  [24]   | `acis.api.load(data)`                                              | ACIS       | parse SAT records or SAB bytes into ACIS bodies        |
|  [25]   | `acis.api.export_dxf(entity, bodies)`                              | ACIS       | bind parsed bodies to an ACIS DXF entity               |

## [04]-[IMPLEMENTATION_LAW]

- import: `import ezdxf` at boundary scope; the geometry kernel imports `from ezdxf import math as ezmath` (aliased so it never shadows the stdlib `math`), the path bridge as `dxfpath`, the render stack from `ezdxf.addons.drawing`, and `ezdxf.DXF2018` and its peers carry the DXF-version literals.
- document axis: `ezdxf.new` is the single construction factory and `readfile`/`read`/`readzip`/`decode_base64` the polymorphic ingestion family discriminating on source shape, never a per-version reader. A damaged file routes to `recover.readfile` (returning `(doc, auditor)` for salvage inspection), never the conforming `readfile`; `audit()` runs before `saveas` so no structurally invalid model reaches disk.
- layout axis: the `Modelspace`/`Paperspace`/`BlockLayout` IS the `GraphicsFactory` — every drawable is an `add_*` builder on the layout, never a constructed entity class. A reusable symbol is one `doc.blocks.new(name)` block placed by `add_blockref`, never a per-placement geometry copy.
- attribute axis: `GfxAttribs(...).asdict()` is the one attribute payload across the whole builder family, so layer/color/linetype application is one uniform axis, never a per-entity setter. True color is `colors.RGB`, indexed color the ACI int (`256`=ByLayer, `0`=ByBlock) the `colors` module names.
- path axis: `path.make_path(entity)` is the single polymorphic lift into one `Path` command-segment object, the lossless bridge to the SVG `d`-path grammar; the `render_*` family is the inverse onto a layout and `to_*` emits virtual entities. Flatten/fillet/triangulate through the `Path` owner, never a hand-sampled coordinate list; a `skia-pathops`/`svgelements.Path` result crosses in through `from_vertices`/`make_path` and back through `render_lines`.
- geometry axis: `ezdxf.math` is the geometry kernel — `Vec3`/`Vec2`, `Matrix44`, `BSpline`/`Bezier4P`, `OCS`/`UCS`, `BoundingBox`, and the `Construction*` analytic primitives; `Vec3.list(...)`/`v.xyz` round-trips to tuples for a `numpy` array or `msgspec` struct, never a re-derived affine, spline evaluator, or OCS transform.
- render axis: `Frontend(RenderContext(doc), backend, config).draw_layout(layout, finalize=True)` is the one render call; the backend selects output and `Configuration(...).with_changes(...)` and the closed policy enums govern behavior, `layout.Page`/`Settings`/`Margins` placing content at a real page size. No foreign DXF renderer is admitted.
- query axis: `doc.query("LINE CIRCLE[layer=='WALLS']")` is the entity-query-language selection refined by `EntityQuery.query`/`filter`/`groupby` and set-algebra, never a `get`/`find_by_layer` family. Spatial selection is `select` (rtree `Window`/`Circle`/`Polygon`/fence), grouping `groupby`, affines `transform.inplace`/`copies`, extents `bbox.extents`.
- evidence: each DXF op captures the dxfversion, per-type entity count, model `BoundingBox` extents, layer/block roster, `audit()` error+fix counts, units, and output byte length as a cad-export receipt; a render op adds the backend, page size/units, and rendered byte length.
- boundary: IFC semantic modeling stays `csharp:Rasm.Bim`; SVG geometry meets `svgelements`/`graphic/vector` at the `Path`/`d`-string wire; raster beyond the bundled backends routes to `pyvips`/`pillow`, PDF assembly to `pymupdf`/`pikepdf`, font shaping to `fonttools`/`uharfbuzz`; the AEC standards vocabularies lower from the `drawing/*` owners onto ezdxf tables, and sheet placement/scale stays `composition/sheet`.

[STACKING]:
- `expression`(`libs/python/.api/expression.md`): `Dxf.of` returns `Result[Dxf, DxfFault]`; `DxfFault.source`/`scalar`/`geometry`/`selection`/`document` close admission failures, the runtime lane converting provider raises after admission.
- `msgspec`(`libs/python/.api/msgspec.md`): `DxfEntity`/`DxfDocument`/`DxfOp`/`DxfComposed` are frozen structs or tagged unions, and `ArtifactReceipt.Cad` receives version, units, kind, byte length, layer/block counts, audit counts, and entity census without importing producer models.
- `numpy`(`libs/python/.api/numpy.md`): `Path.flattening(d)`/`make_path(e).control_vertices()` and `Vec3.list(verts)` feed `numpy.asarray(...)` for the `graphic/vector`/`geometry` numeric lane, `render_lines` crossing the result back — one shared array contract, no second copy.
- `svgelements`(`.api/svgelements.md`): a figure `svgelements.Path` `d`-string crosses into DXF through `dxfpath.from_vertices([...])` + `render_lines`, and a DXF curve crosses out through `make_path(entity).flattening(distance)` (or `.control_vertices()` for the exact NURBS frame) into a `Vec3` sequence the figure composer rebuilds — the two path owners meet at the vertex/`d`-string wire, neither re-implementing the other's geometry.
- `resvg-py`/`pymupdf`(`.api/resvg-py.md`, `.api/pymupdf.md`): `SVGBackend.get_string(...)` produces an SVG the `graphic/vector` owner composites and `resvg-py`/`vl-convert` rasterizes, while `PyMuPdfBackend.get_pdf_bytes(...)` produces a one-page PDF the `composition/imposition`/`composition/sheet` owner places — a DXF figure lowers into the document plane through the existing raster/page owners.
- `anyio`(`libs/python/.api/anyio.md`)/`stamina`(`libs/python/runtime/.api/stamina.md`): `LanePolicy.offload(Kernel.of(_composed, KernelTrait.RELEASING), op)` owns capacity, cancellation, and boundary-fault conversion for rendering and recovery, worker-death retry riding the kernel's trait row.
- `addons.geo`: `addons.geo.proxy(entity)` and `GeoProxy.parse(...).to_dxf_entities()` round-trip DXF geometry against the geospatial frame, so a georeferenced site plan crosses the same GeoJSON wire the geospatial owners read — ezdxf holding only the DXF<->GeoJSON conversion, never the CRS authority.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ezdxf`
- Owns: DXF R12->R2018 read/write/recover, the graphic-entity builder vocabulary, the `ezdxf.path.Path` command-segment algebra, the `ezdxf.math` geometry kernel, the `addons.drawing` render frontend over its backends, symbol-table/block/xref authoring, spatial `select`, affine `transform`, `query`/`groupby` selection, `bbox` extents, `addons.geo` round-trip, cross-document `addons.Importer`, `text2path` outline conversion, and the `r12writer` streaming writer
- Accept: DXF ingestion and salvage, drawing/dimension/annotation/symbol geometry emission (the `drawing/*` AEC vocabularies lowering onto ezdxf tables + entities), DXF figure rendering into the document/composition plane, DXF<->SVG path exchange with the figure rail, the geometry-kernel wire to numpy and the C# `Rasm` seam, and DXF<->GeoJSON round-trip for georeferenced plans
- Reject: a hand-assembled DXF tag stream where the `add_*` family + `GfxAttribs` exist; a per-entity attribute setter where the uniform `dxfattribs=` axis applies; a re-implemented affine/B-spline/OCS transform where `ezdxf.math` owns it; a re-parsed SVG `d`-string where `make_path`/`render_lines` bridge; a foreign DXF renderer where the `Frontend`+backend family renders; a `find`/`get_by_layer`/`filter` query family where `doc.query`/`groupby` discriminate; the conforming `readfile` on a damaged file where `recover.readfile` is correct; IFC semantic authoring the `csharp:Rasm.Bim` owner holds; identity minting the runtime owns
