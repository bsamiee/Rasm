"""Data-block content hashes over members, buffers, pixels, node graphs, and compositor entries, sharing dependencies across roots."""

from collections import deque
from collections.abc import Iterable
from functools import partial, reduce
from graphlib import TopologicalSorter
from itertools import starmap, takewhile
import math
from typing import Final, overload

import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from rasm_bpy.data._ids.images import _pixels
from rasm_bpy.data._ids.store import IdName
from rasm_bpy.data._rna.descriptors import _DTYPES, Descriptor, Value
from rasm_bpy.data._rna.paths import _definition, _struct_at, Native, RnaPath, Struct
from rasm_bpy.data._rna.snapshots import _Capture, Snapshot
from rasm_bpy.nodes._trees import _endpoints, _upstream
from rasm_records import accumulated, content_hash, ContentHash, Fault, Nothing, Ok, Outcome, paired, Reason, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type _Buffer = npt.NDArray[np.bool | np.int32 | np.float32]
type _Elements[T] = bpy.types.bpy_prop_collection[T]
type _Parts = tuple[tuple[Struct, ...], tuple[npt.NDArray[np.bool | np.int32 | np.float32 | np.float64], ...], tuple[tuple[str, str | None], ...]]
type _Subject = bpy.types.ID | bpy.types.Node | bpy.types.SceneCompositorEffect

# --- [CONSTANTS] ------------------------------------------------------------------------

_UNHASHED: Final = frozenset({"MOVIE", "SEQUENCE", "TILED"})
_COMPOSITE: Final = frozenset({"MULTILAYER", "RENDER_RESULT"})

# --- [OPERATIONS] -----------------------------------------------------------------------


