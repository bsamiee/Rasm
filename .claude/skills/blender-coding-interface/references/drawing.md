# [DRAWING]

## [01]-[HANDLERS]

Draw handler types decide coordinates, and a region the space lacks or `args=None` raises `TypeError`:

| [INDEX] | [TYPE]                  | [DRAWS]                                                        |
| :-----: | :---------------------- | :------------------------------------------------------------- |
|  [01]   | `POST_PIXEL`            | Region pixels after the region                                 |
|  [02]   | `POST_VIEW`, `PRE_VIEW` | View matrices in the 3D View, View2D coordinates in 2D editors |
|  [03]   | `BACKDROP`              | Node editor `WINDOW` region pixels before the backdrop         |

Handlers receive what they draw and draw over editor state they restore:
- Handlers read their editor as `context.area`, and Blender refuses none of their data writes
- `args` holds batches built from a timer or operator in a GUI session, and a drawn data change replaces the handler with new batches
- `partial(<handler>, <keyword>=<value>)` registers keyword values, `args` passing positional values alone
- Handlers draw while overlays are off, and overlay handlers return early on a false `space.overlay.show_overlays`
- `preferences.system.use_region_overlap` lays toolbar, sidebar, asset shelf, and tool header over 3D View, Image Editor, and node `WINDOW` regions
- Drawings in an overlapped `WINDOW` region inset by each overlapping region
- Blending restores the mode `gpu.state.blend_get()` read on entry
- Clips intersect enabled parent clips and restore entry box/test state; their caller supplies the test state `gpu.state` cannot read
- `POST_VIEW` bodies call `gpu.state.depth_test_set('LESS_EQUAL')` to hide geometry behind objects and restore the prior test after

## [02]-[COORDINATES]

Each coordinate space converts through the host member that owns it:
- `event.mouse_x` and `mouse_y` hold window pixels, `mouse_region_x` and `mouse_region_y` pixels of the region an event reached
- Region pixels are device pixels, `preferences.system.ui_scale` reading 2.0 on a Retina display at a user scale of 1.0
- `region.view2d.region_to_view(x, y)` and `view_to_region(x, y, clip=False)` convert between region pixels and View2D coordinates
- `view_to_region` with `clip=False` returns points outside the region
- Image Editor View2D coordinates are image fractions, `view_to_region` of `(0, 0)` and `(1, 1)` giving the displayed image's corners
- Node positions draw at `location * preferences.system.ui_scale` in View2D, `view_to_region` of that product giving a node's region pixel
- `view3d_utils.region_2d_to_origin_3d(region, rv3d, xy)` and `region_2d_to_vector_3d` give `xy`'s view ray for `scene.ray_cast` in any projection
- `view3d_utils.location_3d_to_region_2d(region, rv3d, coord, default=None)` returns `default` behind a perspective view, read as an `Option`
- `region_3d.perspective_matrix` keeps its value after a view write until the region draws or `region_3d.update()` runs

## [03]-[THEME]

Drawings match the interface through `context.preferences` read at each draw:
- `themes[0].user_interface` holds widget colors (`wcol_regular.text` an RGB `Color`, `inner` RGBA) and gizmo colors (`gizmo_primary`, `gizmo_hi`)
- Theme colors are display sRGB (`COLOR_GAMMA`) and pass through built-in shaders as they are
- Text takes `user_interface.panel_text` in sidebars, toolbars, and Properties, `regions.channels.text` in channels, `<space>.space.text` elsewhere
- Widget units measure `round(18 * system.ui_scale) + 2 * system.pixel_size` device pixels, and lines `width * system.pixel_size`
- `system.ui_scale` reads 0.0 in background runs, and `view.ui_scale` holds the user scale there
- Caches holding sizes or batches built at interface scale key on `(system.ui_scale, system.pixel_size)` and rebuild on a change

Text draws through `blf` over font `0`, the interface font:
- `blf.size(<font>, <pixels>)` applies no interface scale, and text matching the interface takes `ui_styles[0].widget.points * system.ui_scale`
- `blf.enable(<font>, <flag>)` applies `blf.WORD_WRAP`, `CLIPPING`, `ROTATION`, or `SHADOW` until `blf.disable`, state persisting per font
- `blf.draw` leaves blending at `NONE`

## [04]-[BUILT_IN_SHADERS]

Built-in shaders read `ModelViewProjectionMatrix` from the `gpu.matrix` stack and take `pos` as `VEC3` with 2D tuples accepted, every shader but `POINT_*` converting its sRGB colors on a window's sRGB framebuffer:

