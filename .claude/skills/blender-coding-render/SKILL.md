---
name: blender-coding-render
description: "Use when writing or reviewing Python that sets Blender render, compositor, or sequencer state, covering the output pipeline"
---

# [BLENDER_CODING_RENDER]

Covers `bpy` code that sets render settings, view layers, compositor effects, output files, images, color management, and sequencer strips.

- Use `blender-coding` for member write order, dynamic enums, library loads, handler threads, and pixel buffers
- Use `blender-coding-nodes` for node trees

[REFERENCES]:
- [01]-[FILES](references/files.md): Render files, from multilayer parts and cryptomatte to File Output and metadata, and OpenImageIO reads and writes

## [01]-[SETTINGS]

Governing member writes reset governed members, each chain listed governing member first:

| [INDEX] | [GOVERNING]                       | [RESETS]                                                                                       |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `image_settings.media_type`       | `file_format` outside the new media to its default                                             |
|  [02]   | `image_settings.file_format`      | `color_mode`, `color_depth`, `views_format`, and `linear_colorspace_settings` the format holds |
|  [03]   | `display_settings.display_device` | `view_transform` to a variant of its family the display offers, else `Standard`                |
|  [04]   | `view_settings.view_transform`    | `look` to a look the new view offers, matched by name after ` - `, else `None`                 |

Chain writes follow host rules:
- Media defaults are `PNG` for `IMAGE`, `OPEN_EXR_MULTILAYER` for `MULTI_LAYER_IMAGE`, and `FFMPEG` for `VIDEO`, each reset following `file_format` write rules
- `VIDEO` writes with `ffmpeg.video_bitrate` at 0 apply the H264 preset (`MKV`, `H264`, `MEDIUM`, bitrate 6000), and ffmpeg members write after `media_type`
- `file_format` writes refuse a format outside the current media with `TypeError`, and `MULTI_LAYER_IMAGE` holds `OPEN_EXR_MULTILAYER` alone
- `file_format` writes keep a `color_mode` and `color_depth` the new format holds (`OPEN_EXR` 16 to `PNG`), take its first otherwise (`JPEG` `RGB` 8, `OPEN_EXR` 32)
- `file_format` writes keep `exr_codec` and turn `MULTIVIEW` into `INDIVIDUAL` off EXR
- Linear `file_format` writes reset `linear_colorspace_settings.name` to the working space, display formats and data spaces keeping it
- `linear_colorspace_settings.name` reads `""` until the first linear `file_format` write, and writes of `""` raise `TypeError`
- Display writes keep `look`, move views both ways (`AgX` to `AgX - HDR 1000 nits` under `Rec.2100-PQ` and back), and fall back to `Standard` with no error
- View writes keep an offered look, else take its namesake after ` - ` (`AgX - Punchy` to `False Color - Punchy`, `AgX - High Contrast` to Filmic `High Contrast`)
- Restores write governing members before governed ones, a governing write resetting members restored before it

Records declaring every governed member write them in chain order, and one scope restores the prior record and yields members the host kept:

