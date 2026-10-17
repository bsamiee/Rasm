"""Native record fields bound to Blender properties and custom-property metadata."""

from collections.abc import Callable, Iterable, Mapping
from enum import Enum
import math
from types import MemberDescriptorType
import typing
from typing import ClassVar, overload, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect as native
import numpy as np
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import IdName, written
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.faults import BlenderReadFault, BlenderResult
from blender.bpy.settings.shapes import (
    bare,
    Color,
    Components,
    Declared,
    ENABLED,
    Entries,
    Enumerated,
    field_shape,
    field_shapes,
    Fields,
    Flag,
    Flags,
    Gate,
    Integer,
    items,
    Literals,
    Nested,
    Number,
    OptionalText,
    Pointer,
    Record,
    Records,
    Shape,
    Tagged,
    Text,
    Vector,
)
from blender.imaging.faults import accumulated
from blender.imaging.parameters import Case, Choice, Gated, heading, markers, Presentation, Quantity, Rgb

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

    digits: ClassVar[tuple[int, int]] = (0, 6)
    precision: ClassVar[int] = 2

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
        match Presentation.of(annotation).quantity:
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

    @classmethod
    def extent(cls, low: float | None, high: float | None) -> Option[float]:
        """Returns the positive width of a span whose ends both lie inside float32 storage, absent for an open, empty, or storage-wide span."""
        return Some(high - low) if low is not None and high is not None and -cls.largest < low < high < cls.largest else Nothing

    def spanned(self, presentation: Presentation, bounds: tuple[float, float]) -> tuple[float, float, Self]:
        """Returns the drawn span in host units, the declared soft span else the hard bounds, with this presentation over it, a factor slider drawn as a plain number over a span without an extent."""
        low, high = presentation.soft.map(lambda limits: (limits[0] * self.scale, limits[1] * self.scale)).default_value(bounds)
        return low, high, self if self.number != "FACTOR" or self.extent(low, high).is_some() else msgspec.structs.replace(self, number="NONE")

    def counted(self, presentation: Presentation, kind: native.IntType) -> tuple[int, int, Self]:
        """Returns an integer kind's drawn span in whole values, the declared soft span else the admissible values, with this presentation over it."""
        low, high, shown = self.spanned(presentation, self.integral(kind))
        return round(low), round(high), shown

    def drawn(self, presentation: Presentation, span: tuple[float, float], default: Option[float | tuple[float, ...]]) -> dict[str, float]:
        """Native step and precision in host units over a drawn span.

        Args:
            presentation: Display settings of the field.
            span: Drawn span in host units.
            default: Stored host default, absent for a field with none.

        Returns:
            The declared step, else a hundredth of a finite span, capped at the leading decimal of the default's smallest nonzero component, per click and drag pixel in
            Blender's hundredth units; the declared decimals, else those reaching the step and a finite span's low end where they exceed Blender's default precision,
            widened to the default's shortest decimal digits within Blender's precision range.
        """
        extent = self.extent(*span)
        values = default.map(lambda value: value if isinstance(value, tuple) else (value,)).default_value(())
        leading = tuple(10.0 ** math.floor(math.log10(abs(value))) for value in values if value != 0.0)
        increment = Option.of_optional(min((*presentation.step.map(lambda step: step * self.scale).or_else(extent.map(lambda width: width / 100.0)).to_list(), *leading), default=None))

        def reached(size: float) -> int:
            return max(-math.floor(math.log10(abs(value))) for value in (size, *extent.map(lambda _: span[0]).to_list()) if value != 0.0)

        def written(value: float) -> int:
            return len(np.format_float_positional(np.float32(value), unique=True, trim="-").partition(".")[2])

        shortest = max(map(written, values), default=0)
        places = presentation.decimals.map(lambda count: max(count, shortest)).or_else(Some(max(increment.map(reached).default_value(0), shortest)).filter(lambda count: count > self.precision))
        return {**increment.map(lambda size: {"step": size * 100.0}).default_value({}), **places.map(lambda count: {"precision": min(max(count, self.digits[0]), self.digits[1])}).default_value({})}

    # --- [STORAGE]

    largest: ClassVar[float] = float(np.nextafter(np.float32(np.inf), np.float32(0)))

    @staticmethod
    def endpoints(kind: native.Type) -> tuple[Option[tuple[float, bool]], Option[tuple[float, bool]]]:
        """Returns a numeric kind's declared low and high endpoints with whether each admits itself, the tightest across a tuple's or record's items."""
        match kind:
            case native.Metadata(type=inner):
                return Unit.endpoints(inner)
            case native.TupleType() | native.StructType():
                sides = tuple(map(Unit.endpoints, kind.item_types if isinstance(kind, native.TupleType) else tuple(field.type for field in kind.fields)))
                lows = tuple(end for low, _ in sides for end in low.to_list())
                highs = tuple(end for _, high in sides for end in high.to_list())
                return (Some(max(lows, key=lambda end: (end[0], not end[1]))) if lows else Nothing, Some(min(highs)) if highs else Nothing)
            case native.FloatType() | native.IntType():
                low = Some((float(kind.ge), True)) if kind.ge is not None else Some((float(kind.gt), False)) if kind.gt is not None else Nothing
                high = Some((float(kind.le), True)) if kind.le is not None else Some((float(kind.lt), False)) if kind.lt is not None else Nothing
                return low, high
            case _:
                return Nothing, Nothing

    @staticmethod
    def integral(kind: native.Type) -> tuple[int, int]:
        """Returns a kind's first and last admissible integers clamped to native int32 storage, an open end at the storage limit."""
        storage = np.iinfo(np.int32)
        low, high = Unit.endpoints(kind)

        def admitted(side: Option[tuple[float, bool]], inward: int, limit: int) -> int:
            return side.map(lambda end: min(max(int(end[0]) + (0 if end[1] else inward), int(storage.min)), int(storage.max))).default_value(limit)

        return admitted(low, 1, int(storage.min)), admitted(high, -1, int(storage.max))

    @staticmethod
    def interval(kind: native.Type) -> tuple[float | None, float | None]:
        """Returns a kind's first and last admissible doubles."""
        low, high = Unit.endpoints(kind)

        def admitted(side: Option[tuple[float, bool]], inward: float) -> float | None:
            return side.map(lambda end: end[0] if end[1] else math.nextafter(end[0], inward)).default_value(None)

        return admitted(low, math.inf), admitted(high, -math.inf)

    def bounds(self, kind: native.Type) -> BlenderResult[tuple[float, float]]:
        """Returns a kind's first and last admissible values in native float32 storage, an open end at the storage limit, or the interval holding no finite storage value."""

        def stored(endpoint: float | None, inward: float) -> BlenderResult[float]:
            if endpoint is None:
                return Ok(-self.largest if inward > 0 else self.largest)
            limited = max(endpoint * self.scale, -self.largest) if inward > 0 else min(endpoint * self.scale, self.largest)
            candidate = np.float32(limited)
            outside = float(candidate) / self.scale < endpoint if inward > 0 else float(candidate) / self.scale > endpoint
            with np.errstate(over="ignore"):
                boundary = float(np.nextafter(candidate, np.float32(inward))) if outside else float(candidate)
            return Ok(boundary) if -self.largest <= boundary <= self.largest else Error((BlenderReadFault(request=repr(kind), error=ValueError("No finite native float32 storage endpoint")),))

        def ordered(ends: tuple[float, float]) -> BlenderResult[tuple[float, float]]:
            low, high = ends
            return Error((BlenderReadFault(request=repr(kind), error=ValueError("No native float32 storage value in interval")),)) if low > high else Ok(ends)

        low, high = self.interval(kind)
        return accumulated((stored(low, math.inf), stored(high, -math.inf))).bind(lambda ends: ordered((ends[0], ends[1])))

    @overload
    def stored(self, value: object, kind: native.FloatType) -> BlenderResult[float]: ...

    @overload
    def stored(self, value: object, kind: native.TupleType | native.StructType) -> BlenderResult[tuple[float, ...]]: ...

    def stored(self, value: object, kind: native.FloatType | native.TupleType | native.StructType) -> BlenderResult[float | tuple[float, ...]]:
        """Returns native float32 storage or a value outside its declared interval."""

        def converted(interval: tuple[float, float]) -> BlenderResult[float | tuple[float, ...]]:
            try:
                low, high = interval
                limits = msgspec.Meta(ge=low, le=high)
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

        def converted(interval: tuple[float, float]) -> BlenderResult[float | tuple[float, ...]]:
            try:
                low, high = interval
                limits = msgspec.Meta(ge=low, le=high)
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
    retained: ClassVar[str] = "__record_defaults__"

    # --- [CLASSES]

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

    @staticmethod
    def default(struct: "type[bpy.types.bpy_struct[object]]", name: str) -> object:
        """Record default a constructed native class retains for one field, `NODEFAULT` where it retains none."""
        return vars(struct)[RecordGroup.retained].get(name, msgspec.NODEFAULT)

    @staticmethod
    def body(*structs: "type[bpy.types.bpy_struct[object]]") -> dict[str, object]:
        """Class body a host class takes from constructed native classes: their annotations and retained record defaults."""
        return {
            "__annotations__": {k: v for struct in structs for k, v in struct.__annotations__.items()},
            RecordGroup.retained: {k: v for struct in structs for k, v in vars(struct)[RecordGroup.retained].items()},
        }

    # --- [CONSTRUCTION]

    @staticmethod
    def components(field: msgspec.structs.FieldInfo) -> tuple[msgspec.structs.FieldInfo, ...]:
        """Derives ordered native fields from each fixed tuple's actual annotations."""
        return tuple(
            msgspec.structs.FieldInfo(name=name, encode_name=name, type=annotation, default=field.default[index - 1] if field.default is not msgspec.NODEFAULT else msgspec.NODEFAULT)
            for index, annotation in enumerate(items(field.type), 1)
            for name in (f"item_{index}",)
        )

    @staticmethod
    def entries(field: msgspec.structs.FieldInfo, shape: Entries) -> tuple[msgspec.structs.FieldInfo, ...]:
        """Derives finite dictionary members as the existing enabled/value fields."""
        return tuple(
            msgspec.structs.FieldInfo(
                name=member.name,
                encode_name=member.name,
                type=shape.gate.record,
                default=Gated(enabled=True, value=field.default[member]) if field.default is not msgspec.NODEFAULT and member in field.default else msgspec.NODEFAULT,
            )
            for member in shape.enum
        )

    @staticmethod
    def bound[R: msgspec.Struct](
        identity: Identity, root: Key, record: type[R], changed: Option[Changed] = Nothing, bindings: Option[Mapping[MemberDescriptorType, Choices]] = Nothing
    ) -> "BlenderResult[RecordGroup[R]]":
        """Builds the host classes with native constraints, defaults, and callbacks."""

        def group(path: Key, definitions: dict[str, bpy.props._PropertyDeferred], defaults: Option[dict[str, object]] = Nothing) -> BlenderResult[type[bpy.types.PropertyGroup]]:
            return identity.name(Name.GROUP, path).map(lambda name: type(name, (bpy.types.PropertyGroup,), {"__annotations__": definitions, RecordGroup.retained: defaults.default_value({})}))

        def pointer(built: BlenderResult[type[bpy.types.PropertyGroup]]) -> BlenderResult[bpy.props._PropertyDeferred]:
            return built.map(lambda struct: bpy.props.PointerProperty(type=struct, override={"LIBRARY_OVERRIDABLE"}))

        def updating(member: str) -> Mapping[str, object]:
            return changed.map(lambda body: {"update": lambda holder, context: body(holder, context, holder.bl_rna.properties[member])}).default_value({})

        def defined(field: msgspec.structs.FieldInfo, declared: Declared, path: Key) -> BlenderResult[bpy.props._PropertyDeferred]:
            shape = declared.shape
            default = field.default
            blank = "" if isinstance(shape, OptionalText) and default is None else default
            description = declared.description.default_value("")
            binding = next(iter(markers(field.type, Definition)), Definition())
            presentation = Presentation.of(field.type)
            name = declared.title.default_value(heading(field.name))
            unit = Unit.of(field.type, shape.kind) if isinstance(shape, Number | Vector | Color | Integer) else Unit.quantified(field.type).default_with(Unit)

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
                        return heading(value.name)
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
                            accumulated(defined(component, item, f"{path}_{component.name}").map(lambda prop: (component.name, prop)) for component, item in zip(RecordGroup.components(field), items))
                            .map(dict)
                            .bind(lambda definitions: group(path, definitions))
                        )
                    case Number(kind=kind) | Vector(kind=kind) | Color(kind=kind):

                        def ranged(bounds: tuple[float, float], stored: Option[float | tuple[float, ...]]) -> bpy.props._PropertyDeferred:
                            low, high = bounds
                            soft_low, soft_high, spanned = unit.spanned(presentation, bounds)
                            prop = (
                                bpy.props.FloatProperty()
                                if isinstance(kind, native.FloatType)
                                else bpy.props.FloatVectorProperty(size=len(kind.item_types) if isinstance(kind, native.TupleType) else len(kind.fields))
                            )
                            prop.keywords.update(min=low, max=high, soft_min=soft_low, soft_max=soft_high)
                            prop.keywords.update(unit.drawn(presentation, (soft_low, soft_high), stored))
                            prop.keywords.update(subtype=spanned.number if isinstance(kind, native.FloatType) else spanned.vector, unit=unit.unit)
                            prop.keywords.update(stored.map(lambda value: {"default": value}).default_value({}))
                            return prop

                        return unit.bounds(kind).bind(
                            lambda bounds: (
                                unit.stored(msgspec.to_builtins(default) if isinstance(shape, Color) else default, kind).map(Some) if default is not msgspec.NODEFAULT else Ok(Nothing)
                            ).map(lambda stored: ranged(bounds, stored))
                        )
                    case Nested(record=record):
                        return pointer(recorded(path, record, default))
                    case Gate(value=Option(tag="none")) if default is msgspec.NODEFAULT:
                        return Error((BlenderReadFault(request=repr(field), error=TypeError("A gate whose graph owns the value needs a record default")),))
                    case Gate(record=record):
                        return pointer(recorded(path, record, default, Some(name)))
                    case Integer(kind=whole):
                        soft_low, soft_high, counted = unit.counted(presentation, whole)
                        prop = bpy.props.IntProperty()
                        low, high = Unit.integral(whole)
                        prop.keywords.update(min=low, max=high, soft_min=soft_low, soft_max=soft_high)
                        prop.keywords.update(presentation.step.map(lambda step: {"step": round(step)}).default_value({}))
                        prop.keywords["subtype"] = counted.number
                        return defaulted(prop, default)
                    case Flag():
                        return defaulted(bpy.props.BoolProperty(), default)
                    case Text() | OptionalText():
                        sources = markers(field.type, Choices)
                        source = (
                            sources[0]
                            if sources
                            else Choices(
                                lambda *_: (
                                    *((name, name, space.getDescription()) for space in PyOpenColorIO.GetCurrentConfig().getColorSpaces() for name in (space.getName(),)),
                                    (PyOpenColorIO.ROLE_SCENE_LINEAR, "Working Space", "Working color space of the current file"),
                                )
                            )
                            if isinstance(shape, Text) and shape.space
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
                    case Entries(enum=enum, gate=gate):
                        return pointer(
                            accumulated(
                                defined(entry, Declared(shape=gate, title=Some(enum[entry.name].label if issubclass(enum, Choice) else heading(entry.name))), f"{path}_{entry.name.lower()}").map(
                                    lambda prop: (entry.name, prop)
                                )
                                for entry in RecordGroup.entries(field, shape)
                            )
                            .map(dict)
                            .bind(lambda definitions: group(path, definitions))
                        )
                    case Flags(enum=enum):

                        def flag(member: Enum) -> bpy.props._PropertyDeferred:
                            leaf = bpy.props.BoolProperty(name=member.label if isinstance(member, Choice) else heading(member.name), override={"LIBRARY_OVERRIDABLE"})
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
                    bounds = tuple(
                        f"{label if inclusive else exclusive}: {value * unit.scale:g}"
                        for side, label, exclusive in zip(
                            Unit.endpoints(shape.kind) if isinstance(shape, Number | Vector | Color | Integer) else (Nothing, Nothing), ("Min", "Max"), ("Above", "Below")
                        )
                        for value, inclusive in side.to_list()
                        if abs(value * unit.scale) < Unit.largest
                    )
                    default_text = (f"Default: {shown(blank)}",) if blank is not msgspec.NODEFAULT and field.default_factory is msgspec.NODEFAULT else ()
                    prop.keywords["description"] = ". ".join(part for part in (description, *bounds, *default_text) if part)
                return prop

            return represented().map(described)

        def sourced(record_type: Record, field: msgspec.structs.FieldInfo, shape: Declared) -> BlenderResult[tuple[msgspec.structs.FieldInfo, Declared]]:
            return (
                bindings
                .bind(lambda sources: Option.of_optional(sources.get(getattr(typing.get_origin(record_type) or record_type, field.name))))
                .map(lambda source: msgspec.structs.replace(field, type=typing.Annotated[field.type, source]))
                .map(lambda bound: field_shape(bound.type).map(lambda declared: (bound, declared)))
                .default_value(Ok((field, shape)))
            )

        def recorded(path: Key, record_type: Record, default: object, title: Option[str] = Nothing, inherited: tuple[str, ...] = ()) -> BlenderResult[type[bpy.types.PropertyGroup]]:
            def declared(fields: Fields) -> BlenderResult[type[bpy.types.PropertyGroup]]:
                defaults = {
                    field.name: getattr(default, field.name) if default is not msgspec.NODEFAULT else field.default_factory() if field.default_factory is not msgspec.NODEFAULT else field.default
                    for field, _ in fields
                }
                return (
                    accumulated(
                        defined(
                            msgspec.structs.replace(field, default=defaults[field.name]),
                            msgspec.structs.replace(shape, title=shape.title.or_else(title)) if field.name == ENABLED else shape,
                            f"{path}_{field.name.lower()}",
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
                                if isinstance(shape.shape, Records)
                                or (isinstance(shape.shape, Gate) and shape.shape.value.is_none())
                                or (declarations[field.name].function is bpy.props.EnumProperty and "get" in declarations[field.name].keywords)
                            }),
                        )
                    )
                )

            return field_shapes(record_type).bind(lambda rows: accumulated(sourced(record_type, field, shape) for field, shape in rows if field.name not in inherited)).bind(declared)

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

        def value(field: msgspec.structs.FieldInfo, shape: Shape, owner: "bpy.types.bpy_struct[object]", default: object) -> BlenderResult[object]:
            held = (
                owner.get(field.name, default if default is not msgspec.NODEFAULT else "")
                if isinstance(shape, Text) and isinstance(owner.bl_rna.properties[field.name], bpy.types.EnumProperty)
                else getattr(owner, field.name)
            )
            match shape:
                case Components(items=items):
                    return accumulated(value(component, item.shape, held, component.default) for component, item in zip(RecordGroup.components(msgspec.structs.replace(field, default=default)), items))
                case Number(kind=kind) | Vector(kind=kind):
                    return Unit.of(field.type, kind).restored(held, kind)
                case Color(kind=kind):
                    return Unit.of(field.type, kind).restored(held, kind).map(lambda values: kind.cls(**{channel.name: item for channel, item in zip(msgspec.structs.fields(kind.cls), values)}))
                case Enumerated(enum=enum):
                    return Ok(enum[held])
                case Literals():
                    return Ok(int(held))
                case Nested(record=record_type):
                    return record(held, record_type, type(held))
                case Gate(value=Option(tag="none")) if isinstance(default, Gated):
                    return Ok(Gated(enabled=getattr(held, ENABLED), value=default.value))
                case Gate(value=gated):
                    return (
                        gated
                        .starmap(lambda leaf, inner: value(leaf, inner.shape, held, RecordGroup.default(type(held), leaf.name)))
                        .default_with(lambda: Error((BlenderReadFault(request=f"{owner!r}.{field.name}", error=KeyError(field.name)),)))
                        .map(lambda kept: Gated(enabled=getattr(held, ENABLED), value=kept))
                    )
                case Records(record=item):
                    return accumulated(record(entry, item, type(entry)) for entry in held) if owner.is_property_set(field.name) else Ok(default if default is not msgspec.NODEFAULT else ())
                case Entries(enum=enum, gate=gate):
                    return accumulated(
                        value(leaf, inner.shape, entry, RecordGroup.default(type(entry), leaf.name)).map(lambda held: (enum[member.name], held))
                        for member in RecordGroup.entries(field, shape)
                        for leaf, inner in gate.value.to_list()
                        for entry in (getattr(held, member.name),)
                        if getattr(entry, ENABLED)
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

        def record[R: msgspec.Struct](owner: "bpy.types.bpy_struct[object]", record_type: type[R], struct: "type[bpy.types.bpy_struct[object]]") -> BlenderResult[R]:
            return field_shapes(record_type).bind(
                lambda rows: accumulated(
                    value(field, shape.shape, owner, RecordGroup.default(struct, field.name)).map(lambda held: (field.name, held)) for field, shape in rows if field.name in owner.bl_rna.properties
                ).map(lambda values: record_type(**dict(values)))
            )

        return record(holder, self.record, self.struct)

    # --- [WRITING]

    def write(self, holder: "bpy.types.bpy_struct[object]", record: T) -> BlenderResult[None]:
        """Writes independent changed leaves through the native RNA writer."""

        def value(field: msgspec.structs.FieldInfo, shape: Shape, owner: "bpy.types.bpy_struct[object]", desired: object, default: object) -> BlenderResult[None]:
            match shape:
                case Components(items=items):
                    return accumulated(
                        value(component, item.shape, getattr(owner, field.name), part, component.default)
                        for component, item, part in zip(RecordGroup.components(msgspec.structs.replace(field, default=default)), items, msgspec.convert(desired, tuple))
                    ).map(lambda _: None)
                case Number():
                    return written(owner, field.name, msgspec.convert(desired, float) * Unit.of(field.type, shape.kind).scale).map(lambda _: None)
                case Vector(kind=kind):
                    scale = Unit.of(field.type, kind).scale
                    return written(owner, field.name, tuple(component * scale for component in msgspec.convert(desired, tuple[float, ...]))).map(lambda _: None)
                case Enumerated(enum=enum):
                    return written(owner, field.name, enum(desired).name).map(lambda _: None)
                case Literals():
                    return written(owner, field.name, str(desired)).map(lambda _: None)
                case Color(kind=kind):
                    return written(owner, field.name, tuple(getattr(desired, channel.name) for channel in msgspec.structs.fields(kind.cls))).map(lambda _: None)
                case Nested(record=record_type) | Gate(record=record_type):
                    held = getattr(owner, field.name)
                    return stored(held, desired, type(held), record_type)
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
                case Entries(enum=enum, gate=gate):
                    entries = msgspec.convert(desired, dict)
                    return accumulated(
                        stored(entry, Gated(enabled=True, value=entries[key]), type(entry), gate.record) if key in entries else written(entry, ENABLED, False).map(lambda _: None)
                        for member in RecordGroup.entries(field, shape)
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

        def stored(owner: "bpy.types.bpy_struct[object]", desired: object, struct: "type[bpy.types.bpy_struct[object]]", record_type: Record) -> BlenderResult[None]:
            return field_shapes(record_type).bind(
                lambda rows: accumulated(
                    value(field, shape.shape, owner, getattr(desired, field.name), RecordGroup.default(struct, field.name)) for field, shape in rows if field.name in owner.bl_rna.properties
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
