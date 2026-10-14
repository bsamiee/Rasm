"""File audit, evaluated scene measures, and camera admission before render dispatch."""

import datetime
from functools import partial
from itertools import groupby, starmap
from math import prod
from pathlib import Path
from typing import Annotated, ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Acquired, Done, Empty, InvokePopup, Operator, Stock
from blender.bpy.data.ids.evaluation import evaluated, Instance
from blender.bpy.data.ids.store import resolved
from blender.bpy.data.rna import conditions
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderRenderingFault, BlenderResult, BlenderUnmetFault
from blender.bpy.files.blends import dependencies, Dependency
from blender.bpy.render.outputs.paths import OutputPath, Pathed
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import Label, OperatorButton
from blender.bpy.ui.layout.sections import Section, Single
from blender.imaging.effects.detail.denoise import TemporalDenoise
from blender.imaging.faults import accumulated, paired
from blender.rendering.batch.processes import detached
from blender.rendering.batch.queue import Bound, Fixed, Frames, JobCamera
from blender.rendering.outputs.naming import Count, Number, Output, Slot, Word

# --- [MODELS] ---------------------------------------------------------------------------


class NotCamera(msgspec.Struct, frozen=True, kw_only=True):
    """Camera binding naming an object with another native data type."""

    binding: str
    kind: str

    @property
    def message(self) -> str:
        """Names the binding and the object's native type."""
        return f"Requires a camera at {self.binding}, which names an object of type {self.kind}"


class Unvaried(msgspec.Struct, frozen=True, kw_only=True):
    """Output template missing a field varied by its render scope."""

    field: Word | Count

    @property
    def message(self) -> str:
        """Names the field needed to distinguish the scope's written files."""
        return f"Requires {self.field.label} in the output template, since the render writes one file per {self.field.label.lower()}"


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
        """Displays geometry counts and image memory in mebibytes."""
        return (f"Objects {self.objects:,}", f"Vertices {self.vertices:,}", f"Faces {self.faces:,}", f"Triangles {self.triangles:,}", f"Images {self.images:,} ({self.image_bytes / 2**20:,.1f} MiB)")

    @property
    def caption(self) -> str:
        """Joins the current counts for a status bar."""
        return " | ".join(self.notes)


class Shader(msgspec.Struct, frozen=True, kw_only=True):
    """Native shader class whose material users an operator lists."""

    kind: Annotated[
        str,
        msgspec.Meta(title="Shader", description="Shader node class the listed materials hold"),
        Choices(lambda *_: ((kind.__name__, kind.bl_rna.name, kind.bl_rna.description) for kind in shader_kinds())),  # ty: ignore[unresolved-attribute]
    ]


class JobScope(msgspec.Struct, frozen=True, kw_only=True):
    """One planned scene, layer, camera binding, and optional temporal denoise job."""

    scene: str
    camera: JobCamera
    layer: str | None = None
    temporal: TemporalDenoise | None = None


