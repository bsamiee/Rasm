"""Runtime preview scenes, their compositor stacks, and readback."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager
from enum import auto, Enum
from functools import partial
import math
from pathlib import Path
from types import MappingProxyType
from typing import Annotated, ClassVar, Final, Literal, overload, Self

import bmesh
import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
from mathutils import Vector
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import filled, Tag
from blender.bpy.data.ids.ownership import kept, Mark, Ownership, Saved
from blender.bpy.data.ids.store import IdName, vacant
from blender.bpy.data.rna.descriptors import assigned, Descriptor, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderEditFault, BlenderResult
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.graphs import DataType, Expr, FileItem, FileItems, group_input, NodeGroup
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.nodes.trees import nested
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.compositor.effects import Context, Enabled, Restoring
from blender.bpy.render.compositor.inputs import inputs_of
from blender.bpy.render.outputs.exports import plane
from blender.bpy.render.outputs.formats import Kind, OutputFormat
from blender.bpy.render.outputs.paths import Pathed
from blender.bpy.render.sessions.renders import native, Render, RenderCall
from blender.bpy.render.view_layers.passes import synced
from blender.bpy.scene.cameras import framed
from blender.bpy.scene.objects import spawned
from blender.bpy.scene.worlds import Backdrop, Environment, FlatColor, Hdri, studio, studios, swapped, world_graph, WorldEnvironment
from blender.imaging.color.processors import DisplayView
from blender.records.faults import accumulated, FileFault, paired
from blender.records.fields import Case, Choice, heading, Rgb
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type EntryPlacement = Callable[[Identity, bpy.types.Scene, bpy.types.Scene, Context], BlenderResult[Restoring]]


class PreviewEngine(Enum):
    """Preview engines with their sample setting path."""

    BLENDER_EEVEE = "eevee", "taa_render_samples"
    CYCLES = "cycles", "samples"

    @classmethod
    def of(cls, scene: bpy.types.Scene) -> "PreviewEngine":
        """Returns source engine when listed, else EEVEE."""
        return cls.__members__.get(scene.render.engine, cls.BLENDER_EEVEE)


class PreviewMesh(Choice):
    """Shader preview surface."""

    SPHERE = 1, "Sphere"
    CUBE = 2, "Cube"
    PLANE = 3, "Plane"


class Fixture(Enum):
    """Shared camera and key light fixtures."""

    CAMERA = "camera"
    LAMP = "lamp"


class CameraPose(Enum):
    """Fixed preview camera: orthographic above the surface, panoramic at the world origin, or perspective into a world volume."""

    TOP = auto()
    PANORAMA = auto()
    INTERIOR = auto()


type Piece = PreviewMesh | Fixture
type Ambient = White | Studio
type Viewpoint = CameraPose | Fitted

# --- [CONSTANTS] ------------------------------------------------------------------------

CAMERAS: Final[Mapping[tuple[CameraPose, PreviewEngine | None], tuple[Mapping[str, Value], Mapping[str, Value]]]] = MappingProxyType({
    (CameraPose.TOP, None): ({"type": "ORTHO", "ortho_scale": 2.0}, {"location": (0.0, 0.0, 2.0), "rotation_euler": (0.0, 0.0, 0.0)}),
    (CameraPose.PANORAMA, PreviewEngine.CYCLES): ({"type": "PANO", "panorama_type": "EQUIRECTANGULAR"}, {"location": (0.0, 0.0, 0.0), "rotation_euler": (math.pi / 2.0, 0.0, 0.0)}),
    (CameraPose.PANORAMA, None): ({"type": "PERSP", "lens": 12.0}, {"location": (0.0, 0.0, 0.0), "rotation_euler": (1.3, 0.0, 0.0)}),
    (CameraPose.INTERIOR, None): ({"type": "PERSP", "lens": 45.0}, {"location": (0.0, -4.0, 0.0), "rotation_euler": (math.pi / 2.0, 0.0, 0.0)}),
})
RESOLUTION: Final = (Descriptor.declared(bpy.types.RenderSettings, "resolution_x", bpy.types.IntProperty), Descriptor.declared(bpy.types.RenderSettings, "resolution_y", bpy.types.IntProperty))

# --- [MODELS] ---------------------------------------------------------------------------


class White(Case, frozen=True):
    """Uniform white ambient light."""

    title = "White"


class Studio(Case, frozen=True, kw_only=True):
    """Blender studio light environment."""

    title = "Studio Light"
    name: Annotated[str, msgspec.Meta(title="World"), studios] = "forest.exr"


class Lamp(msgspec.Struct, frozen=True, kw_only=True):
    """Key light energy, type, and transform."""

    energy: float
    kind: Literal["SUN", "POINT"] = "SUN"
    location: tuple[float, float, float] = 0.0, 0.0, 0.0
    rotation: tuple[float, float, float] = 0.9, 0.15, 0.5


class Lighting(msgspec.Struct, frozen=True, kw_only=True):
    """Preview surface, ambient, and key strength."""

    surface: PreviewMesh = PreviewMesh.SPHERE
    ambient: Ambient = Studio()
    strength: Annotated[float, msgspec.Meta(ge=0.0, le=10.0, title="World Light")] = 1.0
    key: Annotated[float, msgspec.Meta(ge=0.0, le=20.0, title="Key Light")] = 2.0

    @property
    def environment(self) -> BlenderResult[Environment]:
        """Environment lit by studio light or flat white backdrop."""
        match self.ambient:
            case White():
                return Ok(Environment(lighting=Backdrop(mode=FlatColor(color=Rgb(red=1.0, green=1.0, blue=1.0)), strength=self.strength)))
            case Studio(name=name):
                return studio(name, bpy.context.preferences).map(lambda path: Environment(lighting=Backdrop(mode=Hdri(path=path), strength=self.strength)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    @property
    def lamp(self) -> Lamp:
        """Key sun at key strength."""
        return Lamp(energy=self.key)


class Fitted(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Orthographic camera fitted to its subject's bounds, the subject linked into the preview scene for the render."""

    subject: bpy.types.Object


