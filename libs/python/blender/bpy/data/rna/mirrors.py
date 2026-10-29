"""`bpy.props` definitions and `EnumProperty` items mirroring host properties, enum types, and module rows, and the form declarations groups register with."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
import enum
import functools
from functools import partial, reduce
import inspect
import operator
import re
from types import MappingProxyType
from typing import Any, Final, get_args, TYPE_CHECKING, TypeAliasType
import zlib

import bpy
from expression import Ok, Option, Result, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np

from blender.bpy.data.ids.store import Handle, holder
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import Struct
from blender.bpy.extension.identity import Identity
from blender.records.faults import Fault, Faults, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import PropertySubtypeStringItems

# --- [TYPES] ----------------------------------------------------------------------------

type _Item = tuple[str, str, str, str, int]
type Changed = Callable[[Struct, bpy.types.Context], None]

# --- [CONSTANTS] ------------------------------------------------------------------------

_REGISTER: Final[Callable[[type[Struct], str, bpy.props._PropertyDeferred], Result[None, ValueError]]] = catch(setattr, exception=ValueError)
_ARRAYED: Final = frozenset({
    "COLOR",
    "TRANSLATION",
    "DIRECTION",
    "VELOCITY",
    "ACCELERATION",
    "MATRIX",
    "EULER",
    "QUATERNION",
    "AXISANGLE",
    "XYZ",
    "XYZ_LENGTH",
    "COLOR_GAMMA",
    "COORDINATES",
    "LAYER",
    "LAYER_MEMBER",
})
_TEXTUAL: Final = frozenset({"FILE_PATH", "DIR_PATH", "FILE_NAME", "BYTE_STRING", "PASSWORD"})
_NUMBERS: Final[Mapping[tuple[type[bpy.types.Property], bool], Callable[..., bpy.props._PropertyDeferred]]] = MappingProxyType({
    (bpy.types.FloatProperty, False): bpy.props.FloatProperty,
    (bpy.types.FloatProperty, True): bpy.props.FloatVectorProperty,
    (bpy.types.IntProperty, False): bpy.props.IntProperty,
    (bpy.types.IntProperty, True): bpy.props.IntVectorProperty,
    (bpy.types.BoolProperty, False): bpy.props.BoolProperty,
    (bpy.types.BoolProperty, True): bpy.props.BoolVectorProperty,
})

# --- [MODELS] ---------------------------------------------------------------------------


class Updated(msgspec.Struct, frozen=True, kw_only=True):
    """Form classes registered with a change callback after each value property's declared update, held through `updating` while registered."""

    groups: tuple[type[Struct], ...]
    change: Changed

    @contextmanager
    def acquisitions(self, _identity: Identity, /) -> Generator[Outcome[tuple[type[Struct], ...]]]:
        """Yields the classes while their fresh definitions stand."""
        held: Outcome[tuple[type[Struct], ...]] = Ok(self.groups)
        with updating(self.groups, self.change):
            yield held


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MIRRORS]


def choices(rows: Iterable[bpy.types.EnumPropertyItem | tuple[str, str, str]]) -> tuple[_Item, ...]:
    """Returns `EnumProperty` items, each value fixed per identifier.

    Host items the owning property declares statically keep their value. Config items and module rows take a value hashed from the identifier, which stays fixed when rows move or the config changes.

    Args:
        rows: Host enum items or module `(identifier, name, description)` rows.

    Returns:
        Five-part items in row order.

    Raises:
        ValueError: Two identifiers hash to one value, host items sharing a static value kept.
    """

    def item(row: bpy.types.EnumPropertyItem | tuple[str, str, str]) -> _Item:
        match row:
            case bpy.types.EnumPropertyItem() as host:
                fixed = (
                    Block
                    .of_seq(reversed(host.rna_ancestors()))
                    .choose(lambda parent: Option.of_optional(try_downcast(bpy.types.EnumProperty, parent)))
                    .filter(lambda prop: host.identifier in prop.enum_items_static)
                )
                return (host.identifier, host.name, host.description, host.icon, host.value if fixed.try_head().is_some() else zlib.crc32(host.identifier.encode()) & 0x7FFFFFFF)
            case (identifier, name, description):
                return (identifier, name, description, "NONE", zlib.crc32(identifier.encode()) & 0x7FFFFFFF)

    items = tuple(map(item, rows))
    values = tuple(held[4] for held in items)
    if collided := tuple(held[0] for held in items if values.count(held[4]) > 1 and held[4] == zlib.crc32(held[0].encode()) & 0x7FFFFFFF):
        raise ValueError(collided)
    return items


