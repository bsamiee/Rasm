"""Field shapes parsed once from record declarations."""

from enum import auto, Enum
import functools
from types import GenericAlias
import typing

import bpy
from expression import Error, Nothing, Ok, Option
import msgspec
import msgspec.inspect
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.ramps import Ramp
from blender.records.faults import accumulated, DomainFault
from blender.records.fields import Case, Choice, Gated, heading, Knot, markers, Rgb

# --- [TYPES] ----------------------------------------------------------------------------


class GraphForm(Enum):
    """Field a node graph holds in place of a socket: an image node it shows, a node member, or text naming an OCIO color space."""

    IMAGE = auto()
    SETTING = auto()
    COLOR_SPACE = auto()


type Record = type[msgspec.Struct] | GenericAlias
type Shape = Number | Vector | Color | Integer | Flag | Text | OptionalText | Enumerated | Literals | Components | Nested | Gate | Records | Entries | Flags | Pointer | Tagged
type Fields = tuple[tuple[msgspec.structs.FieldInfo, Declared], ...]

# --- [CONSTANTS] ------------------------------------------------------------------------

ENABLED, VALUE = Gated.__struct_fields__
EXPANDED = "show_expanded"

# --- [MODELS] ---------------------------------------------------------------------------


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Field shape with title and description from innermost `msgspec.Meta`."""

    shape: Shape
    title: Option[str] = Nothing
    description: Option[str] = Nothing


class Number(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Float scalar."""

    kind: msgspec.inspect.FloatType


