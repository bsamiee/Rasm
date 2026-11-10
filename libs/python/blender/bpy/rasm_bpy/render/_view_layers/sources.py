"""View layer routing settings, render pass sources, ID images and documents, and File Output nodes."""

from collections.abc import Mapping
from enum import auto, Enum
import functools
import itertools
from pathlib import Path
from types import MappingProxyType
from typing import Annotated, Final, Literal

import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
import mathutils
import msgspec
import numpy as np
import numpy.typing as npt

from rasm_bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.store import IdName
from rasm_bpy.data._rna.descriptors import Descriptor, Value
from rasm_bpy.data._rna.paths import RnaPath, rooted
from rasm_bpy.data._rna.properties import choices, enum_choices, flagged
from rasm_bpy.nodes._graphs import Expr, group_input, rgba, Term, xyz
from rasm_bpy.nodes._interfaces import Interface, Socket
from rasm_bpy.nodes._trees import base_socket, DATA_TYPES
from rasm_bpy.nodes.changes import NodeGroup
from rasm_bpy.nodes.functions import built, placed
from rasm_bpy.render._compositor.colors import with_alpha
from rasm_bpy.render._outputs.formats import FileItem, FileItems, OutputFormat
from rasm_bpy.render._outputs.paths import NodePaths, OutputPath, Pathed, ScenePaths, today
from rasm_bpy.render._outputs.results import Finish, Written
from rasm_bpy.render._view_layers.layers import colors, Role
from rasm_bpy.render._view_layers.passes import CryptomatteKind, Derived, Family, layer_of, LightSlot, PassFlags
from rasm_bpy.scene._materials import DataPass
from rasm_bpy.settings._attachments import Attachment
from rasm_imaging import Deflate, Depth, Document, LayerGroup, Matte, Multilayer, Pass, PassLayout, PassRole, PixelLayer, rng, Storage, unassociated
from rasm_records import accumulated, content_hash, Error, Fault, Faults, Nothing, Ok, Outcome, paired, Reason
from rasm_rendering import Output

# --- [TYPES] ----------------------------------------------------------------------------

type _Axis = Literal[-3, -2, -1, 1, 2, 3]
type _PassFiles = bpy.types.bpy_prop_collection_idprop[PassFile]


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
_NUKE_AXES: Final[Mapping[VectorSpace, tuple[_Axis, _Axis, _Axis]]] = MappingProxyType({VectorSpace.WORLD: (1, 3, -2), VectorSpace.CAMERA: (1, 2, -3)})

# --- [MODELS] ---------------------------------------------------------------------------


class PassFile(bpy.types.PropertyGroup):
    """File choice of the source the row names."""

    use_output: Annotated[bool, bpy.props.BoolProperty(name="File", description="Write the pass, skipped when off", default=True)]
    output: Annotated[
        str,
        bpy.props.EnumProperty(
            name="Output",
            description="Output class file holding pass",
            items=choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in Output if member is not Output.PREVIEW),
        ),
    ]


class Routing(bpy.types.PropertyGroup):
    """Routing of view layer passes into files."""

    split: Annotated[str, bpy.props.EnumProperty(name="Files", description="One file, a file per output class, or a file per pass", items=enum_choices(FileSplit))]
    nuke: Annotated[bool, bpy.props.BoolProperty(name="Nuke", description="Write the layer names and axes Nuke reads, host names when off")]
    normals: Annotated[str, bpy.props.EnumProperty(name="Normals", description="Normal pass space", items=enum_choices(VectorSpace))]
    totals: Annotated[set[str], bpy.props.EnumProperty(name="Totals", description="Light families summed into one layer each", options={"ENUM_FLAG"}, items=enum_choices(Family))]
    derived: Annotated[set[str], bpy.props.EnumProperty(name="Derived", description="Light sums and depth layers built from passes", options={"ENUM_FLAG"}, items=enum_choices(Derived))]
    ids: Annotated[set[str], bpy.props.EnumProperty(name="ID Colors", description="Cryptomatte kinds drawn as one color per name", options={"ENUM_FLAG"}, items=enum_choices(CryptomatteKind))]
    files: Annotated[_PassFiles, bpy.props.CollectionProperty(type=PassFile, name="Passes", description="File choice per pass")]


