"""RNA property definitions, their kinds, stored values, native writes, and write order."""

from collections import deque
from collections.abc import Callable, Iterable, Mapping
from functools import partial, reduce
import inspect
from itertools import pairwise
import math
import operator
import os
from pathlib import Path
from types import MappingProxyType
from typing import Annotated, Final, Generic, TypeVar

import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
from idprop.types import IDPropertyGroup
import msgspec
import numpy as np
from numpy.typing import NDArray
from typing_extensions import TypeForm

from rasm_bpy.data._ids.store import IdName
from rasm_bpy.data._rna.paths import _defined, _definition, _definition_at, _rna, _struct_at, Native, RnaPath, root_in, Struct
from rasm_records import Error, Fault, Nothing, Ok, Outcome, Reason, typed

# --- [TYPES] ----------------------------------------------------------------------------

type Scalar = bool | int | float | str
type Value = Scalar | tuple[Scalar, ...] | IdName | None
_P_co = TypeVar("_P_co", bound=bpy.types.Property, covariant=True, default=bpy.types.Property)

# --- [CONSTANTS] ------------------------------------------------------------------------

_LEADING = tuple(
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
_GATED = (
    *(
        (bpy.types.ImageFormatSettings, bpy.types.ImageFormatSettings.bl_rna.properties[member], bpy.types.ImageFormatSettings.bl_rna.properties["color_management"], "OVERRIDE")
        for member in ("linear_colorspace_settings", "view_settings", "display_settings")
    ),
    *(
        (bpy.types.CompositorNodeImage, bpy.types.CompositorNodeImage.bl_rna.properties[member], bpy.types.CompositorNodeImage.bl_rna.properties[governor], True)
        for member, governor in (("layer", "has_layers"), ("view", "has_views"))
    ),
)
_PATHS: Final = frozenset({"FILE_PATH", "DIR_PATH", "FILE_NAME"})
_SCRIPTS: Final[Path] = Path(bpy.utils.system_resource("SCRIPTS"))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_DTYPES: Final[Mapping[type[bpy.types.Property], type[np.bool | np.int32 | np.float32]]] = MappingProxyType({
    bpy.types.BoolProperty: np.bool,
    bpy.types.IntProperty: np.int32,
    bpy.types.FloatProperty: np.float32,
    bpy.types.EnumProperty: np.int32,
})

# --- [MODELS] ---------------------------------------------------------------------------


class Descriptor(msgspec.Struct, Generic[_P_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Property definition from root type, no live data."""

    # --- [STATE]

    root: type[Native]
    owner: RnaPath = ()
    prop: _P_co
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
    def split(path: RnaPath, /) -> Option[tuple[RnaPath, str, tuple[int, ...]]]:
        """Splits a member path at its last identifier into owner path, identifier, and trailing coordinates."""
        coordinates: deque[int] = deque()
        for index in range(len(path) - 1, -1, -1):
            match path[index]:
                case int(coordinate):
                    coordinates.appendleft(coordinate)
                case str(identifier):
                    return Some((path[:index], identifier, tuple(coordinates)))
                case _:
                    return Nothing
        return Nothing

    @staticmethod
    def resolve[K: bpy.types.Property](root: type[Struct] | Native, path: RnaPath, kind: type[K]) -> Outcome["Descriptor[K]"]:
        """Resolves path to descriptor, outside fault for another kind.

        Args:
            root: Struct type whose definitions the path walks, or live struct whose refined definitions apply.
            path: RNA path from root to the member, element coordinates trailing.
            kind: Property class the member must hold.

        Returns:
            Descriptor, absent fault for a path naming no member or element.
        """
        stated = type(root) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else root

        def owned(owner: RnaPath) -> Option[bpy.types.Struct]:
            return _struct_at(root, owner).bind(_defined) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else _definition_at(_definition(root), owner)

        def within(prop: bpy.types.Property, coordinates: tuple[int, ...]) -> bool:
            dimensions = Descriptor.dimensions(prop)
            return not coordinates or (len(coordinates) == len(dimensions) and all(0 <= coordinate < size for coordinate, size in zip(coordinates, dimensions)))

        return (
            Descriptor
            .split(path)
            .bind(lambda split: owned(split[0]).bind(lambda owner: catch(lambda: owner.properties[split[1]], exception=KeyError)().to_option()).map(lambda prop: (split, prop)))
            .filter(lambda found: within(found[1], found[0][2]))
            .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(root), member=_rna(path)))
            .bind(lambda found: typed(kind, _rna(path), found[1]).map(lambda prop: Descriptor[K](root=stated, owner=found[0][0], prop=prop, coordinates=found[0][2])))
        )

    @staticmethod
    def located(root: Native, path: RnaPath) -> "Outcome[tuple[Native, Descriptor]]":
        """Resolves path under a live root to the member's descriptor and holder, a collection holding members it declares (`RenderSlots.active`).

        Args:
            root: Live struct or collection the path starts from.
            path: RNA path from root to the member, element coordinates trailing.

        Returns:
            Holder and descriptor, absent fault for a path naming no member or a holder the host no longer reaches.
        """
        return Descriptor.resolve(root, path, bpy.types.Property).bind(
            lambda descriptor: _struct_at(root, descriptor.owner).map(lambda holder: (holder, descriptor)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(root), member=_rna(descriptor.path)))
        )

    @staticmethod
    def saved(
        root: type[Struct] | Native, where: Callable[[bpy.types.Property], bool] = lambda _: True, *, owned: Callable[[str], bool] = lambda _: False, inactive: bool = False
    ) -> "tuple[Descriptor, ...]":
        """Returns host-declared members a saved file stores, without ID, name, hidden, and registered members and members a live struct's governor disables.

        Script properties assigned onto an existing class hold their definition as a class attribute, unlike host members and group annotations.
        These stay out unless bundled scripts define the pointed group (`Scene.cycles`) or `owned` names them, no read running another add-on's callbacks.
        Script pointers no write reached on a live struct stay out, a read creating storage the host saves.

        Args:
            root: Struct type, or live struct whose refined definition applies.
            where: Members kept beyond the saved rule.
            owned: Whether the reading extension declares a script property by its identifier, kept beside the host's own.
            inactive: Whether saved members disabled by their current governor remain included.

        Returns:
            Descriptors of root's own members in declaration order.
        """
        held, kind = _definition(root), type(root) if isinstance(root, bpy.types.bpy_struct | bpy.types.bpy_prop_collection) else root
        base = bpy.types.ID.bl_rna.properties.values()

        def declared(prop: bpy.types.Property) -> bool:
            match getattr(kind, prop.identifier, None):
                case bpy.props._PropertyDeferred(keywords={"type": type() as group}):
                    return owned(prop.identifier) or catch(inspect.getfile, exception=TypeError)(group).map(lambda file: Path(file).is_relative_to(_SCRIPTS)).default_value(value=False)
                case bpy.props._PropertyDeferred():
                    return owned(prop.identifier)
                case _:
                    return True

        return tuple(
            Descriptor(root=kind, prop=prop)
            for prop in held.properties
            if not (prop.is_hidden or prop.is_skip_save or prop.is_deprecated or prop.is_registered or prop.is_registered_optional or prop == held.name_property or prop in base)
            and declared(prop)
            and not (isinstance(root, bpy.types.bpy_struct) and isinstance(prop, bpy.types.PointerProperty) and prop.is_runtime and not stored(root, prop.identifier))
            and (inactive or not any(isinstance(root, owner) and prop.identifier == member.identifier and getattr(root, governor.identifier) != value for owner, member, governor, value in _GATED))
            and where(prop)
        )

    def owner_of(self, context: Native, item: Option[Native] = Nothing, *, inactive: bool = False) -> Option[Native]:
        """Returns live property owner from item, root, or context."""
        match item.filter(lambda value: isinstance(value, self.root)).default_value(context):
            case start if isinstance(start, self.root):
                found: Option[Native] = Some(start)
            case bpy.types.Context() as start:
                found = root_in(self.root, start)
            case _:
                found = Nothing
        return found.bind(lambda value: _struct_at(value, self.owner)).filter(
            lambda owner: _defined(owner).filter(lambda held: self.prop in held.properties.values()).is_some() and (inactive or _effective(owner))
        )

    def leaves(self, compound: tuple[type[Struct], ...] = (), entered: frozenset[str] = frozenset()) -> "tuple[Descriptor, ...]":
        """Expands a setting into the leaf members a snapshot captures, `compound` pointers kept whole, a nested member kept when valued and structs in `entered` left out."""
        match self.prop:
            case bpy.types.PointerProperty(is_readonly=True, fixed_type=bpy.types.bpy_struct() as pointed) if not isinstance(pointed, (bpy.types.ID, *compound)):
                inside = entered | {pointed.identifier}
                return (
                    ()
                    if pointed.identifier in entered
                    else tuple(leaf for found in Descriptor.saved(type(pointed)) for leaf in Descriptor(root=self.root, owner=self.path, prop=found.prop).leaves(compound, inside))
                )
            case bpy.types.PointerProperty(fixed_type=fixed) if isinstance(fixed, compound):
                return (self,)
            case _:
                return (self,) if self.valued() or not entered else ()

    # --- [KINDS]

    @property
    def compound(self) -> bool:
        """Whether a native capture holds the member whole: a collection of structs other than IDs, or a curve mapping or profile pointer."""
        match self.prop:
            case bpy.types.CollectionProperty(fixed_type=fixed):
                return not isinstance(fixed, bpy.types.ID)
            case bpy.types.PointerProperty(fixed_type=fixed):
                return isinstance(fixed, bpy.types.CurveMapping | bpy.types.CurveProfile)
            case _:
                return False

    def valued(self, *, native: bool = False) -> bool:
        """Whether a capture holds the member as a value, a native capture adding compounds and writable pointers."""
        match self.prop:
            case bpy.types.CollectionProperty() | bpy.types.PointerProperty(is_readonly=True):
                return native and self.compound
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
                return tuple(int(size) for size in np.asarray(array.array_dimensions) if size)
            case _:
                return ()

    def shape(self, owner: Struct) -> tuple[int, ...]:
        """Returns live array dimensions, outer length read from the owner's uncoerced array, a flat `MATRIX` array keeping its flat length."""
        array: bpy.types.bpy_prop_array = owner.path_resolve(self.prop.identifier, False)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]  # ruff: ignore[boolean-positional-value-in-call]
        return (len(array), *self.dimensions(_definition(owner).properties[self.prop.identifier])[1:])  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    def read(self, owner: Native) -> Value:
        """Returns scalar, flat array, element, enum identifiers, `IdName`, struct identifier, or None."""
        name = self.prop.identifier
        match self.prop, owner:
            case bpy.types.EnumProperty(is_enum_flag=True), _:
                identifiers: set[str] = getattr(owner, name)  # ty: ignore[unsound-assignment]
                return tuple(sorted(identifiers))
            case bpy.types.PointerProperty(fixed_type=bpy.types.ID()), _:
                return Option.of_optional(getattr(owner, name)).map(IdName.of).to_optional()  # type: ignore[unreachable]
            case bpy.types.PointerProperty(), _:
                return Option.of_optional(getattr(owner, name)).map(lambda struct: _definition(struct).identifier).to_optional()
            case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True), bpy.types.bpy_struct() if self.coordinates:
                scalar: Scalar = owner.path_resolve(_rna((name, *self.coordinates)))  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                return scalar
            case bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True) as prop, bpy.types.bpy_struct():
                shape = self.shape(owner)
                buffer: np.ndarray[tuple[int], np.dtype[np.bool | np.int32 | np.float32]] = np.empty(math.prod(shape), _DTYPES[type(prop)])
                array: bpy.types.bpy_prop_array = owner.path_resolve(name, False)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]  # ruff: ignore[boolean-positional-value-in-call]
                array.foreach_get(buffer)
                return tuple[bool | int | float, ...](buffer.tolist())
            case bpy.types.BoolProperty(is_array=True), _:
                flags = np.asarray(getattr(owner, name), np.bool)
                return bool(flags[self.coordinates]) if self.coordinates else tuple(map(bool, flags.ravel()))
            case bpy.types.StringProperty(subtype="BYTE_STRING"), _:
                encoded: str = msgspec.to_builtins(getattr(owner, name))  # ty: ignore[unsound-assignment]
                return encoded
            case _:
                value: Scalar = getattr(owner, name)  # ty: ignore[unsound-assignment]
                return value

    def equal(self, held: Value | Struct, desired: object) -> bool:
        """Compares floats by float32 bits, other values exactly."""
        if isinstance(self.prop, bpy.types.StringProperty) and isinstance(desired, bytes):
            desired = msgspec.to_builtins(desired) if self.prop.subtype == "BYTE_STRING" else os.fsdecode(desired)
        if isinstance(self.prop, bpy.types.FloatProperty):
            return (
                _converted(desired, float | tuple[float, ...], repr(self.prop))
                .map(lambda number: np.array_equal(np.asarray(held, np.float32).view(np.uint32), np.asarray(number, np.float32).view(np.uint32)))
                .default_value(value=False)
            )
        return held == desired

    def write(self, owner: Native, value: Value | bytes | Struct) -> Outcome[Option[Value | Struct]]:
        """Assigns value through the host, returning prior value or Nothing when equal.

        Args:
            owner: Struct holding the member.
            value: Stored value, `IdName` of a pointed ID, or struct.

        Returns:
            Prior value, outside fault outside a range the host clamps, refused fault holding the host's error.
        """
        name = self.prop.identifier
        prior: Value | Struct = getattr(owner, name) if isinstance(self.prop, bpy.types.PointerProperty) else self.read(owner)  # ty: ignore[unsound-assignment]
        held = IdName.of(prior) if isinstance(prior, bpy.types.ID) and isinstance(value, IdName) else prior

        def published(_: object) -> Option[Value | Struct]:
            if self.prop.is_runtime and isinstance(owner, bpy.types.bpy_struct):
                bpy.msgbus.publish_rna(key=owner.path_resolve(name, False))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]  # ruff: ignore[boolean-positional-value-in-call]
            return Some(prior)

        def stored() -> Outcome[object]:
            match self.prop, value:
                case bpy.types.FloatProperty() as real, _:
                    return _converted(value, float | tuple[float, ...], repr(real)).bind(partial(self.quantized, real)).map(lambda held: held if held.ndim else float(held))
                case bpy.types.IntProperty() as integer, _:
                    bounded = Annotated[int, self.meta]
                    return _converted(value, bounded | tuple[bounded, ...], repr(integer))
                case bpy.types.StringProperty(subtype="BYTE_STRING") as text, str(encoded):
                    return _converted(encoded, bytes, repr(text)).bind(partial(self.validated, text))
                case bpy.types.StringProperty() as text, str() | bytes() as content:
                    return self.validated(text, content)
                case _:
                    return Ok(value)

        def hosted(stored: object) -> Outcome[object]:
            match self.prop, stored:
                case bpy.types.EnumProperty(is_enum_flag=True), tuple(chosen):
                    return Ok(set(chosen))
                case bpy.types.PointerProperty(fixed_type=bpy.types.ID() as pointed), IdName() as identity:
                    return identity.resolve(type(pointed)).to_result_with(lambda: Fault.of(Reason.ABSENT, type(pointed).__name__, member=identity.name))  # type: ignore[unreachable]
                case _:
                    return Ok(stored)

        def placed(native: object) -> Outcome[None]:
            try:
                match self.prop, owner:
                    case bpy.types.BoolProperty(is_array=True) | bpy.types.IntProperty(is_array=True) | bpy.types.FloatProperty(is_array=True), bpy.types.bpy_struct():
                        array: bpy.types.bpy_prop_array = owner.path_resolve(name, False)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]  # ruff: ignore[boolean-positional-value-in-call]
                        if self.coordinates:
                            operator.setitem(reduce(operator.getitem, self.coordinates[:-1], array), self.coordinates[-1], native)  # type: ignore[call-overload, arg-type]  # ty: ignore[invalid-argument-type]
                        else:
                            array[:] = np.asarray(native).reshape(self.shape(owner)).tolist()  # type: ignore[index]  # ty: ignore[invalid-assignment]
                    case _:
                        setattr(owner, name, native)
                return Ok(None)
            except (AttributeError, TypeError, ValueError, ReferenceError) as error:
                return Error(Fault.of(Reason.REFUSED, _rna(self.path), error))

        return Ok(Nothing) if self.equal(held, value) else stored().bind(hosted).bind(placed).map(published)

    @staticmethod
    @np.errstate(over="ignore")
    def quantized(prop: bpy.types.FloatProperty, value: float | tuple[float, ...] | NDArray[np.float64]) -> Outcome[NDArray[np.float32]]:
        """Returns float32 storage, outside fault outside hard range."""
        return Ok(np.asarray(value, dtype=np.float32)).filter_with(
            lambda target: not np.any((target < prop.hard_min) | (target > prop.hard_max)),
            lambda _: Fault.of(Reason.OUTSIDE, repr(prop), str(value), member=f"values within hard range [{prop.hard_min}, {prop.hard_max}]"),
        )

    @staticmethod
    def validated[T: str | bytes](prop: bpy.types.StringProperty, value: T) -> Outcome[T]:
        """Returns storable value, outside fault for NUL or text past the host's length."""
        encoded = (os.fsencode(value) if prop.subtype in _PATHS else value.encode("utf-8")) if isinstance(value, str) else value
        text = Ok(value) if prop.subtype == "BYTE_STRING" else _converted(os.fsdecode(value) if prop.subtype in _PATHS else value, Annotated[str, msgspec.Meta(pattern=r"^[^\x00]*$")], repr(prop))
        return text.bind(lambda _: _converted(encoded, Annotated[bytes, msgspec.Meta(max_length=prop.length_max - 1)], repr(prop)) if prop.length_max else Ok(encoded)).map(lambda _: value)

    def shown(self, value: Value) -> str:
        """Returns enum label or display text of stored value."""
        match self.prop, value:
            case bpy.types.EnumProperty() as prop, str():
                try:
                    return prop.enum_items[value].name
                except KeyError:
                    return value
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


