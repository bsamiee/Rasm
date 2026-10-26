"""Record fields bound to Blender properties, display units, and dynamic enums."""

from collections.abc import Callable, Iterable, Mapping
from itertools import starmap
import math
from types import MemberDescriptorType
import typing
from typing import ClassVar, overload, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect
import numpy as np
import PyOpenColorIO

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.settings.shapes import (
    Color,
    Components,
    Declared,
    ENABLED,
    Entries,
    Enumerated,
    EXPANDED,
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
    unwrapped,
    Vector,
)
from blender.records.faults import accumulated, DomainFault, paired
from blender.records.fields import bounds, Choice, Gated, heading, markers, Presentation, Quantity, Rgb

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
    """Dynamic enum rows with stable identifier numbers, row sets kept alive for Blender reads, the value read live from its owner when `current` reads it."""

    source: "Callable[[Identity, bpy.types.bpy_struct[object], bpy.types.Context | None], EnumRows]"
    current: "Option[Callable[[Identity, bpy.types.bpy_struct[object]], str]]" = Nothing
    numbers: dict[str, int] = msgspec.field(default_factory=dict)
    rows: dict[EnumItems, EnumItems] = msgspec.field(default_factory=dict)

    @classmethod
    def named(cls, names: "Callable[[Identity, bpy.types.bpy_struct[object], bpy.types.Context | None], Iterable[str]]") -> Self:
        """Returns rows labelling each identifier by itself."""
        return cls(lambda identity, owner, context: ((name, name, "") for name in names(identity, owner, context)))

    @classmethod
    def mirrored(cls, prop: bpy.types.EnumProperty, admitted: Callable[[str], bool] = lambda _: True) -> Self:
        """Returns rows of a host enum property's items, the identifiers `admitted` keeps alone."""
        return cls(lambda *_: ((item.identifier, item.name, item.description, item.icon, item.value) for item in prop.enum_items if admitted(item.identifier)))

    def defined(self, identity: Identity, key: str, default: object) -> bpy.props._PropertyDeferred:
        """Builds enum property keeping identifier numbers across item changes."""

        def number(identifier: str) -> int:
            return self.numbers.setdefault(identifier, len(self.numbers))

        def listed(owner: "bpy.types.bpy_struct[object]", context: bpy.types.Context | None) -> EnumItems:
            rows = tuple((*row[:3], row[3] if len(row) == 5 else "NONE", number(row[0]) if row[0] else -1) if row is not None else None for row in self.source(identity, owner, context))
            return self.rows.setdefault(rows, rows)

        def selected(owner: "bpy.types.bpy_struct[object]") -> int:
            return number(self.current.map(lambda read: read(identity, owner)).default_with(lambda: owner.get(key, default)))

        def stored(owner: "bpy.types.bpy_struct[object]", value: int) -> None:
            owner[key] = next(identifier for identifier, held in self.numbers.items() if held == value)

        return bpy.props.EnumProperty(items=listed, get=selected, set=stored)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]


class Definition[T: bpy.types.ID = bpy.types.ID](msgspec.Struct, frozen=True, kw_only=True):
    """Property options and callbacks a record annotation declares."""

    options: frozenset["rna_enums.PropertyFlagItems"] = frozenset({"ANIMATABLE"})
    text: "rna_enums.PropertySubtypeStringItems" = "NONE"
    search: Option[SearchSource] = Nothing
    suggestion: bool = True
    poll: Option[PointerPoll[T]] = Nothing


