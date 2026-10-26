"""Audits and measures before render dispatch."""

import datetime
import functools
from functools import partial
from itertools import groupby, starmap
from math import prod
from typing import ClassVar, Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Stock
from blender.bpy.commands.operators import Answer, Call, Done, Empty, InvokePopup, Operator
from blender.bpy.data.ids.evaluation import evaluated, Instance
from blender.bpy.data.ids.store import resolved
from blender.bpy.data.rna import conditions
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.files.blends import dependencies, Dependency
from blender.bpy.nodes.trees import nested
from blender.bpy.render.outputs.paths import OutputPath, Pathed
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.layout.sections import Section, Single
from blender.records.faults import accumulated, Fault, Outcome, paired
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


class Shader(bpy.types.PropertyGroup):
    """Shader node class to list material users of."""

    kind: bpy.props.EnumProperty(name="Shader", description="Shader node class to find", items=lambda _owner, _context: shader_rows(shader_kinds()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class JobScope(msgspec.Struct, frozen=True, kw_only=True):
    """Planned job render scope: the scene values its job file takes before writing, and the plan positions of scopes whose frames it reads."""

    scene: str
    camera: JobCamera
    layer: str | None = None
    values: Snapshot | None = None
    prerequisites: tuple[int, ...] = ()


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

    @property
    def operators(self) -> tuple[Operator[Empty] | Operator[Shader], ...]:
        """Audit operators in section order."""
        return (
            Operator(icon=Some("CHECKMARK"), form=Empty, run=self.audit_file, undo=False),
            Operator(form=Empty, run=self.list_users, undo=False),
            Operator[Shader](form=Shader, run=self.list_shader_users, invoke=Some(InvokePopup.SEARCH), undo=False, primary=Some("kind")),
            Operator(form=Empty, run=self.measure_view_layer, undo=False),
            Operator(icon=Some("FILE_BLEND"), form=Empty, run=self.open_library, undo=False),
        )

    @staticmethod
    def audit_file(_call: Call[Empty]) -> Outcome[Answer]:
        """Reports missing files, empty material slots, and empty group or image nodes."""
        return Ok(Done(skipped=audited().swap().to_option()))

    @staticmethod
    def list_users(call: Call[Empty]) -> Outcome[Answer]:
        """Reports the data-blocks using the active data-block."""
        return active(call.context).map(lambda block: Done(notes=tuple(("INFO", repr(user)) for user in users(block))))

    @staticmethod
    def list_shader_users(call: Call[Shader]) -> Outcome[Answer]:
        """Reports the materials using a shader node, groups included."""
        kind = call.properties.kind
        return (
            Option
            .of_optional(next((shader for shader in shader_kinds() if shader.__name__ == kind), None))
            .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject="bpy.types", member=kind),))
            .map(lambda shader: Done(notes=tuple(("INFO", repr(user)) for user in shader_users(shader))))
        )

    @staticmethod
    def measure_view_layer(call: Call[Empty]) -> Outcome[Answer]:
        """Reports the evaluated geometry and image memory of the view layer."""
        return call.root(bpy.types.ViewLayer).bind(measured).map(lambda measure: Done(notes=tuple(("INFO", text) for text in measure.notes)))

    @staticmethod
    def open_library(call: Call[Empty]) -> Outcome[Answer]:
        """Opens the library of the active data-block in a new Blender process."""

        def opened(block: bpy.types.ID) -> Outcome[Answer]:
            match block if isinstance(block, bpy.types.Library) else block.library:
                case bpy.types.Library() as library:
                    return detached((bpy.app.binary_path, bpy.path.abspath(library.filepath)), call.identity.folder / "library.log").map(lambda _: Done())
                case None:
                    return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(block), member="library"),))

        return active(call.context).bind(opened)

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Returns audit operators for registration."""
        return Ok(self.operators)

    def section(self) -> Section:
        """Returns section of audit buttons and Find Missing Files."""
        return Single(key="preflight", title="Preflight", rows=(*(OperatorButton(target=row) for row in self.operators), OperatorButton(target=Stock(operator=bpy.ops.file.find_missing_files))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [AUDIT]


def active(context: bpy.types.Context) -> Outcome[bpy.types.ID]:
    """Reads editor `context.id`, absent when unset."""
    return catch(lambda: context.id, exception=AttributeError)().to_option().bind(Option.of_optional).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject="bpy.context", member="id"),))


def audited() -> Outcome[None]:
    """Accumulates missing files, empty material slots, and empty group or image nodes."""

    def absent(owner: object, key: str) -> Outcome[None]:
        """Returns absent fault naming owner and key."""
        return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(owner), member=key),))

    def lost(missing: tuple[Dependency, ...]) -> Outcome[None]:
        """Resolves missing dependency owners in one pass, one fault per path."""
        return resolved(dependency.owner for dependency in missing).bind(
            lambda owners: accumulated(absent(owner, str(dependency.path)) for dependency, owner in zip(missing, owners)).map(lambda _: None)
        )

    trees = tuple(dict.fromkeys((*(tree for holder in (*bpy.data.materials, *bpy.data.worlds, *bpy.data.lights) for tree in Option.of_optional(holder.node_tree).to_list()), *bpy.data.node_groups)))
    nodes = tuple(node for tree in trees for node in tree.nodes)
    return accumulated((
        lost(tuple(dependency for dependency in dependencies() if not dependency.present)),
        *(absent(item, str(index)) for item in bpy.data.objects for index, slot in enumerate(item.material_slots) if slot.material is None),
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


@functools.cache
def shader_rows(kinds: tuple[type[bpy.types.ShaderNode], ...]) -> tuple[tuple[str, str, str], ...]:
    """Returns enum rows of shader classes, cached so the host's dynamic enum reads keep their strings."""
    return tuple((kind.__name__, kind.bl_rna.name, kind.bl_rna.description) for kind in kinds)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]


