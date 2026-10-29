"""View layer passes: the pass vocabulary, flags of the running build, requests, used layers, presets spread over them, and AOV and light group sync."""

from collections.abc import Callable, Mapping
import enum
import functools
import itertools
from typing import Final, Self

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Answer, tallied, Unchanged
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import rooted, Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.nodes.trees import nested, socket_at
from blender.bpy.scene.materials import ShadingSetup, stored, Utility
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.values import Listing
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Flags = frozenset[Descriptor[bpy.types.BoolProperty]]


class Family(enum.Flag):
    """Light pass family of direct, indirect, and color passes, each a term recombined beauty sums under its own gain, valued by its persisted flag bit."""

    DIFFUSE = 1
    GLOSSY = 2
    TRANSMISSION = 4
    VOLUME = 8

    @property
    def total(self) -> str:
        """Layer name of the family's summed lobes."""
        return f"{bpy.path.display_name(str(self.name).lower())} Total"


class Derived(enum.Flag):
    """Layer built from render passes, named by its member and valued by its persisted flag bit."""

    DIRECT_LIGHT = 1
    INDIRECT_LIGHT = 2
    DEPTH_MAP = 4
    INVERSE_DEPTH = 8


class Entity(enum.Flag):
    """Cryptomatte kind, valued by its persisted flag bit."""

    OBJECT = 1
    MATERIAL = 2
    ASSET = 4

    @property
    def label(self) -> str:
        """Kind name in words."""
        return bpy.path.display_name(str(self.name).lower())

    @property
    def pass_name(self) -> str:
        """Render pass name its ranks extend (`CryptoObject00`)."""
        return f"Crypto{str(self.name).title()}"

    @property
    def flag(self) -> Descriptor[bpy.types.BoolProperty]:
        """View layer flag enabling the kind's passes."""
        return Descriptor.declared(bpy.types.ViewLayer, (f"use_pass_cryptomatte_{str(self.name).lower()}",), bpy.types.BoolProperty)

    @property
    def color(self) -> Descriptor[bpy.types.FloatProperty]:
        """Display color member of the blocks the kind names."""
        return (
            Descriptor.declared(bpy.types.Material, ("diffuse_color",), bpy.types.FloatProperty)
            if self is Entity.MATERIAL
            else Descriptor.declared(bpy.types.Object, ("color",), bpy.types.FloatProperty)
        )

    @classmethod
    def named(cls, name: str) -> Option[Self]:
        """Returns kind with pass name `name`, `Nothing` for another."""
        return Block.of_seq(cls).filter(lambda member: member.pass_name == name).try_head()


# --- [CONSTANTS] ------------------------------------------------------------------------

EMISSION: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_emit",), bpy.types.BoolProperty)
ENVIRONMENT: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_environment",), bpy.types.BoolProperty)
OCCLUSION: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_ambient_occlusion",), bpy.types.BoolProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class PassFlags(msgspec.Struct, frozen=True, kw_only=True):
    """Writable `use_pass_*` flags of `ViewLayer` and its pointer structs in the running build, in RNA order with owning class."""

    every: tuple[tuple[type[Struct], Descriptor[bpy.types.BoolProperty]], ...]

    @classmethod
    @functools.cache
    def of(cls) -> Self:
        """Resolves the flags once per build, after engines register their layer settings."""
        root = bpy.types.ViewLayer.bl_rna
        structs = (
            ((), bpy.types.ViewLayer, root.properties),
            *(
                ((prop.identifier,), type(fixed), fixed.properties)
                for prop in root.properties
                if isinstance(prop, bpy.types.PointerProperty)
                for fixed in Option.of_optional(prop.fixed_type).to_list()
            ),
        )
        return cls(
            every=tuple(
                (owner, Descriptor(root=bpy.types.ViewLayer, owner=path, prop=prop))
                for path, owner, properties in structs
                for prop in properties
                if isinstance(prop, bpy.types.BoolProperty) and not prop.is_readonly and prop.identifier.startswith("use_pass_")
            )
        )

    @property
    def families(self) -> Mapping[Family, tuple[Flags, Flags, Flags]]:
        """Direct, indirect, and color flags of each family, the flags named `use_pass_<family>_<slot>` on any owner."""

        def slot(family: Family, kind: str) -> Flags:
            return frozenset(flag for _, flag in self.every if flag.prop.identifier == f"use_pass_{str(family.name).lower()}_{kind}")

        return {family: (slot(family, "direct"), slot(family, "indirect"), slot(family, "color")) for family in Family}

    @property
    def lobes(self) -> Flags:
        """Every family flag."""
        return frozenset().union(*(slot for slots in self.families.values() for slot in slots))

    def opened(self, totals: Family, derived: Derived, ids: Entity) -> Flags:
        """Returns flags the families summed, the derived layers, and the ID kinds read."""
        families = self.families

        def lit(item: Derived) -> tuple[Flags, ...]:
            match item:
                case Derived.DIRECT_LIGHT:
                    return tuple(direct | color for direct, _, color in families.values())
                case Derived.INDIRECT_LIGHT:
                    return tuple(indirect | color for _, indirect, color in families.values())
                case _:
                    return ()

        return frozenset().union(*(slot for family in totals for slot in families[family]), *(held for item in derived for held in lit(item)), (entity.flag for entity in ids))


class PassRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Passes, condition, and precision a graph needs of one view layer."""

    layer: str
    flags: Flags = frozenset()
    condition: Option[Condition] = Nothing
    precise: bool = False

    @classmethod
    def depth(cls, layer: str, *, precise: bool) -> Self:
        """Returns Depth pass request of a layer."""
        return cls(layer=layer, flags=frozenset({Descriptor.declared(bpy.types.ViewLayer, ("use_pass_z",), bpy.types.BoolProperty)}), precise=precise)


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [LAYERS]


def used(scene: bpy.types.Scene) -> tuple[bpy.types.ViewLayer, ...]:
    """Returns scene layers with `use` on, in list order."""
    return tuple(layer for layer in scene.view_layers if layer.use)


def layer_of(scene: bpy.types.Scene, name: str) -> Outcome[bpy.types.ViewLayer]:
    """Returns the scene's view layer `name`, absent fault naming it."""
    return Option.of_optional(scene.view_layers.get(name)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(scene.view_layers), member=name))


def soloed(scene: bpy.types.Scene, name: str) -> bool:
    """Renders the layer `name` alone, answering whether any layer's `use` changed."""
    changed = tuple(layer for layer in scene.view_layers if layer.use != (layer.name == name))
    for layer in changed:
        layer.use = layer.name == name
    return bool(changed)


def spread[T](
    value: T,
    layers: tuple[bpy.types.ViewLayer, ...],
    captured: Callable[[bpy.types.ViewLayer], Outcome[T]],
    same: Callable[[T, T], bool],
    applied: Callable[[T, bpy.types.ViewLayer], Outcome[object]],
    reason: str,
) -> Outcome[Answer]:
    """Applies value to each layer whose capture differs, `Unchanged` for `reason` when every layer holds it."""

    def wrote(current: tuple[T, ...]) -> Outcome[Answer]:
        if not (differing := tuple(layer for layer, held in zip(layers, current) if not same(held, value))):
            return Ok(Unchanged(reason=reason))
        return tallied(tuple(applied(value, layer) for layer in differing))

    return accumulated(map(captured, layers)).bind(wrote)


def listed[T: msgspec.Struct](
    preset: Preset[T], captured: Callable[[bpy.types.ViewLayer], Outcome[T]], same: Callable[[T, T], bool], applied: Callable[[T, bpy.types.ViewLayer], Outcome[object]]
) -> Listing[T]:
    """Preset listing over the context layer's capture, a preset applied to every used layer of the context scene whose capture differs."""

    def current(context: bpy.types.Context) -> Outcome[T]:
        return rooted(bpy.types.ViewLayer, context).bind(captured)

    return Listing(
        preset=preset,
        current=current,
        matches=lambda context, values: current(context).map(lambda held: tuple(same(held, value) for value in values)),
        applied=lambda context, value: rooted(bpy.types.Scene, context).bind(lambda scene: spread(value, used(scene), captured, same, applied, f"Used view layers of {scene.name} match preset")),
    )


# --- [REQUESTS]