```python
from collections.abc import Generator
from contextlib import ExitStack, contextmanager
from typing import Literal, Self, TYPE_CHECKING

import bpy
from expression import Nothing, Option, Result, Some
from expression.extra.result import catch
import msgspec

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums


class FileFormat(msgspec.Struct, frozen=True, kw_only=True):
    media_type: Literal["IMAGE", "MULTI_LAYER_IMAGE", "VIDEO"]
    file_format: "rna_enums.ImageTypeAllItems"
    color_mode: "rna_enums.ImageColorModeItems"
    color_depth: "rna_enums.ImageColorDepthItems"
    views_format: "rna_enums.ViewsFormatMultiviewItems"
    linear_space: Option[str] = Nothing

    @classmethod
    def of(cls, settings: bpy.types.ImageFormatSettings) -> Self:
        linear: bpy.types.ColorManagedInputColorspaceSettings = settings.linear_colorspace_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return cls(
            media_type=settings.media_type,
            file_format=settings.file_format,
            color_mode=settings.color_mode,
            color_depth=settings.color_depth,
            views_format=settings.views_format,
            linear_space=Some(linear.name).filter(bool),
        )

    def applied(self, settings: bpy.types.ImageFormatSettings) -> Self:
        linear: bpy.types.ColorManagedInputColorspaceSettings = settings.linear_colorspace_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        settings.media_type = self.media_type
        settings.file_format = self.file_format
        settings.color_mode = self.color_mode
        settings.color_depth = self.color_depth
        settings.views_format = self.views_format
        for space in self.linear_space.to_list():
            linear.name = space  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return self.of(settings)


@contextmanager
def formatted(settings: bpy.types.ImageFormatSettings, output: FileFormat) -> Generator[Result[FileFormat, TypeError]]:
    with ExitStack() as restore:
        restore.callback(FileFormat.of(settings).applied, settings)
        yield catch(output.applied, exception=TypeError)(settings)
```

Restores keep a linear space name the scope wrote, `linear_space` reading `Nothing` while the name reads `""`.

Render settings follow host rules:
- Extensions store render state on the `Scene`, ID property writes on nested settings (`RenderSettings`, `ImageFormatSettings`, `ColorManagedViewSettings`, `ColorManagedDisplaySettings`, `SceneEEVEE`) raising `TypeError`
- `bpy.props` assigned to a nested settings class stays a class attribute, instance writes raising `AttributeError`
- `render.compositor_device` (`GPU` by default) and `compositor_precision` set compositor evaluation
- `GPU` compositing refuses a render resolution past 8192 pixels on a side with `Render size too large for GPU, use CPU compositor instead`
- Compositor results clamp each side to 8192 pixels on `GPU` and 32768 on `CPU`, caps no RNA member or `gpu.capabilities` call reads
- Files loaded under `--factory-startup` keep their render settings (border, crop, stamp, motion blur), and render code writes every member its output reads

Color management reads Blender's OCIO config:
- `PyOpenColorIO.GetCurrentConfig()` returns Blender's live config, the file at `bpy.data.colorspace.ocio_config_path` with `blender:*` spaces added
- Working-space changes rewrite the live config object in place with a new `getCacheID()`, and caches key by cache ID
- Blender picks a config once at start from `BLENDER_OCIO`, else `OCIO`, else the bundled file, and a running session keeps it
- `config.getDisplays()` lists `display_device` identifiers, `config.getViews(display)` the `view_transform` identifiers a display offers
- Views take looks named `<view> - <look>`, else `<family> - <look>` for the family before ` - ` (`AgX` for `AgX - HDR 1000 nits`)
- Views with no prefixed look take looks with no ` - ` in their name, and every view takes `None`

Engine settings sit on their engine's owner, `render.engine` taking `BLENDER_EEVEE`, `CYCLES`, or `BLENDER_WORKBENCH`:
- `scene.eevee` holds EEVEE sampling, ray tracing, fast GI, shadow, and volume members, `view_layer.eevee.ambient_occlusion_distance` the AO distance
- Bloom comes from the stock `Bloom` effect or a compositor Glare node
- `scene.cycles` holds Cycles members, `denoiser` among dynamic enums
- Cycles GPU renders set `scene.cycles.device = "GPU"`, then `compute_device_type` and each device's `use` on `context.preferences.addons["cycles"].preferences` after `refresh_devices()` fills `devices`

## [02]-[VIEW_LAYERS_AND_PASSES]

Every view layer with `use` on renders and writes its passes:
- `scene.view_layers.new(name)` starts with `use` and `use_pass_combined` on, its `samples` at 0 taking the scene's count
- `bpy.ops.scene.view_layer_add(type="COPY")` copies the active layer, background runs included
- `view_layer.layer_collection` children set `exclude`, `holdout`, and `indirect_only` per collection
- `holdout` collections render with alpha 0 in both engines, and `indirect_only` collections reach Cycles bounces alone while EEVEE renders them directly
- `view_layer.material_override` and `world_override` replace every material and the world in their layer

