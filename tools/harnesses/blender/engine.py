"""Runs a harness batch to its end in a background Blender, else one case step per timer pass in a hidden Blender it then quits."""

from array import array
import ast
import base64
from collections import deque
from collections.abc import Callable, Generator, Iterable, Iterator, Mapping
from contextlib import ExitStack
import ctypes
import ctypes.util
from dataclasses import dataclass
import faulthandler
from functools import cache, partial, reduce
import glob
import hashlib
from importlib.machinery import EXTENSION_SUFFIXES
from itertools import starmap
import json
import math
import os
from pathlib import Path
import re
import runpy
import shutil
import signal
import sys
import tempfile
import time
import traceback
from types import CodeType, FunctionType, ModuleType
from typing import IO, Literal, NotRequired, Protocol, TypedDict
import warnings

import addon_utils
import bpy
import gpu
import mathutils

# --- [TYPES] ----------------------------------------------------------------------------

type Json = bool | int | float | str | list[Json] | dict[str, Json] | None
type Namespace = dict[str, object]
type Struct = bpy.types.bpy_struct[object]
type Timers = list[tuple[Callable[[], float | None], float]]
type Updates = list[tuple[str, frozenset[str]]]


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
PROHIBITED = 2
LOGGED = re.compile(r"^\S+\s+(?P<category>\S+)\s+\| (?P<message>.*)$")
COLUMNS: Mapping[str, str] = {"FLOAT": "d", "INT": "i", "BOOLEAN": "b"}
UPDATED = "is_updated_"
FLAGS = tuple(prop.identifier.removeprefix(UPDATED) for prop in bpy.types.DepsgraphUpdate.bl_rna.properties if prop.identifier.startswith(UPDATED))
RUNTIME: Mapping[str, tuple[type, list[type]]] = {
    "objc_getClass": (ctypes.c_void_p, [ctypes.c_char_p]),
    "object_getClass": (ctypes.c_void_p, [ctypes.c_void_p]),
    "sel_registerName": (ctypes.c_void_p, [ctypes.c_char_p]),
    "class_getInstanceMethod": (ctypes.c_void_p, [ctypes.c_void_p, ctypes.c_void_p]),
    "method_getImplementation": (ctypes.c_void_p, [ctypes.c_void_p]),
    "method_getTypeEncoding": (ctypes.c_char_p, [ctypes.c_void_p]),
    "class_replaceMethod": (ctypes.c_void_p, [ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_char_p]),
}

# --- [MODELS] ---------------------------------------------------------------------------


class Document(TypedDict, total=False):
    """Case document: a copy of `open` with its `wm.open_mainfile` options, else an empty file in `working_space`, then scene members."""

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


class Picked(TypedDict):
    """File browser answer: a path under the case folder, and the product file copied there first."""

    path: str
    source: NotRequired[str]


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


class Shared(TypedDict):
    """Fields every host case `contract.ts` encodes."""

    name: str
    precondition: Precondition
    read: dict[str, str]
    confirms: list[bool]
    files: list[Picked | None]


class Probe(Shared):
    """Case evaluating one expression."""

    _tag: Literal["Probe"]
    evaluate: str


class Call(Shared):
    """Case calling an original on decoded arguments, the script's names in scope."""

    _tag: Literal["Call"]
    call: str
    args: list[Json]
    keywords: dict[str, Json]
    script: NotRequired[str]


class Run(Shared):
    """Case running a script whole."""

    _tag: Literal["Run"]
    script: str


type Case = Probe | Call | Run


class Session(TypedDict):
    """Session file `host.ts` writes: the case limit in seconds, the files the process writes, and the cases."""

    limit: float
    results: str
    stdout: str
    stderr: str
    log: str
    dictionary: NotRequired[str]
    cases: list[Case]


class Override(TypedDict):
    """Context members the case runs under, resolved once per case."""

    window: bpy.types.Window
    area: NotRequired[bpy.types.Area]
    region: NotRequired[bpy.types.Region]


