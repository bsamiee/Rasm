"""Staged CPU renders: each staged compositor entry's CPU step over the image its prefix renders, its result entering the entry's group, the captured passes replayed through a postprocess engine, then Blender's native render over the prepared groups."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from functools import cache, partial, reduce
from itertools import starmap
from pathlib import Path
from typing import Final, TYPE_CHECKING

import bpy
from expression import Error, Ok, Option, Some
from expression.collections import Block
import msgspec
import numpy as np
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.data.ids.images import loaded, Tag
from blender.bpy.data.ids.ownership import Mark, Saved
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.desired import member_paths
from blender.bpy.nodes.graphs import Expr, group_input
from blender.bpy.render.compositor.colors import COLOR_INPUT
from blender.bpy.render.compositor.effects import Entry, Prepared, Stage
from blender.bpy.render.compositor.entries import EffectRows, Owned
from blender.bpy.render.compositor.inputs import CameraFrame, captured, entered, inputs_of, valued
from blender.bpy.render.compositor.placement import EXECUTING
from blender.bpy.render.compositor.viewers import capturing
from blender.bpy.render.outputs.formats import Colorspace, FileItem, FileItems, OutputFormat
from blender.bpy.render.outputs.framing import Border, Framing
from blender.bpy.render.outputs.paths import Pathed
from blender.bpy.render.outputs.results import intermediate, INTERMEDIATES, PRODUCTIONS, saved
from blender.bpy.render.sessions.engines import Engine, Tile
from blender.bpy.render.sessions.renders import engined, native, RenderCall, sourced
from blender.bpy.render.view_layers.passes import used
from blender.imaging.files.buffers import located, pixels
from blender.imaging.files.images import emitted, Subimage
from blender.imaging.files.multilayer import Layer, Multilayer
from blender.imaging.sampling.integration import Estimate, Precision
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case
from blender.records.stores import removed

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Step[R: Case] = Callable[[Stage[R]], Outcome[Option[Prepared]]]
type _Returned = frozenset[rna_enums.OperatorReturnItems]
type _Authored = tuple[bpy.types.SceneCompositorEffect, bpy.types.NodeTree | None, Snapshot]

# --- [CONSTANTS] ------------------------------------------------------------------------

_CAPTURE: Final[Key] = "capture_file"
_REPLAY: Final[Key] = "staged_render"

# --- [SERVICES] -------------------------------------------------------------------------


class _Job:
    """Staged render of one scene owning its capture files, replay tiles, prepared groups, and temporary data until its scope exits."""

    __slots__ = ("config", "files", "identity", "precision", "prepared", "replay", "scene", "scope", "sources")

    def __init__(self, identity: Identity, scene: bpy.types.Scene, precision: Precision) -> None:
        """Holds the current OCIO config and the scenes the render-enabled groups read."""
        self.identity, self.scene, self.precision = identity, scene, precision
        self.config = PyOpenColorIO.GetCurrentConfig()
        self.scope = ExitStack()
        self.replay: dict[tuple[int, int, str], tuple[Tile, ...]] = {}
        self.prepared: dict[int, tuple[_Authored, ...]] = {}
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
                        copied(child).bind(lambda group: written(live, ("node_tree",), group))
                        for live in result.nodes
                        if isinstance(live, bpy.types.CompositorNodeGroup) and isinstance(child := live.node_tree, bpy.types.CompositorNodeTree)
                    ).map(lambda _: result)
                case _:
                    return Error(Fault.of(Reason.ABSENT, repr(tree), member="copy"))

        def swapped(authored: _Authored, group: bpy.types.CompositorNodeTree) -> Outcome[None]:
            scope.callback(self.identity.reports.reported(self.restored), (authored,))
            return _assigned(authored[0], group, authored[2])

        return accumulated(
            paired(_authored(entry), copied(tree)).bind(lambda pair: swapped(*pair))
            for source in self.sources
            for entry in source.compositor_effects
            if entry.enable_for_render and isinstance(tree := entry.node_group, bpy.types.CompositorNodeTree)
        ).map(lambda _: None)

    def raster(self, frame: int) -> tuple[str, Path, OpenImageIO.ImageBuf]:
        """Returns capture name, EXR path, and an empty buffer of the captured raster, the border window alone while the render crops to it."""
        framing = Framing.of(self.scene.render)
        (width, height), window = framing.bounds
        name = f"{self.scene.session_uid}_{frame}_entering"
        match framing.extent:
            case Border(crop=True):
                spec = OpenImageIO.ImageSpec(window.width, window.height, 4, OpenImageIO.FLOAT)
                spec.x, spec.y = window.xbegin, window.ybegin
            case _:
                spec = OpenImageIO.ImageSpec(width, height, 4, OpenImageIO.FLOAT)
        spec.full_x, spec.full_y, spec.full_width, spec.full_height = 0, 0, width, height
        return name, Path(bpy.app.tempdir, f"{name}.exr"), OpenImageIO.ImageBuf(spec)

    def captured(self, source: bpy.types.Scene, frame: int) -> Outcome[None]:
        """Saves the source render result as multilayer EXR and holds its tiles for replay."""

        def replayed(path: str) -> Outcome[None]:
            self.files.add(Path(path))
            return (
                Multilayer
                .read(Path(path))
                .bind(lambda file: Tile.replayed(file, source))
                .map(lambda tiles: self.replay.update({(frame, source.session_uid, layer): held for layer, held in tiles.items()}))
            )

        return saved(source).bind(replayed)

    def output(self, name: str, spec: OpenImageIO.ImageSpec, *, demand: bool) -> tuple[Expr, ...]:
        """Builds capture roots writing the entering image at spec size, adding source Render Layers on `demand`."""
        domain = Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Size", (spec.width, spec.height)),), socket="Image", shape=bpy.types.NodeSocketColor)
        raster = Expr(
            kind=bpy.types.ShaderNodeMixRGB,
            inputs=(("Fac", 1.0), ("Color1", domain), ("Color2", group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor))),
            socket="Color",
            shape=bpy.types.NodeSocketColor,
        )
        content = FileItems(items=(FileItem(name=name, socket_type="RGBA"),))
        capture = Expr(
            kind=bpy.types.CompositorNodeOutputFile,
            inputs=tuple((stored.name, raster) for stored in content.items),
            members={Pathed.directory.path: bpy.app.tempdir, Pathed.file_name.path: ""},
            content=Some(content),
        ).named(_CAPTURE)
        layers = (
            Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(source), "layer": layer.name}, socket="Image", shape=bpy.types.NodeSocketColor)
            for source in (self.sources if demand else ())
            for layer in used(source)
        )
        return (capture, *layers)

    def formatted(self, tree: bpy.types.NodeTree) -> Outcome[None]:
        """Sets the capture output to 32-bit interleaved multiview scene-linear EXR."""
        captured = OutputFormat(
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
        return (
            Block
            .of_seq(tree.nodes)
            .filter(lambda live: isinstance(live, bpy.types.CompositorNodeOutputFile) and Mark.read(self.identity, live).filter(lambda mark: mark.key == _CAPTURE).is_some())
            .try_head()
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(tree.nodes), member=_CAPTURE))
            .bind(lambda capture: assigned(capture, member_paths(captured.members(("format",)))))
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
            return (assigned(live, {("use_multiview",): True, ("views_format",): "INDIVIDUAL"}) if headed else Ok(None)).map(lambda _: live)

        return emitted(path, parts).bind(lambda held: loaded(str(held), Tag(space=Some(self.config.getColorSpace(PyOpenColorIO.ROLE_DATA).getName()), alpha=Some("PREMUL")))).bind(tagged)

    def stage[R: Case](self, held: Owned[R], step: _Step[R], path: Path, like: OpenImageIO.ImageBuf) -> Outcome[_Authored]:
        """Runs the entry's CPU step per view over the captured pixels, its evaluated input values, and its camera frame."""
        with bpy.context.temp_override(scene=self.scene):
            evaluated = self.scene.evaluated_get(bpy.context.evaluated_depsgraph_get())
        reading = evaluated.compositor_effects[held.entry.name]

        def views(file: Multilayer, record: Entry[R], values: Mapping[Key, Value], frame: CameraFrame) -> Outcome[_Authored]:
            def view(layer: Layer) -> Outcome[tuple[str, Option[Prepared]]]:
                return (
                    file
                    .plane(layer, layer.channels)
                    .bind(lambda plane: located(plane, like))
                    .bind(
                        lambda image: step(Stage(record=record.record, identity=self.identity, values=values, config=self.config, image=image, frame=frame, scene=evaluated, precision=self.precision))
                    )
                    .map(lambda value: (layer.name, value))
                )

            return accumulated(map(view, file.layers)).bind(lambda found: self.published(held, record, found))

        return paired(Multilayer.read(path), held.row.read(self.identity, held.entry), captured(self.identity, reading), CameraFrame.read(self.identity, reading)).bind(
            lambda parts: views(parts[0], parts[1], valued(parts[2]), parts[3])
        )

    def estimated(self, entry: bpy.types.SceneCompositorEffect, view: str, value: OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]) -> OpenImageIO.ImageBuf:
        """Returns a prepared image, logging an estimate's largest replicate standard error."""
        match value:
            case Estimate(value=image, standard_error=error):
                self.identity.reports.logger.info("Staged estimate", entry=entry.name, view=view, standard_error=float(np.max(error, initial=0.0)))
                return image
            case _:
                return value

    def published[R: Case](self, held: Owned[R], record: Entry[R], views: tuple[tuple[str, Option[Prepared]], ...]) -> Outcome[_Authored]:
        """Enters measured values into the entry, or builds and assigns its job-owned group over the prepared image, answering the group and inputs each frame assigns."""
        entry, row = held.entry, held.row
        results = tuple((name, value) for name, found in views for value in found.to_list())
        images = tuple((name, self.estimated(entry, name, value)) for name, value in results if not isinstance(value, Mapping))
        measured = tuple(value for _, value in results if isinstance(value, Mapping))

        def owned(group: bpy.types.NodeTree) -> bpy.types.NodeTree:
            self.scope.callback(bpy.data.node_groups.remove, group)
            return group

        def grouped(image: bpy.types.Image) -> Outcome[bpy.types.NodeTree | None]:
            with bpy.context.temp_override(scene=self.scene):
                return (
                    row
                    .grouped(self.identity, self.scene, record, self.config, entry=Some(entry), staged=Some(IdName.of(image)))
                    .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
                    .bind(lambda group: made(group, self.identity, of=Some(self.scene)))
                    .map(owned)
                )

        def placed(before: _Authored, tree: bpy.types.NodeTree | None) -> Outcome[_Authored]:
            return (
                _assigned(entry, tree, before[2])
                .bind(lambda _: Block.of_seq(measured).try_head().map(lambda values: entered(self.identity, entry, values)).default_value(Ok(False)))
                .bind(lambda _: _authored(entry))
            )

        return paired(_authored(entry), self.image(entry, images).bind(grouped) if images else Ok(entry.node_group)).bind(lambda pair: placed(*pair))

    # --- [LIFETIME]

    @staticmethod
    def restored(groups: tuple[_Authored, ...]) -> Outcome[None]:
        """Assigns entry groups and inputs."""
        return accumulated(starmap(_assigned, groups)).map(lambda _: None)

    def selected(self, scene: bpy.types.Scene) -> Outcome[None]:
        """Assigns the current frame's prepared groups and inputs from `frame_change_pre`."""
        return self.restored(self.prepared.get(self.scene.frame_current, ())) if scene.session_uid == self.scene.session_uid else Ok(None)

    def removed(self) -> Outcome[None]:
        """Deletes capture and prepared files."""
        return accumulated(map(removed, self.files)).map(lambda _: None)

    def frame[R: Case](self, frame: int, subframe: float, engine: str, original: tuple[_Authored, ...], staged: tuple[tuple[Owned[R], _Step[R]], ...]) -> Outcome[None]:
        """Captures the frame render once under the authored groups and engines, then prepares each staged entry from its replayed prefix."""
        name, path, like = self.raster(frame)
        self.files.add(path)

        def prefixed(held: Owned[R], *, demand: bool) -> Outcome[_Returned]:
            with ExitStack() as capture:
                for source in self.sources:
                    capture.enter_context(intermediate(self.identity, source))
                return (
                    self
                    .isolated(capture)
                    .bind(lambda _: capture.enter_context(capturing(self.identity, self.scene, held.entry, self.output(name, like.spec(), demand=demand), stop=True)))
                    .bind(lambda entry: Option.of_optional(entry.node_group).to_result_with(partial(Fault.of, Reason.ABSENT, repr(entry), member="node_group")))
                    .bind(self.formatted)
                    .bind(lambda _: called(bpy.ops.render.render, {"scene": self.scene.name, "write_still": False}))
                )

        def appended(completed: Outcome[tuple[_Authored, ...]], pair: tuple[Owned[R], _Step[R]]) -> Outcome[tuple[_Authored, ...]]:
            return completed.bind(lambda ready: prefixed(pair[0], demand=False).bind(lambda _: self.stage(*pair, path, like)).map(lambda entry: (*ready, entry)))

        (head, step), *tail = staged

        def body() -> Outcome[None]:
            first: Outcome[tuple[_Authored, ...]] = (
                prefixed(head, demand=True)
                .bind(lambda _: accumulated(self.captured(source, frame) for source in self.sources))
                .bind(lambda _: self.stage(head, step, path, like))
                .map(lambda entry: (entry,))
            )
            return reduce(appended, tail, first).map(lambda completed: self.prepared.update({frame: completed}))

        def started(_: None) -> Outcome[None]:
            self.scene.frame_set(frame, subframe=subframe)
            return engined(self.sources, engine, body)

        return self.restored(original).bind(started)

    def rendered[R: Case](self, call: RenderCall, staged: tuple[tuple[Owned[R], _Step[R]], ...]) -> Outcome[_Returned]:
        """Prepares every frame, then renders natively with the replay engine writing the captured passes and each frame assigning its prepared groups."""
        scene = self.scene
        frames = tuple(range(call.frame_start.default_value(scene.frame_start), call.frame_end.default_value(scene.frame_end) + 1, scene.frame_step)) if call.animation else (scene.frame_current,)
        at, subframe = scene.frame_current, scene.frame_subframe
        sample = 0.0 if call.animation else subframe
        original = accumulated(_authored(held.entry) for held, _ in staged)

        def completed() -> Outcome[_Returned]:
            self.scope.enter_context(Handler(handlers=bpy.app.handlers.frame_change_pre, body=self.selected).held(self.identity))
            scene.frame_set(frames[0], subframe=sample)
            return native(self.identity, scene, call)

        def prepared(authored: tuple[_Authored, ...]) -> Outcome[_Returned]:
            self.scope.callback(self.identity.reports.reported(self.restored), authored)
            with Engine.replayed(self.identity, _REPLAY, lambda source, layer: self.replay.get((scene.frame_current, source.session_uid, layer.name), ())) as engine:
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
            self.scope.callback(self.identity.reports.reported(self.removed))
            return original.bind(prepared)