class Recoloring(bpy.types.PropertyGroup):
    """Colors an ID recolor writes."""

    mode: Annotated[str, bpy.props.EnumProperty(name="Colors", description="Evenly spaced hues, or the held colors shuffled among names", items=enum_choices(Recolor))]


class FileSocket(msgspec.Struct, frozen=True, kw_only=True):
    """Source identity, requested filename, term, socket type and storage class of a File Output item."""

    key: Key
    name: str
    term: Expr
    socket_type: type[bpy.types.NodeSocket]
    dimensions: Option[int]
    output: Output


class FileOutput(msgspec.Struct, frozen=True, kw_only=True):
    """Sources, output class, and node format of one File Output node."""

    output: Output
    format: OutputFormat
    sources: tuple[FileSocket, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [IDS]


def _named(layer: bpy.types.ViewLayer, entity: CryptomatteKind) -> tuple[tuple[str, bpy.types.Object | bpy.types.Material, tuple[float, float, float]], ...]:
    """Returns name, block, and scene-linear color per Cryptomatte name of a layer in name order."""
    shown = (item for item in layer.objects if not item.hide_render and item.data is not None)
    roots: dict[bpy.types.Object, bpy.types.Object] = {}

    def root(item: bpy.types.Object) -> bpy.types.Object:
        path = []
        while item not in roots and (parent := item.parent) is not None:
            path.append(item)
            item = parent
        top = roots.setdefault(item, item)
        roots.update(dict.fromkeys(path, top))
        return top

    blocks: dict[str, bpy.types.Object | bpy.types.Material]
    match entity:
        case CryptomatteKind.OBJECT:
            blocks = {item.name: item for item in shown}
        case CryptomatteKind.ASSET:
            blocks = {top.name: top for top in map(root, shown)}
        case _:
            blocks = {held.name: held for held in ((layer.material_override,) if layer.material_override else (slot.material for item in shown for slot in item.material_slots if slot.material))}
    return tuple(
        (name, block, (float(red), float(green), float(blue)))
        for name, block in sorted(blocks.items())
        for red, green, blue, _ in (np.asarray(block.diffuse_color if isinstance(block, bpy.types.Material) else block.color),)
    )


def _located(scene: bpy.types.Scene, matte: Matte, layer: Option[str] = Nothing) -> Outcome[tuple[bpy.types.ViewLayer, CryptomatteKind]]:
    """Resolves the matte's native namespace, or a routed file's known layer and bare pass name."""
    return (
        layer
        .map(lambda name: layer_of(scene, name).map(lambda held: (held,)))
        .default_with(lambda: Ok(tuple(scene.view_layers)))
        .bind(
            lambda layers: next(
                (Some((held, entity)) for held in layers for entity in CryptomatteKind if matte.name == f"{held.name}.{entity.pass_name}" or (layer.is_some() and matte.name == entity.pass_name)),
                Nothing,
            ).to_result_with(lambda: Fault.of(Reason.ABSENT, str(matte.file.path), member=matte.name))
        )
    )


@Operator.of(Recoloring)
def ids_recolor(call: Call[Recoloring]) -> Outcome[Answer]:
    """Set evenly spaced hues on layer ID names, or shuffle their colors among them."""
    mode = Recolor[call.properties.mode]

    def chosen(held: tuple[tuple[float, float, float], ...]) -> tuple[tuple[float, float, float], ...]:
        match mode:
            case Recolor.SPREAD:
                return colors(len(held), saturation=1.0, value=1.0)
            case Recolor.SHUFFLE:
                shuffled = list(held)
                rng(content_hash(Routing, arrays=(np.asarray(held),))).shuffle(shuffled)
                return tuple(shuffled)

    def painted(layer: bpy.types.ViewLayer, routing: Routing) -> Outcome[Answer]:
        if not (kinds := flagged(CryptomatteKind, routing.ids)):
            return Ok(Unchanged(reason=f"{layer.name} draws no ID colors"))
        return accumulated(
            entity.color.write(block, (*color, (block.diffuse_color if isinstance(block, bpy.types.Material) else block.color)[3]))
            for entity in kinds
            for rows in (_named(layer, entity),)
            for (_, block, _), color in zip(rows, chosen(tuple(color for _, _, color in rows)))
        ).map(lambda priors: Done() if any(prior.is_some() for prior in priors) else Unchanged(reason=f"{layer.name} ID colors hold chosen colors"))

    return Result[bpy.types.ViewLayer, Faults].bind(rooted(bpy.types.ViewLayer, call.context), lambda layer: ROUTING.read(call.identity, layer).bind(lambda routing: painted(layer, routing)))


def exported(scene: bpy.types.Scene, file: Written, identity: Identity) -> Outcome[None]:
    """Writes one ID layer file per matte of a written multilayer file, one layer per matte name a scene block colors, refusing a path equal to source."""

    def painted(path: OutputPath, matte: Matte, layer: bpy.types.ViewLayer, entity: CryptomatteKind) -> Outcome[str]:
        shown = {name: color for name, _, color in _named(layer, entity)}
        known = {name: shown[name] for name in matte.ids if name in shown}
        spec = matte.file.parts[matte.ranks[0].part]
        return (
            matte.view
            .or_else(file.view)
            .map(lambda name: catch(lambda: scene.render.views[name], exception=KeyError)().map(Some).map_error(lambda cause: Fault.of(Reason.ABSENT, repr(scene.render.views), cause, member=name)))
            .default_value(Ok(Nothing))
            .bind(lambda view: path.literal(layer.name, Output.MATTE, Some(entity.pass_name), scene.frame_current, suffix=Some(f"{view.map(lambda held: held.file_suffix).default_value('')}.exr")))
            .map(str)
            .filter_with(lambda target: target != file.path, lambda target: Fault.of(Refusal.TAKEN, target, member=file.path))
            .bind(
                lambda target: Multilayer.written(
                    Path(target),
                    tuple(Pass(layer=name, channels=("R", "G", "B", "A"), role=PassRole.COLOR) for name in known),
                    PassLayout.PARTS,
                    Storage(depth=Depth.HALF, compression=Some(Deflate())),
                    (spec.width, spec.height),
                    identity.manifest.stamp,
                    lambda held: matte.filled(frozenset({held.layer}), np.asarray(known[held.layer])),
                )
            )
            .map(str)
        )

    def matted(path: OutputPath, matte: Matte) -> Outcome[str]:
        return _located(scene, matte, file.layer).bind(lambda pair: painted(path, matte, *pair))

    return (
        paired(
            ScenePaths().bound(identity).bind(lambda pathed: pathed.path(scene, today())),
            Multilayer.read(Path(file.path)).bind(lambda frame: Matte.found(frame, header=next(iter(frame.parts.values())))),
        )
        .bind(lambda found: accumulated(matted(found[0], matte) for matte in found[1]))
        .map(lambda _: None)
    )


def document(_identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document]:
    """Builds the ID document of a written frame, one layer group per matte and one layer per matte name in its block's display color."""
    first = next(iter(frame.parts.values()))
    size = (first.height, first.width)

    def grouped(index: int, matte: Matte) -> Outcome[tuple[LayerGroup, Option[npt.NDArray[np.float32]]]]:
        spec = matte.file.parts[matte.ranks[0].part]
        size = (spec.height, spec.width)

        def cover(name: str, color: tuple[float, float, float]) -> Outcome[tuple[str, tuple[float, float, float], npt.NDArray[np.float32]]]:
            shown = mathutils.Color(color).from_scene_linear_to_srgb()
            return matte.coverage(frozenset({name})).map(lambda plane: (name, (shown.r, shown.g, shown.b), plane))

        def covered(layer: bpy.types.ViewLayer, entity: CryptomatteKind) -> Outcome[tuple[tuple[str, tuple[float, float, float], npt.NDArray[np.float32]], ...]]:
            return accumulated(cover(name, color) for name, _, color in _named(layer, entity))

        def layered(name: str, color: tuple[float, float, float], plane: npt.NDArray[np.float32]) -> Outcome[PixelLayer]:
            return paired(depth.coded(np.broadcast_to(np.asarray(color, dtype=np.float32), (*size, 3))), depth.coded(plane)).map(lambda codes: PixelLayer(name=name, color=codes[0], alpha=codes[1]))

        return _located(scene, matte).bind(
            lambda pair: covered(*pair).bind(
                lambda rows: accumulated(itertools.starmap(layered, rows)).map(
                    lambda layers: (
                        LayerGroup(name=matte.view.map(lambda view: f"{matte.name} ({view})").default_value(matte.name), visible=index == 0, layers=layers),
                        Some(sum((plane[..., None] * np.asarray((*color, 1.0), dtype=np.float32) for _, color, plane in rows), start=np.zeros((*size, 4), np.float32))) if index == 0 else Nothing,
                    )
                )
            )
        )

    return Result[tuple[Matte, ...], Faults].bind(
        Matte.found(frame, header=first),
        lambda mattes: accumulated(itertools.starmap(grouped, enumerate(mattes))).bind(
            lambda groups: depth.coded(unassociated(next((composite for _, composite in groups), Nothing).default_with(lambda: np.zeros((*size, 4), np.float32)))).map(
                lambda composite: Document(layers=tuple(group for group, _ in groups), composite=composite, colorspace="sRGB", density=scene.render.ppm_factor / scene.render.ppm_base)
            )
        ),
    )


# --- [LOBES]


def lobe_group() -> Outcome[NodeGroup]:
    """Builds shared lobe total group, `(direct + indirect) * color`."""
    interface = Interface(
        outputs=(Socket(key="total", kind=bpy.types.NodeSocketColor),),
        inputs=tuple(Socket(key=slot, kind=bpy.types.NodeSocketColor, default_value=(1.0, 1.0, 1.0, 1.0) if slot is LightSlot.COLOR else (0.0, 0.0, 0.0, 1.0)) for slot in LightSlot),
    )
    terms = {slot: group_input(slot, bpy.types.NodeSocketColor) for slot in LightSlot}
    return built("lobe_total", interface, {"total": (terms[LightSlot.DIRECT] + terms[LightSlot.INDIRECT]) * terms[LightSlot.COLOR]}, bpy.types.CompositorNodeTree)


def lobed(slots: tuple[frozenset[Descriptor], frozenset[Descriptor], frozenset[Descriptor]], offered: Mapping[str, Term], group: NodeGroup) -> Option[Expr]:
    """Places lobe group over the live Render Layers outputs a family's direct, indirect, and color flags name, `Nothing` without a direct or indirect output."""
    linked: dict[str, Term] = {slot: offered[name] for slot, flags in zip(LightSlot, slots, strict=True) for name in sorted(flag.prop.name for flag in flags) if name in offered}
    return Some(placed(group, linked, "total")) if linked.keys() & {LightSlot.DIRECT, LightSlot.INDIRECT} else Nothing


# --- [AXES]


def _turned(vector: Expr, axes: tuple[_Axis, _Axis, _Axis], camera: Option[bpy.types.Object]) -> Expr:
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
            socket_type=bpy.types.NodeSocketVector,
        )
    ).default_value(vector)
    names: Mapping[int, Literal["X", "Y", "Z"]] = {1: "X", 2: "Y", 3: "Z"}
    return rotated if axes == (1, 2, 3) else xyz(*(-rotated[names[-axis]] if axis < 0 else rotated[names[axis]] for axis in axes))


