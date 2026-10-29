"""Content hashes of data-blocks over their saved members, element buffers, pixels, and node graphs, and the upstream walk of a node."""

from collections.abc import Iterable
from functools import partial, reduce
from itertools import starmap, takewhile
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.ids.images import pixels
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Descriptor, DTYPES
from blender.bpy.data.rna.paths import definition, Struct, struct_at
from blender.bpy.data.rna.snapshots import Snapshot
from blender.records.faults import accumulated, Outcome, paired
from blender.records.identity import content_hash, ContentHash

# --- [TYPES] ----------------------------------------------------------------------------

type _Buffer = npt.NDArray[np.bool | np.int32 | np.float32]
type _Dtype = type[np.bool | np.int32 | np.float32]
type _Elements[T] = bpy.types.bpy_prop_collection[T]
type _Arrays = tuple[npt.NDArray[np.bool | np.int32 | np.float32 | np.float64], ...]
type _Parts = tuple[tuple[Struct, ...], tuple[bpy.types.Attribute, ...], _Arrays, tuple[tuple[str, str | None], ...]]
type _Captured = tuple[Snapshot, tuple[Option[ContentHash], ...]]
type _Frontier = tuple[tuple[bpy.types.Node, ...], frozenset[bpy.types.Node]]
type _Ends = tuple[bpy.types.Node, bpy.types.NodeSocket, bpy.types.Node, bpy.types.NodeSocket]

# --- [CONSTANTS] ------------------------------------------------------------------------

_UNHASHED: Final = frozenset({"MOVIE", "SEQUENCE", "TILED"})
_COMPOSITE: Final = frozenset({"MULTILAYER", "RENDER_RESULT"})

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [HASHES]