class Unit(msgspec.Struct, frozen=True, kw_only=True):
    """Numeric display units and scale from record to host storage."""

    number: "rna_enums.PropertySubtypeNumberItems" = "NONE"
    vector: "rna_enums.PropertySubtypeNumberArrayItems" = "NONE"
    unit: "rna_enums.PropertyUnitItems" = "NONE"
    scale: float = 1.0

    # --- [PRESENTATION]

    digits: ClassVar[tuple[int, int]] = (0, 6)
    precision: ClassVar[int] = 2

    @classmethod
    def of(cls, annotation: object, kind: msgspec.inspect.Type) -> Self:
        """Returns display units of annotation and numeric kind."""
        match kind:
            case msgspec.inspect.Metadata(type=inner):
                return cls.of(annotation, inner)
            case msgspec.inspect.StructType(cls=record) if (typing.get_origin(record) or record) is Rgb:
                return cls(vector="COLOR")
            case msgspec.inspect.IntType():
                return cls(number="FACTOR" if (kind.ge, kind.le) == (0, 1) else "NONE")
            case msgspec.inspect.FloatType(ge=low, le=high):
                return cls.quantified(annotation).default_with(lambda: cls(number="FACTOR" if (low, high) == (0, 1) else "NONE"))
            case msgspec.inspect.TupleType(item_types=items):
                factor = all(isinstance(item, msgspec.inspect.FloatType) and (item.ge, item.le) == (0, 1) for item in map(unwrapped, items))
                return cls.quantified(annotation).default_with(lambda: cls(number="FACTOR" if factor else "NONE"))
            case _:
                return cls.quantified(annotation).default_with(cls)

    @classmethod
    def quantified(cls, annotation: object) -> Option[Self]:
        """Returns display units of annotation quantity, `Nothing` when absent."""
        match Presentation.of(annotation).quantity:
            case Option(tag="some", some=Quantity.LENGTH):
                return Some(cls(number="DISTANCE", vector="XYZ_LENGTH"))
            case Option(tag="some", some=Quantity.FOCAL_LENGTH):
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
        """Returns positive span width inside float32 range, `Nothing` otherwise."""
        return Some(high - low) if low is not None and high is not None and -cls.largest < low < high < cls.largest else Nothing

    def spanned(self, presentation: Presentation, bounds: tuple[float, float]) -> tuple[float, float, Self]:
        """Returns host-unit soft span else bounds, dropping factor subtype for an unbounded span."""
        low, high = presentation.soft.map(lambda limits: (limits[0] * self.scale, limits[1] * self.scale)).default_value(bounds)
        return low, high, self if self.number != "FACTOR" or self.extent(low, high).is_some() else msgspec.structs.replace(self, number="NONE")

    def counted(self, presentation: Presentation, kind: msgspec.inspect.IntType) -> tuple[tuple[int, int], tuple[int, int], Self]:
        """Returns valid range clamped to int32 storage, whole-value soft span else that range, and units."""
        storage = np.iinfo(np.int32)
        low, high = (int(np.clip(end, storage.min, storage.max)) for end in bounds(kind))
        soft_low, soft_high, shown = self.spanned(presentation, (low, high))
        return (low, high), (round(soft_low), round(soft_high)), shown

    def drawn(self, presentation: Presentation, span: tuple[float, float], default: Option[float | tuple[float, ...]]) -> dict[str, float]:
        """Returns step and precision keywords over a drawn span.

        Args:
            presentation: Field display settings.
            span: Drawn span in host units.
            default: Stored host default, `Nothing` when absent.

        Returns:
            `step` in Blender hundredths, declared step else hundredth of finite span, capped at smallest nonzero default's leading decimal.
            `precision` declared, else step and span low end decimals above Blender default, widened to shortest default decimals.
        """
        extent = self.extent(*span)
        values = default.map(lambda value: value if isinstance(value, tuple) else (value,)).default_value(())
        leading = tuple(10.0 ** math.floor(math.log10(abs(value))) for value in values if value != 0.0)
        increment = Option.of_optional(min((*presentation.step.map(lambda step: step * self.scale).or_else(extent.map(lambda width: width / 100.0)).to_list(), *leading), default=None))

        def reached(size: float) -> int:
            return max(-math.floor(math.log10(abs(value))) for value in (size, *extent.map(lambda _: span[0]).to_list()) if value != 0.0)

        def decimals(value: float) -> int:
            return len(np.format_float_positional(np.float32(value), unique=True, trim="-").partition(".")[2])

        shortest = max(map(decimals, values), default=0)
        places = presentation.precision.map(lambda count: max(count, shortest)).or_else(Some(max(increment.map(reached).default_value(0), shortest)).filter(lambda count: count > self.precision))
        return {**increment.map(lambda size: {"step": size * 100.0}).default_value({}), **places.map(lambda count: {"precision": min(max(count, self.digits[0]), self.digits[1])}).default_value({})}

    # --- [STORAGE]

    largest: ClassVar[float] = float(np.nextafter(np.float32(np.inf), np.float32(0)))

    def bounds(self, kind: msgspec.inspect.Type) -> BlenderResult[tuple[float, float]]:
        """Returns first and last valid float32 host values of the declared bounds, open ends at range limit."""

        def bound(endpoint: float, inward: float) -> BlenderResult[float]:
            if math.isinf(endpoint):
                return Ok(-self.largest if inward > 0 else self.largest)
            limited = max(endpoint * self.scale, -self.largest) if inward > 0 else min(endpoint * self.scale, self.largest)
            candidate = np.float32(limited)
            outside = float(candidate) / self.scale < endpoint if inward > 0 else float(candidate) / self.scale > endpoint
            with np.errstate(over="ignore"):
                boundary = float(np.nextafter(candidate, np.float32(inward))) if outside else float(candidate)
            return Ok(boundary) if -self.largest <= boundary <= self.largest else _refused(kind, endpoint, "endpoints with a finite float32 value")

        def ordered(ends: tuple[float, float]) -> BlenderResult[tuple[float, float]]:
            return _refused(kind, ends, "intervals holding a float32 value") if ends[0] > ends[1] else Ok(ends)

        low, high = bounds(kind)
        return paired(bound(low, math.inf), bound(high, -math.inf)).bind(ordered)

    @overload
    def stored(self, value: object, kind: msgspec.inspect.FloatType) -> BlenderResult[float]: ...

    @overload
    def stored(self, value: object, kind: msgspec.inspect.TupleType | msgspec.inspect.StructType) -> BlenderResult[tuple[float, ...]]: ...

    def stored(self, value: object, kind: msgspec.inspect.FloatType | msgspec.inspect.TupleType | msgspec.inspect.StructType) -> BlenderResult[float | tuple[float, ...]]:
        """Returns float32 host value of a domain value, domain fault outside declared interval."""
        return self.converted(value, kind, inward=True)

    @overload
    def restored(self, value: object, kind: msgspec.inspect.FloatType) -> BlenderResult[float]: ...

    @overload
    def restored(self, value: object, kind: msgspec.inspect.TupleType | msgspec.inspect.StructType) -> BlenderResult[tuple[float, ...]]: ...

    def restored(self, value: object, kind: msgspec.inspect.FloatType | msgspec.inspect.TupleType | msgspec.inspect.StructType) -> BlenderResult[float | tuple[float, ...]]:
        """Returns domain value of a host value, domain fault outside declared interval."""
        return self.converted(value, kind, inward=False)

    def converted(self, value: object, kind: msgspec.inspect.FloatType | msgspec.inspect.TupleType | msgspec.inspect.StructType, *, inward: bool) -> BlenderResult[float | tuple[float, ...]]:
        """Checks the host value against the float32 interval, scaling a domain value into host units first when inward and the host value out after otherwise.

        Args:
            value: Domain value when inward, host value otherwise.
            kind: Scalar, tuple, or color record type.
            inward: Domain to host direction.

        Returns:
            Host or domain value, domain fault for a value outside the interval or of another length.
        """

        def checked(interval: tuple[float, float]) -> BlenderResult[float | tuple[float, ...]]:
            bounded = typing.Annotated[float, msgspec.Meta(ge=interval[0], le=interval[1])]
            match kind:
                case msgspec.inspect.FloatType():
                    shape: object = bounded
                case msgspec.inspect.TupleType(item_types=members) | msgspec.inspect.StructType(fields=members):
                    shape = typing.Annotated[tuple[bounded, ...], msgspec.Meta(min_length=len(members), max_length=len(members))]
            try:
                source = np.asarray(value, dtype=np.float64)
            except (TypeError, ValueError) as error:
                return _refused(kind, value, str(error))
            with np.errstate(over="ignore"):
                host = (source * self.scale).astype(np.float32) if inward else source
            held = catch(lambda: msgspec.convert(host.tolist(), shape), exception=msgspec.ValidationError)().map_error(
                lambda error: (DomainFault(subject=repr(kind), value=host.tolist(), accepted=str(error)),)
            )
            return held if inward else held.map(lambda checked: checked / self.scale if isinstance(checked, float) else tuple(item / self.scale for item in checked))

        return self.bounds(kind).bind(checked)


