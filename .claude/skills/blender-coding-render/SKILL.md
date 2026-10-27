---
name: blender-coding-render
description: "Use when writing or reviewing Python that sets Blender render, compositor, or sequencer state or defines a render engine, covering the output pipeline"
---

# [BLENDER_CODING_RENDER]

Covers `bpy` code over render settings, color management, view layers, compositor effects, renders and their files, bakes, images, strips, and render engines.

- Use `blender-coding` for member write order, dynamic enums, library loads, handler threads, and pixel buffers
- Use `blender-coding-nodes` for node trees

[REFERENCES]:
- [01]-[FILES](references/files.md): Written render files, from multilayer parts and cryptomatte to File Output, metadata, and OpenImageIO reads
- [02]-[ENGINES](references/engines.md): `RenderEngine` subclasses, from pass registration and result writes to viewport draws and bakes
- [03]-[SEQUENCER](references/sequencer.md): Sequencer scenes, strip creation, timing writes, effects, modifiers, and metas

## [01]-[SETTINGS]

Governing member writes reset governed members:

| [INDEX] | [GOVERNING]                       | [RESETS]                                                                                     |
| :-----: | :-------------------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `image_settings.media_type`       | `file_format` outside the new media to `PNG`, `OPEN_EXR_MULTILAYER`, or `FFMPEG`             |
|  [02]   | `image_settings.media_type`       | `ffmpeg` at `video_bitrate` 0 to `MKV`, `H264`, `constant_rate_factor` `MEDIUM`, and 6000    |
|  [03]   | `image_settings.file_format`      | `color_mode` and `color_depth` the format lacks to its first (`JPEG` `RGB` 8, `OPEN_EXR` 32) |
|  [04]   | `image_settings.file_format`      | `views_format` `MULTIVIEW` off EXR to `INDIVIDUAL`                                           |
|  [05]   | `image_settings.file_format`      | `linear_colorspace_settings.name` to the working space on a linear format                    |
|  [06]   | `display_settings.display_device` | `view_transform` to a variant of its family the display offers, else `Standard`              |
|  [07]   | `view_settings.view_transform`    | `look` the new view lacks to `None`                                                          |

Governing writes keep every other member and refuse values outside live items:
- `file_format` writes keep `exr_codec` and a `linear_colorspace_settings.name` naming a data space (`Non-Color`)
- Display writes keep `look` and move views both ways (`AgX` to `AgX - HDR 1000 nits` under `Rec.2100-PQ`, and back)
- `file_format` writes refuse a format outside the current media with `TypeError`, `MULTI_LAYER_IMAGE` holding `OPEN_EXR_MULTILAYER` alone
- `linear_colorspace_settings.name` reads `""` with an RNA warning until the first linear format write, and refuses `""` with `TypeError`

Records declare each governing member before the members it resets (`display_settings` before `view_settings` on a scene, `image_settings` before `ffmpeg` on `render`). `assigned` writes each differing member and recurses into nested settings, and `held` restores the prior record on exit and yields what the host kept:

```python
type Struct = bpy.types.bpy_struct[object]


class Colorspace(msgspec.Struct, frozen=True, omit_defaults=True):
    name: str = ""


class FileFormat(msgspec.Struct, frozen=True, kw_only=True):
    media_type: str
    file_format: str
    color_mode: str
    color_depth: str
    views_format: str = "INDIVIDUAL"
    linear_colorspace_settings: Colorspace = Colorspace()


def assigned(owner: Struct, values: Mapping[str, object]) -> Result[None, TypeError]:
    def written(prior: Result[None, TypeError], item: tuple[str, object]) -> Result[None, TypeError]:
        match item:
            case name, Mapping() as nested:
                return prior.bind(lambda _: assigned(getattr(owner, name), nested))
            case name, value if value != getattr(owner, name):
                return prior.bind(lambda _: catch(setattr, exception=TypeError)(owner, name, value))
            case _:
                return prior

    return reduce(written, values.items(), Result[None, TypeError](ok=None))


@contextmanager
def held[T: msgspec.Struct](owner: Struct, record: T) -> Generator[Result[T, TypeError]]:
    kind: type[T] = type(record)
    with ExitStack() as restore:
        restore.callback(assigned, owner, msgspec.to_builtins(msgspec.convert(owner, kind, from_attributes=True)))
        yield assigned(owner, msgspec.to_builtins(record)).map(lambda _: msgspec.convert(owner, kind, from_attributes=True))
```