@functools.cache
def held_choices(rows: tuple[tuple[str, str, str], ...], /) -> tuple[_Item, ...]:
    """Returns `choices` of a dynamic enum's rows, cached by content so the strings Blender reads stay referenced while the rows stand."""
    return choices(rows)


def flagged[F: enum.Flag](kind: type[F], names: Iterable[str]) -> F:
    """Returns the flag an `ENUM_FLAG` property's identifier set holds, its members iterating in declaration order."""
    return reduce(operator.or_, (kind[name] for name in names), kind(0))


def case_choices(union: TypeAliasType) -> tuple[_Item, ...]:
    """Returns `choices` over a tagged union alias's cases: the case tag as identifier, its class name split into words as name, its docstring as description."""
    return choices((case.__name__, bpy.path.display_name(re.sub(r"(?<=[a-z0-9])(?=[A-Z])", "_", case.__name__)), inspect.getdoc(case) or "") for case in get_args(union.__value__))


def enum_choices(kind: type[enum.Enum], /) -> tuple[_Item, ...]:
    """Returns `EnumProperty` items of an `Enum` type, each member's name as identifier and split into words as name, a `Flag` member's bit as its `ENUM_FLAG` value, any other member the value `choices` fixes per identifier."""
    return (
        tuple((name, bpy.path.display_name(name.lower()), "", "NONE", member.value) for member in kind if (name := member.name) is not None)
        if issubclass(kind, enum.Flag)
        else choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in kind)
    )


def mirrored(prop: bpy.types.FloatProperty | bpy.types.IntProperty | bpy.types.BoolProperty | bpy.types.StringProperty | bpy.types.EnumProperty, /) -> bpy.props._PropertyDeferred:
    """Returns the `bpy.props` definition declaring a host property as the host declares it: name, description, translation context, subtype, unit, bounds, step, precision, default, array shape, and items.

    `bpy.props` takes array subtypes on arrays alone, number subtypes on scalars alone, and no `TRANSLATION` string subtype, while RNA declares each on any member, so a subtype the factory refuses declares as `NONE`, the unit keeping its quantity. A dynamic array declares at its current shape.
    """
    shown: dict[str, Any] = {"name": prop.name, "description": prop.description, "translation_context": prop.translation_context}
    match prop:
        case bpy.types.FloatProperty() | bpy.types.IntProperty() | bpy.types.BoolProperty():
            shape, arrayed = Descriptor.dimensions(prop), prop.subtype in _ARRAYED
            return _NUMBERS[type(prop), bool(shape)](
                **shown,
                **(
                    {"size": shape, "default": np.resize(np.asarray(prop.default_array), shape).tolist(), "subtype": prop.subtype if arrayed else "NONE"}
                    if shape
                    else {"default": prop.default, "subtype": "NONE" if arrayed else prop.subtype}
                ),
                **({} if isinstance(prop, bpy.types.BoolProperty) else {"min": prop.hard_min, "max": prop.hard_max, "soft_min": prop.soft_min, "soft_max": prop.soft_max, "step": prop.step}),
                **({"unit": prop.unit, "precision": prop.precision} if isinstance(prop, bpy.types.FloatProperty) else {}),
            )
        case bpy.types.StringProperty():
            text: PropertySubtypeStringItems = prop.subtype if prop.subtype in _TEXTUAL else "NONE"  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return bpy.props.StringProperty(**shown, subtype=text, default=prop.default, maxlen=prop.length_max)
        case bpy.types.EnumProperty(is_enum_flag=True):
            return bpy.props.EnumProperty(**shown, items=choices(prop.enum_items), default=set(prop.default_flag), options={"ENUM_FLAG"})
        case bpy.types.EnumProperty():
            return bpy.props.EnumProperty(**shown, items=choices(prop.enum_items), default=prop.default)


# --- [DECLARATIONS]


def annotated(form: type[Struct]) -> dict[str, bpy.props._PropertyDeferred]:
    """Property definitions a form class and its bases declare, bases first, as Blender registers them."""
    return {name: definition for kind in reversed(form.__mro__) for name, definition in vars(kind).get("__annotations__", {}).items() if isinstance(definition, bpy.props._PropertyDeferred)}


