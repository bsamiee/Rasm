# mypy: disable-error-code="type-arg"
# ty: ignore[missing-type-argument]
"""Native record fields bound to Blender properties and custom-property metadata."""

from collections.abc import Callable, Iterable, Mapping
from enum import Enum
import functools
import math
from types import GenericAlias, MemberDescriptorType
import typing
from typing import overload, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect as native
import numpy as np
import PyOpenColorIO
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from rasm.blender.data.rna.descriptors import IdName, written
from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.faults import BlenderEditFault, BlenderReadFault, BlenderResult
from rasm.imaging.faults import accumulated
from rasm.imaging.parameters import Case, Choice, Gated, GraphForm, Knot, markers, Presentation, Quantity, Ramp, Rgb

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] -----------------------------------------------------------------------------

type Changed = Callable[[bpy.types.bpy_struct, bpy.types.Context, bpy.types.Property], None]
type SearchSource = Callable[[bpy.types.bpy_struct, bpy.types.Context, str], Iterable[str | tuple[str, str]]]
type PointerPoll[T: bpy.types.ID = bpy.types.ID] = Callable[[Identity, bpy.types.bpy_struct, T], bool]
type EnumRows = Iterable[tuple[str, str, str] | tuple[str, str, str, str | int, int] | None]

# --- [MODELS] ----------------------------------------------------------------------------


class Choices:
    """Dynamic native rows with persistent identifiers and retained callback strings."""

    def __init__(self, source: Callable[[Identity, bpy.types.bpy_struct, bpy.types.Context | None], EnumRows]) -> None:
        """Owns source rows and their session identifiers."""
        self.source = source
        self.numbers: dict[str, int] = {}
        self.rows: dict[int, list[tuple[str, str, str, str | int, int] | None]] = {}

    def defined(self, identity: Identity, key: str, default: object) -> bpy.props._PropertyDeferred:
        """Builds native callbacks retaining identifiers across item changes."""

        def number(identifier: str) -> int:
            return self.numbers.setdefault(identifier, len(self.numbers))

        def listed(owner: bpy.types.bpy_struct, context: bpy.types.Context | None) -> list[tuple[str, str, str, str | int, int] | None]:
            rows = self.rows[owner.as_pointer()] = [
                (*row[:3], row[3] if len(row) == 5 else "NONE", number(row[0]) if row[0] else -1) if row is not None else None for row in self.source(identity, owner, context)
            ]
            return rows

        def selected(owner: bpy.types.bpy_struct) -> int:
            return number(owner.get(key, default))

        def stored(owner: bpy.types.bpy_struct, value: int) -> None:
            owner[key] = next(identifier for identifier, held in self.numbers.items() if held == value)

        return bpy.props.EnumProperty(items=listed, get=selected, set=stored)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


class Definition[T: bpy.types.ID = bpy.types.ID](msgspec.Struct, frozen=True, kw_only=True):
    """Host property options and callbacks attached to a record annotation."""

    options: frozenset["rna_enums.PropertyFlagItems"] = frozenset({"ANIMATABLE"})
    text: "rna_enums.PropertySubtypeStringItems" = "NONE"
    search: Option[SearchSource] = Nothing
    suggestion: bool = True
    poll: Option[PointerPoll[T]] = Nothing


