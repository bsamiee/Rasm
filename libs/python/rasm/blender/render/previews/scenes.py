"""Native runtime preview scenes, staged subjects, compositor chains, and readback."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from enum import Enum
from functools import partial
import math
from pathlib import Path
from typing import Annotated, ClassVar, Literal, overload

import bmesh
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
from mathutils import Vector
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm.blender.data.ids.images import filled, Tag
from rasm.blender.data.ids.ownership import Mark, Ownership, Saved
from rasm.blender.data.rna.descriptors import Descriptor, IdName, Value, written
from rasm.blender.data.rna.snapshots import Snapshot
from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult, BlenderWriteFault
from rasm.blender.nodes.graphs import End, ended, FileItem, FileItems, Graph, INPUTS, Interface, Link, Node, node, NodeGroup, Socket
from rasm.blender.render.color.luts import Enabled
from rasm.blender.render.effects.stack import camera_frame, Configured, Context, EffectName, EffectStack, entered, formation, Placeable, viewed
from rasm.blender.render.outputs.exports import plane
from rasm.blender.render.outputs.formats import Kind, OutputFormat
from rasm.blender.render.outputs.framing import FrameSize
from rasm.blender.render.outputs.paths import Pathed
from rasm.blender.render.sessions import execution
from rasm.blender.render.view_layers.passes import synced
from rasm.blender.scene.cameras import framed
from rasm.blender.scene.worlds import Backdrop, Environment, FlatColor, Hdri, studio, studios, swapped, world_graph, WorldEnvironment
from rasm.imaging.chain import Effect
from rasm.imaging.color.processors import DisplayView
from rasm.imaging.compositing.blending import Entry
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Case, Choice, Rgb
from rasm.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------


class PreviewEngine(Enum):
    """Native preview engines and their sample-setting owner and member."""

    BLENDER_EEVEE = "eevee", "taa_render_samples"
    CYCLES = "cycles", "samples"

    @classmethod
    def of(cls, scene: bpy.types.Scene) -> "PreviewEngine":
        """Chooses the source engine where supported, otherwise EEVEE."""
        return cls.__members__.get(scene.render.engine, cls.BLENDER_EEVEE)


class Shape(Choice):
    """Surface receiving a shader preview."""

    SPHERE = 1, "Sphere"
    CUBE = 2, "Cube"
    PLANE = 3, "Plane"


class Rig(Enum):
    """Native camera and key-light pieces shared by scene renders."""

    CAMERA = "camera"
    LAMP = "lamp"


type Piece = Shape | Rig
type Ambient = White | Studio
type View = Top | Framed | Panorama | Interior


# --- [MODELS] ---------------------------------------------------------------------------


class White(Case, frozen=True, tag=1):
    """Uniform white ambient illumination."""

    title = "White"


class Studio(Case, frozen=True, kw_only=True, tag=2):
    """One of Blender's native studio-light environments."""

    title = "Studio Light"
    name: Annotated[str, msgspec.Meta(title="World"), studios] = "forest.exr"


class Lamp(msgspec.Struct, frozen=True, kw_only=True):
    """Key light's energy, native kind, and transform for one staging."""

    energy: float
    kind: Literal["SUN", "POINT"] = "SUN"
    location: tuple[float, float, float] = 0.0, 0.0, 0.0
    rotation: tuple[float, float, float] = 0.9, 0.15, 0.5


class Lighting(msgspec.Struct, frozen=True, kw_only=True):
    """Surface, ambient environment, and key strength for a preview."""

    shape: Shape = Shape.SPHERE
    ambient: Ambient = Studio()
    strength: Annotated[float, msgspec.Meta(ge=0.0, le=10.0, title="World Light")] = 1.0
    key: Annotated[float, msgspec.Meta(ge=0.0, le=20.0, title="Key Light")] = 2.0

    def environment(self, preferences: bpy.types.Preferences) -> BlenderResult[Environment]:
        """Builds the world through the native studio or flat-color integration."""
        match self.ambient:
            case White():
                return Ok(Environment(lighting=Backdrop(mode=FlatColor(color=Rgb(red=1.0, green=1.0, blue=1.0)), strength=self.strength)))
            case Studio(name=name):
                return studio(name, preferences).map(lambda path: Environment(lighting=Backdrop(mode=Hdri(path=path), strength=self.strength)))

    @property
    def lamp(self) -> Lamp:
        """Key sun at the requested energy."""
        return Lamp(energy=self.key)


