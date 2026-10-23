"""Field shapes parsed once from record declarations."""

from enum import Enum
import functools
from types import GenericAlias
import typing

import bpy
from expression import Error, Nothing, Ok, Option
import msgspec
import msgspec.inspect as native
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from blender.bpy.faults import BlenderReadFault, BlenderResult
from blender.imaging.faults import accumulated
from blender.imaging.parameters import Case, Gated, GraphForm, Knot, markers, Ramp, Rgb

# --- [TYPES] ----------------------------------------------------------------------------

type Record = type[msgspec.Struct] | GenericAlias
type Shape = Number | Vector | Color | Integer | Flag | Text | OptionalText | Enumerated | Literals | Components | Nested | Gate | Records | Entries | Flags | Pointer | Tagged
type Fields = tuple[tuple[msgspec.structs.FieldInfo, Declared], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

ENABLED, VALUE = Gated.__struct_fields__

# --- [MODELS] ---------------------------------------------------------------------------


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Field shape with title and description from innermost `msgspec.Meta`."""

    shape: Shape
    title: Option[str] = Nothing
    description: Option[str] = Nothing


class Number(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Float scalar."""

    kind: native.FloatType


class Vector(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Tuple of 2 to 4 floats."""

    kind: native.TupleType


class Color(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Color record of float channels."""

    kind: native.StructType


class Integer(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Integer scalar."""

    kind: native.IntType


class Flag(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Boolean scalar."""


class Text(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Text scalar, or OCIO color space name."""

    space: bool = False


class OptionalText(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Text or absence, stored as empty string."""


class Enumerated(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Enum member."""

    enum: type[Enum]


class Literals(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Literal integer choice."""

    values: tuple[int, ...]


class Components(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Fixed tuple other than float vector."""

    items: tuple[Declared, ...]


class Nested(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Nested record other than color or gate."""

    record: type[msgspec.Struct]


class Gate(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Gated record with enabled flag switching value row, markers passed to it."""

    record: Record
    value: Option[tuple[msgspec.structs.FieldInfo, Declared]]


class Records(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Variable tuple of records."""

    record: type[msgspec.Struct]


class Entries(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Enum member to value mapping, one gate per member."""

    enum: type[Enum]
    gate: Gate


class Flags(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Set of enum members."""

    enum: type[Enum]


class Pointer(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Data-block or absence."""

    block: type[bpy.types.ID]


class Tagged(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Tagged union of case records and their integer tags."""

    tag_field: str
    cases: tuple[tuple[type[Case], int], ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def unwrapped(kind: native.Type) -> native.Type:
    """Returns type info without metadata layers."""
    return unwrapped(kind.type) if isinstance(kind, native.Metadata) else kind


def vector(kind: native.Type) -> bool:
    """Returns whether tuple has Blender float vector shape."""
    match unwrapped(kind):
        case native.TupleType(item_types=(native.FloatType() as component, *tail)):
            return len(tail) in {1, 2, 3} and all(item == component for item in tail)
        case _:
            return False


def refused[T](annotation: object, kind: object) -> BlenderResult[T]:
    """Returns read fault for annotation with no supported shape."""
    return Error((BlenderReadFault(request=repr(annotation), error=TypeError(f"{annotation!r} has no supported field shape ({kind!r})")),))


def inherited(annotation: object) -> tuple[object, ...]:
    """Returns markers gated values and tuple items inherit, `msgspec.Meta` and graph forms excluded."""
    return tuple(marker for marker in markers(annotation, object) if not isinstance(marker, msgspec.Meta | GraphForm))


def items(annotation: object) -> tuple[object, ...]:
    """Returns tuple item annotations with tuple markers."""
    extra = inherited(annotation)
    return tuple(
        typing.Annotated[item, *extra] if extra else item for item in typing.get_args(inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").type)
    )


def gate(record: Record) -> BlenderResult[Gate]:
    """Parses gated record, no value row when graph owns value."""
    return field_shapes(record).map(lambda rows: Gate(record=record, value=Option.of_optional(next((row for row in rows if row[0].name == VALUE), None))))


def field_shape(annotation: object) -> BlenderResult[Declared]:
    """Parses field annotation into field shape."""

    def tagged(kind: native.UnionType) -> Option[Tagged]:
        def members(union: native.UnionType) -> tuple[native.Type, ...]:
            return tuple(inner for member in union.types for inner in (members(member) if isinstance(member, native.UnionType) else (member,)))

        flat = members(kind)
        cases = tuple(
            (member.cls, member.tag, member.tag_field)
            for member in flat
            if isinstance(member, native.StructType) and isinstance(member.tag, int) and isinstance(member.tag_field, str) and issubclass(member.cls, Case)
        )
        fields = {tag_field for _, _, tag_field in cases}
        return (
            Option
            .of_optional(next(iter(fields), None))
            .filter(lambda _: len(cases) == len(flat) and len(fields) == 1)
            .map(lambda tag_field: Tagged(tag_field=tag_field, cases=tuple((record, tag) for record, tag, _ in cases)))
        )

    def titled(held: Declared, schema: dict[str, object] | None) -> Declared:
        outer = Option.of_optional(schema).default_value({})
        return msgspec.structs.replace(
            held,
            title=held.title.or_else(Option.of_optional(outer.get("title")).filter(lambda text: isinstance(text, str)).map(str)),
            description=held.description.or_else(Option.of_optional(outer.get("description")).filter(lambda text: isinstance(text, str)).map(str)),
        )

    def leaf(shape: Shape) -> BlenderResult[Declared]:
        return Ok(Declared(shape=shape))

    def parsed(kind: native.Type) -> BlenderResult[Declared]:
        match kind:
            case native.Metadata(type=inner, extra_json_schema=schema):
                return parsed(inner).map(lambda held: titled(held, schema))
            case native.FloatType():
                return leaf(Number(kind=kind))
            case native.TupleType():
                return leaf(Vector(kind=kind)) if vector(kind) else accumulated(map(field_shape, items(annotation))).map(lambda parts: Declared(shape=Components(items=parts)))
            case native.StructType(cls=record) if (typing.get_origin(record) or record) is Gated:
                inner, extra = typing.get_args(record)[0], inherited(annotation)
                return gate(GenericAlias(Gated, typing.Annotated[inner, *extra]) if extra else record).bind(leaf)
            case native.StructType(cls=record):
                return leaf(Color(kind=kind) if (typing.get_origin(record) or record) is Rgb else Nested(record=record))
            case native.IntType():
                return leaf(Integer(kind=kind))
            case native.BoolType():
                return leaf(Flag())
            case native.StrType():
                return leaf(Text(space=GraphForm.COLOR_SPACE in markers(annotation, GraphForm)))
            case native.EnumType(cls=enum):
                return leaf(Enumerated(enum=enum))
            case native.LiteralType(values=values):
                integers = tuple(value for value in values if type(value) is int)
                return leaf(Literals(values=integers)) if len(integers) == len(values) else refused(annotation, kind)
            case native.VarTupleType(item_type=native.StructType(cls=record)):
                return leaf(Records(record=record))
            case native.DictType(key_type=native.EnumType(cls=enum)):
                return gate(GenericAlias(Gated, typing.get_args(inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").type)[1])).bind(
                    lambda held: leaf(Entries(enum=enum, gate=held))
                )
            case native.FrozenSetType(item_type=native.EnumType(cls=enum)) | native.SetType(item_type=native.EnumType(cls=enum)):
                return leaf(Flags(enum=enum))
            case native.UnionType(types=(native.CustomType(cls=block), native.NoneType()) | (native.NoneType(), native.CustomType(cls=block))) if issubclass(block, bpy.types.ID):
                return leaf(Pointer(block=block))
            case native.UnionType(types=(native.StrType(), native.NoneType()) | (native.NoneType(), native.StrType())):
                return leaf(OptionalText())
            case native.UnionType():
                return tagged(kind).map(leaf).default_with(lambda: refused(annotation, kind))
            case _:
                return refused(annotation, kind)

    return parsed(native.type_info(annotation))


@functools.cache
def field_shapes(record: Record) -> BlenderResult[Fields]:
    """Parses record fields once, leaving out graph-owned images, settings, ramps, and knots."""
    fields = msgspec.structs.fields(record)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    parsed = accumulated(field_shape(field.type).map(lambda shape: (field, shape)) for field in fields if not {GraphForm.IMAGE, GraphForm.SETTING} & set(markers(field.type, GraphForm)))
    return parsed.map(
        lambda rows: tuple((field, held) for field, held in rows if not (isinstance(shape := held.shape, Nested | Records) and (typing.get_origin(shape.record) or shape.record) in {Ramp, Knot}))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ENABLED",
    "VALUE",
    "Color",
    "Components",
    "Declared",
    "Entries",
    "Enumerated",
    "Fields",
    "Flag",
    "Flags",
    "Gate",
    "Integer",
    "Literals",
    "Nested",
    "Number",
    "OptionalText",
    "Pointer",
    "Record",
    "Records",
    "Shape",
    "Tagged",
    "Text",
    "Vector",
    "field_shape",
    "field_shapes",
    "inherited",
    "items",
    "refused",
    "unwrapped",
    "vector",
]
