# [PY_ARTIFACTS_API_DRAWSVG]

`drawsvg` supplies the programmatic SVG-authoring surface for the artifacts figure rail: a `Drawing` document canvas with explicit coordinate origin and optional y-inversion, a closed element vocabulary (`Rectangle`/`Circle`/`Ellipse`/`Line`/`Lines`/`Path`/`Arc`/`ArcLine`/`Text`/`TSpan`/`Image`/`Use`/`Group`/`Raster`/`Raw`), absolute/relative SVG path-command builders on `Path` (`M`/`L`/`C`/`Q`/`A`/`Z` and lowercase relatives), the `def`-tier paint/effect owners (`LinearGradient`/`RadialGradient`/`Pattern`/`Filter`/`ClipPath`/`Mask`/`Marker`), the SMIL native-animation algebra (`Animate`/`AnimateTransform`/`AnimateMotion`/`Set`/`Discard` plus the `SyncedAnimationConfig` synchronized timeline), and the serialization egress (`as_svg`/`save_svg`, `as_html`/`save_html`, `svg_as_data_uri`). The package owner composes `Drawing` + the element vocabulary into the figure-composition path that assembles diagrams, analysis overlays, and machine-readable annotated SVG; it never hand-emits XML tags, never re-implements the SVG `d` grammar, and it routes rasterization downstream — its in-package raster/video egress (`save_png`/`rasterize`/`as_gif`/`as_mp4`/`as_video`/`as_spritesheet`) depends on the optional `cairoSVG`+`imageio`+ffmpeg extras which are NOT installed on the core core, so PNG/PDF rasterization routes to `resvg-py`/`vl-convert`/`pyvips`, and any video assembly that does shell ffmpeg runs through the runtime worker `to_process.run_sync` subprocess seam, never an in-page blocking call.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `drawsvg`
- package: `drawsvg`
- import: `drawsvg`
- owner: `artifacts`
- rail: figure
- license: MIT (`License :: OSI Approved :: MIT License`)
- installed: `2.4.1`
- entry points: none (library only)
- capability: build an SVG document tree programmatically from a closed element vocabulary, author absolute/relative SVG path data through typed builder methods, define gradients/patterns/filters/clips/masks/markers as reusable `def`-tier owners, attach SMIL animation (per-attribute, transform, motion-along-path, discrete set, synchronized-timeline with optional playback controls), z-order children, embed raster/data-URI/Google-font assets inline, and serialize to SVG string, file, standalone HTML, or `data:` URI — all without touching XML

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document canvas and element base hierarchy
- rail: figure

`Drawing` is the document root and a `MutableSequence`-shaped container; every drawable derives `DrawingElement`, splitting into `DrawingBasicElement` (leaf shapes carrying attributes + animation), `DrawingParentElement` (containers holding children), and `DrawingDef`/`DrawingDefSub` (`<defs>`-tier reusable paint/effect owners). The base classes are the bounded algebra a figure consumer dispatches over; a figure op reads `bbox`/append semantics off the base, never off a per-shape special case.

| [INDEX] | [TYPE] | [KIND] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `Drawing` | document root | canvas with `width`/`height`/`origin`; append/draw children, serialize |
| [02] | `DrawingElement` | base | abstract element root; SVG serialization protocol |
| [03] | `DrawingBasicElement` | leaf base | attribute-bearing leaf; `append_anim`/`add_key_frame`/`append_title` |
| [04] | `DrawingParentElement` | container base | holds children; `append`/`draw`/`extend` with z-order |
| [05] | `DrawingDef` | def base | `<defs>`-tier reusable owner (gradient/filter/clip/mask/marker/pattern) |
| [06] | `DrawingDefSub` | def-child base | sub-element of a def (e.g. `GradientStop`, `FilterItem`) |
| [07] | `Context` | render context | document-wide render policy (`invert_y`, `animation_config`) |
| [08] | `LocalContext` | scoped context | per-element render context threaded through serialization |

