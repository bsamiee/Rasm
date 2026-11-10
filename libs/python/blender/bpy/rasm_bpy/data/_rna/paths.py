"""RNA path steps, the host text of a path, and the structs, definitions, subclasses, and context roots RNA reaches."""

import ast
from collections import deque
from collections.abc import Callable, Iterable, Iterator
from functools import partial
from itertools import islice
from typing import overload

import bpy
from expression import Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch

from rasm_records import Error, Fault, Nothing, Ok, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Struct = bpy.types.bpy_struct[object]
type Native = Struct | bpy.types.bpy_prop_collection[Struct]
type _Step = str | int | tuple[str]
type RnaPath = tuple[_Step, ...]
type NativeOwner = tuple[Native, RnaPath]

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PATHS]


def rna(path: RnaPath) -> str:
    """Returns host path text."""

    def token(step: _Step) -> str:
        match step:
            case str():
                return f".{step}"
            case int():
                return f"[{step:d}]"
            case (key,):
                return f'["{bpy.utils.escape_identifier(key)}"]'

    return "".join(map(token, path)).removeprefix(".")


def parsed_path(text: str) -> Outcome[RnaPath]:
    """Reads host path text (an override property's `rna_path`, a `path_from_id` result) as a path.

    Args:
        text: Host path text.

    Returns:
        Path over Python's own grammar, which the host's path grammar is a subset of, unreadable fault for text outside the host's path grammar.
    """
    refused, steps = partial(Fault.of, Reason.UNREADABLE, text), deque[_Step]()
    try:
        node = ast.parse(f"_{text}" if not text or text.startswith("[") else f"_.{text}", mode="eval").body
    except SyntaxError as error:
        return Error(refused(error))
    while True:
        match node:
            case ast.Name(id="_"):
                return Ok(tuple(steps))
            case ast.Attribute(value=inner, attr=name):
                steps.appendleft(name)
            case ast.Subscript(value=inner, slice=ast.Constant(value=index)) if type(index) is int:
                steps.appendleft(index)
            case ast.Subscript(value=inner, slice=ast.Constant(value=str(key))):
                steps.appendleft((key,))
            case _:
                return Error(refused())
        node = inner


def rna_definition(struct: Native | type[Struct]) -> bpy.types.Struct:
    """Returns the RNA definition of a live struct, a collection holding its own definition, or a struct type, `TypeError` for a collection declaring no members (`Scene.objects`)."""
    match getattr(struct, "bl_rna", None):
        case bpy.types.Struct() as held:
            return held
        case _:
            raise TypeError(struct)


def defined(struct: Native) -> Option[bpy.types.Struct]:
    """Returns the RNA definition a live struct or collection holds, Nothing for a collection declaring no members (`Scene.objects`)."""
    return catch(rna_definition, exception=TypeError)(struct).to_option()


def id_path(struct: Struct) -> Option[RnaPath]:
    """Returns path from ID to struct, empty for ID itself.

    Structs whose type builds no RNA path (`CurveMapping`, `Stereo3dFormat`) take their path through the ancestor they were reached from.
    """
    match struct:
        case bpy.types.ID():
            return Some(())
        case _:
            return (
                catch(struct.path_from_id, exception=ValueError)()
                .to_option()
                .bind(lambda text: parsed_path(text).to_option())
                .or_else_with(lambda: Block.of_seq(reversed(struct.rna_ancestors())).try_head().bind(partial(_held, struct)))
            )


def id_data[K: bpy.types.ID](kind: type[K], struct: Struct, /) -> Option[K]:
    """Returns the ID holding a struct as kind, the struct itself for an ID of kind, Nothing for a struct an ID of another kind holds."""
    return Option.of_optional(try_downcast(kind, struct.id_data))


def context_path(owner: Struct, name: str | None = None, /) -> Option[str]:
    """Returns the context data path of an owner, or of its member under name, through `blend_data` or preferences, Nothing for other data or a partial host path."""
    return (
        catch(owner.path_from_module, exception=ValueError)(*Option.of_optional(name).to_list())
        .to_option()
        .filter(lambda path: " ... " not in path and path.startswith(("bpy.data.", "bpy.context.")))
        .map(lambda path: f"blend_data.{path.removeprefix('bpy.data.')}" if path.startswith("bpy.data.") else path.removeprefix("bpy.context."))
    )


@overload
def struct_at(root: Native, path: RnaPath) -> Option[Native]: ...
@overload
def struct_at[K](root: Native, path: RnaPath, kind: type[K]) -> Option[K]: ...
def struct_at(root: Native, path: RnaPath, kind: type[object] = object) -> Option[object]:
    """Returns the struct or collection at relative path, narrowed to kind when given, Nothing for a value, a removed struct, or another kind.

    Collection root resolves through its holder and the collection member holding it.
    """
    if not path:
        return Option.of_optional(try_downcast(kind, root))
    starts = (
        islice(
            (
                (holder, (prop.identifier,))
                for holder in Option.of_optional(root.data).to_list()
                for prop in rna_definition(holder).properties
                if isinstance(prop, bpy.types.CollectionProperty) and getattr(holder, prop.identifier) == root
            ),
            1,
        )
        if isinstance(root, bpy.types.bpy_prop_collection)
        else ((root, ()),)
    )
    for holder, prefix in starts:
        try:
            found = holder.path_resolve(rna((*prefix, *path)), False)  # ruff: ignore[boolean-positional-value-in-call]
        except (ValueError, ReferenceError):
            return Nothing
        return Option.of_optional(try_downcast(kind, found)).filter(lambda value: isinstance(value, bpy.types.bpy_struct | bpy.types.bpy_prop_collection))
    return Nothing