Pass flags sit on their engine's owner:

| [INDEX] | [OWNER]             | [HOLDS]                                                                       |
| :-----: | :------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `view_layer`        | Shared `use_pass_*` and `use_pass_cryptomatte_*` flags, `aovs`, `lightgroups` |
|  [02]   | `view_layer.cycles` | `use_pass_shadow_catcher`, `use_pass_volume_*`, `denoising_store_passes`      |
|  [03]   | `view_layer.eevee`  | `use_pass_transparent`, `use_pass_volume_direct`, `denoising_store_passes`    |

Engines write the passes they support, in an order of their own, and reads find parts by display name:
- Cycles writes no part for `use_pass_shadow` or `use_pass_subsurface_*`
- Cycles adds `Noisy Image`, and `Noisy Shadow Catcher` beside a shadow catcher pass, while `scene.cycles.use_denoising` and `view_layer.cycles.use_denoising` are both on
- EEVEE writes no UV, index, indirect, transmission, subsurface, or light group part
- EEVEE writes `Volume Direct` whenever flagged
- Cycles and EEVEE write `Grease Pencil` with a Grease Pencil object alone
- `view_layer.eevee.use_pass_bloom` writes overwrite every EEVEE pass flag, `True` clearing `use_pass_transparent` and `use_pass_volume_direct` and writing no part
- Workbench writes `Combined` and `Depth` alone
- `view_layer.pass_alpha_threshold` (0.5) keeps surfaces below the threshold out of `Depth`, index, `Normal`, `UV`, and `Vector` parts

Shader AOVs write through AOV Output nodes matching their `aov_name` case for case, in Cycles and EEVEE alike:
- `view_layer.aovs.add()` takes no name, returns `AOV`, then `AOV_001`, and makes the new AOV active
- Name writes on the active AOV store dots as `_` and a taken name with `_001`, other AOVs storing names as written
- `VALUE` AOVs write the node's `Value` input, `COLOR` AOVs its `Color` input
- AOVs named like an enabled pass read `is_valid` false and write no part

Cryptomatte flags `use_pass_cryptomatte_object`, `use_pass_cryptomatte_material`, and `use_pass_cryptomatte_asset` write parts in Cycles and EEVEE alike:
- `view_layer.pass_cryptomatte_depth` (2 to 16, 6 by default) sets the rank count
- `CompositorNodeCryptomatteV2` mattes a render inside the compositor through `source`, `scene`, `layer_name`, and `matte_id`, a comma-separated name list

Light groups gather emitters into Cycles parts `<layer>.Combined_<group>`, EEVEE writing none:
- `view_layer.lightgroups.add(name=...)` keeps spaces and Unicode, stores dots as `_`, `""` as `Lightgroup`, and a taken name with `_001`, cutting at 63 characters
- `Object.lightgroup` and `World.lightgroup` join a group by its stored name, storing any string with no fault
- Group parts sum to `Combined` once every emitter (lights, emissive meshes, world) joins a group
- Compositor Image nodes reading the written EXR expose `Combined_<group>` sockets, regrading groups with no render
- Group renames rewrite the `lightgroup` of every member
- `bpy.ops.scene.view_layer_add_used_lightgroups` and `view_layer_remove_unused_lightgroups` act on `context.view_layer`, other layers under `temp_override(view_layer=...)`

