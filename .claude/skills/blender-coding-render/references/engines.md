# [ENGINES]

## [01]-[DECLARATION]

Engine classes derive from `bpy.types.RenderEngine` directly, `bl_idname` naming the `render.engine` value:
- `bl_use_*` flags state their effect in `bl_rna.properties[<flag>].description`
- Effect stacks skip results of engines with `bl_use_postprocess` off, and `bl_use_image_save` off holds only with it off
- Subclasses of a registered engine class leave the parent with no Python class, its EXR renders crashing Blender
- Engines sharing a body build each class through `type(<name>, (bpy.types.RenderEngine,), <namespace>)`

## [02]-[PASSES]

`update_render_passes` registers each pass through `register_pass(scene, renderlayer, name, channels, chanid, type)`, registered passes appearing in `begin_result` layers and writing `<layer>.<name>` parts, and `Combined` present without registration.

## [03]-[RESULTS]

`render(depsgraph)` writes through `begin_result(x, y, w, h, layer=, view=)` and `end_result(result, cancel=self.test_break())`:
- `resolution_x` and `resolution_y` hold the size `resolution_percentage` scales
- `RenderPass.rect.foreach_set` takes a flat `float32` buffer of `w * h * channels`, bottom row first
- Tiles sit at their bottom-left offset, `update_progress` and `test_break` running between tiles
- `is_animation`, `is_preview`, `camera_override`, and `layer_override` state what the call renders

One table of passes feeds registration and writes, each pass a function of pixel-center coordinates:

```python
type Pass = Callable[[NDArray[np.float32]], NDArray[np.float32]]


class Engine(bpy.types.RenderEngine):
    bl_idname = "ITEM_ENGINE"
    bl_label = "Item"
    bl_use_postprocess = True

    passes: ClassVar[MappingProxyType[tuple[str, str], Pass]] = MappingProxyType({
        ("Combined", "RGBA"): lambda uv: np.dstack((uv, np.zeros_like(uv[..., :1]), np.ones_like(uv[..., :1]))),
        ("Coverage", "X"): lambda uv: uv[..., :1],
    })

    @override
    def update_render_passes(self, scene: bpy.types.Scene | None = None, renderlayer: bpy.types.ViewLayer | None = None) -> None:
        for name, chanid in self.passes:
            self.register_pass(scene, renderlayer, name, len(chanid), chanid, "VALUE" if len(chanid) == 1 else "COLOR" if chanid in {"RGB", "RGBA"} else "VECTOR")

    @override
    def render(self, depsgraph: bpy.types.Depsgraph | None) -> None:  # type: ignore[override]
        width, height = self.resolution_x, self.resolution_y
        uv: NDArray[np.float32] = np.stack(np.meshgrid((np.arange(width, dtype=np.float32) + 0.5) / width, (np.arange(height, dtype=np.float32) + 0.5) / height), axis=-1)
        if (result := self.begin_result(0, 0, width, height)) is not None:
            for (name, _), fill in self.passes.items():
                result.layers[0].passes[name].rect.foreach_set(np.ascontiguousarray(fill(uv)).ravel())  # type: ignore[arg-type]
            self.end_result(result, cancel=self.test_break())
```

Stubs declare `render` as a `RenderSettings` member and as a method, and the method line suppresses `override`.

## [04]-[VIEWPORT]

`view_update(context, depsgraph)` and `view_draw(context, depsgraph)` run in a GUI viewport set to Rendered shading:
- `view_update` syncs changed IDs from `depsgraph.updates`, and `tag_redraw()` and `tag_update()` request the next draw or sync
- `view_draw` draws scene-linear pixels between `bind_display_space_shader(scene)` and `unbind_display_space_shader()`, the shader applying scene view and display

## [05]-[BAKES]

Python engines bake nothing through `bake(depsgraph, object, pass_type, pass_filter, width, height)`:
- `get_result()` holds `Combined` alone, with no pixel-to-surface data, and writes there reach no image
- `begin_result` inside `bake` crashes Blender
