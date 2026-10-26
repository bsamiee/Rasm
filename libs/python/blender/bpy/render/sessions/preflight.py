"""Audits and measures before render dispatch."""

from collections.abc import Callable
import datetime
from functools import partial
from itertools import groupby, starmap
from math import prod
from pathlib import Path
from typing import Annotated, ClassVar, Final, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Stock
from blender.bpy.commands.operators import Done, Empty, InvokePopup, Operator
from blender.bpy.data.ids.evaluation import evaluated, Instance
from blender.bpy.data.ids.store import resolved
from blender.bpy.data.rna import conditions
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderResult, BlenderUnmetFault
from blender.bpy.files.blends import dependencies, Dependency
from blender.bpy.nodes.trees import nested
from blender.bpy.render.outputs.paths import OutputPath, Pathed
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.layout.sections import Section, Single
from blender.records.faults import accumulated, DomainFault, paired
from blender.rendering.batch.processes import detached
from blender.rendering.batch.queue import Bound, Fixed, Frames, JobCamera
from blender.rendering.outputs.naming import Count, Number, Output, Slot, Word

# --- [CONSTANTS] ------------------------------------------------------------------------

PROJECTION: Final = Descriptor.declared(bpy.types.Camera, "type", bpy.types.EnumProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class NotCamera(msgspec.Struct, frozen=True, kw_only=True):
    """Camera binding naming non-camera object."""

    binding: str
    kind: str

    @property
    def message(self) -> str:
        """Requirement text."""
        return f"Requires camera object for {self.binding}, got {self.kind}"


class Unvaried(msgspec.Struct, frozen=True, kw_only=True):
    """Output template lacking a field its scope varies."""

    field: Word | Count

    @property
    def message(self) -> str:
        """Requirement text."""
        return f"Requires {{{self.field}}} in output template to name one file per {self.field}"


class Measure(msgspec.Struct, frozen=True, kw_only=True):
    """Evaluated instance geometry and loaded image memory for one view layer."""

    objects: int
    vertices: int
    faces: int
    triangles: int
    images: int
    image_bytes: int

    @property
    def notes(self) -> tuple[str, ...]:
        """Counts and image memory as report lines."""
        return (f"Objects {self.objects:,}", f"Vertices {self.vertices:,}", f"Faces {self.faces:,}", f"Triangles {self.triangles:,}", f"Images {self.images:,} ({self.image_bytes / 2**20:,.1f} MiB)")


class Shader(msgspec.Struct, frozen=True, kw_only=True):
    """Shader node class to list material users of."""

    kind: Annotated[
        str, msgspec.Meta(title="Shader", description="Shader node class to find"), Choices(lambda *_: ((kind.__name__, kind.bl_rna.name, kind.bl_rna.description) for kind in shader_kinds()))  # ty: ignore[unresolved-attribute]
    ]


class JobScope(msgspec.Struct, frozen=True, kw_only=True):
    """Planned job render scope, a follow-up's key on the job reading another scope's rendered frames."""

    scene: str
    camera: JobCamera
    layer: str | None = None
    follow: Key | None = None

    @property
    def base(self) -> Self:
        """Render scope a follow-up job reads."""
        return msgspec.structs.replace(self, follow=None)

    def followed(self, scopes: tuple["JobScope", ...]) -> bool:
        """Whether a follow-up job among scopes reads this render scope."""
        return any(other.base == self for other in scopes if other.follow is not None)


class Preflight(msgspec.Struct, frozen=True, kw_only=True):
    """Audit operators and dispatch condition."""

    # --- [CONDITIONS]

    condition: ClassVar[conditions.AllOf] = conditions.AllOf(
        conditions=(
            Pathed.unsaved,
            conditions.Count(
                collection=Descriptor.declared(bpy.types.Scene, "view_layers", bpy.types.CollectionProperty),
                where=Some(conditions.Setting(descriptor=Descriptor.declared(bpy.types.ViewLayer, "use", bpy.types.BoolProperty))),
            ),
            conditions.Folder(path=Descriptor.declared(bpy.types.Scene, "render.filepath", bpy.types.StringProperty), least=0),
            conditions.Job(kind="RENDER"),
        )
    )

    # --- [COMMANDS]

    operators: ClassVar[tuple[Operator[Empty] | Operator[Shader], ...]] = (
        Operator(
            key="audit",
            title="Audit File",
            description="Report missing files, empty material slots, and empty group or image nodes",
            icon=Some("CHECKMARK"),
            form=Empty,
            run=lambda _: Ok(Done(skipped=audited().swap().to_option())),
            undo=False,
        ),
        Operator(
            key="users",
            title="List Users",
            description="Report users of active data-block",
            form=Empty,
            run=lambda call: active(call.context).map(lambda block: Done(notes=tuple(("INFO", repr(user)) for user in users(block)))),
            undo=False,
        ),
        Operator[Shader](
            key="shader",
            title="List Shader Users",
            description="Report materials using shader node, groups included",
            form=Shader,
            run=lambda call: listed(call.record.kind),
            invoke=Some(InvokePopup.SEARCH),
            undo=False,
            primary=Some(*msgspec.structs.fields(Shader)),
        ),
        Operator[Empty](
            key="measure",
            title="Measure View Layer",
            description="Report evaluated geometry and image memory",
            form=Empty,
            run=lambda call: call.root(bpy.types.ViewLayer).bind(measured).map(lambda measure: Done(notes=tuple(("INFO", text) for text in measure.notes))),
            undo=False,
        ),
        Operator(
            key="library_open",
            title="Open Library",
            description="Open active data-block's library in new Blender process",
            icon=Some("FILE_BLEND"),
            form=Empty,
            run=lambda call: active(call.context).bind(lambda block: opened(block, call.identity.folder)),
            undo=False,
        ),
    )

    def acquisitions(self, _identity: Identity, /) -> BlenderResult[Acquisitions]:
        """Returns audit operators for registration."""
        return Ok(self.operators)

    def section(self) -> Section:
        """Returns section of audit buttons and Find Missing Files."""
        return Single(key="preflight", title="Preflight", rows=(*(OperatorButton(target=row) for row in self.operators), OperatorButton(target=Stock(operator=bpy.ops.file.find_missing_files))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [AUDIT]


def active(context: bpy.types.Context) -> BlenderResult[bpy.types.ID]:
    """Reads editor `context.id`, absent when unset."""
    return catch(lambda: context.id, exception=AttributeError)().to_option().bind(Option.of_optional).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="id"),))


