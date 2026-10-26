"""Content keys of geometry data-blocks over their saved members, element buffers, and element names."""

from collections.abc import Iterable

import bpy
from expression import Nothing, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.faults import BlenderResult
from blender.records.faults import accumulated
from blender.records.identity import content_hash, ContentHash

# --- [OPERATIONS] -----------------------------------------------------------------------


def fingerprinted(block: bpy.types.ID, *, weighted: bool) -> BlenderResult[ContentHash]:
    """Hashes a data-block's saved members, its shape keys and element buffers, its attribute columns, and the names elements carry.

    Args:
        block: Mesh, curve, text, hair curves, point cloud, Grease Pencil, or any other data-block, the last hashed by its members alone.
        weighted: Whether mesh vertex group weights join the key.

    Returns:
        Content hash, capture faults of any struct accumulated.
    """
    shapes = tuple(Option.of_optional(block.shape_keys).to_list()) if isinstance(block, bpy.types.Mesh | bpy.types.Curve) else ()
    keys = tuple(key for shape in shapes for key in shape.key_blocks)
    materials = tuple(block.materials) if isinstance(block, bpy.types.Mesh | bpy.types.Curve | bpy.types.Curves | bpy.types.PointCloud | bpy.types.GreasePencil) else ()
    structs: tuple[bpy.types.bpy_struct[object], ...] = (block, *shapes, *keys)
    attributes: tuple[bpy.types.Attribute, ...] = ()
    arrays: tuple[npt.NDArray[np.bool | np.int32 | np.float32 | np.float64], ...] = ()
    labels: tuple[tuple[str, str | None], ...] = ()
    match block:
        case bpy.types.Mesh():
            attributes = tuple(block.attributes)
            weights = (np.fromiter((value for index, vertex in enumerate(block.vertices) for element in vertex.groups for value in (index, element.group, element.weight)), np.float64),)
            arrays = (
                *buffers(block.polygons, (bpy.types.MeshPolygon.bl_rna.properties["loop_start"],)),
                *(array for key in keys for array in buffers(key.points, bpy.types.ShapeKeyPoint.bl_rna.properties)),
                *(array for layer in block.skin_vertices for array in buffers(layer.data, bpy.types.MeshSkinVertex.bl_rna.properties)),
                *(weights if weighted else ()),
            )
        case bpy.types.Curve():
            profiles = tuple(Option.of_optional(block.bevel_profile).to_list())
            structs = (*structs, *profiles)
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
        case bpy.types.Curves():
            attributes, arrays = tuple(block.attributes), buffers(block.curve_offset_data, bpy.types.IntAttributeValue.bl_rna.properties)
        case bpy.types.PointCloud():
            attributes = tuple(block.attributes)
        case bpy.types.GreasePencil():
            drawings = tuple(drawing for layer in block.layers for frame in layer.frames for drawing in Option.of_optional(frame.drawing).to_list())
            nodes = (*block.layers, *block.layer_groups)
            structs, attributes = (block, *nodes), (*block.attributes, *(attribute for drawing in drawings for attribute in drawing.attributes))
            arrays = (
                *(array for layer in block.layers for array in buffers(layer.frames, bpy.types.GreasePencilFrame.bl_rna.properties)),
                *(array for drawing in drawings for array in buffers(drawing.curve_offsets, bpy.types.IntAttributeValue.bl_rna.properties)),
            )
            labels = tuple((node.name, Option.of_optional(node.parent_group).map(lambda group: group.name).to_optional()) for node in nodes)
        case _:
            pass
    columns = tuple(
        array
        for attribute in attributes
        for data in (attribute.bl_rna.properties["data"],)
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


def buffers[T](values: "bpy.types.bpy_prop_collection[T]", members: Iterable[bpy.types.Property]) -> tuple[npt.NDArray[np.bool | np.int32 | np.float32], ...]:
    """Reads each boolean, integer, enum, and float member of every element through one `foreach_get` per member.

    Args:
        values: Element collection.
        members: Element members, other kinds left out.

    Returns:
        One flat buffer per read member in member order.
    """
    count = len(values)

    def read(member: bpy.types.Property) -> Option[npt.NDArray[np.bool | np.int32 | np.float32]]:
        buffer: npt.NDArray[np.bool | np.int32 | np.float32]
        match member:
            case bpy.types.BoolProperty(array_length=width):
                buffer = np.empty(count * max(width, 1), np.bool)
            case bpy.types.IntProperty(array_length=width):
                buffer = np.empty(count * max(width, 1), np.int32)
            case bpy.types.FloatProperty(array_length=width):
                buffer = np.empty(count * max(width, 1), np.float32)
            case bpy.types.EnumProperty():
                buffer = np.empty(count, np.int32)
            case _:
                return Nothing
        values.foreach_get(member.identifier, buffer)
        return Some(buffer)

    return tuple(buffer for member in members for buffer in read(member).to_list())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["buffers", "fingerprinted"]
