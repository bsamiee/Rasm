"""Valued structures shared by entry recipes and stack presets, each entry's record, input values, and enables captured in stack order, and sparse recipes merged into and applied over a scene's entries."""

from collections.abc import Mapping
from contextlib import ExitStack
from types import MappingProxyType
from typing import Final, Generic, TypeVar

import bpy
from expression import Option, Some
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import rna
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.render.compositor.effects import CompositorEffect, Entry
from blender.bpy.render.compositor.entries import Enabled, relocated
from blender.bpy.render.compositor.inputs import captured, hold, PreviewFrame, rebased, regrouped, restored
from blender.bpy.render.compositor.offered import OfferedEffects
from blender.records.faults import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason
from blender.records.fields import Name

# --- [CONSTANTS] ------------------------------------------------------------------------

_NAME: Final[bpy.types.StringProperty] = bpy.types.SceneCompositorEffect.bl_rna.properties["name"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
NAME: Final = Descriptor(root=bpy.types.SceneCompositorEffect, prop=_NAME)

# --- [MODELS] ---------------------------------------------------------------------------

_T_co = TypeVar("_T_co", covariant=True)


class Valued(msgspec.Struct, Generic[_T_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Stack, layer, or entry structure with the group input values captured at the item keys under its prefix and the evaluations enabling it, a stock preset holding no values."""

    structure: _T_co
    values: Snapshot | None = None
    prefix: Key = ""
    enables: frozenset[Enabled] = frozenset(Enabled)

    @property
    def captured(self) -> Option[Snapshot]:
        """Captured input values, absent on a stock preset."""
        return Option.of_optional(self.values)

    def rebased(self, prefix: Key) -> Option[Snapshot]:
        """Returns the captured values at the item keys under `prefix` in place of their own."""
        return self.captured.map(lambda values: rebased(values, self.prefix, prefix))


# --- [OPERATIONS] -----------------------------------------------------------------------


def recorded(rows: OfferedEffects, identity: Identity, scene: bpy.types.Scene) -> Outcome[dict[Name, Valued[Entry]]]:
    """Reads owned entry records, input values, and enables by name in stack order."""
    return accumulated(
        paired(held.row.read(identity, held.entry), captured(identity, held.entry)).map(
            lambda pair: (held.entry.name, Valued(structure=pair[0], values=pair[1], enables=frozenset(enabled for enabled in Enabled if enabled.descriptor.read(held.entry) is True)))
        )
        for held in rows.owned(identity, scene)
    ).map(dict)


def applied(
    rows: OfferedEffects,
    identity: Identity,
    scene: bpy.types.Scene,
    recipe: Mapping[Name, Valued[Entry]],
    *,
    locked: frozenset[Name] = frozenset(),
    fields: Mapping[Name, frozenset[Key]] = MappingProxyType({}),
    preview: Option[PreviewFrame] = Nothing,
) -> Outcome[None]:
    """Applies sparse named records, keeping locked, unlisted, and user entries, the active entry kept active."""

    def merged(current: Mapping[Name, Valued[Entry]]) -> Outcome[None]:
        desired = projected(current, recipe, locked=locked, fields=fields)
        requested = {name: desired[name] for name in recipe if name not in locked}
        native = {entry.name: entry for entry in scene.compositor_effects}
        retained = frozenset(type(value.structure.record) for value in desired.values())
        retired = tuple(dict.fromkeys(type(value.structure.record) for value in current.values() if type(value.structure.record) not in retained))

        def write(offered: tuple[CompositorEffect, ...], restores: tuple[CompositorEffect, ...]) -> Outcome[None]:
            order = tuple(native.values())

            def written_entry(name: Name, value: Valued[Entry], row: CompositorEffect) -> Outcome[tuple[bpy.types.SceneCompositorEffect, CompositorEffect]]:
                return (
                    Option
                    .of_optional(native.get(name))
                    .map(lambda entry: row.write(identity, scene, entry, value.structure, preview=preview).map(lambda _: entry))
                    .default_with(lambda: row.appended(identity, scene, value.structure, preview=preview, name=Some(name)))
                ).bind(
                    lambda entry: paired(
                        value.captured.map(lambda values: restored(identity, entry, values)).default_value(Ok(value=False)),
                        accumulated(enabled.descriptor.write(entry, enabled in value.enables) for enabled in Enabled),
                    ).map(lambda _: (entry, row))
                )

            def arranged(placed: tuple[tuple[bpy.types.SceneCompositorEffect, CompositorEffect], ...]) -> Outcome[None]:
                live = dict(zip(requested, (entry for entry, _ in placed)))
                selected = iter(native.get(name, live[name]) if name in live else native[name] for name in desired)
                target = tuple(next(selected) if entry.name in current else entry for entry in order) + tuple(selected)
                kept = frozenset(native[name] for name in current) | frozenset(live.values())
                return (
                    accumulated(rows.release(identity, scene, row) for row in restores)
                    .bind(lambda _: accumulated(hold(identity, scene, row.kind, row.holds) for _, row in placed))
                    .bind(lambda _: accumulated(relocated(scene, entry, index) for index, entry in enumerate(target) if entry in kept))
                    .map(lambda _: None)
                )

            with ExitStack() as scope, bpy.context.temp_override(scene=scene):
                for prior in Option.of_optional(scene.compositor_effects.active).to_list():
                    scope.callback(setattr, prior, "is_active", True)  # ruff: ignore[boolean-positional-value-in-call]
                return accumulated(written_entry(name, value, row) for (name, value), row in zip(requested.items(), offered)).bind(arranged)

        restores = accumulated(map(rows.offering, retired))
        names = accumulated(
            Descriptor.validated(NAME.prop, name).bind(
                lambda _: Error(Fault.of(Reason.TAKEN, name, member=repr(scene.compositor_effects[name]))) if name not in current and name in scene.compositor_effects else Ok(None)
            )
            for name in requested
        )
        return (
            Ok(None) if not requested else paired(names, paired(accumulated(rows.offering(type(value.structure.record)) for value in requested.values()), restores)).bind(lambda pair: write(*pair[1]))
        )

    return recorded(rows, identity, scene).bind(merged)


def projected(
    current: Mapping[Name, Valued[Entry]], recipe: Mapping[Name, Valued[Entry]], *, locked: frozenset[Name] = frozenset(), fields: Mapping[Name, frozenset[Key]] = MappingProxyType({})
) -> dict[Name, Valued[Entry]]:
    """Merges sparse recipe entries and order into current ones, keeping locked entries whole and, on the others, the input values and enables at their locked keys."""

    def kept(prior: Valued[Entry], value: Valued[Entry], keys: frozenset[Key]) -> Valued[Entry]:
        owners = {
            key: owner
            for held, locking in ((value, False), (prior, True))
            for key, owner in held.captured.map(lambda values: values.tree.rows).default_value(())
            if key and (rna(key) in keys) == locking
        }
        return msgspec.structs.replace(
            value,
            values=value.captured.or_else(prior.captured).map(lambda values: regrouped(values, owners)).to_optional(),
            enables=frozenset(enabled for enabled in Enabled if enabled in (prior.enables if enabled.value in keys else value.enables)),
        )

    changed = {name: kept(current[name], value, fields.get(name, frozenset())) if name in current else value for name, value in recipe.items() if name not in locked}
    ordered = iter(name for name in changed if name in current)
    return {selected: changed[selected] if selected in changed else current[selected] for name in current for selected in (next(ordered) if name in changed else name,)} | {
        name: value for name, value in changed.items() if name not in current
    }


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["NAME", "Valued", "applied", "projected", "recorded"]
