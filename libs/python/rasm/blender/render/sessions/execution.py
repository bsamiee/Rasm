"""Native prefix capture, same-runtime CPU effects, and pass-preserving render replay."""

from collections import deque
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

from rasm.blender.commands.calls import called
from rasm.blender.data.ids.images import filled, intermediate, produced, Production, saved, Tag
from rasm.blender.data.ids.ownership import Mark, Saved
from rasm.blender.data.ids.store import Handle
from rasm.blender.data.rna.descriptors import IdName, written
from rasm.blender.data.rna.snapshots import Snapshot
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.faults import BlenderEditFault, BlenderFaults, BlenderImagingFault, BlenderResult, BlenderWriteFault
from rasm.blender.nodes.graphs import End, FileItem, FileItems, Fragment, INPUTS, item_key, node
from rasm.blender.render.effects.stack import capturing, EffectStack, entered, formation, graphed, Shell, StackEntry, stored_graphs, viewed
from rasm.blender.render.outputs.formats import Kind, OutputFormat
from rasm.blender.render.outputs.paths import Pathed
from rasm.blender.render.sessions.engines import Engine, Pass, Tile
from rasm.imaging.chain import Effect
from rasm.imaging.color.processors import DisplayView
from rasm.imaging.compositing.blending import Entry
from rasm.imaging.effects.generators.noise import Estimate, Precision
from rasm.imaging.faults import accumulated, ImagingBufferFault, paired
from rasm.imaging.files.multilayer import Layer, Multilayer
from rasm.rendering.outputs.naming import Output

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Prepared = tuple[Entry[Effect], Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]

# --- [SERVICES] -------------------------------------------------------------------------