def sources(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, rendered: Expr, live: bpy.types.Node, routing: Routing, role: Role, group: NodeGroup) -> Outcome[tuple[FileSocket, ...]]:
    """Returns sources layer files write in socket order, derived layers and ID images after, each kept by its file row's `use_output`."""
    families = PassFlags.of().families
    offered = {socket.name: socket for socket in live.outputs if socket.enabled and socket.name != "Alpha" and base_socket(type(socket)) in DATA_TYPES}
    outputs = {name: msgspec.structs.replace(rendered, socket=socket.identifier, socket_type=type(socket)) for name, socket in offered.items()}
    spaces = {"Normal": VectorSpace[routing.normals], "Position": VectorSpace.WORLD, DataPass.POSITION.name.lower(): VectorSpace.WORLD, DataPass.REFERENCE.name.lower(): VectorSpace.WORLD}
    rows = {row.name: row for row in routing.files}

    def key(owner: type[object], member: str | None) -> Key:
        return f"i{content_hash(owner, member).hex()}"

    def missing(name: str) -> Faults:
        return Fault.of(Reason.ABSENT, repr(live.outputs), member=name)

    def sourced(expr: Expr, key: Key, name: str, socket_type: type[bpy.types.NodeSocket], output: Output, dimensions: Option[int] = Nothing) -> tuple[FileSocket, ...]:
        listed = Option.of_optional(rows.get(name))
        return (
            ()
            if listed.filter(lambda row: not row.use_output).is_some()
            else (FileSocket(key=key, name=name, term=expr, socket_type=socket_type, dimensions=dimensions, output=listed.map(lambda row: Output[row.output]).default_value(output)),)
        )

    def oriented(name: str) -> Outcome[Expr]:
        read = outputs[name]
        match spaces.get(name), routing.nuke:
            case VectorSpace.CAMERA, nuke:
                return (
                    Option
                    .of_optional(scene.camera)
                    .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(scene), member="camera"))
                    .map(lambda held: _turned(read, _NUKE_AXES[VectorSpace.CAMERA] if nuke else (1, 2, 3), Some(held)))
                )
            case VectorSpace() as space, True:
                return Ok(_turned(read, _NUKE_AXES[space], Nothing))
            case None, True if name == "Vector":
                return Ok(rgba(read["Blue"], read["Alpha"], read["Red"], read["Green"]))
            case _:
                return Ok(read)

    def passed(name: str, socket: bpy.types.NodeSocket) -> Outcome[tuple[FileSocket, ...]]:
        dimensions = Descriptor.resolve(socket, ("default_value",), bpy.types.FloatProperty).map(lambda member: Some(member.prop.array_length)) if socket.type == "VECTOR" else Ok(Nothing)
        classed = (
            Output.MATTE
            if any(name.startswith(entity.pass_name) and name.removeprefix(entity.pass_name).isdigit() for entity in CryptomatteKind)
            else Output.DATA
            if role is Role.DATA or socket.type != "RGBA"
            else Output.BEAUTY
        )
        return paired(oriented(name), dimensions).map(lambda pair: sourced(pair[0], key(type(live), socket.identifier), name, base_socket(type(socket)), classed, pair[1]))

    def totaled(family: Family) -> Outcome[tuple[FileSocket, ...]]:
        return (
            lobed(families[family], outputs, group)
            .to_result_with(lambda: missing(family.total))
            .map(lambda summed: sourced(summed, key(type(family), family.name), family.total, bpy.types.NodeSocketColor, Output.BEAUTY))
        )

    def lighting(slots: tuple[tuple[frozenset[Descriptor], frozenset[Descriptor], frozenset[Descriptor]], ...], kind: Derived) -> Outcome[tuple[FileSocket, ...]]:
        match tuple(found for held in slots for found in lobed(held, outputs, group).to_list()):
            case (first, *rest):
                return Ok(sourced(sum(rest, first), key(type(kind), kind.name), kind.label, bpy.types.NodeSocketColor, Output.BEAUTY))
            case _:
                return Ok(())

    def identified(entity: CryptomatteKind) -> Outcome[tuple[FileSocket, ...]]:
        source = f"{layer.name}.{entity.pass_name}"
        return accumulated(
            Ok((Expr(kind=bpy.types.CompositorNodeCryptomatteV2, members={"source": "RENDER", "scene": IdName.of(scene), "layer_name": source, "matte_id": name}, socket="Matte"), color))
            if "," not in name and name == name.strip()
            else Error(Fault.of(Reason.INVALID, name, f"{source} matte IDs split at commas and trim spaces"))
            for name, _, color in _named(layer, entity)
        ).map(
            lambda covers: (
                sourced(
                    with_alpha(functools.reduce(Expr.__add__, (cover * (*color, 1.0) for cover, color in covers)), functools.reduce(Expr.__add__, (cover for cover, _ in covers))),
                    key(type(entity), entity.name),
                    f"{entity.label} IDs",
                    bpy.types.NodeSocketColor,
                    Output.MATTE,
                )
                if covers
                else ()
            )
        )

    derived = flagged(Derived, routing.derived)
    depth = Option.of_optional(outputs.get("Depth")).to_result_with(lambda: missing("Depth"))
    empty = frozenset[Descriptor]()
    return accumulated((
        *itertools.starmap(passed, offered.items()),
        *map(totaled, flagged(Family, routing.totals)),
        *((lighting(tuple((direct, empty, color) for direct, _, color in families.values()), Derived.DIRECT_LIGHT),) if Derived.DIRECT_LIGHT in derived else ()),
        *((lighting(tuple((empty, indirect, color) for _, indirect, color in families.values()), Derived.INDIRECT_LIGHT),) if Derived.INDIRECT_LIGHT in derived else ()),
        *(
            (
                depth.map(
                    lambda read: sourced(
                        Expr(kind=bpy.types.CompositorNodeNormalize, inputs=(("Value", read),)), key(Derived, Derived.DEPTH_MAP.name), Derived.DEPTH_MAP.label, bpy.types.NodeSocketFloat, Output.DATA
                    )
                ),
            )
            if Derived.DEPTH_MAP in derived
            else ()
        ),
        *(
            (depth.map(lambda read: sourced(1.0 / read, key(Derived, Derived.INVERSE_DEPTH.name), Derived.INVERSE_DEPTH.label, bpy.types.NodeSocketFloat, Output.DATA)),)
            if Derived.INVERSE_DEPTH in derived
            else ()
        ),
        *map(identified, flagged(CryptomatteKind, routing.ids)),
    )).map(lambda parts: tuple(source for part in parts for source in part))


