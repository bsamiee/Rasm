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
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.ownership import Mark, Saved
from blender.bpy.data.ids.store import Handle, IdName
from blender.bpy.data.rna.descriptors import assigned, rooted, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.graphs import Expr, FileItem, FileItems, group_input
from blender.bpy.nodes.trees import socket_at
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.effects import CameraFrame, CompositorEffect, entered, Shell
from blender.bpy.render.compositor.entries import capturing, EXECUTING, Owned
from blender.bpy.render.compositor.inputs import captured, inputs_of
from blender.bpy.render.outputs.exports import COMPOSITE
from blender.bpy.render.outputs.formats import Kind, OutputFormat
from blender.bpy.render.outputs.paths import Pathed
from blender.bpy.render.outputs.results import intermediate, produced, Production, saved
from blender.bpy.render.sessions.engines import Captured, Engine, Pass, Tile
from blender.bpy.render.sessions.renders import bounds, engined, RenderCall, sourced
from blender.imaging.color.processors import DisplayView
from blender.imaging.faults import ImagingReason
from blender.imaging.files.buffers import pixels
from blender.imaging.files.images import emitted, Subimage
from blender.imaging.files.multilayer import Layer, Multilayer
from blender.imaging.sampling.integration import Estimate, Precision
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.rendering.outputs.naming import Output

from .stages import Effect, prepared

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

    from .stack import Chain

# --- [TYPES] ----------------------------------------------------------------------------

