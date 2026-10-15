"""Native record fields bound to Blender properties and custom-property metadata."""

from collections.abc import Callable, Iterable, Mapping
from enum import Enum
import math
from types import GenericAlias, MemberDescriptorType
import typing
from typing import ClassVar, overload, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect as native
import numpy as np
import PyOpenColorIO
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from blender.bpy.data.rna.descriptors import IdName, written
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.faults import BlenderReadFault, BlenderResult
from blender.bpy.settings.shapes import (
    bare,
    Color,
    Components,
    Declared,
    Entries,
    Enumerated,
    Fields,
    Flag,
    Flags,
    Integer,
    Literals,
    Nested,
    Number,
    OptionalText,
    Pointer,
    property_fields,
    PropertyShape,
    Record,
    Records,
    Tagged,
    Text,
    Vector,
)
from blender.imaging.faults import accumulated
from blender.imaging.parameters import Case, Choice, Gated, GraphForm, markers, Presentation, Quantity, Rgb

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Changed = Callable[[bpy.types.bpy_struct[object], bpy.types.Context, bpy.types.Property], None]
type SearchSource = Callable[[bpy.types.bpy_struct[object], bpy.types.Context, str], Iterable[str | tuple[str, str]]]
type PointerPoll[T: bpy.types.ID = bpy.types.ID] = Callable[[Identity, bpy.types.bpy_struct[object], T], bool]
type EnumRows = Iterable[tuple[str, str, str] | tuple[str, str, str, str | int, int] | None]
type EnumItems = tuple[tuple[str, str, str, str | int, int] | None, ...]

# --- [MODELS] ---------------------------------------------------------------------------


class Choices(msgspec.Struct, frozen=True, eq=False):
    """Dynamic native rows with persistent identifiers, each distinct row set retained since Blender reads its strings after the callback returns."""

    source: "Callable[[Identity, bpy.types.bpy_struct[object], bpy.types.Context | None], EnumRows]"
    numbers: dict[str, int] = msgspec.field(default_factory=dict)
    rows: dict[EnumItems, EnumItems] = msgspec.field(default_factory=dict)

    def defined(self, identity: Identity, key: str, default: object) -> bpy.props._PropertyDeferred:
        """Builds native callbacks retaining identifiers across item changes."""

        def number(identifier: str) -> int:
            return self.numbers.setdefault(identifier, len(self.numbers))

        def listed(owner: "bpy.types.bpy_struct[object]", context: bpy.types.Context | None) -> EnumItems:
            rows = tuple((*row[:3], row[3] if len(row) == 5 else "NONE", number(row[0]) if row[0] else -1) if row is not None else None for row in self.source(identity, owner, context))
            return self.rows.setdefault(rows, rows)

        def selected(owner: "bpy.types.bpy_struct[object]") -> int:
            return number(owner.get(key, default))

        def stored(owner: "bpy.types.bpy_struct[object]", value: int) -> None:
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
            case native.FloatType(ge=low, le=high):
                return cls.quantified(annotation).default_with(lambda: cls(number="FACTOR" if (low, high) == (0, 1) else "NONE"))
            case native.TupleType(item_types=items):
                factor = all(isinstance(item, native.FloatType) and (item.ge, item.le) == (0, 1) for item in map(bare, items))
                return cls.quantified(annotation).default_with(lambda: cls(number="FACTOR" if factor else "NONE"))
            case _:
                return cls.quantified(annotation).default_with(cls)

    @classmethod
    def quantified(cls, annotation: object) -> Option[Self]:
        """Returns the host presentation of the annotation's declared quantity, absent without one."""
        match next(iter(markers(annotation, Presentation)), Presentation()).quantity:
            case Option(tag="some", some=Quantity.LENGTH):
                return Some(cls(number="DISTANCE", vector="XYZ_LENGTH"))
            case Option(tag="some", some=Quantity.CAMERA_LENGTH):
                return Some(cls(number="DISTANCE_CAMERA", unit="CAMERA", scale=1000.0))
            case Option(tag="some", some=Quantity.ANGLE):
                return Some(cls(number="ANGLE", vector="EULER"))
            case Option(tag="some", some=Quantity.DURATION):
                return Some(cls(number="TIME_ABSOLUTE", unit="TIME_ABSOLUTE"))
            case Option(tag="some", some=Quantity.COLOR_TEMPERATURE):
                return Some(cls(number="COLOR_TEMPERATURE", unit="COLOR_TEMPERATURE"))
            case Option(tag="some", some=Quantity.IRRADIANCE | Quantity.LUMINANCE):
                return Some(cls())
            case Option(tag="some"):
                return Some(cls(number="FACTOR", vector="XYZ"))
            case Option():
                return Nothing

    # --- [STORAGE]

    largest: ClassVar[float] = float(np.nextafter(np.float32(np.inf), np.float32(0)))

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
                answer: list[float | None] = []
                for endpoint, inward in endpoints:
                    if endpoint is None:
                        answer.append(None)
                        continue
                    scaled = endpoint * self.scale
                    limited = max(scaled, -self.largest) if inward > 0 else min(scaled, self.largest)
                    if not -self.largest <= limited <= self.largest:
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
                limits = msgspec.Meta(ge=low if low is not None else -self.largest, le=high if high is not None else self.largest)
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
                limits = msgspec.Meta(ge=low if low is not None else -self.largest, le=high if high is not None else self.largest)
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