class StagedRender[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Renders through the CPU step of each render-enabled entry an effect of `rows` owns, each step over the image the entries before it render, natively while compositing is off or no entry stages, registered as a row beside the rows rendering through it.

    Each step's image enters its entry's group in place of the entry input, measured values enter its inputs, and `precision` bounds each estimate a step samples.
    """

    rows: EffectRows[R]
    precision: Precision = Precision()

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Rows the render reads entries through, the scenes it hides from completed-image consumers, and the productions it records."""
        return (self.rows, INTERMEDIATES, PRODUCTIONS)

    @staticmethod
    def acquisitions(_identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires nothing of its own, its prerequisites holding what it reads."""
        return Ok(())

    def rendered(self, identity: Identity, scene: bpy.types.Scene, call: RenderCall) -> Outcome[_Returned]:
        """Renders a scene through its staged entries, else natively."""
        staged = tuple((held, step) for held in self.rows.owned(identity, scene) if held.entry.enable_for_render for step in held.row.prepared.to_list()) if scene.render.use_compositing else ()
        return _Job(identity, scene, self.precision).rendered(call, staged) if staged else native(identity, scene, call)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _authored(entry: bpy.types.SceneCompositorEffect) -> Outcome[_Authored]:
    """Returns an entry with its group and a snapshot of its inputs, the state `_assigned` puts back."""
    return inputs_of(entry).bind(Snapshot.capture).map(lambda inputs: (entry, entry.node_group, inputs))


def _assigned(entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree | None, inputs: Snapshot) -> Outcome[None]:
    """Points an entry at a group, then applies its input snapshot."""
    return written(entry, ("node_group",), group).bind(lambda _: inputs_of(entry)).bind(lambda found: inputs.apply(found).outcome)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["StagedRender"]