class Unit(msgspec.Struct, frozen=True, kw_only=True):
    """Numeric display units and the conversion from record to host storage."""

    number: "rna_enums.PropertySubtypeNumberItems" = "NONE"
    vector: "rna_enums.PropertySubtypeNumberArrayItems" = "NONE"
    unit: "rna_enums.PropertyUnitItems" = "NONE"
    scale: float = 1.0

    # --- [PRESENTATION]

    @classmethod
    def of(cls, annotation: object, kind: native.Type) -> Self:
        """Returns the host presentation of the selected annotation and numeric kind."""
        match kind:
            case native.Metadata(type=inner):
                return cls.of(annotation, inner)
            case native.StructType(cls=record) if (typing.get_origin(record) or record) is Rgb:
                return cls(vector="COLOR")
            case native.IntType():
                return cls(number="FACTOR" if (kind.ge, kind.le) == (0, 1) else "NONE")
            case _:
                presentation = next(iter(markers(annotation, Presentation)), Presentation())
        match presentation.quantity:
            case Option(tag="some", some=Quantity.LENGTH):
                return cls(number="DISTANCE", vector="XYZ_LENGTH")
            case Option(tag="some", some=Quantity.CAMERA_LENGTH):
                return cls(number="DISTANCE_CAMERA", unit="CAMERA", scale=1000.0)
            case Option(tag="some", some=Quantity.ANGLE):
                return cls(number="ANGLE", vector="EULER")
            case Option(tag="some", some=Quantity.DURATION):
                return cls(number="TIME_ABSOLUTE", unit="TIME_ABSOLUTE")
            case Option(tag="some", some=Quantity.COLOR_TEMPERATURE):
                return cls(number="COLOR_TEMPERATURE", unit="COLOR_TEMPERATURE")
            case Option(tag="some", some=Quantity.IRRADIANCE | Quantity.LUMINANCE):
                return cls()
            case Option(tag="some"):
                return cls(number="FACTOR", vector="XYZ")
            case _:
                match kind:
                    case native.FloatType(ge=low, le=high):
                        return cls(number="FACTOR" if (low, high) == (0, 1) else "NONE")
                    case native.TupleType():
                        factor = all(
                            isinstance(item, native.FloatType) and (item.ge, item.le) == (0, 1) for raw in kind.item_types for item in (raw.type if isinstance(raw, native.Metadata) else raw,)
                        )
                        return cls(number="FACTOR" if factor else "NONE")
                    case _:
                        return cls()

    # --- [STORAGE]

    @staticmethod
    def interval(kind: native.FloatType) -> tuple[float | None, float | None]:
        """Returns the original domain's first and last admissible doubles."""
        return (math.nextafter(kind.gt, math.inf) if kind.gt is not None else kind.ge, math.nextafter(kind.lt, -math.inf) if kind.lt is not None else kind.le)

    def bounds(self, kind: native.Type) -> BlenderResult[tuple[float | None, float | None]]:
        """Returns native float32 endpoints or an empty native storage interval."""
        match kind:
            case native.Metadata(type=inner):
                return self.bounds(inner)
            case native.TupleType() | native.StructType():
                items = kind.item_types if isinstance(kind, native.TupleType) else tuple(field.type for field in kind.fields)

                def intersected(limits: tuple[tuple[float | None, float | None], ...]) -> BlenderResult[tuple[float | None, float | None]]:
                    lows = tuple(low for low, _ in limits if low is not None)
                    highs = tuple(high for _, high in limits if high is not None)
                    low, high = max(lows) if lows else None, min(highs) if highs else None
                    return (
                        Error((BlenderReadFault(request=repr(kind), error=ValueError("No common native float32 storage interval")),))
                        if low is not None and high is not None and low > high
                        else Ok((low, high))
                    )

                return accumulated(self.bounds(item) for item in items).bind(intersected)
            case native.FloatType():
                endpoints = zip(self.interval(kind), (math.inf, -math.inf))
                storage = np.finfo(np.float32)
                answer: list[float | None] = []
                for endpoint, inward in endpoints:
                    if endpoint is None:
                        answer.append(None)
                        continue
                    scaled = endpoint * self.scale
                    limited = max(scaled, float(storage.min)) if inward > 0 else min(scaled, float(storage.max))
                    if not float(storage.min) <= limited <= float(storage.max):
                        return Error((BlenderReadFault(request=repr(kind), error=ValueError("No finite native float32 storage endpoint")),))
                    candidate = np.float32(limited)
                    widened = float(candidate)
                    outside = widened / self.scale < endpoint if inward > 0 else widened / self.scale > endpoint
                    with np.errstate(over="ignore"):
                        boundary = float(np.nextafter(candidate, np.float32(inward))) if outside else widened
                    if boundary == inward:
                        return Error((BlenderReadFault(request=repr(kind), error=ValueError("No finite native float32 storage endpoint")),))
                    answer.append(boundary)
                low, high = answer
                return (
                    Error((BlenderReadFault(request=repr(kind), error=ValueError("No native float32 storage value in interval")),))
                    if low is not None and high is not None and low > high
                    else Ok((low, high))
                )
            case _:
                return Error((BlenderReadFault(request=repr(kind), error=TypeError("Blender numeric properties require floats")),))

    @overload
    def stored(self, value: object, kind: native.FloatType) -> BlenderResult[float]: ...

    @overload
    def stored(self, value: object, kind: native.TupleType | native.StructType) -> BlenderResult[tuple[float, ...]]: ...

    def stored(self, value: object, kind: native.FloatType | native.TupleType | native.StructType) -> BlenderResult[float | tuple[float, ...]]:
        """Returns native float32 storage or a value outside its declared interval."""

        def converted(interval: tuple[float | None, float | None]) -> BlenderResult[float | tuple[float, ...]]:
            try:
                low, high = interval
                storage = np.finfo(np.float32)
                limits = msgspec.Meta(ge=low if low is not None else float(storage.min), le=high if high is not None else float(storage.max))
                source = np.asarray(value, dtype=np.float64)
                match kind:
                    case native.FloatType():
                        with np.errstate(over="ignore"):
                            scaled = float(np.float32(float(source) * self.scale))
                        return Ok(msgspec.convert(scaled, typing.Annotated[float, limits]))
                    case native.TupleType() | native.StructType():
                        with np.errstate(over="ignore"):
                            scaled_items = tuple(map(float, np.asarray(tuple(float(item) * self.scale for item in source), dtype=np.float32)))
                        return Ok(msgspec.convert(scaled_items, tuple[typing.Annotated[float, limits], ...]))
            except (TypeError, ValueError) as error:
                return Error((BlenderReadFault(request=repr(kind), error=error),))

        return self.bounds(kind).bind(converted)

    @overload
    def restored(self, value: object, kind: native.FloatType) -> BlenderResult[float]: ...

    @overload
    def restored(self, value: object, kind: native.TupleType | native.StructType) -> BlenderResult[tuple[float, ...]]: ...

    def restored(self, value: object, kind: native.FloatType | native.TupleType | native.StructType) -> BlenderResult[float | tuple[float, ...]]:
        """Returns native numeric storage in domain units or a storage-range fault."""

        def converted(interval: tuple[float | None, float | None]) -> BlenderResult[float | tuple[float, ...]]:
            try:
                low, high = interval
                storage = np.finfo(np.float32)
                limits = msgspec.Meta(ge=low if low is not None else float(storage.min), le=high if high is not None else float(storage.max))
                source = np.asarray(value, dtype=np.float64)
                match kind:
                    case native.FloatType():
                        return Ok(msgspec.convert(float(source), typing.Annotated[float, limits]) / self.scale)
                    case native.TupleType() | native.StructType():
                        items = kind.item_types if isinstance(kind, native.TupleType) else tuple(field.type for field in kind.fields)
                        admitted = msgspec.convert(tuple(map(float, source)), typing.Annotated[tuple[typing.Annotated[float, limits], ...], msgspec.Meta(min_length=len(items), max_length=len(items))])
                        return Ok(tuple(item / self.scale for item in admitted))
            except (TypeError, ValueError) as error:
                return Error((BlenderReadFault(request=repr(kind), error=error),))

        return self.bounds(kind).bind(converted)


