"""View layer passes: the pass vocabulary, flags of the running build, requests, used layers, and AOV and light group sync."""

from collections.abc import Mapping
import enum
import functools
import itertools
from types import MappingProxyType
from typing import Final, Self, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import RnaPath, Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.trees import nested, socket_at
from blender.bpy.scene.materials import AOVS, Shading, stored, Utility
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.rendering.outputs.naming import Output

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Family(enum.Flag):
    """Light pass family of direct, indirect, and color passes, each a term recombined beauty sums under its own gain, valued by its persisted flag bit."""

    DIFFUSE = 1
    GLOSSY = 2
    TRANSMISSION = 4
    VOLUME = 8


class Derived(enum.Flag):
    """Layer built from render passes, valued by its persisted flag bit."""

    DIRECT = 1
    INDIRECT = 2
    DEPTH = 4
    INVERSE = 8


class Entity(enum.Flag):
    """Cryptomatte kind, valued by its persisted flag bit."""

    OBJECT = 1
    MATERIAL = 2
    ASSET = 4

    @classmethod
    def named(cls, name: str) -> Option[Self]:
        """Returns kind with pass name `name`, `Nothing` for another."""
        return Block.of_seq(cls).filter(lambda member: ENTITIES[member].pass_name == name).try_head()


# --- [CONSTANTS] ------------------------------------------------------------------------