class Mirrored(msgspec.Struct, frozen=True, kw_only=True, eq=False):
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
            if bool(enabled.descriptor.read(entry)) and not rendering(group)
        )


class PreviewStack(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Compositor entries of one preview render: the render input plane, ordered mirrored entries, and the placement adding entries after them."""

    head: Option[npt.NDArray[np.float32]] = Nothing
    mirrored: tuple[Mirrored, ...] = ()
    placing: EntryPlacement = lambda _identity, _source, _scene, _context: Ok(lambda: Ok(None))


class Staging(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Preview scene state of one render."""

    source: bpy.types.Scene
    size: tuple[int, int]
    engine: PreviewEngine
    world: Environment | bpy.types.World
    lamp: Lamp | bpy.types.Light
    stack: PreviewStack = PreviewStack()
    transparent: bool = True
    surface: Option[PreviewMesh] = Nothing
    material: Option[bpy.types.Material] = Nothing
    viewpoint: Viewpoint = CameraPose.TOP
    value: bool = False
    context: Context = Nothing


class Readback(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Associated scene-linear compositor output with covered value AOV samples."""

    plane: npt.NDArray[np.float32]
    values: Option[npt.NDArray[np.float32]] = Nothing
    covered: ClassVar[float] = 0.5

    def formed(self, view: DisplayView, config: PyOpenColorIO.Config) -> BlenderResult[Self]:
        """Returns the readback with its plane formed through a view."""
        return view.form(config, self.plane).map(lambda plane: msgspec.structs.replace(self, plane=plane))


# --- [SERVICES] -------------------------------------------------------------------------


class PreviewScene(msgspec.Struct, frozen=True, kw_only=True):
    """Owned preview scene and reused data its renders stage, each render running through `render`."""

    render: Render = native
    scene: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="preview_scene", saved=Saved.NEVER)
    world: ClassVar[Ownership[bpy.types.World]] = Ownership(kind=bpy.types.World, key="preview_world", saved=Saved.NEVER)
    image: ClassVar[Ownership[bpy.types.Image]] = Ownership(kind=bpy.types.Image, key="preview_image", saved=Saved.NEVER)
    head: ClassVar[Ownership[bpy.types.NodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key="preview_head", saved=Saved.NEVER)
    capture: ClassVar[Ownership[bpy.types.NodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key="preview_capture", saved=Saved.NEVER)
    routes: ClassVar[Ownership[bpy.types.ID]] = Ownership(kind=bpy.types.ID, key="preview_route", saved=Saved.NEVER, single=False)
    value: ClassVar[Key] = "value"
    samples: ClassVar[int] = 16

    # --- [STAGING]

    def made(self, identity: Identity) -> BlenderResult[bpy.types.Scene]:
        """Returns extension runtime preview scene."""
        return self.scene.made(identity, lambda: BlenderEditFault.attempted(self.scene.key, lambda: bpy.data.scenes.new(vacant("Preview", bpy.data.scenes.keys()))))

    def headed(self, identity: Identity, pixels: npt.NDArray[np.float32]) -> BlenderResult[bpy.types.NodeTree]:
        """Returns head group reading pixels through a runtime image."""
        height, width, _ = pixels.shape
        tag = Tag(space=Some(PyOpenColorIO.GetCurrentConfig().getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()), alpha=Some("PREMUL"))

        def built() -> BlenderResult[bpy.types.Image]:
            created = BlenderEditFault.attempted(
                self.image.key, lambda: bpy.data.images.new(vacant("Preview Input", bpy.data.images.keys()), width=width, height=height, alpha=True, float_buffer=True)
            )
            return kept(created, lambda image: tag.applied(image).map(lambda _: image))

        def graphed(image: bpy.types.Image) -> BlenderResult[bpy.types.NodeTree]:
            source = Expr(kind=bpy.types.CompositorNodeImage, members=(("image", IdName.of(image)),), socket="Image", shape="RGBA")
            return (
                BlenderEditFault
                .attempted(self.image.key, lambda: image.scale(width, height))
                .bind(lambda _: filled(image, pixels))
                .bind(lambda _: NodeGroup.built(self.head.key, "Preview Input", Interface(outputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),)), {"image": source}))
                .bind(lambda group: made(msgspec.structs.replace(group, saved=Saved.NEVER), identity))
            )

        return self.image.made(identity, built).bind(graphed)

    def stacked(self, identity: Identity, staging: Staging, scene: bpy.types.Scene) -> BlenderResult[Restoring]:
        """Appends head and mirrored entries with their source inputs, then runs the stack's placement, answering its removal."""
        stack = staging.stack

        def copied(held: bpy.types.SceneCompositorEffect, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[None]:
            return paired(inputs_of(held).bind(Snapshot.capture), inputs_of(entry)).bind(lambda pair: pair[0].apply(pair[1]).outcome).map(lambda _: None)

        with bpy.context.temp_override(scene=scene):
            return (
                stack.head
                .map(lambda pixels: self.headed(identity, pixels).bind(lambda group: appended(scene, group)).map(lambda _: None))
                .default_value(Ok(None))
                .bind(lambda _: accumulated(appended(scene, held.group).bind(lambda entry: copied(held.entry, entry)) for held in stack.mirrored))
                .bind(lambda _: stack.placing(identity, staging.source, scene, staging.context))
            )

    @contextmanager
    def staged(self, identity: Identity, staging: Staging) -> Generator[BlenderResult[bpy.types.Scene]]:
        """Stages one render, running each release step at exit with their faults held for the extension."""
        releases: list[Callable[[], BlenderResult[object]]] = []

        def undone(request: object, step: Callable[[], object]) -> None:
            releases.append(lambda: BlenderEditFault.attempted(repr(request), step))

        def lit(light: bpy.types.Object, data: bpy.types.Light) -> BlenderResult[None]:
            def relit() -> None:
                light.data = data

            undone(light, relit)
            match staging.lamp:
                case bpy.types.Light() as lamp:
                    return written(light, "data", lamp).map(lambda _: None)
                case Lamp(energy=energy, kind=kind, location=location, rotation=rotation):
                    return paired(assigned(data, {"type": kind, "energy": energy}), assigned(light, {"location": location, "rotation_euler": rotation})).map(lambda _: None)

        def surfaced(scene: bpy.types.Scene, shapes: tuple[tuple[PreviewMesh, tuple[bpy.types.Object, bpy.types.Mesh]], ...]) -> BlenderResult[None]:
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
                *(BlenderEditFault.attempted(repr(item), partial(shown, shape, item, mesh)) for shape, (item, mesh) in shapes),
                *(BlenderEditFault.attempted(repr(scene), lambda: linked(view.subject)) for view in (staging.viewpoint,) if isinstance(view, Fitted)),
            )).map(lambda _: None)

        def aimed(scene: bpy.types.Scene, camera: bpy.types.Object, data: bpy.types.Camera) -> BlenderResult[None]:
            match staging.viewpoint:
                case CameraPose() as pose:
                    lens, transform = CAMERAS.get((pose, staging.engine), CAMERAS[pose, None])
                    return paired(assigned(data, lens), assigned(camera, transform)).map(lambda _: None)
                case Fitted(subject=subject):
                    with bpy.context.temp_override(scene=scene):
                        depsgraph = bpy.context.evaluated_depsgraph_get()
                        depsgraph.update()
                        evaluated = subject.evaluated_get(depsgraph)
                        corners = np.asarray([(evaluated.matrix_world @ Vector(corner)).to_tuple() for corner in evaluated.bound_box], dtype=np.float32)
                    rotation = Vector((-0.55, 0.8, -0.5)).to_track_quat("-Z", "Y").to_euler()
                    return paired(assigned(data, {"type": "ORTHO"}), assigned(camera, {"rotation_euler": (rotation.x, rotation.y, rotation.z)})).bind(lambda _: framed(camera, corners, depsgraph))

        def environment(scene: bpy.types.Scene) -> BlenderResult[None]:
            match staging.world:
                case bpy.types.World() as held:
                    return written(scene, "world", held).map(lambda _: None)
                case Environment() as wanted:
                    world = self.world.made(identity, lambda: BlenderEditFault.attempted(self.world.key, lambda: bpy.data.worlds.new(vacant("Preview World", bpy.data.worlds.keys()))))
                    return (
                        paired(world_graph(wanted), world)
                        .bind(lambda pair: written(scene, "world", pair[1]).bind(lambda _: swapped(self.world, (wanted, pair[0]), scene, identity)))
                        .map(lambda _: None)
                    )

        def arranged(
            scene: bpy.types.Scene,
            pieces: tuple[tuple[tuple[PreviewMesh, tuple[bpy.types.Object, bpy.types.Mesh]], ...], tuple[bpy.types.Object, bpy.types.Camera], tuple[bpy.types.Object, bpy.types.Light]],
        ) -> BlenderResult[bpy.types.Scene]:
            shapes, (camera, lens), (light, data) = pieces
            return (
                paired(surfaced(scene, shapes), written(scene, "camera", camera))
                .bind(lambda _: paired(lit(light, data), aimed(scene, camera, lens), environment(scene)))
                .bind(lambda _: synced(scene, identity) if staging.value else Ok(None))
                .bind(lambda _: self.stacked(identity, staging, scene))
                .map(releases.append)
                .map(lambda _: scene)
            )

        def unchained(scene: bpy.types.Scene) -> BlenderResult[object]:
            return accumulated(BlenderEditFault.attempted(repr(entry), partial(scene.compositor_effects.remove, entry)) for entry in tuple(scene.compositor_effects))

        def prepared(scene: bpy.types.Scene) -> BlenderResult[bpy.types.Scene]:
            releases.append(lambda: unchained(scene))
            samples = ".".join(staging.engine.value)
            settings: dict[str, Value] = {
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
                samples: self.samples,
                **({"cycles.use_denoising": True} if staging.engine is PreviewEngine.CYCLES else {}),
                "frame_current": staging.source.frame_current,
            }
            return (
                assigned(scene, settings)
                .bind(
                    lambda _: paired(
                        accumulated(piece(identity, shape, scene).map(lambda made: (shape, made)) for shape in PreviewMesh),
                        piece(identity, Fixture.CAMERA, scene),
                        piece(identity, Fixture.LAMP, scene),
                    )
                )
                .bind(lambda pieces: arranged(scene, pieces))
            )

        try:
            yield self.made(identity).bind(prepared)
        finally:
            for faults in accumulated(release() for release in reversed(releases)).swap().to_option().to_list():
                identity.reports.hold(faults)

    # --- [READBACK]

    def rendered(self, identity: Identity, staging: Staging) -> BlenderResult[Readback]:
        """Renders staging and reads image and value AOV from File Output files."""
        size = staging.stack.head.map(lambda pixels: (pixels.shape[1], pixels.shape[0])).default_value(staging.size)

        def declared(scene: bpy.types.Scene, value: Option[str], paths: Mapping[str, Path]) -> BlenderResult[None]:
            image = group_input(COLOR_INPUT.key, "RGBA")
            domain = Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Size", size),), socket="Image", shape="RGBA", label=Some("domain"))
            passes = value.map(lambda name: {"value": name, "coverage": "Alpha"}).default_value({})
            sources: dict[str, tuple[DataType, Expr]] = {
                "image": ("RGBA", Expr(kind=bpy.types.ShaderNodeMixRGB, inputs=(("Fac", 1.0), ("Color1", domain), ("Color2", image)), socket="Color", shape="RGBA")),
                **{key: ("FLOAT", Expr(kind=bpy.types.CompositorNodeRLayers, members=(("scene", IdName.of(scene)),), socket=socket)) for key, socket in passes.items()},
            }
            files = Expr(
                kind=bpy.types.CompositorNodeOutputFile,
                inputs=tuple((paths[key].stem, term) for key, (_, term) in sources.items()),
                members=((Pathed.directory.path, bpy.app.tempdir), (Pathed.file_name.path, "")),
                content=Some(FileItems(items=tuple(FileItem(name=paths[key].stem, kind=kind) for key, (kind, _) in sources.items()))),
                label=Some("files"),
            )
            capture = NodeGroup.built(self.capture.key, "Preview Capture", Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: image.keeping(files)}).map(
                lambda group: msgspec.structs.replace(group, saved=Saved.NEVER)
            )
            return capture.bind(lambda group: made(group, identity)).bind(formatted).bind(lambda tree: appended(scene, tree)).map(lambda _: None)

        def formatted(tree: bpy.types.NodeTree) -> BlenderResult[bpy.types.NodeTree]:
            native = next(node for node in tree.nodes if Mark.read(identity, node) == Some(Mark(key="files")))
            linear = PyOpenColorIO.GetCurrentConfig().getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()
            return (
                OutputFormat
                .of(Output.DATA, Kind.EXR)
                .bind(lambda output: output.applied(native))
                .bind(lambda _: assigned(native, {"format.color_mode": "RGBA", "format.color_management": "OVERRIDE", "format.linear_colorspace_settings.name": linear}))
                .map(lambda _: tree)
            )

        def covered(values: npt.NDArray[np.float32], coverage: npt.NDArray[np.float32]) -> Option[npt.NDArray[np.float32]]:
            mask = coverage[..., 0] > Readback.covered
            return Some(values[..., 0][mask] / coverage[..., 0][mask])

        def readback(paths: Mapping[str, Path]) -> BlenderResult[Readback]:
            sampled = paired(plane(str(paths["value"]), ("",), ("R",)), plane(str(paths["coverage"]), ("",), ("R",))).map(lambda pair: covered(*pair)) if staging.value else Ok(Nothing)
            return paired(plane(str(paths["image"]), ("",), ("R", "G", "B", "A")), sampled).map(lambda pair: Readback(plane=pair[0], values=pair[1]))

        def captured(scene: bpy.types.Scene) -> BlenderResult[Readback]:
            paths = {key: Path(bpy.app.tempdir, f"{scene.session_uid}_{self.capture.key}_{key}.exr") for key in ("image", "value", "coverage")}
            value = identity.name(Name.ATTACHMENT, self.value).map(Some) if staging.value else Ok(Nothing)
            try:
                return value.bind(lambda name: declared(scene, name, paths)).bind(lambda _: unlinked(paths)).bind(lambda _: self.render(identity, scene, RenderCall())).bind(lambda _: readback(paths))
            finally:
                for faults in unlinked(paths).swap().to_option().to_list():
                    identity.reports.hold(faults)

        with self.staged(identity, staging) as staged:
            return staged.bind(captured)

    def sampled(self, identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], pixels: npt.NDArray[np.float32]) -> BlenderResult[npt.NDArray[np.float32]]:
        """Renders float pixels through entries without adding formation."""
        height, width, channels = pixels.shape
        lighting = Lighting()
        carrier = (max(width, RESOLUTION[0].prop.hard_min), max(height, RESOLUTION[1].prop.hard_min))
        stack = PreviewStack(head=Some(pixels), mirrored=tuple(Mirrored(entry=entry, group=group) for entry in entries for group in Option.of_optional(entry.node_group).to_list()))
        return lighting.environment.bind(lambda world: self.rendered(identity, Staging(source=scene, size=carrier, engine=PreviewEngine.of(scene), world=world, lamp=lighting.lamp, stack=stack))).map(
            lambda readback: readback.plane[..., :channels]
        )

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns owned data rows and world attachment."""
        return Ok((self.scene, self.world, self.image, self.head, self.capture, self.routes, WorldEnvironment.attachment, *(row for kind in (*PreviewMesh, *Fixture) for row in piece_ownership(kind))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def appended(scene: bpy.types.Scene, group: bpy.types.NodeTree) -> BlenderResult[bpy.types.SceneCompositorEffect]:
    """Appends a compositor entry running group."""
    return BlenderEditFault.attempted(repr(scene.compositor_effects), lambda: scene.compositor_effects.new(group.name)).bind(lambda entry: written(entry, "node_group", group).map(lambda _: entry))


def unlinked(paths: Mapping[str, Path]) -> BlenderResult[None]:
    """Deletes capture files a render writes, absent ones included."""

    def unlink(path: Path) -> BlenderResult[None]:
        return catch(partial(path.unlink, missing_ok=True), exception=OSError)().map_error(lambda error: (FileFault(access="write", request=path, error=error),))

    return accumulated(map(unlink, paths.values())).map(lambda _: None)


def piece_ownership(kind: Piece) -> tuple[Ownership[bpy.types.Object], Ownership[bpy.types.Mesh | bpy.types.Camera | bpy.types.Light]]:
    """Returns object and data ownership of a preview piece."""
    key = f"preview_{kind.name.lower()}"
    data = bpy.types.Mesh if isinstance(kind, PreviewMesh) else bpy.types.Camera if kind is Fixture.CAMERA else bpy.types.Light
    return Ownership(kind=bpy.types.Object, key=key, saved=Saved.NEVER), Ownership(kind=data, key=key, saved=Saved.NEVER)


@overload
def piece(identity: Identity, kind: PreviewMesh, scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.Object, bpy.types.Mesh]]: ...


@overload
def piece(identity: Identity, kind: Literal[Fixture.CAMERA], scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.Object, bpy.types.Camera]]: ...


@overload
def piece(identity: Identity, kind: Literal[Fixture.LAMP], scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.Object, bpy.types.Light]]: ...


def piece(identity: Identity, kind: Piece, scene: bpy.types.Scene) -> BlenderResult[tuple[bpy.types.Object, bpy.types.Mesh | bpy.types.Camera | bpy.types.Light]]:
    """Returns owned piece data with its object linked once, placed at the piece's rest transform."""
    objects, data = piece_ownership(kind)
    name = heading(data.key)

    def surfaced(shape: PreviewMesh, mesh: bpy.types.Mesh) -> BlenderResult[bpy.types.Mesh]:
        def filled() -> bpy.types.Mesh:
            surface = bmesh.new()
            try:
                surface.loops.layers.uv.new("UVMap")
                match shape:
                    case PreviewMesh.PLANE:
                        bmesh.ops.create_grid(surface, x_segments=1, y_segments=1, size=1.0, calc_uvs=True)
                    case PreviewMesh.SPHERE:
                        bmesh.ops.create_uvsphere(surface, u_segments=48, v_segments=24, radius=0.92, calc_uvs=True)
                    case PreviewMesh.CUBE:
                        bmesh.ops.create_cube(surface, size=1.0, calc_uvs=True)
                surface.to_mesh(mesh)
            finally:
                surface.free()
            (mesh.shade_smooth if shape is PreviewMesh.SPHERE else mesh.shade_flat)()
            return mesh

        return BlenderEditFault.attempted(data.key, filled)

    def built() -> BlenderResult[bpy.types.Mesh | bpy.types.Camera | bpy.types.Light]:
        match kind:
            case Fixture.CAMERA:
                return BlenderEditFault.attempted(data.key, lambda: bpy.data.cameras.new(vacant(name, bpy.data.cameras.keys())))
            case Fixture.LAMP:
                return BlenderEditFault.attempted(data.key, lambda: bpy.data.lights.new(vacant(name, bpy.data.lights.keys()), type="SUN"))
            case PreviewMesh() as shape:
                return kept(BlenderEditFault.attempted(data.key, lambda: bpy.data.meshes.new(vacant(name, bpy.data.meshes.keys()))), lambda mesh: surfaced(shape, mesh))

    rest: Mapping[str, Value] = {
        "location": (0.0, 0.0, 0.0),
        "rotation_euler": (0.7854, 0.6155, 0.0) if kind is PreviewMesh.CUBE else (0.0, 0.0, 0.0),
        "scale": (1.04, 1.04, 1.0) if kind is PreviewMesh.PLANE else (0.98, 0.98, 0.98) if kind is PreviewMesh.CUBE else (1.0, 1.0, 1.0),
        "hide_render": isinstance(kind, PreviewMesh),
    }
    return data.made(identity, built).bind(
        lambda block: objects.made(identity, lambda: spawned(name, Some(block), scene.collection)).bind(lambda item: assigned(item, rest).map(lambda _: (item, block)))
    )


def rendering(group: bpy.types.NodeTree) -> bool:
    """Checks group and nested groups for a scene render read."""
    return any(
        isinstance(node, bpy.types.CompositorNodeRLayers) or (isinstance(node, bpy.types.CompositorNodeCryptomatteV2) and node.source == "RENDER") for tree in nested((group,)) for node in tree.nodes
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CAMERAS",
    "RESOLUTION",
    "Ambient",
    "CameraPose",
    "EntryPlacement",
    "Fitted",
    "Fixture",
    "Lamp",
    "Lighting",
    "Mirrored",
    "Piece",
    "PreviewEngine",
    "PreviewMesh",
    "PreviewScene",
    "PreviewStack",
    "Readback",
    "Staging",
    "Studio",
    "Viewpoint",
    "White",
    "appended",
    "piece",
    "rendering",
    "unlinked",
]