class RecordGroup[T: msgspec.Struct, S: bpy.types.bpy_struct = bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Constructed native classes binding one record to host storage."""

    record: type[T]
    struct: type[S]

    @property
    def nested(self) -> tuple[type[bpy.types.PropertyGroup], ...]:
        """Nested classes from the retained native property declarations."""

        def classes(struct: type[bpy.types.bpy_struct]) -> tuple[type[bpy.types.PropertyGroup], ...]:
            return tuple(
                child
                for prop in struct.__annotations__.values()
                if prop.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}
                for target in (prop.keywords["type"],)
                if issubclass(target, bpy.types.PropertyGroup)
                for child in (*classes(target), target)
            )

        return classes(self.struct)

    # --- [CONSTRUCTION]

    @staticmethod
    def vector(kind: native.Type) -> bool:
        """Whether a fixed tuple has Blender's shared float-vector socket shape."""
        match kind:
            case native.Metadata(type=inner):
                return RecordGroup.vector(inner)
            case native.TupleType(item_types=(native.FloatType() as component, *tail)):
                return len(tail) in {1, 2, 3} and all(item == component for item in tail)
            case _:
                return False

    @staticmethod
    def components(field: msgspec.structs.FieldInfo) -> tuple[msgspec.structs.FieldInfo, ...]:
        """Derives ordered native fields from each fixed tuple's actual annotations."""
        declared = inspect_annotation(field.type, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager")
        metadata = tuple(marker for marker in declared.metadata if isinstance(marker, Presentation | Definition | Choices))
        return tuple(
            msgspec.structs.FieldInfo(
                name=name,
                encode_name=name,
                type=typing.Annotated[annotation, *metadata] if metadata else annotation,
                default=field.default[index - 1] if field.default is not msgspec.NODEFAULT else msgspec.NODEFAULT,
            )
            for index, annotation in enumerate(typing.get_args(declared.type), 1)
            for name in (f"item_{index}",)
        )

    @staticmethod
    def entries(field: msgspec.structs.FieldInfo, enum: type[Enum]) -> tuple[msgspec.structs.FieldInfo, ...]:
        """Derives finite dictionary members as the existing enabled/value fields."""
        item = GenericAlias(Gated, typing.get_args(inspect_annotation(field.type, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").type)[1])
        return tuple(
            msgspec.structs.FieldInfo(
                name=member.name,
                encode_name=member.name,
                type=item,
                default=Gated(enabled=True, value=field.default[member]) if field.default is not msgspec.NODEFAULT and member in field.default else msgspec.NODEFAULT,
            )
            for member in enum
        )

    @staticmethod
    def bound[R: msgspec.Struct](
        identity: Identity, root: Key, record: type[R], changed: Option[Changed] = Nothing, bindings: Option[Mapping[MemberDescriptorType, Choices]] = Nothing
    ) -> "BlenderResult[RecordGroup[R]]":
        """Builds the host classes with native constraints, defaults, and callbacks."""

        def group(path: Key, definitions: dict[str, bpy.props._PropertyDeferred], defaults: Option[dict[str, object]] = Nothing) -> BlenderResult[type[bpy.types.PropertyGroup]]:
            return identity.name(Name.GROUP, path).map(lambda name: type(name, (bpy.types.PropertyGroup,), {"__annotations__": definitions, "__record_defaults__": defaults.default_value({})}))

        def pointer(built: BlenderResult[type[bpy.types.PropertyGroup]]) -> BlenderResult[bpy.props._PropertyDeferred]:
            return built.map(lambda struct: bpy.props.PointerProperty(type=struct, override={"LIBRARY_OVERRIDABLE"}))

        def defined(field: msgspec.structs.FieldInfo, kind: native.Type, path: Key, annotation: object, title: str | None = None, description: str = "") -> BlenderResult[bpy.props._PropertyDeferred]:
            default = field.default
            if isinstance(kind, native.Metadata):
                metadata = kind.extra_json_schema
                return defined(
                    field,
                    kind.type,
                    path,
                    annotation,
                    metadata.get("title", title) if metadata is not None else title,
                    metadata.get("description", description) if metadata is not None else description,
                )
            binding = next(iter(markers(field.type, Definition)), Definition())
            presentation = next(iter(markers(annotation, Presentation)), Presentation())
            unit = Unit.of(annotation, kind)
            name = title if title is not None else field.name.replace("_", " ").title()

            def shown(value: object) -> str:
                match value:
                    case bool():
                        return "on" if value else "off"
                    case Choice():
                        return value.label
                    case Case():
                        return type(value).title
                    case Enum():
                        return value.name.replace("_", " ").title()
                    case int() | float():
                        return f"{value * unit.scale:g}"
                    case tuple():
                        return ", ".join(map(shown, value))
                    case _:
                        return str(value)

            def represented() -> BlenderResult[bpy.props._PropertyDeferred]:
                match kind:
                    case native.TupleType() if not RecordGroup.vector(kind):
                        return pointer(
                            accumulated(
                                defined(component, native.type_info(component.type), f"{path}_{component.name}", component.type).map(lambda prop: (component.name, prop))
                                for component in RecordGroup.components(field)
                            )
                            .map(dict)
                            .bind(lambda definitions: group(path, definitions))
                        )
                    case native.FloatType() | native.TupleType() | native.StructType():
                        if isinstance(kind, native.StructType) and (typing.get_origin(kind.cls) or kind.cls) is not Rgb:
                            gated = typing.get_origin(kind.cls) is Gated
                            selected = (
                                annotation
                                if gated
                                and any(
                                    isinstance(marker, Presentation | Choices)
                                    for marker in inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata
                                )
                                else msgspec.NODEFAULT
                            )
                            return pointer(recorded(path, kind.cls, default, selected, name if gated else None))

                        def ranged(interval: tuple[float | None, float | None]) -> BlenderResult[bpy.props._PropertyDeferred]:
                            low, high = interval
                            prop = (
                                bpy.props.FloatProperty()
                                if isinstance(kind, native.FloatType)
                                else bpy.props.FloatVectorProperty(size=len(kind.item_types) if isinstance(kind, native.TupleType) else len(kind.fields))
                            )
                            prop.keywords.update({key: value for key, value in (("min", low), ("max", high)) if value is not None})
                            prop.keywords.update(presentation.soft.map(lambda limits: {"soft_min": limits[0] * unit.scale, "soft_max": limits[1] * unit.scale}).default_value({}))
                            prop.keywords.update(presentation.step.map(lambda step: {"step": step * unit.scale * 100}).default_value({}))
                            prop.keywords.update(presentation.decimals.map(lambda decimals: {"precision": decimals}).default_value({}))
                            prop.keywords.update(subtype=unit.number if isinstance(kind, native.FloatType) else unit.vector, unit=unit.unit)

                            def defaulted(value: float | tuple[float, ...]) -> bpy.props._PropertyDeferred:
                                prop.keywords["default"] = value
                                return prop

                            return unit.stored(msgspec.to_builtins(default) if isinstance(kind, native.StructType) else default, kind).map(defaulted) if default is not msgspec.NODEFAULT else Ok(prop)

                        return unit.bounds(kind).bind(ranged)
                    case native.IntType(ge=ge, le=le, gt=gt, lt=lt):
                        prop = bpy.props.IntProperty()
                        low, high = ge if ge is not None else gt + 1 if gt is not None else None, le if le is not None else lt - 1 if lt is not None else None
                        prop.keywords.update({key: value for key, value in (("min", low), ("max", high)) if value is not None})
                        prop.keywords.update(presentation.soft.map(lambda limits: {"soft_min": int(limits[0]), "soft_max": int(limits[1])}).default_value({}))
                        prop.keywords.update(presentation.step.map(lambda step: {"step": int(step)}).default_value({}))
                        prop.keywords["subtype"] = unit.number
                    case native.BoolType():
                        prop = bpy.props.BoolProperty()
                    case native.StrType():
                        sources = markers(annotation, Choices) or markers(field.type, Choices)
                        source = (
                            sources[0]
                            if sources
                            else Choices(
                                lambda *_: (
                                    *((name, name, space.getDescription()) for space in PyOpenColorIO.GetCurrentConfig().getColorSpaces() for name in (space.getName(),)),
                                    (PyOpenColorIO.ROLE_SCENE_LINEAR, "Working Space", "Working color space of the current file"),
                                )
                            )
                            if GraphForm.COLOR_SPACE in markers(field.type, GraphForm)
                            else None
                        )
                        if source is not None:
                            prop = source.defined(identity, field.name, default if default is not msgspec.NODEFAULT else "")
                            prop.keywords["options"] = set(binding.options) - {"ANIMATABLE"}
                            return Ok(prop)
                        prop = bpy.props.StringProperty(subtype=binding.text)
                        prop.keywords.update(binding.search.map(lambda callback: {"search": callback, "search_options": {"SORT", *({"SUGGESTION"} if binding.suggestion else ())}}).default_value({}))
                    case native.EnumType(cls=enum):
                        prop = bpy.props.EnumProperty(items=())
                        prop.keywords["items"] = tuple((member.name, member.label, "", member.value) if isinstance(member, Choice) else (member.name, member.name, "") for member in enum)
                        if default is not msgspec.NODEFAULT:
                            prop.keywords["default"] = next(member.name for member in enum if member == default)
                        return Ok(prop)
                    case native.LiteralType(values=values) if all(type(value) is int for value in values):
                        prop = bpy.props.EnumProperty(items=())
                        prop.keywords["items"] = tuple((identifier, identifier, "", value) for value in values for identifier in (str(value),))
                        if default is not msgspec.NODEFAULT:
                            prop.keywords["default"] = str(default)
                        return Ok(prop)
                    case native.VarTupleType(item_type=native.StructType(cls=item)):

                        def collection(struct: type[bpy.types.PropertyGroup]) -> bpy.props._PropertyDeferred:
                            return bpy.props.CollectionProperty(type=struct, override={"LIBRARY_OVERRIDABLE", "USE_INSERTION"})

                        return recorded(path, item, msgspec.NODEFAULT, inherited=(bpy.types.PropertyGroup.bl_rna.name_property.identifier,)).map(collection)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    case native.DictType(key_type=native.EnumType(cls=enum)):
                        return pointer(
                            accumulated(
                                defined(
                                    entry,
                                    native.type_info(entry.type),
                                    f"{path}_{entry.name.lower()}",
                                    entry.type,
                                    title=enum[entry.name].label if issubclass(enum, Choice) else entry.name.replace("_", " ").title(),
                                ).map(lambda prop: (entry.name, prop))
                                for entry in RecordGroup.entries(field, enum)
                            )
                            .map(dict)
                            .bind(lambda definitions: group(path, definitions))
                        )
                    case native.FrozenSetType(item_type=native.EnumType(cls=enum)):
                        definitions = {}
                        for member in enum:
                            leaf = bpy.props.BoolProperty(name=member.label if isinstance(member, Choice) else member.name.replace("_", " ").title(), override={"LIBRARY_OVERRIDABLE"})
                            if default is not msgspec.NODEFAULT:
                                leaf.keywords["default"] = member in default
                            leaf.keywords.update(changed.map(lambda body: {"update": lambda holder, context, key=member.name: body(holder, context, holder.bl_rna.properties[key])}).default_value({}))
                            definitions[member.name] = leaf
                        return pointer(group(path, definitions))
                    case native.UnionType(types=(native.CustomType(cls=id_type), native.NoneType())) | native.UnionType(types=(native.NoneType(), native.CustomType(cls=id_type))) if issubclass(
                        id_type, bpy.types.ID
                    ):
                        prop = bpy.props.PointerProperty(type=id_type)
                        prop.keywords.update(binding.poll.map(lambda poll: {"poll": functools.partial(poll, identity)}).default_value({}))
                        prop.keywords.update(changed.map(lambda body: {"update": lambda holder, context: body(holder, context, holder.bl_rna.properties[field.name])}).default_value({}))
                        return Ok(prop)
                    case native.UnionType(types=(native.StructType(tag_field=str() as tag_field), *_) as cases):
                        definitions = {}
                        items = []
                        branches = []
                        for case in cases:
                            match case:
                                case native.StructType(cls=case_type, tag=int() as tag) if issubclass(case_type, Case):
                                    items.append((case_type.__name__, case_type.title, case_type.__doc__ or "", tag))
                                    branches.append(
                                        pointer(recorded(f"{path}_{case_type.__name__.lower()}", case_type, default if isinstance(default, case_type) else msgspec.NODEFAULT)).map(
                                            lambda prop: (case_type.__name__, prop)
                                        )
                                    )
                        selector = bpy.props.EnumProperty(items=(), name=name, override={"LIBRARY_OVERRIDABLE"})
                        selector.keywords["items"] = items
                        if default is not msgspec.NODEFAULT:
                            selector.keywords["default"] = type(default).__name__
                        selector.keywords.update(changed.map(lambda body: {"update": lambda holder, context: body(holder, context, holder.bl_rna.properties[tag_field])}).default_value({}))
                        definitions[tag_field] = selector
                        return pointer(accumulated(branches).bind(lambda props: group(path, definitions | dict(props))))
                    case _:
                        raise TypeError(f"No property representation for {kind!r}")
                if default is not msgspec.NODEFAULT and not isinstance(kind, native.FloatType | native.TupleType | native.StructType):
                    prop.keywords["default"] = default
                return Ok(prop)

            def described(prop: bpy.props._PropertyDeferred) -> bpy.props._PropertyDeferred:
                prop.keywords.update(name=name, description=description)
                prop.keywords.setdefault("override", {"LIBRARY_OVERRIDABLE"})
                prop.keywords.setdefault("options", set(binding.options))
                if prop.function not in {bpy.props.CollectionProperty, bpy.props.PointerProperty}:
                    prop.keywords.update(changed.map(lambda body: {"update": lambda holder, context: body(holder, context, holder.bl_rna.properties[field.name])}).default_value({}))
                if not presentation.quantity.map(lambda quantity: quantity in {Quantity.LENGTH, Quantity.ANGLE}).default_value(False):
                    bounds = tuple(f"{key.title()}: {value:g}" for key, value in prop.keywords.items() if key in {"min", "max"})
                    default_text = (f"Default: {shown(default)}",) if default is not msgspec.NODEFAULT and field.default_factory is msgspec.NODEFAULT else ()
                    prop.keywords["description"] = ". ".join(part for part in (description, *bounds, *default_text) if part)
                return prop

            return represented().map(described)

        def recorded(
            path: Key, record_type: type[msgspec.Struct] | GenericAlias, default: object, presentation: object = msgspec.NODEFAULT, title: str | None = None, inherited: tuple[str, ...] = ()
        ) -> BlenderResult[type[bpy.types.PropertyGroup]]:
            fields = tuple(
                bindings
                .bind(lambda rows: Option.of_optional(rows.get(getattr(typing.get_origin(record_type) or record_type, field.name))))
                .map(lambda source: msgspec.structs.replace(field, type=typing.Annotated[field.type, source]))
                .default_value(field)
                for field in msgspec.structs.fields(record_type)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            )
            kinds = native.multi_type_info(field.type for field in fields)
            defaults = {
                field.name: getattr(default, field.name) if default is not msgspec.NODEFAULT else field.default_factory() if field.default_factory is not msgspec.NODEFAULT else field.default
                for field, raw in zip(fields, kinds)
                for kind in (raw.type if isinstance(raw, native.Metadata) else raw,)
                for form in (kind.item_type if isinstance(kind, native.VarTupleType) else kind,)
                if not ({GraphForm.IMAGE, GraphForm.SETTING} & set(markers(field.type, GraphForm)))
                and not (isinstance(form, native.StructType) and (form.cls is Ramp or typing.get_origin(form.cls) is Knot))
                and field.name not in inherited
            }
            definitions = (
                defined(
                    msgspec.structs.replace(field, default=defaults[field.name]),
                    raw,
                    f"{path}_{field.name.lower()}",
                    presentation if field.name == Gated.value.__name__ and presentation is not msgspec.NODEFAULT else field.type,  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
                    title if field.name == Gated.enabled.__name__ else None,  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                ).map(lambda prop: (field.name, prop))
                for field, raw in zip(fields, kinds)
                if field.name in defaults
            )
            return (
                accumulated(definitions)
                .map(dict)
                .bind(
                    lambda declarations: group(
                        path,
                        declarations,
                        Some({
                            field.name: defaults[field.name]
                            for field, raw in zip(fields, kinds)
                            for kind in (raw.type if isinstance(raw, native.Metadata) else raw,)
                            if field.name in declarations
                            and (isinstance(kind, native.VarTupleType) or (declarations[field.name].function is bpy.props.EnumProperty and "get" in declarations[field.name].keywords))
                        }),
                    )
                )
            )

        return recorded(root, record, msgspec.NODEFAULT).map(lambda struct: RecordGroup(record=record, struct=struct))

    @property
    def pointer(self: "RecordGroup[T, bpy.types.PropertyGroup]") -> bpy.props._PropertyDeferred:
        """Root pointer definition with native override support."""
        return bpy.props.PointerProperty(type=self.struct, description=self.record.__doc__ or "", override={"LIBRARY_OVERRIDABLE"})

    # --- [READING]

    def read(self, holder: bpy.types.bpy_struct) -> BlenderResult[T]:
        """Constructs the record from the acquired native field representations."""

        def value(field: msgspec.structs.FieldInfo, kind: native.Type, owner: bpy.types.bpy_struct, default: object, annotation: object) -> BlenderResult[object]:
            if isinstance(kind, native.Metadata):
                return value(field, kind.type, owner, default, annotation)
            held = (
                owner.get(field.name, default if default is not msgspec.NODEFAULT else "")
                if isinstance(kind, native.StrType) and isinstance(owner.bl_rna.properties[field.name], bpy.types.EnumProperty)
                else getattr(owner, field.name)
            )
            match kind:
                case native.TupleType() if not RecordGroup.vector(kind):
                    return accumulated(
                        value(component, native.type_info(component.type), held, component.default, component.type)
                        for component in RecordGroup.components(msgspec.structs.replace(field, default=default))
                    )
                case native.FloatType() | native.TupleType():
                    return Unit.of(annotation, kind).restored(held, kind)
                case native.EnumType(cls=enum):
                    return Ok(enum[held])
                case native.LiteralType():
                    return Ok(int(held))
                case native.StructType(cls=record_type) if (typing.get_origin(record_type) or record_type) is Rgb:
                    return Unit.of(annotation, kind).restored(held, kind).map(lambda values: record_type(**{field.name: item for field, item in zip(msgspec.structs.fields(record_type), values)}))
                case native.StructType(cls=record_type):
                    return record(held, record_type, type(held), annotation if typing.get_origin(record_type) is Gated and markers(annotation, Presentation) else msgspec.NODEFAULT)
                case native.VarTupleType(item_type=native.StructType(cls=item)):
                    return accumulated(record(entry, item, type(entry)) for entry in held) if owner.is_property_set(field.name) else Ok(default if default is not msgspec.NODEFAULT else ())
                case native.DictType(key_type=native.EnumType(cls=enum), value_type=inner):
                    return accumulated(
                        value(leaf, inner, entry, type(entry).__dict__["__record_defaults__"].get(leaf.name, msgspec.NODEFAULT), leaf.type).map(lambda held: (enum[member.name], held))
                        for member in RecordGroup.entries(field, enum)
                        for leaf in msgspec.structs.fields(member.type)
                        if leaf.name == Gated.value.__name__  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
                        for entry in (getattr(held, member.name),)
                        if getattr(entry, Gated.enabled.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    ).map(dict)
                case native.FrozenSetType(item_type=native.EnumType(cls=enum)):
                    return Ok(frozenset(member for member in enum if getattr(held, member.name)))
                case native.UnionType(types=(native.CustomType(), native.NoneType())) | native.UnionType(types=(native.NoneType(), native.CustomType())):
                    return Ok(held)
                case native.UnionType(types=(native.StructType(tag_field=str() as tag_field), *_) as cases):
                    for case in cases:
                        match case:
                            case native.StructType(cls=case_type) if getattr(held, tag_field) == case_type.__name__:
                                selected = getattr(held, case_type.__name__)
                                return record(selected, case_type, type(selected))
                    raise TypeError(f"No selected case for {kind!r}")
                case native.StrType():
                    return catch(lambda: msgspec.convert(held, field.type), exception=msgspec.ValidationError)().map_error(
                        lambda error: (BlenderReadFault(request=f"{owner!r}.{field.name}", error=error),)
                    )
                case native.IntType() | native.BoolType():
                    return Ok(held)
                case _:
                    return Error((BlenderReadFault(request=f"{owner!r}.{field.name}", error=TypeError(f"No property representation for {kind!r}")),))

        def record[R: msgspec.Struct](owner: bpy.types.bpy_struct, record_type: type[R], struct: type[bpy.types.bpy_struct], presentation: object = msgspec.NODEFAULT) -> BlenderResult[R]:
            fields = msgspec.structs.fields(record_type)
            kinds = native.multi_type_info(field.type for field in fields)
            return accumulated(
                value(
                    field,
                    kind,
                    owner,
                    struct.__dict__["__record_defaults__"].get(field.name, msgspec.NODEFAULT),
                    presentation if field.name == Gated.value.__name__ and presentation is not msgspec.NODEFAULT else field.type,  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
                ).map(lambda held: (field.name, held))
                for field, raw in zip(fields, kinds)
                for kind in (raw.type if isinstance(raw, native.Metadata) else raw,)
                if field.name in owner.bl_rna.properties
            ).map(lambda values: record_type(**dict(values)))

        return record(holder, self.record, self.struct)

    # --- [WRITING]

    def write(self, holder: bpy.types.bpy_struct, record: T) -> BlenderResult[None]:
        """Writes independent changed leaves through the native RNA writer."""

        def value(field: msgspec.structs.FieldInfo, kind: native.Type, owner: bpy.types.bpy_struct, desired: object, default: object, annotation: object) -> BlenderResult[None]:
            match kind, desired:
                case native.Metadata(type=inner), _:
                    return value(field, inner, owner, desired, default, annotation)
                case native.TupleType(), tuple() as items if not RecordGroup.vector(kind):
                    return accumulated(
                        value(component, native.type_info(component.type), getattr(owner, field.name), item, component.default, component.type)
                        for component, item in zip(RecordGroup.components(msgspec.structs.replace(field, default=default)), items)
                    ).map(lambda _: None)
                case native.FloatType(), float() | int():
                    return written(owner, field.name, desired * Unit.of(annotation, kind).scale).map(lambda _: None)
                case native.TupleType(), tuple():
                    scale = Unit.of(annotation, kind).scale
                    return written(owner, field.name, tuple(component * scale for component in desired)).map(lambda _: None)
                case native.EnumType(), Enum():
                    return written(owner, field.name, desired.name).map(lambda _: None)
                case native.LiteralType(), _:
                    return written(owner, field.name, str(desired)).map(lambda _: None)
                case native.StructType(cls=record_type), Rgb() if (typing.get_origin(record_type) or record_type) is Rgb:
                    return written(owner, field.name, msgspec.structs.astuple(desired)).map(lambda _: None)
                case native.StructType(cls=record_type), msgspec.Struct():
                    held = getattr(owner, field.name)
                    return stored(held, desired, type(held), record_type, annotation if typing.get_origin(record_type) is Gated and markers(annotation, Presentation) else msgspec.NODEFAULT)
                case native.VarTupleType(item_type=native.StructType(cls=record_type)), tuple():
                    collection = getattr(owner, field.name)
                    if not owner.is_property_set(field.name) and desired == (default if default is not msgspec.NODEFAULT else ()):
                        return Ok(None)
                    if not owner.is_property_set(field.name):
                        collection.add()
                    for index in range(len(collection) - 1, len(desired) - 1, -1):
                        collection.remove(index)
                    for _ in range(len(desired) - len(collection)):
                        collection.add()
                    return accumulated(stored(item, desired_item, type(item), record_type) for item, desired_item in zip(collection, desired)).map(lambda _: None)
                case native.FrozenSetType(item_type=native.EnumType(cls=enum)), frozenset():
                    return accumulated(written(getattr(owner, field.name), member.name, member in desired) for member in enum).map(lambda _: None)
                case native.DictType(key_type=native.EnumType(cls=enum)), dict():
                    return accumulated(
                        stored(entry, Gated(enabled=True, value=desired[key]), type(entry), member.type) if key in desired else written(entry, Gated.enabled.__name__, False).map(lambda _: None)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                        for member in RecordGroup.entries(field, enum)
                        for key in (enum[member.name],)
                        for entry in (getattr(getattr(owner, field.name), member.name),)
                    ).map(lambda _: None)
                case native.UnionType(types=(native.CustomType(), native.NoneType())) | native.UnionType(types=(native.NoneType(), native.CustomType())), bpy.types.ID() | None:
                    return written(owner, field.name, IdName.of(desired) if desired is not None else None).map(lambda _: None)
                case native.UnionType(types=(native.StructType(tag_field=str() as tag_field), *_) as cases), msgspec.Struct():
                    for case in cases:
                        match case:
                            case native.StructType(cls=case_type) if isinstance(desired, case_type):
                                group = getattr(owner, field.name)
                                held = getattr(group, case_type.__name__)
                                return written(group, tag_field, case_type.__name__).bind(lambda _: stored(held, desired, type(held), case_type))
                    raise TypeError(f"No selected case for {kind!r}")
                case native.StrType(), str():
                    equal = isinstance(owner.bl_rna.properties[field.name], bpy.types.EnumProperty) and owner.get(field.name, default if default is not msgspec.NODEFAULT else "") == desired
                    return Ok(None) if equal else written(owner, field.name, desired).map(lambda _: None)
                case native.IntType() | native.BoolType(), int():
                    return written(owner, field.name, desired).map(lambda _: None)
                case _:
                    raise TypeError(f"No property value representation for {kind!r}, {desired!r}")

        def stored(
            owner: bpy.types.bpy_struct, desired: msgspec.Struct, struct: type[bpy.types.bpy_struct], record_type: type[msgspec.Struct] | GenericAlias, presentation: object = msgspec.NODEFAULT
        ) -> BlenderResult[None]:
            fields = msgspec.structs.fields(record_type)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            kinds = native.multi_type_info(field.type for field in fields)
            return accumulated(
                value(
                    field,
                    kind,
                    owner,
                    getattr(desired, field.name),
                    struct.__dict__["__record_defaults__"].get(field.name, msgspec.NODEFAULT),
                    presentation if field.name == Gated.value.__name__ and presentation is not msgspec.NODEFAULT else field.type,  # type: ignore[misc]  # ty: ignore[invalid-attribute-access]
                )
                for field, raw in zip(fields, kinds)
                for kind in (raw.type if isinstance(raw, native.Metadata) else raw,)
                if field.name in owner.bl_rna.properties
            ).map(lambda _: None)

        return stored(holder, record, self.struct, self.record)

    def reset(self, holder: bpy.types.bpy_struct) -> BlenderResult[None]:
        """Restores acquired native defaults and returns pointer-write refusals."""

        def reset(owner: bpy.types.bpy_struct, definitions: dict[str, bpy.props._PropertyDeferred]) -> BlenderResult[None]:
            outcomes: list[BlenderResult[None]] = []
            for key, definition in definitions.items():
                prop = owner.bl_rna.properties[key]
                match prop:
                    case bpy.types.PointerProperty(fixed_type=fixed) if issubclass(type(fixed), bpy.types.PropertyGroup):
                        outcomes.append(reset(getattr(owner, key), type(fixed).__annotations__))
                    case bpy.types.PointerProperty():
                        outcomes.append(written(owner, key, None).map(lambda _: None))
                    case _:
                        if isinstance(prop, bpy.types.EnumProperty) and "get" in definition.keywords:
                            owner.id_properties_ensure().pop(key, None)
                        else:
                            owner.property_unset(key)
                        owner.path_resolve(key, False).update()
            return accumulated(outcomes).map(lambda _: None)

        return reset(holder, {key: definition for key, definition in self.struct.__annotations__.items() if key in self.record.__struct_fields__})

    # --- [CUSTOM_PROPERTIES]

    def stamp(self, owner: bpy.types.ID, prefix: str, record: T) -> BlenderResult[None]:
        """Writes custom values with UI metadata from the registered RNA definitions."""

        def stamped(field: msgspec.structs.FieldInfo) -> BlenderResult[None]:
            key = f"{prefix}{field.encode_name}"
            prop = self.struct.bl_rna.properties[field.name]
            value = getattr(record, field.name)
            kind = native.type_info(field.type)
            kind = kind.type if isinstance(kind, native.Metadata) else kind
            unit = Unit.of(field.type, kind)

            def assigned(data: Mapping[str, object], value: object) -> BlenderResult[None]:
                try:
                    owner[key] = value
                    owner.id_properties_ui(key).update(**data, subtype=prop.subtype, description=prop.description)  # type: ignore[func-returns-value, attr-defined]  # ty: ignore[unresolved-attribute]
                    return Ok(None)
                except (AttributeError, KeyError, TypeError, ValueError, OverflowError, ReferenceError) as error:
                    return Error((BlenderEditFault(request=f"{owner!r}[{key!r}]", error=error),))

            match prop, kind:
                case bpy.types.FloatProperty(), native.FloatType() | native.TupleType() | native.StructType():
                    data: dict[str, object] = {
                        "min": prop.hard_min,
                        "max": prop.hard_max,
                        "soft_min": prop.soft_min,
                        "soft_max": prop.soft_max,
                        "step": prop.step,
                        "precision": prop.precision,
                        "default": tuple(prop.default_array) if prop.is_array else prop.default,  # type: ignore[arg-type]
                    }
                    return unit.stored(msgspec.structs.astuple(value) if isinstance(kind, native.StructType) else value, kind).bind(lambda lowered: assigned(data, lowered))
                case bpy.types.IntProperty(), _:
                    data = {"min": prop.hard_min, "max": prop.hard_max, "soft_min": prop.soft_min, "soft_max": prop.soft_max, "step": prop.step, "default": prop.default}
                case bpy.types.EnumProperty(), _:
                    value = value if isinstance(kind, native.LiteralType) else value.value
                    data = {"items": tuple((item.identifier, item.name, item.description, item.icon, item.value) for item in prop.enum_items), "default": prop.enum_items[prop.default].value}
                case bpy.types.BoolProperty() | bpy.types.StringProperty(), _:
                    data = {"default": prop.default}
                case _:
                    return Error((BlenderReadFault(request=f"{owner!r}[{key!r}]", error=TypeError(f"No custom property representation for {kind!r}")),))
            return assigned(data, value)

        outcome = accumulated(map(stamped, msgspec.structs.fields(record))).map(lambda _: None)
        owner.update_tag()
        return outcome

    def stamped(self, owner: bpy.types.ID, prefix: str) -> BlenderResult[T]:
        """Constructs the record from present native custom values."""
        fields = msgspec.structs.fields(self.record)
        kinds = native.multi_type_info(field.type for field in fields)

        def value(field: msgspec.structs.FieldInfo, kind: native.Type, key: str) -> BlenderResult[object]:
            match kind:
                case native.FloatType() | native.TupleType():
                    return Unit.of(field.type, kind).restored(owner[key], kind)
                case native.StructType(cls=record):
                    return Unit.of(field.type, kind).restored(owner[key], kind).map(lambda values: record(**{channel.name: item for channel, item in zip(msgspec.structs.fields(record), values)}))
                case _:
                    return catch(lambda: msgspec.convert(owner[key], field.type), exception=msgspec.ValidationError)().map_error(lambda error: (BlenderReadFault(request=repr(owner), error=error),))

        return accumulated(
            value(field, kind, key).map(lambda held: (field.name, held))
            for field, raw in zip(fields, kinds)
            for kind in (raw.type if isinstance(raw, native.Metadata) else raw,)
            for key in (f"{prefix}{field.encode_name}",)
            if key in owner  # type: ignore[operator]
        ).bind(lambda values: catch(lambda: self.record(**dict(values)), exception=TypeError)().map_error(lambda error: (BlenderReadFault(request=repr(owner), error=error),)))


# --- [EXPORTS] ---------------------------------------------------------------------------


__all__ = ["Changed", "Choices", "Definition", "EnumRows", "PointerPoll", "RecordGroup", "SearchSource", "Unit"]