| [INDEX] | [SHADER]                                       | [INPUTS]                                                           | [CLIPPED] |
| :-----: | :--------------------------------------------- | :----------------------------------------------------------------- | :-------: |
|  [01]   | `UNIFORM_COLOR`                                | `pos`, uniform `color`                                             |    Yes    |
|  [02]   | `FLAT_COLOR`, `SMOOTH_COLOR`                   | `pos`, `color` as `VEC4`                                           |    Yes    |
|  [03]   | `POLYLINE_UNIFORM_COLOR`                       | `pos`, uniforms `color`, `lineWidth`, `lineSmooth`, `viewportSize` |    Yes    |
|  [04]   | `POLYLINE_FLAT_COLOR`, `POLYLINE_SMOOTH_COLOR` | `pos`, `color`, uniforms `lineWidth`, `lineSmooth`, `viewportSize` |   Crash   |
|  [05]   | `POINT_UNIFORM_COLOR`, `POINT_FLAT_COLOR`      | `pos`, uniform `size`, `color` as uniform or attribute             |   Crash   |
|  [06]   | `IMAGE`                                        | `pos`, `texCoord`, sampler `image` of sRGB texels                  |   Crash   |
|  [07]   | `IMAGE_COLOR`                                  | `IMAGE` inputs, tint uniform `color`                               |   Crash   |
|  [08]   | `IMAGE_SCENE_LINEAR_TO_REC709_SRGB`            | `IMAGE` inputs over scene-linear texels                            |   Crash   |
|  [09]   | `IMAGE_COLOR_SCENE_LINEAR_TO_REC709_SRGB`      | `IMAGE_COLOR` inputs over scene-linear texels                      |   Crash   |

Shader and batch calls refuse or crash by input:
- Shader names take no `2D_` or `3D_` prefix, and another name raises `ValueError`
- `from_builtin` wraps the one compiled program of its name and config on every call
- `config='CLIPPED'` reads push constant `ClipModelMatrix` and a `clip_planes` uniform block (that matrix, then six world planes)
- `clip_planes` binds through `shader.uniform_block` under `gpu.state.clip_distances_set(<n>)`
- `config='CLIPPED'` crashes Blender on shaders marked `Crash`
- `viewportSize` reads `gpu.state.viewport_get()[2:]` in regions and offscreen buffers alike
- `UNIFORM_COLOR`, `FLAT_COLOR`, and `SMOOTH_COLOR` draw lines one pixel wide past `line_width_set` with a `DeprecationWarning`
- `gpu.state.point_size_set` overwrites the `size` uniform unless `gpu.state.program_point_size_set(True)`
- `gpu.texture.from_image` shares cached GPU storage under the image texture size and bit-depth limits
- Float image uploads preserve color space; byte `SRGB8_A8` textures decode sRGB during shader sampling
- Scene-linear textures draw through `SCENE_LINEAR_TO_REC709_SRGB` or `draw_texture_2d(..., is_scene_linear_with_rec709_srgb_target=True)`
- `gpu_extras.presets.draw_circle_2d` and `draw_texture_2d` build a batch per call
- `batch_for_shader` content keys match shader attributes, extra keys raising `ValueError`, missing ones `KeyError`, and unequal lengths `ValueError`
- Batch content takes `float32` numpy arrays shaped `(N, k)`, `k` the attribute width, and `indices=` shares vertices between triangles
- `float64` content builds and draws nothing with no raise, and a reversed view (`[::-1]`, `np.flipud`) crashes Blender in `batch_for_shader`
- Shapes of one shader share one batch, per-vertex `color` on `FLAT_COLOR` and `POLYLINE_FLAT_COLOR` replacing per-shape draws under a `color` uniform

Handlers draw the batch their `args` hold and read theme color and line scale at each draw:

```python
def stroked(batch: gpu.types.GPUBatch, /, *, width: float, alpha: float = 1.0, color: Callable[[bpy.types.ThemeUserInterface], Color]) -> None:
    shader = gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR")
    for preferences in root_in(bpy.types.Preferences, bpy.context).to_list():
        with Canvas.blended("ALPHA"):
            shader.uniform_float("viewportSize", gpu.state.viewport_get()[2:])
            shader.uniform_float("lineWidth", width * preferences.system.pixel_size)
            shader.uniform_float("color", (*np.asarray(color(preferences.themes[0].user_interface)), alpha))
            batch.draw(shader)
```