def _custom_read(struct: Struct, name: str) -> Result[object, TypeError | ReferenceError]:
    """Returns an existing custom value or None, preserving native lookup failures."""
    try:
        return Result[object, TypeError | ReferenceError].Ok(struct.get(name))
    except (TypeError, ReferenceError) as error:
        return Error(error)


def _kept(value: object) -> bool:
    """Whether a stored value holds a written leaf: a scalar, array, ID, or collection item, at any depth of its groups."""
    pending = [iter((value,))]
    while pending:
        for current in pending[-1]:
            match current:
                case bpy.types.PropertyGroup() | IDPropertyGroup():
                    pending.append(iter(current.values()))
                    break
                case None:
                    pass
                case list() if not current:
                    pass
                case _:
                    return True
        else:
            pending.pop()
    return False


def stored(struct: Struct, name: str) -> bool:
    """Whether a write left a value under a script pointer of struct.

    Host creates a script pointer's storage on first read, a drawn panel included, and empty groups and unset ID slots count as no write.

    Args:
        struct: Struct declaring the script pointer.
        name: Identifier of the script pointer.

    Returns:
        Whether the pointer's storage holds a written value.
    """
    return struct.is_property_set(name, ghost=False) and _kept(getattr(struct, name))


def _effective(struct: Native) -> bool:
    """Whether host reads struct values, false under a gated member its governor disables."""
    return all(
        getattr(parent, governor.identifier) == value
        for parent, child in pairwise((*struct.rna_ancestors(), struct) if isinstance(struct, bpy.types.bpy_struct) else ())
        for owner, member, governor, value in _GATED
        if isinstance(parent, owner) and isinstance(member, bpy.types.PointerProperty) and getattr(parent, member.identifier) == child
    )


