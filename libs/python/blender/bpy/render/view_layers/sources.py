"""View layer routing settings and the sources its files write: Render Layers outputs in reader axes, light family totals, derived layers, ID colors with their images, files, and documents, and the File Output nodes filing them."""

from collections.abc import Callable, Mapping
from enum import auto, Enum
import functools
import itertools
from pathlib import Path
from types import MappingProxyType
from typing import Final, Literal, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import mathutils
import msgspec
from msgspec import UNSET
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.mirrors import choices, enum_choices, flagged
from blender.bpy.data.rna.paths import RnaPath, rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.desired import Interface, NodeGroup, Socket
from blender.bpy.nodes.functions import built, placed
from blender.bpy.nodes.graphs import Expr, group_input, rgba, Term, xyz
from blender.bpy.nodes.trees import base_socket, DATA_TYPES
from blender.bpy.render.compositor.colors import with_alpha
from blender.bpy.render.outputs.formats import FileItem, FileItems, OutputFormat
from blender.bpy.render.outputs.paths import NodePaths, OutputPath, Pathed, ScenePaths, today
from blender.bpy.render.outputs.results import Finish, Written
from blender.bpy.render.view_layers.layers import colors, Role
from blender.bpy.render.view_layers.passes import Derived, Entity, Family, Flags, layer_of, PassFlags
from blender.bpy.scene.materials import DataPass
from blender.bpy.settings.attachments import Attachment
from blender.imaging.files.buffers import Coded, Depth, unassociated
from blender.imaging.files.documents import Document, LayerGroup, PixelLayer
from blender.imaging.files.images import Deflate, Storage
from blender.imaging.files.multilayer import Matte, Multilayer, Pass, PassLayout, PassRole
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason
from blender.records.stores import content_hash, rng
from blender.rendering.outputs.naming import Output

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Axis = Literal[-3, -2, -1, 1, 2, 3]
type Color = tuple[float, float, float]
type Named = tuple[tuple[str, bpy.types.ID, Color], ...]


class VectorSpace(Enum):
    """Space of a spatial vector pass."""

    WORLD = auto()
    CAMERA = auto()


class FileSplit(Enum):
    """Split of layer passes into files."""

    ONE = auto()
    CLASSES = auto()
    PASSES = auto()


class Recolor(Enum):
    """Colors an ID recolor writes onto layer names."""

    SPREAD = auto()
    SHUFFLE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_NUKE_LAYERS: Final = MappingProxyType({"Combined": "rgba", "Position": "Pworld", "Depth": "z"})
NUKE_CHANNELS: Final = MappingProxyType({"Depth": "Z"})
_NUKE_AXES: Final[Mapping[VectorSpace, tuple[Axis, Axis, Axis]]] = MappingProxyType({VectorSpace.WORLD: (1, 3, -2), VectorSpace.CAMERA: (1, 2, -3)})
_NUKE_MOTION: Final = (3, 4, 2, 3)

# --- [MODELS] ---------------------------------------------------------------------------


class PassFile(bpy.types.PropertyGroup):
    """File choice of the source the row names."""

    use_output: bpy.props.BoolProperty(name="File", description="Write the pass, skipped when off", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Output", description="Output class file holding pass", items=choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in Output if member is not Output.PREVIEW)
    )


class Routing(bpy.types.PropertyGroup):
    """Routing of view layer passes into files."""

    split: bpy.props.EnumProperty(name="Files", description="One file, a file per output class, or a file per pass", items=enum_choices(FileSplit))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    nuke: bpy.props.BoolProperty(name="Nuke", description="Write the layer names and axes Nuke reads, host names when off")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    normals: bpy.props.EnumProperty(name="Normals", description="Normal pass space", items=enum_choices(VectorSpace))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    totals: bpy.props.EnumProperty(name="Totals", description="Light families summed into one layer each", options={"ENUM_FLAG"}, items=enum_choices(Family))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    derived: bpy.props.EnumProperty(name="Derived", description="Light sums and depth layers built from passes", options={"ENUM_FLAG"}, items=enum_choices(Derived))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    ids: bpy.props.EnumProperty(name="ID Colors", description="Cryptomatte kinds drawn as one color per name", options={"ENUM_FLAG"}, items=enum_choices(Entity))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
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


class Recoloring(bpy.types.PropertyGroup):
    """Colors an ID recolor writes."""

    mode: bpy.props.EnumProperty(name="Colors", description="Evenly spaced hues, or the held colors shuffled among names", items=enum_choices(Recolor))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


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
    member = entity.color.prop.identifier

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
    return tuple((name, block, (red, green, blue)) for name, block in sorted(blocks.items()) for red, green, blue, *_ in (getattr(block, member),))