def opened(block: bpy.types.ID, folder: Path) -> BlenderResult[Done]:
    """Opens block library in detached Blender process."""
    match block if isinstance(block, bpy.types.Library) else block.library:
        case bpy.types.Library() as library:
            return detached((bpy.app.binary_path, bpy.path.abspath(library.filepath)), folder / "library.log").map(lambda _: Done())
        case None:
            return Error((BlenderAbsentFault(owner=repr(block), key="library"),))


def listed(kind: str) -> BlenderResult[Done]:
    """Reports each material user of chosen shader class."""
    return (
        Option
        .of_optional(next((shader for shader in shader_kinds() if shader.__name__ == kind), None))
        .to_result_with(lambda: (BlenderAbsentFault(owner="bpy.types", key=kind),))
        .map(lambda shader: Done(notes=tuple(("INFO", repr(user)) for user in shader_users(shader))))
    )


def audited() -> BlenderResult[None]:
    """Accumulates missing files, empty material slots, and empty group or image nodes."""

    def absent(owner: object, key: str | int) -> BlenderResult[None]:
        """Returns absent fault naming owner and key."""
        return Error((BlenderAbsentFault(owner=repr(owner), key=key),))

    def lost(missing: tuple[Dependency, ...]) -> BlenderResult[None]:
        """Resolves missing dependency owners in one pass, one fault per path."""
        return resolved(dependency.owner for dependency in missing).bind(
            lambda owners: accumulated(absent(owner, str(dependency.path)) for dependency, owner in zip(missing, owners)).map(lambda _: None)
        )

    trees = tuple(dict.fromkeys((*(tree for holder in (*bpy.data.materials, *bpy.data.worlds, *bpy.data.lights) for tree in Option.of_optional(holder.node_tree).to_list()), *bpy.data.node_groups)))
    nodes = tuple(node for tree in trees for node in tree.nodes)
    return accumulated((
        lost(tuple(dependency for dependency in dependencies() if not dependency.present)),
        *(absent(item, index) for item in bpy.data.objects for index, slot in enumerate(item.material_slots) if slot.material is None),
        *(
            absent(node, "node_tree")
            for node in nodes
            if isinstance(node, bpy.types.ShaderNodeGroup | bpy.types.CompositorNodeGroup | bpy.types.GeometryNodeGroup | bpy.types.TextureNodeGroup) and node.node_tree is None
        ),
        *(absent(node, "image") for node in nodes if isinstance(node, bpy.types.ShaderNodeTexImage | bpy.types.ShaderNodeTexEnvironment) and node.image is None),
    )).map(lambda _: None)