`omit_defaults` leaves an empty `Colorspace.name` unwritten, and restores keep a linear space the scope wrote.

Extensions store render state on `Scene`:
- Nested settings (`RenderSettings`, `ImageFormatSettings`, view and display settings) refuse ID properties with `TypeError`
- `bpy.props` assigned to a nested settings class stays a class attribute, instance writes raising `AttributeError`

Render code writes every member its output reads:
- Files loaded under `--factory-startup` keep their render settings (border, crop, stamp, motion blur)
- NTSC frame rates hold `render.fps_base` 1.001
- `render.compositor_device` (`GPU` by default) and `compositor_precision` set compositor evaluation
- `GPU` compositing refuses a render past 8192 pixels a side, and results clamp each side to 8192 on `GPU` and 32768 on `CPU` with no member stating it
- Bloom comes from the stock `Bloom` effect or a Glare node
- `scene.cycles.denoiser` items vary per machine
- Cycles GPU renders set `scene.cycles.device = "GPU"`, then `compute_device_type` and each device's `use` on Cycles preferences after `refresh_devices()`

## [02]-[COLOR_MANAGEMENT]

`PyOpenColorIO.GetCurrentConfig()` returns Blender's live config, the file at `bpy.data.colorspace.ocio_config_path` with `blender:*` spaces added:
- Blender picks a config once at start from `BLENDER_OCIO`, else `OCIO`, else the bundled file
- Working-space changes rewrite the live config in place with a new `getCacheID()`, and caches key by cache ID
- `config.getDisplays()` lists `display_device` identifiers, `config.getViews(display)` the `view_transform` identifiers a display offers
- Views take looks named `<view> - <look>`, else `<family> - <look>` for the family before ` - `, else looks with no ` - `, and every view takes `None`
- `bpy.ops.wm.set_working_color_space(working_space=..., convert_colors=True)` changes the read-only working space

Colors in code follow the working space and config names:
- Colors written in code (socket defaults, light colors) are scene-linear in the working space, `COLOR_GAMMA` members (theme colors) sRGB-encoded
- `mathutils.Color` converts through `from_srgb_to_scene_linear()` and `from_scene_linear_to_<space>()` with no display, view, or look
- Colorspace writes take config names (`Linear Rec.709`, `Non-Color`), `Linear` and `Raw` raising `TypeError`
- `colorspace_settings.interop_id`, `bpy.data.colorspace.working_space_interop_id`, and `ColorSpace.getInteropID()` name spaces across configs (`data` for `Non-Color`)
- `CompositorNodeConvertColorSpace` converts by space name or interop id, `CompositorNodeConvertToDisplay` applying its own view and display settings

`DisplayViewTransform(src=<working space>, display=..., view=...)` reproduces a display write at look `None`, an `ExposureContrastTransform` before it adding `exposure`. Premultiplied buffers divide alpha out of a copy, run one CPU processor, and multiply alpha back:

```python
def displayed(config: PyOpenColorIO.Config, working: str, pixels: NDArray[np.float32], display: str, view: str) -> Result[NDArray[np.float32], PyOpenColorIO.Exception]:
    def encoded(processor: PyOpenColorIO.Processor) -> NDArray[np.float32]:
        buffer, alpha = np.array(pixels, np.float32, order="C"), pixels[..., 3:]
        np.divide(buffer[..., :3], alpha, out=buffer[..., :3], where=alpha > 0)
        processor.getDefaultCPUProcessor().applyRGBA(buffer)
        np.multiply(buffer[..., :3], alpha, out=buffer[..., :3], where=alpha > 0)
        return buffer

    return catch(config.getProcessor, exception=PyOpenColorIO.Exception)(PyOpenColorIO.DisplayViewTransform(src=working, display=display, view=view)).map(encoded)
```

