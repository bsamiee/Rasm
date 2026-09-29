# mypy: disable-error-code="attr-defined, union-attr, arg-type"
# ty: ignore[unresolved-attribute, invalid-argument-type]
"""Operators, RNA types, and add-on settings matching words across stock Blender and every enabled add-on."""

import ast
from collections.abc import Mapping
from enum import auto, StrEnum
import inspect
from itertools import accumulate
from pathlib import Path
import textwrap

import attrs
import bpy
from rna import BPyOpFunction, operators, plain
from scene import viewport

# --- [TYPES] ----------------------------------------------------------------------------


class Tool(StrEnum):
    """MCP tool that answers the next question about a hit."""

    GET_PYTHON_API_DOCS = auto()
    BPY_API_LOOKUP = auto()


# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Operator:
    """Registered operator with the facts that decide its call."""

    call: str
    label: str
    description: str
    owner: str | None
    params: Mapping[str, str]
    poll: bool
    poll_in_view: bool | None
    reads: tuple[str, ...]
    source: str | None
    next: Tool


@attrs.frozen
class Type:
    """Registered RNA type outside operators, with the add-on and file defining it."""

    identifier: str
    label: str
    owner: str | None
    source: str | None
    next: Tool


@attrs.frozen
class Setting:
    """Runtime property an add-on registered on an ID type, with its value on each context member of the type by ID name."""

    path: str
    type: str
    values: dict[str, object]


@attrs.frozen
class Discovery:
    """Hits for the words, grouped by kind."""

    operators: tuple[Operator, ...]
    types: tuple[Type, ...]
    settings: tuple[Setting, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def discover(*words: str) -> Discovery:
    """Operators, RNA types, and add-on settings with every word in their text, polls answered in the calling context and in the largest 3D Viewport when a window shows one."""
    needles = tuple(word.casefold() for word in words)
    addons = {key: addon.module for addon in bpy.context.preferences.addons for key in {addon.module, addon.module.rpartition(".")[2]}}
    resources = Path(bpy.utils.resource_path("LOCAL"))
    members = tuple(bpy.context.copy().values())
    methods = {name for name in dir(bpy.types.Context) if callable(getattr(bpy.types.Context, name))}
    largest = viewport()
    view = None if largest is None else bpy.context.temp_override(window=largest.window, area=largest.area, region=largest.region)

    def matches(text: str) -> bool:
        return all(needle in text.casefold() for needle in needles)

    def descendants(cls: "type[bpy.types.bpy_struct[object]]") -> "set[type[bpy.types.bpy_struct[object]]]":
        return {cls, *(found for sub in cls.__subclasses__() for found in descendants(sub))}

    def own(struct: bpy.types.Struct) -> tuple[bpy.types.Property, ...]:
        """Properties the struct declares, its base's left out."""
        return tuple(p for p in struct.properties if struct.base is None or p.identifier not in struct.base.properties)

    def owner(cls: type) -> str | None:
        """Enabled add-on with a module or extension id naming the package holding the class, none for Blender's own classes and executed code."""
        return next((addons[package] for package in accumulate(cls.__module__.split("."), lambda head, part: f"{head}.{part}") if package in addons), None)

    def source(cls: type) -> str | None:
        """File defining the class, none for a compiled class or one from executed code."""
        try:
            return inspect.getsourcefile(cls)
        except TypeError:
            return None

    def tool(cls: type, who: str | None, file: str | None) -> Tool:
        """Bundled docs for a compiled class or one Blender ships outside its add-ons, live RNA for add-on and executed code."""
        bundled = cls.__module__ == bpy.types.__name__ or (file is not None and Path(file).is_relative_to(resources))
        return Tool.GET_PYTHON_API_DOCS if who is None and bundled else Tool.BPY_API_LOOKUP

    def reads(cls: type) -> tuple[str, ...]:
        """Deepest context chains up to two members the class's source reads, none for a class Python holds no source lines for."""
        try:
            tree = ast.parse(textwrap.dedent(inspect.getsource(cls)))
        except (OSError, TypeError):
            return ()
        dotted = (ast.unparse(node).removeprefix("bpy.").split(".")[:3] for node in ast.walk(tree) if isinstance(node, ast.Attribute))
        chains = {".".join(parts) for parts in dotted if len(parts) > 1 and parts[0] == "context" and parts[1] not in methods and all(map(str.isidentifier, parts))}
        return tuple(sorted(chain for chain in chains if not any(other.startswith(f"{chain}.") for other in chains)))

    def in_view(entry: BPyOpFunction) -> bool | None:
        if view is None:
            return None
        with view:
            return entry.poll()

    def operator(entry: BPyOpFunction) -> Operator | None:
        rna, call = entry.get_rna_type(), f"bpy.ops.{entry.idname_py()}"
        if not matches(f"{call} {rna.name} {rna.description}"):
            return None
        cls = bpy.types.Operator.bl_rna_get_subclass_py(entry.idname())
        who, file = (None, None) if cls is None else (owner(cls), source(cls))
        chains, following = ((), Tool.GET_PYTHON_API_DOCS) if cls is None else (reads(cls), tool(cls, who, file))
        params = {p.identifier: p.type for p in own(rna)}
        return Operator(call, rna.name, rna.description, who, params, entry.poll(), in_view(entry), chains, file, following)

    def rna_type(cls: "type[bpy.types.bpy_struct[object]]") -> Type | None:
        rna = cls.bl_rna
        if issubclass(cls, bpy.types.Operator | bpy.types.Macro | bpy.types.OperatorProperties) or not matches(f"{rna.identifier} {rna.name} {rna.description}"):
            return None
        who, file = owner(cls), source(cls)
        return Type(rna.identifier, rna.name, who, file, tool(cls, who, file))

    def setting(host: type[bpy.types.ID], prop: bpy.types.Property) -> Setting | None:
        path = f"{host.bl_rna.identifier}.{prop.identifier}"
        if not matches(f"{path} {prop.name} {prop.description}"):
            return None
        kind = prop.fixed_type.identifier if isinstance(prop, bpy.types.PointerProperty | bpy.types.CollectionProperty) else prop.type
        return Setting(path, kind, {member.name: plain(getattr(member, prop.identifier)) for member in members if isinstance(member, host)})

    named = (cls for name in dir(bpy.types) if isinstance(cls := getattr(bpy.types, name), type) and issubclass(cls, bpy.types.bpy_struct))
    structs = sorted((cls for cls in {*named, *descendants(bpy.types.bpy_struct)} if "bl_rna" in vars(cls)), key=lambda cls: cls.bl_rna.identifier)
    hits = map(operator, operators())
    types = map(rna_type, structs)
    settings = (setting(host, prop) for host in structs if issubclass(host, bpy.types.ID) for prop in own(host.bl_rna) if prop.is_runtime)
    return Discovery(tuple(filter(None, hits)), tuple(filter(None, types)), tuple(filter(None, settings)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Discovery", "Operator", "Setting", "Tool", "Type", "discover"]
