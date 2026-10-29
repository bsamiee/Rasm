"""Runtime preview scenes: the owned surfaces, camera, and key light every preview render reuses, the lighting staging them, their compositor stacks, and readback."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from enum import auto, Enum
from functools import partial
import math
from pathlib import Path
from types import MappingProxyType
from typing import ClassVar, Final, Self

import bmesh
import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
from mathutils import Vector
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import filled, Tag
from blender.bpy.data.ids.ownership import kept, Ownership, Saved
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import rna, RnaPath, Struct, struct_at
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes import functions
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.desired import Interface, Socket
from blender.bpy.nodes.graphs import Expr, group_input
from blender.bpy.nodes.trees import DATA_TYPES, nested
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.compositor.effects import Enabled
from blender.bpy.render.compositor.entries import added_entry
from blender.bpy.render.compositor.inputs import inputs_of, PreviewFrame
from blender.bpy.render.outputs.formats import Colorspace, FileItem, FileItems, OutputFormat
from blender.bpy.render.outputs.framing import RESOLUTION
from blender.bpy.render.outputs.paths import Pathed
from blender.bpy.render.outputs.results import PRODUCTIONS
from blender.bpy.render.sessions.engines import ENGINE_SAMPLES
from blender.bpy.render.sessions.renders import native, reads_render, Render, RenderCall
from blender.bpy.scene.cameras import framed
from blender.bpy.scene.objects import corners, owned_object
from blender.bpy.scene.worlds import studio, swapped, WorldEnvironment
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason
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


class CameraPose(Enum):
    """Fixed preview camera: orthographic above the surface, panoramic at the world origin, or perspective into a world volume."""

    TOP = auto()
    PANORAMA = auto()
    INTERIOR = auto()


type Ambient = White | Studio
type Restoring = Callable[[], Outcome[None]]

# --- [CONSTANTS] ------------------------------------------------------------------------

CAMERAS: Final[Mapping[tuple[CameraPose, str | None], tuple[Mapping[RnaPath, Value], Mapping[RnaPath, Value]]]] = MappingProxyType({
    (CameraPose.TOP, None): ({("type",): "ORTHO", ("ortho_scale",): 2.0}, {("location",): (0.0, 0.0, 2.0), ("rotation_euler",): (0.0, 0.0, 0.0)}),
    (CameraPose.PANORAMA, "CYCLES"): ({("type",): "PANO", ("panorama_type",): "EQUIRECTANGULAR"}, {("location",): (0.0, 0.0, 0.0), ("rotation_euler",): (math.pi / 2.0, 0.0, 0.0)}),
    (CameraPose.PANORAMA, None): ({("type",): "PERSP", ("lens",): 12.0}, {("location",): (0.0, 0.0, 0.0), ("rotation_euler",): (1.3, 0.0, 0.0)}),
    (CameraPose.INTERIOR, None): ({("type",): "PERSP", ("lens",): 45.0}, {("location",): (0.0, -4.0, 0.0), ("rotation_euler",): (math.pi / 2.0, 0.0, 0.0)}),
})
SURFACES: Final = choices(item for item in Descriptor.declared(bpy.types.Material, ("preview_render_type",), bpy.types.EnumProperty).prop.enum_items if item.identifier in PreviewMesh.__members__)
_FIXED: Final[Mapping[RnaPath, Value]] = MappingProxyType({("location",): (0.0, 0.0, 0.0), ("rotation_euler",): (0.0, 0.0, 0.0), ("scale",): (1.0, 1.0, 1.0), ("hide_render",): False})
_POSES: Final[Mapping[PreviewMesh, tuple[tuple[float, float, float], tuple[float, float, float]]]] = MappingProxyType({
    PreviewMesh.SPHERE: ((0.0, 0.0, 0.0), (1.0, 1.0, 1.0)),
    PreviewMesh.CUBE: ((math.pi / 4.0, math.atan(math.sqrt(0.5)), 0.0), (0.98, 0.98, 0.98)),
    PreviewMesh.FLAT: ((0.0, 0.0, 0.0), (1.04, 1.04, 1.0)),
})

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

    @property
    def environment(self) -> Outcome[Mapping[RnaPath, Value]]:
        """Environment members lighting a studio light or a flat white backdrop."""
        match self.ambient:
            case White():
                return Ok({("lighting", "mode"): "COLOR", ("lighting", "color"): (1.0, 1.0, 1.0), ("lighting", "strength"): self.strength})
            case Studio(name=name):
                preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                return studio(name, preferences).map(lambda path: {("lighting", "mode"): "HDRI", ("lighting", "path"): path, ("lighting", "strength"): self.strength})

    @property
    def lamp(self) -> Mapping[RnaPath, Value]:
        """Key sun at key strength and aim as light object members, its type a `Light.type` item the host checks at the write."""
        return {("data", "type"): "SUN", ("data", "energy"): self.key, ("location",): (0.0, 0.0, 0.0), ("rotation_euler",): self.aim}


class Fitted(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Orthographic camera fitted to its subject's bounds, the subject linked into the preview scene for the render."""

    subject: bpy.types.Object