def required(scene: bpy.types.Scene, context: bpy.types.Context, requests: tuple[PassRequest, ...]) -> Outcome[None]:
    """Enables the passes each request needs and full compositor precision where one asks, faulting unmet conditions."""

    def met(request: PassRequest) -> Outcome[None]:
        gate: Outcome[None] = request.condition.map(lambda condition: condition.check(context, Some(scene))).default_value(Ok(None))
        return (
            gate
            .bind(lambda _: layer_of(scene, request.layer))
            .bind(
                lambda layer: accumulated((
                    *(written(layer, flag.path, value=True) for flag in sorted(request.flags, key=lambda flag: flag.path)),
                    *((written(scene, ("render", "compositor_precision"), "FULL"),) if request.precise else ()),
                ))
            )
            .map(lambda _: None)
        )

    return accumulated(map(met, requests)).map(lambda _: None)


def synced(scene: bpy.types.Scene, identity: Identity) -> Outcome[None]:
    """Adds AOVs and the light groups scene emitters name to each used layer, removing an AOV Blender renames or invalidates."""

    def linked(node: bpy.types.Node) -> Outcome[str]:
        return socket_at(node.inputs, "Color").map(lambda socket: "COLOR" if socket.is_linked else "VALUE")

    def scanned(trees: tuple[bpy.types.NodeTree, ...]) -> Outcome[Mapping[str, str]]:
        return accumulated(linked(held).map(lambda kind: (held.aov_name, kind)) for tree in nested(trees) for held in tree.nodes if isinstance(held, bpy.types.ShaderNodeOutputAOV)).map(dict)

    def setup(material: bpy.types.Material) -> Outcome[Mapping[str, str]]:
        def kinds(held: Option[ShadingSetup]) -> Mapping[str, str]:
            match held.to_optional():
                case Utility(passes=passes):
                    return {str(member.name).lower(): member.kind for member in passes}
                case _:
                    return {}

        tree: Outcome[bpy.types.NodeTree] = Option.of_optional(material.node_tree).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(material), member="node_tree"))
        return Result[bpy.types.NodeTree, Faults].bind(tree, lambda found: stored(identity).bind(lambda store: store.read(found))).map(kinds)

    def wanted(layer: bpy.types.ViewLayer) -> Outcome[Mapping[str, str]]:
        override = Option.of_optional(layer.material_override)
        materials = override.map(lambda material: (material,)).default_with(lambda: tuple(slot.material for item in scene.objects for slot in item.material_slots if slot.material))
        trees = tuple(tree for tree in (*(material.node_tree for material in materials), *(world.node_tree for world in Option.of_optional(scene.world).to_list())) if tree)
        return override.filter(lambda material: Utility.ownership.marked(identity, material).is_some()).map(setup).default_with(lambda: scanned(trees))

    def aovs(layer: bpy.types.ViewLayer, names: Mapping[str, str]) -> Outcome[None]:
        held = {aov.name: aov for aov in layer.aovs}

        def synced_aov(name: str, kind: str) -> Outcome[None]:
            aov = held.get(name) or layer.aovs.add()
            answer = (
                paired(written(aov, ("name",), name), written(aov, ("type",), kind))
                .filter_with(lambda _: aov.name == name and aov.is_valid, lambda _: Fault.of(Reason.TAKEN, name, member=aov.name))
                .map(lambda _: None)
            )
            if answer.is_error() and name not in held:
                layer.aovs.remove(aov)
            return answer

        return accumulated(itertools.starmap(synced_aov, names.items())).map(lambda _: None)

    def layered(layer: bpy.types.ViewLayer) -> Outcome[None]:
        return (
            wanted(layer)
            .bind(lambda names: paired(aovs(layer, names), called(bpy.ops.scene.view_layer_add_used_lightgroups, {}, override=Some(bpy.context.temp_override(scene=scene, view_layer=layer)))))
            .map(lambda _: None)
        )

    return accumulated(map(layered, used(scene))).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EMISSION", "ENVIRONMENT", "OCCLUSION", "Derived", "Entity", "Family", "Flags", "PassFlags", "PassRequest", "layer_of", "listed", "required", "soloed", "spread", "synced", "used"]