`Baker` writes a display, view, and look as LUT text in a format `Baker.getFormats()` lists, `setDisplayView` replacing its empty target space:

```python
def cubed(config: PyOpenColorIO.Config, space: str, display: str, view: str, look: str, size: int) -> Result[str, PyOpenColorIO.Exception]:
    baker: PyOpenColorIO.Baker = PyOpenColorIO.Baker(config=config, format="resolve_cube", inputSpace=space, targetSpace="", looks="" if look == "None" else look, cubeSize=size)
    baker.setDisplayView(display, view)
    return catch(baker.bake, exception=PyOpenColorIO.Exception)()  # type: ignore[return-value]  # ty: ignore[invalid-return-type]
```

LUT bakes hold the OCIO transform alone:
- `view_settings.exposure`, `gamma`, white balance, and `curve_mapping` stay out of the cube
- Cubes from a log input space (`ACEScct`) cover HDR input, a linear input space covering 0 to 1 alone
- Host operations with no OCIO form (strip modifiers, effects) bake by rendering an identity lattice through them and fitting the result

## [03]-[VIEW_LAYERS_AND_PASSES]

Every view layer with `use` on renders and writes its passes:
- `scene.view_layers.new(name)` starts with `use` and `use_pass_combined` on, its `samples` at 0 taking the scene's count
- `bpy.ops.scene.view_layer_add(type="COPY")` copies the active layer, background runs included
- `view_layer.layer_collection` children set `exclude`, `holdout`, and `indirect_only` per collection
- `holdout` collections render with alpha 0, and `indirect_only` collections reach Cycles bounces alone while EEVEE renders them directly
- `view_layer.material_override` and `world_override` replace every material and the world in their layer

Pass flags sit on their engine's owner:

| [INDEX] | [OWNER]             | [HOLDS]                                                                       |
| :-----: | :------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `view_layer`        | Shared `use_pass_*` and `use_pass_cryptomatte_*` flags, `aovs`, `lightgroups` |
|  [02]   | `view_layer.cycles` | `use_pass_shadow_catcher`, `use_pass_volume_*`, `denoising_store_passes`      |
|  [03]   | `view_layer.eevee`  | `use_pass_transparent`, `use_pass_volume_direct`, `denoising_store_passes`    |

Engines write the passes they support, in an order of their own, and reads find parts by display name:
- Cycles writes no part for `use_pass_shadow` or `use_pass_subsurface_*`
- Cycles adds `Noisy Image`, and `Noisy Shadow Catcher` beside a shadow catcher, while scene and layer `use_denoising` are both on
- EEVEE writes no UV, index, indirect, transmission, subsurface, or light group part, and writes `Volume Direct` whenever flagged
- `view_layer.eevee.use_pass_bloom` writes overwrite every EEVEE pass flag, `True` clearing transparent and volume flags and writing no part
- Cycles and EEVEE write `Grease Pencil` with a Grease Pencil object alone, and Workbench writes `Combined` and `Depth` alone
- `view_layer.pass_alpha_threshold` (0.5) keeps surfaces below it out of `Depth`, index, `Normal`, `UV`, and `Vector` parts

Shader AOVs write through AOV Output nodes matching their `aov_name` case for case, in Cycles and EEVEE alike:
- `view_layer.aovs.add()` takes no name and makes the new AOV active, and code names it after the call
- `VALUE` AOVs write the node's `Value` input, `COLOR` AOVs its `Color` input
- AOVs named like an enabled pass read `is_valid` false and write no part

`use_pass_cryptomatte_object`, `use_pass_cryptomatte_material`, and `use_pass_cryptomatte_asset` write parts in Cycles and EEVEE alike:
- `view_layer.pass_cryptomatte_depth` (2 to 16, 6 by default) sets the rank count
- `CompositorNodeCryptomatteV2` mattes inside the compositor through `source`, `scene`, `layer_name`, and `matte_id`, a comma-separated name list

