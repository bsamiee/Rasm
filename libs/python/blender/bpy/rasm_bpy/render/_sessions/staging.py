"""CPU compositor stages with captured inputs, prepared groups, replayed passes, and native rendering."""

from collections import deque
from collections.abc import Callable, Mapping
from contextlib import ExitStack
from functools import partial, reduce
from itertools import starmap
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Final, Self, TYPE_CHECKING

import bpy
from expression import Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from rasm_bpy.commands.operators import called
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions, Declaration, Handler
from rasm_bpy.data._ids.images import loaded, Tag
from rasm_bpy.data._ids.ownership import Saved
from rasm_bpy.data._ids.store import IdName
from rasm_bpy.data._rna.descriptors import Descriptor, Value, written
from rasm_bpy.data._rna.snapshots import assigned, Snapshot
from rasm_bpy.nodes import functions
from rasm_bpy.nodes._addressing import InterfaceKeys, owned, OwnedNodes
from rasm_bpy.nodes._desired import INPUTS, member_paths
from rasm_bpy.nodes._graphs import Expr, group_input, rgba, vector
from rasm_bpy.nodes._interfaces import Interface
from rasm_bpy.nodes._trees import base_socket, created, DATA_TYPES, endpoints, group_set, group_tree, linked_sockets, links_of, nested, socket_at
from rasm_bpy.nodes.changes import made, NodeGroup
from rasm_bpy.render._compositor.colors import COLOR_INPUT, COLOR_RESULT
from rasm_bpy.render._compositor.coordinates import dimensions, FrameSpace
from rasm_bpy.render._compositor.effects import Prepared, Stage
from rasm_bpy.render._compositor.entries import EXECUTING
from rasm_bpy.render._compositor.inputs import CameraFrame, captured, grouped, inputs_of, interface_input, restored, value, valued
from rasm_bpy.render._compositor.masks import relayed
from rasm_bpy.render._compositor.offered import OfferedEffects
from rasm_bpy.render._compositor.stack_service import LayerStackService, stacked_by, StageTarget
from rasm_bpy.render._outputs.formats import Colorspace, FileItem, FileItems, OutputFormat
from rasm_bpy.render._outputs.framing import FrameSize
from rasm_bpy.render._outputs.paths import Pathed
from rasm_bpy.render._outputs.results import intermediate, INTERMEDIATES, RENDER_ORIGINS
from rasm_bpy.render._sessions.engines import Engine, Tile
from rasm_bpy.render._sessions.renders import engined, native, reads_render, RenderCall
from rasm_bpy.render._view_layers.passes import used
from rasm_imaging import checked, emitted, Estimate, ImageFile, Layer, Multilayer, pixels, Precision, Raster, Region, Subimage
from rasm_records import accumulated, content_hash, Error, Fault, Nothing, Ok, Outcome, paired, Reason, removed

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Owner = bpy.types.SceneCompositorEffect | bpy.types.CompositorNodeGroup

# --- [CONSTANTS] ------------------------------------------------------------------------

_CAPTURE: Final[Key] = "capture_file"

# --- [MODELS] ---------------------------------------------------------------------------


class _Authored(msgspec.Struct, frozen=True, kw_only=True):
    """Compositor entry with its group and input snapshot, the state each frame and scope exit assign."""

    entry: bpy.types.SceneCompositorEffect
    group: bpy.types.NodeTree | None
    inputs: Snapshot

    @classmethod
    def of(cls, entry: bpy.types.SceneCompositorEffect) -> Outcome[Self]:
        """Reads an entry's group and inputs."""
        return inputs_of(entry).bind(Snapshot.capture).map(lambda inputs: cls(entry=entry, group=entry.node_group, inputs=inputs))

    def assigned(self) -> Outcome[None]:
        """Points the entry at the group, then applies the input snapshot."""
        return written(self.entry, ("node_group",), self.group).bind(lambda _: inputs_of(self.entry)).bind(lambda found: self.inputs.apply(found).outcome)


# --- [SERVICES] -------------------------------------------------------------------------


