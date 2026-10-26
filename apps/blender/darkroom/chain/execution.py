"""CPU chain render: each staged entry prepared over its captured compositor prefix, replayed through a render engine, then the native render over prepared groups."""

from collections.abc import Mapping
from contextlib import ExitStack
from functools import cache, partial, reduce
from pathlib import Path
from typing import TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.data.ids.images import filled, Tag
from blender.bpy.data.ids.ownership import Mark, Saved
from blender.bpy.data.ids.store import Handle, IdName, vacant
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.graphs import Expr, FileItem, FileItems, group_input
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import CompositorEffect, Shell
from blender.bpy.render.compositor.entries import capturing, EXECUTING, Owned
from blender.bpy.render.compositor.inputs import entered, inputs_of, stored_graphs
from blender.bpy.render.outputs.exports import COMPOSITE
from blender.bpy.render.outputs.formats import Kind, OutputFormat
from blender.bpy.render.outputs.paths import Pathed
from blender.bpy.render.outputs.results import intermediate, produced, Production, saved
from blender.bpy.render.sessions.engines import Captured, Engine, Pass, Tile
from blender.bpy.render.sessions.renders import bounds, engined, RenderCall, sourced
from blender.imaging.color.processors import DisplayView
from blender.imaging.faults import ImagingImageFault
from blender.imaging.files.images import pixels
from blender.imaging.files.multilayer import Layer, Multilayer
from blender.imaging.sampling.integration import Estimate, Precision
from blender.records.faults import accumulated, FileFault, paired
from blender.rendering.outputs.naming import Output

from .stages import Effect

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

    from .stack import Chain

# --- [TYPES] ----------------------------------------------------------------------------

type Returned = frozenset[rna_enums.OperatorReturnItems]
type Prepared = tuple[Entry[Effect], Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]
type Staged = tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree, Snapshot, Entry[Effect]]
type Authored = tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree | None, Snapshot]

# --- [SERVICES] -------------------------------------------------------------------------