Light groups gather emitters into Cycles parts `<layer>.Combined_<group>`, EEVEE writing none:
- `Object.lightgroup` and `World.lightgroup` join a group by its stored name, storing any string with no fault
- `view_layer.lightgroups.add(name=...)` stores dots as `_` and a taken name with `_001`, and renames rewrite each member's `lightgroup`
- Group parts sum to `Combined` once every emitter (lights, emissive meshes, world) joins a group
- Compositor Image nodes reading the written EXR expose `Combined_<group>` sockets, regrading groups with no render
- `bpy.ops.scene.view_layer_add_used_lightgroups` and `view_layer_remove_unused_lightgroups` act on `context.view_layer`

Light linking on an emitter names `light_linking.receiver_collection` and `blocker_collection`, Cycles and EEVEE honoring both:
- `collection_objects` and `collection_children` items set `light_linking.link_state`, `collection_objects` aligning by index with `objects`
- Receivers holding an `INCLUDE` member light those members alone, else every object but their `EXCLUDE` members, child collections counting
- Blocker `EXCLUDE` members cast no shadow from the emitter
- Scenes honor 64 receiver collections, 64 blocker collections, and 63 distinct memberships, links past them dropping with a stdout line

## [04]-[COMPOSITOR]

`scene.compositor_effects` holds the compositor as a stack of `SceneCompositorEffect` items over `CompositorNodeTree` groups:
- `new(name=...)` adds an active effect with `enable_for_render` and `enable_for_preview` on
- Effects run in collection order, skipping effects with no group or with `enable_for_render` off, `render.use_compositing = False` skipping the stack
- Viewport compositors read `enable_for_preview` in place of `enable_for_render`
- First enabled effects take `scene.view_layers[0]` Combined pass at their first input, transparent black while that layer's `use` is off
- Effects with no Group Output node pass transparent black on, and a last enabled one blanks the render
- Later effects take the previous effect's first output, transparent black after no output socket, opaque black after a `NodeSocketFloat` one
- Last effects lacking a color first output leave the render uncomposited
- `NodeSocketFloat` first inputs take image luminance, and later inputs their `properties.inputs` values whatever their names
- Render Layers nodes read layers and passes by name, and nodes from `nodes.new` read the context scene until code sets `scene`
- First effects with no Group Input node and no Render Layers node on the scene skip its render
- Trees evaluate as they stood at frame start, stills taking edits through `render_pre` and animation frames edits before it
- `scene.copy()` shares effect groups with its source, effects sharing one group keeping their own input values
- Effect inputs key through `scene.keyframe_insert(data_path=item.path_from_id("value"))`, per-frame values coming from keys on effect inputs or tree sockets

Stacks reconcile by removing undesired effects, adding missing ones, and moving each effect with a `find(name)` other than its index. `move_compositor_effect_to_index` acts on `context.scene` and raises `RuntimeError` at the current index:

```python
def stacked(context: bpy.types.Context, scene: bpy.types.Scene, wanted: tuple[tuple[str, bpy.types.CompositorNodeTree], ...]) -> Result[None, RuntimeError]:
    effects: bpy.types.SceneCompositorEffects = scene.compositor_effects
    groups: dict[str, bpy.types.CompositorNodeTree] = dict(wanted)
    for stale in tuple(effect for effect in effects if effect.name not in groups):
        effects.remove(stale)
    for name, group in wanted:
        if (effect := effects.get(name) or effects.new(name=name)).node_group != group:
            effect.node_group = group

    def placed(index: int, name: str) -> Result[None, RuntimeError]:
        return Ok(None) if effects.find(name) == index else catch(bpy.ops.scene.move_compositor_effect_to_index, exception=RuntimeError)(name=name, index=index).map(lambda _: None)

    with context.temp_override(scene=scene):
        return reduce(lambda moved, step: moved.bind(lambda _: placed(*step)), enumerate(groups), Result[None, RuntimeError](ok=None))
```

