"""Render jobs running CPU effect stages over captured compositor prefixes."""

from collections.abc import Generator, Mapping
from contextlib import contextmanager, ExitStack
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
from blender.bpy.nodes.trees import nested
from blender.bpy.render.effects.formation import formation, viewed
from blender.bpy.render.effects.records import entered, inputs_of, stored_graphs
from blender.bpy.render.effects.rows import CompositorEffect, Shell
from blender.bpy.render.effects.stack import capturing, EffectStack, EXECUTING, Owned
from blender.bpy.render.outputs.exports import COMPOSITE
from blender.bpy.render.outputs.formats import Kind, OutputFormat
from blender.bpy.render.outputs.paths import Pathed
from blender.bpy.render.outputs.results import intermediate, produced, Production, saved
from blender.bpy.render.sessions.engines import Captured, Engine, Pass, Tile
from blender.imaging.chain import Effect
from blender.imaging.color.processors import DisplayView
from blender.imaging.compositing.blending import Entry
from blender.imaging.faults import Access, accumulated, ImagingBufferFault, ImagingFileFault, paired
from blender.imaging.files.multilayer import Layer, Multilayer
from blender.imaging.sampling import Estimate, Precision
from blender.rendering.outputs.naming import Output

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Prepared = tuple[Entry[Effect], Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]
type Staged = tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree, Snapshot, Entry[Effect]]
type Authored = tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree | None, Snapshot]

# --- [MODELS] ---------------------------------------------------------------------------


class RenderCall(msgspec.Struct, frozen=True, kw_only=True):
    """`render.render` options a render job passes through, an absent layer rendering every layer."""

    animation: bool = False
    write_still: bool = False
    layer: Option[str] = Nothing


# --- [SERVICES] -------------------------------------------------------------------------