def fingerprinted(owner: bpy.types.ID | bpy.types.Node, *, weighted: bool = False, entering: Option[str] = Nothing) -> Outcome[Option[ContentHash]]:
    """Hashes the content of a data-block or of a node's upstream graph.

    Args:
        owner: Image, node tree, node, object, mesh, curve, text, hair curves, point cloud, Grease Pencil, or any other data-block, the last hashed by its members alone.
        weighted: Whether mesh vertex group weights join the key.
        entering: Object modifier whose input geometry the key covers: the object's data and vertex groups, the modifiers before it with their groups, and its own members.

    Returns:
        Content hash, Nothing for an image or graph whose content the file holds no buffer of, capture faults of any struct accumulated.
    """

    def buffers[T](values: _Elements[T], members: Iterable[bpy.types.Property]) -> tuple[_Buffer, ...]:
        count = len(values)

        def read(member: bpy.types.Property, dtype: _Dtype) -> _Buffer:
            buffer: _Buffer = np.empty(count * max(int(np.prod(Descriptor.dimensions(member))), 1), dtype)
            values.foreach_get(member.identifier, buffer)
            return buffer

        return tuple(read(member, DTYPES[type(member)]) for member in members if type(member) in DTYPES)

    def image(image: bpy.types.Image) -> Outcome[Option[ContentHash]]:
        transient = image.source in _UNHASHED or image.type in _COMPOSITE or image.is_multiview
        empty = (image.source == "VIEWER" and not image.has_data) or not np.asarray(image.size).prod()
        if transient or empty:
            return Ok(Nothing)
        members = Descriptor.saved(image, lambda prop: prop != bpy.types.Image.bl_rna.properties["pixels"])
        settings = msgspec.msgpack.encode((image.type, image.is_float))
        packed = tuple(np.frombuffer(file.data, np.uint8) for part in image.packed_files for file in Option.of_optional(part.packed_file).to_list())
        return paired(pixels(image), Snapshot.capture(image, Some(members))).map(lambda pair: Some(content_hash(bpy.types.Image, pair[1], settings, arrays=(pair[0], *packed))))

    def graph(owner: bpy.types.NodeTree | bpy.types.Node) -> Outcome[Option[ContentHash]]:
        tree: bpy.types.NodeTree = owner if isinstance(owner, bpy.types.NodeTree) else owner.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        nodes = tuple(sorted(tree.nodes, key=lambda item: item.name)) if isinstance(owner, bpy.types.NodeTree) else upstream(owner)
        reached = frozenset(nodes)
        settings = Descriptor.saved(tree, lambda prop: not isinstance(prop, bpy.types.CollectionProperty))

        def referenced(struct: Struct, snapshot: Snapshot) -> Outcome[_Captured]:
            blocks = dict.fromkeys(
                block
                for at, held in snapshot.tree.owners.items()
                for name, value in held.values.items()
                if isinstance(value, IdName)
                for block in struct_at(struct, (*at, *name), bpy.types.ID).to_list()
                if isinstance(block, bpy.types.NodeTree | bpy.types.Image)
            )
            return accumulated(map(fingerprinted, blocks)).map(lambda hashes: (snapshot, hashes))

        def linked(link: bpy.types.NodeLink, ends: _Ends) -> Outcome[ContentHash]:
            source, outlet, target, inlet = ends
            connection = msgspec.msgpack.encode((source.name, outlet.identifier, target.name, inlet.identifier))
            return Snapshot.capture(link, Some(Descriptor.saved(link, lambda prop: not prop.is_readonly))).map(lambda snapshot: content_hash(bpy.types.NodeLink, connection, snapshot))

        return paired(
            accumulated(starmap(lambda struct, selection: Snapshot.capture(struct, selection).bind(partial(referenced, struct)), ((tree, Some(settings)), *((item, Nothing) for item in nodes)))),
            accumulated(linked(link, ends) for link in tree.links for ends in (endpoints(link),) if ends[2] in reached),
        ).map(
            lambda parts: reduce(
                lambda held, part: held.map2(lambda prior, current: content_hash(type(owner), prior, current), part),
                (key for _, hashes in parts[0] for key in hashes),
                Some(content_hash(type(owner), *(snapshot for snapshot, _ in parts[0]), *parts[1])),
            )
        )

    def elements(block: bpy.types.ID) -> Outcome[ContentHash]:
        shapes = tuple(Option.of_optional(block.shape_keys).to_list()) if isinstance(block, bpy.types.Mesh | bpy.types.Curve) else ()
        keys = tuple(key for shape in shapes for key in shape.key_blocks)
        materials = tuple(block.materials) if isinstance(block, bpy.types.Mesh | bpy.types.Curve | bpy.types.Curves | bpy.types.PointCloud | bpy.types.GreasePencil) else ()

        def parts(structs: tuple[Struct, ...]) -> _Parts:
            match block:
                case bpy.types.Mesh():
                    weights = (
                        (np.fromiter((value for index, vertex in enumerate(block.vertices) for element in vertex.groups for value in (index, element.group, element.weight)), np.float64),)
                        if weighted
                        else ()
                    )
                    arrays = (
                        *buffers(block.polygons, (bpy.types.MeshPolygon.bl_rna.properties["loop_start"],)),
                        *(array for key in keys for array in buffers(key.points, bpy.types.ShapeKeyPoint.bl_rna.properties)),
                        *(array for layer in block.skin_vertices for array in buffers(layer.data, bpy.types.MeshSkinVertex.bl_rna.properties)),
                        *weights,
                    )
                    return structs, tuple(block.attributes), arrays, ()
                case bpy.types.Curve():
                    profiles = tuple(Option.of_optional(block.bevel_profile).to_list())
                    arrays = (
                        *buffers(block.splines, bpy.types.Spline.bl_rna.properties),
                        *(array for spline in block.splines for array in buffers(spline.points, bpy.types.SplinePoint.bl_rna.properties)),
                        *(array for spline in block.splines for array in buffers(spline.bezier_points, bpy.types.BezierSplinePoint.bl_rna.properties)),
                        *(array for key in keys for array in buffers(key.data, (bpy.types.ShapeKeyPoint.bl_rna.properties["co"],))),
                        *(array for profile in profiles for array in buffers(profile.points, bpy.types.CurveProfilePoint.bl_rna.properties)),
                        *(
                            (*buffers(block.body_format, bpy.types.TextCharacterFormat.bl_rna.properties), *buffers(block.text_boxes, bpy.types.TextBox.bl_rna.properties))
                            if isinstance(block, bpy.types.TextCurve)
                            else ()
                        ),
                    )
                    return (*structs, *profiles), (), arrays, ()
                case bpy.types.Curves():
                    return structs, tuple(block.attributes), buffers(block.curve_offset_data, bpy.types.IntAttributeValue.bl_rna.properties), ()
                case bpy.types.PointCloud():
                    return structs, tuple(block.attributes), (), ()
                case bpy.types.GreasePencil():
                    drawings = tuple(drawing for layer in block.layers for frame in layer.frames for drawing in Option.of_optional(frame.drawing).to_list())
                    nodes = (*block.layers, *block.layer_groups)
                    arrays = (
                        *(array for layer in block.layers for array in buffers(layer.frames, bpy.types.GreasePencilFrame.bl_rna.properties)),
                        *(array for drawing in drawings for array in buffers(drawing.curve_offsets, bpy.types.IntAttributeValue.bl_rna.properties)),
                    )
                    labels = tuple((node.name, Option.of_optional(node.parent_group).map(lambda group: group.name).to_optional()) for node in nodes)
                    return (block, *nodes), (*block.attributes, *(attribute for drawing in drawings for attribute in drawing.attributes)), arrays, labels
                case _:
                    return structs, (), (), ()

        structs, attributes, arrays, labels = parts((block, *shapes, *keys))
        columns = tuple(
            array
            for attribute in attributes
            for data in (definition(attribute).properties["data"],)
            if isinstance(data, bpy.types.CollectionProperty)
            for item in Option.of_optional(data.fixed_type).to_list()
            for array in buffers(getattr(attribute, data.identifier), item.properties)
        )
        named = msgspec.msgpack.encode((
            tuple((attribute.name, attribute.domain, attribute.data_type) for attribute in attributes),
            tuple(Option.of_optional(material).map(IdName.of).to_optional() for material in materials),
            tuple(key.relative_key.name for key in keys),
            labels,
        ))

        def valued(member: bpy.types.Property) -> bool:
            return not (isinstance(member, bpy.types.CollectionProperty) or (isinstance(member, bpy.types.PointerProperty) and member.is_readonly))

        return accumulated(Snapshot.capture(struct, Some(Descriptor.saved(struct, valued))) for struct in structs).map(
            lambda snapshots: content_hash(type(block), *snapshots, named, arrays=(*columns, *arrays))
        )

    def item(block: bpy.types.Object) -> Outcome[Option[ContentHash]]:
        earlier = tuple(takewhile(lambda modifier: entering != Some(modifier.name), block.modifiers))
        named = tuple(modifier for modifier in block.modifiers if entering == Some(modifier.name))[:1]
        groups = msgspec.msgpack.encode(tuple(group.name for group in block.vertex_groups))
        return paired(
            accumulated(Snapshot.capture(modifier) for modifier in (*earlier, *named)),
            accumulated(fingerprinted(group) for modifier in earlier if isinstance(modifier, bpy.types.NodesModifier) for group in Option.of_optional(modifier.node_group).to_list()),
            accumulated(fingerprinted(data, weighted=bool(block.vertex_groups)) for data in Option.of_optional(block.data).to_list()),
        ).map(
            lambda parts: reduce(
                lambda held, part: held.map2(lambda prior, current: content_hash(bpy.types.Object, prior, current), part),
                (*parts[1], *parts[2]),
                Some(content_hash(bpy.types.Object, *parts[0], groups)),
            )
        )

    match owner:
        case bpy.types.Image():
            return image(owner)
        case bpy.types.Object():
            return item(owner)
        case bpy.types.NodeTree() | bpy.types.Node():
            return graph(owner)
        case _:
            return elements(owner).map(Some)


