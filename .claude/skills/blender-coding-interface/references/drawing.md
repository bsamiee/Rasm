# [DRAWING]

Covers `gpu` and `blf` code that draws in editor regions and offscreen buffers.

## [01]-[HANDLERS]

Draw handler types decide coordinates, and a region the space lacks or `args=None` raises `TypeError`:

| [INDEX] | [TYPE]                  | [DRAWS]                                                        |
| :-----: | :---------------------- | :------------------------------------------------------------- |
|  [01]   | `POST_PIXEL`            | Region pixels after the region                                 |
|  [02]   | `POST_VIEW`, `PRE_VIEW` | View matrices in the 3D View, View2D coordinates in 2D editors |
|  [03]   | `BACKDROP`              | Node editor `WINDOW` region pixels before the backdrop         |

Handler bodies draw over editor state they restore:
- Handlers name the drawing editor in `context.area` and run with no write guard
- Handlers draw while overlays are off, and overlay drawing returns early on a false `space.overlay.show_overlays`
- `preferences.system.use_region_overlap` extends 3D View, Image Editor, and node editor `WINDOW` regions under their toolbar, sidebar, asset shelf, and tool header, and drawing insets by them
- Blending restores the mode `gpu.state.blend_get()` read on entry
- Handlers enabling the scissor test disable it before returning, `gpu.state` holding no read for it
- Nested clips set the intersection of their box with `gpu.state.scissor_get()` and restore the box they read
- `POST_VIEW` bodies call `gpu.state.depth_test_set('LESS_EQUAL')` to hide geometry behind objects and restore the prior test after

## [02]-[COORDINATES]

Each coordinate space converts through the host member that owns it:
- `event.mouse_x` and `mouse_y` hold window pixels, `mouse_region_x` and `mouse_region_y` pixels of the region the event reached
- Region pixels are device pixels, `preferences.system.ui_scale` reading 2.0 on a Retina display at a user scale of 1.0
- `region.view2d.region_to_view(x, y)` and `view_to_region(x, y, clip=False)` convert between region pixels and View2D coordinates, `clip=False` answering points outside the region
- Image Editor View2D coordinates are image fractions, `view_to_region` of `(0, 0)` and `(1, 1)` giving the displayed image's corners
- Node positions draw at `location * preferences.system.ui_scale` in View2D, `view_to_region` of that product giving a node's region pixel
- `view3d_utils.region_2d_to_origin_3d(region, rv3d, xy)` and `region_2d_to_vector_3d(region, rv3d, xy)` give the view ray through a region point for `scene.ray_cast`, perspective and orthographic alike
- `view3d_utils.location_3d_to_region_2d(region, rv3d, coord, default=None)` answers `default` behind a perspective view, an `Option` at the read
- `region_3d.perspective_matrix` keeps its value after a view write until the region draws or `region_3d.update()` runs

## [03]-[THEME]

Drawings match the interface through `context.preferences` read at each draw:
- `themes[0].user_interface` holds widget colors (`wcol_regular.text` an RGB `Color`, `wcol_regular.inner` RGBA) and gizmo colors (`gizmo_primary`, `gizmo_hi`)
- Theme colors are display sRGB (`COLOR_GAMMA`) and pass through built-in shaders as they are
- Region text takes `user_interface.panel_text` in sidebars, toolbars, and Properties, `regions.channels.text` in channel regions, and `<space>.space.text` elsewhere
- Widget units measure `round(18 * system.ui_scale) + 2 * system.pixel_size` device pixels, and lines `width * system.pixel_size`
- `system.ui_scale` reads 0.0 in background runs, and `view.ui_scale` holds the user scale there
- Caches holding sizes or batches built at interface scale key on `(system.ui_scale, system.pixel_size)` and rebuild on a change

Text draws through `blf` over font `0`, the interface font:
- `blf.size(<font>, <pixels>)` applies no interface scale, and text matching the interface takes `ui_styles[0].widget.points * system.ui_scale`
- Word wrap, clipping, rotation, and shadow apply between `blf.enable(<font>, blf.WORD_WRAP)` (or `CLIPPING`, `ROTATION`, `SHADOW`) and `blf.disable`, state persisting per font
- `blf.draw` leaves blending at `NONE`