class Piece[D: bpy.types.Mesh | bpy.types.Camera | bpy.types.Light](msgspec.Struct, frozen=True, kw_only=True):
    """Owned preview object over owned data every preview render reuses, the data built by `created` from the session data and filled by `seeded`, the object placed at `rest`."""

    data: Ownership[D]
    created: Callable[[bpy.types.BlendData, str], D]
    rest: Mapping[RnaPath, Value]
    seeded: Callable[[D], Outcome[D]] = Ok

    @property
    def objects(self) -> Ownership[bpy.types.Object]:
        """Ownership of the piece's object, keyed as its data."""
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
            if getattr(entry, enabled) and not any(reads_render(node) for tree in nested((group,)) for node in tree.nodes)
        )


class PreviewStack(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Compositor entries of one preview render: the render input plane, ordered mirrored entries, and the placement adding entries after them."""

    head: Option[npt.NDArray[np.float32]] = Nothing
    mirrored: tuple[RoutedEntry, ...] = ()
    placing: Callable[[Identity, bpy.types.Scene, bpy.types.Scene, Option[PreviewFrame]], Outcome[Restoring]] = lambda _identity, _source, _scene, _preview: Ok(lambda: Ok(None))


class Staging(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Preview scene state of one render."""

    source: bpy.types.Scene
    size: tuple[int, int]
    world: Mapping[RnaPath, Value] | bpy.types.World
    lamp: Mapping[RnaPath, Value] | bpy.types.Light
    stack: PreviewStack = PreviewStack()
    transparent: bool = True
    surface: Option[PreviewMesh] = Nothing
    material: Option[bpy.types.Material] = Nothing
    viewpoint: CameraPose | Fitted = CameraPose.TOP
    value: bool = False
    preview: Option[PreviewFrame] = Nothing


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
        return view.formed(config, self.plane, self.colorspace).map(lambda image: msgspec.structs.replace(self, plane=image.pixels, colorspace=image.colorspace))


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

    # --- [STAGING]

    def made(self, identity: Identity) -> Outcome[bpy.types.Scene]:
        """Returns extension runtime preview scene."""
        return self.scene.made(identity, lambda: catch(bpy.data.scenes.new, exception=RuntimeError)("Preview").map_error(partial(Fault.of, Reason.REFUSED, self.scene.key)))

    def headed(self, identity: Identity, pixels: npt.NDArray[np.float32]) -> Outcome[bpy.types.NodeTree]:
        """Returns head group reading pixels through a runtime image."""
        height, width, _ = pixels.shape
        tag = Tag(space=Some(bpy.data.colorspace.working_space), alpha=Some("PREMUL"))

        def built() -> Outcome[bpy.types.Image]:
            created = catch(bpy.data.images.new, exception=RuntimeError)("Preview Input", width=width, height=height, alpha=True, float_buffer=True)
            return kept(created.map_error(partial(Fault.of, Reason.REFUSED, self.image.key)), lambda image: tag.applied(image).map(lambda _: image))

        def graphed(image: bpy.types.Image) -> Outcome[bpy.types.NodeTree]:
            source = Expr(kind=bpy.types.CompositorNodeImage, members={"image": IdName.of(image)}, socket="Image", shape=bpy.types.NodeSocketColor)
            face = Interface(outputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),))
            return (
                Result[None, Faults]
                .bind(catch(image.scale, exception=RuntimeError)(width, height).map_error(partial(Fault.of, Reason.REFUSED, self.image.key)), lambda _: filled(image, pixels))
                .bind(lambda _: functions.built(self.head.key, face, {"image": source}, bpy.types.CompositorNodeTree, name=Some("Preview Input")))
                .bind(lambda group: made(msgspec.structs.replace(group, saved=Saved.NEVER), identity))
            )

        return self.image.made(identity, built).bind(graphed)

    def stacked(self, identity: Identity, staging: Staging, scene: bpy.types.Scene, release: ExitStack) -> Outcome[Restoring]:
        """Appends head and mirrored entries with their source inputs, removed when `release` closes, then runs the stack's placement, answering its removal."""
        stack = staging.stack

        def copied(held: bpy.types.SceneCompositorEffect, entry: bpy.types.SceneCompositorEffect) -> Outcome[None]:
            return paired(inputs_of(held).bind(Snapshot.capture), inputs_of(entry)).bind(lambda pair: pair[0].apply(pair[1]).outcome).map(lambda _: None)

        with bpy.context.temp_override(scene=scene):
            return (
                stack.head
                .map(lambda pixels: self.headed(identity, pixels).bind(lambda group: added_entry(release, scene, group)).map(lambda _: None))
                .default_value(Ok(None))
                .bind(lambda _: accumulated(added_entry(release, scene, held.group).bind(lambda entry: copied(held.entry, entry)) for held in stack.mirrored))
                .bind(lambda _: stack.placing(identity, staging.source, scene, staging.preview))
            )

    @contextmanager
    def staged(self, identity: Identity, staging: Staging) -> Generator[Outcome[bpy.types.Scene]]:
        """Stages one render, running each release step at exit in reverse with their faults held for the extension."""
        release = ExitStack()

        def undone(request: object, step: Callable[[], object]) -> None:
            release.callback(_held, identity, lambda: catch(step, exception=RuntimeError)().map_error(partial(Fault.of, Reason.REFUSED, repr(request))))

        def lit(light: bpy.types.Object, data: bpy.types.Light) -> Outcome[None]:
            def relit() -> None:
                light.data = data

            undone(light, relit)
            match staging.lamp:
                case bpy.types.Light() as lamp:
                    return written(light, ("data",), lamp).map(lambda _: None)
                case members:
                    return assigned(light, members)

        def surfaced(scene: bpy.types.Scene, shapes: tuple[tuple[PreviewMesh, tuple[bpy.types.Object, bpy.types.Mesh]], ...]) -> Outcome[None]:
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
                *(catch(linked, exception=RuntimeError)(view.subject).map_error(partial(Fault.of, Reason.REFUSED, repr(scene))) for view in (staging.viewpoint,) if isinstance(view, Fitted)),
            )).map(lambda _: None)

        def aimed(scene: bpy.types.Scene, camera: bpy.types.Object, data: bpy.types.Camera) -> Outcome[None]:
            match staging.viewpoint:
                case CameraPose() as pose:
                    lens, transform = CAMERAS.get((pose, staging.source.render.engine), CAMERAS[pose, None])
                    return paired(assigned(data, lens), assigned(camera, transform)).map(lambda _: None)
                case Fitted(subject=subject):
                    with bpy.context.temp_override(scene=scene):
                        depsgraph = bpy.context.evaluated_depsgraph_get()
                        depsgraph.update()
                        bounds = corners((subject,), depsgraph).astype(np.float32)
                    rotation = Vector((-0.55, 0.8, -0.5)).to_track_quat("-Z", "Y").to_euler()
                    return paired(assigned(data, {("type",): "ORTHO"}), assigned(camera, {("rotation_euler",): (rotation.x, rotation.y, rotation.z)})).bind(lambda _: framed(camera, bounds, depsgraph))

        def environment(scene: bpy.types.Scene) -> Outcome[None]:
            match staging.world:
                case bpy.types.World() as held:
                    return written(scene, ("world",), held).map(lambda _: None)
                case values:
                    return swapped(self.world, values, scene, identity).map(lambda _: None)

        def arranged(
            scene: bpy.types.Scene,
            pieces: tuple[tuple[tuple[PreviewMesh, tuple[bpy.types.Object, bpy.types.Mesh]], ...], tuple[bpy.types.Object, bpy.types.Camera], tuple[bpy.types.Object, bpy.types.Light]],
        ) -> Outcome[bpy.types.Scene]:
            shapes, (camera, lens), (light, data) = pieces
            return (
                paired(surfaced(scene, shapes), written(scene, ("camera",), camera))
                .bind(lambda _: paired(lit(light, data), aimed(scene, camera, lens), environment(scene)))
                .bind(lambda _: self.stacked(identity, staging, scene, release))
                .map(lambda restoring: release.callback(_held, identity, restoring))
                .map(lambda _: scene)
            )

        def prepared(scene: bpy.types.Scene) -> Outcome[bpy.types.Scene]:
            source = staging.source
            settings: dict[RnaPath, Value] = {
                ("render", "engine"): source.render.engine,
                ("render", "resolution_x"): staging.size[0],
                ("render", "resolution_y"): staging.size[1],
                ("render", "resolution_percentage"): 100,
                ("render", "film_transparent"): staging.transparent,
                ("render", "use_compositing"): True,
                ("render", "use_sequencer"): False,
                ("render", "use_multiview"): False,
                ("render", "use_motion_blur"): False,
                ("render", "use_border"): False,
                ("render", "use_stamp"): False,
                ("frame_current",): source.frame_current,
            }
            owners: tuple[tuple[Struct, Struct], ...] = ((source.eevee, scene.eevee), (source.cycles, scene.cycles), (source.display, scene.display))  # type: ignore[assignment, attr-defined]  # ty: ignore[invalid-assignment, unresolved-attribute]
            engines = accumulated(Snapshot.capture(held).bind(lambda snapshot: snapshot.apply(own).outcome) for held, own in owners)
            previewed = accumulated(
                Descriptor.resolve(bpy.types.Scene, preview, bpy.types.Property).bind(
                    lambda held: struct_at(source, held.owner).map(lambda owner: (final, held.read(owner))).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(source), member=rna(held.owner)))
                )
                for final, preview in Option.of_optional(ENGINE_SAMPLES.get(source.render.engine)).to_list()
            )
            return (
                paired(assigned(scene, settings), engines)
                .bind(lambda _: previewed.bind(lambda pairs: assigned(scene, dict(pairs))))
                .bind(
                    lambda _: paired(
                        accumulated(_SURFACES[shape].placed(identity, scene).map(lambda pair: (shape, pair)) for shape in PreviewMesh), _CAMERA.placed(identity, scene), _LAMP.placed(identity, scene)
                    )
                )
                .bind(lambda pieces: arranged(scene, pieces))
            )

        with release:
            yield self.made(identity).bind(prepared)

    # --- [READBACK]

    def rendered(self, identity: Identity, staging: Staging) -> Outcome[Readback]:
        """Renders staging and reads image and value AOV from File Output files."""
        size = staging.stack.head.map(lambda pixels: (pixels.shape[1], pixels.shape[0])).default_value(staging.size)

        def declared(scene: bpy.types.Scene, value: Option[str], output: OutputFormat, *, paths: Mapping[str, Path]) -> Outcome[None]:
            image = group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor)
            domain = Expr(kind=bpy.types.CompositorNodeBlankImage, inputs=(("Size", size),), socket="Image", shape=bpy.types.NodeSocketColor, label=Some("domain"))
            passes = value.map(lambda name: {"value": name, "coverage": "Alpha"}).default_value({})
            sources: dict[str, Expr] = {
                "image": Expr(kind=bpy.types.ShaderNodeMixRGB, inputs=(("Fac", 1.0), ("Color1", domain), ("Color2", image)), socket="Color", shape=bpy.types.NodeSocketColor),
                **{key: Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene)}, socket=socket) for key, socket in passes.items()},
            }
            content = FileItems(items=tuple(FileItem(name=paths[key].stem, socket_type=DATA_TYPES[term.shape]) for key, term in sources.items()))
            files = Expr(
                kind=bpy.types.CompositorNodeOutputFile,
                inputs=tuple((stored.name, term) for stored, term in zip(content.items, sources.values())),
                members={Pathed.directory.path: bpy.app.tempdir, Pathed.file_name.path: "", **output.members(("format",))},
                content=Some(content),
            )
            capture = functions.built(
                self.capture.key, Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: image.keeping(files)}, bpy.types.CompositorNodeTree, name=Some("Preview Capture")
            ).map(lambda group: msgspec.structs.replace(group, saved=Saved.NEVER))
            return capture.bind(lambda group: made(group, identity)).bind(lambda tree: added_entry(release, scene, tree)).map(lambda _: None)

        def stored() -> Outcome[OutputFormat]:
            linear = Colorspace(name=bpy.data.colorspace.working_space)
            return OutputFormat.exr(Output.DATA, layered=False).map(lambda held: msgspec.structs.replace(held, color_mode="RGBA", color_management="OVERRIDE", linear_colorspace_settings=linear))

        def covered(values: npt.NDArray[np.float32], coverage: npt.NDArray[np.float32]) -> Option[npt.NDArray[np.float32]]:
            mask = coverage[..., 0] > Readback.covered
            return Some(values[..., 0][mask] / coverage[..., 0][mask])

        def readback(paths: Mapping[str, Path]) -> Outcome[Readback]:
            def read(key: str, channels: tuple[str, ...]) -> Outcome[npt.NDArray[np.float32]]:
                return Multilayer.read(paths[key]).bind(lambda file: file.picked(("",), channels))

            sampled = paired(read("value", ("R",)), read("coverage", ("R",))).map(lambda pair: covered(*pair)) if staging.value else Ok(Nothing)
            return paired(read("image", ("R", "G", "B", "A")), sampled).map(lambda pair: Readback(plane=pair[0], colorspace=bpy.data.colorspace.working_space, values=pair[1]))

        def captured(scene: bpy.types.Scene) -> Outcome[Readback]:
            paths = {key: Path(bpy.app.tempdir, f"{scene.session_uid}_{self.capture.key}_{key}.exr") for key in ("image", "value", "coverage")}
            value = identity.identifier(self.value).map(Some) if staging.value else Ok(Nothing)
            release.callback(_held, identity, partial(_unlinked, paths))
            return (
                paired(value, stored())
                .bind(lambda parts: declared(scene, *parts, paths=paths))
                .bind(lambda _: _unlinked(paths))
                .bind(lambda _: self.render(identity, scene, RenderCall()))
                .bind(lambda _: readback(paths))
            )

        with ExitStack() as release, self.staged(identity, staging) as staged:
            return staged.bind(captured)

    def sampled(self, identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], pixels: npt.NDArray[np.float32]) -> Outcome[npt.NDArray[np.float32]]:
        """Renders float pixels through entries without adding formation."""
        height, width, channels = pixels.shape
        lighting, low = PreviewLighting(), RESOLUTION.prop.hard_min
        stack = PreviewStack(head=Some(pixels), mirrored=tuple(RoutedEntry(entry=entry, group=group) for entry in entries for group in Option.of_optional(entry.node_group).to_list()))
        return lighting.environment.bind(lambda world: self.rendered(identity, Staging(source=scene, size=(max(width, low), max(height, low)), world=world, lamp=lighting.lamp, stack=stack))).map(
            lambda readback: readback.plane[..., :channels]
        )

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns owned data rows and world attachment."""
        return Ok((
            PRODUCTIONS,
            self.scene,
            self.world,
            self.image,
            self.head,
            self.capture,
            self.routes,
            WorldEnvironment.attachment,
            *(row for piece in _SURFACES.values() for row in (piece.objects, piece.data)),
            _CAMERA.objects,
            _CAMERA.data,
            _LAMP.objects,
            _LAMP.data,
        ))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _held(identity: Identity, step: Callable[[], Outcome[object]]) -> None:
    """Runs one release step answering an outcome, holding its faults for the extension."""
    for faults in step().swap().to_option().to_list():
        identity.reports.hold(faults)


def _unlinked(paths: Mapping[str, Path]) -> Outcome[None]:
    """Deletes capture files a render writes, absent ones included."""
    return accumulated(catch(path.unlink, exception=OSError)(missing_ok=True).map_error(partial(Fault.of, Reason.UNWRITABLE, str(path))) for path in paths.values()).map(lambda _: None)


# --- [COMPOSITION] ----------------------------------------------------------------------

_SURFACES: Final[Mapping[PreviewMesh, Piece[bpy.types.Mesh]]] = MappingProxyType({
    shape: Piece(
        data=Ownership(kind=bpy.types.Mesh, key=f"preview_{shape.name.lower()}", saved=Saved.NEVER),
        created=lambda data, name: data.meshes.new(name),
        seeded=shape.filled,
        rest={("location",): (0.0, 0.0, 0.0), ("rotation_euler",): rotation, ("scale",): scale, ("hide_render",): True},
    )
    for shape, (rotation, scale) in _POSES.items()
})
_CAMERA: Final[Piece[bpy.types.Camera]] = Piece(data=Ownership(kind=bpy.types.Camera, key="preview_camera", saved=Saved.NEVER), created=lambda data, name: data.cameras.new(name), rest=_FIXED)
_LAMP: Final[Piece[bpy.types.Light]] = Piece(data=Ownership(kind=bpy.types.Light, key="preview_lamp", saved=Saved.NEVER), created=lambda data, name: data.lights.new(name, type="SUN"), rest=_FIXED)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CAMERAS",
    "SURFACES",
    "Ambient",
    "CameraPose",
    "Fitted",
    "Piece",
    "PreviewLighting",
    "PreviewMesh",
    "PreviewScene",
    "PreviewStack",
    "Readback",
    "Restoring",
    "RoutedEntry",
    "Staging",
    "Studio",
    "White",
]
