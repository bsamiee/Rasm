"""Runs each harness session a scratch folder receives, to its end in a background Blender and one step per timer pass in a windowed one."""

from array import array
import ast
import base64
import builtins
from collections.abc import Callable, Generator, Iterable, Iterator, Mapping
from contextlib import ExitStack
from dataclasses import dataclass
from functools import cache, partial, reduce
import glob
from importlib.machinery import EXTENSION_SUFFIXES
from itertools import starmap
import json
import math
import os
from pathlib import Path
import re
import sys
import time
import traceback
from types import CodeType, FunctionType, ModuleType
from typing import IO, Literal, NotRequired, Protocol, runtime_checkable, TypedDict
import warnings

import addon_utils
import bpy
import gpu
import mathutils

# --- [TYPES] ----------------------------------------------------------------------------

type Json = bool | int | float | str | list[Json] | dict[str, Json] | None
type Namespace = dict[str, object]
type Struct = bpy.types.bpy_struct[object]
type Collection = bpy.types.bpy_prop_collection[Struct]
type Timers = list[tuple[Callable[[], float | None], float]]
type Updates = list[tuple[str, frozenset[str]]]
type Steps = Generator[float]
type Interface = type[bpy.types.Header | bpy.types.Menu | bpy.types.Panel]


@runtime_checkable
class Extended(Protocol):
    """Draw function `append` or `prepend` put on an interface class, calling the functions it holds in order."""

    _draw_funcs: list[Callable[[bpy.types.Context], None]]

    def __call__(self, owner: object, context: bpy.types.Context, /) -> None:
        """Draws the interface through each held function."""
        ...


class Callee(Protocol):
    """Original function or operator a call case runs."""

    def __call__(self, *args: object, **keywords: object) -> object:
        """Runs the original on decoded arguments."""
        ...


class Items(Protocol):
    """Preferences collection a case adds items to and the engine removes them from."""

    def keys(self) -> list[str]:
        """Returns each item's key, empty for items with no name."""
        ...

    def __iter__(self) -> Iterator[Struct]:
        """Iterates the items in order."""
        ...

    def __len__(self) -> int:
        """Returns the item count."""
        ...

    def remove(self, item: Struct, /) -> None:
        """Removes one item."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

SPECIAL: Mapping[str, float] = {"NaN": math.nan, "Infinity": math.inf, "-Infinity": -math.inf, "-0": -0.0}
TAGS: Mapping[str, str] = {repr(special): tag for tag, special in SPECIAL.items()}
SAFE = 2**sys.float_info.mant_dig - 1
REPOSITORY = "harness"
MANIFEST: str = addon_utils._ext_manifest_filename_toml  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
HOST = "host.json"
STDOUT = "stdout.txt"
STDERR = "stderr.txt"
PASS = 0.0
LOGGED = re.compile(r"^\S+\s+(?P<category>\S+)\s+\| (?P<message>.*)$")
COLUMNS: Mapping[str, str] = {"FLOAT": "d", "INT": "i", "BOOLEAN": "b"}
UPDATED = "is_updated_"
FLAGS = tuple(prop.identifier.removeprefix(UPDATED) for prop in bpy.types.DepsgraphUpdate.bl_rna.properties if prop.identifier.startswith(UPDATED))

# --- [MODELS] ---------------------------------------------------------------------------


class Document(TypedDict, total=False):
    """Case document: the case folder copy `open` names with its `wm.open_mainfile` options, else an empty file in `working_space`, then scene members."""

    open: str
    options: dict[str, Json]
    working_space: str
    scene: dict[str, Json]


class Area(TypedDict):
    """Area type the case overrides and the region type inside it."""

    type: str
    region: NotRequired[str]


class Added(TypedDict):
    """ID a `bpy.data` collection creates, bound in `made` under its row key."""

    kind: str
    properties: NotRequired[dict[str, Json]]


class Precondition(TypedDict, total=False):
    """Precondition rows `contract.ts` decodes, applied in field order."""

    extensions: list[str]
    document: Literal["none"] | Document
    language: str
    window: dict[str, str]
    area: Area
    add: dict[str, Added]
    set: list[tuple[str, Json]]
    select: list[str]
    mode: str
    frame: int
    steps: list[str]
    edited: bool


class Source(TypedDict):
    """Script path a case names and the source the harness prepared from it."""

    script: str
    code: str


class Shared(TypedDict):
    """Fields every host case `contract.ts` encodes."""

    name: str
    precondition: Precondition
    read: dict[str, str]
    folder: str
    confirms: NotRequired[list[bool]]
    files: NotRequired[list[str | None]]
    keywords: NotRequired[dict[str, Json]]


class Probe(Shared):
    """Case evaluating one expression."""

    _tag: Literal["Probe"]
    evaluate: str


class Call(Shared):
    """Case calling an original on decoded arguments, the source's names in scope."""

    _tag: Literal["Call"]
    call: str
    args: list[Json]
    source: NotRequired[Source]


class Run(Shared):
    """Case running a source whole."""

    _tag: Literal["Run"]
    source: Source


type Case = Probe | Call | Run


class Session(TypedDict):
    """Session file `host.ts` writes: the records and reply files, whether the reply carries the survey, and the cases."""

    records: str
    reply: str
    survey: bool
    cases: list[Case]


class Override(TypedDict):
    """Context members the case runs under, resolved once per case."""

    window: bpy.types.Window
    area: NotRequired[bpy.types.Area]
    region: NotRequired[bpy.types.Region]


@dataclass(frozen=True, slots=True, kw_only=True)
class Instance:
    """State of this Blender process across sessions: the scratch folder, the log file, the batch extension repository, the owner's user resources, and depsgraph updates."""

    scratch: Path
    log: Path
    repository: Path
    owner: Path
    updates: Updates