class Execution:
    """Render job owning capture files, replay tiles, and temporary data."""

    def __init__(self, identity: Identity, stack: EffectStack, scene: bpy.types.Scene, precision: Precision) -> None:
        """Stores inputs, current OCIO config, and the scenes the stack's render-enabled groups read."""
        self.identity, self.stack, self.scene, self.precision = identity, stack, scene, precision
        self.config = PyOpenColorIO.GetCurrentConfig()
        self.scope = ExitStack()
        self.replay: dict[Captured, Tile] = {}
        self.prepared: dict[int, tuple[Staged, ...]] = {}
        self.files: set[Path] = set()
        self.sources = sourced((scene,))

    # --- [CAPTURE]

    @contextmanager
    def isolated(self) -> Generator[BlenderResult[None]]:
        """Swaps render-enabled groups for copies with output nodes muted until scope exit."""
        with ExitStack() as scope:

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

            yield accumulated(
                paired(inputs_of(entry).bind(Snapshot.capture), copied(tree)).bind(partial(swapped, entry, tree))
                for source in self.sources
                for entry in source.compositor_effects
                if entry.enable_for_render and isinstance(tree := entry.node_group, bpy.types.CompositorNodeTree)
            ).map(lambda _: None)

    @staticmethod
    def bounds(source: bpy.types.Scene) -> tuple[int, int, int, int, int, int]:
        """Returns scaled frame size and border pixel edges in Blender's float32 arithmetic."""
        render = source.render
        width, height = (side * render.resolution_percentage // 100 for side in (render.resolution_x, render.resolution_y))
        edges = (
            (
                int(np.float32(width) * np.float32(render.border_min_x)),
                int(np.float32(height) * np.float32(render.border_min_y)),
                int(np.float32(width) * np.float32(render.border_max_x)),
                int(np.float32(height) * np.float32(render.border_max_y)),
            )
            if render.use_border
            else (0, 0, width, height)
        )
        return width, height, *edges

    def raster(self, frame: int) -> tuple[str, Path, OpenImageIO.ImageSpec]:
        """Returns capture name, EXR path, and spec with border data window."""
        width, height, left, bottom, right, top = self.bounds(self.scene)
        crop = self.scene.render.use_border and self.scene.render.use_crop_to_border
        name = f"{self.scene.session_uid}_{frame}_entering"
        spec = OpenImageIO.ImageSpec(right - left if crop else width, top - bottom if crop else height, 4, OpenImageIO.FLOAT)
        spec.x, spec.y = (left, height - top) if crop else (0, 0)
        spec.full_x, spec.full_y, spec.full_width, spec.full_height = 0, 0, width, height
        return name, Path(bpy.app.tempdir, f"{name}.exr"), spec

    def captured(self, source: bpy.types.Scene, frame: int) -> BlenderResult[None]:
        """Saves source render result as multilayer EXR indexed for replay."""

        def retained(path: str) -> BlenderResult[None]:
            self.files.add(Path(path))
            return Result[Multilayer, BlenderFaults].bind(Multilayer.read(path), lambda file: self.indexed(file, source, frame))

        return saved(source).bind(retained)

    def indexed(self, file: Multilayer, source: bpy.types.Scene, frame: int) -> BlenderResult[None]:
        """Stores file layers as replay tiles cropped to border."""
        groups: dict[tuple[str, str], list[tuple[Pass, npt.NDArray[np.float32]]]] = {}
        names = tuple(sorted((layer.name for layer in source.view_layers), key=len, reverse=True))
        width, height, left, bottom, right, top = self.bounds(source)

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
        """Builds capture roots writing stack input at spec size, adding source Render Layers on `demand`."""
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
        """Sets capture output to 32-bit interleaved multiview scene linear EXR."""
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
        """Returns Image node over buffer pixels as premultiplied data."""

        def imaged(values: npt.NDArray[np.float32]) -> BlenderResult[Expr]:
            def populated(live: bpy.types.Image) -> BlenderResult[Expr]:
                self.scope.callback(bpy.data.images.remove, live)
                return (
                    Tag(space=Some(self.config.getColorSpace(PyOpenColorIO.ROLE_DATA).getName()), alpha=Some("PREMUL"))
                    .applied(live)
                    .bind(lambda _: filled(live, values))
                    .map(lambda _: Expr(kind=bpy.types.CompositorNodeImage, members=(("image", IdName.of(live)),), socket="Image", shape="RGBA").named(key))
                )

            created: BlenderResult[bpy.types.Image] = catch(
                lambda: bpy.data.images.new(vacant(f"Prepared {key}", bpy.data.images.keys()), values.shape[1], values.shape[0], alpha=values.shape[2] == 4, float_buffer=True), exception=RuntimeError
            )().map_error(lambda error: (BlenderEditFault(request=key, error=error),))
            return Result[bpy.types.Image, BlenderFaults].bind(created, populated)

        pixels: BlenderResult[npt.NDArray[np.float32]] = ImagingBufferFault.pixels(buffer)
        return Result[npt.NDArray[np.float32], BlenderFaults].bind(pixels, imaged)

    def stage(
        self, entry: bpy.types.SceneCompositorEffect, row: CompositorEffect[Effect | DisplayView], path: Path, spec: OpenImageIO.ImageSpec
    ) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
        """Prepares entry per view from evaluated controls and captured pixels."""
        with bpy.context.temp_override(scene=self.scene):
            graph = bpy.context.evaluated_depsgraph_get()
            evaluated = self.scene.evaluated_get(graph)
        reading = evaluated.compositor_effects[entry.name]

        def prepared(file: Multilayer) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
            def view(layer: Layer) -> BlenderResult[tuple[str, Prepared]]:
                def pixels(values: npt.NDArray[np.float32]) -> BlenderResult[tuple[str, Prepared]]:
                    buffer = OpenImageIO.ImageBuf(OpenImageIO.ImageSpec(spec))
                    loaded: BlenderResult[None] = Ok(None) if buffer.set_pixels(buffer.roi, values) else Error((ImagingBufferFault(request=buffer, error=buffer.geterror()),))
                    return (
                        loaded
                        .bind(lambda _: row.read(self.identity, reading))
                        .bind(lambda record: row.prepared(self.identity, evaluated, record, self.config, buffer, self.precision, reading))
                        .map(lambda value: (layer.name, value))
                    )

                return Result[npt.NDArray[np.float32], BlenderFaults].bind(file.plane(layer, layer.channels), pixels)

            return accumulated(view(layer) for layer in file.layers).bind(lambda views: self.published(entry, row, reading, views))

        return Result[Multilayer, BlenderFaults].bind(Multilayer.read(str(path)), prepared)

    def published(
        self, entry: bpy.types.SceneCompositorEffect, row: CompositorEffect[Effect | DisplayView], reading: bpy.types.SceneCompositorEffect, views: tuple[tuple[str, Prepared], ...]
    ) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
        """Builds and assigns job-owned entry group over prepared controls and images."""
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
                .bind(lambda _: stored_graphs(self.identity, group, (((), effective.effect, row.record.stage.domain), ((Shell.blend,), effective.blend, row.record.stage.domain))))
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
        """Points entry at group, applies its input snapshot, and re-enters an effective record's shell values."""
        return (
            written(entry, "node_group", group)
            .bind(lambda _: inputs_of(entry))
            .bind(lambda found: inputs.apply(found).outcome)
            .bind(
                lambda _: effective.map(
                    lambda held: paired(entered(self.identity, entry, (), held.effect), entered(self.identity, entry, (Shell.blend,), held.blend)).map(lambda _: None)
                ).default_value(Ok(None))
            )
        )

    def native(self, call: RenderCall) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
        """Runs `render.render`, recording `Production` of Render Result and Viewer images."""
        enabled = tuple(entry for entry in self.scene.compositor_effects if entry.enable_for_render) if self.scene.render.use_compositing else ()
        pinned = frozenset(row.ownership.key for row in self.stack.pinned)

        def routed(link: bpy.types.NodeLink, entering: BlenderResult[Option[str]], space: BlenderResult[Option[str]]) -> BlenderResult[Option[str]]:
            match link.from_node:
                case bpy.types.NodeGroupInput():
                    return entering
                case bpy.types.CompositorNodeImage(image=bpy.types.Image(colorspace_settings=bpy.types.ColorManagedInputColorspaceSettings(is_data=True, name=name))):
                    return Ok(Some(name))
                case bpy.types.CompositorNodeConvertToDisplay() as display:
                    return Ok(viewed(display).colorspace(self.config).map(lambda found: found.getName()))
                case bpy.types.CompositorNodeConvertColorSpace() as conversion:
                    return Ok(Some(self.config.getColorSpace(conversion.to_color_space).getName()))
                case _:
                    return space if any(isinstance(edge.to_node, bpy.types.NodeGroupOutput) for edge in link.from_socket.links) else Ok(Nothing)  # type: ignore[union-attr]  # ty: ignore[not-iterable, unresolved-attribute]

        def passing(entry: bpy.types.SceneCompositorEffect, entering: BlenderResult[Option[str]]) -> BlenderResult[Option[str]]:
            return Option.of_optional(entry.node_group).bind(lambda group: Mark.read(self.identity, group)).filter(lambda mark: mark.key in pinned).map(lambda _: entering).default_value(Ok(Nothing))

        def stepped(state: tuple[BlenderResult[Option[str]], BlenderResult[Option[str]]], entry: bpy.types.SceneCompositorEffect) -> tuple[BlenderResult[Option[str]], BlenderResult[Option[str]]]:
            entering, viewer = state
            space: BlenderResult[Option[str]] = (
                self.stack
                .row(self.identity, entry)
                .map(
                    lambda row: (
                        entering.bind(lambda _: formation.read(self.identity, entry).map(lambda record: record.effect.colorspace(self.config).map(lambda found: found.getName())))
                        if row is formation
                        else entering
                    )
                )
                .default_with(lambda: passing(entry, entering))
            )
            shown = (
                routed(link, entering, space)
                for tree in Option.of_optional(entry.node_group).to_list()
                if entry.is_active
                for live in tree.nodes
                if isinstance(live, bpy.types.CompositorNodeViewer)
                for link in tree.links
                if link.to_node == live
            )
            return space, (viewer, *shown)[-1]

        linear = self.config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()
        initial: tuple[BlenderResult[Option[str]], BlenderResult[Option[str]]] = (Ok(Some(linear)), Ok(Nothing))
        space, viewer = reduce(stepped, enabled, initial)
        layer = COMPOSITE if enabled else call.layer.default_with(lambda: self.scene.view_layers[0].name)
        view = (
            Nothing
            if any(self.stack.row(self.identity, entry).filter(lambda row: row is formation).is_some() for entry in enabled)
            else Some((viewed(self.scene), self.scene.view_settings.exposure, self.scene.view_settings.gamma))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        )

        def rendered(origins: tuple[Option[str], Option[str]]) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
            productions = {
                "RENDER_RESULT": origins[0].map(lambda encoding: Production(scene=Handle.of(self.scene), space=encoding, layer=Some(layer), view=view)),
                "COMPOSITING": origins[1].map(lambda encoding: Production(scene=Handle.of(self.scene), space=encoding, view=view if encoding == linear else Nothing)),
            }
            images = tuple(image for image in bpy.data.images if image.type in productions)
            for image in images:
                produced(self.identity, image, Nothing)

            def published(status: frozenset["rna_enums.OperatorReturnItems"]) -> frozenset["rna_enums.OperatorReturnItems"]:
                for image in bpy.data.images:
                    if image.type in productions:
                        produced(self.identity, image, productions[image.type])
                return status

            return called(bpy.ops.render.render, msgspec.structs.asdict(call) | {"scene": self.scene.name}).map(published)

        return paired(space, viewer).bind(rendered)

    def restored(self, original: tuple[Authored, ...]) -> None:
        """Restores authored entry groups and inputs, holding faults in reports."""
        for entry, group, inputs in original:
            for faults in self.assigned(entry, group, inputs, Nothing).swap().to_option().to_list():
                self.identity.reports.hold(faults)

    def selected(self, scene: bpy.types.Scene, _graph: bpy.types.Depsgraph | None = None) -> None:
        """Assigns current frame's prepared groups from `frame_change_pre`."""
        if scene.session_uid == self.scene.session_uid:
            for entry, group, inputs, effective in self.prepared[self.scene.frame_current]:
                for faults in self.assigned(entry, group, inputs, Some(effective)).swap().to_option().to_list():
                    self.identity.reports.hold(faults)

    def removed(self) -> None:
        """Deletes capture files, holding OS errors in reports."""
        for path in self.files:
            for faults in (
                catch(lambda: path.unlink(missing_ok=True), exception=OSError)()
                .map_error(lambda error: (ImagingFileFault(access=Access.WRITE, request=path, error=error),))
                .swap()
                .to_option()
                .to_list()
            ):
                self.identity.reports.hold(faults)

    def frame(self, frame: int, subframe: float, engine: str, original: tuple[Authored, ...]) -> BlenderResult[None]:
        """Captures frame render once under the authored engines, then prepares each CPU stage from the replayed prefix."""
        self.restored(original)
        self.scene.frame_set(frame, subframe=subframe)
        name, path, spec = self.raster(frame)
        self.files.add(path)

        def prefixed(held: Owned[Effect | DisplayView], *, demand: bool) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
            with ExitStack() as capture:
                for source in self.sources:
                    capture.enter_context(intermediate(self.identity, source))
                isolated = capture.enter_context(self.isolated())
                return (
                    isolated
                    .bind(lambda _: capture.enter_context(capturing(self.identity, held.entry, self.output(name, spec, demand=demand), stop=True)))
                    .bind(lambda entry: Option.of_optional(entry.node_group).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="node_group"),)))
                    .bind(self.formatted)
                    .bind(lambda _: called(bpy.ops.render.render, {"scene": self.scene.name, "write_still": False}))
                )

        def staged(held: Owned[Effect | DisplayView]) -> BlenderResult[Staged]:
            return self.stage(held.entry, held.row, path, spec).map(lambda prepared: (held.entry, *prepared))

        def appended(completed: BlenderResult[tuple[Staged, ...]], held: Owned[Effect | DisplayView]) -> BlenderResult[tuple[Staged, ...]]:
            return completed.bind(lambda ready: prefixed(held, demand=False).bind(lambda _: staged(held)).map(lambda entry: (*ready, entry)))

        head, *tail = self.stack.stages(self.identity, self.scene)
        first: BlenderResult[tuple[Staged, ...]] = (
            prefixed(head, demand=True).bind(lambda _: accumulated(self.captured(source, frame) for source in self.sources)).bind(lambda _: staged(head)).map(lambda entry: (entry,))
        )
        with engined(self.sources, engine):
            return reduce(appended, tail, first).map(lambda completed: self.prepared.update({frame: completed}))

    def rendered(self, call: RenderCall) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
        """Prepares every frame, then renders through replay engine."""
        scene = self.scene
        frames = tuple(range(scene.frame_start, scene.frame_end + 1, scene.frame_step)) if call.animation else (scene.frame_current,)
        at, subframe = scene.frame_current, scene.frame_subframe
        sample = 0.0 if call.animation else subframe
        original = accumulated(inputs_of(held.entry).bind(Snapshot.capture).map(lambda inputs: (held.entry, held.entry.node_group, inputs)) for held in self.stack.stages(self.identity, scene))

        def completed(engine: str) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
            with engined(self.sources, engine):
                bpy.app.handlers.frame_change_pre.append(self.selected)
                self.scope.callback(bpy.app.handlers.frame_change_pre.remove, self.selected)
                scene.frame_set(frames[0], subframe=sample)
                return self.native(call)

        def prepared(authored: tuple[Authored, ...]) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
            self.scope.callback(self.restored, authored)
            with Engine.replayed(self.identity, "prepared_render", self.replay, lambda: scene.frame_current) as engine:
                return engine.bind(lambda name: reduce(lambda done, frame: done.bind(lambda _: self.frame(frame, sample, name, authored)), frames, Ok(None)).bind(lambda _: completed(name)))

        with self.scope:
            executing = EXECUTING.of(self.identity)
            for source in self.sources:
                executing.add(source.session_uid)
                self.scope.callback(executing.remove, source.session_uid)
            self.scope.callback(scene.frame_set, at, subframe=subframe)
            self.scope.callback(self.removed)
            return original.bind(prepared)