class RecordGroup[T: msgspec.Struct, S: "bpy.types.bpy_struct[object]" = bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Constructed native classes binding one record to host storage."""

    record: type[T]
    struct: type[S]

    @property
    def nested(self) -> tuple[type[bpy.types.PropertyGroup], ...]:
        """Nested classes from the retained native property declarations."""

        def classes(struct: "type[bpy.types.bpy_struct[object]]") -> tuple[type[bpy.types.PropertyGroup], ...]:
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
        enabled_field, value_field = Gated.__struct_fields__

        def group(path: Key, definitions: dict[str, bpy.props._PropertyDeferred], defaults: Option[dict[str, object]] = Nothing) -> BlenderResult[type[bpy.types.PropertyGroup]]:
            return identity.name(Name.GROUP, path).map(lambda name: type(name, (bpy.types.PropertyGroup,), {"__annotations__": definitions, "__record_defaults__": defaults.default_value({})}))

        def pointer(built: BlenderResult[type[bpy.types.PropertyGroup]]) -> BlenderResult[bpy.props._PropertyDeferred]:
            return built.map(lambda struct: bpy.props.PointerProperty(type=struct, override={"LIBRARY_OVERRIDABLE"}))

        def updating(member: str) -> Mapping[str, object]:
            return changed.map(lambda body: {"update": lambda holder, context: body(holder, context, holder.bl_rna.properties[member])}).default_value({})

        def defined(
            field: msgspec.structs.FieldInfo, declared: Declared[PropertyShape], path: Key, annotation: object, title: str | None = None, summary: str = ""
        ) -> BlenderResult[bpy.props._PropertyDeferred]:
            shape = declared.shape
            default = field.default
            blank = "" if isinstance(shape, OptionalText) and default is None else default
            description = declared.description.default_value(summary)
            binding = next(iter(markers(field.type, Definition)), Definition())
            presentation = next(iter(markers(annotation, Presentation)), Presentation())
            name = declared.title.or_else(Option.of_optional(title)).default_value(field.name.replace("_", " ").title())
            unit = Unit.of(annotation, shape.kind) if isinstance(shape, Number | Vector | Color | Integer) else Unit.quantified(annotation).default_with(Unit)

            def defaulted(prop: bpy.props._PropertyDeferred, value: object) -> BlenderResult[bpy.props._PropertyDeferred]:
                prop.keywords.update({} if value is msgspec.NODEFAULT else {"default": value})
                return Ok(prop)

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
                match shape:
                    case Components(items=items):
                        return pointer(
                            accumulated(
                                defined(component, item, f"{path}_{component.name}", component.type).map(lambda prop: (component.name, prop))
                                for component, item in zip(RecordGroup.components(field), items)
                            )
                            .map(dict)
                            .bind(lambda definitions: group(path, definitions))
                        )
                    case Number(kind=kind) | Vector(kind=kind) | Color(kind=kind):

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

                            def stored(value: float | tuple[float, ...]) -> bpy.props._PropertyDeferred:
                                prop.keywords["default"] = value
                                return prop

                            return unit.stored(msgspec.to_builtins(default) if isinstance(shape, Color) else default, kind).map(stored) if default is not msgspec.NODEFAULT else Ok(prop)

                        return unit.bounds(kind).bind(ranged)
                    case Nested(record=record):
                        gated = typing.get_origin(record) is Gated
                        selected = (
                            annotation
                            if gated
                            and any(
                                isinstance(marker, Presentation | Choices) for marker in inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata
                            )
                            else msgspec.NODEFAULT
                        )
                        return pointer(recorded(path, record, default, selected, name if gated else None))
                    case Integer(kind=native.IntType(ge=ge, le=le, gt=gt, lt=lt)):
                        prop = bpy.props.IntProperty()
                        low, high = ge if ge is not None else gt + 1 if gt is not None else None, le if le is not None else lt - 1 if lt is not None else None
                        storage = np.iinfo(np.int32)
                        prop.keywords.update({key: min(max(value, int(storage.min)), int(storage.max)) for key, value in (("min", low), ("max", high)) if value is not None})
                        prop.keywords.update(presentation.soft.map(lambda limits: {"soft_min": int(limits[0]), "soft_max": int(limits[1])}).default_value({}))
                        prop.keywords.update(presentation.step.map(lambda step: {"step": int(step)}).default_value({}))
                        prop.keywords["subtype"] = unit.number
                        return defaulted(prop, default)
                    case Flag():
                        return defaulted(bpy.props.BoolProperty(), default)
                    case Text() | OptionalText():
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
                            prop = source.defined(identity, field.name, blank if blank is not msgspec.NODEFAULT else "")
                            prop.keywords["options"] = set(binding.options) - {"ANIMATABLE"}
                            return Ok(prop)
                        prop = bpy.props.StringProperty(subtype=binding.text)
                        prop.keywords.update(binding.search.map(lambda callback: {"search": callback, "search_options": {"SORT", *({"SUGGESTION"} if binding.suggestion else ())}}).default_value({}))
                        return defaulted(prop, blank)
                    case Enumerated(enum=enum):
                        prop = bpy.props.EnumProperty(items=())
                        prop.keywords["items"] = tuple((member.name, member.label, "", member.value) if isinstance(member, Choice) else (member.name, member.name, "") for member in enum)
                        if default is not msgspec.NODEFAULT:
                            prop.keywords["default"] = next(member.name for member in enum if member == default)
                        return Ok(prop)
                    case Literals(values=values):
                        prop = bpy.props.EnumProperty(items=())
                        prop.keywords["items"] = tuple((identifier, identifier, "", value) for value in values for identifier in (str(value),))
                        if default is not msgspec.NODEFAULT:
                            prop.keywords["default"] = str(default)
                        return Ok(prop)
                    case Records(record=record):

                        def collection(struct: type[bpy.types.PropertyGroup]) -> bpy.props._PropertyDeferred:
                            return bpy.props.CollectionProperty(type=struct, override={"LIBRARY_OVERRIDABLE", "USE_INSERTION"})

                        return recorded(path, record, msgspec.NODEFAULT, inherited=(bpy.types.PropertyGroup.bl_rna.name_property.identifier,)).map(collection)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    case Entries(enum=enum):
                        return pointer(
                            accumulated(
                                defined(
                                    entry,
                                    Declared(shape=Nested(record=entry.type)),
                                    f"{path}_{entry.name.lower()}",
                                    entry.type,
                                    title=enum[entry.name].label if issubclass(enum, Choice) else entry.name.replace("_", " ").title(),
                                ).map(lambda prop: (entry.name, prop))
                                for entry in RecordGroup.entries(field, enum)
                            )
                            .map(dict)
                            .bind(lambda definitions: group(path, definitions))
                        )
                    case Flags(enum=enum):

                        def flag(member: Enum) -> bpy.props._PropertyDeferred:
                            leaf = bpy.props.BoolProperty(name=member.label if isinstance(member, Choice) else member.name.replace("_", " ").title(), override={"LIBRARY_OVERRIDABLE"})
                            leaf.keywords.update({} if default is msgspec.NODEFAULT else {"default": member in default})
                            leaf.keywords.update(updating(member.name))
                            return leaf

                        return pointer(group(path, {member.name: flag(member) for member in enum}))
                    case Pointer(block=block):
                        prop = bpy.props.PointerProperty(type=block)
                        prop.keywords.update(binding.poll.map(lambda poll: {"poll": lambda holder, block: poll(identity, holder, block)}).default_value({}))
                        prop.keywords.update(updating(field.name))
                        return Ok(prop)
                    case Tagged(tag_field=tag_field, cases=cases):
                        branches = accumulated(
                            pointer(recorded(f"{path}_{case.__name__.lower()}", case, default if isinstance(default, case) else msgspec.NODEFAULT)).map(lambda prop: (case.__name__, prop))
                            for case, _ in cases
                        )
                        selector = bpy.props.EnumProperty(items=(), name=name, override={"LIBRARY_OVERRIDABLE"})
                        selector.keywords["items"] = [(case.__name__, case.title, case.__doc__ or "", tag) for case, tag in cases]
                        if default is not msgspec.NODEFAULT:
                            selector.keywords["default"] = type(default).__name__
                        selector.keywords.update(updating(tag_field))
                        return pointer(branches.bind(lambda props: group(path, {tag_field: selector} | dict(props))))

            def described(prop: bpy.props._PropertyDeferred) -> bpy.props._PropertyDeferred:
                prop.keywords.update(name=name, description=description)
                prop.keywords.setdefault("override", {"LIBRARY_OVERRIDABLE"})
                prop.keywords.setdefault("options", set(binding.options))
                if prop.function not in {bpy.props.CollectionProperty, bpy.props.PointerProperty}:
                    prop.keywords.update(updating(field.name))
                if not presentation.quantity.map(lambda quantity: quantity in {Quantity.LENGTH, Quantity.ANGLE}).default_value(False):
                    bounds = tuple(f"{key.title()}: {value:g}" for key, value in prop.keywords.items() if key in {"min", "max"})
                    default_text = (f"Default: {shown(blank)}",) if blank is not msgspec.NODEFAULT and field.default_factory is msgspec.NODEFAULT else ()
                    prop.keywords["description"] = ". ".join(part for part in (description, *bounds, *default_text) if part)
                return prop

            return represented().map(described)

        def recorded(
            path: Key, record_type: Record, default: object, presentation: object = msgspec.NODEFAULT, title: str | None = None, inherited: tuple[str, ...] = ()
        ) -> BlenderResult[type[bpy.types.PropertyGroup]]:
            def declared(rows: Fields[PropertyShape]) -> BlenderResult[type[bpy.types.PropertyGroup]]:
                fields = tuple(
                    (
                        bindings
                        .bind(lambda sources: Option.of_optional(sources.get(getattr(typing.get_origin(record_type) or record_type, field.name))))
                        .map(lambda source: msgspec.structs.replace(field, type=typing.Annotated[field.type, source]))
                        .default_value(field),
                        shape,
                    )
                    for field, shape in rows
                    if field.name not in inherited
                )
                defaults = {
                    field.name: getattr(default, field.name) if default is not msgspec.NODEFAULT else field.default_factory() if field.default_factory is not msgspec.NODEFAULT else field.default
                    for field, _ in fields
                }
                return (
                    accumulated(
                        defined(
                            msgspec.structs.replace(field, default=defaults[field.name]),
                            shape,
                            f"{path}_{field.name.lower()}",
                            presentation if field.name == value_field and presentation is not msgspec.NODEFAULT else field.type,
                            title if field.name == enabled_field else None,
                        ).map(lambda prop: (field.name, prop))
                        for field, shape in fields
                    )
                    .map(dict)
                    .bind(
                        lambda declarations: group(
                            path,
                            declarations,
                            Some({
                                field.name: defaults[field.name]
                                for field, shape in fields
                                if isinstance(shape.shape, Records) or (declarations[field.name].function is bpy.props.EnumProperty and "get" in declarations[field.name].keywords)
                            }),
                        )
                    )
                )

            return property_fields(record_type).bind(declared)

        return recorded(root, record, msgspec.NODEFAULT).map(lambda struct: RecordGroup(record=record, struct=struct))

    @property
    def pointer(self: "RecordGroup[T, bpy.types.PropertyGroup]") -> bpy.props._PropertyDeferred:
        """Root pointer definition with native override support."""
        prop = bpy.props.PointerProperty(type=self.struct, override={"LIBRARY_OVERRIDABLE"})
        prop.keywords.update(Option.of_optional(self.record.__doc__).map(lambda doc: {"description": doc}).default_value({}))
        return prop

    # --- [READING]

    def read(self, holder: "bpy.types.bpy_struct[object]") -> BlenderResult[T]:
        """Constructs the record from the acquired native field representations."""
        enabled_field, value_field = Gated.__struct_fields__

        def value(field: msgspec.structs.FieldInfo, shape: PropertyShape, owner: "bpy.types.bpy_struct[object]", default: object, annotation: object) -> BlenderResult[object]:
            held = (
                owner.get(field.name, default if default is not msgspec.NODEFAULT else "")
                if isinstance(shape, Text) and isinstance(owner.bl_rna.properties[field.name], bpy.types.EnumProperty)
                else getattr(owner, field.name)
            )
            match shape:
                case Components(items=items):
                    return accumulated(
                        value(component, item.shape, held, component.default, component.type) for component, item in zip(RecordGroup.components(msgspec.structs.replace(field, default=default)), items)
                    )
                case Number(kind=kind) | Vector(kind=kind):
                    return Unit.of(annotation, kind).restored(held, kind)
                case Color(kind=kind):
                    return Unit.of(annotation, kind).restored(held, kind).map(lambda values: kind.cls(**{channel.name: item for channel, item in zip(msgspec.structs.fields(kind.cls), values)}))
                case Enumerated(enum=enum):
                    return Ok(enum[held])
                case Literals():
                    return Ok(int(held))
                case Nested(record=record_type):
                    return record(held, record_type, type(held), annotation if typing.get_origin(record_type) is Gated and markers(annotation, Presentation) else msgspec.NODEFAULT)
                case Records(record=item):
                    return accumulated(record(entry, item, type(entry)) for entry in held) if owner.is_property_set(field.name) else Ok(default if default is not msgspec.NODEFAULT else ())
                case Entries(enum=enum, value=inner):
                    return accumulated(
                        value(leaf, inner.shape, entry, type(entry).__dict__["__record_defaults__"].get(leaf.name, msgspec.NODEFAULT), leaf.type).map(lambda held: (enum[member.name], held))
                        for member in RecordGroup.entries(field, enum)
                        for leaf in msgspec.structs.fields(member.type)
                        if leaf.name == value_field
                        for entry in (getattr(held, member.name),)
                        if getattr(entry, enabled_field)
                    ).map(dict)
                case Flags(enum=enum):
                    return Ok(frozenset(member for member in enum if getattr(held, member.name)))
                case Pointer():
                    return Ok(held)
                case OptionalText():
                    return Ok(held or None)
                case Tagged(tag_field=tag_field, cases=cases):
                    chosen = {case.__name__: case for case, _ in cases}[getattr(held, tag_field)]
                    selected = getattr(held, chosen.__name__)
                    return record(selected, chosen, type(selected))
                case Text():
                    return catch(lambda: msgspec.convert(held, field.type), exception=msgspec.ValidationError)().map_error(
                        lambda error: (BlenderReadFault(request=f"{owner!r}.{field.name}", error=error),)
                    )
                case Integer() | Flag():
                    return Ok(held)

        def record[R: msgspec.Struct](
            owner: "bpy.types.bpy_struct[object]", record_type: type[R], struct: "type[bpy.types.bpy_struct[object]]", presentation: object = msgspec.NODEFAULT
        ) -> BlenderResult[R]:
            return property_fields(record_type).bind(
                lambda rows: accumulated(
                    value(
                        field,
                        shape.shape,
                        owner,
                        struct.__dict__["__record_defaults__"].get(field.name, msgspec.NODEFAULT),
                        presentation if field.name == value_field and presentation is not msgspec.NODEFAULT else field.type,
                    ).map(lambda held: (field.name, held))
                    for field, shape in rows
                    if field.name in owner.bl_rna.properties
                ).map(lambda values: record_type(**dict(values)))
            )

        return record(holder, self.record, self.struct)

    # --- [WRITING]

    def write(self, holder: "bpy.types.bpy_struct[object]", record: T) -> BlenderResult[None]:
        """Writes independent changed leaves through the native RNA writer."""
        enabled_field, value_field = Gated.__struct_fields__

        def value(field: msgspec.structs.FieldInfo, shape: PropertyShape, owner: "bpy.types.bpy_struct[object]", desired: object, default: object, annotation: object) -> BlenderResult[None]:
            match shape:
                case Components(items=items):
                    return accumulated(
                        value(component, item.shape, getattr(owner, field.name), part, component.default, component.type)
                        for component, item, part in zip(RecordGroup.components(msgspec.structs.replace(field, default=default)), items, msgspec.convert(desired, tuple))
                    ).map(lambda _: None)
                case Number():
                    return written(owner, field.name, msgspec.convert(desired, float) * Unit.of(annotation, shape.kind).scale).map(lambda _: None)
                case Vector(kind=kind):
                    scale = Unit.of(annotation, kind).scale
                    return written(owner, field.name, tuple(component * scale for component in msgspec.convert(desired, tuple[float, ...]))).map(lambda _: None)
                case Enumerated(enum=enum):
                    return written(owner, field.name, enum(desired).name).map(lambda _: None)
                case Literals():
                    return written(owner, field.name, str(desired)).map(lambda _: None)
                case Color(kind=kind):
                    return written(owner, field.name, tuple(getattr(desired, channel.name) for channel in msgspec.structs.fields(kind.cls))).map(lambda _: None)
                case Nested(record=record_type):
                    held = getattr(owner, field.name)
                    return stored(held, desired, type(held), record_type, annotation if typing.get_origin(record_type) is Gated and markers(annotation, Presentation) else msgspec.NODEFAULT)
                case Records() if not owner.is_property_set(field.name) and msgspec.convert(desired, tuple) == (() if default is msgspec.NODEFAULT else default):
                    return Ok(None)
                case Records(record=record_type):
                    collection = getattr(owner, field.name)
                    items = msgspec.convert(desired, tuple)
                    if not owner.is_property_set(field.name):
                        collection.add()
                    for index in range(len(collection) - 1, len(items) - 1, -1):
                        collection.remove(index)
                    for _ in range(len(items) - len(collection)):
                        collection.add()
                    return accumulated(stored(item, desired_item, type(item), record_type) for item, desired_item in zip(collection, items)).map(lambda _: None)
                case Flags(enum=enum):
                    members = msgspec.convert(desired, frozenset)
                    return accumulated(written(getattr(owner, field.name), member.name, member in members) for member in enum).map(lambda _: None)
                case Entries(enum=enum):
                    entries = msgspec.convert(desired, dict)
                    return accumulated(
                        stored(entry, Gated(enabled=True, value=entries[key]), type(entry), member.type) if key in entries else written(entry, enabled_field, False).map(lambda _: None)
                        for member in RecordGroup.entries(field, enum)
                        for key in (enum[member.name],)
                        for entry in (getattr(getattr(owner, field.name), member.name),)
                    ).map(lambda _: None)
                case Pointer():
                    return written(owner, field.name, IdName.of(desired) if isinstance(desired, bpy.types.ID) else None).map(lambda _: None)
                case Tagged(tag_field=tag_field, cases=cases):
                    chosen = next(case for case, _ in cases if isinstance(desired, case))
                    group = getattr(owner, field.name)
                    held = getattr(group, chosen.__name__)
                    return written(group, tag_field, chosen.__name__).bind(lambda _: stored(held, desired, type(held), chosen))
                case Text():
                    text = msgspec.convert(desired, str)
                    equal = isinstance(owner.bl_rna.properties[field.name], bpy.types.EnumProperty) and owner.get(field.name, default if default is not msgspec.NODEFAULT else "") == text
                    return Ok(None) if equal else written(owner, field.name, text).map(lambda _: None)
                case OptionalText():
                    return written(owner, field.name, Option.of_optional(msgspec.convert(desired, str | None)).default_value("")).map(lambda _: None)
                case Integer():
                    return written(owner, field.name, msgspec.convert(desired, int)).map(lambda _: None)
                case Flag():
                    return written(owner, field.name, msgspec.convert(desired, bool)).map(lambda _: None)

        def stored(
            owner: "bpy.types.bpy_struct[object]", desired: object, struct: "type[bpy.types.bpy_struct[object]]", record_type: Record, presentation: object = msgspec.NODEFAULT
        ) -> BlenderResult[None]:
            return property_fields(record_type).bind(
                lambda rows: accumulated(
                    value(
                        field,
                        shape.shape,
                        owner,
                        getattr(desired, field.name),
                        struct.__dict__["__record_defaults__"].get(field.name, msgspec.NODEFAULT),
                        presentation if field.name == value_field and presentation is not msgspec.NODEFAULT else field.type,
                    )
                    for field, shape in rows
                    if field.name in owner.bl_rna.properties
                ).map(lambda _: None)
            )

        return stored(holder, record, self.struct, self.record)

    def reset(self, holder: "bpy.types.bpy_struct[object]") -> BlenderResult[None]:
        """Restores acquired native defaults, writing record defaults into collections RNA cannot default, and returns read or write refusals."""

        def reset(owner: "bpy.types.bpy_struct[object]", definitions: dict[str, bpy.props._PropertyDeferred]) -> BlenderResult[None]:
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

        return (
            reset(holder, {key: definition for key, definition in self.struct.__annotations__.items() if key in self.record.__struct_fields__})
            .bind(lambda _: self.read(holder))
            .bind(lambda record: self.write(holder, record))
        )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Changed", "Choices", "Definition", "EnumItems", "EnumRows", "PointerPoll", "RecordGroup", "SearchSource", "Unit"]