@dataclass(frozen=True, slots=True, kw_only=True)
class Held:
    """Registrations and settings read before a case, which the engine restores after it."""

    handlers: Mapping[str, tuple[object, ...]]
    classes: frozenset[str]
    keymaps: Mapping[tuple[str, str, str], frozenset[int]]
    draws: Mapping[Interface, tuple[object, ...]]
    path: tuple[str, ...]
    modules: frozenset[str]
    preferences: Mapping[str, object]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [HOST]


def preferences() -> bpy.types.Preferences:
    """Returns the preferences the context holds."""
    held: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return held


def manager() -> bpy.types.WindowManager:
    """Returns the window manager the context holds, which every file load replaces."""
    held: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return held


@cache
def jobs() -> tuple[str, ...]:
    """Returns the job types `bpy.app.is_job_running` accepts, read from its refusal of an empty type."""
    try:
        bpy.app.is_job_running("")  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    except (TypeError, ValueError) as error:
        kinds: tuple[str, ...] = ast.literal_eval(str(error).rpartition(" not found in ")[2])
        return kinds
    return ()


def owned() -> Path:
    """Returns the user resource folder Blender resolves without `BLENDER_USER_RESOURCES`, where the owner's extensions sit."""
    held = os.environ.pop("BLENDER_USER_RESOURCES")
    try:
        return Path(bpy.utils.resource_path("USER"))
    finally:
        os.environ["BLENDER_USER_RESOURCES"] = held


def host() -> dict[str, Json]:
    """Returns the host state beside the version that decides answers."""
    options = bpy.app.build_options
    ocio: tuple[int, ...] = bpy.app.ocio.version  # type: ignore[attr-defined]
    return {
        "gpu": {"backend": gpu.platform.backend_type_get(), "vendor": gpu.platform.vendor_get(), "renderer": gpu.platform.renderer_get(), "version": gpu.platform.version_get()},
        "ocio": list(ocio),
        "build": [name for name in sorted(dir(options)) if getattr(options, name) is True],
        "locale": bpy.app.translations.locale,
        "python": sys.version,
        "flags": dict(zip(sys.flags.__match_args__, sys.flags, strict=True)),
        "background": bpy.app.background,
    }


def items(owner: type[Struct], member: str, /) -> list[Json]:
    """Returns the identifiers an enum member declares."""
    rna: bpy.types.EnumProperty = owner.bl_rna.properties[member]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return [item.identifier for item in rna.enum_items]


# --- [VALUES]


def buried(value: object, /) -> Json:
    """Returns the JSON form of a host value, its own struct walked through RNA members, IDs and RNA types referenced, a repeated struct referencing its first place."""
    seen: dict[tuple[int, type], str] = {}

    def walked(item: object, at: str, /, *, top: bool = False) -> Json:
        match item:
            case bool() | str() | None:
                return item
            case int() if abs(item) <= SAFE:
                return item
            case int():
                return {"$": f"int:{item}"}
            case float():
                return exact(item)
            case bytes():
                return {"$": f"bytes:{base64.b64encode(item).decode()}"}
            case bpy.types.Struct() if not top:
                return {"$": f"type:{item.identifier}"}
            case bpy.types.ID() if not top:
                return {"$": located(item, f"repr:{item!r}", "path:")}
            case bpy.types.bpy_struct() if (item.as_pointer(), type(item)) in seen:
                return {"$": located(item, f"seen:{seen[item.as_pointer(), type(item)]}", "path:")}
            case bpy.types.bpy_struct():
                seen[item.as_pointer(), type(item)] = at
                return {prop.identifier: member(item, prop.identifier, f"{at}.{prop.identifier}") for prop in item.bl_rna.properties if prop.identifier != "rna_type"}
            case bpy.types.bpy_prop_collection() if (keys := item.keys()) and len(keys) == len(item) == len(set(keys)):
                return dict(zip(keys, rows(item, [f"{at}.{key}" for key in keys]), strict=True))
            case bpy.types.bpy_prop_collection():
                return rows(item, [f"{at}[{index}]" for index in range(len(item))])
            case Mapping():
                return {str(key): walked(entry, f"{at}.{key}") for key, entry in item.items()}
            case set() | frozenset():
                return [walked(entry, f"{at}[{index}]") for index, entry in enumerate(sorted(item, key=repr))]
            case mathutils.Vector() | mathutils.Color() | mathutils.Euler() | mathutils.Quaternion() | mathutils.Matrix():
                return walked(item[:], at)
            case Iterable():
                return [walked(entry, f"{at}[{index}]") for index, entry in enumerate(item)]
            case _:
                return {"$": f"repr:{item!r}"}

    def member(owner: Struct, name: str, at: str, /) -> Json:
        try:
            return walked(getattr(owner, name), at)
        except Exception as error:
            return {"$": f"unreadable:{error}"}

    def rows(collection: Collection, labels: list[str], /) -> list[Json]:
        match tuple({type(entry) for entry in collection}):
            case (kind,) if issubclass(kind, bpy.types.bpy_struct) and not issubclass(kind, bpy.types.ID) and not any((entry.as_pointer(), kind) in seen for entry in collection):
                seen.update({(entry.as_pointer(), kind): label for entry, label in zip(collection, labels, strict=True)})
                values = {prop.identifier: column(collection, prop, labels) for prop in kind.bl_rna.properties if prop.identifier != "rna_type"}
                return [{name: held[index] for name, held in values.items()} for index in range(len(labels))]
            case _:
                return list(starmap(walked, zip(collection, labels, strict=True)))

    def column(collection: Collection, prop: bpy.types.Property, labels: list[str], /) -> list[Json]:
        dimensions: list[int] = [size for size in prop.array_dimensions if size] if prop.type in COLUMNS else [0, 0]  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        width = math.prod(dimensions)
        match gathered(collection, prop, len(labels) * width) if len(dimensions) <= 1 else None:
            case array() as buffer:
                cast: Callable[[float], Json] = {"FLOAT": exact, "BOOLEAN": bool}.get(prop.type, int)
                read = list(map(cast, buffer))
                return [read[index * width : (index + 1) * width] for index in range(len(labels))] if dimensions else read
            case _:
                return [member(entry, prop.identifier, f"{label}.{prop.identifier}") for entry, label in zip(collection, labels, strict=True)]

    return walked(value, "", top=True)