class _Job:
    """Staged render of one scene owning its capture files, replay tiles, prepared groups, and temporary data until its scope exits."""

    # --- [STATE]

    __slots__ = ("config", "files", "identity", "precision", "prepared", "rasters", "replay", "scene", "scope", "sources")

    def __init__(self, identity: Identity, scene: bpy.types.Scene, precision: Precision, rasters: Option[tuple[Raster, Raster]] = Nothing) -> None:
        """Holds the current OCIO config, the scene, and every scene its render-enabled groups read through unmuted Render Layers or render Cryptomatte nodes, each once."""
        self.identity, self.scene, self.precision, self.rasters = identity, scene, precision, rasters
        self.config = PyOpenColorIO.GetCurrentConfig()
        self.scope = ExitStack()
        self.replay: dict[tuple[int, int, str], tuple[Tile, ...]] = {}
        self.prepared: dict[int, tuple[_Authored, ...]] = {}
        self.files: set[Path] = set()
        reached, pending = dict.fromkeys((scene,)), deque((scene,))
        while pending:
            for source in (
                live.scene
                for tree in nested((entry.node_group for entry in pending.popleft().compositor_effects if entry.enable_for_render and entry.node_group is not None), muted=False)
                for live in tree.nodes
                if not live.mute and reads_render(live) and live.scene is not None
            ):
                if source not in reached:
                    reached[source] = None
                    pending.append(source)
        self.sources = tuple(reached)

    # --- [CAPTURE]

    def copied(self, scope: ExitStack) -> Outcome[Mapping[bpy.types.NodeTree, bpy.types.CompositorNodeTree]]:
        """Swaps render-enabled groups for native copies until the scope exits."""
        entries = tuple(
            (entry, tree) for source in self.sources for entry in source.compositor_effects if entry.enable_for_render and isinstance(tree := entry.node_group, bpy.types.CompositorNodeTree)
        )
        copies: dict[bpy.types.NodeTree, bpy.types.CompositorNodeTree] = {tree: copy for tree in nested(tree for _, tree in entries) for copy in Option.of_optional(tree.copy()).to_list()}
        for copy in copies.values():
            scope.callback(bpy.data.node_groups.remove, copy)
        for live in (live for tree in copies.values() for live in tree.nodes if isinstance(live, bpy.types.CompositorNodeGroup)):
            for child in Option.of_optional(live.node_tree).to_list():
                live.node_tree = copies[child]

        def swapped(authored: _Authored, group: bpy.types.CompositorNodeTree) -> Outcome[None]:
            scope.callback(self.identity.reports.reported(self.restored), (authored,))
            return msgspec.structs.replace(authored, group=group).assigned()

        return accumulated(_Authored.of(entry).bind(lambda authored: swapped(authored, copies[tree])) for entry, tree in entries).map(lambda _: copies)

    def isolated(self, scope: ExitStack) -> Outcome[Mapping[bpy.types.NodeTree, bpy.types.CompositorNodeTree]]:
        """Copies render-enabled groups and mutes their output nodes for an isolated capture."""

        def muted(copies: Mapping[bpy.types.NodeTree, bpy.types.CompositorNodeTree]) -> Mapping[bpy.types.NodeTree, bpy.types.CompositorNodeTree]:
            for live in (live for tree in copies.values() for live in tree.nodes if isinstance(live, bpy.types.CompositorNodeOutputFile | bpy.types.CompositorNodeViewer)):
                live.mute = True
            return copies

        return self.copied(scope).map(muted)

    def captured(self, source: bpy.types.Scene, frame: int) -> Outcome[None]:
        """Reads the source's native pass receipt and holds its tiles for replay."""
        return (
            Multilayer
            .read(self.pass_path(source))
            .bind(lambda file: Tile.replayed(file, source))
            .map(lambda tiles: self.replay.update({(frame, source.session_uid, layer): held for layer, held in tiles.items()}))
        )

    def pass_path(self, source: bpy.types.Scene) -> Path:
        """Owns the raw pass path shared by native production and replay."""
        path = Path(bpy.app.tempdir, f"{source.session_uid}_passes.exr")
        self.files.add(path)
        return path

    def passes(self, tree: bpy.types.NodeTree) -> Outcome[None]:
        """Connects the capture group's actual render-pass sockets to source-owned native files."""

        def recorded(source: bpy.types.Scene) -> Outcome[None]:
            sockets = {
                f"{node.layer}.{Engine.combined.name if socket.identifier == 'Image' else socket.identifier}": socket
                for node in tree.nodes
                for socket in node.outputs
                if isinstance(node, bpy.types.CompositorNodeRLayers) and node.scene == source and socket.enabled and not socket.is_unavailable and socket.identifier != "Alpha"
            }

            def item(key: Key, socket: bpy.types.NodeSocket) -> Outcome[tuple[Key, FileItem]]:
                declared = FileItem(name=key, socket_type=DATA_TYPES[base_socket(type(socket))])
                return (
                    Descriptor.resolve(socket, ("default_value",), bpy.types.FloatProperty).map(
                        lambda descriptor: (key, msgspec.structs.replace(declared, vector_socket_dimensions=descriptor.prop.array_length))
                    )
                    if socket.type == "VECTOR"
                    else Ok((key, declared))
                )

            def wrote(content: FileItems, output: bpy.types.Node) -> Outcome[None]:
                path = self.pass_path(source)
                output_format = msgspec.structs.replace(self.format, media_type="MULTI_LAYER_IMAGE", file_format="OPEN_EXR_MULTILAYER", use_exr_interleave=False)
                return (
                    content
                    .write(output, self.identity)
                    .bind(lambda _: assigned(output, member_paths({Pathed.directory.path: str(path.parent), Pathed.file_name.path: path.stem, **output_format.members(("format",))})))
                    .bind(lambda _: accumulated(linked_sockets(tree, sockets[key], output.inputs[identifier]) for key, identifier in content.sockets(output, self.identity).items()))
                    .map(lambda _: None)
                )

            return accumulated(starmap(item, sockets.items())).bind(lambda items: created(tree, bpy.types.CompositorNodeOutputFile).bind(partial(wrote, FileItems(items=dict(items)))))

        return accumulated(map(recorded, self.sources)).map(lambda _: None)

    def output(self, name: str, raster: Expr, directory: Path, frame: CameraFrame, *, demand: bool) -> tuple[Expr, ...]:
        """Captures realized input pixels and native coordinate bounds in the same multiview render."""
        realized = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", raster),), socket="Image", socket_type=bpy.types.NodeSocketColor)
        location = dimensions(realized).at("Location", bpy.types.NodeSocketVector)
        neutral = Expr(kind=bpy.types.CompositorNodeTranslate, inputs=(("Image", realized), ("X", -location["X"]), ("Y", -location["Y"])), socket="Image", socket_type=bpy.types.NodeSocketColor)
        space = FrameSpace.of(realized, frame.aspect, frame.scale)
        pitch = vector("DIVIDE", (1.0, 1.0, 0.0), space.scale)
        minimum, maximum = (
            tuple(Expr(kind=bpy.types.CompositorNodeLevels, inputs=(("Image", space.point), ("Channel", channel)), socket=socket) for channel in ("Red", "Green")) for socket in ("Minimum", "Maximum")
        )
        terms: Mapping[Key, Expr] = {
            "carrier": Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Size", (2, 1)),), socket="Image", socket_type=bpy.types.NodeSocketColor),
            "bounds": rgba(
                minimum[0] + (location["X"] - 0.5) * pitch["X"],
                minimum[1] + (location["Y"] - 0.5) * pitch["Y"],
                maximum[0] + (location["X"] + 0.5) * pitch["X"],
                maximum[1] + (location["Y"] + 0.5) * pitch["Y"],
            ),
        }
        output = msgspec.structs.replace(
            self.format,
            media_type="MULTI_LAYER_IMAGE",
            file_format="OPEN_EXR_MULTILAYER",
            use_exr_interleave=False,
            linear_colorspace_settings=Colorspace(name=self.config.getColorSpace(PyOpenColorIO.ROLE_DATA).getName()),
        )
        layers = (
            Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(source), "layer": layer.name}, socket="Image", socket_type=bpy.types.NodeSocketColor)
            for source in (self.sources if demand else ())
            for layer in used(source)
        )
        return (
            Expr(
                kind=bpy.types.CompositorNodeOutputFile,
                inputs=(("image", neutral),),
                members={Pathed.directory.path: str(directory), Pathed.file_name.path: "", **msgspec.structs.replace(self.format, use_exr_interleave=False).members(("format",))},
                content=Some(FileItems(items={"image": FileItem(name=name, socket_type="RGBA")})),
            ).keyed(_CAPTURE),
            Expr(
                kind=bpy.types.CompositorNodeOutputFile,
                inputs=tuple(terms.items()),
                members={Pathed.directory.path: str(directory), Pathed.file_name.path: f"{name}_geometry", **output.members(("format",))},
                content=Some(FileItems(items={key: FileItem(name=key, socket_type="RGBA") for key in terms})),
            ).keyed(f"{_CAPTURE}_geometry"),
            *layers,
        )

    @property
    def format(self) -> OutputFormat:
        """Native raw capture format under the job's configuration."""
        return OutputFormat(
            media_type="IMAGE",
            file_format="OPEN_EXR",
            color_mode="RGBA",
            color_depth="32",
            exr_codec="ZIP",
            use_exr_interleave=True,
            views_format="MULTIVIEW",
            color_management="OVERRIDE",
            linear_colorspace_settings=Colorspace(name=self.config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()),
        )

    # --- [PREPARATION]

    @staticmethod
    def resolved(entry: bpy.types.SceneCompositorEffect, path: tuple[bpy.types.CompositorNodeGroup, ...]) -> Outcome[tuple[tuple[_Owner, ...], bpy.types.NodeTree]]:
        """Resolves an instance path against the entry's current job-owned copies."""

        def step(done: Outcome[tuple[tuple[_Owner, ...], bpy.types.NodeTree]], original: bpy.types.CompositorNodeGroup) -> Outcome[tuple[tuple[_Owner, ...], bpy.types.NodeTree]]:
            def entered(found: tuple[tuple[_Owner, ...], bpy.types.NodeTree]) -> Outcome[tuple[tuple[_Owner, ...], bpy.types.NodeTree]]:
                owners, tree = found
                node = {node.name: node for node in tree.nodes if isinstance(node, bpy.types.CompositorNodeGroup)}[original.name]
                return group_tree(node).to_result_with(partial(Fault.of, Reason.ABSENT, node.name, member="node_tree")).map(lambda tree: ((*owners, node), tree))

            return done.bind(entered)

        return reduce(step, path, grouped(entry).map(lambda tree: ((entry,), tree)))

    def active(self, target: tuple[bpy.types.SceneCompositorEffect, StageTarget]) -> Outcome[bool]:
        """Reads parent execution controls and the preparation gate from evaluated native inputs."""
        entry, (path, _, record, _, gates) = target
        gate = record.record.prepared_when
        if not gates and gate is None:
            return Ok(value=True)
        with bpy.context.temp_override(scene=self.scene):
            evaluated = self.scene.evaluated_get(bpy.context.evaluated_depsgraph_get())
        reading = evaluated.compositor_effects[entry.name]
        for owner, keys in gates.items():
            if (
                enabled := self.resolved(entry, owner).bind(lambda found: self.values(found[0], reading, found[1])).bind(lambda values: accumulated(value(values, bool, key) for key in keys).map(all))
            ).is_error():
                return Error(enabled.error)
            if not enabled.ok:
                return Ok(value=False)
        return Ok(value=True) if gate is None else self.resolved(entry, path).bind(lambda found: self.values(found[0], reading, found[1])).bind(lambda values: value(values, bool, gate))

    def values(self, owners: tuple[_Owner, ...], reading: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree) -> Outcome[Mapping[Key, Value]]:
        """Reads target sockets through their native group-input ancestry to evaluated entry values."""
        target = owners[-1]
        if isinstance(target, bpy.types.SceneCompositorEffect):
            return captured(self.identity, reading).map(valued)
        keys = InterfaceKeys.of(tree, self.identity).sockets

        def read(socket: bpy.types.NodeSocket) -> Outcome[Value]:
            for owner in reversed(owners[:-1]):
                if not (links := links_of(socket)):
                    return Descriptor.resolve(socket, ("default_value",), bpy.types.Property).map(lambda descriptor: descriptor.read(socket))
                source, outlet, _, _ = endpoints(links[0])
                if not isinstance(source, bpy.types.NodeGroupInput):
                    return Error(Fault.of(Reason.OUTSIDE, source.name, member="CPU scalar input"))
                if isinstance(owner, bpy.types.SceneCompositorEffect):
                    return interface_input(reading, outlet.identifier).bind(lambda value: Descriptor.resolve(value, ("value",), bpy.types.Property).map(lambda descriptor: descriptor.read(value)))
                socket = owner.inputs[outlet.identifier]
            return Descriptor.resolve(socket, ("default_value",), bpy.types.Property).map(lambda descriptor: descriptor.read(socket))

        return accumulated(
            read(socket).map(lambda value: (key, value)) for key, identifier in keys.items() if key != COLOR_INPUT.key for socket in target.inputs if socket.identifier == identifier
        ).map(dict)

    def image(self, target: tuple[bpy.types.SceneCompositorEffect, StageTarget], buffers: tuple[tuple[str, OpenImageIO.ImageBuf], ...]) -> Outcome[bpy.types.Image]:
        """Writes prepared pixels as one EXR part per view and loads them as one premultiplied data image, multiview across views."""
        entry, (nodes, _, _, _, _) = target
        key = content_hash(Stage, entry.name, tuple(node.name for node in nodes)).hex()
        path = Path(bpy.app.tempdir, f"{self.scene.session_uid}_{self.scene.frame_current}_{key}_prepared.exr")
        views = tuple(name for name, _ in buffers)
        headed = len(views) > 1
        self.files.add(path)

        def parts(_output: OpenImageIO.ImageOutput) -> tuple[Subimage, ...]:
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
            return (assigned(live, {("use_multiview",): True, ("views_format",): "INDIVIDUAL"}) if headed else Ok(None)).map(lambda _: live)

        return emitted(path, parts).bind(lambda held: loaded(str(held), Tag(space=Some(self.config.getColorSpace(PyOpenColorIO.ROLE_DATA).getName()), alpha=Some("PREMUL")))).bind(tagged)

    def stage(self, target: tuple[bpy.types.SceneCompositorEffect, StageTarget], path: Path, camera: Raster) -> Outcome[_Authored]:
        """Runs the entry's CPU step per view over the captured pixels, its evaluated input values, and its camera frame."""
        with bpy.context.temp_override(scene=self.scene):
            evaluated = self.scene.evaluated_get(bpy.context.evaluated_depsgraph_get())
        entry, (_, _, _, prepare, _) = target
        reading = evaluated.compositor_effects[entry.name]

        def views(file: Multilayer, geometry: Multilayer, owners: tuple[_Owner, ...], values: Mapping[Key, Value]) -> Outcome[_Authored]:
            def view(layer: Layer) -> Outcome[tuple[str, Option[Prepared], tuple[Raster, Raster]]]:
                spec = file.parts[layer.part]
                metadata = next(item for item in geometry.layers if geometry.parts[item.part].get_string_attribute("view") == spec.get_string_attribute("view") and item.name == "bounds")

                def prepared(image: OpenImageIO.ImageBuf, bounds: npt.NDArray[np.float32]) -> Outcome[tuple[str, Option[Prepared], tuple[Raster, Raster]]]:
                    left, bottom, right, top = bounds[0, 0]
                    region = Region(left=float(left), bottom=float(bottom), right=float(right), top=float(top))
                    for source, _ in self.rasters.to_list():
                        factor = max(source.bounds.right - source.bounds.left, source.bounds.top - source.bounds.bottom)
                        region = Region(
                            left=source.bounds.left + region.left * factor,
                            bottom=source.bounds.bottom + region.bottom * factor,
                            right=source.bounds.left + region.right * factor,
                            top=source.bounds.bottom + region.top * factor,
                        )
                    rasters = (Raster(bounds=region, shape=(spec.height, spec.width)), camera)
                    return prepare(Stage(identity=self.identity, values=values, config=self.config, image=image, rasters=rasters, scene=evaluated, precision=self.precision)).map(
                        lambda value: (spec.get_string_attribute("view"), value, rasters)
                    )

                return paired(checked(OpenImageIO.ImageBuf(str(path), layer.part, 0)), geometry.plane(metadata, ("R", "G", "B", "A"))).bind(lambda pair: prepared(*pair))

            return accumulated(map(view, file.layers)).bind(lambda found: self.published(target, owners[-1], found))

        def ready(found: tuple[tuple[_Owner, ...], bpy.types.NodeTree]) -> Outcome[_Authored]:
            owners, tree = found
            return self.values(owners, reading, tree).bind(
                lambda values: paired(Multilayer.read(path), Multilayer.read(path.with_stem(f"{path.stem}_geometry"))).bind(lambda files: views(*files, owners, values))
            )

        return self.resolved(target[0], target[1][0]).bind(ready)

    def estimated(self, entry: bpy.types.SceneCompositorEffect, view: str, value: OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]) -> OpenImageIO.ImageBuf:
        """Returns a prepared image, logging an estimate's largest replicate standard error."""
        match value:
            case OpenImageIO.ImageBuf():
                return value
            case _:
                self.identity.reports.logger.debug("Staged estimate", entry=entry.name, view=view, standard_error=float(np.max(value.standard_error, initial=0.0)))
                return value.value

    def published(self, target: tuple[bpy.types.SceneCompositorEffect, StageTarget], owner: _Owner, views: tuple[tuple[str, Option[Prepared], tuple[Raster, Raster]], ...]) -> Outcome[_Authored]:
        """Enters measured values into the entry, or builds and assigns its job-owned group over the prepared image, answering the group and inputs each frame assigns."""
        entry, (_, row, record, _, _) = target
        results = tuple((name, value) for name, found, _ in views for value in found.to_list())
        images = tuple((name, self.estimated(entry, name, value)) for name, value in results if isinstance(value, OpenImageIO.ImageBuf | Estimate))

        def owned(group: bpy.types.NodeTree) -> bpy.types.NodeTree:
            for tree in nested((group,)):
                if tree.is_runtime_data:
                    self.scope.callback(bpy.data.node_groups.remove, tree)
            return group

        def grouped(image: Option[bpy.types.Image]) -> Outcome[bpy.types.NodeTree]:
            def view(name: str, prepared: Option[Prepared], rasters: tuple[Raster, Raster]) -> Outcome[NodeGroup]:
                measured = prepared.to_optional()
                staged = image.map(IdName.of) if isinstance(measured, OpenImageIO.ImageBuf | Estimate) else Nothing
                return row.grouped(
                    self.identity,
                    self.scene,
                    record,
                    self.config,
                    entry=Some(entry) if isinstance(owner, bpy.types.SceneCompositorEffect) else Nothing,
                    staged=staged,
                    rasters=Some(measured if isinstance(measured, tuple) else rasters),
                ).map(lambda group: msgspec.structs.replace(group, key=f"{group.key}_{name}", name=f"{group.name} {name}", single=False, saved=Saved.NEVER))

            def combined(groups: tuple[NodeGroup, ...]) -> Outcome[bpy.types.NodeTree]:
                def placed(name: str, prepared: Option[Prepared], group: NodeGroup) -> tuple[str, Expr]:
                    inputs = relayed(group_input, group.interface)
                    measured = prepared.to_optional()
                    if isinstance(measured, Mapping):
                        inputs.update(measured)
                    return name, functions.placed(group, inputs, COLOR_RESULT.key)

                outputs = tuple(placed(name, prepared, group) for (name, prepared, _), group in zip(views, groups))
                result = outputs[0][1] if len(outputs) == 1 else Expr(kind=bpy.types.CompositorNodeSwitchView, inputs=outputs, socket="Image", socket_type=bpy.types.NodeSocketColor)
                return (
                    functions
                    .built(row.ownership(record).key, groups[0].interface, {COLOR_RESULT.key: result}, bpy.types.CompositorNodeTree, name=Some(row.title))
                    .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
                    .bind(lambda group: made(group, self.identity, of=Some(self.scene)))
                    .map(owned)
                    .bind(lambda tree: row.stored(self.identity, row.record).write(tree, record).map(lambda _: tree))
                )

            with bpy.context.temp_override(scene=self.scene):
                return accumulated(starmap(view, views)).bind(combined)

        def placed(tree: bpy.types.NodeTree) -> Outcome[_Authored]:
            wrote = (
                captured(self.identity, owner).bind(lambda values: written(owner, ("node_group",), tree).bind(lambda _: restored(self.identity, owner, values)).map(lambda _: None))
                if isinstance(owner, bpy.types.SceneCompositorEffect)
                else group_set(owner, tree)
            )
            return wrote.bind(lambda _: _Authored.of(entry))

        return (self.image(target, images).map(Some) if images else Ok(Nothing)).bind(grouped).bind(placed)

    # --- [LIFECYCLE]

    @staticmethod
    def restored(groups: tuple[_Authored, ...]) -> Outcome[None]:
        """Assigns entry groups and inputs."""
        return accumulated(held.assigned() for held in groups).map(lambda _: None)

    def selected(self, scene: bpy.types.Scene) -> Outcome[None]:
        """Assigns the current frame's prepared groups and inputs from `frame_change_pre`."""
        return self.restored(self.prepared.get(self.scene.frame_current, ())) if scene.session_uid == self.scene.session_uid else Ok(None)

    def removed(self) -> Outcome[None]:
        """Deletes capture and prepared files."""
        return accumulated(map(removed, self.files)).map(lambda _: None)

    def frame(
        self, frame: int, subframe: float, engine: str, original: tuple[_Authored, ...], staged: tuple[tuple[bpy.types.SceneCompositorEffect, StageTarget], ...], call: RenderCall
    ) -> Outcome[None]:
        """Captures the frame render once under the authored groups and engines, then prepares each staged entry from its replayed prefix."""
        name = f"{self.scene.session_uid}_{frame}_entering"
        path = Path(bpy.app.tempdir, f"{name}.exr")
        self.files.update((path, path.with_stem(f"{name}_geometry")))
        raster = group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor)

        def prefixed(target: tuple[bpy.types.SceneCompositorEffect, StageTarget], camera: CameraFrame, *, demand: bool) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
            with ExitStack() as capture, bpy.context.temp_override(scene=self.scene):
                at = tuple(self.scene.compositor_effects).index(target[0])
                for entry in tuple(self.scene.compositor_effects)[at + 1 :]:
                    capture.callback(setattr, entry, "enable_for_render", entry.enable_for_render)
                    entry.enable_for_render = False
                for source in self.sources:
                    capture.enter_context(intermediate(self.identity, source))

                def attached(tree: bpy.types.NodeTree) -> Outcome[None]:
                    source = socket_at(owned(tree, self.identity)[INPUTS].outputs, COLOR_INPUT.key, InterfaceKeys.of(tree, self.identity).sockets)
                    roots = self.output(name, raster, path.parent, camera, demand=demand)
                    built = functions.built("stage_capture", Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: raster.keeping(*roots)}, bpy.types.CompositorNodeTree)

                    def placed(source: bpy.types.NodeSocket, group: bpy.types.NodeTree) -> Outcome[None]:
                        capture.callback(bpy.data.node_groups.remove, group)
                        edges = links_of(source)

                        def connected(node: bpy.types.CompositorNodeGroup) -> Outcome[None]:
                            keys = InterfaceKeys.of(group, self.identity).sockets

                            def routed(outlet: bpy.types.NodeSocket, link: bpy.types.NodeLink) -> Outcome[None]:
                                _, _, _, inlet = endpoints(link)

                                def replaced(made: bpy.types.NodeLink) -> None:
                                    if inlet.is_multi_input:
                                        made.swap_multi_input_sort_id(link)
                                    tree.links.remove(link)

                                return linked_sockets(tree, outlet, inlet, verify_limits=False).map(replaced)

                            return (
                                group_set(node, group)
                                .bind(lambda _: paired(socket_at(node.inputs, COLOR_INPUT.key, keys), socket_at(node.outputs, COLOR_RESULT.key, keys)))
                                .bind(lambda pair: linked_sockets(tree, source, pair[0]).bind(lambda _: accumulated(routed(pair[1], link) for link in edges)).map(lambda _: None))
                            )

                        return (self.passes(group) if demand else Ok(None)).bind(lambda _: created(tree, bpy.types.CompositorNodeGroup).bind(connected))

                    return paired(source, built.map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER)).bind(lambda group: made(group, self.identity))).bind(
                        lambda pair: placed(*pair)
                    )

                def selected(found: tuple[tuple[_Owner, ...], bpy.types.NodeTree]) -> Outcome[None]:
                    owners, tree = found
                    owner = owners[-1]

                    def copied(copy: bpy.types.NodeTree) -> Outcome[None]:
                        capture.callback(bpy.data.node_groups.remove, copy)
                        if isinstance(owner, bpy.types.SceneCompositorEffect):
                            return _Authored.of(owner).bind(lambda before: msgspec.structs.replace(before, group=copy).assigned()).bind(lambda _: attached(copy))
                        return group_set(owner, copy).bind(lambda _: attached(copy))

                    return Option.of_optional(tree.copy()).to_result_with(partial(Fault.of, Reason.FAILED, f"{tree!r}.copy")).bind(copied)

                return self.isolated(capture).bind(lambda _: self.resolved(target[0], target[1][0])).bind(selected).bind(lambda _: called(bpy.ops.render.render, {"write_still": False}))

        def body(targets: tuple[tuple[bpy.types.SceneCompositorEffect, StageTarget], ...], camera: CameraFrame, rasters: tuple[Raster, Raster]) -> Outcome[None]:
            head, *tail = targets

            def appended(completed: Outcome[tuple[_Authored, ...]], held: tuple[bpy.types.SceneCompositorEffect, StageTarget]) -> Outcome[tuple[_Authored, ...]]:
                return completed.bind(lambda ready: prefixed(held, camera, demand=False).bind(lambda _: self.stage(held, path, rasters[1])).map(lambda entry: (*ready, entry)))

            first: Outcome[tuple[_Authored, ...]] = (
                prefixed(head, camera, demand=True)
                .bind(lambda _: accumulated(self.captured(source, frame) for source in self.sources))
                .bind(lambda _: self.stage(head, path, rasters[1]))
                .map(lambda entry: (entry,))
            )
            return first.bind(lambda ready: engined(self.sources, engine, lambda: reduce(appended, tail, Ok(ready))) if tail else Ok(ready)).map(
                lambda completed: self.prepared.update({frame: tuple({held.entry: held for held in completed}.values())})
            )

        def started(_: None) -> Outcome[None]:
            self.scene.frame_set(frame, subframe=subframe)

            def selected(targets: tuple[tuple[bpy.types.SceneCompositorEffect, StageTarget], ...]) -> Outcome[None]:
                if not targets:
                    with ExitStack() as capture:
                        for source in self.sources:
                            capture.enter_context(intermediate(self.identity, source))
                            capture.callback(setattr, source.render, "use_compositing", source.render.use_compositing)
                            source.render.use_compositing = False
                        for source in self.sources:
                            capture.callback(setattr, source.render, "filepath", source.render.filepath)
                            source.render.filepath = str(self.pass_path(source))
                            if (
                                result := capture
                                .enter_context(
                                    msgspec.structs.replace(self.format, media_type="MULTI_LAYER_IMAGE", file_format="OPEN_EXR_MULTILAYER", use_exr_interleave=False).held(source.render.image_settings)
                                )
                                .bind(lambda _: native(self.identity, source, RenderCall(write_still=True)))
                                .bind(lambda _: self.captured(source, frame))
                            ).is_error():
                                return Error(result.error)
                    self.prepared[frame] = original
                    return Ok(None)
                reference = self.rasters.map(Ok).default_with(lambda: FrameSize().bound(self.identity).bind(lambda framed: framed.rasters(self.scene)))
                return (
                    self
                    .copied(self.scope)
                    .bind(lambda _: paired(CameraFrame.of(self.identity, self.scene), reference))
                    .bind(
                        lambda held: body(targets, msgspec.structs.replace(held[0], aspect=float(held[1][0].step[0] / held[1][0].step[1]), scale=1.0) if self.rasters.is_some() else held[0], held[1])
                    )
                )

            return (
                accumulated(self.active(target).map(lambda enabled: Some(target) if enabled else Nothing) for target in staged)
                .map(lambda found: tuple(target for active in found for target in active.to_list()))
                .bind(selected)
                if call.animation
                else selected(staged)
            )

        return self.restored(original).bind(started)

    def rendered(self, call: RenderCall, staged: tuple[tuple[bpy.types.SceneCompositorEffect, StageTarget], ...]) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
        """Prepares every frame, then renders natively with the replay engine writing the captured passes and each frame assigning its prepared groups."""
        scene = self.scene
        frames = tuple(range(call.frame_start.default_value(scene.frame_start), call.frame_end.default_value(scene.frame_end) + 1, scene.frame_step)) if call.animation else (scene.frame_current,)
        at, subframe = scene.frame_current, scene.frame_subframe
        sample = 0.0 if call.animation else subframe
        original = accumulated(_Authored.of(entry) for entry in dict.fromkeys(target[0] for target in staged))

        def completed() -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
            def framed(_: None) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
                scene.frame_set(frames[0], subframe=sample)
                return native(self.identity, scene, call)

            return self.scope.enter_context(Handler(handlers=bpy.app.handlers.frame_change_pre, body=self.selected).held(self.identity)).bind(framed)

        def prepared(authored: tuple[_Authored, ...]) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
            self.scope.callback(self.identity.reports.reported(self.restored), authored)
            with Engine.replayed(self.identity, "staged_render", lambda source, layer: self.replay.get((scene.frame_current, source.session_uid, layer.name), ())) as engine:
                return engine.bind(
                    lambda name: reduce(lambda done, frame: done.bind(lambda _: self.frame(frame, sample, name, authored, staged, call)), frames, Ok(None)).bind(
                        lambda _: engined(self.sources, name, completed)
                    )
                )

        with self.scope:
            executing = EXECUTING[self.identity]
            for source in self.sources:
                executing.add(source.session_uid)
                self.scope.callback(executing.remove, source.session_uid)
            self.scope.callback(scene.frame_set, at, subframe=subframe)
            self.scope.callback(self.identity.reports.reported(self.removed))
            return original.bind(prepared)