`SpaceView3D.draw_handler_add(partial(stroked, width=<pixels>, color=lambda ui: ui.<member>), (batch_for_shader(gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR"), "LINE_STRIP", {"pos": points}),), "WINDOW", "POST_VIEW")` draws a world-space line at a caller's interface-pixel width in a theme color. `Canvas.blended` from `rasm_bpy.ui` restores the entry blend mode after a raise.

## [05]-[CUSTOM_SHADERS]

Custom shaders come from `gpu.shader.create_from_info` over a `gpu.types.GPUShaderCreateInfo`:
- `vertex_in(<slot>, <type>, <name>)` declares each attribute `batch_for_shader` fills by name
- `push_constant("MAT4", "ModelViewProjectionMatrix")` receives the `gpu.matrix` stack at each `batch.draw`
- `push_constant("BOOL", "srgbTarget")` reads `True` on a window's sRGB framebuffer, where fragments linearize sRGB colors to match built-in shaders
- Metal sources leave `blender_srgb_to_framebuffer_space` undeclared
- `GPUStageInterfaceInfo(<name>)` with `smooth`, `flat`, or `no_perspective` members passes values to the fragment stage through `vertex_out`
- Per-shape values repeat per vertex and reach fragments through `flat` members
- Batches take no per-instance attribute, and `vertbuf_add` adds per-vertex ones
- `sampler(<slot>, <type>, <name>)` declares a texture, bound through `shader.uniform_sampler(<name>, <texture>)`
- Push constants past 128 bytes print a portability warning at declaration and compile
- Data past 128 bytes goes through `uniform_buf` over a `typedef_source` struct
- `GPUUniformBuf` values for `shader.uniform_block` stay bound to a name through `batch.draw`, inline buffers freeing first and drawing nothing
- Sources compile to Metal Shading Language on macOS, MSL keywords (`vertex`, `fragment`, `kernel`) being reserved names
- Matrix constructors under Metal take one scalar, all scalars, whole columns, or a square matrix, a split column failing to compile
- Compile errors raise a plain `Exception` and log the detail under `gpu.shader`

Shapes built-in shaders lack draw as a signed distance over one quad per shape, every shape of a kind in one batch:

```python
WIDTHS = {"rect": 4, "fill": 4, "edge": 4, "shape": 2}
BOX = np.dtype([(name, np.float32, width) for name, width in WIDTHS.items()])


@cache
def boxed() -> gpu.types.GPUShader:
    stage = gpu.types.GPUStageInterfaceInfo("box_interface")
    stage.smooth("VEC2", "local")
    info = gpu.types.GPUShaderCreateInfo()
    info.vertex_in(0, "VEC2", "corner")
    for slot, (name, width) in enumerate(WIDTHS.items(), start=1):
        info.vertex_in(slot, kind := ("FLOAT", "VEC2", "VEC3", "VEC4")[width - 1], name)
        stage.flat(kind, f"box_{name}")
    info.vertex_out(stage)
    info.push_constant("MAT4", "ModelViewProjectionMatrix")
    info.push_constant("BOOL", "srgbTarget")
    info.fragment_out(0, "VEC4", "fragColor")
    info.vertex_source(f"void main() {{ {' '.join(f'box_{name} = {name};' for name in WIDTHS)} local = corner * rect.zw; gl_Position = ModelViewProjectionMatrix * vec4(rect.xy + local, 0.0, 1.0); }}")
    info.fragment_source("""
        vec3 decoded(vec3 srgb) { return srgbTarget ? mix(srgb / 12.92, pow((srgb + 0.055) / 1.055, vec3(2.4)), step(vec3(0.04045), srgb)) : srgb; }
        void main() {
            vec2 q = abs(local - 0.5 * box_rect.zw) - 0.5 * box_rect.zw + box_shape.x;
            float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - box_shape.x;
            float rim = clamp(0.5 + (d + box_shape.y) / fwidth(d), 0.0, 1.0) * box_edge.a;
            float body = box_fill.a * (1.0 - rim);
            fragColor = vec4(decoded(box_edge.rgb) * rim + decoded(box_fill.rgb) * body, rim + body) * clamp(0.5 - d / fwidth(d), 0.0, 1.0);
        }
    """)
    return gpu.shader.create_from_info(info)


def framed(rows: NDArray[np.void], /) -> gpu.types.GPUBatch:
    corners = np.tile(np.array(((0.0, 0.0), (1.0, 0.0), (1.0, 1.0), (0.0, 0.0), (1.0, 1.0), (0.0, 1.0)), np.float32), (len(rows), 1))
    return batch_for_shader(boxed(), "TRIS", {"corner": corners} | {name: np.repeat(rows[name], 6, axis=0) for name in WIDTHS})
```