## [04]-[BUILT_IN_SHADERS]

Built-in shaders read `ModelViewProjectionMatrix` from the `gpu.matrix` stack and take `pos` as `VEC3` with 2D tuples accepted, and every shader but the `POINT_*` pair converts its sRGB colors on the window's sRGB framebuffer:

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
- `config='CLIPPED'` reads push constant `ClipModelMatrix` and a `clip_planes` uniform block (that matrix, then six world planes) bound through `shader.uniform_block` under `gpu.state.clip_distances_set(<n>)`
- `config='CLIPPED'` crashes Blender on shaders marked `Crash`
- `viewportSize` reads `gpu.state.viewport_get()[2:]` in regions and offscreen buffers alike
- `UNIFORM_COLOR`, `FLAT_COLOR`, and `SMOOTH_COLOR` draw lines one pixel wide past `line_width_set` with a `DeprecationWarning`
- `gpu.state.point_size_set` overwrites the `size` uniform unless `gpu.state.program_point_size_set(True)`
- `gpu.texture.from_image` returns the image's cached texture of scene-linear texels, byte images sampling through `SRGB8_A8`
- `from_image` textures draw through a `SCENE_LINEAR_TO_REC709_SRGB` shader or `draw_texture_2d(..., is_scene_linear_with_rec709_srgb_target=True)`
- `gpu_extras.presets.draw_circle_2d` and `draw_texture_2d` build a batch per call
- `batch_for_shader` content keys match the shader's attributes, an extra key raising `ValueError`, a missing one `KeyError`, and unequal lengths `ValueError`
- Batch content takes `float32` numpy arrays shaped `(N, k)`, `k` the attribute width, and `indices=` shares vertices between triangles
- `float64` content builds and draws nothing with no raise, and a reversed view (`[::-1]`, `np.flipud`) crashes Blender in `batch_for_shader`
- Shapes of one shader draw as one batch, per-vertex `color` on `FLAT_COLOR` and `POLYLINE_FLAT_COLOR` replacing a draw per shape under a `color` uniform

Handlers keep a frozen record of their batches, built once per change of its points, and draw through a method that reads only the record and its arguments:

```python
class Stroke(msgspec.Struct, frozen=True, kw_only=True):
    shader: gpu.types.GPUShader
    batch: gpu.types.GPUBatch
    end: tuple[float, float]

    @classmethod
    def of(cls, points: NDArray[np.float32]) -> Self:
        shader = gpu.shader.from_builtin("POLYLINE_UNIFORM_COLOR")
        return cls(shader=shader, batch=batch_for_shader(shader, "LINE_STRIP", {"pos": points}), end=tuple(points[-1, :2].tolist()))  # type: ignore[dict-item]  # ty: ignore[invalid-argument-type]

    def drawn(self, label: str, preferences: bpy.types.Preferences) -> None:
        ui = preferences.themes[0].user_interface
        with ExitStack() as scope:
            scope.callback(gpu.state.blend_set, gpu.state.blend_get())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            gpu.state.blend_set("ALPHA")
            self.shader.uniform_float("viewportSize", gpu.state.viewport_get()[2:])
            self.shader.uniform_float("lineWidth", 2.0 * preferences.system.pixel_size)
            self.shader.uniform_float("color", (*np.asarray(ui.gizmo_primary), 1.0))
            self.batch.draw(self.shader)
            blf.size(0, preferences.ui_styles[0].widget.points * preferences.system.ui_scale)
            blf.color(0, ui.wcol_regular.text.r, ui.wcol_regular.text.g, ui.wcol_regular.text.b, 1.0)
            blf.position(0, *self.end, 0.0)
            blf.draw(0, label)
```

Handler bodies call `drawn` on the `Stroke` cached for current points with `context.preferences`, and the scope restores the entry blend mode after a raise and after `blf.draw`.

