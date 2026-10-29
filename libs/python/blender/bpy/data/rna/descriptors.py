"""RNA property definitions, their kinds, stored values, native writes, and write order."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial
from itertools import pairwise
import os
from types import MappingProxyType
from typing import Annotated, Final, Generic, TYPE_CHECKING, TypeVar

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
from numpy.typing import NDArray

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.paths import definition, definition_at, Native, rna, RnaPath, root_in, Struct, struct_at
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome, RecordReason, typed

if TYPE_CHECKING:

    class _UncoercedMember(bpy.types.Property):
        """Uncoerced host member `path_resolve(name, False)` answers, which the stubs type as a bare property."""

        def __len__(self) -> int: ...

        def foreach_get(self, seq: NDArray[np.generic], /) -> None: ...


# --- [TYPES] ----------------------------------------------------------------------------

type Scalar = bool | int | float | str
type Value = Scalar | tuple[Scalar, ...] | IdName | None
P_co = TypeVar("P_co", bound=bpy.types.Property, covariant=True, default=bpy.types.Property)


type _Uncoerced = _UncoercedMember

# --- [CONSTANTS] ------------------------------------------------------------------------

LEADING = tuple(
    struct.bl_rna.properties[name]
    for struct, name in (
        (bpy.types.RenderSettings, "engine"),
        (bpy.types.ImageFormatSettings, "media_type"),
        (bpy.types.ImageFormatSettings, "file_format"),
        (bpy.types.ImageFormatSettings, "color_management"),
        (bpy.types.ColorManagedDisplaySettings, "display_device"),
        (bpy.types.ColorManagedViewSettings, "view_transform"),
        (bpy.types.UnitSettings, "system"),
        (bpy.types.DriverTarget, "id_type"),
    )
)
_GATED = tuple(
    (bpy.types.ImageFormatSettings, bpy.types.ImageFormatSettings.bl_rna.properties[member], bpy.types.ImageFormatSettings.bl_rna.properties["color_management"], "OVERRIDE")
    for member in ("linear_colorspace_settings", "view_settings", "display_settings")
)
_PATHS: Final = frozenset({"FILE_PATH", "DIR_PATH", "FILE_NAME"})
DTYPES: Final[Mapping[type[bpy.types.Property], type[np.bool | np.int32 | np.float32]]] = MappingProxyType({
    bpy.types.BoolProperty: np.bool,
    bpy.types.IntProperty: np.int32,
    bpy.types.FloatProperty: np.float32,
    bpy.types.EnumProperty: np.int32,
})

# --- [MODELS] ---------------------------------------------------------------------------


class Descriptor(msgspec.Struct, Generic[P_co], frozen=True, kw_only=True):
    """Property definition from root type, no live data."""

    root: type[Native]
    owner: RnaPath = ()
    prop: P_co
    coordinates: tuple[int, ...] = ()

    # --- [RESOLUTION]

    @property
    def path(self) -> RnaPath:
        """RNA path from root to property with element coordinates."""
        return (*self.owner, self.prop.identifier, *self.coordinates)

    @property
    def member(self) -> RnaPath:
        """RNA path from root to property, element coordinates left out, the data path an F-curve or driver names."""
        return (*self.owner, self.prop.identifier)

    @property
    def index(self) -> Option[int]:
        """Flat element index the host's `index=` keyword takes, Nothing for a whole member."""
        return Some(self.coordinates).filter(bool).map(lambda at: int(np.ravel_multi_index(at, self.dimensions(self.prop))))

    @staticmethod
    def resolve[K: bpy.types.Property](root: type[Struct] | Native, path: RnaPath, kind: type[K]) -> Outcome["Descriptor[K]"]:
        """Resolves path to descriptor, choice fault for another kind.

        Args:
            root: Struct type whose definitions the path walks, or live struct whose refined definitions answer.
            path: RNA path from root to the member, element coordinates trailing.
            kind: Property class the member must hold.

        Returns:
            Descriptor, absent fault for a path naming no member or element.
        """
        stated = type(root) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else root

        def owned(owner: RnaPath) -> Option[bpy.types.Struct]:
            return struct_at(root, owner, bpy.types.bpy_struct).map(definition) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else definition_at(definition(root), owner)

        def within(prop: bpy.types.Property, coordinates: tuple[int, ...]) -> bool:
            dimensions = Descriptor.dimensions(prop)
            return not coordinates or (len(coordinates) == len(dimensions) and all(0 <= coordinate < size for coordinate, size in zip(coordinates, dimensions)))

        return (
            _split(path)
            .bind(lambda split: owned(split[0]).bind(lambda owner: Option.of_optional(owner.properties.get(split[1]))).map(lambda prop: (split, prop)))
            .filter(lambda found: within(found[1], found[0][2]))
            .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(root), member=rna(path)))
            .bind(lambda found: typed(kind, rna(path), found[1]).map(lambda prop: Descriptor[K](root=stated, owner=found[0][0], prop=prop, coordinates=found[0][2])))
        )

    @staticmethod
    def declared[K: bpy.types.Property](root: type[Struct], path: RnaPath, kind: type[K]) -> "Descriptor[K]":
        """Resolves path of a declaration over the pinned host, a missing member raising as a defect."""
        match Descriptor.resolve(root, path, kind):
            case Result(tag="ok", ok=descriptor):
                return descriptor
            case Result(error=faults):
                raise LookupError(path, faults)

    @staticmethod
    def located(root: Native, path: RnaPath) -> "Outcome[tuple[Struct, Descriptor]]":
        """Resolves path under a live root to the struct holding the member and the member's descriptor.

        Args:
            root: Live struct or collection the path starts from.
            path: RNA path from root to the member, element coordinates trailing.

        Returns:
            Holder and descriptor, absent fault for a path naming no member or a holder the host no longer reaches.
        """
        return Descriptor.resolve(root, path, bpy.types.Property).bind(
            lambda descriptor: (
                struct_at(root, descriptor.owner, bpy.types.bpy_struct)
                .map(lambda holder: (holder, descriptor))
                .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(root), member=rna(descriptor.path)))
            )
        )

    @staticmethod
    def saved(root: type[Struct] | Native, where: Callable[[bpy.types.Property], bool] = lambda _: True) -> "tuple[Descriptor, ...]":
        """Returns members a saved file stores, ID, name, hidden, and registered members left out.

        Args:
            root: Struct type, or live struct whose refined definition answers.
            where: Members kept beyond the saved rule.

        Returns:
            Descriptors of root's own members in declaration order.
        """
        held = definition(root)
        base = bpy.types.ID.bl_rna.properties.values()
        return tuple(
            Descriptor(root=type(root) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else root, prop=prop)
            for prop in held.properties
            if not (prop.is_hidden or prop.is_skip_save or prop.is_deprecated or prop.is_registered or prop.is_registered_optional or prop == held.name_property or prop in base) and where(prop)
        )

    def owner_of(self, context: Native, item: Option[Native] = Nothing) -> Option[Struct]:
        """Returns live property owner from item, root, or context."""
        return (
            item
            .filter(lambda value: isinstance(value, self.root))
            .or_else_with(lambda: Some(context) if isinstance(context, self.root) else root_in(self.root, context) if isinstance(context, bpy.types.Context) else Nothing)
            .bind(lambda value: struct_at(value, self.owner, bpy.types.bpy_struct))
            .filter(lambda owner: self.prop in definition(owner).properties.values() and effective(owner))
        )

    def leaves(self, compound: tuple[type[Struct], ...] = ()) -> "tuple[Descriptor, ...]":
        """Expands a setting into the leaf members a snapshot captures, `compound` pointers kept whole."""

        def walked(struct: type[Struct], owner: RnaPath, entered: frozenset[str]) -> tuple[Descriptor, ...]:
            def member(found: Descriptor) -> tuple[Descriptor, ...]:
                match found.prop:
                    case _ if found.valued() or (isinstance(found.prop, bpy.types.PointerProperty) and isinstance(found.prop.fixed_type, compound)):
                        return (Descriptor(root=self.root, owner=owner, prop=found.prop),)
                    case bpy.types.PointerProperty(is_readonly=True, fixed_type=bpy.types.bpy_struct() as pointed) if not isinstance(pointed, bpy.types.ID):  # type: ignore[unreachable]
                        return walked(type(pointed), (*owner, found.prop.identifier), entered | {pointed.identifier}) if pointed.identifier not in entered else ()
                    case _:
                        return ()

            return tuple(descriptor for found in Descriptor.saved(struct) for descriptor in member(found))

        match self.prop:
            case bpy.types.PointerProperty(is_readonly=True, fixed_type=bpy.types.bpy_struct() as pointed) if not isinstance(pointed, (bpy.types.ID, *compound)):
                return walked(type(pointed), self.path, frozenset({pointed.identifier}))
            case _:
                return (self,)

    # --- [KINDS]

    @property
    def curve(self) -> bool:
        """Whether the member points at a curve mapping or profile."""
        return isinstance(self.prop, bpy.types.PointerProperty) and isinstance(self.prop.fixed_type, bpy.types.CurveMapping | bpy.types.CurveProfile)

    def valued(self, *, native: bool = False) -> bool:
        """Whether a capture holds the member as a value, a native capture adding struct collections and curve or writable pointers."""
        match self.prop:
            case bpy.types.CollectionProperty():
                return native and not isinstance(self.prop.fixed_type, bpy.types.ID)
            case bpy.types.PointerProperty(is_readonly=True):
                return native and self.curve
            case bpy.types.PointerProperty():
                return native or isinstance(self.prop.fixed_type, bpy.types.ID)
            case _:
                return not self.prop.is_readonly

    # --- [BOUNDS]

    @property
    def meta(self) -> msgspec.Meta:
        """Record constraint of the member: hard range of a number."""
        match self.prop:
            case bpy.types.IntProperty() | bpy.types.FloatProperty() as number:
                return msgspec.Meta(ge=number.hard_min, le=number.hard_max)
            case _:
                return msgspec.Meta()

    # --- [VALUES]

    @staticmethod
    def dimensions(prop: bpy.types.Property) -> tuple[int, ...]:
        """Returns declared numeric array dimensions, empty for other properties."""
        match prop:
            case bpy.types.BoolProperty() | bpy.types.IntProperty() | bpy.types.FloatProperty() as array:
                sizes: Iterable[int] = array.array_dimensions  # type: ignore[assignment]
                return tuple(size for size in sizes if size)
            case _:
                return ()

    def shape(self, owner: Struct) -> tuple[int, ...]:
        """Returns live array dimensions, outer length read from the owner's uncoerced array, a flat `MATRIX` array keeping its flat length."""
        return (len(_uncoerced(owner, self.prop.identifier)), *self.dimensions(definition(owner).properties[self.prop.identifier])[1:])

    def read(self, owner: Native) -> Value:
        """Returns scalar, flat array, element, enum identifiers, `IdName`, struct identifier, or None."""
        name = self.prop.identifier
        match self.prop, owner:
            case bpy.types.EnumProperty(is_enum_flag=True), _:
                return tuple(sorted(getattr(owner, name)))
            case bpy.types.PointerProperty(fixed_type=bpy.types.ID()), _:
                return Option.of_optional(getattr(owner, name)).map(IdName.of).to_optional()  # type: ignore[unreachable]
            case bpy.types.PointerProperty(), _:
                return Option.of_optional(getattr(owner, name)).map(lambda struct: definition(struct).identifier).to_optional()
            case bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True) as prop, bpy.types.bpy_struct():
                shape = self.shape(owner)
                buffer = np.empty(int(np.prod(shape)), DTYPES[type(prop)])
                _uncoerced(owner, name).foreach_get(buffer)
                return buffer.reshape(shape)[self.coordinates].item() if self.coordinates else tuple(buffer.tolist())
            case bpy.types.BoolProperty(is_array=True), _:
                flags = np.asarray(getattr(owner, name), np.bool)
                return flags[self.coordinates].item() if self.coordinates else tuple(flags.ravel().tolist())
            case _:
                return msgspec.convert(msgspec.to_builtins(getattr(owner, name)), bool | int | float | str)

    def equal(self, held: Value | Struct, desired: object) -> bool:
        """Compares floats by float32 bits, other values exactly."""
        if isinstance(self.prop, bpy.types.StringProperty) and isinstance(desired, bytes):
            desired = msgspec.to_builtins(desired) if self.prop.subtype == "BYTE_STRING" else os.fsdecode(desired)
        return np.array_equal(np.asarray(held, np.float32).view(np.uint32), np.asarray(desired, np.float32).view(np.uint32)) if isinstance(self.prop, bpy.types.FloatProperty) else held == desired

    def write(self, owner: Native, value: Value | bytes | Struct) -> Outcome[Option[Value | Struct]]:
        """Assigns value through the host, returning prior value or Nothing when equal.

        Args:
            owner: Struct holding the member.
            value: Stored value, `IdName` of a pointed ID, or struct.

        Returns:
            Prior value, domain fault outside a range the host clamps, refusal fault carrying the host's error.
        """
        name = self.prop.identifier
        prior = getattr(owner, name) if isinstance(self.prop, bpy.types.PointerProperty) else self.read(owner)
        held = IdName.of(prior) if isinstance(prior, bpy.types.ID) and isinstance(value, IdName) else prior

        def published(_: object) -> Option[Value | Struct]:
            if self.prop.is_runtime and isinstance(owner, bpy.types.bpy_struct):
                bpy.msgbus.publish_rna(key=_uncoerced(owner, name))
            return Some(prior)

        def stored() -> Outcome[object]:
            match self.prop, value:
                case bpy.types.FloatProperty() as real, _:
                    number: Outcome[float | tuple[float, ...]] = catch(msgspec.convert, exception=msgspec.ValidationError)(value, float | tuple[float, ...]).map_error(
                        partial(_outside, repr(real), value)
                    )
                    return number.bind(partial(self.quantized, real)).map(lambda held: tuple(held.tolist()) if held.ndim else float(held))
                case bpy.types.IntProperty(hard_min=low, hard_max=high) as integer, _:
                    bounded = Annotated[int, msgspec.Meta(ge=low, le=high)]
                    return _converted(value, bounded | tuple[bounded, ...], repr(integer))
                case bpy.types.StringProperty(subtype="BYTE_STRING") as text, str(encoded):
                    decoded: Outcome[bytes] = catch(msgspec.convert, exception=msgspec.ValidationError)(encoded, bytes).map_error(partial(_outside, repr(text), value))
                    return decoded.bind(partial(self.validated, text))
                case bpy.types.StringProperty() as text, str() | bytes() as content:
                    return self.validated(text, content)
                case _:
                    return Ok(value)

        def hosted(stored: object) -> Outcome[object]:
            match self.prop, stored:
                case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True), _ if self.coordinates and isinstance(
                    owner, bpy.types.bpy_struct
                ):
                    buffer = np.array(msgspec.structs.replace(self, coordinates=()).read(owner)).reshape(self.shape(owner))
                    buffer[self.coordinates] = stored
                    return Ok(buffer.tolist())
                case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True), tuple() if isinstance(owner, bpy.types.bpy_struct):
                    return Ok(np.asarray(stored).reshape(self.shape(owner)).tolist())
                case bpy.types.EnumProperty(is_enum_flag=True), tuple(chosen):
                    return Ok(set(chosen))
                case bpy.types.PointerProperty(fixed_type=bpy.types.ID() as pointed), IdName() as identity:
                    return identity.resolve(type(pointed)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, type(pointed).__name__, member=identity.name))  # type: ignore[unreachable]
                case _:
                    return Ok(stored)

        return (
            Ok(Nothing)
            if self.equal(held, value)
            else stored().bind(hosted).bind(lambda native: _assign(owner, name, native).map_error(partial(Fault.of, BlenderReason.REFUSED, rna(self.path)))).map(published)
        )

    @staticmethod
    @np.errstate(over="ignore")
    def quantized(prop: bpy.types.FloatProperty, value: float | tuple[float, ...] | NDArray[np.float64]) -> Outcome[NDArray[np.float32]]:
        """Returns float32 storage, domain fault outside hard range."""
        return Ok(np.asarray(value, dtype=np.float32)).filter_with(
            lambda target: not np.any((target < prop.hard_min) | (target > prop.hard_max)),
            lambda _: Fault.of(RecordReason.OUTSIDE, repr(prop), str(value), member=f"values within hard range [{prop.hard_min}, {prop.hard_max}]"),
        )

    @staticmethod
    def validated[T: str | bytes](prop: bpy.types.StringProperty, value: T) -> Outcome[T]:
        """Returns storable value, domain fault for NUL or text past the host's length."""
        encoded = (os.fsencode(value) if prop.subtype in _PATHS else value.encode("utf-8")) if isinstance(value, str) else value
        text = Ok(value) if prop.subtype == "BYTE_STRING" else _converted(os.fsdecode(value) if prop.subtype in _PATHS else value, Annotated[str, msgspec.Meta(pattern=r"^[^\x00]*$")], repr(prop))
        return text.bind(lambda _: _converted(encoded, Annotated[bytes, msgspec.Meta(max_length=prop.length_max - 1)], repr(prop)) if prop.length_max else Ok(encoded)).map(lambda _: value)

    def shown(self, value: Value) -> str:
        """Returns enum label or display text of stored value."""
        match self.prop, value:
            case bpy.types.EnumProperty() as prop, str():
                return Option.of_optional(prop.enum_items.get(value)).map(lambda item: item.name).default_value(value)
            case _, bool():
                return "on" if value else "off"
            case bpy.types.StringProperty(subtype="BYTE_STRING"), str():
                return repr(msgspec.convert(value, bytes))
            case _, str():
                return f'"{value}"'
            case _, IdName(name=block):
                return block
            case _, tuple():
                return ", ".join(map(self.shown, value))
            case _, None:
                return "none"
            case _:
                return str(value)


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [READS]

