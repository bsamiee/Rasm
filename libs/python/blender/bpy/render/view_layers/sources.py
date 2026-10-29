"""View layer routing settings and the sources its files write: Render Layers outputs in reader axes, light family totals, derived layers, ID images, and the File Output nodes filing them."""

from collections.abc import Mapping
from enum import auto, Enum
import itertools
from types import MappingProxyType
from typing import Final, Literal, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
from msgspec import UNSET

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.mirrors import choices, flag_choices, flagged
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.desired import NodeGroup
from blender.bpy.nodes.functions import built, placed
from blender.bpy.nodes.graphs import Expr, group_input, rgba, Shape, Term, xyz
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.render.outputs.formats import FileItem, FileItems, OutputFormat
from blender.bpy.render.outputs.paths import NodePaths, OutputPath, Pathed
from blender.bpy.render.view_layers.overrides import Role
from blender.bpy.render.view_layers.passes import DERIVED, Derived, ENTITIES, Entity, Family, PassFlags, total
from blender.bpy.scene.materials import DataPass
from blender.bpy.settings.attachments import Attachment
from blender.imaging.files.cryptomatte import Matte
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.rendering.outputs.naming import Output

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Axis = Literal[-3, -2, -1, 1, 2, 3]
type Named = tuple[tuple[str, bpy.types.ID, tuple[float, float, float]], ...]


class VectorSpace(Enum):
    """Space of a spatial vector pass."""

    WORLD = auto()
    CAMERA = auto()


class FileSplit(Enum):
    """Split of layer passes into files."""

    ONE = auto()
    CLASSES = auto()
    PASSES = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

NUKE_LAYERS: Final = MappingProxyType({"Combined": "rgba", "Position": "Pworld", "Depth": "z"})
NUKE_CHANNELS: Final = MappingProxyType({"Depth": "Z"})
NUKE_AXES: Final[Mapping[VectorSpace, tuple[Axis, Axis, Axis]]] = MappingProxyType({VectorSpace.WORLD: (1, 3, -2), VectorSpace.CAMERA: (1, 2, -3)})
NUKE_MOTION: Final = (3, 4, 2, 3)
_SOCKETS: Final[Mapping[str, tuple["rna_enums.NodeSocketDataTypeItems", Shape]]] = MappingProxyType({
    "VALUE": ("FLOAT", bpy.types.NodeSocketFloat),
    "VECTOR": ("VECTOR", bpy.types.NodeSocketVector),
    "RGBA": ("RGBA", bpy.types.NodeSocketColor),
})

# --- [MODELS] ---------------------------------------------------------------------------


class PassFile(bpy.types.PropertyGroup):
    """File choice of the source the row names."""

    use_output: bpy.props.BoolProperty(name="File", description="Write the pass, skipped when off", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Output", description="Output class file holding pass", items=choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in Output if member is not Output.PREVIEW)
    )


class Routing(bpy.types.PropertyGroup):
    """Routing of view layer passes into files."""

    split: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Files", description="One file, a file per output class, or a file per pass", items=choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in FileSplit)
    )
    nuke: bpy.props.BoolProperty(name="Nuke", description="Write the layer names and axes Nuke reads, host names when off")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    normals: bpy.props.EnumProperty(name="Normals", description="Normal pass space", items=choices((space.name, bpy.path.display_name(space.name.lower()), "") for space in VectorSpace))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    totals: bpy.props.EnumProperty(name="Totals", description="Light families summed into one layer each", options={"ENUM_FLAG"}, items=flag_choices(Family))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    derived: bpy.props.EnumProperty(name="Derived", description="Light sums and depth layers built from passes", options={"ENUM_FLAG"}, items=flag_choices(Derived))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    ids: bpy.props.EnumProperty(name="ID Colors", description="Cryptomatte kinds drawn as one color per name", options={"ENUM_FLAG"}, items=flag_choices(Entity))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    files: bpy.props.CollectionProperty(type=PassFile, name="Passes", description="File choice per pass")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @property
    def families(self) -> Family:
        """Light families summed."""
        return flagged(Family, self.totals)

    @property
    def layers(self) -> Derived:
        """Derived layers built."""
        return flagged(Derived, self.derived)

    @property
    def kinds(self) -> Entity:
        """Cryptomatte kinds drawn as ID colors."""
        return flagged(Entity, self.ids)


class PassName(bpy.types.PropertyGroup):
    """Source name a file row adds from sources no row holds."""

    name: bpy.props.StringProperty(name="Pass", description="Pass the file row names")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class FileSocket(msgspec.Struct, frozen=True, kw_only=True):
    """Socket a layer's files write, typed by the File Output item socket type."""

    name: str
    term: Expr
    socket_type: "rna_enums.NodeSocketDataTypeItems"
    dimensions: Option[int]
    output: Output