@overload
def fingerprinted(owner: _Subject, *, weighted: bool = False, before_modifier: Option[str] = Nothing) -> Outcome[Option[ContentHash]]: ...
@overload
def fingerprinted(owner: tuple[_Subject, ...], *, weighted: bool = False, before_modifier: Option[str] = Nothing) -> Outcome[tuple[Option[ContentHash], ...]]: ...
def fingerprinted(owner: _Subject | tuple[_Subject, ...], *, weighted: bool = False, before_modifier: Option[str] = Nothing) -> Outcome[Option[ContentHash]] | Outcome[tuple[Option[ContentHash], ...]]:
    """Hashes captured data-block content, upstream node graphs, or compositor entries, sharing dependencies across tuple roots.

    Args:
        owner: Image, node tree, node, compositor entry, object, any data-block, or tuple of roots; geometry adds element buffers to members.
        weighted: Whether mesh vertex group weights join the key.
        before_modifier: Object modifier whose input geometry the key covers: object data, vertex groups, earlier modifiers with their groups, and its own members.

    Returns:
        Content hash or tuple of hashes, Nothing for transient images or uncaptured external content, capture faults in root and dependency order.

    Notes:
        Graph keys capture saved members and referenced node trees and images. Evaluated frame, scene, engine, and color context belong to callers.
    """
    type Key = _Subject | tuple[bpy.types.Mesh, bool]
    type Reading = Outcome[Option[ContentHash]]
    type Step = Reading | Key
    type Prepared = tuple[Reading, tuple[Step, ...]]
    type Captured[T] = Outcome[tuple[T, tuple[bpy.types.ID, ...], bool]]
    type Content = Value | tuple[IdName | None, ...]

    def references[T](struct: Native, tree: Tree[T, RnaPath, RnaPath]) -> tuple[tuple[bpy.types.ID, ...], bool]:
        blocks: dict[bpy.types.ID, None] = {}
        paths = False
        for at, held in tree.rows:
            for name, value in held.rows:
                match value:
                    case IdName():
                        blocks.update(dict.fromkeys(_struct_at(struct, (*at, *name), bpy.types.ID).to_list()))
                    case tuple() if value and (value[0] is None or isinstance(value[0], IdName)):
                        blocks.update(dict.fromkeys(block for collection in _struct_at(struct, (*at, *name), bpy.types.bpy_prop_collection).to_list() for block in collection if block is not None))
                    case str() if value and not paths:
                        paths = any(
                            descriptor.prop.subtype == "FILE_PATH" and not descriptor.prop.is_path_output
                            for descriptor in Descriptor.resolve(struct, (*at, *name), bpy.types.Property).to_option().to_list()
                            if isinstance(descriptor.prop, bpy.types.StringProperty)
                        )
                    case _:
                        pass
        return tuple(blocks), paths

    def capture(struct: Native, selection: Option[tuple[Descriptor, ...]] = Nothing) -> Captured[Snapshot]:
        return Snapshot.capture(struct, selection).map(lambda snapshot: (snapshot, *references(struct, snapshot.tree)))

    def cacheable[T](captures: Iterable[Captured[T]]) -> bool:
        return all(not paths and all(isinstance(block, bpy.types.NodeTree | bpy.types.Image) for block in blocks) for result in captures for _, blocks, paths in result.to_option().to_list())

    def ordered[T](captures: Iterable[Captured[T]]) -> tuple[Step, ...]:
        return tuple[Step, ...](
            step
            for result in captures
            for step in (result.map(lambda _: Nothing), *(block for _, blocks, _ in result.to_option().to_list() for block in blocks if isinstance(block, bpy.types.NodeTree | bpy.types.Image)))
        )

    def buffers[T](values: _Elements[T], members: Iterable[bpy.types.Property]) -> tuple[_Buffer, ...]:
        count = len(values)

        def fetched(member: bpy.types.Property, dtype: type[np.bool | np.int32 | np.float32]) -> _Buffer:
            buffer: _Buffer = np.empty(count * math.prod(Descriptor.dimensions(member)), dtype)
            values.foreach_get(member.identifier, buffer)
            return buffer

        return tuple(fetched(member, _DTYPES[type(member)]) for member in members if type(member) in _DTYPES)

    def image(image: bpy.types.Image) -> Outcome[Option[ContentHash]]:
        transient = image.source in _UNHASHED or image.type in _COMPOSITE or image.is_multiview
        empty = (image.source == "VIEWER" and not image.has_data) or not np.asarray(image.size).prod()
        if transient or empty:
            return Ok(Nothing)
        members = Descriptor.saved(image, lambda prop: prop != bpy.types.Image.bl_rna.properties["pixels"])
        settings = msgspec.msgpack.encode((image.type, image.is_float))
        packed = tuple(np.frombuffer(file.data, np.uint8) for part in image.packed_files for file in Option.of_optional(part.packed_file).to_list())
        return paired(_pixels(image), Snapshot.capture(image, Some(members))).map(lambda pair: Some(content_hash(bpy.types.Image, pair[1], settings, arrays=(pair[0], *packed))))

    def graph(owner: bpy.types.NodeTree | bpy.types.Node) -> Prepared:
        tree: bpy.types.NodeTree = owner if isinstance(owner, bpy.types.NodeTree) else owner.id_data  # ty: ignore[invalid-assignment]
        nodes = tuple(sorted(tree.nodes, key=lambda item: item.name)) if isinstance(owner, bpy.types.NodeTree) else _upstream(owner)
        reached = frozenset(nodes)
        settings = Descriptor.saved(tree, lambda prop: not isinstance(prop, bpy.types.CollectionProperty))

        def linked(link: bpy.types.NodeLink) -> Outcome[ContentHash]:
            source, outlet, target, inlet = _endpoints(link)
            connection = msgspec.msgpack.encode((source.name, outlet.identifier, target.name, inlet.identifier, link.multi_input_sort_id))
            return Snapshot.capture(link, Some(Descriptor.saved(link, lambda prop: not prop.is_readonly))).map(lambda snapshot: content_hash(bpy.types.NodeLink, connection, snapshot))

        captured = tuple(starmap(capture, ((tree, Some(settings)), *((item, Nothing) for item in nodes))))
        links = accumulated(linked(link) for link in tree.links if link.to_node in reached)
        seed = paired(accumulated(result.map(lambda parts: parts[0]) for result in captured), links).map(
            lambda parts: Some(content_hash(type(owner), *parts[0], *parts[1])) if cacheable(captured) else Nothing
        )
        return seed, (*ordered(captured), links.map(lambda _: Nothing))

    def elements(block: bpy.types.ID, *, weighted: bool) -> Prepared:
        shapes = tuple(Option.of_optional(block.shape_keys).to_list()) if isinstance(block, bpy.types.Mesh | bpy.types.Curve | bpy.types.Lattice) else ()
        keys = tuple(key for shape in shapes for key in shape.key_blocks)
        materials = (
            tuple(block.materials)
            if isinstance(block, bpy.types.Mesh | bpy.types.Curve | bpy.types.Curves | bpy.types.PointCloud | bpy.types.GreasePencil | bpy.types.MetaBall | bpy.types.Volume)
            else ()
        )
        drawings = tuple(drawing for layer in block.layers for frame in layer.frames for drawing in Option.of_optional(frame.drawing).to_list()) if isinstance(block, bpy.types.GreasePencil) else ()
        attributes = (
            *(block.attributes if isinstance(block, bpy.types.Mesh | bpy.types.Curves | bpy.types.PointCloud | bpy.types.GreasePencil) else ()),
            *(attribute for drawing in drawings for attribute in drawing.attributes),
        )

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
                    return structs, arrays, ()
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
                    return (*structs, *profiles), arrays, ()
                case bpy.types.Curves():
                    return structs, buffers(block.curve_offset_data, bpy.types.IntAttributeValue.bl_rna.properties), ()
                case bpy.types.Lattice():
                    arrays = (
                        *buffers(block.points, (descriptor.prop for descriptor in Descriptor.saved(bpy.types.LatticePoint, lambda prop: not prop.is_readonly))),
                        *(array for key in keys for array in buffers(key.points, bpy.types.ShapeKeyPoint.bl_rna.properties)),
                    )
                    return structs, arrays, ()
                case bpy.types.MetaBall():
                    return structs, buffers(block.elements, bpy.types.MetaElement.bl_rna.properties), ()
                case bpy.types.GreasePencil():
                    nodes = (*block.layers, *block.layer_groups)
                    arrays = (
                        *(array for layer in block.layers for array in buffers(layer.frames, bpy.types.GreasePencilFrame.bl_rna.properties)),
                        *(array for drawing in drawings for array in buffers(drawing.curve_offsets, bpy.types.IntAttributeValue.bl_rna.properties)),
                    )
                    labels = tuple((node.name, Option.of_optional(node.parent_group).map(lambda group: group.name).to_optional()) for node in nodes)
                    return (block, *nodes), arrays, labels
                case _:
                    return structs, (), ()

        structs, arrays, labels = parts((block, *shapes, *keys))
        data = tuple(
            (getattr(attribute, data.identifier), item.properties)
            for attribute in attributes
            for data in (_definition(attribute).properties["data"],)
            if isinstance(data, bpy.types.CollectionProperty)
            for item in Option.of_optional(data.fixed_type).to_list()
        )
        columns = tuple(array for values, members in data for array in buffers(values, members))
        text = tuple(tuple(getattr(item, member.identifier) for item in values) for values, members in data for member in members if isinstance(member, bpy.types.StringProperty))
        named = msgspec.msgpack.encode((
            tuple((attribute.name, attribute.domain, attribute.data_type) for attribute in attributes),
            tuple(Option.of_optional(material).map(IdName.of).to_optional() for material in materials),
            tuple(key.relative_key.name for key in keys),
            labels,
            *text,
        ))

        def valued(member: bpy.types.Property) -> bool:
            return not (isinstance(member, bpy.types.CollectionProperty) or (isinstance(member, bpy.types.PointerProperty) and member.is_readonly))

        captured = tuple(capture(struct, Some(Descriptor.saved(struct, valued))) for struct in structs)
        seed = accumulated(result.map(lambda parts: parts[0]) for result in captured).map(
            lambda snapshots: Some(content_hash(type(block), *snapshots, named, arrays=(*columns, *arrays))) if cacheable(captured) else Nothing
        )
        return seed, ordered(captured)

    def members(block: bpy.types.ID) -> Prepared:
        native = _Capture[Content](
            block,
            lambda descriptor, struct, _inside, _compound: (
                Some(tuple(Option.of_optional(block).map(IdName.of).to_optional() for block in getattr(struct, descriptor.prop.identifier)))
                if isinstance(descriptor.prop, bpy.types.CollectionProperty)
                else Some(descriptor.read(struct))
            ),
            valued=Some(lambda descriptor: not isinstance(descriptor.prop, bpy.types.CollectionProperty | bpy.types.PointerProperty) or isinstance(descriptor.prop.fixed_type, bpy.types.ID)),
            inactive=True,
        )
        captured = (catch(native.tree, exception=ReferenceError)().map_error(partial(Fault.of, Reason.REFUSED, repr(block))).map(lambda tree: (tree, *references(block, tree))),)
        seed = captured[0].map(
            lambda parts: (
                Some(
                    content_hash(
                        type(block),
                        parts[0],
                        *(
                            (msgspec.msgpack.encode(tuple((bone.path_from_id(), Option.of_optional(bone.parent).map(lambda parent: parent.path_from_id()).to_optional()) for bone in block.bones)),)
                            if isinstance(block, bpy.types.Armature)
                            else ()
                        ),
                    )
                )
                if cacheable(captured)
                else Nothing
            )
        )
        return seed, ordered(captured)

    def item(block: bpy.types.Object) -> Prepared:
        earlier = tuple(takewhile(lambda modifier: before_modifier != Some(modifier.name), block.modifiers))
        named = tuple(modifier for modifier in block.modifiers if before_modifier == Some(modifier.name))[:1]
        groups = msgspec.msgpack.encode(tuple(group.name for group in block.vertex_groups))
        data = tuple(Option.of_optional(block.data).to_list())
        selection = Descriptor.saved(block, lambda prop: prop != bpy.types.Object.bl_rna.properties["modifiers"])
        captured = accumulated((capture(block, Some(selection)), *(capture(modifier) for modifier in (*earlier, *named))))
        seed = captured.map(
            lambda parts: (
                Some(content_hash(bpy.types.Object, *(snapshot for snapshot, _, _ in parts), groups))
                if all(not paths and all(isinstance(held, bpy.types.NodeTree) or held in data for held in blocks) for _, blocks, paths in parts)
                else Nothing
            )
        )
        steps: tuple[Step, ...] = (
            captured.map(lambda _: Nothing),
            *(group for modifier in earlier if isinstance(modifier, bpy.types.NodesModifier) for group in Option.of_optional(modifier.node_group).to_list()),
            *((held, bool(block.vertex_groups)) if isinstance(held, bpy.types.Mesh) else held for held in data),
        )
        return seed, steps

    def effect(entry: bpy.types.SceneCompositorEffect) -> Prepared:
        enables = msgspec.msgpack.encode((entry.enable_for_render, entry.enable_for_preview))
        captured = tuple(capture(struct) for struct in _struct_at(entry, ("properties", "inputs")).to_list())
        seed = accumulated(result.map(lambda parts: parts[0]) for result in captured).map(
            lambda snapshots: Some(content_hash(bpy.types.SceneCompositorEffect, *snapshots, enables)) if cacheable(captured) else Nothing
        )
        return seed, (*ordered(captured), *Option.of_optional(entry.node_group).to_list())

    def prepare(key: Key) -> Prepared:
        subject, weighted = key if isinstance(key, tuple) else (key, False)
        match subject:
            case bpy.types.Image():
                return image(subject), ()
            case bpy.types.Object():
                return item(subject)
            case bpy.types.SceneCompositorEffect():
                return effect(subject)
            case bpy.types.NodeTree() | bpy.types.Node():
                return graph(subject)
            case bpy.types.Mesh() | bpy.types.Curve() | bpy.types.Curves() | bpy.types.PointCloud() | bpy.types.GreasePencil() | bpy.types.Lattice() | bpy.types.MetaBall() | bpy.types.Volume():
                return elements(subject, weighted=weighted)
            case _:
                return members(subject)

    roots: tuple[Key, ...] = tuple((subject, weighted) if isinstance(subject, bpy.types.Mesh) else subject for subject in (owner if isinstance(owner, tuple) else (owner,)))  # ty: ignore[unsound-assignment]
    captured: dict[Key, Prepared] = {}
    pending = deque(roots)
    while pending:
        if (key := pending.popleft()) not in captured:
            _, steps = captured[key] = prepare(key)
            pending.extend(step for step in steps if not isinstance(step, Result))
    hashes: dict[Key, Reading] = {}
    dependencies = {key: tuple(step for step in steps if not isinstance(step, Result)) for key, (_, steps) in captured.items()}
    for key in TopologicalSorter(dependencies).static_order():
        seed, steps = captured[key]
        hashes[key] = accumulated(step if isinstance(step, Result) else hashes[step] for step in steps).bind(
            lambda readings: seed.map(
                lambda own: reduce(
                    lambda held, needed: held.map2(partial(content_hash, type(key[0] if isinstance(key, tuple) else key)), needed),
                    (reading for step, reading in zip(steps, readings) if not isinstance(step, Result)),
                    own,
                )
            )
        )
    return accumulated(hashes[key] for key in roots) if isinstance(owner, tuple) else hashes[roots[0]]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["fingerprinted"]
