"""Planned text edits of File Output and frame node strings and item names, with the stored spellings the host keeps."""

from collections.abc import Callable, Iterable
from functools import partial
from itertools import chain, count
import re
from typing import Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Answer, Call, tallied, Unchanged
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.paths import definition
from blender.bpy.faults import BlenderReason
from blender.bpy.render.outputs.formats import FileItems
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.rendering.outputs.naming import stepped

# --- [MODELS] ---------------------------------------------------------------------------


class Replace(bpy.types.PropertyGroup):
    """Pattern, replacement text, regex mode, and case sensitivity."""

    find: bpy.props.StringProperty(name="Find", description="Text to find in selected paths, labels, and item names")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    replace: bpy.props.StringProperty(name="Replace", description=r"Replacement, regex groups as \1 or \g<name>")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    regex: bpy.props.BoolProperty(name="Regular Expression", description="Read Find as a Python regular expression")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    case: bpy.props.BoolProperty(name="Match Case", description="Match upper and lower case as written")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Bump(bpy.types.PropertyGroup):
    """Version-token offset."""

    offset: bpy.props.IntProperty(name="Offset", description="Versions each token moves by", default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Rename(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """String edit with original, requested, and predicted stored spelling."""

    node: bpy.types.Node
    owner: bpy.types.Node | bpy.types.NodeCompositorFileOutputItem
    prop: bpy.types.StringProperty
    before: str
    requested: str
    stored: str

    @classmethod
    def field(cls, node: bpy.types.Node, prop: bpy.types.StringProperty, before: str, requested: str) -> Self:
        """Returns node string edit, stored spelling cut to member byte capacity."""
        return cls(node=node, owner=node, prop=prop, before=before, requested=requested, stored=requested.encode()[: prop.length_max - 1].decode(errors="ignore") if prop.length_max else requested)


# --- [OPERATIONS] -----------------------------------------------------------------------


def edits(node: bpy.types.CompositorNodeOutputFile | bpy.types.NodeFrame, transform: Callable[[str], Outcome[Option[str]]]) -> Outcome[tuple[Rename, ...]]:
    """Plans node strings and File Output item names a transform changes, item names predicted as the host stores them."""
    texts = tuple(prop for prop in definition(node).properties if isinstance(prop, bpy.types.StringProperty) and (prop.is_path_supports_templates or prop.identifier == "label"))
    items = tuple(
        (item, prop) for item in (node.file_output_items if isinstance(node, bpy.types.CompositorNodeOutputFile) else ()) for prop in Option.of_optional(definition(item).name_property).to_list()
    )
    before, held = tuple(getattr(node, prop.identifier) for prop in texts), tuple(getattr(item, prop.identifier) for item, prop in items)

    def named(asked: tuple[Option[str], ...]) -> tuple[Rename, ...]:
        requests = tuple(request.default_value(name) for request, name in zip(asked, held))
        return tuple(
            Rename(node=node, owner=item, prop=prop, before=text, requested=request, stored=stored)
            for (item, prop), text, request, stored in zip(items, held, requests, FileItems.stored(requests, held))
            if request != text
        )

    return paired(
        accumulated(map(transform, before)).map(lambda asked: tuple(Rename.field(node, prop, text, request) for prop, text, changed in zip(texts, before, asked) for request in changed.to_list())),
        accumulated(map(transform, held)).map(named),
    ).map(lambda pair: (*pair[0], *pair[1]))


def planned(nodes: Iterable[bpy.types.Node], properties: Replace) -> Outcome[tuple[Rename, ...]]:
    """Plans every string edit before writes."""

    def substituted(pattern: re.Pattern[str], text: str) -> Outcome[Option[str]]:
        insert: Callable[[re.Match[str]], str] = (lambda found: found.expand(properties.replace)) if properties.regex else (lambda _: properties.replace)
        return (
            catch(pattern.sub, exception=re.PatternError)(insert, text)
            .map_error(partial(Fault.of, RecordReason.INVALID, properties.replace))
            .map(lambda changed: Some(changed) if changed != text else Nothing)
        )

    return (
        Result[re.Pattern[str], Faults]
        .bind(
            catch(re.compile, exception=re.PatternError)(properties.find if properties.regex else re.escape(properties.find), 0 if properties.case else re.IGNORECASE).map_error(
                partial(Fault.of, RecordReason.INVALID, properties.find)
            ),
            lambda pattern: accumulated(edits(node, partial(substituted, pattern)) for node in nodes if isinstance(node, bpy.types.CompositorNodeOutputFile | bpy.types.NodeFrame)),
        )
        .map(lambda groups: tuple(chain.from_iterable(groups)))
    )


def previewed(context: bpy.types.Context, properties: Replace) -> str:
    """Returns planned spellings or regex fault text for confirmation."""
    return (
        planned(context.selected_nodes, properties)
        .map(lambda renames: "\n".join(f"{rename.node.name} · {rename.before} → {rename.stored}" for rename in renames))
        .default_with(lambda faults: "\n".join(fault.message for fault in faults))
    )


def applied(renames: tuple[Rename, ...], unmatched: str) -> Outcome[Answer]:
    """Writes each request after freeing renamed items for swaps, reporting what the host stored, Unchanged when no stored spelling differs."""
    if not (changed := tuple(rename for rename in renames if rename.stored != rename.before)):
        return Ok(Unchanged(reason=unmatched))
    taken = {
        *(rename.before for rename in changed),
        *(rename.requested for rename in changed),
        *(rename.stored for rename in changed),
        *(item.name for rename in changed if isinstance(rename.node, bpy.types.CompositorNodeOutputFile) for item in rename.node.file_output_items),
    }

    def reported(writes: tuple[Outcome[object], ...]) -> Outcome[Answer]:
        respelled = tuple((rename.requested, stored) for rename in changed for stored in (getattr(rename.owner, rename.prop.identifier),) if stored != rename.requested)
        return tallied(
            writes,
            (
                ("INFO", f"Replaced {len(changed)} fields across {len({rename.node.name for rename in changed})} nodes"),
                *((("WARNING", f"Blender stores {respelled[0][0]} as {respelled[0][1]}, {len(respelled)} names respelled"),) if respelled else ()),
            ),
        )

    return accumulated(
        written(rename.owner, (rename.prop.identifier,), next(f"{index}{'_' * number}" for number in count() if f"{index}{'_' * number}" not in taken))
        for index, rename in enumerate(changed)
        if rename.owner is not rename.node
    ).bind(lambda _: reported(tuple(written(rename.owner, (rename.prop.identifier,), rename.requested) for rename in changed)))


def output_replace(call: Call[Replace]) -> Outcome[Answer]:
    """Replace text in selected paths, labels, and item names."""
    return planned(call.context.selected_nodes, call.properties).bind(lambda renames: applied(renames, "No selected path holds find text"))


def output_version(call: Call[Bump]) -> Outcome[Answer]:
    """Move version tokens in active File Output node."""
    match call.context.active_node:
        case bpy.types.CompositorNodeOutputFile() as node:
            return edits(node, lambda text: Ok(stepped(text, call.properties.offset))).bind(lambda renames: applied(renames, "No version token in node paths"))
        case _:
            return Error(Fault.of(BlenderReason.ABSENT, "bpy.context", member="active_node"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bump", "Rename", "Replace", "applied", "edits", "output_replace", "output_version", "planned", "previewed"]