def itemized[T: Struct](form: type[T], /, **items: tuple[_Item, ...] | Callable[[Struct, bpy.types.Context | None], Iterable[_Item]]) -> type[T]:
    """Returns a subclass of form under its name declaring each named enum property again with the given items, every other keyword as form declares it.

    Args:
        form: Form class declaring each named property, its items empty.
        **items: Static items, or a dynamic items callback whose strings its caller keeps referenced (`held_choices`), per property name.

    Returns:
        Form subclass the host registers in place of form.
    """
    declared = annotated(form)
    derived: type[T] = type(form.__name__, (form,), {"__annotations__": {name: declared[name].function(**{**declared[name].keywords, "items": rows}) for name, rows in items.items()}})  # ty: ignore[invalid-assignment]
    return derived


def nested(form: type[bpy.types.PropertyGroup]) -> tuple[type[bpy.types.PropertyGroup], ...]:
    """Property group classes a form's pointer and collection properties hold, children first."""
    return tuple(
        child
        for definition in annotated(form).values()
        if definition.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}
        for target in (definition.keywords["type"],)
        if issubclass(target, bpy.types.PropertyGroup)
        for child in (*nested(target), target)
    )


def updated(definition: bpy.props._PropertyDeferred, change: Changed) -> bpy.props._PropertyDeferred:
    """Returns a fresh definition of a value property running the change callback after its declared update, any other definition as declared."""
    valued = definition.function is not bpy.props.CollectionProperty and not issubclass(definition.keywords.get("type", bpy.types.ID), bpy.types.PropertyGroup)
    declared: Changed | None = definition.keywords.get("update")

    def changed(owner: Struct, context: bpy.types.Context) -> None:
        if declared is not None:
            declared(owner, context)
        change(owner, context)

    return definition.function(**{**definition.keywords, "update": changed}) if valued else definition


@contextmanager
def updating(groups: tuple[type[Struct], ...], change: Changed) -> Generator[None]:
    """Declares every value property of the groups and their bases through `updated` while the scope registers them.

    Each declaring class holds the fresh definitions for the scope and its own definitions again at exit, no definition changing.

    Args:
        groups: Form classes the scope registers.
        change: Update callback every value property runs.

    Yields:
        None while the fresh definitions stand.
    """
    with ExitStack() as stack:
        for kind in dict.fromkeys(kind for group in groups for kind in reversed(group.__mro__)):
            declared: dict[str, object] = vars(kind).get("__annotations__", {})
            for name, definition in tuple(declared.items()):
                if isinstance(definition, bpy.props._PropertyDeferred):
                    declared[name] = updated(definition, change)
                    stack.callback(declared.__setitem__, name, definition)
        yield


@contextmanager
def pointed(owner: type[Struct], name: str, definition: bpy.props._PropertyDeferred) -> Generator[Outcome[None]]:
    """Registers property under a free name for scope duration, the host refusing a name past its byte limit."""
    request = f"{owner.__name__}.{name}"
    rna: bpy.types.Struct = owner.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    with ExitStack() as stack:
        yield (
            Result[None, Faults](ok=None)
            .filter_with(lambda _: not (name in rna.properties or name in rna.functions or hasattr(owner, name)), lambda _: Fault.of(Reason.TAKEN, request, member=owner.__name__))
            .bind(lambda _: _REGISTER(owner, name, definition).map_error(partial(Fault.of, Reason.REFUSED, request)))
            .map(lambda _: stack.callback(delattr, owner, name))
            .map(lambda _: None)
        )


@contextmanager
def reconciling[T: bpy.types.ID](kind: type[T], write: Callable[[Handle[T], Identity], Outcome[Option[str]]], identity: Identity) -> Generator[Changed]:
    """Yields the update callback deferring one reconcile per changed holder of kind, an attachment binding kind and write through `partial`.

    Args:
        kind: Holder class write resolves.
        write: Deferred write reconciling holder.
        identity: Extension whose reports defer the write.

    Yields:
        Update callback while the deferral stands.
    """
    with identity.reports.deferred(write) as deferred:

        def changed(owner: Struct, _context: bpy.types.Context) -> None:
            for found in Option.of_optional(owner.id_data).bind(holder).to_list():
                if isinstance(found, kind):
                    deferred(Handle.of(found), identity)

        yield changed


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Changed", "Updated", "annotated", "case_choices", "choices", "enum_choices", "flagged", "held_choices", "itemized", "mirrored", "nested", "pointed", "reconciling", "updated", "updating"]