@dataclass(frozen=True, slots=True, kw_only=True)
class Instance:
    """State of this Blender process across cases: the session, the batch extension repository, the owner's user resources, and depsgraph updates."""

    session: Session
    repository: Path
    owner: Path
    updates: Updates


@dataclass(frozen=True, slots=True, kw_only=True)
class Held:
    """Registrations and settings read before a case, which the engine restores after it."""

    handlers: Mapping[str, tuple[object, ...]]
    classes: frozenset[str]
    keymaps: Mapping[tuple[str, str, str], frozenset[int]]
    draws: Mapping[type, tuple[object, ...]]
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


@cache
def runtime() -> ctypes.CDLL:
    """Returns the Objective-C runtime with the signatures the engine calls."""
    objc = ctypes.CDLL(ctypes.util.find_library("objc"))
    for name, (result, arguments) in RUNTIME.items():
        function = getattr(objc, name)
        function.restype, function.argtypes = result, arguments
    return objc


def application() -> int:
    """Returns the shared `NSApplication`."""
    objc = runtime()
    send = ctypes.CFUNCTYPE(ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p)(("objc_msgSend", objc))
    shared: int = send(objc.objc_getClass(b"NSApplication"), objc.sel_registerName(b"sharedApplication"))
    return shared


def inert() -> None:
    """Replaces `activateIgnoringOtherApps:` on the application's class with `-[NSObject self]`, which reads its receiver alone, so no window Blender raises activates it."""
    objc, shared = runtime(), application()
    owner, selector = objc.object_getClass(shared), objc.sel_registerName(b"activateIgnoringOtherApps:")
    nothing = objc.method_getImplementation(objc.class_getInstanceMethod(objc.objc_getClass(b"NSObject"), objc.sel_registerName(b"self")))
    objc.class_replaceMethod(owner, selector, nothing, objc.method_getTypeEncoding(objc.class_getInstanceMethod(owner, selector)))


def concealed() -> None:
    """Hides Blender and makes it a background-only application, so a window a case left behind leaves the screen."""
    objc = runtime()
    send = ctypes.CFUNCTYPE(ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p)(("objc_msgSend", objc))
    policy = ctypes.CFUNCTYPE(ctypes.c_bool, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_long)(("objc_msgSend", objc))
    shared = application()
    send(shared, objc.sel_registerName(b"hide:"), None)
    policy(shared, objc.sel_registerName(b"setActivationPolicy:"), PROHIBITED)


def owned() -> Path:
    """Returns the user resource folder Blender resolves without `BLENDER_USER_RESOURCES`, where the owner's extensions sit."""
    held = os.environ.pop("BLENDER_USER_RESOURCES")
    try:
        return Path(bpy.utils.resource_path("USER"))
    finally:
        os.environ["BLENDER_USER_RESOURCES"] = held


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

    def rows(collection: bpy.types.bpy_prop_collection[Struct], labels: list[str], /) -> list[Json]:
        match tuple({type(entry) for entry in collection}):
            case (kind,) if issubclass(kind, bpy.types.bpy_struct) and not issubclass(kind, bpy.types.ID) and not any((entry.as_pointer(), kind) in seen for entry in collection):
                seen.update({(entry.as_pointer(), kind): label for entry, label in zip(collection, labels, strict=True)})
                values = {prop.identifier: column(collection, prop, labels) for prop in kind.bl_rna.properties if prop.identifier != "rna_type"}
                return [{name: held[index] for name, held in values.items()} for index in range(len(labels))]
            case _:
                return list(starmap(walked, zip(collection, labels, strict=True)))

    def column(collection: bpy.types.bpy_prop_collection[Struct], prop: bpy.types.Property, labels: list[str], /) -> list[Json]:
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


def gathered(collection: bpy.types.bpy_prop_collection[Struct], prop: bpy.types.Property, size: int, /) -> array[int] | array[float] | None:
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