class Top(msgspec.Struct, frozen=True, kw_only=True):
    """Orthographic camera above the preview surface."""


class Framed(msgspec.Struct, frozen=True, kw_only=True):
    """Orthographic camera fitted to the subject's evaluated bounds."""


class Panorama(msgspec.Struct, frozen=True, kw_only=True):
    """Camera at the world center sampling every direction."""


class Interior(msgspec.Struct, frozen=True, kw_only=True):
    """Perspective view into a world volume."""


class Mirrored(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Source entry repeated through its native or routed group."""

    entry: bpy.types.SceneCompositorEffect
    group: bpy.types.NodeTree
    row: Option[Placeable] = Nothing


class Offered(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Typed effect placed among the mirrored entries at its processing rank."""

    row: Placeable
    record: Entry[Effect]


class Chain(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Input plane, ordered native mirrors, and typed additions for one render."""

    head: Option[npt.NDArray[np.float32]] = Nothing
    mirrored: tuple[Mirrored, ...] = ()
    offered: tuple[Offered, ...] | Mapping[EffectName, Configured[Effect]] = ()
    initial_view: Option[DisplayView] = Nothing


class Staging(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Complete desired preview scene for one native render."""

    source: bpy.types.Scene
    size: tuple[int, int]
    engine: PreviewEngine
    world: Environment | bpy.types.World
    lamp: Lamp | bpy.types.Light
    chain: Chain = Chain()
    transparent: bool = True
    shape: Option[Shape] = Nothing
    material: Option[bpy.types.Material] = Nothing
    subject: Option[bpy.types.Object] = Nothing
    view: View = Top()
    value: bool = False
    context: Context = Nothing


class Readback(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Formed associated display pixels and covered scalar AOV samples."""

    plane: npt.NDArray[np.float32]
    values: Option[npt.NDArray[np.float32]] = Nothing
    covered: ClassVar[float] = 0.5


# --- [SERVICES] -------------------------------------------------------------------------


class PreviewScene(msgspec.Struct, frozen=True, kw_only=True):
    """Owned native preview scene and the reusable data its renders stage."""

    stack: EffectStack
    scene: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="preview_scene", saved=Saved.NEVER)
    world: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key="preview_world", saved=Saved.NEVER)
    image: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="preview_image", saved=Saved.NEVER)
    head: ClassVar[Ownership[bpy.types.NodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key="preview_head", saved=Saved.NEVER)
    capture: ClassVar[Ownership[bpy.types.NodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key="preview_capture", saved=Saved.NEVER)
    routes: ClassVar[Ownership[bpy.types.ID]] = Ownership(kind=bpy.types.ID, key="preview_route", saved=Saved.NEVER, single=False)
    value: ClassVar[Key] = "value"
    samples: ClassVar[int] = 16

    def made(self, identity: Identity) -> BlenderResult[bpy.types.Scene]:
        """Returns this extension's runtime preview scene."""
        return self.scene.made(
            identity, lambda: catch(lambda: bpy.data.scenes.new("Preview"), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=self.scene.key, error=error),))
        )

    @overload
    def piece(self, identity: Identity, piece: Shape, scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.Object, bpy.types.Mesh]]: ...

    @overload
    def piece(self, identity: Identity, piece: Literal[Rig.CAMERA], scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.Object, bpy.types.Camera]]: ...

    @overload
    def piece(self, identity: Identity, piece: Literal[Rig.LAMP], scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.Object, bpy.types.Light]]: ...

    def piece(self, identity: Identity, piece: Piece, scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.Object, bpy.types.Mesh | bpy.types.Camera | bpy.types.Light]]:
        """Owns a native mesh, camera, or light and links its object exactly once."""
        objects, data = piece_ownership(piece)

        def built() -> BlenderResult[bpy.types.Mesh | bpy.types.Camera | bpy.types.Light]:
            def native() -> bpy.types.Mesh | bpy.types.Camera | bpy.types.Light:
                match piece:
                    case Rig.CAMERA:
                        return bpy.data.cameras.new(data.key)
                    case Rig.LAMP:
                        return bpy.data.lights.new(data.key, type="SUN")
                    case Shape() as shape:
                        with ExitStack() as scope:
                            mesh = bpy.data.meshes.new(data.key)
                            rollback = scope.enter_context(ExitStack())
                            rollback.callback(bpy.data.meshes.remove, mesh)
                            surface = bmesh.new()
                            scope.callback(surface.free)
                            surface.loops.layers.uv.new("UVMap")
                            match shape:
                                case Shape.PLANE:
                                    bmesh.ops.create_grid(surface, x_segments=1, y_segments=1, size=1.0, calc_uvs=True)
                                case Shape.SPHERE:
                                    bmesh.ops.create_uvsphere(surface, u_segments=48, v_segments=24, radius=0.92, calc_uvs=True)
                                case Shape.CUBE:
                                    bmesh.ops.create_cube(surface, size=1.0, calc_uvs=True)
                            surface.to_mesh(mesh)
                            for face in mesh.polygons:
                                face.use_smooth = shape is Shape.SPHERE
                            rollback.pop_all()
                            return mesh

            return catch(native, exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=data.key, error=error),))

        def linked(block: bpy.types.Mesh | bpy.types.Camera | bpy.types.Light) -> BlenderResult[bpy.types.Object]:
            def native() -> bpy.types.Object:
                with ExitStack() as rollback:
                    item = bpy.data.objects.new(objects.key, block)
                    rollback.callback(bpy.data.objects.remove, item)
                    scene.collection.objects.link(item)
                    rollback.pop_all()
                    return item

            return catch(native, exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=objects.key, error=error),))

        transforms = Snapshot.of(
            bpy.types.Object,
            {
                "location": (0.0, 0.0, 0.0),
                "rotation_euler": (0.7854, 0.6155, 0.0) if piece is Shape.CUBE else (0.0, 0.0, 0.0),
                "scale": (1.04, 1.04, 1.0) if piece is Shape.PLANE else (0.98, 0.98, 0.98) if piece is Shape.CUBE else (1.0, 1.0, 1.0),
                "hide_render": isinstance(piece, Shape),
            },
        )
        return data.made(identity, built).bind(lambda block: objects.made(identity, lambda: linked(block)).bind(lambda item: transforms.apply(item).outcome.map(lambda _: (item, block))))

    def headed(self, identity: Identity, plane: npt.NDArray[np.float32]) -> BlenderResult[bpy.types.NodeTree]:
        """Fills a runtime image and returns a compositor-only head group."""
        height, width, _ = plane.shape
        tag = Tag(space=Some(PyOpenColorIO.GetCurrentConfig().getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()), alpha=Some("PREMUL"))

        def built() -> BlenderResult[bpy.types.Image]:
            with ExitStack() as rollback:
                try:
                    image = bpy.data.images.new("Preview Input", width=width, height=height, alpha=True, float_buffer=True)
                except (MemoryError, RuntimeError, ValueError) as error:
                    return Error((BlenderEditFault(request=self.image.key, error=error),))
                rollback.callback(bpy.data.images.remove, image)

                def kept(_: None) -> bpy.types.Image:
                    rollback.pop_all()
                    return image

                return tag.applied(image).map(kept)

        def graphed(image: bpy.types.Image) -> BlenderResult[bpy.types.NodeTree]:
            try:
                image.scale(width, height)
            except (MemoryError, RuntimeError, ValueError) as error:
                return Error((BlenderEditFault(request=self.image.key, error=error),))
            return (
                Result[None, BlenderFaults]
                .bind(filled(image, plane), lambda _: node("head", bpy.types.CompositorNodeImage, {}, {"image": IdName.of(image)}).map(ended("head", "Image")))
                .bind(lambda source: NodeGroup.built(self.head.key, "Preview Input", Interface(outputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),)), {"image": source}))
                .map(lambda group: msgspec.structs.replace(group, saved=Saved.NEVER))
                .bind(lambda group: group.made(identity))
            )

        return self.image.made(identity, built).bind(graphed)

    def held(self, identity: Identity, source: bpy.types.Scene, enabled: Enabled) -> tuple[Mirrored, ...]:
        """Returns enabled source entries that consume no scene-render pass."""
        return tuple(
            Mirrored(entry=held.entry, group=group, row=held.row)
            for held in self.stack.held(identity, source)
            for group in Option.of_optional(held.entry.node_group).to_list()
            if bool(enabled.descriptor.read(held.entry)) and not rendering(group)
        )

    def chain(self, source: bpy.types.Scene, head: Option[npt.NDArray[np.float32]], mirrored: tuple[Mirrored, ...], offered: tuple[Offered, ...]) -> Chain:
        """Adds the source's native formation only where the chain contains none."""
        formed = any(held.row.filter(lambda row: row.record is DisplayView).is_some() for held in mirrored) or any(isinstance(item.record.effect, DisplayView) for item in offered)
        return Chain(head=head, mirrored=mirrored, offered=offered if formed else (Offered(row=formation, record=Entry(effect=viewed(source))), *offered))

    def chained(self, identity: Identity, scene: bpy.types.Scene, chain: Chain, *, context: Context = Nothing) -> BlenderResult[None]:
        """Appends the head and mirrors, then places effects or applies a named recipe."""

        def appended(group: bpy.types.NodeTree, source: Option[bpy.types.SceneCompositorEffect]) -> BlenderResult[None]:
            def made() -> bpy.types.SceneCompositorEffect:
                return scene.compositor_effects.new(group.name)

            made_entry: BlenderResult[bpy.types.SceneCompositorEffect] = catch(made, exception=RuntimeError)().map_error(
                lambda error: (BlenderEditFault(request=repr(scene.compositor_effects), error=error),)
            )
            return (
                Result[bpy.types.SceneCompositorEffect, BlenderFaults]
                .bind(
                    made_entry,
                    lambda entry: written(entry, "node_group", group).bind(
                        lambda _: source.map(
                            lambda held: Snapshot.capture(held.properties.inputs).bind(lambda snapshot: snapshot.apply(entry.properties.inputs).outcome).map(lambda _: None)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                        ).default_value(Ok(None))
                    ),
                )
                .map(lambda _: None)
            )

        with bpy.context.temp_override(scene=scene):
            return (
                chain.head
                .map(lambda pixels: self.headed(identity, pixels).bind(lambda group: appended(group, Nothing)))
                .default_value(Ok(None))
                .bind(lambda _: accumulated(appended(held.group, Some(held.entry)) for held in chain.mirrored))
                .bind(
                    lambda _: (
                        self.stack.applied(
                            identity, scene, {name: msgspec.structs.replace(item, render=item.preview) for name, item in chain.offered.items()}, initial_view=chain.initial_view, context=context
                        )
                        if isinstance(chain.offered, Mapping)
                        else accumulated(self.stack.placed(identity, scene, item.row, item.record, context=context) for item in chain.offered).map(lambda _: None)
                    )
                )
            )

    @contextmanager
    def staged(self, identity: Identity, staging: Staging) -> Generator[BlenderResult[bpy.types.Scene]]:
        """Stages one render and releases subjects, mirrors, additions, and route copies."""
        subjects = ExitStack()
        cleanup: list[BlenderResult[None]] = []

        def released(operation: Callable[[], object]) -> None:
            try:
                operation()
                cleanup.append(Ok(None))
            except (ReferenceError, RuntimeError) as error:
                cleanup.append(Error((BlenderEditFault(request=repr(staging.source), error=error),)))

        def arranged(scene: bpy.types.Scene, world: bpy.types.World | tuple[Environment, Graph]) -> BlenderResult[bpy.types.Scene]:
            engine_owner, samples = staging.engine.value
            settings = Snapshot.of(
                bpy.types.Scene,
                {
                    "render.engine": staging.engine.name,
                    "render.resolution_x": staging.size[0],
                    "render.resolution_y": staging.size[1],
                    "render.resolution_percentage": 100,
                    "render.film_transparent": staging.transparent,
                    "render.use_compositing": True,
                    "render.use_sequencer": False,
                    "render.use_multiview": False,
                    "render.use_motion_blur": False,
                    "render.use_border": False,
                    "render.use_stamp": False,
                    f"{engine_owner}.{samples}": self.samples,
                    **({"cycles.use_denoising": True} if staging.engine is PreviewEngine.CYCLES else {}),
                    "frame_current": staging.source.frame_current,
                },
            )

            def configured(
                shapes: tuple[tuple[Shape, tuple[bpy.types.Object, bpy.types.Mesh]], ...],
                camera_piece: tuple[bpy.types.Object, bpy.types.Camera],
                light_piece: tuple[bpy.types.Object, bpy.types.Light],
            ) -> BlenderResult[bpy.types.Scene]:
                camera, camera_data = camera_piece
                light, light_data = light_piece
                subjects.callback(released, partial(setattr, light, "data", light_data))
                for _shape, (item, mesh) in shapes:
                    subjects.callback(released, partial(setattr, item, "hide_render", True))
                    subjects.callback(released, mesh.materials.clear)

                def surfaced() -> None:
                    for shape, (item, mesh) in shapes:
                        mesh.materials.clear()
                        if staging.shape == Some(shape):
                            item.hide_render = False
                            for material in staging.material.to_list():
                                mesh.materials.append(material)
                    for subject in staging.subject.to_list():
                        scene.collection.objects.link(subject)
                        subjects.callback(released, partial(scene.collection.objects.unlink, subject))
                    scene.camera = camera

                def lit() -> BlenderResult[None]:
                    match staging.lamp:
                        case bpy.types.Light() as lamp:
                            return written(light, "data", lamp).map(lambda _: None)
                        case Lamp(energy=energy, kind=kind, location=location, rotation=rotation):
                            return accumulated((
                                Snapshot.of(bpy.types.Light, {"type": kind, "energy": energy}).apply(light_data).outcome,
                                Snapshot.of(bpy.types.Object, {"location": location, "rotation_euler": rotation}).apply(light).outcome,
                            )).map(lambda _: None)

                def viewed_camera() -> BlenderResult[None]:
                    data: Mapping[str, Value]
                    transform: Mapping[str, Value]
                    match staging.view:
                        case Top():
                            data, transform = {"type": "ORTHO", "ortho_scale": 2.0}, {"location": (0.0, 0.0, 2.0), "rotation_euler": (0.0, 0.0, 0.0)}
                        case Panorama() if staging.engine is PreviewEngine.CYCLES:
                            data, transform = {"type": "PANO", "panorama_type": "EQUIRECTANGULAR"}, {"location": (0.0, 0.0, 0.0), "rotation_euler": (math.pi / 2.0, 0.0, 0.0)}
                        case Panorama():
                            data, transform = {"type": "PERSP", "lens": 12.0}, {"location": (0.0, 0.0, 0.0), "rotation_euler": (1.3, 0.0, 0.0)}
                        case Interior():
                            data, transform = {"type": "PERSP", "lens": 45.0}, {"location": (0.0, -4.0, 0.0), "rotation_euler": (math.pi / 2.0, 0.0, 0.0)}
                        case Framed():
                            subject: BlenderResult[bpy.types.Object] = staging.subject.to_result_with(lambda: (BlenderAbsentFault(owner=repr(staging), key="subject"),))

                            def fitted(item: bpy.types.Object) -> BlenderResult[None]:
                                with bpy.context.temp_override(scene=scene):
                                    depsgraph = bpy.context.evaluated_depsgraph_get()
                                    depsgraph.update()
                                    evaluated = item.evaluated_get(depsgraph)
                                    corners = np.asarray([(evaluated.matrix_world @ Vector(corner)).to_tuple() for corner in evaluated.bound_box], dtype=np.float32)
                                rotation = Vector((-0.55, 0.8, -0.5)).to_track_quat("-Z", "Y").to_euler()
                                return (
                                    Snapshot
                                    .of(bpy.types.Camera, {"type": "ORTHO"})
                                    .apply(camera_data)
                                    .outcome.bind(lambda _: Snapshot.of(bpy.types.Object, {"rotation_euler": (rotation.x, rotation.y, rotation.z)}).apply(camera).outcome)
                                    .bind(lambda _: framed(camera, corners, depsgraph))
                                )

                            return Result[bpy.types.Object, BlenderFaults].bind(subject, fitted)
                    return paired(Snapshot.of(bpy.types.Camera, data).apply(camera_data).outcome, Snapshot.of(bpy.types.Object, transform).apply(camera).outcome).map(lambda _: None)

                def environment() -> BlenderResult[None]:
                    match world:
                        case bpy.types.World() as held_world:
                            return written(scene, "world", held_world).map(lambda _: None)
                        case (Environment(), Graph()) as prepared:
                            return (
                                self.world
                                .made(
                                    identity,
                                    lambda: catch(lambda: bpy.data.worlds.new("Preview World"), exception=RuntimeError)().map_error(
                                        lambda error: (BlenderEditFault(request=self.world.key, error=error),)
                                    ),
                                )
                                .bind(lambda held: written(scene, "world", held).bind(lambda _: swapped(self.world, prepared, scene, identity)))
                                .map(lambda _: None)
                            )

                surfaced_result: BlenderResult[None] = catch(surfaced, exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(scene), error=error),))
                return (
                    Result[None, BlenderFaults]
                    .bind(surfaced_result, lambda _: accumulated((lit(), viewed_camera(), environment())))
                    .bind(lambda _: synced(scene, identity) if staging.value else Ok(None))
                    .bind(lambda _: self.chained(identity, scene, staging.chain, context=staging.context))
                    .bind(
                        lambda _: (
                            FrameSize()
                            .bound(identity)
                            .bind(lambda framed: camera_frame(framed, staging.source))
                            .bind(lambda frame: accumulated(entered(identity, held.entry, (), frame) for held in self.stack.held(identity, scene) if held.row.is_some()))
                        )
                    )
                    .map(lambda _: scene)
                )

            return (
                settings
                .apply(scene)
                .outcome.bind(
                    lambda _: paired(
                        accumulated(self.piece(identity, shape, scene).map(lambda piece: (shape, piece)) for shape in Shape),
                        paired(self.piece(identity, Rig.CAMERA, scene), self.piece(identity, Rig.LAMP, scene)),
                    )
                )
                .bind(lambda pieces: configured(pieces[0], *pieces[1]))
            )

        world = staging.world
        wanted: BlenderResult[bpy.types.World | tuple[Environment, Graph]] = world_graph(world).map(lambda graph: (world, graph)) if isinstance(world, Environment) else Ok(world)
        scene = Result[bpy.types.World | tuple[Environment, Graph], BlenderFaults].bind(wanted, lambda world: self.made(identity).map(lambda scene: (scene, world)))
        try:
            yield Result[tuple[bpy.types.Scene, bpy.types.World | tuple[Environment, Graph]], BlenderFaults].bind(scene, lambda pair: arranged(*pair))
        finally:
            try:
                for held, _ in scene.to_option().to_list():
                    for entry, row in tuple((item.entry, item.row) for item in self.stack.held(identity, held)):
                        if row.is_some() and (isinstance(staging.chain.offered, Mapping) or any(item is offered.row for offered in staging.chain.offered for item in row.to_list())):
                            cleanup.append(self.stack.removed(identity, held, entry.name))
                        else:
                            released(partial(held.compositor_effects.remove, entry))
            finally:
                subjects.close()
                for faults in accumulated(cleanup).swap().to_option().to_list():
                    identity.reports.hold(faults)

    def rendered(self, identity: Identity, staging: Staging) -> BlenderResult[Readback]:
        """Captures the chain's native domain and covered value AOV through declared outputs."""
        files: list[Path] = []

        def removed() -> BlenderResult[None]:
            return accumulated(catch(lambda: file.unlink(missing_ok=True), exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=str(file), error=error),)) for file in files).map(
                lambda _: None
            )

        def declared(scene: bpy.types.Scene, value: Option[str]) -> BlenderResult[dict[str, Path]]:
            sources: tuple[tuple[str, Literal["RGBA", "FLOAT"], End], ...] = (
                ("image", "RGBA", End(node="raster", socket="Color")),
                *(("value", "FLOAT", End(node="passes", socket=name)) for name in value.to_list()),
                *(("coverage", "FLOAT", End(node="passes", socket="Alpha")) for _ in value.to_list()),
            )
            names = {key: f"{scene.session_uid}_{self.capture.key}_{key}" for key, _, _ in sources}
            paths = {key: Path(bpy.app.tempdir, f"{name}.exr") for key, name in names.items()}
            files.extend(paths.values())
            face = Interface(inputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),), outputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),))
            output = Node(
                kind=bpy.types.CompositorNodeOutputFile,
                members={Pathed.directory: bpy.app.tempdir, Pathed.file_name: ""},
                content=Some(FileItems(items=tuple(FileItem(name=names[key], kind=kind) for key, kind, _ in sources))),
            )

            def formatted(tree: bpy.types.NodeTree) -> BlenderResult[bpy.types.NodeTree]:
                native = next(node for node in tree.nodes if Mark.read(identity, node) == Some(Mark(key="files")))
                return (
                    OutputFormat
                    .of(Output.DATA, Kind.EXR)
                    .bind(lambda output: output.applied(native))
                    .bind(
                        lambda _: (
                            Snapshot
                            .of(
                                bpy.types.CompositorNodeOutputFile,
                                {
                                    "format.color_mode": "RGBA",
                                    "format.color_management": "OVERRIDE",
                                    "format.linear_colorspace_settings.name": PyOpenColorIO.GetCurrentConfig().getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName(),
                                },
                            )
                            .apply(native)
                            .outcome
                        )
                    )
                    .map(lambda _: tree)
                )

            def appended(tree: bpy.types.NodeTree) -> BlenderResult[dict[str, Path]]:
                def native() -> None:
                    entry = scene.compositor_effects.new(self.capture.key)
                    entry.node_group = tree

                return catch(native, exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=self.capture.key, error=error),)).map(lambda _: paths)

            size = staging.chain.head.map(lambda pixels: (pixels.shape[1], pixels.shape[0])).default_value(staging.size)
            return (
                removed()
                .bind(lambda _: NodeGroup.built(self.capture.key, "Preview Capture", face, {"image": End(node=INPUTS, socket="image")}))
                .map(
                    lambda group: msgspec.structs.replace(
                        group,
                        saved=Saved.NEVER,
                        graph=msgspec.structs.replace(
                            group.graph,
                            nodes=group.graph.nodes
                            | {"files": output, "domain": Node(kind=bpy.types.CompositorNodeBlankImage, inputs={"Size": size}), "raster": Node(kind=bpy.types.ShaderNodeMixRGB, inputs={"Fac": 1.0})}
                            | ({"passes": Node(kind=bpy.types.CompositorNodeRLayers)} if value.is_some() else {}),
                            links=(
                                *group.graph.links,
                                Link(source=End(node=INPUTS, socket="image"), target=End(node="raster", socket="Color2")),
                                Link(source=End(node="domain", socket="Image"), target=End(node="raster", socket="Color1")),
                                *(Link(source=end, target=End(node="files", socket=names[key])) for key, _, end in sources),
                            ),
                        ),
                    )
                )
                .bind(lambda group: group.made(identity))
                .bind(formatted)
                .bind(appended)
            )

        def readback(paths: dict[str, Path]) -> BlenderResult[Readback]:
            sampled = (
                paired(plane(str(paths["value"]), ("",), ("V",)), plane(str(paths["coverage"]), ("",), ("V",))).map(lambda pair: Some(pair[0][..., 0][pair[1][..., 0] > Readback.covered]))
                if staging.value
                else Ok(Nothing)
            )
            return paired(plane(str(paths["image"]), ("",), ("R", "G", "B", "A")), sampled).map(lambda pair: Readback(plane=pair[0], values=pair[1]))

        try:
            with self.staged(identity, staging) as staged:
                return staged.bind(
                    lambda scene: (
                        (identity.name(Name.ATTACHMENT, self.value).map(Some) if staging.value else Ok(Nothing))
                        .bind(lambda value: declared(scene, value))
                        .bind(
                            lambda paths: (
                                self.stack
                                .precision(identity)
                                .bind(lambda precision: execution.rendered(identity, self.stack, scene, precision, {"write_still": False}))
                                .bind(lambda _: readback(paths))
                            )
                        )
                    )
                )
        finally:
            for faults in removed().swap().to_option().to_list():
                identity.reports.hold(faults)

    def sampled(self, identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], plane: npt.NDArray[np.float32]) -> BlenderResult[npt.NDArray[np.float32]]:
        """Samples a float plane through the supplied entries without adding a formation."""
        height, width, channels = plane.shape
        lighting = Lighting()
        carrier = paired(
            Descriptor.resolve(bpy.types.RenderSettings, "resolution_x", bpy.types.IntProperty).map(lambda prop: max(width, prop.prop.hard_min)),
            Descriptor.resolve(bpy.types.RenderSettings, "resolution_y", bpy.types.IntProperty).map(lambda prop: max(height, prop.prop.hard_min)),
        )
        return (
            paired(carrier, lighting.environment(bpy.context.preferences))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            .bind(
                lambda pair: self.rendered(
                    identity,
                    Staging(
                        source=scene,
                        size=pair[0],
                        engine=PreviewEngine.of(scene),
                        world=pair[1],
                        lamp=lighting.lamp,
                        chain=Chain(head=Some(plane), mirrored=tuple(Mirrored(entry=entry, group=group) for entry in entries for group in Option.of_optional(entry.node_group).to_list())),
                    ),
                )
            )
            .map(lambda readback: readback.plane[..., :channels])
        )

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns native ownership and the world attachment used by preview staging."""
        return Ok((self.scene, self.world, self.image, self.head, self.capture, self.routes, WorldEnvironment.attachment, *(row for piece in (*Shape, *Rig) for row in piece_ownership(piece))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def piece_ownership(piece: Piece) -> tuple[Ownership[bpy.types.Object], Ownership[bpy.types.Mesh | bpy.types.Camera | bpy.types.Light]]:
    """Derives the object and native data ownership from one preview piece."""
    key = f"preview_{piece.name.lower()}"
    kind = bpy.types.Mesh if isinstance(piece, Shape) else bpy.types.Camera if piece is Rig.CAMERA else bpy.types.Light
    return Ownership(kind=bpy.types.Object, key=key, saved=Saved.NEVER), Ownership(kind=kind, key=key, saved=Saved.NEVER)


def rendering(group: bpy.types.NodeTree) -> bool:
    """Reads whether the group or a nested group requests a scene render."""
    return any(
        isinstance(node, bpy.types.CompositorNodeRLayers)
        or (isinstance(node, bpy.types.CompositorNodeCryptomatteV2) and node.source == "RENDER")
        or (isinstance(node, bpy.types.CompositorNodeGroup) and Option.of_optional(node.node_tree).filter(rendering).is_some())
        for node in group.nodes
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Ambient",
    "Chain",
    "Framed",
    "Interior",
    "Lamp",
    "Lighting",
    "Mirrored",
    "Offered",
    "Panorama",
    "Piece",
    "PreviewEngine",
    "PreviewScene",
    "Readback",
    "Rig",
    "Shape",
    "Staging",
    "Studio",
    "Top",
    "View",
    "White",
    "rendering",
]