Light linking on an emitter names `obj.light_linking.receiver_collection` and `blocker_collection`, Cycles and EEVEE honoring both:
- `collection.collection_objects` and `collection_children` items set `light_linking.link_state` to `INCLUDE` or `EXCLUDE`
- `collection_objects` takes no name key and aligns by index with `collection.objects`
- Receiver collections holding an `INCLUDE` member light their `INCLUDE` members alone, and with none light every object but their `EXCLUDE` members
- Receiver collections count child collection members
- Blocker `EXCLUDE` members cast no shadow from the emitter
- Scenes honor 64 receiver collections, 64 blocker collections, and 63 distinct memberships, links past the limits dropping with a stdout line
- Emitters naming one collection count it once toward scene limits

## [03]-[COMPOSITOR]

`scene.compositor_effects` holds the compositor as a stack of `SceneCompositorEffect` items over `CompositorNodeTree` groups:
- `new(name=...)` adds an effect that becomes active with `enable_for_render` and `enable_for_preview` on, `properties` at `None` until `effect.node_group = tree`
- Effect names follow ID naming (`.001` suffix, 63 bytes), and effects sharing one group keep their own input values
- Effects run in collection order, skipping effects with no group or with `enable_for_render` off, `render.use_compositing = False` skipping the stack
- Viewport compositors read `enable_for_preview` in place of `enable_for_render`
- First enabled effects take the Combined pass of `scene.view_layers[0]` at their first input, transparent black while that layer's `use` is off
- Effects with no Group Output node in their group pass transparent black on, a last enabled one blanking the render
- Later effects take the previous effect's first output, transparent black after no output socket, opaque black after a `NodeSocketFloat` one
- Last effects lacking a color first output leave the render uncomposited
- `NodeSocketFloat` first inputs take image luminance, and later inputs their `properties.inputs` values whatever their names
- Render Layers nodes in a group read layers and passes by name
- Render Layers nodes from `nodes.new` read the context scene until code sets `scene`
- First effects with no Group Input node and no Render Layers node on the scene skip its render
- Compositor trees evaluate as they stood at frame start, stills taking edits through `render_pre` and animation frames edits before it
- Per-frame compositor values come from keyframes on effect inputs or tree sockets
- `scene.copy()` shares effect groups with its source, group edits reaching both scenes
- Effect inputs key through `scene.keyframe_insert(data_path=item.path_from_id("value"))`, the path naming `compositor_effects["<name>"].properties.inputs.<identifier>.value`

`bpy.ops.scene` effect operators act on `context.scene`, and calls on another scene run under `context.temp_override(scene=scene)`:
- `move_compositor_effect_to_index(name=..., index=...)` places an effect at `index`, raising `RuntimeError` at its current index or past the end
- Stacks reach a desired order by removing undesired effects, adding missing ones, then moving each effect with a `find(name)` differing from its index

`compositing_nodes_essentials.blend` in `bpy.utils.system_resource("DATAFILES", path="assets/nodes")` holds stock groups:
- Stock groups with `allow_usage_in_scene_compositor_effect` set are effects (`Bloom`, `Vignette`, `Film Grain`), the rest helpers for use inside a tree (`Retime`, `Rounded Square Mask`)
- Scripted groups run as effects without the flag

`CompositorNodeOutputFile` nodes in an effect group write their own files during a render:
- `directory` starts as a copy of the scene's `render.filepath` and names a folder (`<dir>/r_` writing into `<dir>/r_/`)
- Nodes start with `file_name` at `{blend_name}`, `format` at `OPEN_EXR_MULTILAYER` 32-bit with codec `NONE`, and no items
- Items with `override_node_format` take their own `format`
- Multilayer stills write `<directory>/<file_name>.exr` with one part per item, single-layer stills `<directory>/<file_name><item>.<ext>`
- Animations insert frame numbers before the extension (`sg_beauty0003.png`)
- `#` runs in `file_name` take the frame on stills too, `h_##_x` writing `h_01_xbeauty.png` at frame 1
- Muted nodes and nodes with no linked input write nothing

## [04]-[RENDERS_AND_OUTPUTS]