def dictionary() -> dict[str, Json]:
    """Returns the RNA dictionary of this build: every `bpy.types` struct and every `bpy.ops` operator's properties, walked through RNA."""

    def own(rna: bpy.types.Struct, members: Literal["properties", "functions"], /) -> dict[str, Struct]:
        inherited = getattr(rna.base, members) if rna.base is not None else {}
        return {item.identifier: item for item in getattr(rna, members) if item.identifier != "rna_type" and item.identifier not in inherited}

    structs = {
        name: {"base": rna.base, "description": rna.description, "properties": own(rna, "properties"), "functions": own(rna, "functions")}
        for name in dir(bpy.types)
        if (rna := getattr(getattr(bpy.types, name), "bl_rna", None)) is not None
    }
    operators = {
        f"{module}.{name}": {
            "description": (rna := getattr(getattr(bpy.ops, module), name).get_rna_type()).description,
            "properties": {prop.identifier: prop for prop in rna.properties if prop.identifier != "rna_type"},
        }
        for module in dir(bpy.ops)
        for name in dir(getattr(bpy.ops, module))
    }
    return {"structs": buried(structs), "operators": buried(operators)}


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


def described(error: BaseException, /) -> str:
    """Returns the traceback without engine frames."""
    report = traceback.TracebackException.from_exception(error)
    report.stack = traceback.StackSummary.from_list([frame for frame in report.stack if frame.filename != __file__])
    return "".join(report.format())


def thrown(error: BaseException, /) -> dict[str, Json]:
    """Returns the completion of a raising original: the exception line, and the file and line of its deepest frame."""
    frames = [frame for frame in traceback.extract_tb(error.__traceback__) if frame.filename != __file__]
    deepest: dict[str, Json] = {"file": frames[-1].filename, "line": frames[-1].lineno} if frames and frames[-1].lineno is not None else {}
    return {"_tag": "Thrown", "message": "".join(traceback.format_exception_only(error)).strip(), **deepest}


def warned(caught: warnings.WarningMessage, /) -> dict[str, Json]:
    """Returns one warning a case raised with its category and source line."""
    return {"category": caught.category.__name__, "message": str(caught.message), "file": caught.filename, "line": caught.lineno}


# --- [OUTPUT]


def recorded(stream: IO[str], line: Mapping[str, Json], /) -> None:
    """Appends one results line and syncs it to disk, so a crash keeps every line written before it."""
    stream.write(f"{json.dumps(line, allow_nan=False)}\n")
    stream.flush()
    os.fsync(stream.fileno())


def offsets(session: Session, /) -> tuple[int, int, int]:
    """Flushes Python output and returns the output, error, and log file sizes."""
    sys.stdout.flush()
    sys.stderr.flush()
    return os.fstat(1).st_size, os.fstat(2).st_size, Path(session["log"]).stat().st_size


def printed(session: Session, start: tuple[int, int, int], /) -> dict[str, Json]:
    """Returns what the process wrote to its output, error, and log files since `start`, C-level output included, log lines without their clock."""

    def tail(name: str, at: int, /) -> str:
        with Path(name).open("rb") as source:
            source.seek(at)
            return source.read().decode(errors="replace")

    def logged(lines: tuple[Json, ...], line: str, /) -> tuple[Json, ...]:
        match LOGGED.match(line), lines:
            case re.Match() as hit, _:
                return (*lines, {"category": hit["category"], "message": hit["message"]})
            case None, (*rest, {"category": str(category), "message": str(message)}):
                return (*rest, {"category": category, "message": f"{message}\n{line}"})
            case _:
                return lines

    offsets(session)
    folded: tuple[Json, ...] = reduce(logged, tail(session["log"], start[2]).splitlines(), ())
    return {"stdout": tail(session["stdout"], start[0]), "stderr": tail(session["stderr"], start[1]), "log": list(folded)}


def digested(path: Path, /) -> dict[str, Json]:
    """Returns a file's size and SHA-256, and for an image Blender reads the SHA-256 of its float pixels, which stamp metadata leaves unchanged."""
    with path.open("rb") as source:
        digest: dict[str, Json] = {"size": path.stat().st_size, "sha256": hashlib.file_digest(source, "sha256").hexdigest()}
    extensions: frozenset[str] = bpy.path.extensions_image  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    if path.suffix.lower() not in extensions:
        return digest
    image = bpy.data.images.load(str(path), check_existing=False)
    try:
        size = array("i", (0, 0))
        image.size.foreach_get(size)
        pixels = array("f", [0.0]) * (size[0] * size[1] * image.channels)
        image.pixels.foreach_get(pixels)
        return {**digest, "pixels": hashlib.sha256(pixels).hexdigest()}
    finally:
        bpy.data.images.remove(image)