[PUBLIC_TYPE_SCOPE]: drawable element vocabulary
- rail: figure

The shape vocabulary is the bounded grammar a figure builds from. `Lines`/`Line` and `Arc`/`ArcLine` derive from `Path`/`Circle`, so a polyline or arc IS a path/circle — a figure transform applies to the typed element, never to a re-emitted tag. `Group` is the structural `<g>` container that carries shared transform/style and z-ordered children.

| [INDEX] | [TYPE] | [BASE] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `Rectangle` | DrawingBasicElement | `Rectangle(x, y, width, height, **kwargs)` axis-aligned rect |
| [02] | `Circle` | DrawingBasicElement | `Circle(cx, cy, r, **kwargs)` |
| [03] | `Ellipse` | DrawingBasicElement | `Ellipse(cx, cy, rx, ry, **kwargs)` |
| [04] | `Path` | DrawingBasicElement | `Path(d='', **kwargs)` SVG path with typed command builders |
| [05] | `Lines` | Path | `Lines(sx, sy, *points, close=False, **kwargs)` polyline/polygon |
| [06] | `Line` | Lines | `Line(sx, sy, ex, ey, **kwargs)` single segment |
| [07] | `Arc` | Path | `Arc(cx, cy, r, start_deg, end_deg, cw=False, **kwargs)` arc path |
| [08] | `ArcLine` | Circle | `ArcLine(cx, cy, r, start_deg, end_deg, **kwargs)` circle-derived arc |
| [09] | `Text` | DrawingParentElement | `Text(text, font_size, x=None, y=None, *, center=False, line_height=1, line_offset=0, path=None, start_offset=None, ...)` multiline/on-path text |
| [10] | `TSpan` | (_TextContainingElement) | `TSpan(text, **kwargs)` styled text run inside `Text` |
| [11] | `Image` | DrawingBasicElement | `Image(x, y, width, height, path=None, data=None, embed=False, mime_type=None, ...)` linked/embedded raster |
| [12] | `Use` | DrawingBasicElement | `Use(other_elem, x, y, **kwargs)` `<use>` instanced reference |
| [13] | `Group` | DrawingParentElement | `Group(children=(), ordered_children=None, **args)` `<g>` transform/style container |
| [14] | `Raster` | object | `Raster(png_data=None, png_file=None)` raw raster payload for embedding |
| [15] | `Raw` | DrawingElement | `Raw(content, defs=())` verbatim SVG/markup escape hatch |
| [16] | `ForeignObject` | DrawingParentElement | `<foreignObject>` embedded non-SVG (HTML) content |
| [17] | `NoElement` | DrawingElement | empty sentinel element (renders nothing) |

[PUBLIC_TYPE_SCOPE]: def-tier paint and effect owners
- rail: figure

Paint, clip, mask, marker, and filter owners live in `<defs>` and are referenced by id; they are `DrawingDef` subclasses a figure registers once via `append_def`/`draw_def` and reuses across shapes. `LinearGradient`/`RadialGradient` carry `GradientStop` children through `add_stop`; `Filter` holds `FilterItem` primitive children.

| [INDEX] | [TYPE] | [KIND] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `LinearGradient` | paint def | `LinearGradient(x1, y1, x2, y2, gradientUnits='userSpaceOnUse', **kwargs)`; `.add_stop(offset, color, opacity=None, **kwargs)` |
| [02] | `RadialGradient` | paint def | `RadialGradient(cx, cy, r, gradientUnits='userSpaceOnUse', fy=None, **kwargs)`; `.add_stop(...)` |
| [03] | `GradientStop` | paint sub | `<stop>` color/offset child of a gradient |
| [04] | `Pattern` | paint def | `Pattern(width, height, x=None, y=None, patternUnits='userSpaceOnUse', **kwargs)` tiled fill |
| [05] | `Filter` | effect def | `Filter(children=(), ordered_children=None, **args)` filter container |
| [06] | `FilterItem` | effect sub | `FilterItem(tag_name, **args)` one `<fe*>` filter primitive |
| [07] | `ClipPath` | clip def | `ClipPath(children=(), ordered_children=None, **args)` `<clipPath>` geometric clip |
| [08] | `Mask` | mask def | `Mask(children=(), ordered_children=None, **args)` `<mask>` luminance/alpha mask |
| [09] | `Marker` | marker def | `Marker(minx, miny, maxx, maxy, scale=1, orient='auto', **kwargs)` line-endpoint/vertex marker |

