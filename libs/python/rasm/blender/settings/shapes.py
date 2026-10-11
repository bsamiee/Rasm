"""Closed field shapes parsed once from record declarations for native properties and node sockets."""

from enum import Enum
import functools
from types import GenericAlias
import typing

import bpy
from expression import Error, Nothing, Ok, Option
import msgspec
import msgspec.inspect as native

from rasm.blender.faults import BlenderReadFault, BlenderResult
from rasm.imaging.faults import accumulated
from rasm.imaging.parameters import Case, GraphForm, Knot, markers, Ramp, Rgb

# --- [TYPES] ----------------------------------------------------------------------------

type Record = type[msgspec.Struct] | GenericAlias
type PropertyShape = (
    Number | Vector | Color | Integer | Flag | Text | OptionalText | Enumerated | Literals | Components[PropertyShape] | Nested | Records | Entries[PropertyShape] | Flags | Pointer | Tagged
)
type Fields[S] = tuple[tuple[msgspec.structs.FieldInfo, Declared[S]], ...]

# --- [MODELS] ---------------------------------------------------------------------------


class Declared[S](msgspec.Struct, frozen=True, kw_only=True):
    """Field shape with the title and description its innermost `msgspec.Meta` declares."""

    shape: S
    title: Option[str] = Nothing
    description: Option[str] = Nothing