def listed(folder: Path, /) -> dict[str, int]:
    """Returns each file under the folder with its modification time."""
    return {str(path.relative_to(folder)): path.stat().st_mtime_ns for path in folder.rglob("*") if path.is_file()}


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
            case bpy.types.Addon(module=str(module)):
                addon_utils.disable(module, default_set=True, handle_error=raised)
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


def interfaces() -> Iterator[type]:
    """Yields every Menu, Panel, and Header class Python defines, subclasses of subclasses included."""
    pending: list[type] = [bpy.types.Menu, bpy.types.Panel, bpy.types.Header]
    while pending:
        held = pending.pop()
        yield held
        pending.extend(held.__subclasses__())


def keymaps() -> bpy.types.KeyMaps:
    """Returns the add-on keyconfig's keymaps, which every window manager holds."""
    held: bpy.types.KeyConfig = manager().keyconfigs.addon  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return held.keymaps


def bindings() -> dict[tuple[str, str, str], frozenset[int]]:
    """Returns each add-on keymap by name, space, and region with the ids of its items."""
    return {(keymap.name, keymap.space_type, keymap.region_type): frozenset(item.id for item in keymap.keymap_items) for keymap in keymaps()}


def environment() -> tuple[Path, ...]:
    """Returns the folders holding Blender's own modules and the profile's installed wheels, which no case owns."""
    return Path(bpy.utils.resource_path("LOCAL")), Path(bpy.utils.resource_path("USER"))


def holding() -> Held:
    """Reads every registration and setting a case can leave behind."""
    return Held(
        handlers={name: tuple(callbacks) for name in dir(bpy.app.handlers) if isinstance(callbacks := getattr(bpy.app.handlers, name), list)},
        classes=frozenset(dir(bpy.types)),
        keymaps=bindings(),
        draws={held: tuple(getattr(draw, "_draw_funcs", None) or (draw,)) for held in interfaces() if (draw := getattr(held, "draw", None)) is not None},
        path=tuple(sys.path),
        modules=frozenset(sys.modules),
        preferences=settings(),
    )


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
        if getattr(held_class := getattr(bpy.types, name), "is_registered", False):
            bpy.utils.unregister_class(held_class)
            left.append(f"classes.{name}")
    added = keymaps()
    for keymap in list(added):
        match held.keymaps.get(key := (keymap.name, keymap.space_type, keymap.region_type)):
            case None:
                added.remove(keymap)
                left.append(f"keymaps.{key[0]}")
            case items:
                for item in [item for item in keymap.keymap_items if item.id not in items]:
                    left.append(f"keymaps.{key[0]}.{item.idname}")
                    keymap.keymap_items.remove(item)
    for owner, before in held.draws.items():
        for function in [function for function in getattr(getattr(owner, "draw", None), "_draw_funcs", ()) if function not in before]:
            owner.remove(function)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            left.append(f"draws.{owner.__name__}.{named(function)}")
    if tuple(sys.path) != held.path:
        left.extend(f"sys.path.{entry}" for entry in sys.path if entry not in held.path)
        sys.path[:] = held.path
    installed = environment()
    for name in sorted(sys.modules.keys() - held.modules):
        match getattr(sys.modules[name], "__file__", None):
            case str(origin) if not any(Path(origin).is_relative_to(root) for root in installed):
                left.append(f"modules.{name}")
                if not origin.endswith(tuple(EXTENSION_SUFFIXES)):
                    del sys.modules[name]
            case _:
                pass
    left.extend(f"preferences.{path}" for path in restored(held.preferences) if path not in expected)
    return left


# --- [PRODUCT]


def raised(error: BaseException, /) -> None:
    """Raises an `addon_utils` failure in place of printing it."""
    raise error


