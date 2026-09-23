# ast-grep-ignore: no-parenthesized-except-tuple, no-stdlib-record
# mypy: disable-error-code="unreachable"
"""Find operators, RNA types, and add-on settings matching words across stock Blender and every enabled add-on, run inside Blender through `runpy.run_path`."""

import ast
from contextlib import AbstractContextManager
from dataclasses import asdict, dataclass, fields
from enum import auto, StrEnum
import inspect
from itertools import accumulate
from operator import itemgetter
from pathlib import Path
import sys
import textwrap
from typing import cast

import bpy
import numpy as np

# --- [TYPES] ----------------------------------------------------------------------------


class Tool(StrEnum):
    """MCP tool that answers the next question about a hit."""

    GET_PYTHON_API_DOCS = auto()
    BPY_API_LOOKUP = auto()


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True)
class Operator:
    """Registered operator with the facts that decide its call."""

    call: str
    label: str
    description: str
    owner: str | None
    params: dict[str, str]
    poll: bool
    poll_in_view: bool
    reads: tuple[str, ...]
    source: str | None
    next: Tool


@dataclass(frozen=True)
class Type:
    """Registered RNA type outside operators, with the add-on and file defining it."""

    identifier: str
    label: str
    owner: str | None
    source: str | None
    next: Tool


@dataclass(frozen=True)
class Setting:
    """Runtime property an add-on registered on an ID type, with its fields' values on the context's instance of that type."""

    path: str
    group: str
    values: dict[str, object] | None
    next: Tool


