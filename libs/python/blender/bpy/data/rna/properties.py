"""`bpy.props` definitions and `EnumProperty` items copied from host properties, enum types, and module rows, and form registration with callbacks."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
import enum
import functools
from functools import partial, reduce
import inspect
import operator
import re
from types import FunctionType, MappingProxyType, MethodType
from typing import Any, Final, get_args, overload, TYPE_CHECKING, TypeAliasType, TypedDict
import zlib

import bpy
from expression import Nothing, Option, Result, try_downcast
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

type EnumItem = tuple[str, str, str, str, int]
type IconedItem = tuple[str, str, str, int, int]
type Changed = Callable[[Struct, bpy.types.Context], None]


class Callbacks[T: Struct = Struct](TypedDict, total=False):
    """Host callbacks of one property definition over owners of `T`, as `bpy.props` takes them, `items` also taking its static item rows."""

    items: tuple[EnumItem | IconedItem, ...] | Callable[[T, bpy.types.Context | None], Iterable[EnumItem | IconedItem]]
    get: Callable[[T], Any]
    set: Callable[[T, Any], None]
    poll: Callable[[T, Any], bool]
    update: Callable[[T, bpy.types.Context], None]


# --- [CONSTANTS] ------------------------------------------------------------------------

_EXACT: Final = (1 << 24) - 1
_NONE: Final[tuple[EnumItem]] = (("NONE", "None", "", "NONE", 0),)
_ARITIES: Final[Mapping[str, int]] = MappingProxyType({"items": 2, "get": 1, "set": 2, "poll": 2, "update": 2})
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


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Form group and nested groups under fresh definitions, named properties taking `callbacks` and value properties running `change` after update."""

    form: type[bpy.types.PropertyGroup]
    callbacks: Mapping[str, Callbacks] = MappingProxyType({})
    change: Option[Changed] = Nothing

    @contextmanager
    def acquisitions(self, _identity: Identity, /) -> Generator[Outcome[tuple[type[bpy.types.PropertyGroup], ...]]]:
        """Yields the nested groups, then the form, under their fresh definitions."""
        held = Result[tuple[type[bpy.types.PropertyGroup], ...], Faults].Ok((*nested(self.form), self.form))
        with declaring(self.form, self.callbacks, self.change):
            yield held


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DEFINITIONS]


def choices(rows: Iterable[bpy.types.EnumPropertyItem | tuple[str, str, str]]) -> tuple[EnumItem, ...]:
    """Returns `EnumProperty` items, each value fixed per identifier.

    Host items the owning property declares statically keep their value, config items and module rows take a value hashed from the identifier.
    Interface buttons compare enum values as float32, and hashed values stay within its exact integers for an expanded row to show its item.

    Args:
        rows: Host enum items or module `(identifier, name, description)` rows.

    Returns:
        Five-part items in row order.

    Raises:
        ValueError: Two identifiers hash to one value, host items sharing a static value kept.
    """

    def item(row: bpy.types.EnumPropertyItem | tuple[str, str, str]) -> EnumItem:
        match row:
            case bpy.types.EnumPropertyItem() as host:
                fixed = (
                    Block
                    .of_seq(reversed(host.rna_ancestors()))
                    .choose(lambda parent: Option.of_optional(try_downcast(bpy.types.EnumProperty, parent)))
                    .filter(lambda prop: host.identifier in prop.enum_items_static)
                )
                return (host.identifier, host.name, host.description, host.icon, host.value if fixed.try_head().is_some() else zlib.crc32(host.identifier.encode()) & _EXACT)
            case (identifier, name, description):
                return (identifier, name, description, "NONE", zlib.crc32(identifier.encode()) & _EXACT)

    items = tuple(map(item, rows))
    values = tuple(held[4] for held in items)
    if collided := tuple(held[0] for held in items if values.count(held[4]) > 1 and held[4] == zlib.crc32(held[0].encode()) & _EXACT):
        raise ValueError(collided)
    return items