def gathered(collection: Collection, prop: bpy.types.Property, size: int, /) -> array[int] | array[float] | None:
    """Returns one scalar or vector RNA member of every collection item through one `foreach_get`, flattened, and nothing for a member raw access refuses (a dynamic array)."""
    buffer = array(COLUMNS[prop.type], [0]) * size
    try:
        collection.foreach_get(prop.identifier, buffer)
    except (RuntimeError, TypeError):
        return None
    return buffer


def exact(value: float, /) -> Json:
    """Returns a double as JSON, `NaN`, `±Infinity`, and `-0` as their `{"$": ...}` forms."""
    return value if math.isfinite(value) and (value != 0 or math.copysign(1.0, value) > 0) else {"$": TAGS[repr(value)]}


def located(struct: Struct, unplaced: str, tag: str = "", /) -> str:
    """Returns the reference to an ID or repeated struct: its full data path after `tag`, else `unplaced` for one outside every module path (an evaluated ID, a struct the walk wrote earlier)."""
    try:
        return f"{tag}{struct.path_from_module()}"
    except ValueError:
        return unplaced


def decoded(tag: str, made: Namespace, /) -> object:
    """Returns the host value one `{"$": <tag>}` form names."""
    match tag.partition(":"):
        case (special, "", "") if special in SPECIAL:
            return SPECIAL[special]
        case ("int", _, digits):
            return int(digits)
        case ("bytes", _, text):
            return base64.b64decode(text)
        case ("path", _, path):
            return evaluated(path, {"bpy": bpy})
        case ("made", _, key):
            return made[key]
        case _:
            raise ValueError(f"{tag} names no host value")


def unburied(value: Json, made: Namespace, /) -> object:
    """Returns the host value a JSON form names."""
    match value:
        case {"$": str(tag), **rest} if not rest:
            return decoded(tag, made)
        case list():
            return [unburied(item, made) for item in value]
        case dict():
            return {key: unburied(item, made) for key, item in value.items()}
        case _:
            return value


def written(owner: Struct, record: Mapping[str, Json], made: Namespace, /) -> None:
    """Writes a record onto RNA members in RNA order, a nested record into a pointer member, so the host checks every key and enum."""
    members = owner.bl_rna.properties
    if unknown := sorted(record.keys() - members.keys()):
        raise AttributeError(f"{type(owner).__name__} has no members {unknown}")
    for prop in members:
        match record.get(prop.identifier):
            case None:
                pass
            case dict(nested) if prop.type == "POINTER" and "$" not in nested:
                written(getattr(owner, prop.identifier), nested, made)
            case value:
                setattr(owner, prop.identifier, unburied(value, made))


def translated(language: str, /) -> dict[str, Json]:
    """Returns the view preferences that set the interface language and turn on every translation `PreferencesView` offers."""
    return {"language": language, **{prop.identifier: True for prop in bpy.types.PreferencesView.bl_rna.properties if prop.identifier.startswith("use_translate_")}}


def assigned(path: str, /) -> CodeType:
    """Compiles `<path> = __value__` from the parsed path, any assignable target included."""
    match target := ast.parse(path, mode="eval").body:
        case ast.Attribute() | ast.Subscript() | ast.Name():
            target.ctx = ast.Store()
            return compile(ast.fix_missing_locations(ast.Module([ast.Assign([target], ast.Name("__value__", ast.Load()))], [])), f"<set {path}>", "exec")
        case _:
            raise SyntaxError(f"{path} is not an assignable target")


def evaluated(expression: str, names: Namespace, /) -> object:
    """Evaluates a case expression over its namespace."""
    return eval(compile(expression, f"<{expression}>", "eval"), names)


def module(script: str, code: str, name: str, /) -> Namespace:
    """Runs a prepared source as the module `name`, its frames naming `script`, and returns the namespace it built."""
    namespace: Namespace = {"__name__": name, "__file__": script, "__builtins__": builtins}
    exec(compile(code, script, "exec"), namespace)
    return namespace


def described(error: BaseException, /) -> str:
    """Returns the traceback without engine frames."""
    report = traceback.TracebackException.from_exception(error)
    report.stack = traceback.StackSummary.from_list([frame for frame in report.stack if frame.filename != __file__])
    return "".join(report.format())


def thrown(error: BaseException, script: str | None, /) -> dict[str, Json]:
    """Returns the completion of a raising original: the exception line, and the frames innermost first, a prepared source frame by its line alone and every other by file and line."""
    frames: list[dict[str, Json]] = [
        {"line": frame.lineno, **({} if frame.filename == script else {"file": frame.filename})}
        for frame in reversed(traceback.extract_tb(error.__traceback__))
        if frame.filename != __file__ and frame.lineno is not None
    ]
    located: dict[str, Json] = {**frames[0], "stack": [*frames]} if frames else {}
    return {"_tag": "Thrown", "message": "".join(traceback.format_exception_only(error)).strip(), **located}


# --- [OUTPUT]


def recorded(stream: IO[str], line: Mapping[str, Json], /) -> None:
    """Appends one records line and syncs it to disk, so a crash keeps every line written before it."""
    stream.write(f"{json.dumps(line, allow_nan=False)}\n")
    stream.flush()
    os.fsync(stream.fileno())


def replaced(target: Path, line: Mapping[str, Json], /) -> None:
    """Writes one JSON file beside its target and moves it into place, readers seeing the file whole or not at all."""
    staged = target.with_name(f".{target.name}")
    staged.write_text(json.dumps(line, allow_nan=False), encoding="utf-8")
    staged.replace(target)


def offsets(instance: Instance, /) -> tuple[int, int, int]:
    """Flushes Python output and returns the output, error, and log file sizes."""
    sys.stdout.flush()
    sys.stderr.flush()
    return os.fstat(1).st_size, os.fstat(2).st_size, instance.log.stat().st_size