`framed(np.array(<rows>, BOX))` takes one row per frame: `rect` as pixel origin and size, `fill` and `edge` as sRGB colors, `shape` as corner radius and line width. Fragments write premultiplied color, and `framed(rows).draw(boxed())` runs under `Canvas.blended("ALPHA_PREMULT")` like `stroked`. `fwidth` of the signed distance keeps edges one pixel soft at any scale. `boxed` compiles at first call inside a GPU context, and a raise caches nothing.

## [06]-[OFFSCREEN]

Offscreen buffers draw outside a window:
- `gpu.types.GPUOffScreen(<width>, <height>, format=<format>)` takes `RGBA8`, `RGBA16`, `RGBA16F`, or `RGBA32F`
- Allocations clamp dimensions to at least one; reads and pixel extents take the allocated buffer's `width` and `height`
- Buffer dimensions stay within `gpu.capabilities.max_texture_size_get()`
- `GPUOffScreen` construction binds the default framebuffer; allocate before entering another framebuffer's drawing scope
- Framebuffer scopes restore binding; save and restore viewport and scissor rectangles across backend bind resets
- Offscreen buffers bind in any window's GPU context, outliving the window that made them, and access after `free()` raises `ReferenceError`
- Offscreen framebuffers are no sRGB target, `srgbTarget` reading `False` and colors writing as given
- `read_color(<x>, <y>, <width>, <height>, <channels>, <slot>, "FLOAT")` returns a `[height, width, channels]` float buffer bottom row first
- `"FLOAT"` reads normalize integer formats and retain floating values; `texture_color.read()` returns `RGBA8` texels as bytes
- `draw_view3d(scene, view_layer, view3d, region, view_matrix, projection_matrix, do_color_management=<bool>)` uses `view3d` shading and overlays
- Camera views take `camera.calc_matrix_camera(depsgraph, x=<width>, y=<height>)` as projection and `matrix_world.normalized().inverted()` as view
- `gpu.types.Buffer` borrows buffer-protocol storage and retains its owner; sequences copy, and borrowed dtype matches GPU format

Panels render previews through owned color and depth textures attached to a framebuffer, pixels bottom row first as preview buffers store them:

```python
def rendered(draw: Callable[[], None], extent: float, size: int, /) -> NDArray[np.float32]:
    color = gpu.types.GPUTexture((size, size), format="RGBA8")
    depth = gpu.types.GPUTexture((color.width, color.height), format="DEPTH32F_STENCIL8")
    framebuffer = gpu.types.GPUFrameBuffer(depth_slot=depth, color_slots=color)
    with ExitStack() as scope:
        scope.callback(gpu.state.viewport_set, *gpu.state.viewport_get())
        scope.callback(gpu.state.scissor_set, *gpu.state.scissor_get())
        scope.enter_context(framebuffer.bind())  # type: ignore[func-returns-value]  # ty: ignore[invalid-argument-type]
        gpu.matrix.push()
        scope.callback(gpu.matrix.pop)
        gpu.matrix.push_projection()
        scope.callback(gpu.matrix.pop_projection)
        framebuffer.clear(color=(0.0, 0.0, 0.0, 0.0), depth=1.0)
        gpu.matrix.load_identity()
        gpu.matrix.load_projection_matrix(Matrix.Translation((-1.0, -1.0, 0.0)) @ Matrix.Diagonal((2.0 / extent, 2.0 / extent, 1.0, 1.0)))
        draw()
        return np.asarray(framebuffer.read_color(0, 0, color.width, color.height, 4, 0, "FLOAT"), np.float32)


def pictured(previews: bpy.utils.previews.ImagePreviewCollection, name: str, draw: Callable[[], None], extent: float, /) -> int:
    preview: bpy.types.ImagePreview = previews.get(name) or previews.new(name)
    pixels = rendered(draw, extent, bpy.app.render_preview_size)
    preview.image_size = preview.icon_size = (pixels.shape[1], pixels.shape[0])
    pixels = pixels.ravel()
    preview.image_pixels_float.foreach_set(pixels)
    preview.icon_pixels_float.foreach_set(pixels)
    return preview.icon_id
```

Panel `draw` calls `pictured(previews, <name>, partial(stroked, batch, width=<pixels>, color=lambda ui: ui.<member>), <extent>)` once per change of drawn data or interface scale and shows the returned id through `layout.template_icon(icon_value=<id>, scale=<n>)`. Projection maps drawing coordinates over `[0, extent]` on both axes onto the buffer, and both matrix stacks return to caller values, a draw handler's included.
