"""Runtime preview scenes: the owned surfaces, camera, and key light every preview render reuses, the lighting staging them, their compositor stacks, and readback."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from enum import auto, Enum, StrEnum
from functools import partial
import math
from pathlib import Path
from types import MappingProxyType
from typing import ClassVar, Final, Self

import bmesh
import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
from mathutils import Vector
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import filled, loaded, Tag
from blender.bpy.data.ids.ownership import kept, Ownership, Saved
from blender.bpy.data.ids.store import Handle, IdName
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.paths import rna, RnaPath, struct_at
from blender.bpy.data.rna.properties import choices
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes import functions
from blender.bpy.nodes.changes import made, reconciled
from blender.bpy.nodes.desired import Graph
from blender.bpy.nodes.graphs import Expr, group_input, Term
from blender.bpy.nodes.interfaces import Interface, Socket
from blender.bpy.nodes.trees import DATA_TYPES, nested, tree_of
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.compositor.entries import added_entry, Enabled
from blender.bpy.render.compositor.inputs import inputs_of
from blender.bpy.render.outputs.formats import Colorspace, FileItem, FileItems, OutputFormat
from blender.bpy.render.outputs.framing import RESOLUTION
from blender.bpy.render.outputs.paths import Pathed
from blender.bpy.render.outputs.results import intermediate, INTERMEDIATES, RENDER_ORIGINS
from blender.bpy.render.sessions.engines import ENGINE_SAMPLES
from blender.bpy.render.sessions.renders import native, reads_render, Render, RenderCall
from blender.bpy.render.sessions.staging import StagedRender
from blender.bpy.scene.cameras import framed
from blender.bpy.scene.objects import corners, owned_object
from blender.bpy.scene.worlds import shaders, studio
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.multilayer import Multilayer
from blender.imaging.geometry.frames import Raster, Region
from blender.records.faults import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason
from blender.records.fields import Case, NonNegative
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------


class PreviewMesh(Enum):
    """Shader preview surface, named after the material preview type the host offers."""

    SPHERE = auto()
    CUBE = auto()
    FLAT = auto()

    def filled(self, mesh: bpy.types.Mesh) -> Outcome[bpy.types.Mesh]:
        """Fills a mesh with the surface's UV-mapped geometry and shading."""
        with ExitStack() as release:
            surface = bmesh.new()
            release.callback(surface.free)
            surface.loops.layers.uv.new("UVMap")
            match self:
                case PreviewMesh.FLAT:
                    bmesh.ops.create_grid(surface, x_segments=1, y_segments=1, size=1.0, calc_uvs=True)
                case PreviewMesh.SPHERE:
                    bmesh.ops.create_uvsphere(surface, u_segments=48, v_segments=24, radius=0.92, calc_uvs=True)
                case PreviewMesh.CUBE:
                    bmesh.ops.create_cube(surface, size=1.0, calc_uvs=True)
            surface.to_mesh(mesh)
        (mesh.shade_smooth if self is PreviewMesh.SPHERE else mesh.shade_flat)()
        return Ok(mesh)

    @property
    def rest(self) -> Mapping[RnaPath, Value]:
        """Members of the surface's object between renders: hidden at the origin, turned and scaled to fill the frame."""
        match self:
            case PreviewMesh.SPHERE:
                rotation, scale = (0.0, 0.0, 0.0), (1.0, 1.0, 1.0)
            case PreviewMesh.CUBE:
                rotation, scale = (math.pi / 4.0, math.atan(math.sqrt(0.5)), 0.0), (0.98, 0.98, 0.98)
            case PreviewMesh.FLAT:
                rotation, scale = (0.0, 0.0, 0.0), (1.04, 1.04, 1.0)
        return {("location",): (0.0, 0.0, 0.0), ("rotation_euler",): rotation, ("scale",): scale, ("hide_render",): True}