def printed(instance: Instance, start: tuple[int, int, int], /) -> dict[str, Json]:
    """Returns what the process wrote to its output, error, and log files since `start`, C-level output included, log lines without their clock."""

    def tail(name: Path, at: int, /) -> list[Json]:
        with name.open("rb") as source:
            source.seek(at)
            return source.read().decode(errors="replace").splitlines()

    def logged(lines: tuple[Json, ...], line: str, /) -> tuple[Json, ...]:
        match LOGGED.match(line), lines:
            case re.Match() as hit, _:
                return (*lines, {"category": hit["category"], "message": hit["message"]})
            case None, (*rest, {"category": str(category), "message": str(message)}):
                return (*rest, {"category": category, "message": f"{message}\n{line}"})
            case _:
                return lines

    offsets(instance)
    folded: tuple[Json, ...] = reduce(logged, [str(line) for line in tail(instance.log, start[2])], ())
    return {"console": tail(instance.scratch / STDOUT, start[0]), "stderr": tail(instance.scratch / STDERR, start[1]), "log": list(folded)}


# --- [ISOLATION]


def entries(collection: Items, /) -> list[tuple[object, str, Struct]]:
    """Returns each collection item with its identity and label: its key where every item has a unique one, else its pointer and index."""
    keys = collection.keys()
    keyed = len(set(keys)) == len(collection)
    return [(keys[index], json.dumps(keys[index]), item) if keyed else (item.as_pointer(), str(index), item) for index, item in enumerate(collection)]


def settings() -> dict[str, object]:
    """Returns each writable preferences member by path and each removable preferences collection's item identities, walking pointer members through their RNA."""

    def walked(owner: Struct, prefix: str, /) -> Iterable[tuple[str, object]]:
        for prop in owner.bl_rna.properties:
            held = getattr(owner, prop.identifier)
            match prop:
                case bpy.types.PointerProperty() if prop.identifier != "rna_type" and isinstance(held, bpy.types.bpy_struct) and not isinstance(held, bpy.types.ID):
                    yield from walked(held, f"{prefix}{prop.identifier}.")
                case bpy.types.CollectionProperty() if prop.srna is not None and "remove" in prop.srna.functions:
                    yield f"{prefix}{prop.identifier}", frozenset(identity for identity, _, _ in entries(held))
                case bpy.types.PointerProperty() | bpy.types.CollectionProperty():
                    pass
                case _ if prop.is_readonly:
                    pass
                case _:
                    yield f"{prefix}{prop.identifier}", tuple(held) if isinstance(held, Iterable) and not isinstance(held, str) else held

    return dict(walked(preferences(), ""))


def evicted(collection: Items, path: str, before: frozenset[object], /) -> list[str]:
    """Removes each item the case added to a preferences collection, disabling an added add-on, returning each item's path."""
    added = [(label, item) for identity, label, item in entries(collection) if identity not in before]
    for _, item in added:
        match item:
            case bpy.types.Addon(module=str(module_name)):
                addon_utils.disable(module_name, default_set=True, handle_error=raised)
            case _:
                collection.remove(item)
    return [f"{path}[{label}]" for label, _ in added]


def restored(held: Mapping[str, object], /) -> list[str]:
    """Removes each preferences collection item the case added, then writes back each member that differs from `held`, returning each path."""
    current, owner = settings(), preferences()
    left: list[str] = []
    for path, value in current.items():
        match value, held.get(path):
            case frozenset(), frozenset(before):
                collection: Items = owner.path_resolve(path)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                left.extend(evicted(collection, path, before))
            case frozenset(), _:
                pass
            case _, previous if previous != value:
                parent, _, name = path.rpartition(".")
                setattr(owner.path_resolve(parent) if parent else owner, name, previous)
                left.append(path)
            case _:
                pass
    return left


def named(function: object, /) -> str:
    """Returns a callback's qualified name, else its representation."""
    return function.__qualname__ if isinstance(function, FunctionType) else repr(function)


def interfaces() -> Iterator[Interface]:
    """Yields every Menu, Panel, and Header class Python defines, subclasses of subclasses included."""
    pending: list[Interface] = [bpy.types.Menu, bpy.types.Panel, bpy.types.Header]
    while pending:
        held = pending.pop()
        yield held
        pending.extend(held.__subclasses__())


def keymaps() -> bpy.types.KeyMaps:
    """Returns the add-on keyconfig's keymaps, which every window manager holds."""
    held: bpy.types.KeyConfig = manager().keyconfigs.addon  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return held.keymaps


def leftover(timers: Timers, held: Held, expected: frozenset[str], /) -> list[str]:
    """Undoes every timer, handler, class, keymap, draw function, import path, module, and preference the case left, returning each one."""
    left = [f"timers.{named(function)}" for function, _ in timers if bpy.app.timers.is_registered(function)]
    for function, _ in timers:
        if bpy.app.timers.is_registered(function):
            bpy.app.timers.unregister(function)
    for name, before in held.handlers.items():
        callbacks: list[object] = getattr(bpy.app.handlers, name)
        for callback in [callback for callback in callbacks if callback not in before]:
            callbacks.remove(callback)
            left.append(f"handlers.{name}.{named(callback)}")
    for name in sorted(frozenset(dir(bpy.types)) - held.classes):
        bpy.utils.unregister_class(getattr(bpy.types, name))
        left.append(f"classes.{name}")
    added = keymaps()
    for keymap in list(added):
        match held.keymaps.get(key := (keymap.name, keymap.space_type, keymap.region_type)):
            case None:
                added.remove(keymap)
                left.append(f"keymaps.{key[0]}")
            case registered:
                for item in [item for item in keymap.keymap_items if item.id not in registered]:
                    left.append(f"keymaps.{key[0]}.{item.idname}")
                    keymap.keymap_items.remove(item)
    for owner, before in held.draws.items():
        if isinstance(draw := owner.draw, Extended):
            for appended in [entry for entry in draw._draw_funcs if entry not in before]:
                owner.remove(appended)
                left.append(f"draws.{owner.__name__}.{named(appended)}")
    if tuple(sys.path) != held.path:
        left.extend(f"sys.path.{entry}" for entry in sys.path if entry not in held.path)
        sys.path[:] = held.path
    installed = (Path(bpy.utils.resource_path("LOCAL")), Path(bpy.utils.resource_path("USER")))
    for name in sorted(sys.modules.keys() - held.modules):
        if (origin := sys.modules[name].__file__) is not None and not any(Path(origin).is_relative_to(root) for root in installed):
            left.append(f"modules.{name}")
            if not origin.endswith(tuple(EXTENSION_SUFFIXES)):
                del sys.modules[name]
    left.extend(f"preferences.{path}" for path in restored(held.preferences) if path not in expected)
    return left