class Filing(msgspec.Struct, frozen=True, kw_only=True):
    """Sources, output class, and node format of one File Output node."""

    output: Output
    format: OutputFormat
    sources: tuple[FileSocket, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [IDS]


def named(layer: bpy.types.ViewLayer, entity: Entity) -> Named:
    """Returns name, block, and scene-linear color per Cryptomatte name of a layer in name order."""
    shown = tuple(item for item in layer.objects if not item.hide_render and item.data is not None)

    def root(item: bpy.types.Object) -> bpy.types.Object:
        return root(item.parent) if item.parent else item

    blocks: dict[str, bpy.types.ID]
    match entity:
        case Entity.OBJECT:
            blocks = {item.name: item for item in shown}
        case Entity.ASSET:
            blocks = {top.name: top for top in map(root, shown)}
        case _:
            blocks = {held.name: held for held in ((layer.material_override,) if layer.material_override else (slot.material for item in shown for slot in item.material_slots if slot.material))}
    return tuple((name, block, (red, green, blue)) for name, block in sorted(blocks.items()) for red, green, blue, *_ in (getattr(block, ENTITIES[entity].color.prop.identifier),))


def located(scene: bpy.types.Scene, matte: Matte, layer: str) -> Outcome[tuple[bpy.types.ViewLayer, Entity]]:
    """Returns view layer `layer` and the Cryptomatte kind its matte names after the layer prefix."""
    return paired(
        Option.of_optional(scene.view_layers.get(layer)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(scene.view_layers), member=layer)),
        Entity.named(matte.name.removeprefix(f"{layer}.")).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, str(matte.file.path), member=matte.name)),
    )


def entity_image(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, entity: Entity, rows: Named) -> Outcome[Option[Expr]]:
    """Builds entity ID image of name colors under mattes summed into alpha, `Nothing` without names."""

    def matte(name: str) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeCryptomatteV2,
            members={"source": "RENDER", "scene": IdName.of(scene), "layer_name": f"{layer.name}.{ENTITIES[entity].pass_name}", "matte_id": name},
            socket="Matte",
        )

    def image(covers: tuple[tuple[Expr, tuple[float, float, float]], ...]) -> Option[Expr]:
        match tuple(cover * (*color, 1.0) for cover, color in covers), tuple(cover for cover, _ in covers):
            case (first, *rest), (base, *others):
                return Some(
                    Expr(
                        kind=bpy.types.CompositorNodeSetAlpha,
                        inputs=(("Image", sum(rest, first)), ("Alpha", sum(others, base)), ("Type", "Replace Alpha")),
                        socket="Image",
                        shape=bpy.types.NodeSocketColor,
                    )
                )
            case _:
                return Nothing

    return accumulated(
        Ok((matte(name), color)) if "," not in name and name == name.strip() else Error(Fault.of(BlenderReason.TAKEN, name, member=f"{layer.name}.{ENTITIES[entity].pass_name}"))
        for name, _, color in rows
    ).map(image)


# --- [LOBES]


def lobe() -> Outcome[NodeGroup]:
    """Builds shared lobe total group, `(direct + indirect) * color`."""
    interface = Interface(
        outputs=(Socket(key="total", name="Total", kind=bpy.types.NodeSocketColor),),
        inputs=tuple(
            Socket(key=key, name=bpy.path.display_name(key), kind=bpy.types.NodeSocketColor, default_value=value)
            for key, value in (("direct", (0.0, 0.0, 0.0, 1.0)), ("indirect", (0.0, 0.0, 0.0, 1.0)), ("color", (1.0, 1.0, 1.0, 1.0)))
        ),
    )
    return built(
        "lobe_total",
        "Lobe Total",
        interface,
        {"total": (group_input("direct", bpy.types.NodeSocketColor) + group_input("indirect", bpy.types.NodeSocketColor)) * group_input("color", bpy.types.NodeSocketColor)},
        bpy.types.CompositorNodeTree,
    )


def lobed(
    slots: tuple[frozenset[Descriptor[bpy.types.BoolProperty]], frozenset[Descriptor[bpy.types.BoolProperty]], frozenset[Descriptor[bpy.types.BoolProperty]]],
    offered: Mapping[str, Term],
    group: NodeGroup,
) -> Option[Expr]:
    """Places lobe group over the live Render Layers outputs a family's direct, indirect, and color flags name, `Nothing` without a direct or indirect output."""
    linked = {key: offered[name] for key, flags in zip(("direct", "indirect", "color"), slots, strict=True) for name in sorted(flag.prop.name for flag in flags) if name in offered}
    return Some(placed(group, linked, "total")) if linked.keys() & {"direct", "indirect"} else Nothing


# --- [AXES]


