# [ENGINES]

## [01]-[DECLARATION]

Engine classes subclass `bpy.types.RenderEngine`, and code selects one through `scene.render.engine = Engine.bl_idname`. Flags decide which host stages run around the engine:
- `bl_use_postprocess` (on) runs compositor effects and the sequencer on each layer's result, and off hands the engine the whole frame with neither
- `bl_use_image_save = False` skips Blender's still and animation writes while `bl_use_postprocess` is on, and Blender writes files while it is off
- `bl_use_shading_nodes_custom` (on) hides Cycles and EEVEE shader nodes in the node editor, and engines reading built-in materials turn it off
- `bl_use_preview` renders material, light, and world previews in the engine at 128 by 128 with `is_preview` true and no compositor or render handler
- `bl_use_eevee_viewport` draws Material Preview shading through EEVEE
- `bl_use_gpu_context` runs `update` and `render` inside a GPU context, `gpu` offscreen drawing working in both
- Freestyle strokes render through EEVEE after the engine unless `bl_use_custom_freestyle` is on

Final renders construct the class twice per frame and keep one instance across that frame's view layers, `render.use_persistent_data` or `bl_use_gpu_context` keeping it across frames. `__init__(self, *args, **kwargs)` calls `super().__init__(*args, **kwargs)` and acquires nothing, and `update` acquires render state and holds it on `self`.

Built-in property panels poll `context.engine in cls.COMPAT_ENGINES`. `COMPAT_ENGINES` is optional class metadata; missing attributes raise `AttributeError`. Registration adds engine names to sets holding a reference engine; cleanup discards added names from the held sets.

`bpy.types.HydraRenderEngine` syncs, renders, and draws the viewport itself, and engines over a USD Hydra render delegate subclass it:
- `bl_delegate_id` names an `HdRendererPlugin` type, Blender holding `HdStormRendererPlugin` alone
- `register` classmethods add other delegates through `pxr.Plug.Registry().RegisterPlugins([<plugin folder>])`
- `get_render_settings(engine_type)` returns delegate settings for `PREVIEW`, `FINAL`, or `VIEWPORT`
- `aovToken:<pass>` settings keys bind a registered pass to a delegate AOV
- `bl_use_materialx` exports materials as MaterialX in place of `UsdPreviewSurface`

## [02]-[PASSES]

`update_render_passes(scene, renderlayer)` declares each layer's passes through `register_pass(scene, renderlayer, name, channels, chanid, type)`:
- Blender calls it per layer with `use` on at render result creation, Render Layers socket builds, and `view_layer.update_render_passes()`
- `register_pass` outside `update_render_passes` adds nothing
- `chanid` holds one letter per channel (`RGBA`, `XYZ`, `X`), and `type` is `COLOR`, `VECTOR`, or `VALUE`
- Every result layer holds `Combined` with or without registration, and a table filling passes by name holds a `Combined` entry
- Passes write as `<layer>.<name>` parts, read by compositor Render Layers nodes under `<name>`

## [03]-[RESULTS]

`update(data, depsgraph)` then `render(depsgraph)` run once per view layer with `use` on, `depsgraph.view_layer` naming the layer, and `render_frame_finish()` runs once after the last layer:
- `update` runs under the interface draw lock and holds every read of original data, and `render` runs after the lock releases on render job thread
- `begin_result(x, y, w, h)` without `layer` allocates every rendered layer, `result.layers[0]` naming the first whatever layer renders
- `begin_result` with `layer=depsgraph.view_layer.name` allocates the rendering layer alone
- `resolution_x` and `resolution_y` hold the full frame at `resolution_percentage`
- Border renders clamp tiles to a window sized `result.resolution_x` by `resolution_y` at floored float32 `border_min * resolution`
- `RenderPass.rect` reads a `(pixels, channels)` copy, and `foreach_set` takes a flat `float32` buffer, bottom row first
- Multi-view results hold each pass once per view in `layer.passes`, `view_id` naming its view
- `camera_model_matrix(camera)` reads the eye `active_view_set(view.name)` selects
- Tiles sit at their bottom-left offset in the window
- `update_result(result)` shows progress without freeing the tile, and `end_result(result, cancel=True)` frees it unmerged
- `frame_set(frame, subframe)` re-evaluates the engine's depsgraph at a subframe for motion blur
- External renderer multilayer EXRs load through `result.load_from_file(path)` or `layer.load_from_file(path)` by `<layer>.<pass>` and view
- `layer.load_from_file` takes unprefixed `<pass>` parts
- EXRs sized other than the window raise `RuntimeError` on load
- `get_result().stamp_data_add_field(field, value)` writes a header key into the written file, a tile from `begin_result` dropping it

