"""Audits and measures before render dispatch, and the condition a dispatch requires."""

import functools
from functools import partial
from math import prod
import os
from pathlib import Path
from typing import ClassVar, override

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Placement
from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.ids.evaluation import evaluated, instances
from blender.bpy.data.ids.store import embedded, resolved
from blender.bpy.data.rna import conditions
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import definition, rooted, Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.files.sessions import dependencies, Dependency
from blender.bpy.nodes.trees import nested
from blender.bpy.render.outputs.paths import Pathed
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.layout.sections import Section
from blender.records.faults import accumulated, Fault, Outcome
from blender.records.stores import existing
from blender.rendering.batch.processes import detached

# --- [MODELS] ---------------------------------------------------------------------------


class _Folder(conditions.Condition, frozen=True, kw_only=True):
    """Writable output folder of the scene frame path."""

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return Option.of_optional(context.scene).map(lambda scene: existing(Path(scene.render.frame_path()))).filter(lambda folder: os.access(folder, os.W_OK)).is_some()

    @property
    @override
    def message(self) -> str:
        return f"writable {definition(bpy.types.RenderSettings).properties['filepath'].name} folder"


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


class Preflight(msgspec.Struct, frozen=True, kw_only=True):
    """Audit operators and dispatch condition."""

    # --- [CONDITIONS]

    condition: ClassVar[conditions.AllOf] = (
        Pathed.unsaved
        & conditions.Count(
            collection=Descriptor.declared(bpy.types.Scene, ("view_layers",), bpy.types.CollectionProperty),
            where=Some(conditions.Setting(descriptor=Descriptor.declared(bpy.types.ViewLayer, ("use",), bpy.types.BoolProperty))),
        )
        & _Folder()
        & conditions.Job(kind="RENDER")
    )

    # --- [COMMANDS]

    @property
    def operators(self) -> tuple[Operator[Empty] | Operator[Shader], ...]:
        """Audit operators in section order."""
        return (
            Operator(icon=Some("CHECKMARK"), form=Empty, run=self.audit_file, undo=Nothing),
            Operator(form=Empty, run=self.list_users, undo=Nothing),
            Operator[Shader](form=Shader, run=self.list_shader_users, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), undo=Nothing, primary=Some("kind")),
            Operator(form=Empty, run=self.measure_view_layer, undo=Nothing),
            Operator(icon=Some("FILE_BLEND"), form=Empty, run=self.open_library, undo=Nothing),
        )

    @staticmethod
    def audit_file(_call: Call[Empty]) -> Outcome[Answer]:
        """Reports missing files, empty material slots, and empty group or image nodes."""
        return Ok(Done(skipped=audited().swap().to_option()))

    @staticmethod
    def list_users(call: Call[Empty]) -> Outcome[Answer]:
        """Reports the data-blocks using the active data-block."""
        return _active(call.context).map(lambda block: Done(notes=tuple(("INFO", repr(user)) for user in users(block))))

    @staticmethod
    def list_shader_users(call: Call[Shader]) -> Outcome[Answer]:
        """Reports the materials using a shader node, groups included."""
        kind = call.properties.kind
        return (
            Block
            .of_seq(shader_kinds())
            .filter(lambda shader: shader.__name__ == kind)
            .try_head()
            .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, "bpy.types", member=kind))
            .map(lambda shader: Done(notes=tuple(("INFO", repr(user)) for user in shader_users(shader))))
        )

    @staticmethod
    def measure_view_layer(call: Call[Empty]) -> Outcome[Answer]:
        """Reports the evaluated geometry and image memory of the view layer."""
        return rooted(bpy.types.ViewLayer, call.context).bind(measured).map(lambda measure: Done(notes=tuple(("INFO", text) for text in measure.notes)))

    @staticmethod
    def open_library(call: Call[Empty]) -> Outcome[Answer]:
        """Opens the library of the active data-block in a new Blender process."""

        def opened(block: bpy.types.ID) -> Outcome[Answer]:
            match block if isinstance(block, bpy.types.Library) else block.library:
                case bpy.types.Library() as library:
                    return detached((bpy.app.binary_path, bpy.path.abspath(library.filepath)), call.identity.folder / "library.log").map(lambda _: Done())
                case None:
                    return Error(Fault.of(BlenderReason.ABSENT, repr(block), member="library"))

        return _active(call.context).bind(opened)

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Returns audit operators for registration."""
        return Ok(self.operators)

    def section(self) -> Section:
        """Returns section of audit buttons and Find Missing Files."""
        return Section(key="preflight", title="Preflight", rows=(*(OperatorButton(target=row) for row in self.operators), OperatorButton(target=Placement(bpy.ops.file.find_missing_files))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [AUDIT]


def _active(context: bpy.types.Context) -> Outcome[bpy.types.ID]:
    """Reads editor `context.id`, absent when unset."""
    return catch(lambda: context.id, exception=AttributeError)().to_option().bind(Option.of_optional).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, "bpy.context", member="id"))


def audited() -> Outcome[None]:
    """Accumulates missing files, empty material slots, group nodes with no group tree, and image texture nodes with no image."""

    def absent(owner: object, key: str) -> Outcome[None]:
        """Returns absent fault naming owner and key."""
        return Error(Fault.of(BlenderReason.ABSENT, repr(owner), member=key))

    def lost(missing: tuple[Dependency, ...]) -> Outcome[None]:
        """Resolves missing dependency owners in one pass, one fault per path."""
        return resolved(dependency.owner for dependency in missing).bind(
            lambda owners: accumulated(absent(owner, str(dependency.path)) for dependency, owner in zip(missing, owners)).map(lambda _: None)
        )

    nodes = tuple(node for tree in (*(tree for block in bpy.data.all_ids for tree in embedded(block).to_list()), *bpy.data.node_groups) for node in tree.nodes)
    return accumulated((
        lost(tuple(dependency for dependency in dependencies() if not dependency.present)),
        *(absent(item, str(index)) for item in bpy.data.objects for index, slot in enumerate(item.material_slots) if slot.material is None),
        *(
            absent(node, "node_tree")
            for node in nodes
            if isinstance(node, bpy.types.ShaderNodeGroup | bpy.types.CompositorNodeGroup | bpy.types.GeometryNodeGroup | bpy.types.TextureNodeGroup | bpy.types.NodeCustomGroup)
            and node.node_tree is None
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
    return tuple((kind.__name__, rna.name, rna.description) for kind in kinds for rna in (definition(kind),))


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
        copies = instances(depsgraph)
        meshes = tuple(data for instance in copies for data in (instance.source.evaluated_get(depsgraph).data,) if isinstance(data, bpy.types.Mesh))
        images = tuple(image for block in depsgraph.ids for image in (block.original,) if isinstance(image, bpy.types.Image))
        return Measure(
            objects=len(copies),
            vertices=sum(len(mesh.vertices) for mesh in meshes),
            faces=sum(len(mesh.polygons) for mesh in meshes),
            triangles=sum(len(mesh.loop_triangles) for mesh in meshes),
            images=len(images),
            image_bytes=sum(prod(image.size) * image.channels * (4 if image.is_float else 1) for image in images),  # type: ignore[call-overload]
        )

    return evaluated(view_layer).map(counted)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Measure", "Preflight", "Shader", "audited", "measured", "shader_kinds", "shader_rows", "shader_users", "users"]