def definition_at(definition: bpy.types.Struct, steps: RnaPath) -> Option[bpy.types.Struct]:
    """Returns the definition a path reaches through pointers, collection elements, and a collection's own definition, Nothing past a value."""
    held, remaining = definition, deque(steps)
    while remaining:
        name = remaining.popleft()
        match held.properties[name] if isinstance(name, str) and name in held.properties else None, remaining[0] if remaining else None:
            case bpy.types.CollectionProperty(fixed_type=target), int() | tuple():
                remaining.popleft()
            case ((bpy.types.CollectionProperty(srna=target) | bpy.types.PointerProperty(fixed_type=target)), _):
                pass
            case _:
                return Nothing
        if target is None:
            return Nothing
        held = target
    return Some(held)


# --- [CONTEXT]


def subclass[T: Struct](base: type[T], identifier: str) -> Outcome[type[T]]:
    """Returns the `base` subclass `bpy.types` names by identifier, building a native class on first read."""
    match getattr(bpy.types, identifier, None):
        case type() as kind if issubclass(kind, base) and kind is not base:
            return Ok(kind)
        case _:
            return Error(Fault.of(Reason.ABSENT, f"bpy.types.{base.__name__}", member=identifier))


def addon_preferences(preferences: bpy.types.Preferences, package: str) -> Option[bpy.types.AddonPreferences]:
    """Returns the preferences of the enabled add-on or extension package, Nothing when disabled or holding none."""
    try:
        return Option.of_optional(preferences.addons[package].preferences)
    except KeyError:
        return Nothing


def preferred[T](preferences: bpy.types.Preferences, package: str, write: Callable[[bpy.types.AddonPreferences], Outcome[T]]) -> Outcome[T]:
    """Writes the package's add-on preferences and marks them dirty on success, a mark the host skips, absent fault for a package holding none."""
    answer = addon_preferences(preferences, package).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(preferences), member=package)).bind(write)
    preferences.is_dirty = preferences.is_dirty or answer.is_ok()
    return answer


def root_in[K: Native](root: type[K], context: bpy.types.Context) -> Option[K]:
    """Returns the root of type the context holds: the context, add-on preferences by `bl_idname`, else the first `Context` pointer typed to hold it."""
    candidates: Iterable[object]
    if root is bpy.types.Context:
        candidates = (context,)
    elif issubclass(root, bpy.types.AddonPreferences):
        candidates = root_in(bpy.types.Preferences, context).bind(lambda preferences: addon_preferences(preferences, root.bl_idname)).to_list()
    else:
        candidates = (getattr(context, member) for member in _members(root))
    return Block.of_seq(candidates).choose(lambda value: Option.of_optional(try_downcast(root, value))).try_head()


def rooted[K: Native](root: type[K], context: bpy.types.Context) -> Outcome[K]:
    """Returns context-held root, absent fault naming first matching member."""
    return root_in(root, context).to_result_with(lambda: Fault.of(Reason.ABSENT, "bpy.context", member=Block.of_seq(_members(root)).try_head().default_value(root.__name__)))


# --- [READS]


def _held(struct: Struct, parent: Struct) -> Option[RnaPath]:
    """Returns path of the pointer member or collection element of parent holding struct, the first holding member read alone."""
    steps: Iterator[RnaPath] = (
        (prop.identifier,) if isinstance(prop, bpy.types.PointerProperty) else (prop.identifier, index)
        for prop in rna_definition(parent).properties
        if isinstance(prop, bpy.types.PointerProperty | bpy.types.CollectionProperty) and isinstance(struct, type(prop.fixed_type))
        for index, item in (((0, getattr(parent, prop.identifier)),) if isinstance(prop, bpy.types.PointerProperty) else enumerate(getattr(parent, prop.identifier)))
        if item == struct
    )
    return Option.of_optional(next(steps, None)).bind(lambda found: id_path(parent).map(lambda base: (*base, *found)))


def _members[K: Native](root: type[K]) -> tuple[str, ...]:
    """Returns context pointers able to hold root type."""
    return tuple(prop.identifier for prop in bpy.types.Context.bl_rna.properties if isinstance(prop, bpy.types.PointerProperty) and issubclass(root, type(prop.fixed_type)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Native",
    "NativeOwner",
    "RnaPath",
    "Struct",
    "addon_preferences",
    "context_path",
    "defined",
    "definition_at",
    "id_data",
    "id_path",
    "parsed_path",
    "preferred",
    "rna",
    "rna_definition",
    "root_in",
    "rooted",
    "struct_at",
    "subclass",
]