`compositing_nodes_essentials.blend` in `bpy.utils.system_resource("DATAFILES", path="assets/nodes")` holds stock groups:
- Groups with `allow_usage_in_scene_compositor_effect` set are effects (`Bloom`, `Vignette`, `Film Grain`), the rest helpers (`Retime`)
- Scripted groups run as effects without the flag

`CompositorNodeOutputFile` nodes in an effect group write their own files during a render:
- `directory` starts as a copy of `render.filepath` and names a folder (`<dir>/r_` writing into `<dir>/r_/`)
- Nodes start with `file_name` at `{blend_name}`, `format` at `OPEN_EXR_MULTILAYER` 32-bit with codec `NONE`, and no items
- Items with `override_node_format` take their own `format`
- Multilayer stills write `<directory>/<file_name>.exr` with one part per item, single-layer stills `<directory>/<file_name><item>.<ext>`
- Animations insert frame numbers before the extension (`sg_beauty0003.png`), and `#` runs in `file_name` take the frame on stills too
- Muted nodes and nodes with no linked input write nothing

## [05]-[RENDERS]

`bpy.ops.render.render` renders the context scene, and code picks scene, layer, frame, and camera before each call:
- `context.temp_override(scene=scene, view_layer=layer)` renders `scene` at its own frame
- Operator keyword `scene="<name>"` renders the named scene at context scene's frame and writes that frame onto it
- `INVOKE_DEFAULT` calls start a render job answering `RUNNING_MODAL` in a GUI session, background runs rendering at the call
- `frame_start` and `frame_end` override the range of one `animation=True` call, and raise `RuntimeError` on a still
- `timeline_markers.new(name, frame=...).camera = camera` binds a camera from that frame, earlier frames taking the first marker's camera
- Renders and `frame_set` write a bound camera into `scene.camera`
- `layer=` and `use_single_layer` name the layer a single-layer file writes, background runs rendering every layer with `use` on beside it
- Renders with every layer's `use` off write a transparent image, and crash Blender writing EXR while `compositor_effects` is empty
- Failed renders (no camera, `write_still` under `VIDEO`) raise `RuntimeError` and set a background exit code of 1 a later success clears
- Render Layers nodes naming another scene render it inside the call with no `render_*` handler call for it

Render handlers fire `render_init`, then per frame `render_pre`, `frame_change_pre`, `frame_change_post`, `render_post`, and `render_write`, then `render_complete` or `render_cancel`:
- `render_*` handlers take `(scene, None)`, and `render_stats` `(str, None)` repeatedly
- `render_write` fires on calls that write files, the file present by then, and refused renders run `render_pre` through `render_write` before `render_cancel`
- `render.filepath` writes in `render_init` steer every file of the call, writes in `render_pre` each animation frame's path alone
- Render Result images read `size` `(0, 0)` and no pixels in background runs, and `save_render(filepath, scene=scene)` writes the last render
- `bpy.app.is_job_running("RENDER")` reads `False` in background runs

Render calls name files from `render.filepath`, PNG output at frame 1, video under the default `MKV` container and scene range 1 to 250:

| [INDEX] | [CALL]                    | [FILEPATH]             | [WRITES]                      |
| :-----: | :------------------------ | :--------------------- | :---------------------------- |
|  [01]   | `write_still=True`        | `<dir>/a.png`          | `<dir>/a.png`                 |
|  [02]   | `write_still=True`        | `<dir>/b_####`         | `<dir>/b_####.png`            |
|  [03]   | `animation=True`          | `<dir>/b_##`           | `<dir>/b_01.png`              |
|  [04]   | `animation=True`          | `<dir>/m_##_n_###`     | `<dir>/m_##_n_001.png`        |
|  [05]   | `animation=True`          | `<dir>/c_`             | `<dir>/c_0001.png`            |
|  [06]   | `animation=True`          | `<dir>/a.png`          | `<dir>/a.png0001.png`         |
|  [07]   | `write_still=True`        | `<dir>/`               | `<dir>/Untitled.png`          |
|  [08]   | `animation=True`          | `<dir>/`               | `<dir>/0001.png`              |
|  [09]   | `write_still=True`        | `<dir>/{blend_name}_t` | `<dir>/Unsaved_t.png`         |
|  [10]   | `animation=True`, `VIDEO` | `<dir>/clip_`          | `<dir>/clip_0001-0250.mkv`    |
|  [11]   | `animation=True`, `VIDEO` | `<dir>/shot.mkv`       | `<dir>/shot.mkv`              |
|  [12]   | `animation=True`, `VIDEO` | `<dir>/shot.mp4`       | `<dir>/shot.mp40001-0250.mkv` |

Written names derive from `render.filepath`, its templates, and the scene format:
- Video names take the container extension and scene `frame_start` to `frame_end` whatever the call's range
- `render.filepath` and File Output paths resolve templates (`{blend_name}`, `{scene_name}`, `{camera_name}`, `{resolution_x}`, `{fps}`)
- Text joined into a template path doubles its braces (`{{`), a lone brace raising `RuntimeError` at `frame_path` and render
- `render.frame_path(frame=..., view=...)` returns the written name with templates expanded, `bpy.path.abspath` leaving them in braces
- `use_file_extension` swaps a known extension of another format (`a.jpg` written as `a.png`), and off it writes no extension
- `render.file_extension` reads the suffix the scene format writes

Render settings decide what each write adds or skips:
- Renders create missing folders on the output path
- `use_overwrite = False` skips existing animation frames with no handler call and `use_placeholder` writes an empty file before each frame
- `image_settings.use_preview` writes a display-encoded JPEG beside each EXR
- Float renders written at 8 bits (8-bit PNG and video, preview JPEGs) dither by `render.dither_intensity`
- Single-layer EXR with `color_mode = "RGB"` writes no alpha channel, and reloads read alpha 1

Output color follows format and scene color management:
- Display formats (PNG, JPEG, TIFF, WEBP) hold view-transformed values, EXR, HDR, and DPX scene-linear values, and Cineon log values
- Display formats under an HDR display (`Rec.2100-PQ`) hold PQ-encoded values
- `image_settings.color_management = "OVERRIDE"` copies the scene's view and display settings once, `view_settings` then driving display formats
- `linear_colorspace_settings.name` under `OVERRIDE` names the linear formats' space (`ACEScg`), view and exposure leaving linear files untouched
- File Output files take an `OVERRIDE` item or node format, else scene `image_settings` under `OVERRIDE`, else scene display and view

Multi-view renders follow `render.use_multiview` and `render.views`, each view writing its `file_suffix`:
- `render.views_format = "STEREO_3D"` renders `left` and `right`, `"MULTIVIEW"` every view with `use`, `views.new` views starting with no suffix
- Stills write `<filepath><file_suffix>.<ext>`, animations `<filepath><frame><file_suffix>.<ext>`
- `image_settings.views_format = "STEREO_3D"` packs views in one file, `SIDEBYSIDE` doubling width and `TOPBOTTOM` height
- `image_settings.views_format = "MULTIVIEW"` writes every view into one EXR

## [06]-[BAKES]

`bpy.ops.object.bake` renders through Cycles into the image of each material's active Image Texture node:
- EEVEE and Workbench refuse with `RuntimeError("Current render engine does not support baking")`
- Bakes read `selected_objects` and `active_object`, an empty selection raising `No valid selected objects`
- Materials with an active node holding no image bake into an Image Texture node Blender picks, and code sets `nodes.active`
- Image texture bakes need an active UV map, a mesh with none raising `RuntimeError`
- `scene.render.bake` starts with `target` at `IMAGE_TEXTURES`, `save_mode` at `INTERNAL`, `use_clear` on, and `margin` 16
- `INTERNAL` saves leave baked pixels in the image until `save()` or `pack()`, byte images storing values in their colorspace