class StagedRender(msgspec.Struct, frozen=True, kw_only=True):
    """Renders through the CPU step of each render-enabled entry an effect of `rows` owns, each step over the image the entries before it render, natively while compositing is off or no entry stages.

    Each step's image enters its entry's group in place of the entry input, measured values enter its inputs, and `precision` bounds each estimate a step samples.
    """

    rows: OfferedEffects
    stacks: Option[Callable[[Identity], Outcome[LayerStackService]]] = Nothing
    precision: Precision = Precision()

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Rows the render reads entries through, the scenes it hides from completed-image consumers, and the productions it records."""
        return (self.rows, INTERMEDIATES, RENDER_ORIGINS)

    @staticmethod
    def acquisitions(_identity: Identity, /) -> Acquisitions:
        """Acquires nothing of its own, its prerequisites holding what it reads."""
        return ()

    def rendered(self, identity: Identity, scene: bpy.types.Scene, call: RenderCall, *, rasters: Option[tuple[Raster, Raster]] = Nothing) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
        """Renders a scene through its staged entries, else natively."""

        def targets(stacks: Option[LayerStackService]) -> Outcome[tuple[tuple[bpy.types.SceneCompositorEffect, StageTarget], ...]]:
            direct = {held.entry: held.row for held in self.rows.owned(identity, scene)}

            def entry(held: bpy.types.SceneCompositorEffect) -> Outcome[tuple[tuple[bpy.types.SceneCompositorEffect, StageTarget], ...]]:
                return (
                    Option
                    .of_optional(direct.get(held))
                    .map(
                        lambda row: row.read(identity, held).map(
                            lambda record: tuple[tuple[bpy.types.SceneCompositorEffect, StageTarget], ...](
                                (held, ((), row, record, prepare, dict[tuple[bpy.types.CompositorNodeGroup, ...], tuple[Key, ...]]()))
                                for prepare in Option.of_optional(record.record.prepared).to_list()
                            )
                        )
                    )
                    .default_with(
                        lambda: stacks.map(
                            lambda stack: stacked_by(identity, held.node_group).map(lambda tree: stack.stages(tree).map(lambda found: tuple((held, target) for target in found))).default_value(Ok(()))
                        ).default_value(Ok(()))
                    )
                )

            return accumulated(entry(held) for held in scene.compositor_effects if held.enable_for_render).map(lambda found: tuple(target for entries in found for target in entries))

        def rendered(staged: tuple[tuple[bpy.types.SceneCompositorEffect, StageTarget], ...]) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
            job = _Job(identity, scene, self.precision, rasters)
            return (
                job.rendered(call, staged)
                if call.animation and staged
                else accumulated(job.active(target).map(lambda enabled: Some(target) if enabled else Nothing) for target in staged)
                .map(lambda found: tuple(target for active in found for target in active.to_list()))
                .bind(lambda selected: job.rendered(call, selected) if selected else native(identity, scene, call))
            )

        return self.stacks.map(lambda bind: bind(identity).map(Some)).default_value(Ok(Nothing)).bind(targets).bind(rendered) if scene.render.use_compositing else native(identity, scene, call)


# --- [OPERATIONS] -----------------------------------------------------------------------


def captured_input(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, original: bpy.types.NodeTree, space: str) -> Outcome[ImageFile]:
    """Renders the entry input of the original group through isolated groups into an owned EXR in its captured encoding."""
    with ExitStack() as selected, TemporaryDirectory() as directory, bpy.context.temp_override(scene=scene):
        entries = tuple(scene.compositor_effects)
        selected_at = entries.index(entry)
        for position, held in enumerate(entries):
            selected.callback(setattr, held, "enable_for_render", held.enable_for_render)
            held.enable_for_render = held.enable_for_preview if position < selected_at else held == entry
        job = _Job(identity, scene, Precision())
        scope = selected.enter_context(job.scope)
        for rendered in job.sources:
            executing = EXECUTING[identity]
            executing.add(rendered.session_uid)
            scope.callback(executing.remove, rendered.session_uid)
            scope.enter_context(intermediate(identity, rendered))
        path = Path(directory, "entering.exr")

        def captured(copies: Mapping[bpy.types.NodeTree, bpy.types.CompositorNodeTree]) -> Outcome[None]:
            def output(tree: bpy.types.NodeTree, socket: bpy.types.NodeSocket, target: bpy.types.Node) -> Outcome[None]:
                return (
                    FileItems(items={path.stem: FileItem(name=path.stem, socket_type="RGBA")})
                    .write(target, identity)
                    .bind(lambda _: assigned(target, member_paths({Pathed.directory.path: str(path.parent), Pathed.file_name.path: "", **job.format.members(("format",))})))
                    .bind(lambda _: socket_at(target.inputs, 0))
                    .bind(lambda inlet: linked_sockets(tree, socket, inlet))
                    .map(lambda _: None)
                )

            tree = copies[original]
            return OwnedNodes.of(tree, identity).socket(INPUTS, COLOR_INPUT.key, output=True).bind(lambda socket: created(tree, bpy.types.CompositorNodeOutputFile).bind(partial(output, tree, socket)))

        def read(_: object) -> Outcome[ImageFile]:
            image = OpenImageIO.ImageBuf(str(path))
            return pixels(image).map(lambda values: ImageFile(pixels=values, colorspace=space, header=image.spec()))

        return job.isolated(scope).bind(captured).bind(lambda _: called(bpy.ops.render.render, {"write_still": False})).bind(read)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["StagedRender", "captured_input"]