class CameraPose(Enum):
    """Fixed preview camera: orthographic above the surface, panoramic at the world origin, or perspective from the front into a world volume."""

    TOP = auto()
    PANORAMA = auto()
    FRONT = auto()


class _Capture(StrEnum):
    """File Output item of a preview render, each value its file name part."""

    IMAGE = auto()
    VALUE = auto()
    COVERAGE = auto()


type Ambient = White | Studio
type Restoring = Callable[[], Outcome[None]]

# --- [CONSTANTS] ------------------------------------------------------------------------

CAMERAS: Final[Mapping[tuple[CameraPose, str | None], tuple[Mapping[RnaPath, Value], Mapping[RnaPath, Value]]]] = MappingProxyType({
    (CameraPose.TOP, None): ({("type",): "ORTHO", ("ortho_scale",): 2.0}, {("location",): (0.0, 0.0, 2.0), ("rotation_euler",): (0.0, 0.0, 0.0)}),
    (CameraPose.PANORAMA, "CYCLES"): ({("type",): "PANO", ("panorama_type",): "EQUIRECTANGULAR"}, {("location",): (0.0, 0.0, 0.0), ("rotation_euler",): (math.pi / 2.0, 0.0, 0.0)}),
    (CameraPose.PANORAMA, None): ({("type",): "PERSP", ("lens",): 12.0}, {("location",): (0.0, 0.0, 0.0), ("rotation_euler",): (1.3, 0.0, 0.0)}),
    (CameraPose.FRONT, None): ({("type",): "PERSP", ("lens",): 45.0}, {("location",): (0.0, -4.0, 0.0), ("rotation_euler",): (math.pi / 2.0, 0.0, 0.0)}),
})
_SURFACE_TYPE: Final[bpy.types.EnumProperty] = bpy.types.Material.bl_rna.properties["preview_render_type"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
SURFACES: Final = choices(item for item in _SURFACE_TYPE.enum_items if item.identifier in PreviewMesh.__members__)
_FIXED: Final[Mapping[RnaPath, Value]] = MappingProxyType({("location",): (0.0, 0.0, 0.0), ("rotation_euler",): (0.0, 0.0, 0.0), ("scale",): (1.0, 1.0, 1.0), ("hide_render",): False})

# --- [MODELS] ---------------------------------------------------------------------------


class White(Case, frozen=True):
    """Uniform white ambient light."""


class Studio(Case, frozen=True, kw_only=True):
    """Blender studio light environment."""

    name: str = "forest.exr"


class PreviewLighting(msgspec.Struct, frozen=True, kw_only=True):
    """Preview surface, ambient, its strength, and the key sun's strength and rotation."""

    surface: PreviewMesh = PreviewMesh.SPHERE
    ambient: Ambient = Studio()
    strength: NonNegative = 1.0
    key: NonNegative = 2.0
    aim: tuple[float, float, float] = (0.9, 0.15, 0.5)

    def prepared(self) -> Outcome[tuple[Graph, tuple[bpy.types.Image, ...]]]:
        """Builds the world graph over its actual studio image or flat white backdrop."""

        def graphed(color: Term, images: tuple[bpy.types.Image, ...]) -> Outcome[tuple[Graph, tuple[bpy.types.Image, ...]]]:
            return shaders(Expr(kind=bpy.types.ShaderNodeBackground, inputs=(("Color", color), ("Strength", self.strength)), socket="Background")).map(lambda graph: (graph, images))

        match self.ambient:
            case White():
                return graphed((1.0, 1.0, 1.0, 1.0), ())
            case Studio(name=name):
                preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                return (
                    studio(name, preferences)
                    .bind(lambda path: loaded(path, Tag()))
                    .bind(lambda image: graphed(Expr(kind=bpy.types.ShaderNodeTexEnvironment, members={"image": IdName.of(image)}, socket="Color", socket_type=bpy.types.NodeSocketColor), (image,)))
                )

    @property
    def sun(self) -> Mapping[RnaPath, Value]:
        """Key sun at key strength and aim as light object members."""
        return {("data", "energy"): self.key, ("rotation_euler",): self.aim}


class PreviewObject[D: bpy.types.Mesh | bpy.types.Camera | bpy.types.Light](msgspec.Struct, frozen=True, kw_only=True):
    """Owned preview object over owned data every preview render reuses, the data built by `created` from the session data and filled by `seeded`, the object placed at `rest`."""

    data: Ownership[D]
    created: Callable[[bpy.types.BlendData, str], D]
    rest: Mapping[RnaPath, Value]
    seeded: Callable[[D], Outcome[D]] = Ok

    @property
    def objects(self) -> Ownership[bpy.types.Object]:
        """Ownership of the object, keyed as its data."""
        return Ownership(kind=bpy.types.Object, key=self.data.key, saved=Saved.NEVER)

    def placed(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[tuple[bpy.types.Object, D]]:
        """Returns the owned object and data, the object linked into the scene once and placed at rest."""
        name = bpy.path.display_name(self.data.key)
        return owned_object((self.objects, self.data), name, partial(self.created, bpy.data, name), scene.collection, identity, seeded=self.seeded).bind(
            lambda pair: assigned(pair[0], self.rest).map(lambda _: pair)
        )


class RoutedEntry(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Source effect entry with own or routed group."""

    entry: bpy.types.SceneCompositorEffect
    group: bpy.types.NodeTree

    @classmethod
    def of(cls, source: bpy.types.Scene, enabled: Enabled) -> tuple[Self, ...]:
        """Returns enabled source entries with groups reading no scene render."""
        return tuple(
            cls(entry=entry, group=group)
            for entry in source.compositor_effects
            for group in Option.of_optional(entry.node_group).to_list()
            if enabled.descriptor.read(entry) is True and not any(reads_render(node) for tree in nested((group,)) for node in tree.nodes)
        )


class PreviewStack(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Compositor entries of one preview render: the render input plane, ordered routed entries, and the placement adding entries after them."""

    routed: tuple[RoutedEntry, ...] = ()
    placing: Callable[[Identity, bpy.types.Scene, bpy.types.Scene, Option[tuple[Raster, Raster]]], Outcome[Restoring]] = lambda _identity, _source, _scene, _rasters: Ok(lambda: Ok(None))


class Staging(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Preview scene state of one render."""

    source: bpy.types.Scene
    input: tuple[int, int] | npt.NDArray[np.float32]
    world: Graph | bpy.types.World
    sun: Mapping[RnaPath, Value] | bpy.types.Light
    stack: PreviewStack = PreviewStack()
    transparent: bool = True
    surface: Option[PreviewMesh] = Nothing
    material: Option[bpy.types.Material] = Nothing
    viewpoint: CameraPose | bpy.types.Object = CameraPose.TOP
    value: bool = False
    rasters: Option[tuple[Raster, Raster]] = Nothing

    @property
    def size(self) -> tuple[int, int]:
        """Width and height of the actual input plane or rendered surface."""
        return (np.size(source, axis=1), np.size(source, axis=0)) if isinstance(source := self.input, np.ndarray) else (source[0], source[1])


class Readback(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Associated compositor output in its colorspace with covered value AOV samples."""

    plane: npt.NDArray[np.float32]
    colorspace: str
    values: Option[npt.NDArray[np.float32]] = Nothing
    covered: ClassVar[float] = 0.5

    @property
    def uniform(self) -> Option[float]:
        """Mean of the covered value samples within a thousandth of their magnitude, `Nothing` when they vary or none is covered."""
        return self.values.filter(lambda held: held.size > 0 and float(np.ptp(held)) <= 1e-3 * max(1.0, float(np.abs(held).max()))).map(lambda held: float(held.mean()))

    def formed(self, view: DisplayView, config: PyOpenColorIO.Config) -> Outcome[Self]:
        """Returns the readback with its plane formed through a view into the view's colorspace."""
        return view.image(config, self.plane, self.colorspace).map(lambda image: msgspec.structs.replace(self, plane=image.pixels, colorspace=image.colorspace))


# --- [SERVICES] -------------------------------------------------------------------------


class PreviewScene(msgspec.Struct, frozen=True, kw_only=True):
    """Owned preview scene and reused data its renders stage, each render running through `render`."""

    render: Render | StagedRender = native
    scene: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="preview_scene", saved=Saved.NEVER)
    world: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key="preview_world", saved=Saved.NEVER)
    image: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="preview_image", saved=Saved.NEVER)
    input_group: ClassVar[Ownership[bpy.types.NodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key="preview_input", saved=Saved.NEVER)
    capture: ClassVar[Ownership[bpy.types.NodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key="preview_capture", saved=Saved.NEVER)
    value: ClassVar[Key] = "value"

    # --- [STAGING]

    @staticmethod
    def engine(scene: bpy.types.Scene) -> Outcome[Snapshot]:
        """Captures the copied engine settings with preview samples replacing final samples."""
        samples = Option.of_optional(ENGINE_SAMPLES.get(scene.render.engine))
        captured = accumulated(Descriptor.resolve(bpy.types.Scene, final[:1], bpy.types.Property) for final, _preview in samples.to_list()).bind(
            lambda owners: Snapshot.capture(scene, Some((Descriptor(root=bpy.types.Scene, owner=("render",), prop=bpy.types.RenderSettings.bl_rna.properties["engine"]), *owners)))
        )
        previewed = accumulated(
            Descriptor.resolve(bpy.types.Scene, preview, bpy.types.Property).bind(
                lambda held: struct_at(scene, held.owner).map(lambda owner: (final, held.read(owner))).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(scene), member=rna(held.owner)))
            )
            for final, preview in samples.to_list()
        )

        def effective(snapshot: Snapshot, overrides: tuple[tuple[RnaPath, Value], ...]) -> Snapshot:
            values = dict(overrides)
            return msgspec.structs.replace(
                snapshot,
                tree=msgspec.structs.replace(
                    snapshot.tree,
                    rows=tuple((path, msgspec.structs.replace(owner, rows=tuple((name, values.get((*path, *name), value)) for name, value in owner.rows))) for path, owner in snapshot.tree.rows),
                ),
            )

        return paired(captured, previewed).map(lambda values: effective(*values))

    def input_built(self, identity: Identity, pixels: npt.NDArray[np.float32]) -> Outcome[bpy.types.NodeTree]:
        """Returns the group reading pixels through a runtime image."""
        height, width, _ = pixels.shape
        tag = Tag(space=Some(bpy.data.colorspace.working_space), alpha=Some("PREMUL"))

        def built() -> Outcome[bpy.types.Image]:
            created = catch(bpy.data.images.new, exception=RuntimeError)("Preview Input", width=width, height=height, alpha=True, float_buffer=True)
            return kept(created.map_error(partial(Fault.of, Reason.REFUSED, self.image.key)), lambda image: tag.applied(image).map(lambda _: image))

        def graphed(image: bpy.types.Image) -> Outcome[bpy.types.NodeTree]:
            source = Expr(kind=bpy.types.CompositorNodeImage, members={"image": IdName.of(image)}, socket="Image", socket_type=bpy.types.NodeSocketColor)
            face = Interface(outputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),))
            return (
                catch(image.scale, exception=RuntimeError)(width, height)
                .map_error(partial(Fault.of, Reason.REFUSED, self.image.key))
                .bind(lambda _: filled(image, pixels))
                .bind(lambda _: functions.built(self.input_group.key, face, {"image": source}, bpy.types.CompositorNodeTree, name=Some("Preview Input")))
                .bind(lambda group: made(msgspec.structs.replace(group, saved=Saved.NEVER), identity))
            )

        return self.image.made(identity, built).bind(graphed)

    def stacked(self, identity: Identity, staging: Staging, scene: bpy.types.Scene, release: ExitStack) -> Outcome[Restoring]:
        """Appends input and routed entries with their source inputs, removed when `release` closes, then runs the stack's placement, returning its removal."""
        stack = staging.stack
        width, height = staging.size
        greater = max(width, height)
        rasters = Some(staging.rasters.default_with(lambda: (raster := Raster(bounds=Region(right=width / greater, top=height / greater), shape=(height, width)), raster)))

        def copied(held: bpy.types.SceneCompositorEffect, entry: bpy.types.SceneCompositorEffect) -> Outcome[None]:
            return paired(inputs_of(held).bind(Snapshot.capture), inputs_of(entry)).bind(lambda pair: pair[0].apply(pair[1]).outcome).map(lambda _: None)

        with bpy.context.temp_override(scene=scene):
            return (
                (Some(staging.input) if isinstance(staging.input, np.ndarray) else Nothing)
                .map(lambda pixels: self.input_built(identity, pixels).bind(lambda group: added_entry(release, scene, group)).map(lambda _: None))
                .default_value(Ok(None))
                .bind(lambda _: accumulated(added_entry(release, scene, held.group).bind(lambda entry: copied(held.entry, entry)) for held in stack.routed))
                .bind(lambda _: stack.placing(identity, staging.source, scene, rasters))
            )

    @contextmanager
    def staged(self, identity: Identity, staging: Staging) -> Generator[Outcome[bpy.types.Scene]]:
        """Stages one render, running each release step at exit in reverse with their faults held for the extension."""
        release = ExitStack()

        def undone(request: object, step: Callable[[], object]) -> None:
            release.callback(_released, identity, lambda: catch(step, exception=RuntimeError)().map_error(partial(Fault.of, Reason.REFUSED, repr(request))))

        def lit(light: bpy.types.Object, data: bpy.types.Light) -> Outcome[None]:
            def relit() -> None:
                light.data = data

            undone(light, relit)
            match staging.sun:
                case bpy.types.Light() as held:
                    return written(light, ("data",), held).map(lambda _: None)
                case members:
                    return assigned(light, members)

        def meshed(scene: bpy.types.Scene, shapes: tuple[tuple[PreviewMesh, tuple[bpy.types.Object, bpy.types.Mesh]], ...]) -> Outcome[None]:
            def shown(shape: PreviewMesh, item: bpy.types.Object, mesh: bpy.types.Mesh) -> None:
                def hidden() -> None:
                    item.hide_render = True

                undone(item, hidden)
                undone(mesh, mesh.materials.clear)
                mesh.materials.clear()
                item.hide_render = staging.surface != Some(shape)
                for material in staging.material.filter(lambda _: staging.surface == Some(shape)).to_list():
                    mesh.materials.append(material)

            def linked(subject: bpy.types.Object) -> None:
                scene.collection.objects.link(subject)
                undone(subject, lambda: scene.collection.objects.unlink(subject))

            return accumulated((
                *(catch(shown, exception=RuntimeError)(shape, item, mesh).map_error(partial(Fault.of, Reason.REFUSED, repr(item))) for shape, (item, mesh) in shapes),
                *(
                    catch(linked, exception=RuntimeError)(subject).map_error(partial(Fault.of, Reason.REFUSED, repr(scene)))
                    for subject in (staging.viewpoint,)
                    if isinstance(subject, bpy.types.Object)
                ),
            )).map(lambda _: None)

        def aimed(scene: bpy.types.Scene, camera: bpy.types.Object, data: bpy.types.Camera) -> Outcome[None]:
            match staging.viewpoint:
                case CameraPose() as pose:
                    lens, transform = CAMERAS.get((pose, staging.source.render.engine), CAMERAS[pose, None])
                    start: bpy.types.FloatProperty = bpy.types.Camera.bl_rna.properties["clip_start"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                    end: bpy.types.FloatProperty = bpy.types.Camera.bl_rna.properties["clip_end"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                    return paired(assigned(data, {("clip_start",): start.default, ("clip_end",): end.default, **lens}), assigned(camera, transform)).map(lambda _: None)
                case bpy.types.Object() as subject:
                    rotation = Vector((-0.55, 0.8, -0.5)).to_track_quat("-Z", "Y").to_euler()
                    with bpy.context.temp_override(scene=scene):
                        return paired(assigned(data, {("type",): "ORTHO"}), assigned(camera, {("rotation_euler",): (rotation.x, rotation.y, rotation.z)})).bind(
                            lambda _: framed(camera, corners((subject,), graph := bpy.context.evaluated_depsgraph_get()).astype(np.float32), graph)
                        )

        def environment(scene: bpy.types.Scene) -> Outcome[None]:
            match staging.world:
                case bpy.types.World() as held:
                    return written(scene, ("world",), held).map(lambda _: None)
                case graph:
                    return (
                        self.world
                        .created(identity, bpy.data.worlds.new, "Preview World")
                        .bind(lambda world: tree_of(world).bind(lambda tree: reconciled(graph, tree, identity)).bind(lambda _: written(scene, ("world",), world)))
                        .map(lambda _: None)
                    )

        def arranged(
            scene: bpy.types.Scene,
            pieces: tuple[tuple[tuple[PreviewMesh, tuple[bpy.types.Object, bpy.types.Mesh]], ...], tuple[bpy.types.Object, bpy.types.Camera], tuple[bpy.types.Object, bpy.types.Light]],
        ) -> Outcome[bpy.types.Scene]:
            shapes, (camera, lens), (light, data) = pieces
            return (
                paired(meshed(scene, shapes), written(scene, ("camera",), camera))
                .bind(lambda _: paired(lit(light, data), aimed(scene, camera, lens), environment(scene)))
                .bind(lambda _: self.stacked(identity, staging, scene, release))
                .map(lambda restoring: release.callback(_released, identity, restoring))
                .map(lambda _: scene)
            )

        def prepared(scene: bpy.types.Scene) -> Outcome[bpy.types.Scene]:
            release.enter_context(intermediate(identity, scene))
            source = staging.source
            width, height = staging.size
            minimum = RESOLUTION.prop.hard_min
            settings: dict[RnaPath, Value] = {
                ("render", "resolution_x"): max(width, minimum),
                ("render", "resolution_y"): max(height, minimum),
                ("render", "resolution_percentage"): 100,
                ("render", "fps"): source.render.fps,
                ("render", "fps_base"): source.render.fps_base,
                ("render", "frame_map_old"): source.render.frame_map_old,
                ("render", "frame_map_new"): source.render.frame_map_new,
                ("render", "film_transparent"): staging.transparent,
                ("render", "use_compositing"): True,
                ("render", "use_sequencer"): False,
                ("render", "use_multiview"): False,
                ("render", "use_motion_blur"): False,
                ("render", "use_border"): False,
                ("render", "use_stamp"): False,
            }
            return (
                paired(assigned(scene, settings), self.engine(source).bind(lambda snapshot: snapshot.apply(scene).outcome))
                .map(lambda _: scene.frame_set(source.frame_current, subframe=source.frame_subframe))
                .bind(
                    lambda _: paired(
                        accumulated(_SURFACES[shape].placed(identity, scene).map(lambda pair: (shape, pair)) for shape in PreviewMesh), _CAMERA.placed(identity, scene), _SUN.placed(identity, scene)
                    )
                )
                .bind(lambda pieces: arranged(scene, pieces))
            )

        with release:
            yield self.scene.created(identity, bpy.data.scenes.new, "Preview").bind(prepared)

    # --- [READBACK]

    def rendered(self, identity: Identity, staging: Staging) -> Outcome[Readback]:
        """Renders staging and reads image and value AOV from File Output files."""
        size = staging.size

        def declared(scene: bpy.types.Scene, value: Option[str], output: OutputFormat, *, paths: Mapping[_Capture, Path]) -> Outcome[None]:
            image = group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor)
            domain = Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Size", size),), socket="Image", socket_type=bpy.types.NodeSocketColor, key=Some("domain"))
            passes = value.map(lambda name: {_Capture.VALUE: name, _Capture.COVERAGE: "Alpha"}).default_value({})
            sources: dict[_Capture, Expr] = {
                _Capture.IMAGE: Expr(kind=bpy.types.ShaderNodeMixRGB, inputs=(("Fac", 1.0), ("Color1", domain), ("Color2", image)), socket="Color", socket_type=bpy.types.NodeSocketColor),
                **{key: Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene)}, socket=socket) for key, socket in passes.items()},
            }

            files = Expr(
                kind=bpy.types.CompositorNodeOutputFile,
                inputs=tuple(sources.items()),
                members={Pathed.directory.path: bpy.app.tempdir, Pathed.file_name.path: "", **output.members(("format",))},
                content=Some(FileItems(items={key: FileItem(name=paths[key].stem, socket_type=DATA_TYPES[term.socket_type]) for key, term in sources.items()})),
            )
            return (
                functions
                .built(
                    self.capture.key, Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: image.keeping(files)}, bpy.types.CompositorNodeTree, name=Some("Preview Capture")
                )
                .map(lambda group: msgspec.structs.replace(group, saved=Saved.NEVER))
                .bind(lambda group: made(group, identity))
                .bind(lambda tree: added_entry(release, scene, tree))
                .map(lambda _: None)
            )

        def stored() -> Outcome[OutputFormat]:
            linear = Colorspace(name=bpy.data.colorspace.working_space)
            return OutputFormat.exr(Output.DATA, layered=False).map(lambda held: msgspec.structs.replace(held, color_mode="RGBA", color_management="OVERRIDE", linear_colorspace_settings=linear))

        def covered(values: npt.NDArray[np.float32], coverage: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
            mask = coverage[..., 0] > Readback.covered
            return values[..., 0][mask] / coverage[..., 0][mask]

        def readback(paths: Mapping[_Capture, Path]) -> Outcome[Readback]:
            def read(key: _Capture, channels: tuple[str, ...]) -> Outcome[npt.NDArray[np.float32]]:
                return Multilayer.read(paths[key]).bind(lambda file: file.picked(("",), channels))

            sampled = paired(read(_Capture.VALUE, ("R",)), read(_Capture.COVERAGE, ("R",))).map(lambda pair: Some(covered(*pair))) if staging.value else Ok(Nothing)
            return paired(read(_Capture.IMAGE, ("R", "G", "B", "A")), sampled).map(lambda pair: Readback(plane=pair[0], colorspace=bpy.data.colorspace.working_space, values=pair[1]))

        def captured(scene: bpy.types.Scene) -> Outcome[Readback]:
            paths = {key: Path(bpy.app.tempdir, f"{scene.session_uid}_{self.capture.key}_{key}.exr") for key in _Capture}
            value = Some(identity.identifier(self.value)) if staging.value else Nothing
            release.callback(_released, identity, partial(_unlinked, paths))
            for name in value.to_list():
                aovs = scene.view_layers[0].aovs
                aov = aovs.add()
                release.callback(aovs.remove, aov)
                aov.name, aov.type = name, "VALUE"
            return (
                stored()
                .bind(lambda output: declared(scene, value, output, paths=paths))
                .bind(lambda _: _unlinked(paths))
                .bind(lambda _: self.render.rendered(identity, scene, RenderCall(), rasters=staging.rasters) if isinstance(self.render, StagedRender) else self.render(identity, scene, RenderCall()))
                .bind(lambda _: readback(paths))
            )

        with intermediate(identity, staging.source), ExitStack() as release, self.staged(identity, staging) as staged:
            return staged.bind(captured)

    def sampled(self, identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], pixels: npt.NDArray[np.float32]) -> Outcome[npt.NDArray[np.float32]]:
        """Renders float pixels through entries without adding formation."""
        channels = pixels.shape[2]
        lighting = PreviewLighting()
        stack = PreviewStack(routed=tuple(RoutedEntry(entry=entry, group=group) for entry in entries for group in Option.of_optional(entry.node_group).to_list()))
        return (
            lighting
            .prepared()
            .bind(lambda prepared: self.rendered(identity, Staging(source=scene, input=pixels, world=prepared[0], sun=lighting.sun, stack=stack)))
            .map(lambda readback: readback.plane[..., :channels])
        )

    def sampled_from(self, identity: Identity, source: Handle[bpy.types.Scene], entries: tuple[str, ...], pixels: npt.NDArray[np.float32]) -> Outcome[npt.NDArray[np.float32]]:
        """Resolves a deferred scene and ordered entry names before sampling, retaining missing-source faults."""
        match source.resolve():
            case Result(tag="ok", ok=scene):
                held = {entry.name: entry for entry in scene.compositor_effects}
                return accumulated(Option.of_optional(held.get(name)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=name)) for name in entries).bind(
                    lambda chosen: self.sampled(identity, scene, chosen, pixels)
                )
            case Result(error=faults):
                return Error(faults)

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns owned preview scene, world, input, capture, surfaces, camera, and light."""
        return (
            RENDER_ORIGINS,
            INTERMEDIATES,
            self.scene,
            self.world,
            self.image,
            self.input_group,
            self.capture,
            *(row for piece in _SURFACES.values() for row in (piece.objects, piece.data)),
            _CAMERA.objects,
            _CAMERA.data,
            _SUN.objects,
            _SUN.data,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _released(identity: Identity, step: Callable[[], Outcome[object]]) -> None:
    """Runs one release step, logging its faults."""
    identity.reports.faulted(step())


def _unlinked(paths: Mapping[_Capture, Path]) -> Outcome[None]:
    """Deletes capture files a render writes, absent ones included."""
    return accumulated(catch(path.unlink, exception=OSError)(missing_ok=True).map_error(partial(Fault.of, Reason.UNWRITABLE, str(path))) for path in paths.values()).map(lambda _: None)


# --- [COMPOSITION] ----------------------------------------------------------------------

_SURFACES: Final[Mapping[PreviewMesh, PreviewObject[bpy.types.Mesh]]] = MappingProxyType({
    shape: PreviewObject(
        data=Ownership(kind=bpy.types.Mesh, key=f"preview_{shape.name.lower()}", saved=Saved.NEVER), created=lambda data, name: data.meshes.new(name), seeded=shape.filled, rest=shape.rest
    )
    for shape in PreviewMesh
})
_CAMERA: Final[PreviewObject[bpy.types.Camera]] = PreviewObject(
    data=Ownership(kind=bpy.types.Camera, key="preview_camera", saved=Saved.NEVER), created=lambda data, name: data.cameras.new(name), rest=_FIXED
)
_SUN: Final[PreviewObject[bpy.types.Light]] = PreviewObject(
    data=Ownership(kind=bpy.types.Light, key="preview_sun", saved=Saved.NEVER), created=lambda data, name: data.lights.new(name, type="SUN"), rest=_FIXED
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CAMERAS",
    "SURFACES",
    "Ambient",
    "CameraPose",
    "PreviewLighting",
    "PreviewMesh",
    "PreviewObject",
    "PreviewScene",
    "PreviewStack",
    "Readback",
    "Restoring",
    "RoutedEntry",
    "Staging",
    "Studio",
    "White",
]