def located(scene: bpy.types.Scene, matte: Matte, layer: str) -> Outcome[tuple[bpy.types.ViewLayer, Entity]]:
    """Returns view layer `layer` and the Cryptomatte kind its matte names after the layer prefix."""
    return paired(layer_of(scene, layer), Entity.named(matte.name.removeprefix(f"{layer}.")).to_result_with(lambda: Fault.of(Reason.ABSENT, str(matte.file.path), member=matte.name)))


def entity_image(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, entity: Entity, rows: Named) -> Outcome[Option[Expr]]:
    """Builds entity ID image of name colors under mattes summed into alpha, `Nothing` without names."""
    source = f"{layer.name}.{entity.pass_name}"

    def matte(name: str) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeCryptomatteV2, members={"source": "RENDER", "scene": IdName.of(scene), "layer_name": source, "matte_id": name}, socket="Matte")

    def image(covers: tuple[tuple[Expr, Color], ...]) -> Option[Expr]:
        match tuple(cover * (*color, 1.0) for cover, color in covers), tuple(cover for cover, _ in covers):
            case (first, *rest), (base, *others):
                return Some(with_alpha(sum(rest, first), sum(others, base)))
            case _:
                return Nothing

    return accumulated(Ok((matte(name), color)) if "," not in name and name == name.strip() else Error(Fault.of(Reason.TAKEN, name, member=source)) for name, _, color in rows).map(image)


def colors_written(layer: bpy.types.ViewLayer, entity: Entity, chosen: Callable[[tuple[Color, ...]], tuple[Color, ...]]) -> Outcome[int]:
    """Writes chosen colors onto entity names, each block keeping its alpha."""
    rows, member = named(layer, entity), entity.color
    return accumulated(member.write(block, (*color, getattr(block, member.prop.identifier)[3])) for (_, block, _), color in zip(rows, chosen(tuple(color for _, _, color in rows)))).map(
        lambda priors: sum(prior.is_some() for prior in priors)
    )


@Operator.of(Recoloring)
def ids_recolor(call: Call[Recoloring]) -> Outcome[Answer]:
    """Set evenly spaced hues on layer ID names, or shuffle their colors among them."""
    mode = Recolor[call.properties.mode]

    def chosen(held: tuple[Color, ...]) -> tuple[Color, ...]:
        match mode:
            case Recolor.SPREAD:
                return colors(len(held), saturation=1.0, value=1.0)
            case Recolor.SHUFFLE:
                return tuple(held[index] for index in rng(content_hash(Routing, arrays=(np.asarray(held),))).permutation(len(held)))

    def painted(layer: bpy.types.ViewLayer, routing: Routing) -> Outcome[Answer]:
        if not routing.kinds:
            return Ok(Unchanged(reason=f"{layer.name} draws no ID colors"))
        return accumulated(colors_written(layer, entity, chosen) for entity in routing.kinds).map(
            lambda counts: Done() if sum(counts) else Unchanged(reason=f"{layer.name} ID colors hold chosen colors")
        )

    return Result[bpy.types.ViewLayer, Faults].bind(rooted(bpy.types.ViewLayer, call.context), lambda layer: ROUTING.read(call.identity, layer).bind(lambda routing: painted(layer, routing)))


def exported(scene: bpy.types.Scene, file: Written, identity: Identity) -> Outcome[None]:
    """Writes one ID layer file per matte of a written multilayer file, one layer per matte name a scene block colors, refusing a path equal to source."""

    def painted(path: OutputPath, matte: Matte, layer: bpy.types.ViewLayer, entity: Entity) -> Outcome[str]:
        shown = {name: color for name, _, color in named(layer, entity)}
        known = {name: shown[name] for name in matte.ids if name in shown}
        return (
            path
            .written(file.layer, Output.MATTE, Some(entity.pass_name), scene.frame_current, ".exr")
            .map(str)
            .filter_with(lambda target: target != file.path, lambda target: Fault.of(Reason.TAKEN, target, member=file.path))
            .bind(
                lambda target: Multilayer.written(
                    Path(target),
                    tuple(Pass(layer=name, channels=("R", "G", "B", "A"), role=PassRole.COLOR) for name in known),
                    PassLayout.PARTS,
                    Storage(depth=Depth.HALF, compression=Some(Deflate())),
                    (matte.file.parts[0].width, matte.file.parts[0].height),
                    identity.manifest.stamp,
                    lambda held: matte.filled(frozenset({held.layer}), np.asarray(known[held.layer])),
                )
            )
            .map(str)
        )

    def matted(path: OutputPath, matte: Matte) -> Outcome[str]:
        return located(scene, matte, file.layer).bind(lambda pair: painted(path, matte, *pair))

    return (
        paired(ScenePaths().bound(identity).bind(lambda pathed: pathed.path(scene, today())), Multilayer.read(Path(file.path)).bind(Matte.found))
        .bind(lambda found: accumulated(matted(found[0], matte) for matte in found[1]))
        .map(lambda _: None)
    )