# --- [PRODUCT]


def raised(error: BaseException, /) -> None:
    """Raises an `addon_utils` failure in place of printing it."""
    raise error


def packaged(script: Path, /) -> Path | None:
    """Returns the extension or legacy add-on folder a case's `__init__.py` source belongs to, read through its manifest or a module-level `bl_info` assignment, and nothing for a plain script."""
    declares = script.name == "__init__.py" and (
        (script.parent / MANIFEST).is_file()
        or any(isinstance(node, ast.Assign) and any(isinstance(target, ast.Name) and target.id == "bl_info" for target in node.targets) for node in ast.parse(script.read_bytes()).body)
    )
    return script.parent if declares else None


def addon(folder: Path, repository: Path, stack: ExitStack, /) -> str:
    """Links an extension folder into the batch repository or a `bl_info` folder onto the user add-on path, returning the module Blender lists for the link."""
    link = repository / folder.name if (folder / MANIFEST).is_file() else Path(bpy.utils.user_resource("SCRIPTS", path="addons", create=True), folder.name)
    link.symlink_to(folder)
    stack.callback(link.unlink)
    if link.parent == repository:
        addon_utils.extensions_refresh(ensure_wheels=True, handle_error=raised)
    else:
        addon_utils.modules_refresh()
    modules: Iterable[ModuleType] = addon_utils.modules()  # type: ignore[assignment, func-returns-value]  # ty: ignore[invalid-assignment]
    match [held.__name__ for held in modules if held.__file__ is not None and Path(held.__file__).resolve().is_relative_to(folder.resolve())]:
        case [name]:
            return name
        case listed:
            raise ImportError(f"{folder} is listed as {listed} by addon_utils.modules")


def disabled(module_name: str, /) -> None:
    """Disables an add-on and drops its modules, so the next case imports it fresh."""
    addon_utils.disable(module_name, default_set=True, handle_error=raised)
    for name in [name for name in sys.modules if name == module_name or name.startswith(f"{module_name}.")]:
        del sys.modules[name]


def enabled(folder: Path, repository: Path, stack: ExitStack, /) -> Namespace:
    """Enables an extension or legacy add-on as Blender enables one, disabling it and dropping its modules when the case ends, and returns its namespace."""
    module_name = addon(folder, repository, stack)
    stack.callback(disabled, module_name)
    addon_utils.enable(module_name, default_set=True, handle_error=raised)
    return vars(sys.modules[module_name])


def installed(owner: Path, name: str, /) -> Path:
    """Returns the folder of one extension installed in the owner's repositories."""
    match sorted(owner.glob(f"extensions/*/{glob.escape(name)}/{glob.escape(MANIFEST)}")):
        case [found]:
            return found.parent
        case found:
            raise FileNotFoundError(f"{name} names {len(found)} extensions in {owner / 'extensions'}: {[str(path.parent) for path in found]}")


def timed(stack: ExitStack, /) -> Timers:
    """Records each timer the case registers with the instant it first falls due, restoring `bpy.app.timers.register` when the case ends."""
    held: Timers = []
    original = bpy.app.timers.register

    def register(function: Callable[[], float | None], *, first_interval: float | None = 0, persistent: bool | None = False) -> None:
        held.append((function, time.monotonic() + (first_interval or 0)))
        original(function, first_interval=first_interval, persistent=persistent)

    bpy.app.timers.register = register  # ty: ignore[invalid-assignment]
    stack.callback(setattr, bpy.app.timers, "register", original)
    return held


def sourced(case: Case, /) -> Source | None:
    """Returns a case's script path and prepared source, nothing for a case naming no script."""
    match case:
        case {"source": {"script": str(script), "code": str(code)}}:
            return {"script": script, "code": code}
        case _:
            return None


# --- [PRECONDITIONS]


def document(row: Literal["none"] | Document, folder: Path, made: Namespace, /) -> None:
    """Loads the case document: a factory home file for `none`, the case folder copy `open` names, or an empty file saved in its working color space, its scene members written."""
    filepaths = preferences().filepaths
    match row:
        case "none":
            bpy.ops.wm.read_homefile(use_factory_startup=True, load_ui=filepaths.use_load_ui)
        case {"open": str(copy)}:
            options = {"load_ui": filepaths.use_load_ui, "use_scripts": filepaths.use_scripts_auto_execute, **row.get("options", {})}
            bpy.ops.wm.open_mainfile(filepath=copy, **options)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            written(manager().windows[0].scene, row.get("scene", {}), made)
        case {"working_space": str(space), "scene": {"render": dict(render)} as scene}:
            bpy.ops.wm.read_homefile(use_empty=True, load_ui=filepaths.use_load_ui)
            bpy.ops.wm.set_working_color_space(working_space=space)
            written(manager().windows[0].scene, {**scene, "render": {"filepath": "//", **render}}, made)
            bpy.ops.wm.save_as_mainfile(filepath=str(folder / "case.blend"))
        case _:
            raise ValueError(f"{row} names no document")