## [05]-[CUSTOM_SHADERS]

Custom shaders come from `gpu.shader.create_from_info` over a `gpu.types.GPUShaderCreateInfo`:
- `vertex_in(<slot>, <type>, <name>)` declares each attribute `batch_for_shader` fills by name
- `push_constant("MAT4", "ModelViewProjectionMatrix")` receives the `gpu.matrix` stack at each `batch.draw`
- `push_constant("BOOL", "srgbTarget")` receives `True` on the window's sRGB framebuffer, and the fragment converts sRGB colors to linear there to match built-in shaders
- `GPUStageInterfaceInfo(<name>)` with `smooth`, `flat`, or `no_perspective` members passes values to the fragment stage through `vertex_out`
- `sampler(<slot>, <type>, <name>)` declares a texture, bound through `shader.uniform_sampler(<name>, <texture>)`
- Push constants past 128 bytes print a portability warning at declaration and compile, and larger data goes through `uniform_buf` over a `typedef_source` struct
- `GPUUniformBuf` values passed to `shader.uniform_block` stay bound to a name through `batch.draw`, an inline buffer freeing before the draw and drawing nothing
- Sources compile to Metal Shading Language on macOS, MSL keywords (`vertex`, `fragment`, `kernel`) being reserved names
- Matrix constructors under Metal take one scalar, all scalars, whole columns, or a square matrix, a split column failing to compile
- Compile errors raise a plain `Exception` and log the detail under `gpu.shader`

Shapes the built-in shaders lack draw from one unit quad and a uniform block describing the shape, an antialiased rounded frame here:

```python
FRAME = """
vec3 decoded(vec3 srgb) { return srgbTarget ? mix(srgb / 12.92, pow((srgb + 0.055) / 1.055, vec3(2.4)), step(vec3(0.04045), srgb)) : srgb; }
void main() {
    vec2 q = abs(local - 0.5 * box.rect.zw) - 0.5 * box.rect.zw + box.widths.x;
    float d = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - box.widths.x;
    float rim = clamp(0.5 + (d + box.widths.y) / fwidth(d), 0.0, 1.0) * box.edge.a;
    float body = box.fill.a * (1.0 - rim);
    fragColor = vec4(decoded(box.edge.rgb) * rim + decoded(box.fill.rgb) * body, rim + body) * clamp(0.5 - d / fwidth(d), 0.0, 1.0);
}
"""


@cache
def framing() -> Result[tuple[gpu.types.GPUShader, gpu.types.GPUBatch], Exception]:
    stage = gpu.types.GPUStageInterfaceInfo("framing_interface")
    stage.smooth("VEC2", "local")
    info = gpu.types.GPUShaderCreateInfo()
    info.typedef_source("struct Box { vec4 rect; vec4 fill; vec4 edge; vec4 widths; };")
    info.uniform_buf(0, "Box", "box")
    info.vertex_in(0, "VEC2", "pos")
    info.vertex_out(stage)
    info.push_constant("MAT4", "ModelViewProjectionMatrix")
    info.push_constant("BOOL", "srgbTarget")
    info.fragment_out(0, "VEC4", "fragColor")
    info.vertex_source("void main() { local = pos * box.rect.zw; gl_Position = ModelViewProjectionMatrix * vec4(box.rect.xy + local, 0.0, 1.0); }")
    info.fragment_source(FRAME)
    return catch(gpu.shader.create_from_info, exception=Exception)(info).map(
        lambda shader: (shader, batch_for_shader(shader, "TRI_STRIP", {"pos": ((0.0, 0.0), (1.0, 0.0), (0.0, 1.0), (1.0, 1.0))}))
    )


def framed(rect: tuple[float, float, float, float], fill: tuple[float, ...], edge: tuple[float, ...], radius: float, line: float) -> Result[None, Exception]:
    def drawn(program: tuple[gpu.types.GPUShader, gpu.types.GPUBatch]) -> None:
        shader, quad = program
        box = gpu.types.GPUUniformBuf(gpu.types.Buffer("FLOAT", 16, (*rect, *fill, *edge, radius, line, 0.0, 0.0)))
        shader.uniform_block("box", box)
        with ExitStack() as scope:
            scope.callback(gpu.state.blend_set, gpu.state.blend_get())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            gpu.state.blend_set("ALPHA_PREMULT")
            quad.draw(shader)

    return framing().map(drawn)
```