def document(_identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document[Coded]]:
    """Builds the ID document of a written frame, one layer group per matte and one layer per matte name in its block's display color."""
    size = (frame.parts[0].height, frame.parts[0].width)

    def grouped(index: int, matte: Matte) -> Outcome[tuple[LayerGroup[Coded], npt.NDArray[np.float32]]]:
        def cover(name: str, color: Color) -> Outcome[tuple[str, Color, npt.NDArray[np.float32]]]:
            shown = mathutils.Color(color).from_scene_linear_to_srgb()
            return matte.coverage(frozenset({name})).map(lambda plane: (name, (shown.r, shown.g, shown.b), plane))

        def covered(layer: bpy.types.ViewLayer, entity: Entity) -> Outcome[tuple[tuple[str, Color, npt.NDArray[np.float32]], ...]]:
            return accumulated(cover(name, color) for name, _, color in named(layer, entity))

        def layered(name: str, color: Color, plane: npt.NDArray[np.float32]) -> Outcome[PixelLayer[Coded]]:
            return paired(depth.coded(np.broadcast_to(np.asarray(color, dtype=np.float32), (*size, 3))), depth.coded(plane)).map(lambda codes: PixelLayer(name=name, color=codes[0], alpha=codes[1]))

        return located(scene, matte, matte.name.rpartition(".")[0]).bind(
            lambda pair: covered(*pair).bind(
                lambda rows: accumulated(itertools.starmap(layered, rows)).map(
                    lambda layers: (
                        LayerGroup(name=matte.name, visible=index == 0, layers=layers),
                        functools.reduce(np.add, (plane[..., None] * np.asarray((*color, 1.0), dtype=np.float32) for _, color, plane in rows), np.zeros((*size, 4), np.float32)),
                    )
                )
            )
        )

    return Result[tuple[Matte, ...], Faults].bind(
        Matte.found(frame),
        lambda mattes: accumulated(itertools.starmap(grouped, enumerate(mattes))).bind(
            lambda groups: depth.coded(unassociated(next((filled for _, filled in groups), np.zeros((*size, 4), np.float32)))).map(
                lambda composite: Document(layers=tuple(group for group, _ in groups), composite=composite, colorspace="sRGB", density=scene.render.ppm_factor / scene.render.ppm_base)
            )
        ),
    )


# --- [LOBES]


def lobe() -> Outcome[NodeGroup]:
    """Builds shared lobe total group, `(direct + indirect) * color`."""
    interface = Interface(
        outputs=(Socket(key="total", kind=bpy.types.NodeSocketColor),),
        inputs=tuple(
            Socket(key=key, kind=bpy.types.NodeSocketColor, default_value=value)
            for key, value in (("direct", (0.0, 0.0, 0.0, 1.0)), ("indirect", (0.0, 0.0, 0.0, 1.0)), ("color", (1.0, 1.0, 1.0, 1.0)))
        ),
    )
    terms = {key: group_input(key, bpy.types.NodeSocketColor) for key in ("direct", "indirect", "color")}
    return built("lobe_total", interface, {"total": (terms["direct"] + terms["indirect"]) * terms["color"]}, bpy.types.CompositorNodeTree)


def lobed(slots: tuple[Flags, Flags, Flags], offered: Mapping[str, Term], group: NodeGroup) -> Option[Expr]:
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