def shader_users(kind: type[bpy.types.ShaderNode]) -> tuple[bpy.types.Material, ...]:
    """Returns materials using shader class, groups included."""
    return tuple(
        material for material in bpy.data.materials for tree in Option.of_optional(material.node_tree).to_list() if any(isinstance(node, kind) for held in nested((tree,)) for node in held.nodes)
    )


# --- [MEASURES]


def measured(view_layer: bpy.types.ViewLayer) -> Outcome[Measure]:
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


def cameras(scene: bpy.types.Scene, scope: JobScope, frames: Frames) -> Outcome[None]:
    """Accumulates frames without camera and missing or non-camera bindings."""
    match scope.camera:
        case Bound(camera=camera, markers=markers) as bound:
            numbers = frames.numbers
            gaps = tuple(next(run) for missing, run in groupby(numbers, key=lambda frame: bound.at(frame).is_none()) if missing)
            bindings = (*(("scene camera", name) for name in Option.of_optional(camera).to_list()), *((f"marker at frame {marker.frame}", marker.camera) for marker in markers))
        case Fixed(camera=camera):
            gaps, bindings = (), (("fixed camera", camera),)

    def checked(binding: str, name: str) -> Outcome[None]:
        def native(bound_object: bpy.types.Object) -> Outcome[None]:
            return Ok(None) if isinstance(bound_object.data, bpy.types.Camera) else Error((Fault(reason=BlenderReason.UNMET, subject=NotCamera(binding=binding, kind=bound_object.type).message),))

        return Option.of_optional(bpy.data.objects.get(name)).map(native).default_with(lambda: Error((Fault(reason=BlenderReason.ABSENT, subject=repr(bpy.data.objects), member=name),)))

    return accumulated((*(Error((Fault(reason=BlenderReason.ABSENT, subject=repr(scene), member=str(frame)),)) for frame in gaps), *starmap(checked, bindings))).map(lambda _: None)


# --- [DISPATCH]


def ranged(scene: bpy.types.Scene) -> Frames:
    """Returns inclusive frame range and step of scene."""
    return Frames(start=scene.frame_start, end=scene.frame_end, step=scene.frame_step)


def templated(path: OutputPath, scope: JobScope, scene: bpy.types.Scene, frames: Frames) -> Outcome[None]:
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
        accumulated(Error((Fault(reason=BlenderReason.UNMET, subject=Unvaried(field=field).message),)) for field in varied),
    ).map(lambda _: None)


def checked(pathed: Pathed, scopes: tuple[JobScope, ...], day: datetime.date) -> Outcome[tuple[Measure, ...]]:
    """Audits every scope, then measures each distinct view layer."""
    planned = tuple((scope, scene, ranged(scene)) for scope in scopes for scene in (bpy.data.scenes[scope.scene],))

    layers = tuple(
        dict.fromkeys(
            layer for scope, scene, _frames in planned for layer in ((scene.view_layers[scope.layer],) if scope.layer is not None else tuple(layer for layer in scene.view_layers if layer.use))
        )
    )
    return paired(
        paired(audited(), accumulated(cameras(scene, scope, frames) for scope, scene, frames in planned)),
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
    "shader_rows",
    "shader_users",
    "templated",
    "users",
]