def declares(path: Path, /) -> bool:
    """Returns whether a legacy add-on source assigns `bl_info` at module level, read through its syntax tree."""
    source = path / "__init__.py" if path.is_dir() else path
    return source.is_file() and any(
        isinstance(node, ast.Assign) and any(isinstance(target, ast.Name) and target.id == "bl_info" for target in node.targets) for node in ast.parse(source.read_bytes()).body
    )


def addon(path: Path, repository: Path, stack: ExitStack, /) -> str | None:
    """Links an extension folder into the batch repository or a `bl_info` source onto the user add-on path, returning the module Blender lists for the link, and nothing for a plain script."""
    manifest: str = addon_utils._ext_manifest_filename_toml  # type: ignore[attr-defined]  # ruff: ignore[private-member-access]  # ty: ignore[unresolved-attribute]
    match (path / manifest).is_file(), declares(path):
        case True, _:
            link = repository / path.name
        case False, True:
            link = Path(bpy.utils.user_resource("SCRIPTS", path="addons", create=True), path.name)
        case _:
            return None
    link.symlink_to(path)
    stack.callback(link.unlink)
    if link.parent == repository:
        addon_utils.extensions_refresh(ensure_wheels=True, handle_error=raised)
    else:
        addon_utils.modules_refresh()
    modules: Iterable[ModuleType] = addon_utils.modules()  # type: ignore[assignment, func-returns-value]  # ty: ignore[invalid-assignment]
    return next((module.__name__ for module in modules if module.__file__ is not None and Path(module.__file__).resolve().is_relative_to(path.resolve())), None)


def disabled(module: str, /) -> None:
    """Disables an add-on and drops its modules, so the next case imports it fresh."""
    addon_utils.disable(module, default_set=True, handle_error=raised)
    for name in [name for name in sys.modules if name == module or name.startswith(f"{module}.")]:
        del sys.modules[name]


def enabled(path: Path, repository: Path, stack: ExitStack, /) -> Namespace | None:
    """Enables an extension or legacy add-on as Blender enables one, disabling it and dropping its modules when the case ends, and returns nothing for a plain script."""
    match addon(path, repository, stack):
        case str(module):
            stack.callback(disabled, module)
            addon_utils.enable(module, default_set=True, handle_error=raised)
            return vars(sys.modules[module])
        case None:
            return None


def installed(owner: Path, name: str, /) -> Path:
    """Returns the folder of one extension installed in the owner's repositories."""
    manifest: str = addon_utils._ext_manifest_filename_toml  # type: ignore[attr-defined]  # ruff: ignore[private-member-access]  # ty: ignore[unresolved-attribute]
    match sorted(owner.glob(f"extensions/*/{glob.escape(name)}/{manifest}")):
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


# --- [PRECONDITIONS]


def document(row: Literal["none"] | Document, folder: Path, made: Namespace, /) -> None:
    """Loads the case document into the case folder: a factory home file for `none`, a copy of `open`, or an empty file saved in its working color space, its scene members written."""
    filepaths = preferences().filepaths
    match row:
        case "none":
            bpy.ops.wm.read_homefile(use_factory_startup=True, load_ui=filepaths.use_load_ui)
        case {"open": str(source)}:
            copy = folder / Path(source).name
            shutil.copyfile(source, copy)
            options = {"load_ui": filepaths.use_load_ui, "use_scripts": filepaths.use_scripts_auto_execute, **row.get("options", {})}
            bpy.ops.wm.open_mainfile(filepath=str(copy), **options)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            written(manager().windows[0].scene, row.get("scene", {}), made)
        case {"working_space": str(space), "scene": {"render": dict(render)} as scene}:
            bpy.ops.wm.read_homefile(use_empty=True, load_ui=filepaths.use_load_ui)
            bpy.ops.wm.set_working_color_space(working_space=space)
            written(manager().windows[0].scene, {**scene, "render": {"filepath": "//", **render}}, made)
            bpy.ops.wm.save_as_mainfile(filepath=str(folder / "case.blend"))
        case _:
            raise ValueError(f"{row} names no document")