custom_read: Final[Callable[[Struct, str], Result[object, TypeError | ReferenceError]]] = catch(catch(bpy.types.bpy_struct.get, exception=ReferenceError), exception=TypeError)


def effective(struct: Native) -> bool:
    """Whether host reads struct values, false under a gated member its governor disables."""
    return all(
        getattr(parent, governor.identifier) == value
        for parent, child in pairwise((*struct.rna_ancestors(), struct) if isinstance(struct, bpy.types.bpy_struct) else ())
        for owner, member, governor, value in _GATED
        if isinstance(parent, owner) and getattr(parent, member.identifier) == child
    )


def differing[K: RnaPath | Descriptor](root: Native, members: Mapping[K, Value]) -> dict[K, Value]:
    """Returns members whose owner under root is absent or holds another value under `Descriptor.equal`.

    Args:
        root: Struct where descriptor owner paths and paths start.
        members: Value per member descriptor or path.

    Returns:
        Differing members under their given keys, an unresolved path among them.
    """
    return {
        key: value
        for key, value in members.items()
        if not any(
            descriptor.equal(descriptor.read(owner), value)
            for descriptor in ((key,) if isinstance(key, Descriptor) else Descriptor.resolve(root, key, bpy.types.Property).to_option().to_list())
            for owner in struct_at(root, descriptor.owner, bpy.types.bpy_struct).to_list()
        )
    }