`bake(depsgraph, object, pass_type, pass_filter, width, height)` runs after `update` once per target image, and `begin_result(0, 0, width, height, layer=depsgraph.view_layer.name)` returns the bake's surface data:
- `BakePrimitive` holds u, v, and the primitive index as `int32` bits, -1 off the baked object
- `BakeDifferential` holds du/dx, du/dy, dv/dx, and dv/dy
- `end_result` copies `Combined` into the target image at object pixels outside the margin
- `begin_result` without `layer` inside `bake` crashes Blender, and `object.bake` under an engine without `bake` raises `RuntimeError`

One table of fills over normalized coordinates registers passes, writes every layer, view, and border window, and bakes from surface UVs:

```python
type Fill = Callable[[NDArray[np.float32]], NDArray[np.float32]]


class Engine(bpy.types.RenderEngine):
    bl_idname = "ITEM_ENGINE"
    bl_label = "Item"

    passes: ClassVar[Mapping[str, tuple[str, Fill]]] = MappingProxyType({
        "Combined": ("RGBA", lambda uv: np.concatenate((uv, np.zeros_like(uv[..., :1]), np.ones_like(uv[..., :1])), axis=-1)),
        "Coverage": ("X", lambda uv: uv[..., :1]),
    })

    @override
    def update_render_passes(self, scene: bpy.types.Scene | None = None, renderlayer: bpy.types.ViewLayer | None = None) -> None:
        for name, (chanid, _) in self.passes.items():
            self.register_pass(scene, renderlayer, name, len(chanid), chanid, "COLOR" if chanid.startswith("RGB") else "VALUE" if len(chanid) == 1 else "VECTOR")

    @override
    def render(self, depsgraph: bpy.types.Depsgraph | None) -> None:
        match depsgraph:
            case bpy.types.Depsgraph(scene=bpy.types.Scene() as scene, view_layer=bpy.types.ViewLayer() as layer):
                frame = np.array((self.resolution_x, self.resolution_y), np.float32)
            case _:
                return self.report({"ERROR"}, f"{self.bl_label} received no depsgraph")
        origin = np.floor(np.array((scene.render.border_min_x, scene.render.border_min_y), np.float32) * frame) * scene.render.use_border
        if (result := self.begin_result(0, 0, self.resolution_x, self.resolution_y, layer=layer.name)) is None:
            return self.report({"ERROR"}, f"{layer.name} allocated no render result")
        uv = np.moveaxis(np.indices((result.resolution_y, result.resolution_x), np.float32)[::-1] + (origin + 0.5)[:, None, None], 0, -1) / frame
        for rendered in result.layers[0].passes:
            _, fill = self.passes[rendered.name]
            rendered.rect.foreach_set(np.ascontiguousarray(fill(uv), np.float32).ravel())
        self.end_result(result, cancel=self.test_break())

    @override
    def bake(
        self,
        depsgraph: bpy.types.Depsgraph | None,
        object: bpy.types.Object | None,
        pass_type: str | None,
        pass_filter: int | None,
        width: int | None,
        height: int | None,
    ) -> None:
        match depsgraph, width, height:
            case bpy.types.Depsgraph(view_layer=bpy.types.ViewLayer() as layer), int(), int():
                primitive = np.empty((width * height, 3), np.float32)
            case _:
                return self.report({"ERROR"}, f"{self.bl_label} received no depsgraph or size")
        if (result := self.begin_result(0, 0, width, height, layer=layer.name)) is None:
            return self.report({"ERROR"}, f"{layer.name} allocated no render result")
        result.layers[0].passes["BakePrimitive"].rect.foreach_get(primitive.ravel())
        _, fill = self.passes["Combined"]
        result.layers[0].passes["Combined"].rect.foreach_set(np.ascontiguousarray(fill(primitive[:, :2]), np.float32).ravel())
        self.end_result(result)
```

Stereo border renders write both eyes of each layer inside the border, `Coverage` reading each pixel center's horizontal frame position, and bakes write each texel's surface UV. Fills receive `(height, width, 2)` arrays in renders and `(texels, 2)` arrays in bakes, each indexing its last axis. Signatures take the optional parameters the `bpy` stubs declare, and one class pattern narrows them.

## [04]-[VIEWPORT]

`view_update(context, depsgraph)` and `view_draw(context, depsgraph)` run on the main thread for a 3D viewport in Rendered shading, one instance per viewport:
- `view_update` runs before `view_draw` on each depsgraph change, its first call holding empty `depsgraph.updates`
- Engines holding no scene state sync every object before applying `updates`
- `view_draw` supplies scene-linear pixels; viewport applies display/view/look and full scene settings with scene lights or world enabled
- `bind_display_space_shader(scene)` binds the built-in image shader; its scene argument applies no color transform
- Region-filling draws upload `gpu.types.GPUTexture((width, height), format="RGBA16F", data=gpu.types.Buffer("FLOAT", width * height * 4, pixels))`
- `gpu_extras.presets.draw_texture_2d(texture, (0, 0), width, height)` draws the texture at `context.region` size
- `tag_redraw()` requests the next `view_draw` and `tag_update()` the next `view_update`, a timer refining an image progressively calling them
- `view_pause(context)` and `view_resume(context)` bracket each interface render job