`Box` packs four `vec4` rows in std140 order, the last holding radius and line width in pixels. The fragment answers premultiplied color for `ALPHA_PREMULT`, and `fwidth` of the signed distance keeps the edge one pixel soft at any scale. `framing` compiles at its first call and returns the cached `Result` to every later call, a first call outside a GPU context holding its `SystemError` for the session.

## [06]-[OFFSCREEN]

Offscreen buffers draw away from the window:
- `gpu.types.GPUOffScreen(<width>, <height>, format=<format>)` takes `RGBA8`, `RGBA16`, `RGBA16F`, or `RGBA32F`
- Sizes past `gpu.capabilities.max_texture_size_get()` construct and abort Blender at the buffer's first bind
- Offscreen buffers bind in any window's GPU context, outliving the window that made them, and access after `free()` raises `ReferenceError`
- Offscreen framebuffers are no sRGB target, `srgbTarget` reading `False` and colors writing as given
- `read_color(<x>, <y>, <width>, <height>, <channels>, <slot>, "FLOAT")` returns a `[height, width, channels]` float buffer bottom row first
- `"FLOAT"` reads normalize every format, `texture_color.read()` returning `RGBA8` texels as bytes
- `draw_view3d(scene, view_layer, view3d, region, view_matrix, projection_matrix, do_color_management=<bool>)` draws the shading mode the 3D View shows with its overlays
- Camera views take `camera.calc_matrix_camera(depsgraph, x=<width>, y=<height>)` as projection and `matrix_world.normalized().inverted()` as view

Panels show a custom drawing as a preview icon drawn offscreen, its pixels bottom row first as preview buffers store them:

```python
def pictured(previews: bpy.utils.previews.ImagePreviewCollection, slot: str, size: int, draw: Callable[[], None]) -> Option[int]:
    def painted(offscreen: gpu.types.GPUOffScreen) -> NDArray[np.float32]:
        with ExitStack() as scope:
            scope.callback(offscreen.free)
            scope.enter_context(offscreen.bind())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            scope.enter_context(gpu.matrix.push_pop())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            scope.enter_context(gpu.matrix.push_pop_projection())  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            framebuffer = gpu.state.active_framebuffer_get()
            framebuffer.clear(color=(0.0, 0.0, 0.0, 0.0))
            gpu.matrix.load_identity()
            gpu.matrix.load_projection_matrix(Matrix.Translation((-1.0, -1.0, 0.0)) @ Matrix.Diagonal((2.0 / size, 2.0 / size, 1.0, 1.0)))
            draw()
            return np.asarray(framebuffer.read_color(0, 0, size, size, 4, 0, "FLOAT"), np.float32).ravel()

    def filled(pixels: NDArray[np.float32]) -> int:
        preview: bpy.types.ImagePreview = previews.get(slot) or previews.new(slot)
        preview.image_size = preview.icon_size = (size, size)  # type: ignore[assignment]
        preview.image_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
        preview.icon_pixels_float.foreach_set(pixels)  # type: ignore[arg-type]
        return preview.icon_id

    return Some(filled(painted(gpu.types.GPUOffScreen(size, size)))) if size <= gpu.capabilities.max_texture_size_get() else Nothing
```

`layout.template_icon(icon_value=pictured(previews, <slot>, <size>, partial(stroke.drawn, label, preferences)).default_value(0), scale=<n>)` shows a handler's drawing in a panel, the pixel projection placing `POST_PIXEL` coordinates as a region does. Both matrix stacks return to the caller's values, a draw handler included, and redraws call `pictured` again only after the drawing's data or interface scale changes.