# --- [WRITES]


def written(struct: Native, path: RnaPath, value: Value | bytes | Struct) -> Outcome[Option[Value | Struct]]:
    """Writes member at path through `Descriptor.write`."""
    return Descriptor.located(struct, path).bind(lambda found: found[1].write(found[0], value))


def sequenced[W](writes: Iterable[tuple[RnaPath, Struct, Descriptor, W]]) -> tuple[tuple[RnaPath, Struct, Descriptor, W], ...]:
    """Orders writes in host order.

    `LEADING` members go first, each resetting members it governs.
    Others group by owner path in holder class declaration order (socket `subtype` and `dimensions` before `default_value`).
    Elements follow their whole array.

    Args:
        writes: Owner path, holder struct, member descriptor, and value per write.

    Returns:
        Sorted writes.
    """
    return tuple(
        sorted(
            writes,
            key=lambda write: (
                LEADING.index(write[2].prop) if write[2].prop in LEADING else len(LEADING),
                write[0],
                definition(write[1]).properties.find(write[2].prop.identifier),
                write[2].coordinates,
            ),
        )
    )


# --- [READERS]


def _split(path: RnaPath, coordinates: tuple[int, ...] = ()) -> Option[tuple[RnaPath, str, tuple[int, ...]]]:
    """Splits a member path at its last identifier into owner path, identifier, and trailing coordinates."""
    match path:
        case (*owner, str(identifier)):
            return Some((tuple(owner), identifier, coordinates))
        case (*rest, int(coordinate)):
            return _split(tuple(rest), (coordinate, *coordinates))
        case _:
            return Nothing


_assign: Final[Callable[[Native, str, object], Result[object, AttributeError | TypeError | ValueError]]] = catch(
    catch(catch(setattr, exception=AttributeError), exception=TypeError), exception=ValueError
)


def _uncoerced(owner: Struct, name: str) -> _Uncoerced:
    """Returns a member's uncoerced host property, the msgbus key of its value and, for an array, its live length and buffer read."""
    held: _Uncoerced = owner.path_resolve(name, False)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return held


def _outside(subject: str, value: object, error: Exception) -> Faults:
    """Returns the domain fault of a value a conversion refused."""
    return Fault.of(RecordReason.OUTSIDE, subject, str(value), member=str(error))


def _converted(value: object, kind: object, subject: str) -> Outcome[object]:
    """Returns value converted to kind, domain fault naming the refused constraint."""
    held: Outcome[object] = catch(msgspec.convert, exception=msgspec.ValidationError)(value, kind).map_error(partial(_outside, subject, value))
    return held


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["DTYPES", "LEADING", "Descriptor", "P_co", "Scalar", "Value", "custom_read", "differing", "effective", "sequenced", "written"]