One scope adds a target node to every material, makes it active, and removes it after the bake on every path:

```python
def baked(context: bpy.types.Context, item: bpy.types.Object, image: bpy.types.Image, kind: "rna_enums.BakePassTypeItems") -> Result[bpy.types.Image, RuntimeError]:
    with ExitStack() as scope:
        for tree in {slot.material.node_tree for slot in item.material_slots if slot.material is not None and slot.material.node_tree is not None}:
            target: bpy.types.ShaderNodeTexImage = tree.nodes.new("ShaderNodeTexImage")  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            scope.callback(setattr, tree.nodes, "active", tree.nodes.active)
            scope.callback(tree.nodes.remove, target)
            target.image, tree.nodes.active = image, target
        with context.temp_override(active_object=item, selected_objects=[item]):
            return catch(bpy.ops.object.bake, exception=RuntimeError)(type=kind, target="IMAGE_TEXTURES", save_mode="INTERNAL").map(lambda _: image)
```

## [07]-[IMAGES]

Images hold a colorspace tag, a format, and an alpha mode the host applies at each read and write:
- Images take `colorspace_settings.name` before their first pixel write, a later colorspace write clearing pixels to black
- New float images take `bpy.data.colorspace.working_space`, byte images `sRGB`, and `is_data=True` images `Non-Color`
- New images hold `file_format` `PNG` and `alpha_mode` `STRAIGHT`, and `images.new` defaults to `alpha=False`, saves writing RGB alone
- Float buffers hold premultiplied alpha, byte buffers straight alpha
- Float images holding data (positions, normals, masks) take `Non-Color` and round-trip through EXR unchanged, PNG saves dividing color by alpha
- Loaded EXR images default to `alpha_mode` `PREMUL`, PNG images to `STRAIGHT`
- File images take their header's space (`colorInteropID`) at load, and a tag code writes persists in the .blend over it

`image.save()` writes `image.file_format` in its colorspace tag, path extensions choosing nothing (PNG bytes in `a.exr`):
- Saves to a new path write `file_format` and `filepath_raw`, then call `save()`, the image becoming a file image at that path
- `save(filepath=...)` turns an image into a file image with `image.filepath` unset, its pixels lost at next reload or file load
- `save()` on a generated color image retags it `sRGB` in every format, alpha below 1 writing wrong colors that `save_render()` writes right
- `save(save_copy=True)` writes an identical file and keeps the image generated
- `image.save_render(filepath, scene=scene)` writes a copy in scene `image_settings` format and color management

Script-written pixels of a generated image persist through `pack()` or a saved file alone, a fake user keeping the image without them:
- `pack()` stores float images as full-precision EXR and byte images as PNG, keeping the tag
- Packed float images with alpha below 1 keep their values across reloads with `alpha_mode = "PREMUL"`, `STRAIGHT` premultiplying again

Tiled images (`tiled=True`) save one file per tile through `<UDIM>` in `filepath_raw`:
- `tiles.new()` adds a tile with no buffer that `save()` refuses until `bpy.ops.image.tile_fill()` runs under `temp_override(edit_image=image)`
- `bpy.ops.image.tile_add(number=..., fill=True)` under the same override adds a filled tile

Linear exports write a `FileFormat` through a scratch scene under `OVERRIDE`, the image keeping its source and tag:

```python
def exported(image: bpy.types.Image, path: str, output: FileFormat) -> Result[str, TypeError | RuntimeError]:
    with ExitStack() as scope:
        scene: bpy.types.Scene = bpy.data.scenes.new(".export")
        scope.callback(bpy.data.scenes.remove, scene)
        scene.render.image_settings.color_management = "OVERRIDE"
        formatted: Result[None, TypeError] = assigned(scene.render.image_settings, msgspec.to_builtins(output))
        return Result[None, TypeError | RuntimeError].bind(formatted, lambda _: catch(image.save_render, exception=RuntimeError)(path, scene=scene)).map(lambda _: path)
```
