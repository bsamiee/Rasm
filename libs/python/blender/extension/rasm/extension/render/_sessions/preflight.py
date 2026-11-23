"""Render dispatch conditions, asset audits, and resource measurements."""

from functools import cache, cached_property, partial
from math import prod
import os
from pathlib import Path
from typing import Annotated, ClassVar, override

import bpy
from expression import Error, NOTHING, Ok, Option, option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from rasm.extension.commands.invocations import Popup
from rasm.extension.commands.operators import Answer, Call, Done, Empty, Operator, Placement
from rasm.extension.data._ids.evaluation import evaluated_depsgraph
from rasm.extension.data._ids.store import embedded_tree
from rasm.extension.data._lifecycle.identity import Identity
from rasm.extension.data._lifecycle.registration import Acquisitions
from rasm.extension.data._rna import conditions
from rasm.extension.data._rna.descriptors import Descriptor
from rasm.extension.data._rna.paths import rna_definition, rooted, Struct
from rasm.extension.nodes._trees import nested
from rasm.extension.render._outputs.paths import Pathed
from rasm.extension.ui._layout.rows import OperatorButton
from rasm.extension.ui._layout.sections import Section
from rasm.records import accumulated, existing, Fault, Outcome, Reason
from rasm.rendering import spawned

# --- [MODELS] ---------------------------------------------------------------------------


class _Folder(conditions.Condition, frozen=True, kw_only=True):
    """Writable output folder of the scene frame path."""

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = NOTHING) -> bool:
        return option.of_optional(context.scene).map(lambda scene: existing(Path(scene.render.frame_path()))).filter(lambda folder: os.access(folder, os.W_OK)).is_some()

    @property
    @override
    def message(self) -> str:
        return f"writable {rna_definition(bpy.types.RenderSettings).properties['filepath'].name} folder"


class Shader(bpy.types.PropertyGroup):
    """Shader node class to list material users of."""

    kind: Annotated[str, bpy.props.EnumProperty(name="Shader", description="Shader node class to find", items=lambda _owner, _context: shader_rows(shader_kinds()))]