def users(block: bpy.types.ID) -> tuple[bpy.types.ID, ...]:
    """Returns IDs using block."""
    return tuple(bpy.data.user_map(subset=(block,))[block])


def shader_kinds() -> tuple[type[bpy.types.ShaderNode], ...]:
    """Returns every registered `ShaderNode` subclass in `bpy.types`."""
    return tuple(kind for kind in map(partial(getattr, bpy.types), dir(bpy.types)) if isinstance(kind, type) and issubclass(kind, bpy.types.ShaderNode) and kind is not bpy.types.ShaderNode)


def shader_users(kind: type[bpy.types.ShaderNode]) -> tuple[bpy.types.Material, ...]:
    """Returns materials using shader class, groups included."""
    return tuple(
        material for material in bpy.data.materials for tree in Option.of_optional(material.node_tree).to_list() if any(isinstance(node, kind) for held in nested((tree,)) for node in held.nodes)
    )


# --- [MEASURES]


def measured(view_layer: bpy.types.ViewLayer) -> BlenderResult[Measure]:
    """Counts evaluated instance geometry and image memory of view layer."""

    def counted(depsgraph: bpy.types.Depsgraph) -> Measure:
        """Counts instance meshes and images of depsgraph."""
        instances = Instance.read(depsgraph)
        meshes = tuple(data for instance in instances for data in (instance.source.evaluated_get(depsgraph).data,) if isinstance(data, bpy.types.Mesh))
        images = tuple(image for block in depsgraph.ids for image in (block.original,) if isinstance(image, bpy.types.Image))
        return Measure(
            objects=len(instances),
            vertices=sum(len(mesh.vertices) for mesh in meshes),
            faces=sum(len(mesh.polygons) for mesh in meshes),
            triangles=sum(len(mesh.loop_triangles) for mesh in meshes),
            images=len(images),
            image_bytes=sum(prod(image.size) * image.channels * (4 if image.is_float else 1) for image in images),  # type: ignore[call-overload]
        )

    return evaluated(view_layer).map(counted)


# --- [CAMERAS]


