# [PY_ARTIFACTS_API_DRAWSVG]

`drawsvg` mints the programmatic SVG-authoring surface for the figure rail: a `Drawing` canvas over the drawable-element vocabulary, typed `Path` command builders, `<defs>`-tier paint/effect owners, the SMIL animation algebra, and SVG/HTML/`data:`-URI serialization, never hand-emitting XML nor re-implementing the `d` grammar. Rasterization routes downstream — the raster/video egress binds optional `cairoSVG`/`imageio`/ffmpeg extras absent on core, so PNG/PDF crosses to `resvg-py`/`vl-convert`/`pyvips` and ffmpeg video rides the runtime `to_process.run_sync` seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `drawsvg`
- package: `drawsvg` (MIT)
- import: `drawsvg`
- owner: `artifacts`
- rail: figure
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document canvas and element base hierarchy

Every drawable derives `DrawingElement` — the base algebra a figure op dispatches over, reading append and `bbox` semantics off the base, never a per-shape special case.

| [INDEX] | [TYPE]                 | [KIND]         | [ROLE]                                                                  |
| :-----: | :--------------------- | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `Drawing`              | document root  | canvas with `width`/`height`/`origin`; append/draw children, serialize  |
|  [02]   | `DrawingElement`       | base           | abstract element root; SVG serialization protocol                       |
|  [03]   | `DrawingBasicElement`  | leaf base      | attribute-bearing leaf; `append_anim`/`add_key_frame`/`append_title`    |
|  [04]   | `DrawingParentElement` | container base | holds children; `append`/`draw`/`extend` with z-order                   |
|  [05]   | `DrawingDef`           | def base       | `<defs>`-tier reusable owner (gradient/filter/clip/mask/marker/pattern) |
|  [06]   | `DrawingDefSub`        | def-child base | sub-element of a def (e.g. `GradientStop`, `FilterItem`)                |
|  [07]   | `Context`              | render context | document-wide render policy (`invert_y`, `animation_config`)            |
|  [08]   | `LocalContext`         | scoped context | per-element render context threaded through serialization               |

[PUBLIC_TYPE_SCOPE]: drawable element vocabulary

Every constructor carries a trailing `**kwargs` for SVG presentation attributes; `Text`'s long constructor is spelled below the table.

| [INDEX] | [TYPE]          | [BASE]                   | [ROLE]                                                                                      |
| :-----: | :-------------- | :----------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `Rectangle`     | DrawingBasicElement      | `Rectangle(x, y, width, height)` axis-aligned rect                                          |
|  [02]   | `Circle`        | DrawingBasicElement      | `Circle(cx, cy, r)`                                                                         |
|  [03]   | `Ellipse`       | DrawingBasicElement      | `Ellipse(cx, cy, rx, ry)`                                                                   |
|  [04]   | `Path`          | DrawingBasicElement      | `Path(d='')` SVG path with typed command builders                                           |
|  [05]   | `Lines`         | Path                     | `Lines(sx, sy, *points, close=False)` polyline/polygon                                      |
|  [06]   | `Line`          | Lines                    | `Line(sx, sy, ex, ey)` single segment                                                       |
|  [07]   | `Arc`           | Path                     | `Arc(cx, cy, r, start_deg, end_deg, cw=False)` arc path                                     |
|  [08]   | `ArcLine`       | Circle                   | `ArcLine(cx, cy, r, start_deg, end_deg)` circle-derived arc                                 |
|  [09]   | `Text`          | DrawingParentElement     | multiline / on-path text; full constructor `[09]` below                                     |
|  [10]   | `TSpan`         | (_TextContainingElement) | `TSpan(text)` styled text run inside `Text`                                                 |
|  [11]   | `Image`         | DrawingBasicElement      | `Image(x, y, width, height, path=None, data=None, embed=False, mime_type=None, ...)` raster |
|  [12]   | `Use`           | DrawingBasicElement      | `Use(other_elem, x, y)` `<use>` instanced reference                                         |
|  [13]   | `Group`         | DrawingParentElement     | `Group(children=(), ordered_children=None)` `<g>` transform/style container                 |
|  [14]   | `Raster`        | object                   | `Raster(png_data=None, png_file=None)` raw raster payload for embedding                     |
|  [15]   | `Raw`           | DrawingElement           | `Raw(content, defs=())` verbatim SVG/markup escape hatch                                    |
|  [16]   | `ForeignObject` | DrawingParentElement     | `<foreignObject>` embedded non-SVG (HTML) content                                           |
|  [17]   | `NoElement`     | DrawingElement           | empty sentinel element (renders nothing)                                                    |