class Vector(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Tuple of 2 to 4 floats."""

    kind: msgspec.inspect.TupleType


class Color(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Color record of float channels."""

    kind: msgspec.inspect.StructType


class Integer(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Integer scalar."""

    kind: msgspec.inspect.IntType


class Flag(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Boolean scalar."""


class Text(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Text scalar, or OCIO color space name."""

    space: bool = False


class OptionalText(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Text or absence, stored as empty string."""


class Enumerated(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Choice member."""

    enum: type[Choice]


class Literals(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Literal integers or host identifiers, each shown under its heading."""

    values: tuple[int, ...] | tuple[str, ...]

    @property
    def labels(self) -> dict[object, str]:
        """Display text per value."""
        return labelled(self.values)


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
    """Choice member to value mapping, one gate per member."""

    enum: type[Choice]
    gate: Gate


class Flags(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Set of choice members."""

    enum: type[Choice]


class Pointer(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Data-block or absence."""

    block: type[bpy.types.ID]


class Tagged(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Tagged union of case records in declared order."""

    tag_field: str
    cases: tuple[type[Case], ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def labelled(values: tuple[object, ...]) -> dict[object, str]:
    """Display text per literal value, its heading."""
    return {value: heading(str(value)) for value in values}


def unwrapped(kind: msgspec.inspect.Type) -> msgspec.inspect.Type:
    """Returns type info without metadata layers."""
    return unwrapped(kind.type) if isinstance(kind, msgspec.inspect.Metadata) else kind


def vector(kind: msgspec.inspect.Type) -> bool:
    """Returns whether tuple has Blender float vector shape."""
    match unwrapped(kind):
        case msgspec.inspect.TupleType(item_types=(msgspec.inspect.FloatType() as component, *tail)):
            return len(tail) in {1, 2, 3} and all(item == component for item in tail)
        case _:
            return False


def refused[T](annotation: object, kind: object) -> BlenderResult[T]:
    """Returns domain fault for annotation with no supported shape."""
    return Error((DomainFault(subject="Blender property binding", value=f"{annotation!r} ({kind!r})", accepted="annotations with a supported field shape"),))


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

    def tagged(kind: msgspec.inspect.UnionType) -> Option[Tagged]:
        def members(union: msgspec.inspect.UnionType) -> tuple[msgspec.inspect.Type, ...]:
            return tuple(inner for member in union.types for inner in (members(member) if isinstance(member, msgspec.inspect.UnionType) else (member,)))

        flat = members(kind)
        cases = tuple((member.cls, member.tag_field) for member in flat if isinstance(member, msgspec.inspect.StructType) and isinstance(member.tag_field, str) and issubclass(member.cls, Case))
        fields = {tag_field for _, tag_field in cases}
        return (
            Option
            .of_optional(next(iter(fields), None))
            .filter(lambda _: len(cases) == len(flat) and len(fields) == 1)
            .map(lambda tag_field: Tagged(tag_field=tag_field, cases=tuple(record for record, _ in cases)))
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

    def parsed(kind: msgspec.inspect.Type) -> BlenderResult[Declared]:
        match kind:
            case msgspec.inspect.Metadata(type=inner, extra_json_schema=schema):
                return parsed(inner).map(lambda held: titled(held, schema))
            case msgspec.inspect.FloatType():
                return leaf(Number(kind=kind))
            case msgspec.inspect.TupleType():
                return leaf(Vector(kind=kind)) if vector(kind) else accumulated(map(field_shape, items(annotation))).map(lambda parts: Declared(shape=Components(items=parts)))
            case msgspec.inspect.StructType(cls=record) if (typing.get_origin(record) or record) is Gated:
                inner, extra = typing.get_args(record)[0], inherited(annotation)
                return gate(GenericAlias(Gated, typing.Annotated[inner, *extra]) if extra else record).bind(leaf)
            case msgspec.inspect.StructType(cls=record):
                return leaf(Color(kind=kind) if (typing.get_origin(record) or record) is Rgb else Nested(record=record))
            case msgspec.inspect.IntType():
                return leaf(Integer(kind=kind))
            case msgspec.inspect.BoolType():
                return leaf(Flag())
            case msgspec.inspect.StrType():
                return leaf(Text(space=GraphForm.COLOR_SPACE in markers(annotation, GraphForm)))
            case msgspec.inspect.EnumType(cls=enum) if issubclass(enum, Choice):
                return leaf(Enumerated(enum=enum))
            case msgspec.inspect.LiteralType(values=values):
                integers, identifiers = tuple(value for value in values if type(value) is int), tuple(value for value in values if type(value) is str)
                return leaf(Literals(values=integers or identifiers)) if len(integers or identifiers) == len(values) else refused(annotation, kind)
            case msgspec.inspect.VarTupleType(item_type=msgspec.inspect.StructType(cls=record)):
                return leaf(Records(record=record))
            case msgspec.inspect.DictType(key_type=msgspec.inspect.EnumType(cls=keys)) if issubclass(keys, Choice):
                return gate(GenericAlias(Gated, typing.get_args(inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").type)[1])).bind(
                    lambda held: leaf(Entries(enum=keys, gate=held))
                )
            case msgspec.inspect.FrozenSetType(item_type=msgspec.inspect.EnumType(cls=enum)) | msgspec.inspect.SetType(item_type=msgspec.inspect.EnumType(cls=enum)) if issubclass(enum, Choice):
                return leaf(Flags(enum=enum))
            case msgspec.inspect.UnionType(types=(msgspec.inspect.CustomType(cls=block), msgspec.inspect.NoneType()) | (msgspec.inspect.NoneType(), msgspec.inspect.CustomType(cls=block))) if (
                issubclass(block, bpy.types.ID)
            ):
                return leaf(Pointer(block=block))
            case msgspec.inspect.UnionType(types=(msgspec.inspect.StrType(), msgspec.inspect.NoneType()) | (msgspec.inspect.NoneType(), msgspec.inspect.StrType())):
                return leaf(OptionalText())
            case msgspec.inspect.UnionType():
                return tagged(kind).map(leaf).default_with(lambda: refused(annotation, kind))
            case _:
                return refused(annotation, kind)

    return parsed(msgspec.inspect.type_info(annotation))


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
    "EXPANDED",
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
    "GraphForm",
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
    "labelled",
    "refused",
    "unwrapped",
    "vector",
]