# --- [WALKS]


def upstream(node: bpy.types.Node) -> tuple[bpy.types.Node, ...]:
    """Returns node and every node its input links reach, sorted by name."""
    tree: bpy.types.NodeTree = node.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    ends = tuple(map(endpoints, tree.links))
    feeding = {target: tuple(source for source, _, held, _ in ends if held == target) for _, _, target, _ in ends}

    def step(frontier: _Frontier) -> Option[tuple[bpy.types.Node, _Frontier]]:
        pending, seen = frontier
        match tuple(dict.fromkeys(item for item in pending if item not in seen)):
            case (current, *rest):
                return Some((current, ((*rest, *feeding.get(current, ())), seen | {current})))
            case _:
                return Nothing

    start: _Frontier = ((node,), frozenset())
    return tuple(sorted(Block.unfold(step, start), key=lambda item: item.name))


def endpoints(link: bpy.types.NodeLink) -> _Ends:
    """Returns the nodes and sockets at both ends of a link."""
    source: bpy.types.Node = link.from_node  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    outlet: bpy.types.NodeSocket = link.from_socket  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    target: bpy.types.Node = link.to_node  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    inlet: bpy.types.NodeSocket = link.to_socket  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return source, outlet, target, inlet


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["endpoints", "fingerprinted", "upstream"]