- [09]: `Text(text, font_size, x=None, y=None, *, center=False, line_height=1, line_offset=0, path=None, start_offset=None, ...)` — multiline / on-path text run.

[PUBLIC_TYPE_SCOPE]: def-tier paint and effect owners

Every constructor carries a trailing `**kwargs`; both gradients default `gradientUnits='userSpaceOnUse'`.

| [INDEX] | [TYPE]           | [KIND]     | [ROLE]                                                                               |
| :-----: | :--------------- | :--------- | :----------------------------------------------------------------------------------- |
|  [01]   | `LinearGradient` | paint def  | `LinearGradient(x1, y1, x2, y2)`; `.add_stop(offset, color, opacity=None)`           |
|  [02]   | `RadialGradient` | paint def  | `RadialGradient(cx, cy, r, fy=None)`; `.add_stop(...)`                               |
|  [03]   | `GradientStop`   | paint sub  | `<stop>` color/offset child of a gradient                                            |
|  [04]   | `Pattern`        | paint def  | `Pattern(width, height, x=None, y=None, patternUnits='userSpaceOnUse')` tiled fill   |
|  [05]   | `Filter`         | effect def | `Filter(children=(), ordered_children=None)` filter container                        |
|  [06]   | `FilterItem`     | effect sub | `FilterItem(tag_name)` one `<fe*>` filter primitive                                  |
|  [07]   | `ClipPath`       | clip def   | `ClipPath(children=(), ordered_children=None)` `<clipPath>` geometric clip           |
|  [08]   | `Mask`           | mask def   | `Mask(children=(), ordered_children=None)` `<mask>` luminance/alpha mask             |
|  [09]   | `Marker`         | marker def | `Marker(minx, miny, maxx, maxy, scale=1, orient='auto')` line-endpoint/vertex marker |

[PUBLIC_TYPE_SCOPE]: animation algebra

Every `Animate*` constructor carries a trailing `**kwargs`; the long `AnimateTransform` and `SyncedAnimationConfig` constructors are spelled below the table.

| [INDEX] | [TYPE]                  | [BASE]              | [ROLE]                                                                                   |
| :-----: | :---------------------- | :------------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `Animate`               | DrawingBasicElement | `Animate(attributeName, dur, from_or_values=None, to=None, begin=None, other_elem=None)` |
|  [02]   | `AnimateTransform`      | Animate             | transform animation; full constructor `[02]` below                                       |
|  [03]   | `AnimateMotion`         | Animate             | `AnimateMotion(path, dur, from_or_values=None, to=None, begin=None)` motion along a path |
|  [04]   | `Set`                   | Animate             | `Set(attributeName, dur, to=None, begin=None)` discrete attribute set                    |
|  [05]   | `Discard`               | Animate             | `Discard(attributeName, begin=None)` `<discard>` element-removal hint                    |
|  [06]   | `SyncedAnimationConfig` | object              | synchronized timeline + playback controls; full constructor `[06]` below                 |
|  [07]   | `FrameAnimation`        | object              | `FrameAnimation(draw_func=None, callback=None)` per-frame redraw driver                  |