def turned(vector: Expr, axes: tuple[Axis, Axis, Axis], camera: Option[bpy.types.Object]) -> Expr:
    """Converts vector to reader axes, in camera space when given a camera."""
    rotated = camera.map(
        lambda held: Expr(
            kind=bpy.types.FunctionNodeRotateVector,
            inputs=(
                ("Vector", vector),
                (
                    "Rotation",
                    Expr(
                        kind=bpy.types.FunctionNodeInvertRotation,
                        inputs=(("Rotation", Expr(kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", IdName.of(held)),), socket="Rotation")),),
                        socket="Rotation",
                    ),
                ),
            ),
            socket="Vector",
            shape=bpy.types.NodeSocketVector,
        )
    ).default_value(vector)
    names: Mapping[int, Literal["X", "Y", "Z"]] = {1: "X", 2: "Y", 3: "Z"}
    return rotated if axes == (1, 2, 3) else xyz(*(-rotated[names[-axis]] if axis < 0 else rotated[names[axis]] for axis in axes))


def reordered(vector: Expr, order: tuple[int, int, int, int]) -> Expr:
    """Reorders motion vector channels to reader order."""
    channels: tuple[Literal["Red", "Green", "Blue", "Alpha"], ...] = ("Red", "Green", "Blue", "Alpha")
    red, green, blue, alpha = (vector[channels[source - 1]] for source in order)
    return rgba(red, green, blue, alpha)


# --- [SOURCES]


def sources(
    scene: bpy.types.Scene, layer: bpy.types.ViewLayer, rendered: Expr, live: bpy.types.Node, routing: Routing, role: Role, flags: PassFlags, group: NodeGroup
) -> Outcome[tuple[FileSocket, ...]]:
    """Returns sources layer files write in socket order, derived layers and ID images after, each under its file row's gate."""
    offered = {socket.name: socket for socket in live.outputs if socket.enabled and socket.name != "Alpha" and socket.type in _SOCKETS}
    outputs = {name: msgspec.structs.replace(rendered, socket=socket.identifier, shape=_SOCKETS[socket.type][1]) for name, socket in offered.items()}
    spaces = {"Normal": VectorSpace[routing.normals], "Position": VectorSpace.WORLD, DataPass.POSITION.name.lower(): VectorSpace.WORLD, DataPass.REFERENCE.name.lower(): VectorSpace.WORLD}
    gates = {row.name: row for row in routing.files}

    def missing(name: str) -> Faults:
        return Fault.of(BlenderReason.ABSENT, repr(live.outputs), member=name)

    def sourced(expr: Expr, name: str, socket_type: "rna_enums.NodeSocketDataTypeItems", output: Output, dimensions: Option[int] = Nothing) -> Outcome[tuple[FileSocket, ...]]:
        match gates.get(name):
            case None:
                return Ok((FileSocket(name=name, term=expr, socket_type=socket_type, dimensions=dimensions, output=output),))
            case PassFile(use_output=True, output=held):
                return Ok((FileSocket(name=name, term=expr, socket_type=socket_type, dimensions=dimensions, output=Output[held]),))
            case _:
                return Ok(())

    def oriented(name: str) -> Outcome[Expr]:
        read = outputs[name]
        match spaces.get(name), routing.nuke:
            case VectorSpace.CAMERA, nuke:
                return (
                    Option
                    .of_optional(scene.camera)
                    .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(scene), member="camera"))
                    .map(lambda held: turned(read, NUKE_AXES[VectorSpace.CAMERA] if nuke else (1, 2, 3), Some(held)))
                )
            case VectorSpace() as space, True:
                return Ok(turned(read, NUKE_AXES[space], Nothing))
            case None, True if name == "Vector":
                return Ok(reordered(read, NUKE_MOTION))
            case _:
                return Ok(read)

    def passed(name: str, socket: bpy.types.NodeSocket) -> Outcome[tuple[FileSocket, ...]]:
        dimensions = Descriptor.resolve(socket, ("default_value",), bpy.types.FloatProperty).map(lambda member: Some(member.prop.array_length)) if socket.type == "VECTOR" else Ok(Nothing)
        classed = (
            Output.MATTE
            if any(name.startswith(held.pass_name) and name.removeprefix(held.pass_name).isdigit() for held in ENTITIES.values())
            else Output.DATA
            if role is Role.DATA or socket.type != "RGBA"
            else Output.BEAUTY
        )
        return paired(oriented(name), dimensions).bind(lambda pair: sourced(pair[0], name, _SOCKETS[socket.type][0], classed, pair[1]))

    def totaled(family: Family) -> Outcome[tuple[FileSocket, ...]]:
        return lobed(flags.families[family], outputs, group).to_result_with(lambda: missing(total(family))).bind(lambda summed: sourced(summed, total(family), "RGBA", Output.BEAUTY))

    def derived(item: Derived) -> Outcome[tuple[FileSocket, ...]]:
        layered = DERIVED[item]
        depth = Option.of_optional(outputs.get(DataPass.DEPTH.name.lower())).to_result_with(lambda: missing(DataPass.DEPTH.name.lower()))
        match item:
            case Derived.DIRECT | Derived.INDIRECT:
                match tuple(
                    found
                    for direct, indirect, color in flags.families.values()
                    for found in lobed((direct, frozenset(), color) if item is Derived.DIRECT else (frozenset(), indirect, color), outputs, group).to_list()
                ):
                    case (first, *rest):
                        return sourced(sum(rest, first), layered.name, layered.socket_type, layered.output)
                    case _:
                        return Ok(())
            case Derived.DEPTH:
                return depth.bind(lambda read: sourced(Expr(kind=bpy.types.CompositorNodeNormalize, inputs=(("Value", read),)), layered.name, layered.socket_type, layered.output))
            case _:
                return depth.bind(lambda read: sourced(1.0 / read, layered.name, layered.socket_type, layered.output))

    def identified(entity: Entity) -> Outcome[tuple[FileSocket, ...]]:
        return entity_image(scene, layer, entity, named(layer, entity)).bind(
            lambda found: found.map(lambda image: sourced(image, f"{ENTITIES[entity].name} IDs", "RGBA", Output.MATTE)).default_with(lambda: Ok(()))
        )

    return accumulated((*itertools.starmap(passed, offered.items()), *map(totaled, routing.families), *map(derived, routing.layers), *map(identified, routing.kinds))).map(
        lambda parts: tuple(source for part in parts for source in part)
    )


# --- [FILES]


def filings(found: tuple[FileSocket, ...], split: FileSplit) -> Outcome[tuple[Filing, ...]]:
    """Splits layer sources into File Output nodes, one multilayer file unless split, each node in its output class's EXR."""
    match split:
        case FileSplit.ONE:
            classed: tuple[tuple[Output, tuple[FileSocket, ...]], ...] = ((Output.BEAUTY, found),) if found else ()
        case FileSplit.CLASSES | FileSplit.PASSES:
            classed = tuple((output, held) for output in Output if output is not Output.PREVIEW for held in (tuple(source for source in found if source.output is output),) if held)
    return accumulated(OutputFormat.exr(output, layered=split is not FileSplit.PASSES).map(lambda chosen: Filing(output=output, format=chosen, sources=held)) for output, held in classed)


def filed(pathed: Pathed, path: OutputPath, layer: str, filing: Filing, key: Key, *, nuke: bool) -> Outcome[Expr]:
    """Builds File Output node of a filing under `key` in its node format, data items of a file per pass overriding it with full-float EXR, items under the names the host stores."""
    data = OutputFormat.exr(Output.DATA, layered=False).to_option().filter(lambda _: not filing.format.layered)

    def item(source: FileSocket, requested: str) -> FileItem:
        overridden = data.filter(lambda _: source.socket_type != "RGBA")
        return FileItem(
            name=requested,
            socket_type=source.socket_type,
            vector_socket_dimensions=source.dimensions.default_value(UNSET),
            override_node_format=overridden.map(lambda _: True).default_value(UNSET),
            save_as_render=overridden.map(lambda _: False).default_value(UNSET),
            format=overridden.default_value(UNSET),
        )

    def node(paths: NodePaths) -> Expr:
        content = FileItems(items=tuple(map(item, filing.sources, paths.items)))
        members = dict[str | RnaPath, Value](itertools.chain(((member.path, held) for member, held in paths.members.items()), filing.format.members(("format",)).items()))
        return Expr(
            kind=bpy.types.CompositorNodeOutputFile,
            inputs=tuple((stored.name, source.term) for stored, source in zip(content.items, filing.sources)),
            members=members,
            content=Some(content),
            label=Some(key),
        )

    return pathed.node(path, layer, filing.output, tuple(layer_name(source.name, nuke=nuke) for source in filing.sources), layered=filing.format.layered).map(node)


def layer_name(name: str, *, nuke: bool) -> str:
    """Returns the layer name a file reader takes for a source pass, the name Nuke reads when asked."""
    return NUKE_LAYERS.get(name, name) if nuke else name


# --- [COMPOSITION] ----------------------------------------------------------------------

ROUTING: Final = Attachment(key="passes", form=Routing, owner=bpy.types.ViewLayer)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "NUKE_AXES",
    "NUKE_CHANNELS",
    "NUKE_LAYERS",
    "NUKE_MOTION",
    "ROUTING",
    "Axis",
    "FileSocket",
    "FileSplit",
    "Filing",
    "Named",
    "PassFile",
    "PassName",
    "Routing",
    "VectorSpace",
    "entity_image",
    "filed",
    "filings",
    "layer_name",
    "lobe",
    "lobed",
    "located",
    "named",
    "reordered",
    "sources",
    "turned",
]