def loaded(precondition: Precondition, folder: Path, made: Namespace, /) -> Generator[None]:
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
        yield


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


def executed(case: Case, product: Namespace | None, names: Namespace, made: Namespace, /) -> dict[str, Json]:
    """Runs the case body and returns its value: an expression, a call on decoded arguments, or a script run whole, which an enabled add-on's registration already ran."""
    if case["_tag"] == "Probe":
        return {"value": buried(evaluated(case["evaluate"], names))}
    if case["_tag"] == "Call":
        callee: Callee = evaluated(case["call"], {**(product or {}), **names})  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        args = [unburied(arg, made) for arg in case["args"]]
        keywords = {key: unburied(value, made) for key, value in case["keywords"].items()}
        return {"value": buried(callee(*args, **keywords))}
    if product is None:
        runpy.run_path(case["script"], run_name="__main__")
    return {}


def ran(case: Case, product: Namespace | None, names: Namespace, made: Namespace, override: Override, /) -> dict[str, Json]:
    """Runs the case body under the override, a raise or `SystemExit` becoming its completion."""
    try:
        with bpy.context.temp_override(**override):
            return {"completion": {"_tag": "Ran"}, **executed(case, product, names, made)}
    except SystemExit as error:
        return {"completion": {"_tag": "Exited"}, "value": buried(error.code)}
    except Exception as error:
        return {"completion": thrown(error)}


def pressed(window: bpy.types.Window, kind: str, /) -> Generator[None]:
    """Posts one key press and release to the window, then passes the event loop so the popup it answers handles it."""
    window.event_simulate(type=kind, value="PRESS")  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    window.event_simulate(type=kind, value="RELEASE")  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    yield


def settled(timers: Timers, window: bpy.types.Window, deadline: float, /) -> Generator[None, None, list[Json]]:
    """Evaluates the view layer, then passes the event loop until no job runs and every case timer fell due and ran once, returning the jobs still running at the deadline, a background run holding no event loop to pass."""
    window.view_layer.update()
    due = max((at for _, at in timers), default=0.0)
    while True:
        running: list[Json] = [job for job in jobs() if bpy.app.is_job_running(job)]  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        if bpy.app.background or (not running and time.monotonic() >= due) or time.monotonic() >= deadline:
            yield
            return running
        yield


def picked(rows: list[Picked | None], folder: Path, /) -> Generator[None, None, list[Json]]:
    """Answers each file browser window in order with the next `files` row, a case folder path through `file.execute` and a null or missing row through `file.cancel`, then closes the windows past the first, returning each window's area types and path given."""
    answers, shown = iter(rows), list[Json]()
    for window in manager().windows[1:]:
        areas: list[Json] = [area.type for area in window.screen.areas]
        match next((area for area in window.screen.areas if area.type == "FILE_BROWSER"), None), next(answers, None) if "FILE_BROWSER" in areas else None:
            case bpy.types.Area() as area, {"path": str(relative)}:
                target = folder / relative
                params: bpy.types.FileSelectParams = area.spaces.active.params  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                params.directory, params.filename = str(target.parent).encode(), target.name
                yield
                with bpy.context.temp_override(window=window, area=area):
                    bpy.ops.file.execute()
                shown.append({"areas": areas, "picked": relative})
            case bpy.types.Area() as area, None:
                with bpy.context.temp_override(window=window, area=area):
                    bpy.ops.file.cancel()
                shown.append({"areas": areas, "picked": None})
            case _:
                shown.append({"areas": areas})
    yield
    closed()
    return shown


def closed() -> None:
    """Closes every window past the first under its own override."""
    for window in manager().windows[1:]:
        with bpy.context.temp_override(window=window):
            bpy.ops.wm.window_close()


def pushed() -> list[str]:
    """Returns the undo step names, none before the first push."""
    return [step.name for step in stack.steps] if (stack := manager().undo_stack) is not None else []


def writes(before: Mapping[int, str], steps: int, updates: Updates, /) -> dict[str, Json]:
    """Returns what the case wrote: IDs created and removed by data path, IDs the depsgraph updated with each update kind, undo steps pushed, and the file's edited state."""
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