`bpy.ops.render.render` renders the context scene, and code picks scene, layer, frame, and camera before each call:
- `context.temp_override(scene=scene, view_layer=layer)` renders `scene` at its own frame
- Operator keyword `scene="<name>"` renders the named scene at context scene's frame and writes that frame onto it
- `INVOKE_DEFAULT` calls start a render job returning `RUNNING_MODAL` in a GUI session, and background runs render at the call returning `FINISHED`
- `frame_current` writes select the animated state a still renders
- `frame_start` and `frame_end` override the range of one `animation=True` call, and raise `RuntimeError` on a still
- `scene.timeline_markers.new(name, frame=...).camera = camera` binds a camera from that frame, frames before the first camera marker taking its camera
- Renders and `frame_set` write a bound camera into `scene.camera`
- `layer=` and `use_single_layer` name the layer a single-layer file writes, background runs rendering every layer with `use` on beside it and GUI sessions that layer alone
- Renders with every layer's `use` off write a transparent image, and crash Blender writing EXR while `compositor_effects` is empty
- Failed renders (no camera, `write_still` under `VIDEO` media) raise `RuntimeError` and set a background exit code of 1, cleared by a later successful render or an explicit `sys.exit`

Render handlers fire `render_init`, then per frame `render_pre`, `frame_change_pre`, `frame_change_post`, `render_post`, and `render_write`, then `render_complete` or `render_cancel` and a frame change restoring the scene frame:
- `render_*` handlers take `(scene, None)`, and `render_stats` `(str, None)` repeatedly
- `bpy.app.is_job_running("RENDER")` reads `False` in background runs
- `render_write` fires on calls that write files, the file present by then
- Refused renders run `render_pre` through `render_write` before `render_cancel`
- `render.filepath` writes in `render_init` steer every file of the call, writes in `render_pre` each animation frame's path alone
- Render Result images (`type == "RENDER_RESULT"`) read `size` `(0, 0)` and no pixels in background runs, and their `save_render(filepath, scene=scene)` writes the last render
- Handler exceptions print a traceback, and the render continues

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

Output names follow host rules:
- Video names take container extension and scene `frame_start` to `frame_end` whatever the call's range
- `render.filepath` and File Output paths resolve templates (`{blend_name}`, `{scene_name}`, `{camera_name}`, `{resolution_x}`, `{fps}`), `{blend_name}` reading `Unsaved` in an unsaved file
- Text joined into a template path doubles its braces (`{{`), a lone brace raising `RuntimeError` at `frame_path` and render, and template values expand as stored
- Animation names match `render.frame_path(frame=...)`, which expands templates `bpy.path.abspath` leaves in braces
- Stills keep `#` runs in `render.filepath` literal
- `use_file_extension` swaps a known extension of another format (`a.jpg` written as `a.png`), and off it writes no extension
- `render.file_extension` reads the suffix the scene format writes (`.png`, `.exr`)

Output writes follow host rules:
- Renders create missing folders on the output path
- `use_overwrite = False` skips existing animation frames with no handler call and `use_placeholder` writes an empty file before each frame, stills ignoring both
- `image_settings.use_preview` writes a display-encoded JPEG beside each EXR
- Float renders written at 8 bits (8-bit PNG and video, preview JPEGs) dither by `render.dither_intensity`, deeper files staying undithered
- Single-layer EXR with `color_mode = "RGB"` writes no alpha channel, and reloads read alpha 1

Output color follows format and scene color management:
- Display formats (PNG, JPEG, TIFF, WEBP) hold view-transformed values, EXR, HDR, and DPX scene-linear values, and Cineon log-encoded values
- Display formats under an HDR display (`Rec.2100-PQ`) hold PQ-encoded values
- `image_settings.color_management = "OVERRIDE"` copies the scene's view and display settings once, `view_settings` then driving display formats
- `linear_colorspace_settings.name` under `OVERRIDE` names the linear formats' space (`ACEScg`), view and exposure leaving linear files untouched
- File Output files take the color management of an `OVERRIDE` item or node format, else scene `image_settings` under `OVERRIDE`, else scene display and view
- `image.save_render(filepath, scene=scene)` writes a copy in scene `image_settings` format and color management, the path's extension choosing nothing