def _differing[K: RnaPath | Descriptor](root: Native, members: Mapping[K, Value]) -> dict[K, Value]:
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
            for owner in _struct_at(root, descriptor.owner).to_list()
        )
    }


# --- [WRITES]


def _assign(owner: object, name: str, value: object) -> Result[None, AttributeError | TypeError | ValueError | ReferenceError]:
    """Assigns a native member, preserving its lookup, value, and lifetime failures."""
    try:
        setattr(owner, name, value)
        return Result[None, AttributeError | TypeError | ValueError | ReferenceError].Ok(None)
    except (AttributeError, TypeError, ValueError, ReferenceError) as error:
        return Error(error)


def written(struct: Native, path: RnaPath, value: Value | bytes | Struct) -> Outcome[Option[Value | Struct]]:
    """Writes member at path through `Descriptor.write`."""
    return Descriptor.located(struct, path).bind(lambda found: found[1].write(found[0], value))


def _sequenced[W](writes: Iterable[tuple[RnaPath, Native, Descriptor, W]]) -> tuple[tuple[RnaPath, Native, Descriptor, W], ...]:
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
                _LEADING.index(write[2].prop) if write[2].prop in _LEADING else len(_LEADING),
                write[0],
                _definition(write[1]).properties.find(write[2].prop.identifier),
                write[2].coordinates,
            ),
        )
    )


# --- [CONVERSIONS]


def _converted[T](value: object, kind: TypeForm[T], subject: str) -> Outcome[T]:
    """Returns value converted to kind, outside fault naming the refused constraint."""
    try:
        held: T = msgspec.convert(value, kind)  # ty: ignore[unsound-assignment]
    except msgspec.ValidationError as error:
        return Error(Fault.of(Reason.OUTSIDE, subject, error, member=repr(kind)))
    return Ok(held)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Descriptor", "Scalar", "Value", "stored", "written"]