def loaded(precondition: Precondition, folder: Path, made: Namespace, /) -> Steps:
    """Loads the case document, sets the interface language, and sets the window members, passing the event loop after a workspace change."""
    document(precondition.get("document", "none"), folder, made)
    if (language := precondition.get("language")) is not None:
        written(preferences().view, translated(language), made)
    window, members = manager().windows[0], precondition.get("window", {})
    if "scene" in members:
        window.scene = bpy.data.scenes[members["scene"]]
    if "view_layer" in members:
        window.view_layer = window.scene.view_layers[members["view_layer"]]
    if "workspace" in members:
        window.workspace = bpy.data.workspaces[members["workspace"]]
        yield PASS


def overridden(row: Area | None, /) -> Override:
    """Resolves the case override once: the window, and the area of the asked type (the largest area turned to it when none has it) with its region."""
    window = manager().windows[0]
    if row is None:
        return {"window": window}
    area = next((item for item in window.screen.areas if item.type == row["type"]), None) or max(window.screen.areas, key=lambda item: item.width * item.height)
    area.type = row["type"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    wanted = row.get("region", "WINDOW")
    match next((item for item in area.regions if item.type == wanted), None):
        case bpy.types.Region() as region:
            return {"window": window, "area": area, "region": region}
        case _:
            raise ValueError(f"{area.type} area holds no {wanted} region, only {[item.type for item in area.regions]}")


def prepared(precondition: Precondition, override: Override, names: Namespace, made: Namespace, /) -> None:
    """Applies the rows that act on the loaded document under the case override, in field order."""
    window = override["window"]
    with bpy.context.temp_override(**override):
        for key, row in precondition.get("add", {}).items():
            owner = getattr(bpy.data, row["kind"])
            accepted = {prop.identifier for prop in owner.bl_rna.functions["new"].parameters if not prop.is_output}
            properties = row.get("properties", {})
            made[key] = created = owner.new(**{"name": key, **{name: unburied(value, made) for name, value in properties.items() if name in accepted}})
            written(created, {name: value for name, value in properties.items() if name not in accepted}, made)
            if isinstance(created, bpy.types.Object):
                window.scene.collection.objects.link(created)
        for path, value in precondition.get("set", []):
            exec(assigned(path), {**names, "__value__": unburied(value, made)})
        if (selected := precondition.get("select")) is not None:
            chosen = [evaluated(expression, names) for expression in selected]
            for item in window.view_layer.objects:
                item.select_set(item in chosen)
            window.view_layer.objects.active = next((item for item in chosen if isinstance(item, bpy.types.Object)), None)
        if (mode := precondition.get("mode")) is not None:
            bpy.ops.object.mode_set(mode=mode)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        if (frame := precondition.get("frame")) is not None:
            window.scene.frame_set(frame)
        for statement in precondition.get("steps", []):
            exec(compile(statement, "<step>", "exec"), names)
        if precondition.get("edited", False):
            bpy.ops.ed.undo_push(message="precondition")


def contextual(value: object, /) -> bool:
    """Returns whether a context member names document state: an ID, a list of IDs, a scalar, or nothing."""
    return value is None or isinstance(value, bpy.types.ID | str | int | float) or (isinstance(value, list) and all(isinstance(item, bpy.types.ID) for item in value))


def documented(override: Override, /) -> Json:
    """Returns the document state a case runs on: units, color management, working color space, output size and rate, frame, context members, file version, path, and edited state."""
    scene = override["window"].scene
    with bpy.context.temp_override(**override):
        context = {key: value for key, value in bpy.context.copy().items() if contextual(value)}
    return buried({
        "units": scene.unit_settings,
        "display": scene.display_settings,
        "view": scene.view_settings,
        "sequencer": scene.sequencer_colorspace_settings,
        "colorspace": bpy.data.colorspace,
        "resolution": (scene.render.resolution_x, scene.render.resolution_y, scene.render.resolution_percentage),
        "fps": (scene.render.fps, scene.render.fps_base),
        "frame": (scene.frame_current, scene.frame_subframe),
        "context": context,
        "version": bpy.data.version,
        "filepath": bpy.data.filepath,
        "dirty": bpy.data.is_dirty,
    })


# --- [CASES]


def reads(case: Case, override: Override, names: Namespace, /) -> tuple[dict[str, Json], dict[str, Json]]:
    """Evaluates each read under the case override, splitting values from refusals."""

    def read(expression: str, /) -> tuple[bool, Json]:
        try:
            with bpy.context.temp_override(**override):
                return True, buried(evaluated(expression, names))
        except (Exception, SystemExit) as error:
            return False, described(error)

    answers = {key: read(expression) for key, expression in case["read"].items()}
    return {key: value for key, (ok, value) in answers.items() if ok}, {key: value for key, (ok, value) in answers.items() if not ok}


def executed(case: Case, product: Namespace, names: Namespace, made: Namespace, /) -> dict[str, Json]:
    """Runs the case body and returns its value: an expression, a call on decoded arguments, or a plain script run as `__main__`, an enabled add-on's registration having run already."""
    match case:
        case {"_tag": "Probe", "evaluate": str(expression)}:
            return {"value": buried(evaluated(expression, names))}
        case {"_tag": "Call", "call": str(call), "args": list(args)}:
            callee: Callee = evaluated(call, {**product, **names})  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            keywords = {key: unburied(value, made) for key, value in case.get("keywords", {}).items()}
            return {"value": buried(callee(*[unburied(arg, made) for arg in args], **keywords))}
        case {"_tag": "Run", "source": {"script": str(script), "code": str(code)}} if packaged(Path(script)) is None:
            module(script, code, "__main__")
            return {}
        case _:
            return {}


def ran(case: Case, product: Namespace, names: Namespace, made: Namespace, override: Override, /) -> dict[str, Json]:
    """Runs the case body under the override, a raise or `SystemExit` becoming its completion."""
    source = sourced(case)
    try:
        with bpy.context.temp_override(**override):
            return {"completion": {"_tag": "Ran"}, **executed(case, product, names, made)}
    except SystemExit as error:
        return {"completion": {"_tag": "Exited"}, "value": buried(error.code)}
    except Exception as error:
        return {"completion": thrown(error, source["script"] if source is not None else None)}


def pressed(window: bpy.types.Window, kind: str, /) -> Steps:
    """Posts one key press and release to the window, then passes the event loop so the popup it answers handles it."""
    window.event_simulate(type=kind, value="PRESS")  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    window.event_simulate(type=kind, value="RELEASE")  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    yield PASS


def settled(timers: Timers, window: bpy.types.Window, /) -> Steps:
    """Evaluates the view layer, then passes the event loop until no job runs and every case timer fell due and ran once, a background run holding no event loop to pass."""
    window.view_layer.update()
    due = max((at for _, at in timers), default=0.0)
    while not bpy.app.background and (any(bpy.app.is_job_running(job) for job in jobs()) or time.monotonic() < due):  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        yield PASS
    yield PASS


def answered(window: bpy.types.Window, area: bpy.types.Area, space: bpy.types.SpaceFileBrowser, answer: str | None, /) -> Steps:
    """Answers one file browser: a path through `file.execute` after the event loop reads it, nothing through `file.cancel`."""
    match answer:
        case str(target):
            params: bpy.types.FileSelectParams = space.params  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            params.directory, params.filename = str(Path(target).parent).encode(), Path(target).name
            yield PASS
            with bpy.context.temp_override(window=window, area=area):
                bpy.ops.file.execute()
        case None:
            with bpy.context.temp_override(window=window, area=area):
                bpy.ops.file.cancel()


def picked(rows: list[str | None], /) -> Generator[float, None, tuple[list[Json], list[Json]]]:
    """Answers each file browser window in order with the next `files` row, then closes the windows past the first, returning each window's area types and each browser's operator."""
    answers = iter(rows)
    windows: list[Json] = []
    pickers: list[Json] = []
    for window in manager().windows[1:]:
        windows.append([item.type for item in window.screen.areas])
        if (area := next((item for item in window.screen.areas if item.type == "FILE_BROWSER"), None)) is not None:
            space: bpy.types.SpaceFileBrowser = area.spaces.active  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            pickers.append(space.active_operator.bl_idname if space.active_operator is not None else space.type)
            yield from answered(window, area, space, next(answers, None))
    yield PASS
    closed()
    return windows, pickers


def closed() -> None:
    """Closes every window past the first under its own override."""
    for window in manager().windows[1:]:
        with bpy.context.temp_override(window=window):
            bpy.ops.wm.window_close()


def pushed() -> list[str]:
    """Returns the undo step names, none before the first push."""
    return [step.name for step in stack.steps] if (stack := manager().undo_stack) is not None else []


def changed(before: Mapping[int, str], steps: int, updates: Updates, /) -> dict[str, Json]:
    """Returns what the case changed: IDs created and removed by data path, IDs the depsgraph updated with each update kind, undo steps pushed, and the file's edited state."""
    after = {item.session_uid: item for item in bpy.data.all_ids}
    updated: dict[str, frozenset[str]] = {}
    for path, flags in updates:
        updated[path] = updated.get(path, frozenset()) | flags
    return {
        "created": [*sorted(after[uid].path_from_module() for uid in after.keys() - before.keys())],
        "removed": [*sorted(before[uid] for uid in before.keys() - after.keys())],
        "updated": {path: [flag for flag in FLAGS if flag in flags] for path, flags in sorted(updated.items())},
        "undo": [*pushed()[steps:]],
        "dirty": bpy.data.is_dirty,
    }


def arranged(case: Case, instance: Instance, folder: Path, stack: ExitStack, /) -> Generator[float, None, tuple[Override, Namespace, Namespace, Namespace]]:
    """Enables the case's extensions and add-on, loads its document, applies its preconditions, and runs a call case's plain script as its module, then passes the event loop once."""
    precondition, made = case["precondition"], dict[str, object]()
    for name in precondition.get("extensions", []):
        enabled(installed(instance.owner, name), instance.repository, stack)
    source = sourced(case)
    package = packaged(Path(source["script"])) if source is not None else None
    product = enabled(package, instance.repository, stack) if package is not None else {}
    stack.callback(closed)
    yield from loaded(precondition, folder, made)
    override = overridden(precondition.get("area"))
    names: Namespace = {"bpy": bpy, "context": bpy.context, "scene": override["window"].scene, "made": made}
    prepared(precondition, override, names, made)
    if case["_tag"] == "Call" and source is not None and package is None:
        product = module(source["script"], source["code"], Path(source["script"]).stem)
    yield PASS
    return override, names, made, product


def acted(case: Case, instance: Instance, stack: ExitStack, timers: Timers, ready: tuple[Override, Namespace, Namespace, Namespace], /) -> Generator[float, None, dict[str, Json]]:
    """Runs, answers, settles, and reads one arranged case, recording every warning it raised and every ID the depsgraph updated."""
    override, names, made, product = ready
    state = documented(override)
    caught = stack.enter_context(warnings.catch_warnings(record=True))
    warnings.simplefilter("always")
    instance.updates.clear()
    before, unread = reads(case, override, names)
    ids, steps = {item.session_uid: item.path_from_module() for item in bpy.data.all_ids}, len(pushed())
    completion = ran(case, product, names, made, override)
    for answer in case.get("confirms", []):
        yield from pressed(manager().windows[0], "RET" if answer else "ESC")
    yield from settled(timers, manager().windows[0])
    modal: list[Json] = [operator.bl_idname for window in manager().windows for operator in window.modal_operators]
    windows, pickers = yield from picked(case.get("files", []))
    if not bpy.app.background:
        yield from pressed(manager().windows[0], "ESC")
    names["depsgraph"] = manager().windows[0].view_layer.depsgraph
    after, failed = reads(case, override, names)
    readings: dict[str, Json] = {"before": before, "read": after} if case["read"] else {}
    refusals: dict[str, Json] = {**({"beforeFailed": unread} if unread else {}), **({"readFailed": failed} if failed else {})}
    return {
        **completion,
        **readings,
        **refusals,
        "opened": [state],
        "changes": changed(ids, steps, instance.updates),
        "modal": modal,
        "windows": windows,
        "files": pickers,
        "warnings": [{"category": item.category.__name__, "message": str(item.message), "file": item.filename, "line": item.lineno} for item in caught],
    }


def stepped(instance: Instance, index: int, case: Case, stream: IO[str], /) -> Steps:
    """Runs one case to its outcome line, its output read from after its preconditions, a failure anywhere recorded as `Failed`, and undoes what the case left registered."""
    recorded(stream, {"_tag": "Started", "index": index})
    held = Held(
        handlers={name: tuple(callbacks) for name in dir(bpy.app.handlers) if isinstance(callbacks := getattr(bpy.app.handlers, name), list)},
        classes=frozenset(dir(bpy.types)),
        keymaps={(keymap.name, keymap.space_type, keymap.region_type): frozenset(item.id for item in keymap.keymap_items) for keymap in keymaps()},
        draws={owner: tuple(draw._draw_funcs) if isinstance(draw := owner.draw, Extended) else (draw,) for owner in interfaces()},
        path=tuple(sys.path),
        modules=frozenset(sys.modules),
        preferences=settings(),
    )
    start = offsets(instance)
    body: dict[str, Json]
    with ExitStack() as stack:
        timers = timed(stack)
        try:
            ready = yield from arranged(case, instance, Path(case["folder"]), stack)
            start = offsets(instance)
            body = yield from acted(case, instance, stack, timers, ready)
        except (Exception, SystemExit) as error:
            body = {"completion": {"_tag": "Failed", "reason": described(error)}}
    expected = frozenset(f"view.{name}" for name in translated(case["precondition"]["language"])) if "language" in case["precondition"] else frozenset()
    leaked: list[Json] = [*leftover(timers, held, expected)]
    recorded(stream, {"_tag": "Finished", "outcome": {"name": case["name"], **body, **printed(instance, start), "leaked": leaked}})


# --- [SESSIONS]


def batch(instance: Instance, session: Session, /) -> Steps:
    """Steps every case of one session into its records file, then writes the reply: the application and version, with the host state and the RNA items precondition rows name when the survey is asked, else the failure outside every case."""
    reply: dict[str, Json]
    try:
        with Path(session["records"]).open("a", encoding="utf-8") as stream:
            for index, case in enumerate(session["cases"]):
                yield from stepped(instance, index, case, stream)
        survey: dict[str, Json] = {
            "host": host(),
            "collections": [prop.identifier for prop in bpy.types.BlendData.bl_rna.properties if prop.type == "COLLECTION"],
            "modes": items(bpy.types.Object, "mode"),
            "areas": items(bpy.types.Area, "type"),
            "regions": items(bpy.types.Region, "type"),
        }
        reply = {"application": Path(bpy.app.binary_path).stem, "version": bpy.app.version_string, **({"survey": survey} if session["survey"] else {})}
    except (Exception, SystemExit) as error:
        reply = {"failed": described(error)}
    replaced(Path(session["reply"]), reply)


def batches(instance: Instance, noted: Callable[[bpy.types.Scene, bpy.types.Depsgraph], None], /) -> Steps:
    """Records depsgraph updates through `noted` while running each session file the scratch folder receives in order, idling between them, until the quit file appears."""
    bpy.app.handlers.depsgraph_update_post.append(noted)
    seen: set[Path] = set()
    while not (instance.scratch / "quit").exists():
        pending = sorted(path for path in instance.scratch.glob("*/session.json") if path not in seen)
        seen.update(pending)
        for path in pending:
            yield from batch(instance, json.loads(path.read_text(encoding="utf-8")))
        yield 0.1
    bpy.app.handlers.depsgraph_update_post.remove(noted)


def stepping(steps: Steps, /) -> float | None:
    """Advances the sessions one step per timer pass, returning the pause before the next, and quits Blender once they end."""
    match next(steps, None):
        case float(pause):
            return pause
        case None:
            with bpy.context.temp_override(window=manager().windows[0]):
                bpy.ops.wm.quit_blender()
            return None


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Moves process output into the scratch folder, writes the host state, then runs the sessions to their end in a background run and on a persistent timer in a windowed one."""
    scratch = Path(sys.argv[sys.argv.index("--") + 1])
    for descriptor, name in ((1, STDOUT), (2, STDERR)):
        os.dup2(os.open(scratch / name, os.O_WRONLY | os.O_CREAT | os.O_APPEND), descriptor)
    sys.pycache_prefix = os.environ.get("PYTHONPYCACHEPREFIX")
    held = preferences()
    held.view.show_splash = False
    repository = scratch / "repository"
    repository.mkdir(exist_ok=True)
    held.extensions.repos.new(name=REPOSITORY, module=REPOSITORY, custom_directory=str(repository), source="USER")
    updates: Updates = []

    @bpy.app.handlers.persistent  # type: ignore[untyped-decorator]  # ty: ignore[dynamic-function-decorator-return]
    def noted(_scene: bpy.types.Scene, depsgraph: bpy.types.Depsgraph, /) -> None:
        updates.extend(
            (located(origin, repr(origin)), frozenset(flag for flag in FLAGS if getattr(update, f"{UPDATED}{flag}")))
            for update in depsgraph.updates
            if (source := update.id) is not None and (origin := source.original) is not None
        )

    replaced(scratch / HOST, host())
    steps = batches(Instance(scratch=scratch, log=Path(sys.argv[sys.argv.index("--log-file") + 1]), repository=repository, owner=owned(), updates=updates), noted)
    if bpy.app.background:
        gpu.init()
        for pause in steps:
            time.sleep(pause)
    else:
        bpy.app.timers.register(partial(stepping, steps), persistent=True)


if __name__ == "__main__":
    main()