Multi-view renders follow `render.use_multiview` and `render.views`, each view writing its `file_suffix`:
- `render.views_format = "STEREO_3D"` renders `left` and `right`, `"MULTIVIEW"` every view with `use`
- `render.views.new` views start with no `file_suffix`
- Stills write `<filepath><file_suffix>.<ext>`, animations `<filepath><frame><file_suffix>.<ext>`, as `frame_path(frame=..., view=...)` returns
- `image_settings.views_format = "STEREO_3D"` packs views in one file, `SIDEBYSIDE` doubling width and `TOPBOTTOM` height
- `image_settings.views_format = "MULTIVIEW"` exists for EXR alone and writes every view into one file

## [05]-[IMAGES_AND_COLOR]

Images hold a colorspace tag, a format, and an alpha mode the host applies at each read and write:
- Images take `colorspace_settings.name` before their first pixel write, a later colorspace write clearing pixels to black
- New float images take the working space (`bpy.data.colorspace.working_space`), byte images `sRGB`, and `is_data=True` images `Non-Color`
- New images hold `file_format` `PNG` and `alpha_mode` `STRAIGHT`
- `bpy.ops.wm.set_working_color_space(working_space=..., convert_colors=True)` changes the read-only working space
- Float buffers hold premultiplied alpha, byte buffers straight alpha
- `images.new` defaults to `alpha=False`, which writes RGB files composited over black whatever the buffer's alpha
- Float images holding data (positions, normals, masks) take `Non-Color` and round-trip through EXR unchanged under every `alpha_mode`, PNG saves dividing color by alpha
- Loaded EXR images default to `alpha_mode` `PREMUL`, PNG images to `STRAIGHT`
- File images take their header's space (`colorInteropID`) at load, and a tag code writes persists in the .blend over it

Saves follow host rules:
- `image.save()` writes `image.file_format` in its colorspace tag whatever the path's extension (PNG bytes in `a.exr`)
- Saves to a new path write `file_format` and `filepath_raw`, then call `save()`, the image becoming a file image at that path
- `save(filepath=...)` turns an image into a file image with `image.filepath` unset, its pixels lost at next reload or file load
- `save()` on a generated color image writes and retags it `sRGB` in every format, EXR included, alpha below 1 writing wrong colors that `save_render()` writes right
- `save(save_copy=True)` writes an identical file and keeps the image generated

Script-written pixels of a generated image persist through `pack()` or a saved file alone, a fake user keeping the image without them:
- `pack()` stores float images as full-precision EXR and byte images as PNG, keeping the tag
- Packed float images with alpha below 1 keep their values across reloads with `alpha_mode = "PREMUL"`, the default `STRAIGHT` premultiplying again

Tiled images (`tiled=True`) save one file per tile through `<UDIM>` in `filepath_raw`:
- `tiles.new()` adds a tile with no buffer that `save()` refuses until `bpy.ops.image.tile_fill()` runs under `temp_override(edit_image=image)` with that tile active
- `bpy.ops.image.tile_add(number=..., fill=True)` under the same override adds a filled tile

Colors in code follow the working space and config names:
- Colors written in code (socket defaults, light colors) are scene-linear in the working space, `COLOR_GAMMA` members (theme colors) sRGB-encoded
- `mathutils.Color` converts through `from_srgb_to_scene_linear()` and `from_scene_linear_to_<space>()` (`srgb`, `aces`, `acescg`, `rec709_linear`, `rec2020_linear`, `xyz_d65`) with no display, view, or look
- Colorspace writes take config names (`Linear Rec.709`, `Non-Color`), and `Linear` or `Raw` raise `TypeError`
- `colorspace_settings.interop_id`, `bpy.data.colorspace.working_space_interop_id`, and OCIO `ColorSpace.getInteropID()` name spaces across configs (`lin_rec709_scene`, `data` for `Non-Color`)
- `DisplayViewTransform(src=<working space>, display=..., view=...)` processors reproduce a display write at look `None`, an `ExposureContrastTransform` before them adding `exposure`
- `CompositorNodeConvertColorSpace` converts by space name or interop id inside a tree, `CompositorNodeConvertToDisplay` applying its own `view_settings` and `display_settings`