class Preflight(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Audit operators and dispatch condition."""

    # --- [CONDITIONS]

    condition: ClassVar[conditions.AllOf] = (
        Pathed.unsaved
        & conditions.Count(
            collection=Descriptor(root=bpy.types.Scene, prop=bpy.types.Scene.bl_rna.properties["view_layers"]),
            where=Some(conditions.Setting(descriptor=Descriptor(root=bpy.types.ViewLayer, prop=bpy.types.ViewLayer.bl_rna.properties["use"]))),
        )
        & _Folder()
        & conditions.Job(kind="RENDER")
    )

    # --- [COMMANDS]

    @cached_property
    def operators(self) -> tuple[Operator[Empty] | Operator[Shader], ...]:
        """Audit operators in section order."""
        return (
            Operator.of(Empty, icon=Some("CHECKMARK"), options=frozenset())(self.audit_file),
            Operator.of(Empty, options=frozenset())(self.list_users),
            Operator.of(Shader, invoke=Some(Popup[Shader](partial(bpy.types.WindowManager.invoke_search_popup))), options=frozenset(), primary=Some("kind"))(self.list_shader_users),
            Operator.of(Empty, options=frozenset())(self.measure_view_layer),
            Operator.of(Empty, icon=Some("FILE_BLEND"), options=frozenset())(self.open_library),
        )

    @staticmethod
    def audit_file(_call: Call[Empty]) -> Outcome[Answer]:
        """Reports missing files, empty material slots, and empty group or image nodes."""
        return Ok(Done(skipped=file_audit().swap().default_value(())))

    @staticmethod
    def list_users(call: Call[Empty]) -> Outcome[Answer]:
        """Reports the data-blocks using the active data-block."""
        return _active(call.context).map(lambda block: Done(notes=tuple(("INFO", repr(user)) for user in bpy.data.user_map(subset=(block,))[block])))

    @staticmethod
    def list_shader_users(call: Call[Shader]) -> Outcome[Answer]:
        """Reports the materials using a shader node, groups included."""
        kind = call.properties.kind
        return (
            Block
            .of_seq(shader_kinds())
            .filter(lambda shader: shader.__name__ == kind)
            .try_head()
            .to_result_with(lambda: Fault.of(Reason.ABSENT, "bpy.types", member=kind))
            .map(
                lambda shader: Done(
                    notes=tuple(
                        ("INFO", repr(material))
                        for material in bpy.data.materials
                        for tree in option.of_optional(material.node_tree).to_list()
                        if any(isinstance(node, shader) for held in nested((tree,)) for node in held.nodes)
                    )
                )
            )
        )

    @staticmethod
    def measure_view_layer(call: Call[Empty]) -> Outcome[Answer]:
        """Reports the evaluated instance geometry and loaded image memory of the view layer."""

        def counted(depsgraph: bpy.types.Depsgraph) -> Answer:
            data = tuple(item.data for instance in depsgraph.object_instances for item in option.of_optional(instance.object).to_list())
            meshes = tuple(mesh for mesh in data if isinstance(mesh, bpy.types.Mesh))
            images = tuple(image for block in depsgraph.ids for image in (block.original,) if isinstance(image, bpy.types.Image))
            counts = {
                "Objects": len(data),
                "Vertices": sum(len(mesh.vertices) for mesh in meshes),
                "Faces": sum(len(mesh.polygons) for mesh in meshes),
                "Triangles": sum(len(mesh.loop_triangles) for mesh in meshes),
            }
            memory = sum(prod(image.size) * image.channels * (4 if image.is_float else 1) for image in images)
            return Done(notes=(*(("INFO", f"{name} {count:,}") for name, count in counts.items()), ("INFO", f"Images {len(images):,} ({memory / 2**20:,.1f} MiB)")))

        return rooted(bpy.types.ViewLayer, call.context).bind(evaluated_depsgraph).map(counted)

    @staticmethod
    def open_library(call: Call[Empty]) -> Outcome[Answer]:
        """Opens the library of the active data-block in a new Blender process."""

        def opened(block: bpy.types.ID) -> Outcome[Answer]:
            match block if isinstance(block, bpy.types.Library) else block.library:
                case bpy.types.Library() as library:
                    return spawned((bpy.app.binary_path, bpy.path.abspath(library.filepath)), detached=True).map(lambda _: Done())
                case None:
                    return Error(Fault.of(Reason.ABSENT, repr(block), member="library"))

        return _active(call.context).bind(opened)

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity, /) -> Acquisitions:
        """Returns audit operators for registration."""
        return self.operators

    # --- [SECTIONS]

    @cached_property
    def section(self) -> Section:
        """Audit buttons and Find Missing Files."""
        return Section(key="preflight", rows=(*(OperatorButton(target=row) for row in self.operators), OperatorButton(target=Placement(bpy.ops.file.find_missing_files))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _active(context: bpy.types.Context) -> Outcome[bpy.types.ID]:
    """Reads editor `context.id`, absent when unset."""
    absent = partial(Fault.of, Reason.ABSENT, "bpy.context", member="id")
    return catch(lambda: context.id, exception=AttributeError)().map_error(absent).bind(lambda value: option.of_optional(value).to_result_with(absent))


def file_audit() -> Outcome[None]:
    """Accumulates missing files, empty material slots, group nodes with no group tree, and image texture nodes with no image."""

    def absent(owner: object, key: str) -> Outcome[None]:
        """Returns absent fault naming owner and key."""
        return Error(Fault.of(Reason.ABSENT, repr(owner), member=key))

    lost: list[Outcome[None]] = []

    def visited(owner: bpy.types.ID, path: str, _: bpy.types.BlendDataPathMeta) -> None:
        if not (absolute := Path(os.path.abspath(bpy.path.abspath(path, library=owner.library)))).exists():
            lost.append(absent(owner, str(absolute)))

    bpy.data.file_path_foreach(visited, flags={"SKIP_PACKED", "SKIP_WEAK_REFERENCES", "EXPAND_TOKENS", "EXPAND_SEQUENCES"})
    nodes = tuple(node for tree in (*(tree for block in bpy.data.all_ids for tree in embedded_tree(block).to_list()), *bpy.data.node_groups) for node in tree.nodes)
    return accumulated((
        *lost,
        *(absent(item, str(index)) for item in bpy.data.objects for index, slot in enumerate(item.material_slots) if slot.material is None),
        *(
            absent(node, "node_tree")
            for node in nodes
            if isinstance(node, bpy.types.ShaderNodeGroup | bpy.types.CompositorNodeGroup | bpy.types.GeometryNodeGroup | bpy.types.TextureNodeGroup | bpy.types.NodeCustomGroup)
            and node.node_tree is None
        ),
        *(absent(node, "image") for node in nodes if isinstance(node, bpy.types.ShaderNodeTexImage | bpy.types.ShaderNodeTexEnvironment) and node.image is None),
    )).map(lambda _: None)


def shader_kinds() -> tuple[type[bpy.types.ShaderNode], ...]:
    """Returns every registered `ShaderNode` subclass in `bpy.types`."""
    return tuple(kind for kind in map(partial(getattr, bpy.types), dir(bpy.types)) if isinstance(kind, type) and issubclass(kind, bpy.types.ShaderNode) and kind is not bpy.types.ShaderNode)


@cache
def shader_rows(kinds: tuple[type[bpy.types.ShaderNode], ...]) -> tuple[tuple[str, str, str], ...]:
    """Returns enum rows of shader classes, cached so the host's dynamic enum reads keep their strings."""
    return tuple((kind.__name__, rna.name, rna.description) for kind in kinds for rna in (rna_definition(kind),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Preflight", "Shader", "file_audit", "shader_kinds", "shader_rows"]
