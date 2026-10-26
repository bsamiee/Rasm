"""Entry inputs by interface key with their captures, restores, and defaults, and scene holds stored as JSON beside owned data."""

from collections.abc import Mapping
from typing import Final

import bpy
from expression import Error, Ok, Option, Result
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor, Native, Struct, struct_at, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.interface import addresses, item_key, slot
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.trees import Owner, Tree

# --- [MODELS] ---------------------------------------------------------------------------


class JsonSlot[T](msgspec.Struct, frozen=True, kw_only=True):
    """Typed JSON text in an ID property under the extension's attachment name, read as `{}` while absent."""

    key: Key
    form: type[T]

    def read(self, identity: Identity, holder: bpy.types.ID) -> Outcome[T]:
        """Decodes the slot of holder."""
        return identity.name(Name.ATTACHMENT, self.key).bind(
            lambda name: catch(msgspec.json.decode, exception=msgspec.DecodeError)(holder.get(name, b"{}"), type=self.form).map_error(
                lambda error: (Fault(reason=RecordReason.UNREADABLE, subject=repr(holder), cause=error),)
            )
        )

    def written(self, identity: Identity, holder: bpy.types.ID, value: T) -> Outcome[bool]:
        """Writes value as deterministic JSON text when it differs from the value the slot reads, tagging the holder for its evaluated copies, answering whether it wrote."""
        text = msgspec.json.encode(value, order="deterministic")
        return paired(self.read(identity, holder), identity.name(Name.ATTACHMENT, self.key)).bind(
            lambda held: (
                Ok(False)
                if msgspec.json.encode(held[0], order="deterministic") == text
                else catch(holder.__setitem__, exception=TypeError)(held[1], text.decode())
                .map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(holder), cause=error),))
                .map(lambda _: holder.update_tag())
                .map(lambda _: True)
            )
        )


# --- [SLOTS] ----------------------------------------------------------------------------

HELD: Final = JsonSlot(key="held", form=dict[Key, Snapshot])

# --- [OPERATIONS] -----------------------------------------------------------------------


def keyed(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[tuple[bpy.types.NodeTree, Mapping[Key, str]]]:
    """Returns the entry's group with its live input socket identifiers by interface item key, panel keys at their toggles."""
    return Result[bpy.types.NodeTree, Faults].bind(
        Option.of_optional(entry.node_group).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(entry), member="node_group"),)),
        lambda group: slot(identity).map(lambda name: (group, addresses(group, name))),
    )


def entry_input(identity: Identity, entry: bpy.types.SceneCompositorEffect, key: Key) -> Outcome[Struct]:
    """Resolves live entry input by interface item key after interface rebuilds."""
    return (
        keyed(identity, entry)
        .bind(lambda held: Option.of_optional(held[1].get(key)).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(entry), member=key),)))
        .bind(lambda address: interface_input(entry, address))
    )


def inputs_of(entry: bpy.types.SceneCompositorEffect) -> Outcome[Native]:
    """Resolves the entry's generated `properties.inputs`, absent without a group."""
    return struct_at(entry, "properties.inputs").to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(entry), member="properties.inputs"),))


def interface_input(entry: bpy.types.SceneCompositorEffect, identifier: str) -> Outcome[Struct]:
    """Resolves live entry input by interface socket identifier."""
    match struct_at(entry, f"properties.inputs.{identifier}").to_optional():
        case bpy.types.bpy_struct() as value:
            return Ok(value)
        case _:
            return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(entry), member=identifier),))


def captured(identity: Identity, entry: bpy.types.SceneCompositorEffect, prefix: Key = "") -> Outcome[Snapshot]:
    """Captures the entry's input values at the item keys under a prefix, every key while it is empty, each input owner named by its item key."""

    def renamed(snapshot: Snapshot, keys: Mapping[Key, str]) -> Snapshot:
        named = {identifier: key for key, identifier in keys.items() if not prefix or key == prefix or key.startswith(f"{prefix}_")}
        owners = snapshot.tree.owners
        held = {named[identifier]: owner for identifier, owner in owners.items() if identifier in named}
        return msgspec.structs.replace(snapshot, tree=Tree(roots=("",), owners={"": Owner(values={}, children=tuple(held)), **held}))

    return paired(inputs_of(entry).bind(Snapshot.capture), keyed(identity, entry)).map(lambda pair: renamed(pair[0], pair[1][1]))


def rebased(values: Snapshot, source: Key, target: Key) -> Snapshot:
    """Returns captured input values with the item keys under `source` moved under `target`, the empty key holding every item key."""

    def moved(key: Key) -> Key:
        return item_key(*(part for part in (target, key.removeprefix(source).removeprefix("_")) if part)) if not source or key == source or key.startswith(f"{source}_") else key

    held = {moved(key): owner for key, owner in values.tree.owners.items() if key}
    return msgspec.structs.replace(values, tree=Tree(roots=("",), owners={"": Owner(values={}, children=tuple(held)), **held}))


def restored(identity: Identity, entry: bpy.types.SceneCompositorEffect, values: Snapshot) -> Outcome[bool]:
    """Writes captured input values at the entry's item keys they name, answering whether any changed."""

    def placed(inputs: Native, keys: Mapping[Key, str]) -> Outcome[bool]:
        moved = {keys[key]: owner for key, owner in values.tree.owners.items() if key in keys}
        applied = msgspec.structs.replace(values, tree=Tree(roots=("",), owners={"": Owner(values={}, children=tuple(moved)), **moved})).apply(inputs)
        return applied.outcome.map(lambda _: bool(applied.written))

    return paired(inputs_of(entry), keyed(identity, entry)).bind(lambda pair: placed(pair[0], pair[1][1]))


def defaulted(identity: Identity, entry: bpy.types.SceneCompositorEffect, prefix: Key = "") -> Outcome[bool]:
    """Writes each group socket's interface default into the entry's inputs at the item keys under a prefix, every key while it is empty, answering whether any changed."""

    def reset(group: bpy.types.NodeTree, keys: Mapping[Key, str]) -> Outcome[bool]:
        sockets = {item.identifier: item for item in group.interface.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket) and item.in_out == "INPUT"}  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        defaults = {
            identifier: descriptor.read(sockets[identifier])
            for key, identifier in keys.items()
            if (not prefix or key == prefix or key.startswith(f"{prefix}_")) and identifier in sockets
            for descriptor in Descriptor.resolve(sockets[identifier], "default_value", bpy.types.Property).to_option().to_list()
        }
        return accumulated(interface_input(entry, identifier).bind(lambda held: written(held, "value", value)).map(Option.is_some) for identifier, value in defaults.items()).map(any)

    return keyed(identity, entry).bind(lambda held: reset(*held))


def path(identity: Identity, entry: bpy.types.SceneCompositorEffect, key: Key) -> Outcome[str]:
    """Returns animation data path of entry input value."""
    return entry_input(identity, entry, key).map(lambda value: value.path_from_id("value"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["HELD", "JsonSlot", "captured", "defaulted", "entry_input", "inputs_of", "interface_input", "keyed", "path", "rebased", "restored"]