def cameras(scene: bpy.types.Scene, scope: JobScope, frames: Frames, refused: frozenset[str] = frozenset()) -> BlenderResult[None]:
    """Accumulates frames without camera, missing or non-camera bindings, and projections the scope refuses."""
    match scope.camera:
        case Bound(camera=camera, markers=markers) as bound:
            numbers = frames.numbers
            gaps = tuple(next(run) for missing, run in groupby(numbers, key=lambda frame: bound.at(frame).is_none()) if missing)
            bindings = (*(("scene camera", name) for name in Option.of_optional(camera).to_list()), *((f"marker at frame {marker.frame}", marker.camera) for marker in markers))
        case Fixed(camera=camera):
            gaps, bindings = (), (("fixed camera", camera),)

    def verified(accepted: tuple[str, ...]) -> BlenderResult[None]:
        """Checks object and projection of each camera binding."""

        def checked(binding: str, name: str) -> BlenderResult[None]:
            """Returns object, camera, or projection fault."""

            def native(bound_object: bpy.types.Object) -> BlenderResult[None]:
                camera = bound_object.data
                return (
                    Error((BlenderUnmetFault(requirement=NotCamera(binding=binding, kind=bound_object.type)),))
                    if not isinstance(camera, bpy.types.Camera)
                    else Error((DomainFault(subject=f"{binding}: {camera.path_from_id('type')}", value=camera.type, accepted=", ".join(accepted)),))
                    if camera.type in refused
                    else Ok(None)
                )

            return Option.of_optional(bpy.data.objects.get(name)).map(native).default_with(lambda: Error((BlenderAbsentFault(owner=repr(bpy.data.objects), key=name),)))

        return accumulated((*(Error((BlenderAbsentFault(owner=repr(scene), key=frame),)) for frame in gaps), *starmap(checked, bindings))).map(lambda _: None)

    return verified(tuple(item.identifier for item in PROJECTION.prop.enum_items if item.identifier not in refused))


# --- [DISPATCH]


def ranged(scene: bpy.types.Scene) -> Frames:
    """Returns inclusive frame range and step of scene."""
    return Frames(start=scene.frame_start, end=scene.frame_end, step=scene.frame_step)


def templated(path: OutputPath, scope: JobScope, scene: bpy.types.Scene, frames: Frames) -> BlenderResult[None]:
    """Accumulates output path faults and missing layer, pass, or frame template fields."""
    layers = Option.of_optional(scope.layer).map(lambda layer: (layer,)).default_with(lambda: tuple(layer.name for layer in scene.view_layers if layer.use))
    slots = path.name.slots
    varied = (
        *((Word.LAYER,) if len(layers) > 1 and Slot(key=Word.LAYER) not in slots else ()),
        *((Word.PASS,) if not slots & {Slot(key=Word.PASS), Slot(key=Word.OUTPUT)} else ()),
        *((Count.FRAME,) if len(frames.numbers) > 1 and not any(isinstance(part, Number) and part.key is Count.FRAME for part in slots) else ()),
    )
    return paired(
        accumulated(path.written(layer, output, Nothing, frames.start, "") for layer in layers for output in Output),
        accumulated(Error((BlenderUnmetFault(requirement=Unvaried(field=field)),)) for field in varied),
    ).map(lambda _: None)


def checked(pathed: Pathed, scopes: tuple[JobScope, ...], day: datetime.date, refused: Callable[[JobScope], frozenset[str]] = lambda _: frozenset()) -> BlenderResult[tuple[Measure, ...]]:
    """Audits every scope against the camera projections it refuses, then measures each distinct view layer."""
    planned = tuple(
        (scope, scene, msgspec.structs.replace(ranged(scene), step=1) if scope.follow is None and scope.followed(scopes) else ranged(scene))
        for scope in scopes
        for scene in (bpy.data.scenes[scope.scene],)
    )

    layers = tuple(
        dict.fromkeys(
            layer for scope, scene, _frames in planned for layer in ((scene.view_layers[scope.layer],) if scope.layer is not None else tuple(layer for layer in scene.view_layers if layer.use))
        )
    )
    return paired(
        paired(audited(), accumulated(cameras(scene, scope, frames, refused(scope)) for scope, scene, frames in planned)),
        accumulated(pathed.path(scene, day).bind(partial(templated, scope=scope, scene=scene, frames=frames)) for scope, scene, frames in planned),
    ).bind(lambda _: accumulated(map(measured, layers)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "PROJECTION",
    "JobScope",
    "Measure",
    "NotCamera",
    "Preflight",
    "Shader",
    "Unvaried",
    "audited",
    "cameras",
    "checked",
    "measured",
    "ranged",
    "shader_kinds",
    "shader_users",
    "templated",
    "users",
]