def sources(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, rendered: Expr, live: bpy.types.Node, routing: Routing, role: Role, group: NodeGroup) -> Outcome[tuple[FileSocket, ...]]:
    """Returns sources layer files write in socket order, derived layers and ID images after, each under its file row's gate."""
    families = PassFlags.of().families
    offered = {socket.name: socket for socket in live.outputs if socket.enabled and socket.name != "Alpha" and base_socket(type(socket)) in DATA_TYPES}
    outputs = {name: msgspec.structs.replace(rendered, socket=socket.identifier, shape=type(socket)) for name, socket in offered.items()}
    spaces = {"Normal": VectorSpace[routing.normals], "Position": VectorSpace.WORLD, DataPass.POSITION.name.lower(): VectorSpace.WORLD, DataPass.REFERENCE.name.lower(): VectorSpace.WORLD}
    gates = {row.name: row for row in routing.files}

    def missing(name: str) -> Faults:
        return Fault.of(Reason.ABSENT, repr(live.outputs), member=name)

    def sourced(expr: Expr, name: str, socket_type: "rna_enums.NodeSocketDataTypeItems", output: Output, dimensions: Option[int] = Nothing) -> tuple[FileSocket, ...]:
        gate = Option.of_optional(gates.get(name))
        return (
            ()
            if gate.filter(lambda row: not row.use_output).is_some()
            else (FileSocket(name=name, term=expr, socket_type=socket_type, dimensions=dimensions, output=gate.map(lambda row: Output[row.output]).default_value(output)),)
        )

    def oriented(name: str) -> Outcome[Expr]:
        read = outputs[name]
        match spaces.get(name), routing.nuke:
            case VectorSpace.CAMERA, nuke:
                return (
                    Option
                    .of_optional(scene.camera)
                    .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(scene), member="camera"))
                    .map(lambda held: turned(read, _NUKE_AXES[VectorSpace.CAMERA] if nuke else (1, 2, 3), Some(held)))
                )
            case VectorSpace() as space, True:
                return Ok(turned(read, _NUKE_AXES[space], Nothing))
            case None, True if name == "Vector":
                return Ok(reordered(read, _NUKE_MOTION))
            case _:
                return Ok(read)

    def passed(name: str, socket: bpy.types.NodeSocket) -> Outcome[tuple[FileSocket, ...]]:
        dimensions = Descriptor.resolve(socket, ("default_value",), bpy.types.FloatProperty).map(lambda member: Some(member.prop.array_length)) if socket.type == "VECTOR" else Ok(Nothing)
        classed = (
            Output.MATTE
            if any(name.startswith(entity.pass_name) and name.removeprefix(entity.pass_name).isdigit() for entity in Entity)
            else Output.DATA
            if role is Role.DATA or socket.type != "RGBA"
            else Output.BEAUTY
        )
        return paired(oriented(name), dimensions).map(lambda pair: sourced(pair[0], name, DATA_TYPES[base_socket(type(socket))], classed, pair[1]))

    def totaled(family: Family) -> Outcome[tuple[FileSocket, ...]]:
        return lobed(families[family], outputs, group).to_result_with(lambda: missing(family.total)).map(lambda summed: sourced(summed, family.total, "RGBA", Output.BEAUTY))

    def derived(item: Derived) -> Outcome[tuple[FileSocket, ...]]:
        name = bpy.path.display_name(str(item.name).lower())
        depth = Option.of_optional(outputs.get(DataPass.DEPTH.name.lower())).to_result_with(lambda: missing(DataPass.DEPTH.name.lower()))
        match item:
            case Derived.DIRECT_LIGHT | Derived.INDIRECT_LIGHT:
                match tuple(
                    found
                    for direct, indirect, color in families.values()
                    for found in lobed((direct, frozenset(), color) if item is Derived.DIRECT_LIGHT else (frozenset(), indirect, color), outputs, group).to_list()
                ):
                    case (first, *rest):
                        return Ok(sourced(sum(rest, first), name, "RGBA", Output.BEAUTY))
                    case _:
                        return Ok(())
            case Derived.DEPTH_MAP:
                return depth.map(lambda read: sourced(Expr(kind=bpy.types.CompositorNodeNormalize, inputs=(("Value", read),)), name, "FLOAT", Output.DATA))
            case _:
                return depth.map(lambda read: sourced(1.0 / read, name, "FLOAT", Output.DATA))

    def identified(entity: Entity) -> Outcome[tuple[FileSocket, ...]]:
        return entity_image(scene, layer, entity, named(layer, entity)).map(lambda found: found.map(lambda image: sourced(image, f"{entity.label} IDs", "RGBA", Output.MATTE)).default_value(()))

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
    return _NUKE_LAYERS.get(name, name) if nuke else name


# --- [COMPOSITION] ----------------------------------------------------------------------

ROUTING: Final = Attachment(key="passes", form=Routing, owner=bpy.types.ViewLayer)
ids_export: Final = Finish(run=exported, formats=frozenset({"OPEN_EXR_MULTILAYER"}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "NUKE_CHANNELS",
    "ROUTING",
    "Axis",
    "Color",
    "FileSocket",
    "FileSplit",
    "Filing",
    "Named",
    "PassFile",
    "Recolor",
    "Recoloring",
    "Routing",
    "VectorSpace",
    "colors_written",
    "document",
    "entity_image",
    "exported",
    "filed",
    "filings",
    "ids_export",
    "ids_recolor",
    "layer_name",
    "lobe",
    "lobed",
    "located",
    "named",
    "reordered",
    "sources",
    "turned",
]