def arranged(case: Case, instance: Instance, folder: Path, stack: ExitStack, /) -> Generator[None, None, tuple[Override, Namespace, Namespace, Namespace | None]]:
    """Copies the case's input files, enables its extensions and add-on, loads its document, and applies its preconditions, then passes the event loop once."""
    for row in case["files"]:
        match row:
            case {"path": str(relative), "source": str(source)}:
                target = folder / relative
                target.parent.mkdir(parents=True, exist_ok=True)
                (shutil.copytree if Path(source).is_dir() else shutil.copyfile)(source, target)
            case _:
                pass
    precondition, made = case["precondition"], dict[str, object]()
    for name in precondition.get("extensions", []):
        enabled(installed(instance.owner, name), instance.repository, stack)
    match case:
        case {"script": str(source)}:
            script: Path | None = Path(source)
        case _:
            script = None
    product = enabled(script, instance.repository, stack) if script is not None else None
    if not bpy.app.background:
        stack.callback(concealed)
    stack.callback(closed)
    yield from loaded(precondition, folder, made)
    override = overridden(precondition.get("area"))
    names: Namespace = {"bpy": bpy, "context": bpy.context, "scene": override["window"].scene, "made": made}
    prepared(precondition, override, names, made)
    if case["_tag"] == "Call" and script is not None and product is None:
        product = runpy.run_path(str(script), run_name=script.stem)
    yield
    return override, names, made, product


def acted(case: Case, instance: Instance, folder: Path, stack: ExitStack, timers: Timers, ready: tuple[Override, Namespace, Namespace, Namespace | None], /) -> Generator[None, None, dict[str, Json]]:
    """Runs, answers, settles, and reads one arranged case, recording every warning it raised and every ID the depsgraph updated."""
    override, names, made, product = ready
    deadline, state = time.monotonic() + instance.session["limit"], documented(override)
    caught = stack.enter_context(warnings.catch_warnings(record=True))
    warnings.simplefilter("always")
    instance.updates.clear()
    before, unread = reads(case, override, names)
    ids, steps = {item.session_uid: item.path_from_module() for item in bpy.data.all_ids}, len(pushed())
    completion = ran(case, product, names, made, override)
    for answer in case["confirms"]:
        yield from pressed(manager().windows[0], "RET" if answer else "ESC")
    running = yield from settled(timers, manager().windows[0], deadline)
    modal: list[Json] = [operator.bl_idname for window in manager().windows for operator in window.modal_operators]
    windows = yield from picked(case["files"], folder)
    if not bpy.app.background:
        yield from pressed(manager().windows[0], "ESC")
    names["depsgraph"] = manager().windows[0].view_layer.depsgraph
    after, failed = reads(case, override, names)
    readings: dict[str, Json] = {"before": before, "read": after} if case["read"] else {}
    refusals: dict[str, Json] = {**({"beforeFailed": unread} if unread else {}), **({"readFailed": failed} if failed else {})}
    changes = writes(ids, steps, instance.updates)
    return {**completion, **readings, **refusals, "document": state, "writes": changes, "modal": modal, "jobs": running, "windows": windows, "warnings": [warned(item) for item in caught]}


def stepped(instance: Instance, index: int, case: Case, stream: IO[str], /) -> Generator[None]:
    """Runs one case to its outcome line, its output and files read from after its preconditions, a failure anywhere recorded as `Failed`, and undoes what the case left registered."""
    session = instance.session
    folder = Path(session["results"]).parent / str(index)
    folder.mkdir()
    recorded(stream, {"_tag": "Started", "name": case["name"]})
    held, start, files = holding(), offsets(session), listed(folder)
    body: dict[str, Json]
    with ExitStack() as stack:
        timers = timed(stack)
        try:
            ready = yield from arranged(case, instance, folder, stack)
            start, files = offsets(session), listed(folder)
            body = yield from acted(case, instance, folder, stack, timers, ready)
        except (Exception, SystemExit) as error:
            body = {"completion": {"_tag": "Failed", "reason": described(error)}}
    expected = frozenset(f"view.{name}" for name in translated(case["precondition"]["language"])) if "language" in case["precondition"] else frozenset()
    leaked: list[Json] = [*leftover(timers, held, expected)]
    changed: dict[str, Json] = {path: digested(folder / path) for path, stamp in listed(folder).items() if files.get(path) != stamp}
    recorded(stream, {"_tag": "Finished", "outcome": {"name": case["name"], **body, **printed(session, start), "files": changed, "leaked": leaked}})


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
    }