@overload
def held_choices(rows: tuple[tuple[str, str, str], ...], /) -> tuple[EnumItem, ...]: ...
@overload
def held_choices[R: EnumItem | IconedItem](rows: tuple[R, ...], /) -> tuple[R, ...]: ...
@functools.cache
def held_choices(rows: tuple[tuple[str, str, str] | EnumItem | IconedItem, ...], /) -> tuple[EnumItem | IconedItem, ...]:
    """Returns the items of a dynamic enum's rows, cached by content to keep the strings Blender reads referenced.

    Args:
        rows: `(identifier, name, description)` rows taking the values `choices` fixes, or full items stating an icon name or `icon_value` and their value.

    Returns:
        Five-part items in row order, full items as given and three-part rows through one `choices` call.
    """
    named = tuple(row for row in rows if len(row) == 3)
    return choices(named) if named else tuple(row for row in rows if len(row) == 5)


def offered[R: EnumItem | IconedItem](rows: tuple[R, ...], /) -> tuple[R | EnumItem, *tuple[R | EnumItem, ...]]:
    """Returns a dynamic enum's rows, the host's `NONE` item at value 0 for no rows, every enum read naming an item and its first row a total read."""
    match rows:
        case (first, *rest):
            return (first, *rest)
        case _:
            return _NONE


def flagged[F: enum.Flag](kind: type[F], names: Iterable[str]) -> F:
    """Returns the flag an `ENUM_FLAG` property's identifier set holds, its members iterating in declaration order."""
    return reduce(operator.or_, (kind[name] for name in names), kind(0))


def titled(kind: type, /) -> str:
    """Returns a class name split into title-cased words, an acronym run kept whole (`CDL`, `HDRTone` as `HDR Tone`)."""
    return " ".join(word if word.isupper() else word.capitalize() for word in re.findall(r"[A-Z]+(?![a-z])|[A-Z]?[a-z]+|[0-9]+", kind.__name__))


def case_choices(union: TypeAliasType) -> tuple[EnumItem, ...]:
    """Returns `choices` over a tagged union alias's cases: tag as identifier, class name in `titled` words as name, docstring as description."""
    return choices((case.__name__, titled(case), inspect.getdoc(case) or "") for case in get_args(union.__value__))


def enum_choices(kind: type[enum.Enum], /) -> tuple[EnumItem, ...]:
    """Returns `EnumProperty` items of an `Enum` type, member name as identifier and name, a `Flag` bit as value, others the value `choices` fixes."""
    return (
        tuple((name, bpy.path.display_name(name.lower()), "", "NONE", member.value) for member in kind if (name := member.name) is not None)
        if issubclass(kind, enum.Flag)
        else choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in kind)
    )