EMISSION: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_emit",), bpy.types.BoolProperty)
ENVIRONMENT: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_environment",), bpy.types.BoolProperty)
OCCLUSION: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_ambient_occlusion",), bpy.types.BoolProperty)
_DEPTH: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_z",), bpy.types.BoolProperty)
_USE: Final = Descriptor.declared(bpy.types.ViewLayer, ("use",), bpy.types.BoolProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class FamilyPasses(msgspec.Struct, frozen=True, kw_only=True):
    """Gain description and flag paths per slot of one light family."""

    description: str
    direct: tuple[RnaPath, ...]
    indirect: tuple[RnaPath, ...]
    color: tuple[RnaPath, ...]


class DerivedLayer(msgspec.Struct, frozen=True, kw_only=True):
    """Layer name, item socket type, and output class of a derived layer."""

    name: str
    socket_type: "rna_enums.NodeSocketDataTypeItems"
    output: Output


class IdPass(msgspec.Struct, frozen=True, kw_only=True):
    """Cryptomatte kind's name, Blender pass name, enabling flag, and block color member."""

    name: str
    pass_name: str
    flag: Descriptor[bpy.types.BoolProperty]
    color: Descriptor[bpy.types.FloatProperty]


class PassFlags(msgspec.Struct, frozen=True, kw_only=True):
    """Pass flags of running build, resolved once."""

    every: tuple[tuple[type[Struct], Descriptor[bpy.types.BoolProperty]], ...]
    families: Mapping[Family, tuple[frozenset[Descriptor[bpy.types.BoolProperty]], frozenset[Descriptor[bpy.types.BoolProperty]], frozenset[Descriptor[bpy.types.BoolProperty]]]]

    @classmethod
    @functools.cache
    def of(cls) -> Outcome[Self]:
        """Resolves every pass flag and each family's direct, indirect, and color flags from its declared paths once per build, faulting each path the build lacks."""

        def slot(paths: tuple[RnaPath, ...]) -> Outcome[frozenset[Descriptor[bpy.types.BoolProperty]]]:
            return accumulated(Descriptor.resolve(bpy.types.ViewLayer, path, bpy.types.BoolProperty) for path in paths).map(frozenset)

        return accumulated(paired(slot(held.direct), slot(held.indirect), slot(held.color)) for held in FAMILIES.values()).map(
            lambda families: cls(every=pass_flags(), families=dict(zip(FAMILIES, families)))
        )

    @property
    def lobes(self) -> frozenset[Descriptor[bpy.types.BoolProperty]]:
        """Every family flag."""
        return frozenset().union(*(slot for slots in self.families.values() for slot in slots))

    def opened(self, totals: Family, derived: Derived, ids: Entity) -> frozenset[Descriptor[bpy.types.BoolProperty]]:
        """Returns flags the families summed, the derived layers, and the ID kinds read."""

        def lit(item: Derived) -> tuple[frozenset[Descriptor[bpy.types.BoolProperty]], ...]:
            match item:
                case Derived.DIRECT:
                    return tuple(direct | color for direct, _, color in self.families.values())
                case Derived.INDIRECT:
                    return tuple(indirect | color for _, indirect, color in self.families.values())
                case _:
                    return ()

        return frozenset().union(*(slot for family in totals for slot in self.families[family]), *(held for item in derived for held in lit(item)), (ENTITIES[entity].flag for entity in ids))


class PassRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Passes, condition, and precision a graph needs of one view layer."""

    layer: str
    flags: frozenset[Descriptor[bpy.types.BoolProperty]] = frozenset()
    condition: Option[Condition] = Nothing
    precise: bool = False

    @classmethod
    def depth(cls, layer: str, *, precise: bool) -> Self:
        """Returns Depth pass request of a layer."""
        return cls(layer=layer, flags=frozenset({_DEPTH}), precise=precise)


# --- [TABLES] ---------------------------------------------------------------------------

FAMILIES: Final[Mapping[Family, FamilyPasses]] = MappingProxyType({
    Family.DIFFUSE: FamilyPasses(description="Diffuse light gain", direct=(("use_pass_diffuse_direct",),), indirect=(("use_pass_diffuse_indirect",),), color=(("use_pass_diffuse_color",),)),
    Family.GLOSSY: FamilyPasses(description="Glossy light gain", direct=(("use_pass_glossy_direct",),), indirect=(("use_pass_glossy_indirect",),), color=(("use_pass_glossy_color",),)),
    Family.TRANSMISSION: FamilyPasses(
        description="Transmitted light gain", direct=(("use_pass_transmission_direct",),), indirect=(("use_pass_transmission_indirect",),), color=(("use_pass_transmission_color",),)
    ),
    Family.VOLUME: FamilyPasses(
        description="Volume light gain", direct=(("cycles", "use_pass_volume_direct"), ("eevee", "use_pass_volume_direct")), indirect=(("cycles", "use_pass_volume_indirect"),), color=()
    ),
})
DERIVED: Final[Mapping[Derived, DerivedLayer]] = MappingProxyType({
    Derived.DIRECT: DerivedLayer(name="Direct Light", socket_type="RGBA", output=Output.BEAUTY),
    Derived.INDIRECT: DerivedLayer(name="Indirect Light", socket_type="RGBA", output=Output.BEAUTY),
    Derived.DEPTH: DerivedLayer(name="Depth Map", socket_type="FLOAT", output=Output.DATA),
    Derived.INVERSE: DerivedLayer(name="Inverse Depth", socket_type="FLOAT", output=Output.DATA),
})
ENTITIES: Final[Mapping[Entity, IdPass]] = MappingProxyType({
    entity: IdPass(
        name=bpy.path.display_name(str(entity.name).lower()),
        pass_name=pass_name,
        flag=Descriptor.declared(bpy.types.ViewLayer, (flag,), bpy.types.BoolProperty),
        color=Descriptor.declared(owner, (color,), bpy.types.FloatProperty),
    )
    for entity, pass_name, flag, owner, color in (
        (Entity.OBJECT, "CryptoObject", "use_pass_cryptomatte_object", bpy.types.Object, "color"),
        (Entity.MATERIAL, "CryptoMaterial", "use_pass_cryptomatte_material", bpy.types.Material, "diffuse_color"),
        (Entity.ASSET, "CryptoAsset", "use_pass_cryptomatte_asset", bpy.types.Object, "color"),
    )
})

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LAYERS]


def used(scene: bpy.types.Scene) -> tuple[bpy.types.ViewLayer, ...]:
    """Returns scene layers with `use` on, in list order."""
    return tuple(layer for layer in scene.view_layers if bool(_USE.read(layer)))


def soloed(scene: bpy.types.Scene, name: str) -> Outcome[bool]:
    """Renders the layer `name` alone, answering whether any layer's `use` changed."""
    return accumulated(_USE.write(layer, layer.name == name) for layer in scene.view_layers).map(lambda priors: any(prior.is_some() for prior in priors))


def total(family: Family) -> str:
    """Returns the layer name of a family's summed lobes."""
    return f"{bpy.path.display_name(str(family.name).lower())} Total"


# --- [FLAGS]


def pass_flags() -> tuple[tuple[type[Struct], Descriptor[bpy.types.BoolProperty]], ...]:
    """Returns writable pass flags of `ViewLayer` and its pointer structs in RNA order with owning class."""
    root = bpy.types.ViewLayer.bl_rna
    structs = (
        ((), bpy.types.ViewLayer, root.properties),
        *(((prop.identifier,), type(fixed), fixed.properties) for prop in root.properties if isinstance(prop, bpy.types.PointerProperty) for fixed in Option.of_optional(prop.fixed_type).to_list()),
    )
    return tuple(
        (owner, Descriptor(root=bpy.types.ViewLayer, owner=path, prop=prop))
        for path, owner, properties in structs
        for prop in properties
        if isinstance(prop, bpy.types.BoolProperty) and not prop.is_readonly and prop.identifier.startswith("use_pass_")
    )


# --- [REQUESTS]


def required(scene: bpy.types.Scene, context: bpy.types.Context, requests: tuple[PassRequest, ...], precision: Descriptor[bpy.types.EnumProperty]) -> Outcome[None]:
    """Enables passes and precision each request needs, faulting unmet conditions."""

    def met(request: PassRequest) -> Outcome[None]:
        gate: Outcome[None] = request.condition.map(lambda condition: condition.check(context, Some(scene))).default_value(Ok(None))
        return (
            gate
            .bind(lambda _: Option.of_optional(scene.view_layers.get(request.layer)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(scene.view_layers), member=request.layer)))
            .bind(
                lambda layer: accumulated((
                    *(written(layer, flag.path, value=True) for flag in sorted(request.flags, key=lambda flag: flag.path)),
                    *((written(scene, precision.path, "FULL"),) if request.precise else ()),
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
        def kinds(held: Option[Shading]) -> Mapping[str, str]:
            match held.to_optional():
                case Shading(setup=Utility(passes=passes)):
                    return {str(member.name).lower(): str(AOVS[member].kind) for member in passes}
                case _:
                    return {}

        tree: Outcome[bpy.types.NodeTree] = Option.of_optional(material.node_tree).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(material), member="node_tree"))
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
                .filter_with(lambda _: aov.name == name and aov.is_valid, lambda _: Fault.of(BlenderReason.TAKEN, name, member=aov.name))
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

__all__ = [
    "DERIVED",
    "EMISSION",
    "ENTITIES",
    "ENVIRONMENT",
    "FAMILIES",
    "OCCLUSION",
    "Derived",
    "DerivedLayer",
    "Entity",
    "Family",
    "FamilyPasses",
    "IdPass",
    "PassFlags",
    "PassRequest",
    "pass_flags",
    "required",
    "soloed",
    "synced",
    "total",
    "used",
]