Display encodings of a premultiplied buffer divide alpha out of a copy, run one CPU processor over it, and multiply alpha back, an unknown display or view answering its `PyOpenColorIO.Exception`:

```python
from expression import Result
from expression.extra.result import catch
import numpy as np
from numpy.typing import NDArray
import PyOpenColorIO


def displayed(config: PyOpenColorIO.Config, working: str, pixels: NDArray[np.float32], display: str, view: str) -> Result[NDArray[np.float32], PyOpenColorIO.Exception]:
    def encoded(processor: PyOpenColorIO.Processor) -> NDArray[np.float32]:
        buffer, alpha = np.array(pixels, np.float32, order="C"), pixels[..., 3:]
        np.divide(buffer[..., :3], alpha, out=buffer[..., :3], where=alpha > 0)
        processor.getDefaultCPUProcessor().applyRGBA(buffer)
        np.multiply(buffer[..., :3], alpha, out=buffer[..., :3], where=alpha > 0)
        return buffer

    return catch(config.getProcessor, exception=PyOpenColorIO.Exception)(PyOpenColorIO.DisplayViewTransform(src=working, display=display, view=view)).map(encoded)
```

`PyOpenColorIO.Baker(config=, format=, inputSpace=, looks=, cubeSize=)` bakes the display and view `setDisplayView` names into LUT text through `bake()`, `Baker.getFormats()` listing format and extension pairs:
- Bakes leave out `view_settings.exposure`, `gamma`, white balance, and `curve_mapping`
- Cubes from a log input space (`ACEScct`) cover HDR input, a linear input space covering 0 to 1 alone
- Bakes take `""` for the look `None`, and an unknown display, view, look, or space raises `PyOpenColorIO.Exception`

Linear EXR exports write a `FileFormat` through a scratch scene under `OVERRIDE`, the image keeping its source and tag:

```python
from contextlib import ExitStack

import bpy
from expression import Error, Result, Some
from expression.extra.result import catch


def exported(image: bpy.types.Image, path: str, space: str) -> Result[str, TypeError | RuntimeError]:
    output: FileFormat = FileFormat(media_type="IMAGE", file_format="OPEN_EXR", color_mode="RGBA", color_depth="32", views_format="INDIVIDUAL", linear_space=Some(space))
    with ExitStack() as scope:
        scene: bpy.types.Scene = bpy.data.scenes.new(".export")
        scope.callback(bpy.data.scenes.remove, scene)
        scene.render.image_settings.color_management = "OVERRIDE"
        match catch(output.applied, exception=TypeError)(scene.render.image_settings):
            case Result(tag="ok"):
                return catch(image.save_render, exception=RuntimeError)(path, scene=scene).map(lambda _: path)
            case Result(error=error):
                return Error(error)
```

## [06]-[SEQUENCER]

Strips sit on `context.sequencer_scene` (workspace `sequencer_scene`), while `context.scene` names the window scene:
- Background runs hold no sequencer scene until `context.workspace.sequencer_scene = scene`
- `bpy.ops.render.render(write_still=True, use_sequencer_scene=True)` renders the sequencer scene at its own frame, size, and `render.filepath`
- Renders asking for a sequencer scene with none set, or with no unmuted strip other than sound, raise `RuntimeError`
- `render.use_sequencer` renders strips while one unmuted strip other than sound exists, else the 3D scene, empty frames rendering transparent black
- Sequencer renders apply the scene's view transform over strip colors
- `scene.sequencer_colorspace_settings` (`sRGB`) names the space strips blend in, movie and image strips taking `colorspace_settings` from their files
- `scene.sequence_editor_create()` returns the editor, `strips` holding its top-level strips and `strips_all` every strip depth first
- `sequence_editor.channels[n]` is channel `n`, its `mute` hiding strips from renders and its `lock` leaving Python writes open