# --- [FILES]


def file_outputs(found: tuple[FileSocket, ...], split: FileSplit) -> Outcome[tuple[FileOutput, ...]]:
    """Splits layer sources into File Output nodes, one multilayer file unless split, each node in its output class's EXR."""
    match split:
        case FileSplit.ONE:
            classed: tuple[tuple[Output, tuple[FileSocket, ...]], ...] = ((Output.BEAUTY, found),) if found else ()
        case FileSplit.CLASSES | FileSplit.PASSES:
            classed = tuple((output, held) for output in Output if output is not Output.PREVIEW for held in (tuple(source for source in found if source.output is output),) if held)
    return accumulated(OutputFormat.exr(output, layered=split is not FileSplit.PASSES).map(lambda chosen: FileOutput(output=output, format=chosen, sources=held)) for output, held in classed)


def file_node(pathed: Pathed, path: OutputPath, layer: str, file_output: FileOutput, key: Key, *, nuke: bool) -> Outcome[Expr]:
    """Builds File Output node under `key` in its node format, data items of a file per pass overriding it with full-float EXR, items under the names the host stores."""
    data = OutputFormat.exr(Output.DATA, layered=False).to_option().filter(lambda _: not file_output.format.layered)

    def item(source: FileSocket, requested: str) -> FileItem:
        overridden = data.filter(lambda _: source.socket_type is not bpy.types.NodeSocketColor)
        return FileItem(
            name=requested,
            socket_type=DATA_TYPES[source.socket_type],
            vector_socket_dimensions=source.dimensions.default_value(msgspec.UNSET),
            override_node_format=overridden.map(lambda _: True).default_value(msgspec.UNSET),
            save_as_render=overridden.map(lambda _: False).default_value(msgspec.UNSET),
            format=overridden.default_value(msgspec.UNSET),
        )

    def node(paths: NodePaths) -> Expr:
        content = FileItems(items={source.key: item(source, requested) for source, requested in zip(file_output.sources, paths.items)})
        members = dict[str | RnaPath, Value](itertools.chain(((member.path, held) for member, held in paths.members.items()), file_output.format.members(("format",)).items()))
        return Expr(kind=bpy.types.CompositorNodeOutputFile, inputs=tuple((source.key, source.term) for source in file_output.sources), members=members, content=Some(content), key=Some(key))

    return pathed.node(
        path, layer, file_output.output, tuple(_NUKE_LAYERS.get(source.name, source.name) if nuke else source.name for source in file_output.sources), layered=file_output.format.layered
    ).map(node)


# --- [COMPOSITION] ----------------------------------------------------------------------

ROUTING: Final = Attachment(key="passes", form=Routing, owner=bpy.types.ViewLayer)
ids_export: Final = Finish(run=exported, formats=frozenset({"OPEN_EXR_MULTILAYER"}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "NUKE_CHANNELS",
    "ROUTING",
    "FileOutput",
    "FileSocket",
    "FileSplit",
    "PassFile",
    "Recolor",
    "Recoloring",
    "Routing",
    "VectorSpace",
    "document",
    "exported",
    "file_node",
    "file_outputs",
    "ids_export",
    "ids_recolor",
    "lobe_group",
    "lobed",
    "sources",
]