class Number(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Float scalar."""

    kind: native.FloatType


class Vector(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Fixed tuple of two to four equal float components."""

    kind: native.TupleType


class Color(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Color record of float channels."""

    kind: native.StructType


class Integer(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Integer scalar with its declared bounds."""

    kind: native.IntType


class Flag(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Boolean scalar."""


class Text(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Text scalar."""


class OptionalText(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Text or absence, held natively as an empty string."""


class Enumerated(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Member of an enum."""

    enum: type[Enum]


class Literals(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """One of literal integers."""

    values: tuple[int, ...]


class Components[S](msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Fixed tuple of components other than a float vector."""

    items: tuple[Declared[S], ...]


class Nested(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Nested record other than a color."""

    record: type[msgspec.Struct]


class Records(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Variable tuple of records."""

    record: type[msgspec.Struct]


class Entries[S](msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Mapping from enum members to values."""

    enum: type[Enum]
    value: Declared[S]


class Flags(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Set of enum members."""

    enum: type[Enum]


class Pointer(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Native data-block or absence."""

    block: type[bpy.types.ID]


class Tagged(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Tagged union of case records and their integer tags."""

    tag_field: str
    cases: tuple[tuple[type[Case], int], ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def bare(kind: native.Type) -> native.Type:
    """Returns a declaration without its metadata layers."""
    return bare(kind.type) if isinstance(kind, native.Metadata) else kind


def vector(kind: native.Type) -> bool:
    """Whether a fixed tuple has Blender's shared float-vector shape."""
    match kind:
        case native.Metadata(type=inner):
            return vector(inner)
        case native.TupleType(item_types=(native.FloatType() as component, *tail)):
            return len(tail) in {1, 2, 3} and all(item == component for item in tail)
        case _:
            return False


def tagged(kind: native.UnionType) -> Option[Tagged]:
    """Returns a union's case records, nested union aliases flattened, absent unless every member is a tagged case."""

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


def titled[S](held: Declared[S], schema: dict[str, object] | None) -> Declared[S]:
    """Folds an outer metadata layer into an inner shape, the innermost title and description winning."""
    outer = Option.of_optional(schema).default_value({})
    return msgspec.structs.replace(
        held,
        title=held.title.or_else(Option.of_optional(outer.get("title")).filter(lambda text: isinstance(text, str)).map(str)),
        description=held.description.or_else(Option.of_optional(outer.get("description")).filter(lambda text: isinstance(text, str)).map(str)),
    )


def refused[S](annotation: object, kind: native.Type) -> BlenderResult[Declared[S]]:
    """Returns the fault naming an annotation whose declaration has no supported shape."""
    return Error((BlenderReadFault(request=repr(annotation), error=TypeError(f"No supported field shape for {annotation!r}, inspected as {kind!r}")),))


def property_shape(kind: native.Type, annotation: object) -> BlenderResult[Declared[PropertyShape]]:
    """Parses one inspected declaration into its native property shape or the refusal naming its annotation."""

    def leaf(shape: PropertyShape) -> BlenderResult[Declared[PropertyShape]]:
        return Ok(Declared(shape=shape))

    match kind:
        case native.Metadata(type=inner, extra_json_schema=schema):
            return property_shape(inner, annotation).map(lambda held: titled(held, schema))
        case native.FloatType():
            return leaf(Number(kind=kind))
        case native.TupleType(item_types=items):
            return leaf(Vector(kind=kind)) if vector(kind) else accumulated(property_shape(item, annotation) for item in items).map(lambda parts: Declared(shape=Components(items=parts)))
        case native.StructType(cls=record):
            return leaf(Color(kind=kind) if (typing.get_origin(record) or record) is Rgb else Nested(record=record))
        case native.IntType():
            return leaf(Integer(kind=kind))
        case native.BoolType():
            return leaf(Flag())
        case native.StrType():
            return leaf(Text())
        case native.EnumType(cls=enum):
            return leaf(Enumerated(enum=enum))
        case native.LiteralType(values=values):
            integers = tuple(value for value in values if type(value) is int)
            return leaf(Literals(values=integers)) if len(integers) == len(values) else refused(annotation, kind)
        case native.VarTupleType(item_type=native.StructType(cls=record)):
            return leaf(Records(record=record))
        case native.DictType(key_type=native.EnumType(cls=enum), value_type=inner):
            return property_shape(inner, annotation).map(lambda value: Declared(shape=Entries(enum=enum, value=value)))
        case native.FrozenSetType(item_type=native.EnumType(cls=enum)):
            return leaf(Flags(enum=enum))
        case native.UnionType(types=(native.CustomType(cls=block), native.NoneType()) | (native.NoneType(), native.CustomType(cls=block))) if issubclass(block, bpy.types.ID):
            return leaf(Pointer(block=block))
        case native.UnionType(types=(native.StrType(), native.NoneType()) | (native.NoneType(), native.StrType())):
            return leaf(OptionalText())
        case native.UnionType():
            return tagged(kind).map(leaf).default_with(lambda: refused(annotation, kind))
        case _:
            return refused(annotation, kind)


@functools.cache
def property_fields(record: Record) -> BlenderResult[Fields[PropertyShape]]:
    """Parses a record's native property fields once, omitting graph-owned images, settings, ramps, and knots."""

    def omitted(field: msgspec.structs.FieldInfo, kind: native.Type) -> bool:
        bare = kind.type if isinstance(kind, native.Metadata) else kind
        form = bare.item_type if isinstance(bare, native.VarTupleType) else bare
        return bool({GraphForm.IMAGE, GraphForm.SETTING} & set(markers(field.type, GraphForm))) or (isinstance(form, native.StructType) and (form.cls is Ramp or typing.get_origin(form.cls) is Knot))

    fields = msgspec.structs.fields(record)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    return accumulated(
        property_shape(kind, field.type).map(lambda shape: (field, shape)) for field, kind in zip(fields, native.multi_type_info(field.type for field in fields)) if not omitted(field, kind)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Color",
    "Components",
    "Declared",
    "Entries",
    "Enumerated",
    "Fields",
    "Flag",
    "Flags",
    "Integer",
    "Literals",
    "Nested",
    "Number",
    "OptionalText",
    "Pointer",
    "PropertyShape",
    "Record",
    "Records",
    "Tagged",
    "Text",
    "Vector",
    "bare",
    "property_fields",
    "property_shape",
    "refused",
    "tagged",
    "titled",
    "vector",
]