Strips come from the editor's `strips.new_*` calls, file-add operators failing `poll` outside a Sequencer editor area:
- Calls place a strip on the span given, an occupied span staying overlapped
- Created strips start selected, and operators reading `selected_strips` act on each of them
- Missing media files make a one-frame strip with no raise, a movie reading `fps` 0.0
- Image sequences grow one frame per `strip.elements.append(file)`, file names relative to `strip.directory`
- Movie strips hold one timeline frame per media frame whatever `render.fps`, and sound strips convert by duration
- Scene strips take the source scene's frame range at creation, later source range changes moving `content_duration` alone
- `new_effect` takes `length` for effects with no input (`COLOR`, `TEXT`), and two-input effects raise `RuntimeError` without both inputs
- `COLOR` strips take render resolution at creation as writable `width` and `height`, the space `transform` scales in
- Scene strips showing their own sequencer scene render transparent

Writes to strip timing members move content and handles:

| [INDEX] | [MEMBER]                                 | [WRITE]                                        |
| :-----: | :--------------------------------------- | :--------------------------------------------- |
|  [01]   | `left_handle`                            | Moves visible start, content staying placed    |
|  [02]   | `right_handle`                           | Moves visible end, the first frame left out    |
|  [03]   | `duration`                               | Sets `right_handle` from `left_handle`         |
|  [04]   | `content_start`                          | Slides content and both handles together       |
|  [05]   | `content_trim_start`, `content_trim_end` | Cuts content, moving its start or end          |
|  [06]   | `channel`                                | Shuffles off an occupied span in its direction |

Timing edits follow host rules:
- Handles past content hold the boundary frame, and durations clamp to 1
- Handles written across the other handle sit one frame from it
- Single images and `COLOR` strips move content with `left_handle`, image sequences keep it placed
- Handle, `duration`, and `content_start` writes that overlap another strip push the written strip up a channel
- `strip.split(frame, "SOFT")` keeps full content in both halves and returns the right half (`<name>.001`), and `"HARD"` cuts content
- Splits at a left handle or outside the strip return `None`
- `retiming_keys.add(timeline_frame=...)` adds a key with start and end keys
- Moving the last retiming key stretches content and handles, and `reset()` undoes it
- Retiming key writes on a strip with missing media crash Blender

Effects, modifiers, and metas follow host rules:
- `new_effect` orders inputs to start `input_1` first and keeps the channel given
- Effects render only from a channel above both inputs
- Effect handles are read-only, spanning input overlap or the gap between inputs
- Overlapping strips composite the higher channel over lower ones by its `blend_type`, `ALPHA_OVER` by default, `blend_alpha` setting its opacity
- `CROSS` mixes by `(frame - left_handle) / duration`, and `use_default_fade = False` holds the mix at `effect_fader`
- `COMPOSITOR` effects run `node_group`, its Color inputs taking input strips in order, its first Float input the fade, and later inputs their `properties.inputs` values
- `strip.modifiers.new(name=..., type=...)` adds modifiers (`COLOR_BALANCE`, `CURVES`, `COMPOSITOR`, `MASK`) that run before the strip transform
- `new_meta` makes a one-frame meta, and `strip.move_to_meta(meta)` moves a strip, an effect input bringing its effect and other input
- Metas fit their children through `left_handle_offset = 0`, then `right_handle_offset = 0`
- Meta `content_start` writes move children in time
- Child channels count inside their meta, and meta trims hide children outside its handles
- `strip.parent_meta()` returns the meta holding a strip, and `meta.separate()` returns children to top level at their timeline positions