class Execution:
    """One native job's leased engine, captured passes, images, and transient entry groups."""

    def __init__(self, identity: Identity, stack: EffectStack, scene: bpy.types.Scene, precision: Precision) -> None:
        """Retains request inputs before acquiring the job's native resources."""
        self.identity, self.stack, self.scene, self.precision = identity, stack, scene, precision
        self.config = PyOpenColorIO.GetCurrentConfig()
        self.scope = ExitStack()
        self.replay: dict[tuple[int, int, str, str], Tile] = {}
        self.prepared: dict[int, tuple[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree, Snapshot, Entry[Effect]], ...]] = {}
        self.files: set[Path] = set()
        self.sources: tuple[bpy.types.Scene, ...] = ()

    # --- [CAPTURE]

    @contextmanager
    def isolated(self, sources: tuple[bpy.types.Scene, ...]) -> Generator[BlenderResult[None]]:
        """Copies capture graphs so authored file outputs and Viewers remain untouched."""
        with ExitStack() as scope:

            @cache
            def copied(tree: bpy.types.CompositorNodeTree) -> bpy.types.CompositorNodeTree:
                result: bpy.types.CompositorNodeTree = tree.copy()  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                scope.callback(bpy.data.node_groups.remove, result)
                for live in result.nodes:
                    if isinstance(live, bpy.types.CompositorNodeGroup) and isinstance(child := live.node_tree, bpy.types.CompositorNodeTree):
                        live.node_tree = copied(child)
                    elif isinstance(live, bpy.types.CompositorNodeOutputFile | bpy.types.CompositorNodeViewer):
                        live.mute = True
                return result

            def assigned(entry: bpy.types.SceneCompositorEffect, original: bpy.types.CompositorNodeTree, inputs: Snapshot) -> BlenderResult[None]:
                group = copied(original)
                scope.callback(self.restored, ((entry, original, inputs),))
                return written(entry, "node_group", group).bind(lambda _: inputs.apply(entry.properties.inputs).outcome).map(lambda _: None)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

            yield accumulated(
                Snapshot.capture(entry.properties.inputs).bind(partial(assigned, entry, tree))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                for source in sources
                for entry in source.compositor_effects
                if entry.enable_for_render and isinstance(tree := entry.node_group, bpy.types.CompositorNodeTree)
            ).map(lambda _: None)

    @staticmethod
    def bounds(source: bpy.types.Scene) -> tuple[int, int, int, int, int, int]:
        """Reads whole-frame and native float32 border coordinates before EXR loses placement."""
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
        """Declares an exact raster and separately retained camera DATA and FULL windows."""
        width, height, left, bottom, right, top = self.bounds(self.scene)
        crop = self.scene.render.use_border and self.scene.render.use_crop_to_border
        name = f"{self.scene.session_uid}_{frame}_entering"
        spec = OpenImageIO.ImageSpec(right - left if crop else width, top - bottom if crop else height, 4, OpenImageIO.FLOAT)
        spec.x, spec.y = (left, height - top) if crop else (0, 0)
        spec.full_x, spec.full_y, spec.full_width, spec.full_height = 0, 0, width, height
        return name, Path(bpy.app.tempdir, f"{name}.exr"), spec

    def captured(self, source: bpy.types.Scene, frame: int) -> BlenderResult[None]:
        """Reads this source scene's actual multilayer result with native pass and view identities."""

        def retained(path: str) -> BlenderResult[None]:
            self.files.add(Path(path))
            return Result[Multilayer, BlenderFaults].bind(Multilayer.read(path).map_error(lambda faults: (BlenderImagingFault(faults=faults),)), lambda file: self.indexed(file, source, frame))

        return saved(source).bind(retained)

    def indexed(self, file: Multilayer, source: bpy.types.Scene, frame: int) -> BlenderResult[None]:
        """Adapts native channel identities, stamps, and border-relative tiles to replay."""
        groups: dict[tuple[str, str], list[tuple[Pass, npt.NDArray[np.float32]]]] = {}
        names = tuple(sorted((layer.name for layer in source.view_layers), key=len, reverse=True))
        width, height, left, bottom, right, top = self.bounds(source)

        def plane(layer: Layer) -> BlenderResult[None]:
            owner = next((name for name in names if layer.name.startswith(f"{name}.")), None)
            if owner is None:
                return Ok(None)
            part = file.parts[layer.part]
            view = part.header.get("view", "")
            ids = "".join(channel.name for channel in layer.channels)
            declaration = Pass(name=layer.name[len(owner) + 1 :], chanid=ids, kind="VALUE" if len(ids) == 1 else "COLOR" if ids in {"RGB", "RGBA"} else "VECTOR")

            def retained(values: npt.NDArray[np.float32]) -> None:
                raw = values[height - top : height - bottom, left:right] if values.shape[:2] == (height, width) and source.render.use_border else values
                groups.setdefault((owner, view), []).append((declaration, np.ascontiguousarray(raw)))

            return file.plane(layer, layer.channels).map_error(lambda faults: (BlenderImagingFault(faults=faults),)).map(retained)

        def retained(_: object) -> None:
            stamps = tuple((key, value) for part in file.parts[:1] for key, value in part.header.items())
            self.replay.update({(frame, source.session_uid, owner, view): Tile(x=0, y=0, planes=(planes[0], *planes[1:]), view=Some(view), stamps=stamps) for (owner, view), planes in groups.items()})

        return accumulated(plane(layer) for layer in file.layers).map(retained)

    def output(self, name: str, spec: OpenImageIO.ImageSpec, *, demand: bool) -> BlenderResult[Fragment]:
        """Realizes the prefix on its known canvas while retaining suffix scene render demands."""
        raw = tuple((source, layer.name) for source in self.sources for layer in source.view_layers if layer.use) if demand else ()
        return Fragment.joined(
            node("capture_domain", bpy.types.CompositorNodeBlankImage, {"Size": (spec.width, spec.height)}, {}),
            node("capture_raster", bpy.types.ShaderNodeMixRGB, {"Fac": 1.0, "Color1": End(node="capture_domain", socket="Image"), "Color2": End(node=INPUTS, socket=Shell.image)}, {}),
            node(
                "capture_file",
                bpy.types.CompositorNodeOutputFile,
                {name: End(node="capture_raster", socket="Color")},
                {Pathed.directory.prop.identifier: bpy.app.tempdir, Pathed.file_name.prop.identifier: ""},
                Some(FileItems(items=(FileItem(name=name, kind="RGBA"),))),
            ),
            *(node(f"capture_source_{index}", bpy.types.CompositorNodeRLayers, {}, {"scene": IdName.of(source), "layer": layer}) for index, (source, layer) in enumerate(raw)),
        )

    def formatted(self, tree: bpy.types.NodeTree) -> BlenderResult[None]:
        """Sets full-float data capture with interleaved views and no display conversion."""
        capture = next(live for live in tree.nodes if Mark.read(self.identity, live).filter(lambda mark: mark.key == "capture_file").is_some())
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

    def image(self, key: Key, buffer: OpenImageIO.ImageBuf) -> BlenderResult[tuple[Fragment, End]]:
        """Publishes associated numerical components without another color transform."""

        def made(values: npt.NDArray[np.float32]) -> BlenderResult[tuple[Fragment, End]]:
            def populated(live: bpy.types.Image) -> BlenderResult[tuple[Fragment, End]]:
                self.scope.callback(bpy.data.images.remove, live)
                return (
                    Tag(space=Some(self.config.getColorSpace(PyOpenColorIO.ROLE_DATA).getName()), alpha=Some("PREMUL"))
                    .applied(live)
                    .bind(lambda _: filled(live, values))
                    .bind(lambda _: node(key, bpy.types.CompositorNodeImage, {}, {"image": IdName.of(live)}).map(lambda graph: (graph, End(node=key, socket="Image"))))
                )

            created: BlenderResult[bpy.types.Image] = catch(
                lambda: bpy.data.images.new(f"Prepared {key}", values.shape[1], values.shape[0], alpha=values.shape[2] == 4, float_buffer=True), exception=RuntimeError
            )().map_error(lambda error: (BlenderEditFault(request=key, error=error),))
            return Result[bpy.types.Image, BlenderFaults].bind(created, populated)

        pixels: BlenderResult[npt.NDArray[np.float32]] = Option.of_optional(buffer.get_pixels(OpenImageIO.FLOAT)).to_result_with(
            lambda: (BlenderImagingFault(faults=(ImagingBufferFault(request=buffer, error=buffer.geterror()),)),)
        )
        return Result[npt.NDArray[np.float32], BlenderFaults].bind(pixels, made)

    def stage(self, held: StackEntry, path: Path, spec: OpenImageIO.ImageSpec) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
        """Prepares each native view from evaluated entry controls and real entering pixels."""
        with bpy.context.temp_override(scene=self.scene):
            graph = bpy.context.evaluated_depsgraph_get()
            evaluated = self.scene.evaluated_get(graph)
        reading, row = evaluated.compositor_effects[held.entry.name], held.row.value

        def prepared(file: Multilayer) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
            def view(layer: Layer) -> BlenderResult[tuple[str, Prepared]]:
                def pixels(values: npt.NDArray[np.float32]) -> BlenderResult[tuple[str, Prepared]]:
                    buffer = OpenImageIO.ImageBuf(OpenImageIO.ImageSpec(spec))
                    loaded: BlenderResult[None] = (
                        Ok(None) if buffer.set_pixels(buffer.roi, values) else Error((BlenderImagingFault(faults=(ImagingBufferFault(request=buffer, error=buffer.geterror()),)),))
                    )
                    return (
                        loaded
                        .bind(lambda _: row.read(self.identity, reading))
                        .bind(lambda record: row.prepared(self.identity, evaluated, record, self.config, buffer, self.precision, reading))
                        .map(lambda value: (layer.name, value))
                    )

                return Result[npt.NDArray[np.float32], BlenderFaults].bind(file.plane(layer, layer.channels).map_error(lambda faults: (BlenderImagingFault(faults=faults),)), pixels)

            return accumulated(view(layer) for layer in file.layers).bind(lambda views: self.published(held, reading, views))

        return Result[Multilayer, BlenderFaults].bind(Multilayer.read(str(path)).map_error(lambda faults: (BlenderImagingFault(faults=faults),)), prepared)

    def published(self, held: StackEntry, reading: bpy.types.SceneCompositorEffect, views: tuple[tuple[str, Prepared], ...]) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
        """Builds the job-owned group with blend, effective controls, hidden inputs, and native view routing."""
        row, effective = held.row.value, views[0][1][0]

        def selected(key: Key) -> BlenderResult[tuple[Key, tuple[Fragment, End]]]:
            return accumulated(
                self.image(key if len(views) == 1 else item_key(key, "view", index), value.value if isinstance(value, Estimate) else value)
                for index, (_name, (_record, images)) in enumerate(views)
                for value in (images[key],)
            ).bind(
                lambda values: (
                    Ok((key, values[0]))
                    if len(values) == 1
                    else node(key, bpy.types.CompositorNodeSwitchView, {name: value for (name, _), value in zip(views, values, strict=True)}, {}).map(
                        lambda graph: (key, (graph, End(node=key, socket="Image")))
                    )
                )
            )

        def assigned(group: bpy.types.NodeTree, before: Snapshot) -> BlenderResult[tuple[bpy.types.NodeTree, Snapshot, Entry[Effect]]]:
            self.scope.callback(bpy.data.node_groups.remove, group)
            return (
                written(held.entry, "node_group", group)
                .bind(lambda _: before.apply(held.entry.properties.inputs).outcome)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                .bind(lambda _: stored_graphs(self.identity, group, (((), effective.effect, row.record.stage.domain), ((Shell.blend,), effective.blend, row.record.stage.domain))))
                .bind(
                    lambda _: paired(
                        entered(self.identity, held.entry, (), effective.effect, frozenset(graphed(row.record, value=Some(effective.effect)))),
                        entered(self.identity, held.entry, (Shell.blend,), effective.blend),
                    )
                )
                .map(lambda _: (group, before, effective))
            )

        with bpy.context.temp_override(scene=self.scene):
            return Snapshot.capture(reading.properties.inputs).bind(  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                lambda before: (
                    accumulated(selected(key) for key in views[0][1][1])
                    .bind(lambda images: row.grouped(self.identity, self.scene, effective, self.config, entry=Some(held.entry), prepared=True, images=dict(images)))
                    .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
                    .bind(lambda group: group.made(self.identity, of=Some(self.scene)))
                    .bind(lambda group: assigned(group, before))
                )
            )

    # --- [LIFETIME]

    def native(self, properties: Mapping[str, object]) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
        """Records the explicit job's output encoding at production, before restoring authored controls."""
        enabled = tuple(held for held in self.stack.held(self.identity, self.scene) if held.entry.enable_for_render) if self.scene.render.use_compositing else ()
        space: BlenderResult[Option[str]] = Ok(Some(self.config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()))
        viewer: BlenderResult[Option[str]] = Ok(Nothing)
        for held in enabled:
            entering = space
            if held.row.is_none():
                space = Ok(Nothing)
            elif held.row.value.record is DisplayView:
                space = space.bind(lambda _: formation.read(self.identity, held.entry).map(lambda record: record.effect.colorspace(self.config).map(lambda found: found.getName())))
            for tree in Option.of_optional(held.entry.node_group).to_list():
                for live in tree.nodes:
                    if not held.entry.is_active or not isinstance(live, bpy.types.CompositorNodeViewer):
                        continue
                    for link in live.inputs[0].links:  # type: ignore[index, union-attr]  # ty: ignore[not-iterable, not-subscriptable]
                        match link.from_node:
                            case bpy.types.NodeGroupInput():
                                viewer = entering
                            case bpy.types.CompositorNodeImage(image=bpy.types.Image() as image) if image.colorspace_settings.is_data:  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                                viewer = Ok(Some(image.colorspace_settings.name))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                            case bpy.types.CompositorNodeConvertToDisplay() as display:
                                viewer = Ok(viewed(display).colorspace(self.config).map(lambda found: found.getName()))
                            case bpy.types.CompositorNodeConvertColorSpace() as conversion:
                                viewer = Ok(Some(self.config.getColorSpace(conversion.to_color_space).getName()))
                            case _:
                                viewer = space if any(isinstance(edge.to_node, bpy.types.NodeGroupOutput) and edge.from_socket == link.from_socket for edge in tree.links) else Ok(Nothing)
        layer = (
            "Composite.Combined"
            if enabled
            else f"{Option.of_optional(properties.get('layer')).bind(lambda value: value if isinstance(value, Option) else Some(value)).default_value(self.scene.view_layers[0].name)}.Combined"
        )
        view = (
            Nothing
            if any(held.row.filter(lambda row: row.record is DisplayView).is_some() for held in enabled)
            else Some((viewed(self.scene), self.scene.view_settings.exposure, self.scene.view_settings.gamma))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        )

        def rendered(origins: tuple[Option[str], Option[str]]) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
            for image in bpy.data.images:
                if image.type in {"RENDER_RESULT", "COMPOSITING"}:
                    produced(image, Nothing)

            def published(status: frozenset["rna_enums.OperatorReturnItems"]) -> frozenset["rna_enums.OperatorReturnItems"]:
                for image in bpy.data.images:
                    if image.type == "RENDER_RESULT":
                        produced(image, origins[0].map(lambda encoding: Production(scene=Handle.of(self.scene), space=encoding, layer=Some(layer), view=view)))
                    elif image.type == "COMPOSITING":
                        produced(
                            image,
                            origins[1].map(
                                lambda encoding: Production(
                                    scene=Handle.of(self.scene), space=encoding, view=view if encoding == self.config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName() else Nothing
                                )
                            ),
                        )
                return status

            return called(bpy.ops.render.render, dict(properties) | {"scene": self.scene.name}).map(published)

        return paired(space, viewer).bind(rendered)

    def restored(self, original: tuple[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree | None, Snapshot], ...]) -> None:
        """Restores authored pointers before their original native input values."""
        for entry, group, inputs in original:
            entry.node_group = group
            for faults in inputs.apply(entry.properties.inputs).outcome.swap().to_option().to_list():  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                self.identity.reports.hold(faults)

    def selected(self, scene: bpy.types.Scene, _graph: bpy.types.Depsgraph | None = None) -> None:
        """Assigns a frame's prepared groups before native animation dependency evaluation."""
        if scene.session_uid == self.scene.session_uid:
            for entry, group, inputs, effective in self.prepared[self.scene.frame_current]:
                entry.node_group = group
                answer = inputs.apply(entry.properties.inputs).outcome.bind(  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                    lambda _: paired(
                        entered(self.identity, entry, (), effective.effect, frozenset(graphed(type(effective.effect), value=Some(effective.effect)))),
                        entered(self.identity, entry, (Shell.blend,), effective.blend),
                    )
                )
                for faults in answer.swap().to_option().to_list():
                    self.identity.reports.hold(faults)

    def removed(self) -> None:
        """Releases capture files and retains filesystem refusals in native reports."""
        for path in self.files:
            for faults in catch(lambda: path.unlink(missing_ok=True), exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=str(path), error=error),)).swap().to_option().to_list():
                self.identity.reports.hold(faults)

    def frame(
        self,
        frame: int,
        subframe: float,
        engine: str,
        original: tuple[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree | None, Snapshot], ...],
        engines: tuple[tuple[bpy.types.Scene, str], ...],
    ) -> BlenderResult[None]:
        """Captures geometry once, then advances CPU preparation through native replay prefixes."""
        self.restored(original)
        for source, original_engine in engines:
            source.render.engine = original_engine  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        self.scene.frame_set(frame, subframe=subframe)
        name, path, spec = self.raster(frame)
        self.files.add(path)
        completed: list[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree, Snapshot, Entry[Effect]]] = []
        for index, held in enumerate(self.stack.stages(self.identity, self.scene)):
            declaration = self.output(name, spec, demand=index == 0)
            if declaration.is_error():
                return Error(declaration.error)
            with ExitStack() as capture:
                for source, _original_engine in engines:
                    capture.enter_context(intermediate(source))
                isolated = capture.enter_context(self.isolated(tuple(source for source, _original_engine in engines)))
                leased = capture.enter_context(capturing(self.identity, held.entry, declaration.ok, stop=True))
                answer = (
                    isolated.bind(lambda _: leased).bind(lambda entry: self.formatted(entry.node_group)).bind(lambda _: called(bpy.ops.render.render, {"scene": self.scene.name, "write_still": False}))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                )
            if answer.is_error():
                return Error(answer.error)
            if index == 0:
                captured = accumulated(self.captured(source, frame) for source, _original_engine in engines)
                if captured.is_error():
                    return Error(captured.error)
                for source, _original_engine in engines:
                    source.render.engine = engine  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            prepared = self.stage(held, path, spec)
            if prepared.is_error():
                return Error(prepared.error)
            completed.append((held.entry, *prepared.ok))
        self.prepared[frame] = tuple(completed)
        return Ok(None)

    def rendered(self, properties: Mapping[str, object]) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
        """Holds all resources through the final native still, animation, or movie output."""
        scene = self.scene
        pending = deque((scene,))
        found: dict[int, bpy.types.Scene] = {}
        while pending:
            source = pending.popleft()
            if source.session_uid in found:
                continue
            found[source.session_uid] = source
            trees = deque(entry.node_group for entry in source.compositor_effects if entry.enable_for_render and entry.node_group is not None)
            seen: set[int] = set()
            while trees:
                tree = trees.popleft()
                if tree.session_uid in seen:
                    continue
                seen.add(tree.session_uid)
                trees.extend(live.node_tree for live in tree.nodes if isinstance(live, bpy.types.CompositorNodeGroup) and not live.mute and live.node_tree is not None)
                pending.extend(
                    live.scene
                    for live in tree.nodes
                    if isinstance(live, bpy.types.CompositorNodeRLayers | bpy.types.CompositorNodeCryptomatteV2)
                    and not live.mute
                    and live.scene is not None
                    and (not isinstance(live, bpy.types.CompositorNodeCryptomatteV2) or live.source == "RENDER")
                )
        sources = tuple(found.values())
        self.sources = sources
        engines = tuple((source, source.render.engine) for source in sources)
        animation = properties.get("animation") is True
        frames = tuple(range(scene.frame_start, scene.frame_end + 1, scene.frame_step)) if animation else (scene.frame_current,)
        at, subframe = scene.frame_current, scene.frame_subframe
        sample = 0.0 if animation else subframe
        original = accumulated(Snapshot.capture(held.entry.properties.inputs).map(lambda inputs: (held.entry, held.entry.node_group, inputs)) for held in self.stack.stages(self.identity, scene))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

        def completed(engine: str) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
            for source, _original_engine in engines:
                source.render.engine = engine  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            bpy.app.handlers.frame_change_pre.append(self.selected)
            self.scope.callback(bpy.app.handlers.frame_change_pre.remove, self.selected)
            scene.frame_set(frames[0], subframe=sample)
            return self.native(properties)

        def prepared(original: tuple[tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree | None, Snapshot], ...]) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
            self.scope.callback(self.restored, original)
            with Engine.replayed(self.identity, "prepared_render", self.replay, lambda: scene.frame_current) as engine:
                try:
                    return engine.bind(
                        lambda name: reduce(lambda done, frame: done.bind(lambda _: self.frame(frame, sample, name, original, engines)), frames, Ok(None)).bind(lambda _: completed(name))
                    )
                finally:
                    for source, original_engine in engines:
                        source.render.engine = original_engine

        with self.scope:
            for source in sources:
                self.stack.executing.add(source.session_uid)
                self.scope.callback(self.stack.executing.remove, source.session_uid)
            self.scope.callback(scene.frame_set, at, subframe=subframe)
            self.scope.callback(self.removed)
            return original.bind(prepared)


# --- [OPERATIONS] -----------------------------------------------------------------------


def rendered(identity: Identity, stack: EffectStack, scene: bpy.types.Scene, precision: Precision, properties: Mapping[str, object]) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
    """Runs numerical stages where present, otherwise invokes the native renderer directly."""
    execution = Execution(identity, stack, scene, precision)
    return execution.rendered(properties) if scene.render.use_compositing and stack.stages(identity, scene) else execution.native(properties)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Execution", "rendered"]