[PUBLIC_TYPE_SCOPE]: animation algebra
- rail: figure

SMIL animation elements attach to a parent via `append_anim`/`extend_anim`; `Animate` is the per-attribute root, the three subclasses specialize transform/motion/discrete-set, and `SyncedAnimationConfig` (threaded through `Drawing(animation_config=...)` or `Context`) drives a synchronized timeline with optional on-canvas playback controls. This is the static-SVG animation path; live UI animation stays outside the package.

| [INDEX] | [TYPE] | [BASE] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `Animate` | DrawingBasicElement | `Animate(attributeName, dur, from_or_values=None, to=None, begin=None, other_elem=None, **kwargs)` |
| [02] | `AnimateTransform` | Animate | `AnimateTransform(type, dur, from_or_values, to=None, begin=None, attributeName='transform', ...)` |
| [03] | `AnimateMotion` | Animate | `AnimateMotion(path, dur, from_or_values=None, to=None, begin=None, ...)` motion along a path |
| [04] | `Set` | Animate | `Set(attributeName, dur, to=None, begin=None, ...)` discrete attribute set |
| [05] | `Discard` | Animate | `Discard(attributeName, begin=None, **kwargs)` `<discard>` element-removal hint |
| [06] | `SyncedAnimationConfig` | object | `SyncedAnimationConfig(duration, start_delay=0, end_delay=0, repeat_count='indefinite', fill='freeze', show_playback_controls=False, ...)` synchronized timeline + controls |
| [07] | `FrameAnimation` | object | `FrameAnimation(draw_func=None, callback=None)` per-frame redraw driver (frame-render egress) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Drawing` build, z-order, serialize
- rail: figure

`Drawing(width, height, origin=(0,0), context=None, animation_config=None, id_prefix='d')` is the canvas; `append`/`draw`/`extend` (each taking keyword `z=` for z-order) is the single polymorphic insertion surface — `draw` accepts any drawable or an object exposing a draw protocol, `append` takes an element directly, so there is no per-shape `add_rect`/`add_circle` family. `append_def`/`draw_def` register a `<defs>` owner; `append_css`/`append_javascript`/`append_title`/`embed_google_font` attach document-level assets. Serialization is the one egress family: `as_svg`/`save_svg` for the SVG string/file, `as_html`/`save_html` for standalone HTML, with the raster/video rows gated behind the absent extras.

| [INDEX] | [MEMBER] | [KIND] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `Drawing(width, height, origin=(0, 0), context=None, animation_config=None, id_prefix='d', **svg_args)` | construct | canvas; `origin='center'`/`(x,y)` and `context.invert_y` set coordinate frame |
| [02] | `Drawing.append(element, *, z=None)` / `draw(obj, *, z=None, **kwargs)` / `extend(iterable, *, z=None)` | build | insert a drawable (or draw-protocol object); `z` orders |
| [03] | `Drawing.append_def(element)` / `draw_def(obj, **kwargs)` | build | register a `<defs>`-tier paint/effect owner |
| [04] | `Drawing.append_css(css_text)` / `append_javascript(js_text, onload=None)` / `append_title(text, **kwargs)` | build | attach document-level CSS / JS / title |
| [05] | `Drawing.embed_google_font(family, text=None, display='swap', **kwargs)` | build | inline a Google font `@font-face` (network fetch at build) |
| [06] | `Drawing.insert(i, element)` / `remove(element)` / `clear()` / `reverse()` / `count(element)` / `index(element)` | build | `MutableSequence` editing of the child list |
| [07] | `Drawing.set_pixel_scale(s=1)` / `set_render_size(w=None, h=None)` / `calc_render_size()` | size | declared render/output pixel size and scale |
| [08] | `Drawing.as_svg(output_file=None, randomize_ids=False, header=..., skip_js=False, skip_css=False, context=None)` / `save_svg(fname, encoding='utf-8', context=None)` | serialize | SVG string / file egress |
| [09] | `Drawing.as_html(output_file=None, title=None, randomize_ids=False, context=None, fix_embed_iframe=False)` / `save_html(fname, title=None, encoding='utf-8', context=None)` | serialize | standalone HTML egress |
| [10] | `Drawing.all_elements(context=None)` / `all_css(context=None)` / `all_javascript(context=None)` | query | flattened element/CSS/JS streams for inspection |
| [11] | `Drawing.rasterize(to_file=None, context=None)` / `save_png(fname, context=None)` | raster | [GATED] PNG via optional `cairoSVG`; absent on core — route raster to `resvg-py`/`vl-convert`/`pyvips` |
| [12] | `Drawing.as_gif/as_mp4/as_video/as_spritesheet/as_animation_frames(...)` / `save_gif/save_mp4/save_video/save_spritesheet(...)` | raster | [GATED] video/frame egress via optional `imageio`+ffmpeg; absent on core — run any ffmpeg assembly through the runtime worker `to_process.run_sync` seam |
| [13] | `Drawing.display_inline/display_image/display_iframe(context=None)` | notebook | Jupyter rich display (notebook boundary only) |

[ENTRYPOINT_SCOPE]: `Path` command builders
- rail: figure

`Path(d='')` builds path data through typed command methods, one per SVG path command: uppercase = absolute, lowercase = relative — `M`/`m` moveto, `L`/`l` lineto, `H`/`h`/`V`/`v` axis lines, `C`/`c`/`S`/`s` cubic, `Q`/`q`/`T`/`t` quadratic, `A`/`a` arc, `Z` close. `arc(cx, cy, r, start_deg, end_deg, ...)` is the high-level circular-arc convenience that emits the right `A` command. The builders return `self` for chaining; a figure composes a glyph or connector through one `Path`, never a hand-built `d` string. `append(command_str, *args)` is the low-level escape for an unmodeled command.

| [INDEX] | [MEMBER] | [KIND] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `Path.M(x, y)` / `Path.m(dx, dy)` | build | absolute / relative moveto |
| [02] | `Path.L(x, y)` / `Path.l(dx, dy)` / `H(x)` / `h(dx)` / `V(y)` / `v(dy)` | build | lineto and axis-aligned line commands |
| [03] | `Path.C(cx1, cy1, cx2, cy2, ex, ey)` / `c(...)` / `S(cx2, cy2, ex, ey)` / `s(...)` | build | cubic Bezier and smooth cubic |
| [04] | `Path.Q(cx, cy, ex, ey)` / `q(...)` / `T(ex, ey)` / `t(...)` | build | quadratic Bezier and smooth quadratic |
| [05] | `Path.A(rx, ry, rot, large_arc, sweep, ex, ey)` / `a(...)` | build | elliptical arc command |
| [06] | `Path.Z()` | build | close current subpath |
| [07] | `Path.arc(cx, cy, r, start_deg, end_deg, cw=True, include_m=True, include_l=False)` | build | high-level circular-arc emit (degrees) |
| [08] | `Path.append(command_str, *args)` | build | low-level raw path-command append |

[ENTRYPOINT_SCOPE]: element animation and titling
- rail: figure

Every `DrawingBasicElement`/`DrawingParentElement` carries the animation-attach surface: `append_anim`/`extend_anim` attach SMIL `Animate*` children, while `add_key_frame`/`add_attribute_key_sequence` are the keyframe-builder shortcut that constructs the animation timeline from value/time pairs. `append_title` adds an accessible `<title>`. These are shared across the vocabulary, so animation dispatches off the base, not a per-shape method.

| [INDEX] | [MEMBER] | [KIND] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `<element>.append_anim(animate_element)` / `extend_anim(animate_iterable)` | animate | attach one / many SMIL `Animate*` children |
| [02] | `<element>.add_key_frame(time, animation_args=None, **attr_values)` | animate | build a keyframe at `time` for the given attribute values |
| [03] | `<element>.add_attribute_key_sequence(attr, times, values, *, animation_args=None)` | animate | build an attribute's full keyframe timeline from sequences |
| [04] | `<element>.append_title(text, **kwargs)` | a11y | attach an accessible `<title>` annotation |

[ENTRYPOINT_SCOPE]: module-level helpers
- rail: figure

The data-URI helpers (`svg_as_data_uri`/`svg_as_utf8_data_uri`/`bytes_as_data_uri`) emit `data:` URIs for inline embedding; `escape_cdata` sanitizes verbatim content; the `animate_*`/`render_svg_frames`/`save_video`/`frame_animate_*` helpers drive frame-sequence animation. The frame/video helpers depend on the absent extras and on ffmpeg, so they are boundary-gated alongside the `Drawing` raster rows.

| [INDEX] | [MEMBER] | [KIND] | [ROLE] |
| --- | --- | --- | --- |
| [01] | `svg_as_data_uri(txt, ..., mime='image/svg+xml')` / `svg_as_utf8_data_uri(txt, ...)` / `bytes_as_data_uri(data, ..., mime='image/svg+xml')` | encode | SVG/byte `data:` URI for inline embedding |
| [02] | `escape_cdata(content)` | encode | escape verbatim markup for safe inline inclusion |
| [03] | `animate_element_sequence(times, element_sequence)` / `animate_text_sequence(container, times, values, *text_args, kwargs_list=None, **text_kwargs)` | animate | build a frame-keyed element/text animation sequence |
| [04] | `render_svg_frames(frames, align_bottom=False, align_right=False, bg=(255,255,255,255), verbose=False, **kwargs)` / `save_video(frames, file, verbose=False, **kwargs)` | raster | [GATED] frame-render / video assembly via optional extras + ffmpeg |
| [05] | `frame_animate_video(out_file, draw_func=None, jupyter=False, **video_args)` / `frame_animate_spritesheet(...)` / `frame_animate_jupyter(...)` | raster | [GATED] frame-driven animation egress (optional extras) |

## [04]-[IMPLEMENTATION_LAW]

- import: `import drawsvg` at boundary scope only; the distribution and import name are both `drawsvg`; the version is `importlib.metadata.version("drawsvg")`, not `__version__`.
- canvas axis: `Drawing(width, height, origin=, context=)` is the single document owner; `origin='center'`/`(x,y)` plus `Context(invert_y=True)` set the coordinate frame once, so figure code works in a domain-natural frame and never hand-flips y per element.
- insertion axis: `append`/`draw`/`extend` (each taking `z=`) is the one polymorphic child-insertion surface — `draw` accepts any drawable or draw-protocol object and `z` orders, so there is no `add_rect`/`add_circle`/`add_text` family; the shape vocabulary is constructed and handed to one entrypoint.
- path axis: `Path` builds path data through typed command methods (`M`/`L`/`C`/`Q`/`A`/`Z` absolute, lowercase relative, `arc()` high-level), each returning `self` for chaining; a figure composes connectors/glyphs through the typed builder, never a concatenated `d` string, and `Lines`/`Arc` derive from `Path` so a polyline/arc is the same owner.
- def axis: gradients/patterns/filters/clips/masks/markers are `DrawingDef` owners registered once via `append_def`/`draw_def` and referenced by id; `add_stop`/`FilterItem` build their sub-elements, so reusable paint/effect is defined once and shared, never duplicated inline per shape.
- animation axis: SMIL animation attaches via `append_anim`/`extend_anim` on the element base, with `Animate`/`AnimateTransform`/`AnimateMotion`/`Set`/`Discard` the bounded element grammar and `add_key_frame`/`add_attribute_key_sequence` the keyframe builder; `SyncedAnimationConfig` threads a synchronized timeline (and optional playback controls) through `Drawing(animation_config=)`, owning declarative static-SVG animation — live UI animation stays out of this package.
- egress axis: `as_svg`/`save_svg` (SVG string/file), `as_html`/`save_html` (standalone HTML), and `svg_as_data_uri` (`data:` URI) are the core serialization family; the SVG string is the artifact this owner produces, recorded under the runtime content-key.
- evidence: each figure op captures element count (`all_elements`), declared canvas/render size (`width`/`height`/`calc_render_size`), def-owner count, and output SVG byte length as a figure `ArtifactReceipt`, keyed by `ContentIdentity` over the serialized SVG bytes.
- boundary: drawsvg owns programmatic SVG authoring, the element vocabulary, path-command building, def-tier paint/effect owners, and SMIL animation; geometry parse/transform/bbox query over an existing SVG routes to `svgelements`; rasterization routes to `resvg-py`/`vl-convert`/`pyvips`/`pillow`; chart/table SVG sources arrive from `vl-convert`/`great-tables`/`segno`; Jupyter display stays at the notebook boundary.

[STACKING]:
- A diagram figure builds a `Drawing`, registers a `LinearGradient`/`Marker`/`Filter` via `append_def`, draws the `Rectangle`/`Path`/`Text` vocabulary with `z=` ordering, then `as_svg()` emits the SVG string the figure owner records under a `ContentIdentity` content-key — one authoring rail produces the durable SVG artifact.
- A drawsvg-authored SVG and a `segno` QR / `great-tables` table SVG co-compose through `svgelements` (`SVG.parse` + `Matrix * shape` + `bbox()`) into one n-up sheet — drawsvg authors net-new vector marks, svgelements lays out the parsed tree, never overlapping owners.
- `svg_as_utf8_data_uri(drawing.as_svg())` produces a `data:` URI a `jinja2`/`weasyprint` HTML/PDF document embeds inline, so the figure artifact threads into the document rail without a temp-file round-trip.
- A synchronized animation built with `SyncedAnimationConfig` + `add_key_frame` serializes to standalone `as_html`; if a rasterized GIF/MP4 deliverable is required, the frame egress runs through the runtime worker `to_process.run_sync` seam (ffmpeg subprocess), keeping the blocking call off the async figure rail.

## [05]-[LOCAL_ADMISSION]

- Package: `drawsvg`
- Owns: programmatic SVG document authoring, the closed drawable-element vocabulary (`Rectangle`/`Circle`/`Ellipse`/`Line`/`Lines`/`Path`/`Arc`/`ArcLine`/`Text`/`TSpan`/`Image`/`Use`/`Group`/`Raster`/`Raw`/`ForeignObject`), typed absolute/relative SVG path-command builders, the `<defs>`-tier paint/effect owners (gradients/patterns/filters/clips/masks/markers), the SMIL animation algebra plus synchronized timeline, and SVG/HTML/`data:`-URI serialization
- Accept: building diagrams, analysis overlays, machine-readable annotated SVG, and declarative SVG animation as durable vector artifacts, composing with `svgelements` layout and downstream raster owners
- Reject: in-page rasterization where `resvg-py`/`vl-convert`/`pyvips`/`pillow` cover it (the `save_png`/`as_mp4` extras are absent on core); a blocking ffmpeg shell-out where the runtime worker `to_process.run_sync` seam owns subprocess work; a hand-emitted XML tag or hand-built `d` string where the element vocabulary and `Path` builders exist; an `add_rect`/`add_circle` insertion family where `append`/`draw(..., z=)` discriminates; SVG geometry parse/transform/bbox where `svgelements` owns it; live UI animation the figure rail does not produce; identity minting the runtime owns