# --- [OPERATIONS] -----------------------------------------------------------------------


def sourced(scenes: tuple[bpy.types.Scene, ...]) -> tuple[bpy.types.Scene, ...]:
    """Returns scenes and every scene their render-enabled groups read through unmuted Render Layers or render Cryptomatte nodes, each once."""
    reached = tuple(
        dict.fromkeys((
            *scenes,
            *(
                live.scene
                for scene in scenes
                for tree in nested((entry.node_group for entry in scene.compositor_effects if entry.enable_for_render and entry.node_group is not None), muted=False)
                for live in tree.nodes
                if isinstance(live, bpy.types.CompositorNodeRLayers | bpy.types.CompositorNodeCryptomatteV2)
                and not live.mute
                and live.scene is not None
                and (not isinstance(live, bpy.types.CompositorNodeCryptomatteV2) or live.source == "RENDER")
            ),
        ))
    )
    return reached if reached == scenes else sourced(reached)


@contextmanager
def engined(sources: tuple[bpy.types.Scene, ...], engine: str) -> Generator[None]:
    """Renders every source through `engine` for the scope, each source's own engine restored after."""
    held = tuple((source, source.render.engine) for source in sources)
    for source in sources:
        source.render.engine = engine  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    try:
        yield
    finally:
        for source, authored in held:
            source.render.engine = authored


def rendered(identity: Identity, stack: EffectStack, scene: bpy.types.Scene, precision: Precision, call: RenderCall) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
    """Renders through any stack CPU stages under compositing, else runs `render.render`."""
    execution = Execution(identity, stack, scene, precision)
    return execution.rendered(call) if scene.render.use_compositing and stack.stages(identity, scene) else execution.native(call)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Authored", "Execution", "RenderCall", "engined", "rendered", "sourced"]