- [02]: `AnimateTransform(type, dur, from_or_values, to=None, begin=None, attributeName='transform')` — transform animation over the named attribute.
- [06]: `SyncedAnimationConfig(duration, start_delay=0, end_delay=0, repeat_count='indefinite', fill='freeze', show_playback_controls=False, ...)` — synchronized timeline threaded through `Drawing(animation_config=)`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Drawing` build, z-order, serialize

`Drawing(width, height, origin=(0,0), context=None, animation_config=None, id_prefix='d', **svg_args)` lands `svg_args` as root `<svg>` attributes, so a namespace declaration rides the constructor; the constructor-owned `width`/`height`/origin-derived `viewBox` are reserved keys the admission boundary rejects from `svg_args` rather than letting a duplicate override the constructor fields. Both serialize overloads carry full signatures below.

- call: `as_svg(output_file=None, randomize_ids=False, header=..., skip_js=False, skip_css=False, context=None)` / `save_svg(fname, encoding='utf-8', context=None)` — SVG string / file egress
- call: `as_html(output_file=None, title=None, randomize_ids=False, context=None, fix_embed_iframe=False)` / `save_html(fname, title=None, encoding='utf-8', context=None)` — standalone HTML egress

| [INDEX] | [MEMBER]                                                           | [KIND]    | [ROLE]                                                  |
| :-----: | :----------------------------------------------------------------- | :-------- | :------------------------------------------------------ |
|  [01]   | `Drawing(...)`                                                     | construct | the document canvas (signature in the lead)             |
|  [02]   | `append(element)` / `draw(obj)` / `extend(iterable)`               | build     | insert a drawable (or draw-protocol object); `z` orders |
|  [03]   | `append_def(element)` / `draw_def(obj)`                            | build     | register a `<defs>`-tier paint/effect owner             |
|  [04]   | `append_css(css_text)` / `append_title(text)`                      | build     | attach document-level CSS / title                       |
|  [05]   | `append_javascript(js_text, onload=None)`                          | build     | attach document-level JS (`onload` hook)                |
|  [06]   | `embed_google_font(family, text=None, display='swap')`             | build     | inline a Google font `@font-face` (network fetch)       |
|  [07]   | `insert(i, element)` / `remove(element)` / `clear()` / `reverse()` | build     | `MutableSequence` edits of the child list               |
|  [08]   | `count(element)` / `index(element)`                                | query     | `MutableSequence` count / index lookup                  |
|  [09]   | `set_pixel_scale(s=1)` / `calc_render_size()`                      | size      | pixel scale / computed render size                      |
|  [10]   | `set_render_size(w=None, h=None)`                                  | size      | declared output render size                             |
|  [11]   | `as_svg(...)` / `save_svg(...)`                                    | serialize | SVG string / file egress (call above)                   |
|  [12]   | `as_html(...)` / `save_html(...)`                                  | serialize | standalone HTML egress (call above)                     |
|  [13]   | `all_elements/all_css/all_javascript(context=None)`                | query     | flattened element / CSS / JS streams for inspection     |
|  [14]   | `rasterize(to_file=None, context=None)`                            | raster    | [GATED] SVG-to-pixels via `cairoSVG` (absent)           |
|  [15]   | `save_png(fname, context=None)`                                    | raster    | [GATED] PNG file; route to `resvg-py`/`vl-convert`      |
|  [16]   | `as_gif/as_mp4/as_video/as_spritesheet/as_animation_frames(...)`   | raster    | [GATED] in-memory video/frame (`imageio`+ffmpeg)        |
|  [17]   | `save_gif/save_mp4/save_video/save_spritesheet(...)`               | raster    | [GATED] file egress; ffmpeg via `to_process.run_sync`   |
|  [18]   | `display_inline/display_image/display_iframe(context=None)`        | notebook  | Jupyter rich display (notebook boundary only)           |

[ENTRYPOINT_SCOPE]: `Path` command builders

Uppercase commands are absolute, lowercase relative; `arc(cx, cy, r, start_deg, end_deg, ...)` is the high-level circular-arc convenience emitting the right `A`. Every builder returns `self` for chaining, and `append(command_str, *args)` is the escape for an unmodeled command.

| [INDEX] | [MEMBER]                                                                            | [KIND] | [ROLE]                                 |
| :-----: | :---------------------------------------------------------------------------------- | :----- | :------------------------------------- |
|  [01]   | `Path.M(x, y)` / `Path.m(dx, dy)`                                                   | build  | absolute / relative moveto             |
|  [02]   | `Path.L(x, y)` / `Path.l(dx, dy)` / `H(x)` / `h(dx)` / `V(y)` / `v(dy)`             | build  | lineto and axis-aligned line commands  |
|  [03]   | `Path.C(cx1, cy1, cx2, cy2, ex, ey)` / `c(...)` / `S(cx2, cy2, ex, ey)` / `s(...)`  | build  | cubic Bezier and smooth cubic          |
|  [04]   | `Path.Q(cx, cy, ex, ey)` / `q(...)` / `T(ex, ey)` / `t(...)`                        | build  | quadratic Bezier and smooth quadratic  |
|  [05]   | `Path.A(rx, ry, rot, large_arc, sweep, ex, ey)` / `a(...)`                          | build  | elliptical arc command                 |
|  [06]   | `Path.Z()`                                                                          | build  | close current subpath                  |
|  [07]   | `Path.arc(cx, cy, r, start_deg, end_deg, cw=True, include_m=True, include_l=False)` | build  | high-level circular-arc emit (degrees) |
|  [08]   | `Path.append(command_str, *args)`                                                   | build  | low-level raw path-command append      |

[ENTRYPOINT_SCOPE]: element animation and titling

These attach off the `DrawingBasicElement`/`DrawingParentElement` base, so animation dispatches off the base, not a per-shape method (drop the `<element>.` prefix below).

| [INDEX] | [MEMBER]                                                                  | [KIND]  | [ROLE]                                          |
| :-----: | :------------------------------------------------------------------------ | :------ | :---------------------------------------------- |
|  [01]   | `append_anim(animate_element)` / `extend_anim(animate_iterable)`          | animate | attach one / many SMIL `Animate*` children      |
|  [02]   | `add_key_frame(time, animation_args=None, **attr_values)`                 | animate | build a keyframe at `time` for attribute values |
|  [03]   | `add_attribute_key_sequence(attr, times, values, *, animation_args=None)` | animate | build an attribute's full keyframe timeline     |
|  [04]   | `append_title(text)`                                                      | a11y    | attach an accessible `<title>` annotation       |

[ENTRYPOINT_SCOPE]: module-level helpers

`data:`-URI helpers emit inline-embed URIs and `escape_cdata` sanitizes verbatim content; the `animate_*`/`render_svg_frames`/`frame_animate_*` frame helpers are boundary-gated alongside the `Drawing` raster rows (absent extras + ffmpeg).

- call: `render_svg_frames(frames, align_bottom=False, align_right=False, bg=(255,255,255,255), verbose=False, **kwargs)` — [GATED] frame-render via optional extras + ffmpeg
- call: `animate_text_sequence(container, times, values, *text_args, kwargs_list=None, **text_kwargs)` — frame-keyed text animation sequence

| [INDEX] | [MEMBER]                                                             | [KIND]  | [ROLE]                                           |
| :-----: | :------------------------------------------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `svg_as_data_uri(txt, ..., mime='image/svg+xml')`                    | encode  | SVG `data:` URI for inline embedding             |
|  [02]   | `svg_as_utf8_data_uri(txt, ...)`                                     | encode  | UTF-8 SVG `data:` URI                            |
|  [03]   | `bytes_as_data_uri(data, ..., mime='image/svg+xml')`                 | encode  | raw-byte `data:` URI                             |
|  [04]   | `escape_cdata(content)`                                              | encode  | escape verbatim markup for safe inclusion        |
|  [05]   | `animate_element_sequence(times, element_sequence)`                  | animate | frame-keyed element animation sequence           |
|  [06]   | `animate_text_sequence(...)`                                         | animate | frame-keyed text animation sequence (call above) |
|  [07]   | `render_svg_frames(...)` / `save_video(frames, file, verbose=False)` | raster  | [GATED] frame-render / video assembly (ffmpeg)   |
|  [08]   | `frame_animate_video(out_file, draw_func=None, jupyter=False)`       | raster  | [GATED] frame-driven video egress                |
|  [09]   | `frame_animate_spritesheet(...)` / `frame_animate_jupyter(...)`      | raster  | [GATED] spritesheet / jupyter frame egress       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `import drawsvg` at boundary scope only; the distribution and import name are both `drawsvg`.
- canvas axis: `Drawing(origin=, context=Context(invert_y=))` sets the coordinate frame once, so figure code works in a domain-natural frame and never hand-flips y per element.
- insertion axis: `append`/`draw`/`extend` (each taking `z=`) is the one polymorphic child-insertion surface — `draw` accepts any drawable or draw-protocol object and `z` orders, so there is no `add_rect`/`add_circle`/`add_text` family.
- path axis: `Path` builds path data through typed command methods (`M`/`L`/`C`/`Q`/`A`/`Z` absolute, lowercase relative, `arc()` high-level) each returning `self`, so a connector/glyph composes through the builder, never a concatenated `d` string; `Lines`/`Arc` derive from `Path`.
- def axis: gradients/patterns/filters/clips/masks/markers are `DrawingDef` owners registered once via `append_def`/`draw_def` and referenced by id, so reusable paint/effect is defined once and shared, never duplicated inline per shape.
- animation axis: SMIL animation attaches via `append_anim`/`extend_anim` on the element base over the `Animate`/`AnimateTransform`/`AnimateMotion`/`Set`/`Discard` grammar with `add_key_frame`/`add_attribute_key_sequence` the keyframe builder; `SyncedAnimationConfig` threads a synchronized timeline through `Drawing(animation_config=)`, owning declarative static-SVG animation — live UI animation stays out.
- egress axis: `as_svg`/`save_svg`, `as_html`/`save_html`, and `svg_as_data_uri` are the serialization family; the SVG string is the artifact this owner produces, recorded under the runtime content-key.
- evidence: each figure op captures element count, canvas/render size, def-owner count, and output SVG byte length as a figure `ArtifactReceipt`, keyed PRE-RUN by `ContentIdentity.key` over the length-framed canonical seed spanning every output-affecting input (element tree, root `svg_args`, attached assets, resolved `Context`, `randomize_ids`, serialization header); the serialized byte length rides the receipt bytes slot, never the key preimage.
- boundary: drawsvg owns programmatic SVG authoring, the element vocabulary, path-command building, def-tier paint/effect owners, and SMIL animation; geometry parse/transform/bbox over an existing SVG routes to `svgelements`; rasterization to `resvg-py`/`vl-convert`/`pyvips`/`pillow`; chart/table/QR SVG sources arrive from `vl-convert`/`great-tables`/`segno`; Jupyter display stays at the notebook boundary.

[STACKING]:
- A diagram figure builds a `Drawing`, registers a `LinearGradient`/`Marker`/`Filter` via `append_def`, draws the `Rectangle`/`Path`/`Text` vocabulary with `z=` ordering, then `as_svg()` emits the SVG string the figure owner records under a `ContentIdentity` content-key — one authoring rail produces the durable SVG artifact.
- A drawsvg-authored SVG and a `segno` QR / `great-tables` table SVG co-compose through `svgelements` (`SVG.parse` + `Matrix * shape` + `bbox()`) into one n-up sheet — drawsvg authors net-new vector marks, svgelements lays out the parsed tree.
- `svg_as_utf8_data_uri(drawing.as_svg())` produces a `data:` URI a `jinja2`/`weasyprint` HTML/PDF document embeds inline, threading the figure artifact into the document rail without a temp-file round-trip.
- A `SyncedAnimationConfig` + `add_key_frame` timeline serializes to standalone `as_html`; a rasterized GIF/MP4 deliverable runs the frame egress through the runtime worker `to_process.run_sync` seam (ffmpeg subprocess), keeping the blocking call off the async figure rail.

[RAIL_LAW]:
- Package: `drawsvg`
- Owns: programmatic SVG document authoring — the closed drawable-element vocabulary, typed absolute/relative SVG path-command builders, the `<defs>`-tier paint/effect owners, the SMIL animation algebra with synchronized timeline, and SVG/HTML/`data:`-URI serialization
- Accept: building diagrams, analysis overlays, machine-readable annotated SVG, and declarative SVG animation as durable vector artifacts, composing with `svgelements` layout and downstream raster owners
- Reject: in-page rasterization (`resvg-py`/`vl-convert`/`pyvips`/`pillow` cover it, extras absent on core); a blocking ffmpeg shell-out where `to_process.run_sync` owns subprocess work; a hand-emitted XML tag or `d` string where the element vocabulary and `Path` exist; an `add_rect`/`add_circle` family where `append`/`draw(z=)` discriminates; SVG geometry parse/transform/bbox where `svgelements` owns it; live UI animation; identity minting the runtime owns