type Returned = frozenset[rna_enums.OperatorReturnItems]
type Prepared = Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]
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
        self.prepared: dict[int, tuple[Authored, ...]] = {}
        self.files: set[Path] = set()
        self.sources = sourced((scene,))

    # --- [CAPTURE]

    def isolated(self, scope: ExitStack) -> Outcome[None]:
        """Swaps render-enabled groups for copies with output nodes muted until the scope exits."""

        @cache
        def copied(tree: bpy.types.CompositorNodeTree) -> Outcome[bpy.types.CompositorNodeTree]:
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
                    return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(tree), member="copy"),))

        def swapped(entry: bpy.types.SceneCompositorEffect, original: bpy.types.CompositorNodeTree, pair: tuple[Snapshot, bpy.types.CompositorNodeTree]) -> Outcome[None]:
            inputs, group = pair
            scope.callback(self.restored, ((entry, original, inputs),))
            return self.assigned(entry, group, inputs)

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

    def captured(self, source: bpy.types.Scene, frame: int) -> Outcome[None]:
        """Saves the source render result as multilayer EXR indexed for replay."""

        def retained(path: str) -> Outcome[None]:
            self.files.add(Path(path))
            return Result[Multilayer, Faults].bind(Multilayer.read(path), lambda file: self.indexed(file, source, frame))

        return saved(source).bind(retained)

    def indexed(self, file: Multilayer, source: bpy.types.Scene, frame: int) -> Outcome[None]:
        """Stores file layers as replay tiles cropped to the border."""
        groups: dict[tuple[str, str], list[tuple[Pass, npt.NDArray[np.float32]]]] = {}
        names = tuple(sorted((layer.name for layer in source.view_layers), key=len, reverse=True))
        width, height, left, bottom, right, top = bounds(source)

        def plane(layer: Layer) -> Outcome[None]:
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

    def formatted(self, tree: bpy.types.NodeTree) -> Outcome[None]:
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

    def image(self, entry: bpy.types.SceneCompositorEffect, buffers: tuple[tuple[str, OpenImageIO.ImageBuf], ...]) -> Outcome[bpy.types.Image]:
        """Writes prepared pixels as one EXR part per view and loads them as one premultiplied data image, multiview across views."""
        path = Path(bpy.app.tempdir, f"{self.scene.session_uid}_{entry.name}_prepared.exr")
        views = tuple(name for name, _ in buffers)
        headed = len(views) > 1
        self.files.add(path)

        def parts(_output: OpenImageIO.ImageOutput) -> tuple[Subimage[np.float32], ...]:
            return tuple(
                Subimage(
                    spec=buffer.spec(),
                    attributes=(("view", OpenImageIO.TypeString, name), ("multiView", OpenImageIO.TypeDesc(f"string[{len(views)}]"), views)) if headed else (),
                    pixels=partial(pixels, buffer),
                )
                for name, buffer in buffers
            )

        def tagged(live: bpy.types.Image) -> Outcome[bpy.types.Image]:
            self.scope.callback(bpy.data.images.remove, live)
            return (assigned(live, {"use_multiview": True, "views_format": "INDIVIDUAL"}) if headed else Ok(None)).map(lambda _: live)

        return (
            emitted(str(path), parts).bind(lambda written_path: loaded(written_path, Tag(space=Some(self.config.getColorSpace(PyOpenColorIO.ROLE_DATA).getName()), alpha=Some("PREMUL")))).bind(tagged)
        )

    def stage(self, held: Owned[Effect], path: Path, spec: OpenImageIO.ImageSpec) -> Outcome[Authored]:
        """Prepares the entry per view from its evaluated input values and camera frame over the captured pixels."""
        with bpy.context.temp_override(scene=self.scene):
            evaluated = self.scene.evaluated_get(bpy.context.evaluated_depsgraph_get())
        reading = evaluated.compositor_effects[held.entry.name]

        def views(file: Multilayer, record: Entry[Effect], values: Mapping[Key, Value], frame: CameraFrame) -> Outcome[Authored]:
            def view(layer: Layer) -> Outcome[tuple[str, Prepared]]:
                def loaded_view(plane: npt.NDArray[np.float32]) -> Outcome[tuple[str, Prepared]]:
                    buffer = OpenImageIO.ImageBuf(OpenImageIO.ImageSpec(spec))
                    stored: Outcome[None] = Ok(None) if buffer.set_pixels(buffer.roi, plane) else Error((Fault(reason=ImagingReason.BUFFER, subject=buffer.name, cause=buffer.geterror()),))
                    return stored.bind(lambda _: prepared(record.record, values, self.config, buffer, self.precision, frame, evaluated)).map(lambda value: (layer.name, value))

                return Result[npt.NDArray[np.float32], Faults].bind(file.plane(layer, layer.channels), loaded_view)

            return accumulated(view(layer) for layer in file.layers).bind(lambda found: self.published(held.entry, held.row, record, found))

        return paired(Multilayer.read(str(path)), held.row.read(self.identity, held.entry), captured(self.identity, reading), CameraFrame.read(self.identity, reading)).bind(
            lambda parts: views(parts[0], parts[1], {key: owner.values["value"] for key, owner in parts[2].tree.owners.items() if key}, parts[3])
        )

    def published(self, entry: bpy.types.SceneCompositorEffect, row: CompositorEffect[Effect], record: Entry[Effect], views: tuple[tuple[str, Prepared], ...]) -> Outcome[Authored]:
        """Enters measured values into the entry, or builds and assigns its job-owned group over the prepared image, answering the group and inputs each frame assigns."""
        results = tuple((name, value) for name, held in views for value in held.to_list())
        images = tuple((name, value.value if isinstance(value, Estimate) else value) for name, value in results if not isinstance(value, Mapping))
        measured = tuple(value for _, value in results if isinstance(value, Mapping))

        def owned_group(group: bpy.types.NodeTree) -> bpy.types.NodeTree:
            self.scope.callback(bpy.data.node_groups.remove, group)
            return group

        def grouped(image: bpy.types.Image) -> Outcome[bpy.types.NodeTree | None]:
            staged = msgspec.structs.replace(record, record=msgspec.structs.replace(record.record, staged=IdName.of(image)))
            with bpy.context.temp_override(scene=self.scene):
                return (
                    row
                    .grouped(self.identity, self.scene, staged, self.config, entry=Some(entry))
                    .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
                    .bind(lambda group: made(group, self.identity, of=Some(self.scene)))
                    .map(owned_group)
                )

        def placed(before: Snapshot, tree: bpy.types.NodeTree | None) -> Outcome[Authored]:
            return (
                self
                .assigned(entry, tree, before)
                .bind(lambda _: entered(self.identity, entry, measured[0]) if measured else Ok(False))
                .bind(lambda _: inputs_of(entry).bind(Snapshot.capture))
                .map(lambda inputs: (entry, tree, inputs))
            )

        return paired(inputs_of(entry).bind(Snapshot.capture), self.image(entry, images).bind(grouped) if images else Ok(entry.node_group)).bind(lambda pair: placed(*pair))

    # --- [LIFETIME]

    @staticmethod
    def assigned(entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree | None, inputs: Snapshot) -> Outcome[None]:
        """Points the entry at a group, then applies its input snapshot."""
        return written(entry, "node_group", group).bind(lambda _: inputs_of(entry)).bind(lambda found: inputs.apply(found).outcome)

    def native(self, call: RenderCall) -> Outcome[Returned]:
        """Runs `render.render`, recording the Render Result in the space the chain leaves it and the Viewer in the space its active entry shows."""
        enabled = tuple(entry for entry in self.scene.compositor_effects if entry.enable_for_render) if self.scene.render.use_compositing else ()
        linear = self.config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()
        rows = self.chain.rows
        formed = tuple(index for index, entry in enumerate(enabled) if rows.row(self.identity, entry).filter(lambda row: row is self.chain.formation).is_some())

        def spaces(view: Option[DisplayView]) -> tuple[Option[str], ...]:
            encoded = view.bind(lambda held: held.colorspace(self.config)).map(lambda space: space.getName())
            return tuple(encoded if formed and index > formed[0] else Some(linear) for index in range(len(enabled) + 1))

        def shown(entering: Option[str], leaving: Option[str], entry: bpy.types.SceneCompositorEffect) -> Option[str]:
            def routed(link: bpy.types.NodeLink) -> Option[str]:
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
                        return leaving if any(isinstance(other.to_node, bpy.types.NodeGroupOutput) for other in link.from_socket.links) else Nothing  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute, not-iterable]

            return Option.of_optional(
                next(
                    (
                        routed(link)
                        for tree in Option.of_optional(entry.node_group).to_list()
                        for live in tree.nodes
                        if isinstance(live, bpy.types.CompositorNodeViewer)
                        for socket in socket_at(live.inputs, "Image").to_option().to_list()  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        for link in socket.links  # type: ignore[union-attr]  # ty: ignore[not-iterable]
                    ),
                    None,
                )
            ).bind(lambda space: space)

        def rendered(view: Option[DisplayView], layer: str) -> Outcome[Returned]:
            spaced = spaces(view)
            active = next((index for index, entry in enumerate(enabled) if entry.is_active), None)
            viewer = Option.of_optional(active).bind(lambda index: shown(spaced[index], spaced[index + 1], enabled[index]))
            scene_view = Nothing if formed else Some(viewed(self.scene))
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

        named = Ok(COMPOSITE) if enabled else call.layer.map(Ok).default_with(lambda: rooted(bpy.types.ViewLayer, bpy.context).map(lambda held: held.name))
        return paired(self.chain.view(self.identity, self.scene), named).bind(lambda pair: rendered(*pair))

    def restored(self, groups: tuple[Authored, ...]) -> None:
        """Assigns entry groups and inputs, holding faults in reports."""
        for entry, group, inputs in groups:
            for faults in self.assigned(entry, group, inputs).swap().to_option().to_list():
                self.identity.reports.hold(faults)

    def selected(self, scene: bpy.types.Scene, _graph: bpy.types.Depsgraph | None = None) -> None:
        """Assigns the current frame's prepared groups and inputs from `frame_change_pre`."""
        if scene.session_uid == self.scene.session_uid:
            self.restored(self.prepared[self.scene.frame_current])

    def removed(self) -> None:
        """Deletes capture files, holding OS errors in reports."""
        for path in self.files:
            for faults in (
                catch(path.unlink, exception=OSError)(missing_ok=True).map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=str(path), cause=error),)).swap().to_option().to_list()
            ):
                self.identity.reports.hold(faults)

    def frame(self, frame: int, subframe: float, engine: str, original: tuple[Authored, ...], staged: tuple[Owned[Effect], ...]) -> Outcome[None]:
        """Captures the frame render once under the authored engines, then prepares each CPU stage from its replayed prefix."""
        self.restored(original)
        self.scene.frame_set(frame, subframe=subframe)
        name, path, spec = self.raster(frame)
        self.files.add(path)

        def prefixed(held: Owned[Effect], *, demand: bool) -> Outcome[Returned]:
            with ExitStack() as capture:
                for source in self.sources:
                    capture.enter_context(intermediate(self.identity, source))
                return (
                    self
                    .isolated(capture)
                    .bind(lambda _: capture.enter_context(capturing(self.identity, self.scene, held.entry, self.output(name, spec, demand=demand), stop=True)))
                    .bind(lambda entry: Option.of_optional(entry.node_group).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(entry), member="node_group"),)))
                    .bind(self.formatted)
                    .bind(lambda _: called(bpy.ops.render.render, {"scene": self.scene.name, "write_still": False}))
                )

        def appended(completed: Outcome[tuple[Authored, ...]], held: Owned[Effect]) -> Outcome[tuple[Authored, ...]]:
            return completed.bind(lambda ready: prefixed(held, demand=False).bind(lambda _: self.stage(held, path, spec)).map(lambda entry: (*ready, entry)))

        head, *tail = staged

        def body() -> Outcome[None]:
            first: Outcome[tuple[Authored, ...]] = (
                prefixed(head, demand=True)
                .bind(lambda _: accumulated(self.captured(source, frame) for source in self.sources))
                .bind(lambda _: self.stage(head, path, spec))
                .map(lambda entry: (entry,))
            )
            return reduce(appended, tail, first).map(lambda completed: self.prepared.update({frame: completed}))

        return engined(self.sources, engine, body)

    def rendered(self, call: RenderCall, staged: tuple[Owned[Effect], ...]) -> Outcome[Returned]:
        """Prepares every frame, then renders through the replay engine."""
        scene = self.scene
        frames = tuple(range(scene.frame_start, scene.frame_end + 1, scene.frame_step)) if call.animation else (scene.frame_current,)
        at, subframe = scene.frame_current, scene.frame_subframe
        sample = 0.0 if call.animation else subframe
        original = accumulated(inputs_of(held.entry).bind(Snapshot.capture).map(lambda inputs: (held.entry, held.entry.node_group, inputs)) for held in staged)

        def completed() -> Outcome[Returned]:
            bpy.app.handlers.frame_change_pre.append(self.selected)
            self.scope.callback(bpy.app.handlers.frame_change_pre.remove, self.selected)
            scene.frame_set(frames[0], subframe=sample)
            return self.native(call)

        def prepared(authored: tuple[Authored, ...]) -> Outcome[Returned]:
            self.scope.callback(self.restored, authored)
            with Engine.replayed(self.identity, "prepared_render", self.replay, lambda: scene.frame_current) as engine:
                return engine.bind(
                    lambda name: reduce(lambda done, frame: done.bind(lambda _: self.frame(frame, sample, name, authored, staged)), frames, Ok(None)).bind(
                        lambda _: engined(self.sources, name, completed)
                    )
                )

        with self.scope:
            executing = self.identity.held[EXECUTING]
            for source in self.sources:
                executing.add(source.session_uid)
                self.scope.callback(executing.remove, source.session_uid)
            self.scope.callback(scene.frame_set, at, subframe=subframe)
            self.scope.callback(self.removed)
            return original.bind(prepared)


# --- [OPERATIONS] -----------------------------------------------------------------------


def rendered(identity: Identity, chain: "Chain", scene: bpy.types.Scene, precision: Precision, call: RenderCall) -> Outcome[Returned]:
    """Renders through the chain's CPU stages under compositing, else through `render.render` alone."""
    staged = (
        tuple(held for held in chain.rows.owned(identity, scene) if held.entry.enable_for_render and prepared.dispatch(held.row.record) is not prepared.dispatch(Effect))
        if scene.render.use_compositing
        else ()
    )
    execution = Execution(identity, chain, scene, precision)
    return execution.rendered(call, staged) if staged else execution.native(call)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Authored", "Execution", "Prepared", "Returned", "rendered"]