def redeclared(prop: bpy.types.FloatProperty | bpy.types.IntProperty | bpy.types.BoolProperty | bpy.types.StringProperty | bpy.types.EnumProperty, /) -> bpy.props._PropertyDeferred:
    """Returns the `bpy.props` definition matching a host property's text, subtype, unit, bounds, step, precision, default, array shape, and items.

    Subtypes `bpy.props` refuses (array on scalars, number on arrays, string `TRANSLATION`) declare as `NONE`, the unit keeping its quantity.
    Dynamic arrays declare at their current shape.
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


def declared(definition: bpy.props._PropertyDeferred, callbacks: Mapping[str, object], /) -> bpy.props._PropertyDeferred:
    """Returns a fresh definition with each callback a plain function, other keywords as declared.

    Args:
        definition: Property definition the callbacks extend.
        callbacks: Callbacks per `bpy.props` keyword, static items and plain functions kept as given.

    Returns:
        Definition whose partials, bound methods, and callable objects call through a function of the positional arity the host checks.
    """

    def hosted(name: str, callback: object) -> object:
        parts: tuple[Callable[..., object], tuple[object, ...], Mapping[str, object]]
        match callback:
            case partial(func=func, args=args, keywords=keywords):
                parts = (func, args, keywords)
            case MethodType(__func__=func, __self__=receiver):
                parts = (func, (receiver,), {})
            case Callable() if not isinstance(callback, FunctionType):
                parts = (callback, (), {})
            case _:
                return callback
        call, bound, named = parts
        return (lambda owner: call(*bound, owner, **named)) if _ARITIES[name] == 1 else (lambda owner, value: call(*bound, owner, value, **named))

    fresh: bpy.props._PropertyDeferred = definition.function(**{**definition.keywords, **{name: hosted(name, callback) for name, callback in callbacks.items()}})
    return fresh


def rebound[T: Struct](form: type[T], /, **callbacks: Callbacks | Callbacks[T]) -> type[T]:
    """Returns a same-named subclass of form declaring each named property with its callbacks, the class one registration holds of a shared form.

    Args:
        form: Form class declaring each named property.
        **callbacks: Callbacks per property name, dynamic item strings kept referenced by their caller (`held_choices`).

    Returns:
        Form subclass the host registers in place of form.
    """
    held = annotated(form)
    derived: type[T] = type(form.__name__, (form,), {"__annotations__": {name: declared(held[name], bound) for name, bound in callbacks.items()}})  # ty: ignore[invalid-assignment]
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


def _updated(definition: bpy.props._PropertyDeferred, change: Changed) -> bpy.props._PropertyDeferred:
    """Returns a fresh definition of a value property running the change callback after its declared update, any other definition as declared."""
    valued = definition.function is not bpy.props.CollectionProperty and not issubclass(definition.keywords.get("type", bpy.types.ID), bpy.types.PropertyGroup)
    prior: Changed | None = definition.keywords.get("update")

    def changed(owner: Struct, context: bpy.types.Context) -> None:
        if prior is not None:
            prior(owner, context)
        change(owner, context)

    return declared(definition, Callbacks(update=changed)) if valued else definition


@contextmanager
def declaring(form: type[bpy.types.PropertyGroup], callbacks: Mapping[str, Callbacks], change: Option[Changed]) -> Generator[None]:
    """Declares the properties of a form, the groups it nests, and their bases again while the scope registers them.

    Properties `callbacks` names on the form or its bases take those callbacks, then every value property runs `change` after its declared update.
    Each declaring class holds the fresh definitions for the scope and its own definitions again at exit, no definition changing.

    Args:
        form: Form class the scope registers after the groups it nests.
        callbacks: Callbacks per property name of the form.
        change: Update callback every value property runs.

    Yields:
        None under the fresh definitions.
    """
    named = frozenset(form.__mro__)
    kinds = dict.fromkeys(kind for group in (*nested(form), form) for kind in reversed(group.__mro__))
    declarations: tuple[tuple[type, dict[str, object]], ...] = tuple((kind, vars(kind).get("__annotations__", {})) for kind in kinds)
    with ExitStack() as stack:
        for kind, table, name, definition in ((kind, held, name, value) for kind, held in declarations for name, value in tuple(held.items()) if isinstance(value, bpy.props._PropertyDeferred)):
            bound = declared(definition, callbacks[name]) if kind in named and name in callbacks else definition
            table[name] = change.map(partial(_updated, bound)).default_value(bound)
            stack.callback(table.__setitem__, name, definition)
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
            .bind(lambda _: catch(setattr, exception=ValueError)(owner, name, definition).map_error(partial(Fault.of, Reason.REFUSED, request)))
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
        Update callback while the deferral is open.
    """
    with identity.reports.deferred(write) as deferred:

        def changed(owner: Struct, _context: bpy.types.Context) -> None:
            for found in Option.of_optional(owner.id_data).bind(holder).to_list():
                if isinstance(found, kind):
                    deferred(Handle.of(found), identity)

        yield changed


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Callbacks",
    "Changed",
    "Declared",
    "EnumItem",
    "IconedItem",
    "annotated",
    "case_choices",
    "choices",
    "declared",
    "declaring",
    "enum_choices",
    "flagged",
    "held_choices",
    "nested",
    "offered",
    "pointed",
    "rebound",
    "reconciling",
    "redeclared",
    "titled",
]