@dataclass(frozen=True)
class Discovery:
    """Every hit for the words, one tuple per surface."""

    operators: tuple[Operator, ...]
    types: tuple[Type, ...]
    settings: tuple[Setting, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def discover(*words: str) -> Discovery:
    """Operators, RNA types, and add-on settings whose text holds every word, polls answered in the timer context and in the largest 3D Viewport."""
    needles = tuple(word.lower() for word in words)
    addons = {key: addon.module for addon in cast("bpy.types.Preferences", bpy.context.preferences).addons for key in {addon.module, addon.module.rpartition(".")[2]}}
    resources = Path(bpy.utils.resource_path("LOCAL"))
    members = tuple(bpy.context.copy().values())
    create = vars(bpy.ops)["_ops_module"].create_function
    arguments = set(bpy.types.OperatorProperties.bl_rna.properties.keys())
    inherited = set(bpy.types.PropertyGroup.bl_rna.properties.keys())
    methods = {name for name in dir(bpy.types.Context) if callable(getattr(bpy.types.Context, name))}
    viewports = (
        (area.width * area.height, window, area, region)
        for manager in bpy.data.window_managers
        for window in manager.windows
        for area in window.screen.areas
        if isinstance(area.spaces.active, bpy.types.SpaceView3D)
        for region in area.regions
        if region.type == "WINDOW"
    )
    match max(viewports, key=itemgetter(0), default=None):
        case (_, window, area, region):
            override = bpy.context.temp_override(window=window, area=area, region=region)
        case None:
            override = bpy.context.temp_override()
    view = cast("AbstractContextManager[None]", override)

    def matches(text: str) -> bool:
        lowered = text.lower()
        return all(needle in lowered for needle in needles)

    def owner(cls: type) -> str | None:
        """Enabled add-on whose module or extension id names a package holding the class, none for Blender's own and agent code."""
        packages = (sys.modules.get(package) for package in accumulate(cls.__module__.split("."), lambda head, part: f"{head}.{part}"))
        return next((addons[package.__name__] for package in packages if package and package.__name__ in addons), None)

    def source(cls: type) -> str | None:
        """File defining the class, none for a compiled class or one from agent code."""
        try:
            return inspect.getsourcefile(cls)
        except TypeError:
            return None

    def tool(cls: type, who: str | None, file: str | None) -> Tool:
        """Bundled docs for a compiled class or one Blender ships outside its add-ons, live RNA for add-on and agent code."""
        bundled = cls.__module__ == bpy.types.__name__ or (file is not None and Path(file).is_relative_to(resources))
        return Tool.GET_PYTHON_API_DOCS if who is None and bundled else Tool.BPY_API_LOOKUP

    def reads(cls: type) -> tuple[str, ...]:
        """Deepest context chains up to two members the class's source reads, none for a class whose source Python cannot find."""
        try:
            tree = ast.parse(textwrap.dedent(inspect.getsource(cls)))
        except (OSError, TypeError):
            return ()
        dotted = (ast.unparse(node).removeprefix("bpy.").split(".")[:3] for node in ast.walk(tree) if isinstance(node, ast.Attribute))
        chains = {".".join(parts) for parts in dotted if len(parts) > 1 and parts[0] == "context" and parts[1] not in methods and all(map(str.isidentifier, parts))}
        return tuple(sorted(chain for chain in chains if not any(other.startswith(f"{chain}.") for other in chains)))

    def value(raw: object) -> object:
        """JSON form of an RNA value, a data-block by name, another struct by its type, a collection by its length, arrays as nested lists."""
        match raw:
            case bpy.types.ID():
                return raw.name
            case bpy.types.bpy_struct():
                return type(raw).__name__
            case bpy.types.bpy_prop_collection():
                return len(raw)
            case set():
                return sorted(raw)
            case str() | int() | float() | bool() | None:
                return raw
            case _:
                return np.asarray(raw).tolist()

    def operator(category: str, name: str) -> Operator | None:
        entry = create(category, name)
        rna, call = entry.get_rna_type(), f"bpy.ops.{category}.{name}"
        if not matches(f"{call} {rna.name} {rna.description}"):
            return None
        match bpy.types.Operator.bl_rna_get_subclass_py(entry.idname()):
            case type() as cls:
                who, chains, file = owner(cls), reads(cls), source(cls)
                following = tool(cls, who, file)
            case _:
                who, chains, file, following = None, (), None, Tool.GET_PYTHON_API_DOCS
        with view:
            poll_in_view = entry.poll()
        params = {p.identifier: p.type for p in rna.properties if p.identifier not in arguments}
        return Operator(call, rna.name, rna.description, who, params, entry.poll(), poll_in_view, chains, file, following)

    def rna_type(identifier: str, cls: type[bpy.types.bpy_struct]) -> Type | None:
        rna = cast("bpy.types.Struct", cls.bl_rna)
        if issubclass(cls, bpy.types.Operator) or not matches(f"{identifier} {rna.name} {rna.description}"):
            return None
        who, file = owner(cls), source(cls)
        return Type(identifier, rna.name, who, file, tool(cls, who, file))

    def setting(host: type[bpy.types.ID], prop: bpy.types.Property) -> Setting | None:
        match prop:
            case bpy.types.PointerProperty(fixed_type=bpy.types.PropertyGroup() as group):
                kind, names = group.identifier, tuple(q.identifier for q in group.properties if q.identifier not in inherited)
            case _:
                group, kind, names = None, prop.type, (prop.identifier,)
        path = f"{host.__name__}.{prop.identifier}"
        if not matches(f"{path} {prop.name} {kind}"):
            return None
        instance = next((m for m in members if isinstance(m, host)), None)
        target = instance if instance is None or group is None else getattr(instance, prop.identifier)
        return Setting(path, kind, None if target is None else {n: value(getattr(target, n)) for n in names}, Tool.BPY_API_LOOKUP)

    operators = (operator(category, name) for category in dir(bpy.ops) for name in dir(getattr(bpy.ops, category)))
    types = (rna_type(name, cls) for name in dir(bpy.types) if isinstance(cls := getattr(bpy.types, name), type) and issubclass(cls, bpy.types.bpy_struct) and cls is not bpy.types.bpy_struct)
    settings = (setting(host, prop) for host in bpy.types.ID.__subclasses__() for prop in host.bl_rna.properties if prop.is_runtime)
    return Discovery(tuple(o for o in operators if o), tuple(t for t in types if t), tuple(s for s in settings if s))


def as_result(value: Discovery) -> dict[str, object]:
    """`result` dict for `execute_blender_code`, the case name under `kind` and a count per surface."""
    return {"kind": type(value).__name__, "counts": {f.name: len(getattr(value, f.name)) for f in fields(value)}, **asdict(value)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Discovery", "Operator", "Setting", "Tool", "Type", "as_result", "discover"]