class Execution:
    """Render job of one scene owning its capture files, replay tiles, prepared groups, and temporary data until its scope exits."""

    def __init__(self, identity: Identity, chain: "Chain", scene: bpy.types.Scene, precision: Precision) -> None:
        """Holds the chain, the current OCIO config, and the scenes the render-enabled groups read."""
        self.identity, self.chain, self.scene, self.precision = identity, chain, scene, precision
        self.config = PyOpenColorIO.GetCurrentConfig()
        self.scope = ExitStack()
        self.replay: dict[Captured, Tile] = {}
        self.prepared: dict[int, tuple[Staged, ...]] = {}
        self.files: set[Path] = set()
        self.sources = sourced((scene,))

    # --- [CAPTURE]

    def isolated(self, scope: ExitStack) -> BlenderResult[None]:
        """Swaps render-enabled groups for copies with output nodes muted until the scope exits."""

        @cache
        def copied(tree: bpy.types.CompositorNodeTree) -> BlenderResult[bpy.types.CompositorNodeTree]:
            match tree.copy():
                case bpy.types.CompositorNodeTree() as result:
                    scope.callback(bpy.data.node_groups.remove, result)
                    for live in result.nodes:
                        if isinstance(live, bpy.types.CompositorNodeOutputFile | bpy.types.CompositorNodeViewer):
                            live.mute = True
                    return accumulated(
                        copied(child).bind(lambda group: written(live, "node_tree", group))
                        for live in result.nodes
                        if isinstance(live, bpy.types.CompositorNodeGroup) and isinstance(child := live.node_tree, bpy.types.CompositorNodeTree)
                    ).map(lambda _: result)
                case _:
                    return Error((BlenderAbsentFault(owner=repr(tree), key="copy"),))

        def swapped(entry: bpy.types.SceneCompositorEffect, original: bpy.types.CompositorNodeTree, pair: tuple[Snapshot, bpy.types.CompositorNodeTree]) -> BlenderResult[None]:
            inputs, group = pair
            scope.callback(self.restored, ((entry, original, inputs),))
            return self.assigned(entry, group, inputs, Nothing)

        return accumulated(
            paired(inputs_of(entry).bind(Snapshot.capture), copied(tree)).bind(partial(swapped, entry, tree))
            for source in self.sources
            for entry in source.compositor_effects
            if entry.enable_for_render and isinstance(tree := entry.node_group, bpy.types.CompositorNodeTree)
        ).map(lambda _: None)

    def raster(self, frame: int) -> tuple[str, Path, OpenImageIO.ImageSpec]:
        """Returns capture name, EXR path, and spec with the border data window."""
        width, height, left, bottom, right, top = bounds(self.scene)
        crop = self.scene.render.use_border and self.scene.render.use_crop_to_border
        name = f"{self.scene.session_uid}_{frame}_entering"
        spec = OpenImageIO.ImageSpec(right - left if crop else width, top - bottom if crop else height, 4, OpenImageIO.FLOAT)
        spec.x, spec.y = (left, height - top) if crop else (0, 0)
        spec.full_x, spec.full_y, spec.full_width, spec.full_height = 0, 0, width, height
        return name, Path(bpy.app.tempdir, f"{name}.exr"), spec

    def captured(self, source: bpy.types.Scene, frame: int) -> BlenderResult[None]:
        """Saves the source render result as multilayer EXR indexed for replay."""

        def retained(path: str) -> BlenderResult[None]:
            self.files.add(Path(path))
            return Result[Multilayer, BlenderFaults].bind(Multilayer.read(path), lambda file: self.indexed(file, source, frame))

        return saved(source).bind(retained)

    def indexed(self, file: Multilayer, source: bpy.types.Scene, frame: int) -> BlenderResult[None]:
        """Stores file layers as replay tiles cropped to the border."""
        groups: dict[tuple[str, str], list[tuple[Pass, npt.NDArray[np.float32]]]] = {}
        names = tuple(sorted((layer.name for layer in source.view_layers), key=len, reverse=True))
        width, height, left, bottom, right, top = bounds(source)

        def plane(layer: Layer) -> BlenderResult[None]:
            if (owner := next((name for name in names if layer.name.startswith(f"{name}.")), None)) is None:
                return Ok(None)
            view = file.parts[layer.part].header.get("view", "")
            declaration = Pass(name=layer.name[len(owner) + 1 :], chanid="".join(channel.name for channel in layer.channels))

            def retained(values: npt.NDArray[np.float32]) -> None:
                raw = values[height - top : height - bottom, left:right] if values.shape[:2] == (height, width) and source.render.use_border else values
                groups.setdefault((owner, view), []).append((declaration, np.ascontiguousarray(raw)))

            return file.plane(layer, layer.channels).map(retained)

        def retained(_: object) -> None:
            stamps = tuple((key, value) for part in file.parts[:1] for key, value in part.header.items())
            self.replay.update({
                Captured(frame=frame, scene=source.session_uid, layer=owner, view=view): Tile(x=0, y=0, planes=(planes[0], *planes[1:]), view=Some(view), stamps=stamps)
                for (owner, view), planes in groups.items()
            })

        return accumulated(plane(layer) for layer in file.layers).map(retained)

    def output(self, name: str, spec: OpenImageIO.ImageSpec, *, demand: bool) -> tuple[Expr, ...]:
        """Builds capture roots writing the chain input at spec size, adding source Render Layers on `demand`."""
        domain = Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Size", (spec.width, spec.height)),), socket="Image", shape="RGBA")
        raster = Expr(kind=bpy.types.ShaderNodeMixRGB, inputs=(("Fac", 1.0), ("Color1", domain), ("Color2", group_input(Shell.image, "RGBA"))), socket="Color", shape="RGBA")
        capture = Expr(
            kind=bpy.types.CompositorNodeOutputFile,
            inputs=((name, raster),),
            members=((Pathed.directory.prop.identifier, bpy.app.tempdir), (Pathed.file_name.prop.identifier, "")),
            content=Some(FileItems(items=(FileItem(name=name, kind="RGBA"),))),
        ).named("capture_file")
        layers = (
            Expr(kind=bpy.types.CompositorNodeRLayers, members=(("scene", IdName.of(source)), ("layer", layer.name)), socket="Image", shape="RGBA")
            for source in (self.sources if demand else ())
            for layer in source.view_layers
            if layer.use
        )
        return (capture, *layers)

    def formatted(self, tree: bpy.types.NodeTree) -> BlenderResult[None]:
        """Sets the capture output to 32-bit interleaved multiview scene-linear EXR."""
        capture = next(live for live in tree.nodes if isinstance(live, bpy.types.CompositorNodeOutputFile) and Mark.read(self.identity, live).filter(lambda mark: mark.key == "capture_file").is_some())
        return (
            OutputFormat
            .of(Output.DATA, Kind.EXR)
            .bind(lambda output: output.applied(capture))
            .bind(
                lambda _: (
                    Snapshot
                    .of(
                        bpy.types.CompositorNodeOutputFile,
                        {
                            "format.color_mode": "RGBA",
                            "format.color_depth": "32",
                            "format.views_format": "MULTIVIEW",
                            "format.use_exr_interleave": True,
                            "format.color_management": "OVERRIDE",
                            "format.linear_colorspace_settings.name": self.config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(),
                        },
                    )
                    .apply(capture)
                    .outcome
                )
            )
        )

    # --- [PREPARATION]

    def image(self, key: Key, buffer: OpenImageIO.ImageBuf) -> BlenderResult[Expr]:
        """Returns an Image node over buffer pixels as premultiplied data."""

        def imaged(values: npt.NDArray[np.float32]) -> BlenderResult[Expr]:
            def populated(live: bpy.types.Image) -> BlenderResult[Expr]:
                self.scope.callback(bpy.data.images.remove, live)
                return (
                    Tag(space=Some(self.config.getColorSpace(PyOpenColorIO.ROLE_DATA).getName()), alpha=Some("PREMUL"))
                    .applied(live)
                    .bind(lambda _: filled(live, values))
                    .map(lambda _: Expr(kind=bpy.types.CompositorNodeImage, members=(("image", IdName.of(live)),), socket="Image", shape="RGBA").named(key))
                )

            created = BlenderEditFault.attempted(
                key, lambda: bpy.data.images.new(vacant(f"Prepared {key}", bpy.data.images.keys()), values.shape[1], values.shape[0], alpha=values.shape[2] == 4, float_buffer=True)
            )
            return created.bind(populated)

        return Result[npt.NDArray[np.float32], BlenderFaults].bind(pixels(buffer), imaged)

    def stage(self, held: Owned[Effect], path: Path, spec: OpenImageIO.ImageSpec) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
        """Prepares the entry per view from evaluated controls and captured pixels."""
        with bpy.context.temp_override(scene=self.scene):
            evaluated = self.scene.evaluated_get(bpy.context.evaluated_depsgraph_get())
        reading = evaluated.compositor_effects[held.entry.name]

        def prepared(file: Multilayer) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
            def view(layer: Layer) -> BlenderResult[tuple[str, Prepared]]:
                def loaded(values: npt.NDArray[np.float32]) -> BlenderResult[tuple[str, Prepared]]:
                    buffer = OpenImageIO.ImageBuf(OpenImageIO.ImageSpec(spec))
                    stored: BlenderResult[None] = Ok(None) if buffer.set_pixels(buffer.roi, values) else Error((ImagingImageFault(request=buffer, error=buffer.geterror()),))
                    return (
                        stored
                        .bind(lambda _: held.row.read(self.identity, reading))
                        .bind(lambda record: held.row.prepared(self.identity, evaluated, record, self.config, buffer, self.precision, reading))
                        .map(lambda value: (layer.name, value))
                    )

                return Result[npt.NDArray[np.float32], BlenderFaults].bind(file.plane(layer, layer.channels), loaded)

            return accumulated(view(layer) for layer in file.layers).bind(lambda views: self.published(held.entry, held.row, reading, views))

        return Result[Multilayer, BlenderFaults].bind(Multilayer.read(str(path)), prepared)

    def published(
        self, entry: bpy.types.SceneCompositorEffect, row: CompositorEffect[Effect], reading: bpy.types.SceneCompositorEffect, views: tuple[tuple[str, Prepared], ...]
    ) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
        """Builds and assigns the job-owned entry group over prepared controls and images, one view switch per image across views."""
        effective = views[0][1][0]

        def selected(key: Key) -> BlenderResult[tuple[Key, Expr]]:
            return accumulated(
                self.image(key if len(views) == 1 else item_key(key, "view", index), value.value if isinstance(value, Estimate) else value)
                for index, (_name, (_record, images)) in enumerate(views)
                for value in (images[key],)
            ).map(
                lambda values: (
                    key,
                    values[0]
                    if len(values) == 1
                    else Expr(kind=bpy.types.CompositorNodeSwitchView, inputs=tuple((name, value) for (name, _), value in zip(views, values)), socket="Image", shape="RGBA").named(key),
                )
            )

        def assigned(group: bpy.types.NodeTree, before: Snapshot) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
            self.scope.callback(bpy.data.node_groups.remove, group)
            return (
                self
                .assigned(entry, group, before, Some(effective))
                .bind(lambda _: stored_graphs(self.identity, group, (((), effective.record, row.domain), ((Shell.blend,), effective.blend, row.domain))))
                .map(lambda _: (group, before, effective))
            )

        with bpy.context.temp_override(scene=self.scene):
            return (
                inputs_of(reading)
                .bind(Snapshot.capture)
                .bind(
                    lambda before: (
                        accumulated(selected(key) for key in views[0][1][1])
                        .bind(lambda images: row.grouped(self.identity, self.scene, effective, self.config, entry=Some(entry), prepared=True, images=dict(images)))
                        .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
                        .bind(lambda group: made(group, self.identity, of=Some(self.scene)))
                        .bind(lambda group: assigned(group, before))
                    )
                )
            )

    # --- [LIFETIME]

    def assigned(self, entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree | None, inputs: Snapshot, effective: Option[Entry[Effect]]) -> BlenderResult[None]:
        """Points the entry at a group, applies its input snapshot, and re-enters an effective record's values."""
        return (
            written(entry, "node_group", group)
            .bind(lambda _: inputs_of(entry))
            .bind(lambda found: inputs.apply(found).outcome)
            .bind(
                lambda _: effective.map(
                    lambda held: paired(entered(self.identity, entry, (), held.record), entered(self.identity, entry, (Shell.blend,), held.blend)).map(lambda _: None)
                ).default_value(Ok(None))
            )
        )

    def native(self, call: RenderCall) -> BlenderResult[Returned]:
        """Runs `render.render`, recording the Render Result in the space the chain leaves it and the Viewer in the space its active entry shows."""
        enabled = tuple(entry for entry in self.scene.compositor_effects if entry.enable_for_render) if self.scene.render.use_compositing else ()
        linear = self.config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()
        rows = self.chain.rows
        formed = tuple(index for index, entry in enumerate(enabled) if rows.row(self.identity, entry).filter(lambda row: row is self.chain.formation).is_some())

        def spaces(view: Option[DisplayView]) -> tuple[Option[str], ...]:
            encoded = view.bind(lambda held: held.colorspace(self.config)).map(lambda space: space.getName())
            return tuple(encoded if formed and index > formed[0] else Some(linear) for index in range(len(enabled) + 1))

        def shown(entering: Option[str], leaving: Option[str], entry: bpy.types.SceneCompositorEffect) -> Option[str]:
            def routed(tree: bpy.types.NodeTree, link: bpy.types.NodeLink) -> Option[str]:
                match link.from_node:
                    case bpy.types.NodeGroupInput():
                        return entering
                    case bpy.types.CompositorNodeImage(image=bpy.types.Image(colorspace_settings=bpy.types.ColorManagedInputColorspaceSettings(is_data=True, name=name))):
                        return Some(name)
                    case bpy.types.CompositorNodeConvertToDisplay() as display:
                        return viewed(display).colorspace(self.config).map(lambda found: found.getName())
                    case bpy.types.CompositorNodeConvertColorSpace() as conversion:
                        return Some(self.config.getColorSpace(conversion.to_color_space).getName())
                    case _:
                        return leaving if any(isinstance(other.to_node, bpy.types.NodeGroupOutput) and other.from_socket == link.from_socket for other in tree.links) else Nothing

            return Option.of_optional(
                next(
                    (
                        routed(tree, link)
                        for tree in Option.of_optional(entry.node_group).to_list()
                        for live in tree.nodes
                        if isinstance(live, bpy.types.CompositorNodeViewer)
                        for link in tree.links
                        if link.to_node == live
                    ),
                    None,
                )
            ).bind(lambda space: space)

        def rendered(view: Option[DisplayView]) -> BlenderResult[Returned]:
            spaced = spaces(view)
            active = next((index for index, entry in enumerate(enabled) if entry.is_active), None)
            viewer = Option.of_optional(active).bind(lambda index: shown(spaced[index], spaced[index + 1], enabled[index]))
            scene_view = Nothing if formed else Some(viewed(self.scene))
            layer = COMPOSITE if enabled else call.layer.default_with(lambda: self.scene.view_layers[0].name)
            productions = {
                "RENDER_RESULT": spaced[-1].map(lambda space: Production(scene=Handle.of(self.scene), space=space, layer=Some(layer), view=scene_view)),
                "COMPOSITING": viewer.map(lambda space: Production(scene=Handle.of(self.scene), space=space, view=scene_view if space == linear else Nothing)),
            }
            for image in (image for image in bpy.data.images if image.type in productions):
                produced(self.identity, image, Nothing)

            def published(status: Returned) -> Returned:
                for image in (image for image in bpy.data.images if image.type in productions):
                    produced(self.identity, image, productions[image.type])
                return status

            return called(bpy.ops.render.render, msgspec.structs.asdict(call) | {"scene": self.scene.name}).map(published)

        return self.chain.view(self.identity, self.scene).bind(rendered)

    def restored(self, original: tuple[Authored, ...]) -> None:
        """Restores authored entry groups and inputs, holding faults in reports."""
        for entry, group, inputs in original:
            for faults in self.assigned(entry, group, inputs, Nothing).swap().to_option().to_list():
                self.identity.reports.hold(faults)

    def selected(self, scene: bpy.types.Scene, _graph: bpy.types.Depsgraph | None = None) -> None:
        """Assigns the current frame's prepared groups from `frame_change_pre`."""
        if scene.session_uid == self.scene.session_uid:
            for entry, group, inputs, effective in self.prepared[self.scene.frame_current]:
                for faults in self.assigned(entry, group, inputs, Some(effective)).swap().to_option().to_list():
                    self.identity.reports.hold(faults)

    def removed(self) -> None:
        """Deletes capture files, holding OS errors in reports."""
        for path in self.files:
            for faults in (
                catch(partial(path.unlink, missing_ok=True), exception=OSError)().map_error(lambda error: (FileFault(access="write", request=path, error=error),)).swap().to_option().to_list()
            ):
                self.identity.reports.hold(faults)

    def frame(self, frame: int, subframe: float, engine: str, original: tuple[Authored, ...], staged: tuple[Owned[Effect], ...]) -> BlenderResult[None]:
        """Captures the frame render once under the authored engines, then prepares each CPU stage from its replayed prefix."""
        self.restored(original)
        self.scene.frame_set(frame, subframe=subframe)
        name, path, spec = self.raster(frame)
        self.files.add(path)

        def prefixed(held: Owned[Effect], *, demand: bool) -> BlenderResult[Returned]:
            with ExitStack() as capture:
                for source in self.sources:
                    capture.enter_context(intermediate(self.identity, source))
                return (
                    self
                    .isolated(capture)
                    .bind(lambda _: capture.enter_context(capturing(self.identity, self.scene, held.entry, self.output(name, spec, demand=demand), stop=True)))
                    .bind(lambda entry: Option.of_optional(entry.node_group).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="node_group"),)))
                    .bind(self.formatted)
                    .bind(lambda _: called(bpy.ops.render.render, {"scene": self.scene.name, "write_still": False}))
                )

        def prepared(held: Owned[Effect]) -> BlenderResult[Staged]:
            return self.stage(held, path, spec).map(lambda value: (held.entry, *value))

        def appended(completed: BlenderResult[tuple[Staged, ...]], held: Owned[Effect]) -> BlenderResult[tuple[Staged, ...]]:
            return completed.bind(lambda ready: prefixed(held, demand=False).bind(lambda _: prepared(held)).map(lambda entry: (*ready, entry)))

        head, *tail = staged

        def body() -> BlenderResult[None]:
            first: BlenderResult[tuple[Staged, ...]] = (
                prefixed(head, demand=True).bind(lambda _: accumulated(self.captured(source, frame) for source in self.sources)).bind(lambda _: prepared(head)).map(lambda entry: (entry,))
            )
            return reduce(appended, tail, first).map(lambda completed: self.prepared.update({frame: completed}))

        return engined(self.sources, engine, body)

    def rendered(self, call: RenderCall, staged: tuple[Owned[Effect], ...]) -> BlenderResult[Returned]:
        """Prepares every frame, then renders through the replay engine."""
        scene = self.scene
        frames = tuple(range(scene.frame_start, scene.frame_end + 1, scene.frame_step)) if call.animation else (scene.frame_current,)
        at, subframe = scene.frame_current, scene.frame_subframe
        sample = 0.0 if call.animation else subframe
        original = accumulated(inputs_of(held.entry).bind(Snapshot.capture).map(lambda inputs: (held.entry, held.entry.node_group, inputs)) for held in staged)

        def completed() -> BlenderResult[Returned]:
            bpy.app.handlers.frame_change_pre.append(self.selected)
            self.scope.callback(bpy.app.handlers.frame_change_pre.remove, self.selected)
            scene.frame_set(frames[0], subframe=sample)
            return self.native(call)

        def prepared(authored: tuple[Authored, ...]) -> BlenderResult[Returned]:
            self.scope.callback(self.restored, authored)
            with Engine.replayed(self.identity, "prepared_render", self.replay, lambda: scene.frame_current) as engine:
                return engine.bind(
                    lambda name: reduce(lambda done, frame: done.bind(lambda _: self.frame(frame, sample, name, authored, staged)), frames, Ok(None)).bind(
                        lambda _: engined(self.sources, name, completed)
                    )
                )

        with self.scope:
            executing = EXECUTING.of(self.identity)
            for source in self.sources:
                executing.add(source.session_uid)
                self.scope.callback(executing.remove, source.session_uid)
            self.scope.callback(scene.frame_set, at, subframe=subframe)
            self.scope.callback(self.removed)
            return original.bind(prepared)


# --- [OPERATIONS] -----------------------------------------------------------------------


def rendered(identity: Identity, chain: "Chain", scene: bpy.types.Scene, precision: Precision, call: RenderCall) -> BlenderResult[Returned]:
    """Renders through the chain's CPU stages under compositing, else through `render.render` alone."""
    staged = chain.rows.staged(identity, scene) if scene.render.use_compositing else ()
    execution = Execution(identity, chain, scene, precision)
    return execution.rendered(call, staged) if staged else execution.native(call)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Authored", "Execution", "Prepared", "Returned", "Staged", "rendered"]