def cases(instance: Instance, stream: IO[str], /) -> Generator[None]:
    """Records the host facts, writes the RNA dictionary a session asks for, and steps every case, a raise inside one recorded as that case's failure."""
    recorded(stream, {"_tag": "Host", "host": host()})
    if (target := instance.session.get("dictionary")) is not None:
        Path(target).write_text(json.dumps(dictionary(), allow_nan=False), encoding="utf-8")
        yield
    for index, case in enumerate(instance.session["cases"]):
        try:
            yield from stepped(instance, index, case, stream)
        except (Exception, SystemExit) as error:
            recorded(stream, {"_tag": "Finished", "outcome": {"name": case["name"], "completion": {"_tag": "Failed", "reason": described(error)}}})


def batch(instance: Instance, /) -> Generator[None]:
    """Runs every case, a raise outside every case recorded as the batch failure."""
    with Path(instance.session["results"]).open("a", encoding="utf-8") as stream:
        try:
            yield from cases(instance, stream)
        except (Exception, SystemExit) as error:
            recorded(stream, {"_tag": "Failed", "reason": described(error)})


def quitted(steps: Generator[None], /) -> Generator[None]:
    """Runs the batch, then quits Blender whatever raises."""
    try:
        yield from steps
    finally:
        with bpy.context.temp_override(window=manager().windows[0]):
            bpy.ops.wm.quit_blender()


def stepping(steps: Generator[None], /) -> float | None:
    """Advances the batch one step per timer pass, unregistering the timer once the batch ends."""
    match next(steps, False):
        case None:
            return 0.0
        case _:
            return None


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Moves process output into the session's files, dumps Python stacks on SIGUSR1, and records depsgraph updates, then runs the batch to its end in a background run, else keeps Blender inactive and hidden and steps the batch on a persistent timer."""
    session: Session = json.loads(Path(sys.argv[sys.argv.index("--") + 1]).read_text(encoding="utf-8"))
    for descriptor, name in ((1, session["stdout"]), (2, session["stderr"])):
        os.dup2(os.open(name, os.O_WRONLY | os.O_CREAT | os.O_APPEND), descriptor)
    faulthandler.register(signal.SIGUSR1, all_threads=True)  # ty: ignore[possibly-missing-attribute]
    sys.pycache_prefix = os.environ.get("PYTHONPYCACHEPREFIX")
    held = preferences()
    held.view.show_splash = False
    held.view.use_save_prompt = False
    repository = Path(tempfile.mkdtemp())
    held.extensions.repos.new(name=REPOSITORY, module=REPOSITORY, custom_directory=str(repository), source="USER")
    updates: Updates = []

    @bpy.app.handlers.persistent  # type: ignore[untyped-decorator]  # ty: ignore[dynamic-function-decorator-return]
    def noted(_scene: bpy.types.Scene, depsgraph: bpy.types.Depsgraph, /) -> None:
        updates.extend(
            (located(origin, repr(origin)), frozenset(flag for flag in FLAGS if getattr(update, f"{UPDATED}{flag}")))
            for update in depsgraph.updates
            if (source := update.id) is not None and (origin := source.original) is not None
        )

    bpy.app.handlers.depsgraph_update_post.append(noted)
    steps = batch(Instance(session=session, repository=repository, owner=owned(), updates=updates))
    if bpy.app.background:
        gpu.init()
        deque(steps, maxlen=0)
    else:
        inert()
        concealed()
        bpy.app.timers.register(partial(stepping, quitted(steps)), persistent=True)


main()