class Preflight(msgspec.Struct, frozen=True, kw_only=True):
    """Native audit commands, dispatch conditions, and current scene statistics."""

    measures: ClassVar[tuple[Label]] = (Label(text=lambda context: Option.of_optional(context.view_layer).bind(lambda layer: measured(layer).to_option()).map(lambda measure: measure.caption)),)
    counts: ClassVar[tuple[Label]] = (
        Label(
            text=lambda context: (
                Option
                .of_optional(context.scene)
                .map(lambda scene: tuple(scene.compositor_effects))
                .filter(bool)
                .map(lambda effects: f"Nodes {sum(len(tree.nodes) for effect in effects for tree in Option.of_optional(effect.node_group).to_list()):,}")
            )
        ),
    )

    # --- [CONDITIONS]

    @property
    def condition(self) -> BlenderResult[conditions.AllOf]:
        """Requires a saved file, used layers, writable output space, and no render job."""
        return paired(
            paired(Descriptor.resolve(bpy.types.Context, "blend_data.filepath", bpy.types.StringProperty), Descriptor.resolve(bpy.types.Scene, "view_layers", bpy.types.CollectionProperty)),
            paired(Descriptor.resolve(bpy.types.ViewLayer, "use", bpy.types.BoolProperty), Descriptor.resolve(bpy.types.Scene, "render.filepath", bpy.types.StringProperty)),
        ).map(
            lambda pair: conditions.AllOf(
                conditions=(
                    conditions.Not(condition=conditions.Setting(descriptor=pair[0][0], accepted=("",))),
                    conditions.Count(collection=pair[0][1], where=Some(conditions.Setting(descriptor=pair[1][0]))),
                    conditions.Folder(path=pair[1][1], least=0),
                    conditions.Job(kind="RENDER"),
                )
            )
        )

    # --- [COMMANDS]

    @property
    def operators(self) -> tuple[Operator[Empty] | Operator[Shader], ...]:
        """File audit, users, shader search, measurement, and library commands."""

        def active(context: bpy.types.Context) -> BlenderResult[bpy.types.ID]:
            """Reads the editor's optional native data-block context."""
            return catch(lambda: context.id, exception=AttributeError)().to_option().bind(Option.of_optional).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="id"),))

        def opened(block: bpy.types.ID, folder: Path) -> BlenderResult[Done]:
            """Opens the block's library in a detached native Blender process."""
            match block if isinstance(block, bpy.types.Library) else block.library:
                case bpy.types.Library() as library:
                    return (
                        detached((bpy.app.binary_path, bpy.path.abspath(library.filepath)), folder / "library.log")
                        .map_error(lambda faults: (BlenderRenderingFault(faults=faults),))
                        .map(lambda _: Done())
                    )
                case None:
                    return Error((BlenderAbsentFault(owner=repr(block), key="library"),))

        def listed(kind: str) -> BlenderResult[Done]:
            """Resolves the chosen native class and reports each material user."""
            return (
                Option
                .of_optional(next((shader for shader in shader_kinds() if shader.__name__ == kind), None))
                .to_result_with(lambda: (BlenderAbsentFault(owner="bpy.types", key=kind),))
                .map(lambda shader: Done(notes=tuple(("INFO", repr(user)) for user in shader_users(shader))))
            )

        return (
            Operator(
                key="audit",
                title="Audit File",
                description="Report missing files, material slots, group trees, and texture images",
                icon=Some("CHECKMARK"),
                form=Empty,
                run=lambda _: Ok(Done(skipped=audited().swap().to_option())),
                undo=False,
            ),
            Operator(
                key="users",
                title="List Users",
                description="Report each data-block using the active data-block",
                form=Empty,
                run=lambda call: active(call.context).map(lambda block: Done(notes=tuple(("INFO", repr(user)) for user in users(block)))),
                undo=False,
            ),
            Operator[Shader](
                key="shader",
                title="List Shader Users",
                description="Report each material containing the chosen shader node",
                form=Shader,
                run=lambda call: listed(call.record.kind),
                invoke=Some(InvokePopup.SEARCH),
                undo=False,
                primary=Some(next(field for field in msgspec.structs.fields(Shader) if field.name == "kind")),
            ),
            Operator[Empty](
                key="measure",
                title="Measure View Layer",
                description="Report evaluated geometry and image memory of the active view layer",
                form=Empty,
                run=lambda call: (
                    Option
                    .of_optional(call.context.view_layer)
                    .map(measured)
                    .default_with(lambda: Error((BlenderAbsentFault(owner="bpy.context", key="view_layer"),)))
                    .map(lambda measure: Done(notes=tuple(("INFO", text) for text in measure.notes)))
                ),
                undo=False,
            ),
            Operator(
                key="library_open",
                title="Open Library",
                description="Open the active data-block's library in another Blender",
                icon=Some("FILE_BLEND"),
                form=Empty,
                run=lambda call: active(call.context).bind(lambda block: opened(block, call.identity.folder)),
                undo=False,
            ),
        )

    def acquisitions(self, identity: Identity, /) -> BlenderResult[Acquisitions]:
        """Retains every native audit command declaration for registration."""
        return Ok(self.operators)

    def section(self) -> Section:
        """Returns audit commands beside Blender's missing-file browser."""
        return Single(
            key="preflight",
            title="Preflight",
            rows=(*(OperatorButton(target=Acquired(key=row.key), icon=row.icon) for row in self.operators), OperatorButton(target=Stock(operator=bpy.ops.file.find_missing_files))),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [AUDIT]


def audited() -> BlenderResult[None]:
    """Accumulates missing external files, empty material slots, group trees, and images."""

    def absent(owner: object, key: str | int) -> BlenderResult[None]:
        """Preserves the missing native owner and member in a typed finding."""
        return Error((BlenderAbsentFault(owner=repr(owner), key=key),))

    def lost(missing: tuple[Dependency, ...]) -> BlenderResult[None]:
        """Resolves the missing dependencies' original owners once before reporting their paths."""
        return resolved(dependency.owner for dependency in missing).bind(
            lambda owners: accumulated(absent(owner, str(dependency.path)) for dependency, owner in zip(missing, owners, strict=True)).map(lambda _: None)
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
    """Returns the data-block's native users."""
    return tuple(bpy.data.user_map(subset=(block,))[block])


def shader_kinds() -> tuple[type[bpy.types.ShaderNode], ...]:
    """Reads every registered shader class from Blender's native type namespace."""
    return tuple(kind for kind in map(partial(getattr, bpy.types), dir(bpy.types)) if isinstance(kind, type) and issubclass(kind, bpy.types.ShaderNode) and kind is not bpy.types.ShaderNode)


def shader_users(kind: type[bpy.types.ShaderNode]) -> tuple[bpy.types.Material, ...]:
    """Returns materials using the shader directly or through a native node group."""

    def contains(tree: bpy.types.NodeTree) -> bool:
        return any(isinstance(node, kind) or (isinstance(node, bpy.types.ShaderNodeGroup) and node.node_tree is not None and contains(node.node_tree)) for node in tree.nodes)

    return tuple(material for material in bpy.data.materials if material.node_tree is not None and contains(material.node_tree))


# --- [MEASURES]


def measured(view_layer: bpy.types.ViewLayer) -> BlenderResult[Measure]:
    """Counts evaluated geometry per instance and image memory through native buffers."""

    def counted(depsgraph: bpy.types.Depsgraph) -> Measure:
        """Reads copied instances and their evaluated mesh data in native order."""
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


def cameras(scene: bpy.types.Scene, scope: JobScope, frames: Frames) -> BlenderResult[None]:
    """Accumulates missing cameras, invalid bindings, and unsupported temporal projection."""
    custom = "CUSTOM"
    match scope.camera:
        case Bound(camera=camera, markers=markers) as bound:
            numbers = frames.numbers
            gaps = tuple(next(run) for missing, run in groupby(numbers, key=lambda frame: bound.at(frame).is_none()) if missing)
            bindings = (*(("the scene camera", name) for name in Option.of_optional(camera).to_list()), *((f"the marker at frame {marker.frame}", marker.camera) for marker in markers))
        case Fixed(camera=camera):
            gaps, bindings = (), (("the fixed camera", camera),)

    def admitted(accepted: tuple[str, ...]) -> BlenderResult[None]:
        """Admits each camera binding through its native object and projection."""

        def checked(binding: str, name: str) -> BlenderResult[None]:
            """Returns the absent object, wrong data kind, or unsupported projection."""

            def native(bound_object: bpy.types.Object) -> BlenderResult[None]:
                camera = bound_object.data
                return (
                    Error((BlenderUnmetFault(requirement=NotCamera(binding=binding, kind=bound_object.type)),))
                    if not isinstance(camera, bpy.types.Camera)
                    else Error((BlenderChoiceFault(path=f"{binding}: {camera.path_from_id('type')}", identifier=camera.type, accepted=accepted),))
                    if scope.temporal is not None and camera.type == custom
                    else Ok(None)
                )

            return Option.of_optional(bpy.data.objects.get(name)).map(native).default_with(lambda: Error((BlenderAbsentFault(owner=repr(bpy.data.objects), key=name),)))

        return accumulated((*(Error((BlenderAbsentFault(owner=repr(scene), key=frame),)) for frame in gaps), *starmap(checked, bindings))).map(lambda _: None)

    return Descriptor.resolve(bpy.types.Camera, "type", bpy.types.EnumProperty).bind(
        lambda descriptor: admitted(tuple(item.identifier for item in descriptor.prop.enum_items if item.identifier != custom))
    )


# --- [DISPATCH]


def templated(path: OutputPath, scope: JobScope, scene: bpy.types.Scene, frames: Frames) -> BlenderResult[None]:
    """Accumulates missing varying fields and written-path composition refusals."""
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


def checked(pathed: Pathed, scopes: tuple[JobScope, ...], day: datetime.date) -> BlenderResult[tuple[Measure, ...]]:
    """Audits every scope before measuring its distinct native scene and layer owners."""
    planned = tuple(
        (
            scope,
            scene,
            Frames(
                start=scene.frame_start,
                end=scene.frame_end,
                step=1 if scope.temporal is None and any(msgspec.structs.replace(other, temporal=None) == scope for other in scopes if other.temporal is not None) else scene.frame_step,
            ),
        )
        for scope in scopes
        for scene in (bpy.data.scenes[scope.scene],)
    )

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

__all__ = ["JobScope", "Measure", "NotCamera", "Preflight", "Shader", "Unvaried", "audited", "cameras", "checked", "measured", "shader_kinds", "shader_users", "templated", "users"]