class RecordGroup[T: msgspec.Struct, S: "bpy.types.bpy_struct[object]" = bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Property group classes binding one record to host storage."""

    record: type[T]
    struct: type[S]
    retained: ClassVar[str] = "__record_defaults__"

    # --- [CLASSES]

    @property
    def nested(self) -> tuple[type[bpy.types.PropertyGroup], ...]:
        """Nested property group classes, children first."""

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
        """Returns built class field default, `NODEFAULT` when absent."""
        return vars(struct)[RecordGroup.retained].get(name, msgspec.NODEFAULT)

    @staticmethod
    def body(*structs: "type[bpy.types.bpy_struct[object]]") -> dict[str, object]:
        """Returns class body of annotations and record defaults."""
        return {
            "__annotations__": {k: v for struct in structs for k, v in struct.__annotations__.items()},
            RecordGroup.retained: {k: v for struct in structs for k, v in vars(struct)[RecordGroup.retained].items()},
        }

    # --- [CONSTRUCTION]

    @staticmethod
    def components(field: msgspec.structs.FieldInfo) -> tuple[msgspec.structs.FieldInfo, ...]:
        """Returns item fields of fixed tuple field in order."""
        return tuple(
            msgspec.structs.FieldInfo(name=name, encode_name=name, type=annotation, default=field.default[index - 1] if field.default is not msgspec.NODEFAULT else msgspec.NODEFAULT)
            for index, annotation in enumerate(items(field.type), 1)
            for name in (f"item_{index}",)
        )

    @staticmethod
    def entries(field: msgspec.structs.FieldInfo, shape: Entries) -> tuple[msgspec.structs.FieldInfo, ...]:
        """Returns gated field per enum member of dictionary field."""
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
        """Builds property group classes of record."""
        return _Build(identity=identity, changed=changed, bindings=bindings).recorded(root, record, msgspec.NODEFAULT).map(lambda struct: RecordGroup(record=record, struct=struct))

    @property
    def pointer(self: "RecordGroup[T, bpy.types.PropertyGroup]") -> bpy.props._PropertyDeferred:
        """Root pointer property, library overridable."""
        prop = bpy.props.PointerProperty(type=self.struct, override={"LIBRARY_OVERRIDABLE"})
        prop.keywords.update(Option.of_optional(self.record.__doc__).map(lambda doc: {"description": doc}).default_value({}))
        return prop

    # --- [VALUES]

    def read(self, holder: "bpy.types.bpy_struct[object]") -> BlenderResult[T]:
        """Reads record from property values."""
        return _loaded(holder, self.record, self.struct)

    def write(self, holder: "bpy.types.bpy_struct[object]", record: T) -> BlenderResult[None]:
        """Writes record leaves through `written`."""
        return _stored(holder, record, self.struct, self.record)

    def reset(self, holder: "bpy.types.bpy_struct[object]") -> BlenderResult[None]:
        """Restores property defaults, writing record defaults into collections RNA cannot default."""

        def reset(owner: "bpy.types.bpy_struct[object]", definitions: dict[str, bpy.props._PropertyDeferred]) -> BlenderResult[None]:
            def unset(key: str, definition: bpy.props._PropertyDeferred) -> BlenderResult[None]:
                match owner.bl_rna.properties[key]:
                    case bpy.types.PointerProperty(fixed_type=fixed) if issubclass(type(fixed), bpy.types.PropertyGroup):
                        return reset(getattr(owner, key), type(fixed).__annotations__)
                    case bpy.types.PointerProperty():
                        return _put(owner, key, None)
                    case bpy.types.EnumProperty() if "get" in definition.keywords:
                        owner.id_properties_ensure().pop(key, None)
                    case _:
                        owner.property_unset(key)
                owner.path_resolve(key, False).update()
                return Ok(None)

            return accumulated(starmap(unset, definitions.items())).map(lambda _: None)

        return (
            reset(holder, {key: definition for key, definition in self.struct.__annotations__.items() if key in self.record.__struct_fields__})
            .bind(lambda _: self.read(holder))
            .bind(lambda record: self.write(holder, record))
        )


class _Build(msgspec.Struct, frozen=True, kw_only=True):
    """Group construction of one record binding: its extension, change callback, and dynamic enum sources."""

    identity: Identity
    changed: Option[Changed]
    bindings: Option[Mapping[MemberDescriptorType, Choices]]

    def group(self, path: Key, definitions: dict[str, bpy.props._PropertyDeferred], defaults: Option[dict[str, object]] = Nothing) -> BlenderResult[type[bpy.types.PropertyGroup]]:
        """Builds a named property group class."""
        return self.identity.name(Name.GROUP, path).map(lambda name: type(name, (bpy.types.PropertyGroup,), {"__annotations__": definitions, RecordGroup.retained: defaults.default_value({})}))

    def grouped(self, path: Key, members: Iterable[BlenderResult[tuple[str, bpy.props._PropertyDeferred]]]) -> BlenderResult[bpy.props._PropertyDeferred]:
        """Builds pointer to a group of member definitions."""
        return _pointer(accumulated(members).map(dict).bind(lambda definitions: self.group(path, definitions)))

    def updating(self, member: str) -> Mapping[str, object]:
        """Returns update keyword calling the change callback with member's property."""
        return self.changed.map(lambda body: {"update": lambda holder, context: body(holder, context, holder.bl_rna.properties[member])}).default_value({})

    def defined(self, field: msgspec.structs.FieldInfo, declared: Declared, path: Key) -> BlenderResult[bpy.props._PropertyDeferred]:
        """Builds field property with name, description, override, options, and update."""
        at = _Field(build=self, field=field, declared=declared, path=path)
        return _codec(declared.shape).defined(at).map(at.described)

    def recorded(
        self, path: Key, record: Record, default: object, title: Option[str] = Nothing, inherited: tuple[str, ...] = (), *, expanded: bool = False
    ) -> BlenderResult[type[bpy.types.PropertyGroup]]:
        """Builds record group with retained defaults of members RNA cannot default."""

        def sourced(field: msgspec.structs.FieldInfo, shape: Declared) -> BlenderResult[tuple[msgspec.structs.FieldInfo, Declared]]:
            return (
                self.bindings
                .bind(lambda sources: Option.of_optional(sources.get(getattr(typing.get_origin(record) or record, field.name))))
                .map(lambda source: msgspec.structs.replace(field, type=typing.Annotated[field.type, source]))
                .map(lambda bound: field_shape(bound.type).map(lambda held: (bound, held)))
                .default_value(Ok((field, shape)))
            )

        def declared(fields: Fields) -> BlenderResult[type[bpy.types.PropertyGroup]]:
            defaults = {
                field.name: getattr(default, field.name) if default is not msgspec.NODEFAULT else field.default_factory() if field.default_factory is not msgspec.NODEFAULT else field.default
                for field, _ in fields
            }

            def retained(declarations: dict[str, bpy.props._PropertyDeferred]) -> dict[str, object]:
                return {
                    field.name: defaults[field.name]
                    for field, shape in fields
                    if isinstance(shape.shape, Records)
                    or (isinstance(shape.shape, Gate) and shape.shape.value.is_none())
                    or (declarations[field.name].function is bpy.props.EnumProperty and "get" in declarations[field.name].keywords)
                }

            return (
                accumulated(
                    self.defined(
                        msgspec.structs.replace(field, default=defaults[field.name]),
                        msgspec.structs.replace(shape, title=shape.title.or_else(title)) if field.name == ENABLED else shape,
                        f"{path}_{field.name.lower()}",
                    ).map(lambda prop: (field.name, prop))
                    for field, shape in fields
                )
                .map(dict)
                .bind(
                    lambda declarations: self.group(
                        path, declarations | ({EXPANDED: bpy.props.BoolProperty(name="Expanded", options=set(), override={"LIBRARY_OVERRIDABLE"})} if expanded else {}), Some(retained(declarations))
                    )
                )
            )

        return field_shapes(record).bind(lambda rows: accumulated(sourced(field, shape) for field, shape in rows if field.name not in inherited)).bind(declared)


class _Field(msgspec.Struct, frozen=True, kw_only=True):
    """One field's definition inputs."""

    build: _Build
    field: msgspec.structs.FieldInfo
    declared: Declared
    path: Key

    @property
    def name(self) -> str:
        """Declared title, else field name heading."""
        return self.declared.title.default_value(heading(self.field.name))

    @property
    def binding(self) -> Definition:
        """Declared property options and callbacks."""
        return next(iter(markers(self.field.type, Definition)), Definition())

    @property
    def presentation(self) -> Presentation:
        """Declared display settings."""
        return Presentation.of(self.field.type)

    def defaulted(self, prop: bpy.props._PropertyDeferred, value: object) -> BlenderResult[bpy.props._PropertyDeferred]:
        """Sets host default unless absent."""
        prop.keywords.update({} if value is msgspec.NODEFAULT else {"default": value})
        return Ok(prop)

    def described(self, prop: bpy.props._PropertyDeferred) -> bpy.props._PropertyDeferred:
        """Sets name, description, override, options, and update on a non-pointer property."""
        prop.keywords.update(name=self.name, description=self.declared.description.default_value(""))
        prop.keywords.setdefault("override", {"LIBRARY_OVERRIDABLE"})
        prop.keywords.setdefault("options", set(self.binding.options))
        if prop.function not in {bpy.props.CollectionProperty, bpy.props.PointerProperty}:
            prop.keywords.update(self.build.updating(self.field.name))
        return prop

    def nested(self, member: str, field: msgspec.structs.FieldInfo, declared: Declared) -> BlenderResult[tuple[str, bpy.props._PropertyDeferred]]:
        """Builds member definition under this field's path."""
        return self.build.defined(field, declared, f"{self.path}_{member}").map(lambda prop: (field.name, prop))

    def texted(self, *, space: bool) -> BlenderResult[bpy.props._PropertyDeferred]:
        """Builds dynamic enum for a marked or color space text field, else string property."""
        blank = "" if self.field.default is None else self.field.default

        def chosen(source: Choices) -> BlenderResult[bpy.props._PropertyDeferred]:
            prop = source.defined(self.build.identity, self.field.name, _given(blank, ""))
            prop.keywords["options"] = set(self.binding.options) - {"ANIMATABLE"}
            return Ok(prop)

        def typed() -> BlenderResult[bpy.props._PropertyDeferred]:
            prop = bpy.props.StringProperty(subtype=self.binding.text)
            prop.keywords.update(self.binding.search.map(lambda callback: {"search": callback, "search_options": {"SORT", *({"SUGGESTION"} if self.binding.suggestion else ())}}).default_value({}))
            return self.defaulted(prop, blank)

        return Option.of_optional(next(iter((*markers(self.field.type, Choices), *((SPACES,) if space else ()))), None)).map(chosen).default_with(typed)

    def enumerated(self, rows: tuple[tuple[str, str, str, int], ...], named: Callable[[object], str]) -> BlenderResult[bpy.props._PropertyDeferred]:
        """Builds static enum property of rows, the default named by its identifier."""
        prop = bpy.props.EnumProperty(items=())
        prop.keywords["items"] = rows
        prop.keywords.update({} if self.field.default is msgspec.NODEFAULT else {"default": named(self.field.default)})
        return Ok(prop)


class _Codec(msgspec.Struct, frozen=True, kw_only=True):
    """Property definition, value read, and value write of one field shape."""

    defined: Callable[[_Field], BlenderResult[bpy.props._PropertyDeferred]]
    read: "Callable[[bpy.types.bpy_struct[object], msgspec.structs.FieldInfo, object], BlenderResult[object]]"
    write: "Callable[[bpy.types.bpy_struct[object], msgspec.structs.FieldInfo, object, object], BlenderResult[None]]"

    @classmethod
    def leaf(
        cls,
        defined: Callable[[_Field], BlenderResult[bpy.props._PropertyDeferred]],
        read: Callable[[object, msgspec.structs.FieldInfo], BlenderResult[object]],
        write: Callable[[object, msgspec.structs.FieldInfo], Value],
    ) -> Self:
        """Returns codec of a field held as one host value."""
        return cls(defined=defined, read=lambda owner, field, _: read(getattr(owner, field.name), field), write=lambda owner, field, desired, _: _put(owner, field.name, write(desired, field)))


# --- [CHOICES] --------------------------------------------------------------------------

SPACES = Choices(
    lambda *_: (
        *((name, name, space.getDescription()) for space in PyOpenColorIO.GetCurrentConfig().getColorSpaces() for name in (space.getName(),)),
        (PyOpenColorIO.ROLE_SCENE_LINEAR, "Working Space", "Scene-linear shading and compositing space"),
    )
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _put(owner: "bpy.types.bpy_struct[object]", name: str, value: Value) -> BlenderResult[None]:
    """Writes one member through `written`."""
    return written(owner, name, value).map(lambda _: None)


def _pointer(built: BlenderResult[type[bpy.types.PropertyGroup]]) -> BlenderResult[bpy.props._PropertyDeferred]:
    """Returns library overridable pointer to built group."""
    return built.map(lambda struct: bpy.props.PointerProperty(type=struct, override={"LIBRARY_OVERRIDABLE"}))


def _given(default: object, absent: object) -> object:
    """Returns default, `absent` in place of `NODEFAULT`."""
    return absent if default is msgspec.NODEFAULT else default


def _refused[T](request: object, value: object, accepted: str) -> BlenderResult[T]:
    """Returns domain fault of a declaration or value Blender cannot hold."""
    return Error((DomainFault(subject=repr(request), value=value, accepted=accepted),))


def _loaded[R: msgspec.Struct](owner: "bpy.types.bpy_struct[object]", record: type[R], struct: "type[bpy.types.bpy_struct[object]]") -> BlenderResult[R]:
    """Reads record fields the owner's group declares."""
    return field_shapes(record).bind(
        lambda rows: accumulated(
            _codec(shape.shape).read(owner, field, RecordGroup.default(struct, field.name)).map(lambda held: (field.name, held)) for field, shape in rows if field.name in owner.bl_rna.properties
        ).map(lambda values: record(**dict(values)))
    )


def _stored(owner: "bpy.types.bpy_struct[object]", desired: object, struct: "type[bpy.types.bpy_struct[object]]", record: Record) -> BlenderResult[None]:
    """Writes record fields the owner's group declares."""
    return field_shapes(record).bind(
        lambda rows: accumulated(
            _codec(shape.shape).write(owner, field, getattr(desired, field.name), RecordGroup.default(struct, field.name)) for field, shape in rows if field.name in owner.bl_rna.properties
        ).map(lambda _: None)
    )


def _floating(kind: msgspec.inspect.FloatType | msgspec.inspect.TupleType | msgspec.inspect.StructType) -> Callable[[_Field], BlenderResult[bpy.props._PropertyDeferred]]:
    """Returns float or float vector definition over declared and soft bounds, units, step, and precision."""

    def defined(at: _Field) -> BlenderResult[bpy.props._PropertyDeferred]:
        unit, default = Unit.of(at.field.type, kind), at.field.default

        def ranged(bounds: tuple[float, float], stored: Option[float | tuple[float, ...]]) -> bpy.props._PropertyDeferred:
            low, high = bounds
            soft_low, soft_high, spanned = unit.spanned(at.presentation, bounds)
            match kind:
                case msgspec.inspect.TupleType(item_types=members) | msgspec.inspect.StructType(fields=members):
                    prop = bpy.props.FloatVectorProperty(size=len(members), subtype=spanned.vector)
                case msgspec.inspect.FloatType():
                    prop = bpy.props.FloatProperty(subtype=spanned.number)
            prop.keywords.update(min=low, max=high, soft_min=soft_low, soft_max=soft_high, unit=unit.unit)
            prop.keywords.update(unit.drawn(at.presentation, (soft_low, soft_high), stored))
            prop.keywords.update(stored.map(lambda value: {"default": value}).default_value({}))
            return prop

        host = (
            unit.converted(msgspec.to_builtins(default) if isinstance(kind, msgspec.inspect.StructType) else default, kind, inward=True).map(Some) if default is not msgspec.NODEFAULT else Ok(Nothing)
        )
        return paired(unit.bounds(kind), host).map(lambda pair: ranged(*pair))

    return defined


def _codec(shape: Shape) -> _Codec:
    """Returns definition, read, and write of a field shape."""
    match shape:
        case Number(kind=kind):
            return _Codec.leaf(
                _floating(kind), lambda held, field: Unit.of(field.type, kind).restored(held, kind), lambda desired, field: msgspec.convert(desired, float) * Unit.of(field.type, kind).scale
            )
        case Vector(kind=kind):
            return _Codec.leaf(
                _floating(kind),
                lambda held, field: Unit.of(field.type, kind).restored(held, kind),
                lambda desired, field: tuple(component * Unit.of(field.type, kind).scale for component in msgspec.convert(desired, tuple[float, ...])),
            )
        case Color(kind=kind):
            channels = msgspec.structs.fields(kind.cls)
            return _Codec.leaf(
                _floating(kind),
                lambda held, field: Unit.of(field.type, kind).restored(held, kind).map(lambda values: kind.cls(**{channel.name: item for channel, item in zip(channels, values)})),
                lambda desired, _: tuple(getattr(desired, channel.name) for channel in channels),
            )
        case Integer(kind=whole):

            def counted(at: _Field) -> BlenderResult[bpy.props._PropertyDeferred]:
                (low, high), (soft_low, soft_high), units = Unit.of(at.field.type, whole).counted(at.presentation, whole)
                prop = bpy.props.IntProperty(min=low, max=high, soft_min=soft_low, soft_max=soft_high, subtype=units.number)
                prop.keywords.update(at.presentation.step.map(lambda step: {"step": round(step)}).default_value({}))
                return at.defaulted(prop, at.field.default)

            return _Codec.leaf(counted, lambda held, _: Ok(held), lambda desired, _: msgspec.convert(desired, int))
        case Flag():
            return _Codec.leaf(lambda at: at.defaulted(bpy.props.BoolProperty(), at.field.default), lambda held, _: Ok(held), lambda desired, _: msgspec.convert(desired, bool))
        case Pointer(block=block):

            def pointed(at: _Field) -> BlenderResult[bpy.props._PropertyDeferred]:
                prop = bpy.props.PointerProperty(type=block)
                prop.keywords.update(at.binding.poll.map(lambda poll: {"poll": lambda holder, item: poll(at.build.identity, holder, item)}).default_value({}))
                prop.keywords.update(at.build.updating(at.field.name))
                return Ok(prop)

            return _Codec.leaf(pointed, lambda held, _: Ok(held), lambda desired, _: IdName.of(desired) if isinstance(desired, bpy.types.ID) else None)
        case OptionalText():
            return _Codec.leaf(lambda at: at.texted(space=False), lambda held, _: Ok(held or None), lambda desired, _: Option.of_optional(msgspec.convert(desired, str | None)).default_value(""))
        case Enumerated(enum=enum):
            return _Codec.leaf(
                lambda at: at.enumerated(
                    tuple((member.name, member.label, member.description.default_value(""), member.value) for member in enum), lambda value: next(member.name for member in enum if member == value)
                ),
                lambda held, _: Ok(enum[msgspec.convert(held, str)]),
                lambda desired, _: msgspec.convert(desired, enum).name,
            )
        case Literals(values=values) as literal:
            stored = {str(value): value for value in values}
            return _Codec.leaf(
                lambda at: at.enumerated(tuple((str(value), label, "", value if isinstance(value, int) else index) for index, (value, label) in enumerate(literal.labels.items())), str),
                lambda held, _: Ok(stored[msgspec.convert(held, str)]),
                lambda desired, _: str(desired),
            )
        case Text(space=space):

            def chosen(owner: "bpy.types.bpy_struct[object]", field: msgspec.structs.FieldInfo, default: object) -> Option[str]:
                return Some(owner.get(field.name, _given(default, ""))) if isinstance(owner.bl_rna.properties[field.name], bpy.types.EnumProperty) else Nothing

            def text(owner: "bpy.types.bpy_struct[object]", field: msgspec.structs.FieldInfo, default: object) -> BlenderResult[object]:
                held = chosen(owner, field, default).default_with(lambda: getattr(owner, field.name))
                return catch(lambda: msgspec.convert(held, field.type), exception=msgspec.ValidationError)().map_error(
                    lambda error: (DomainFault(subject=f"{owner!r}.{field.name}", value=held, accepted=str(error)),)
                )

            def wrote(owner: "bpy.types.bpy_struct[object]", field: msgspec.structs.FieldInfo, desired: object, default: object) -> BlenderResult[None]:
                value = msgspec.convert(desired, str)
                return Ok(None) if chosen(owner, field, default) == Some(value) else _put(owner, field.name, value)

            return _Codec(defined=lambda at: at.texted(space=space), read=text, write=wrote)
        case Components(items=parts):
            return _Codec(
                defined=lambda at: at.build.grouped(at.path, (at.nested(component.name, component, item) for component, item in zip(RecordGroup.components(at.field), parts))),
                read=lambda owner, field, default: accumulated(
                    _codec(item.shape).read(getattr(owner, field.name), component, component.default)
                    for component, item in zip(RecordGroup.components(msgspec.structs.replace(field, default=default)), parts)
                ),
                write=lambda owner, field, desired, default: accumulated(
                    _codec(item.shape).write(getattr(owner, field.name), component, part, component.default)
                    for component, item, part in zip(RecordGroup.components(msgspec.structs.replace(field, default=default)), parts, msgspec.convert(desired, tuple))
                ).map(lambda _: None),
            )
        case Nested(record=child):
            return _Codec(
                defined=lambda at: _pointer(at.build.recorded(at.path, child, at.field.default)),
                read=lambda owner, field, _: _loaded(held := getattr(owner, field.name), child, type(held)),
                write=lambda owner, field, desired, _: _stored(held := getattr(owner, field.name), desired, type(held), child),
            )
        case Gate(record=gating, value=gated):

            def gate(at: _Field) -> BlenderResult[bpy.props._PropertyDeferred]:
                if gated.is_none() and at.field.default is msgspec.NODEFAULT:
                    return _refused(at.field, at.field.default, "a record default for a gate whose graph owns the value")
                return _pointer(at.build.recorded(at.path, gating, at.field.default, Some(at.name)))

            def opened(owner: "bpy.types.bpy_struct[object]", field: msgspec.structs.FieldInfo, default: object) -> BlenderResult[object]:
                held = getattr(owner, field.name)
                value = (
                    Ok(default.value)
                    if gated.is_none() and isinstance(default, Gated)
                    else gated.starmap(lambda leaf, inner: _codec(inner.shape).read(held, leaf, RecordGroup.default(type(held), leaf.name))).default_with(
                        lambda: Error((BlenderAbsentFault(owner=repr(held), key=field.name),))
                    )
                )
                return value.map(lambda kept: Gated(enabled=getattr(held, ENABLED), value=kept))

            return _Codec(defined=gate, read=opened, write=lambda owner, field, desired, _: _stored(held := getattr(owner, field.name), desired, type(held), gating))
        case Records(record=item):

            def listed(owner: "bpy.types.bpy_struct[object]", field: msgspec.structs.FieldInfo, default: object) -> BlenderResult[object]:
                if not owner.is_property_set(field.name):
                    return Ok(_given(default, ()))
                return accumulated(_loaded(entry, item, type(entry)) for entry in getattr(owner, field.name))

            def filled(owner: "bpy.types.bpy_struct[object]", field: msgspec.structs.FieldInfo, desired: object, default: object) -> BlenderResult[None]:
                collection, rows, unset = getattr(owner, field.name), msgspec.convert(desired, tuple), not owner.is_property_set(field.name)
                if unset and rows == _given(default, ()):
                    return Ok(None)
                if unset:
                    collection.add()
                for index in range(len(collection) - 1, len(rows) - 1, -1):
                    collection.remove(index)
                for _ in range(len(rows) - len(collection)):
                    collection.add()
                return accumulated(_stored(entry, row, type(entry), item) for entry, row in zip(collection, rows)).map(lambda _: None)

            return _Codec(
                defined=lambda at: at.build.recorded(at.path, item, msgspec.NODEFAULT, inherited=(bpy.types.PropertyGroup.bl_rna.name_property.identifier,), expanded=True).map(  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    lambda struct: bpy.props.CollectionProperty(type=struct, override={"LIBRARY_OVERRIDABLE", "USE_INSERTION"})
                ),
                read=listed,
                write=filled,
            )
        case Entries(enum=enum, gate=Gate(record=entry_record, value=gated) as entry_gate) as entries:

            def kept(owner: "bpy.types.bpy_struct[object]", field: msgspec.structs.FieldInfo, _: object) -> BlenderResult[object]:
                held = getattr(owner, field.name)
                return accumulated(
                    _codec(inner.shape).read(entry, leaf, RecordGroup.default(type(entry), leaf.name)).map(lambda value: (enum[member.name], value))
                    for member in RecordGroup.entries(field, entries)
                    for leaf, inner in gated.to_list()
                    for entry in (getattr(held, member.name),)
                    if getattr(entry, ENABLED)
                ).map(dict)

            def placed(owner: "bpy.types.bpy_struct[object]", field: msgspec.structs.FieldInfo, desired: object, _: object) -> BlenderResult[None]:
                values, held = msgspec.convert(desired, dict), getattr(owner, field.name)
                return accumulated(
                    _stored(entry, Gated(enabled=True, value=values[key]), type(entry), entry_record) if key in values else _put(entry, ENABLED, False)
                    for member in RecordGroup.entries(field, entries)
                    for key, entry in ((enum[member.name], getattr(held, member.name)),)
                ).map(lambda _: None)

            return _Codec(
                defined=lambda at: at.build.grouped(
                    at.path, (at.nested(entry.name.lower(), entry, Declared(shape=entry_gate, title=Some(enum[entry.name].label))) for entry in RecordGroup.entries(at.field, entries))
                ),
                read=kept,
                write=placed,
            )
        case Flags(enum=enum):

            def flagged(at: _Field) -> BlenderResult[bpy.props._PropertyDeferred]:
                def flag(member: Choice) -> bpy.props._PropertyDeferred:
                    leaf = bpy.props.BoolProperty(name=member.label, description=member.description.default_value(at.declared.description.default_value("")), override={"LIBRARY_OVERRIDABLE"})
                    leaf.keywords.update({} if at.field.default is msgspec.NODEFAULT else {"default": member in msgspec.convert(at.field.default, frozenset)})
                    leaf.keywords.update(at.build.updating(member.name))
                    return leaf

                return _pointer(at.build.group(at.path, {member.name: flag(member) for member in enum}))

            return _Codec(
                defined=flagged,
                read=lambda owner, field, _: Ok(frozenset(member for member in enum if getattr(getattr(owner, field.name), member.name))),
                write=lambda owner, field, desired, _: accumulated(_put(getattr(owner, field.name), member.name, member in msgspec.convert(desired, frozenset)) for member in enum).map(lambda _: None),
            )
        case Tagged(tag_field=tag_field, cases=cases):

            def tagged(at: _Field) -> BlenderResult[bpy.props._PropertyDeferred]:
                selector = bpy.props.EnumProperty(items=(), name=at.name, override={"LIBRARY_OVERRIDABLE"})
                selector.keywords["items"] = [(case.__name__, case.title, case.__doc__ or "", number) for number, case in enumerate(cases)]
                selector.keywords.update({} if at.field.default is msgspec.NODEFAULT else {"default": type(at.field.default).__name__})
                selector.keywords.update(at.build.updating(tag_field))
                branches = accumulated(
                    _pointer(at.build.recorded(f"{at.path}_{case.__name__.lower()}", case, at.field.default if isinstance(at.field.default, case) else msgspec.NODEFAULT)).map(
                        lambda prop: (case.__name__, prop)
                    )
                    for case in cases
                )
                return _pointer(branches.bind(lambda props: at.build.group(at.path, {tag_field: selector} | dict(props))))

            def selected(owner: "bpy.types.bpy_struct[object]", field: msgspec.structs.FieldInfo, _: object) -> BlenderResult[object]:
                group = getattr(owner, field.name)
                chosen = next(case for case in cases if case.__name__ == getattr(group, tag_field))
                return _loaded(held := getattr(group, chosen.__name__), chosen, type(held))

            def switched(owner: "bpy.types.bpy_struct[object]", field: msgspec.structs.FieldInfo, desired: object, _: object) -> BlenderResult[None]:
                chosen, group = next(case for case in cases if isinstance(desired, case)), getattr(owner, field.name)
                return _put(group, tag_field, chosen.__name__).bind(lambda _: _stored(held := getattr(group, chosen.__name__), desired, type(held), chosen))

            return _Codec(defined=tagged, read=selected, write=switched)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["SPACES", "Changed", "Choices", "Definition", "EnumItems", "EnumRows", "PointerPoll", "RecordGroup", "SearchSource", "Unit"]
