"""View layer passes: pass kinds, flags of the running build, requests, used layers, presets spread over them, and AOV and light group sync."""

from collections import defaultdict
from collections.abc import Callable, Mapping
import enum
import functools
import itertools
from typing import Final, Self

import bpy
from expression import Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands.operators import Answer, called, tallied, Unchanged
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._rna.conditions import Condition, Setting
from rasm_bpy.data._rna.descriptors import Descriptor, written
from rasm_bpy.data._rna.paths import rooted, Struct
from rasm_bpy.nodes._trees import _nested, _socket_at
from rasm_bpy.scene._materials import _ShadingSetup, stored, Utility
from rasm_bpy.settings._presets import Preset
from rasm_bpy.ui._layout.values import Listing
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Flags = frozenset[Descriptor]


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


class _LightSlot(enum.StrEnum):
    """Term of a light family's total, valued by its flag suffix and lobe group input."""

    DIRECT = enum.auto()
    INDIRECT = enum.auto()
    COLOR = enum.auto()


class Derived(enum.Flag):
    """Layer built from render passes, named by its member and valued by its persisted flag bit."""

    DIRECT_LIGHT = 1
    INDIRECT_LIGHT = 2
    DEPTH_MAP = 4
    INVERSE_DEPTH = 8

    @property
    def label(self) -> str:
        """Display name of a derived pass."""
        return bpy.path.display_name(str(self.name).lower())


class CryptomatteKind(enum.Flag):
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
    def flag(self) -> Descriptor:
        """View layer flag enabling the kind's passes."""
        return Descriptor(root=bpy.types.ViewLayer, prop=bpy.types.ViewLayer.bl_rna.properties[f"use_pass_cryptomatte_{str(self.name).lower()}"])

    @property
    def index(self) -> Option[tuple[Descriptor, str]]:
        """View layer flag and pass name of the kind's index pass, which Cycles alone writes, Nothing for a kind the view layer holds no index flag of."""
        return (
            Some(f"use_pass_{str(self.name).lower()}_index")
            .filter(lambda flag: flag in bpy.types.ViewLayer.bl_rna.properties)
            .map(lambda flag: (Descriptor(root=bpy.types.ViewLayer, prop=bpy.types.ViewLayer.bl_rna.properties[flag]), f"{self.label} Index"))
        )

    @property
    def color(self) -> Descriptor:
        """Display color member of the blocks the kind names."""
        owner, member = (bpy.types.Material, "diffuse_color") if self is CryptomatteKind.MATERIAL else (bpy.types.Object, "color")
        return Descriptor(root=owner, prop=owner.bl_rna.properties[member])

    @classmethod
    def named(cls, name: str) -> Option[Self]:
        """Returns kind with pass name `name`, `Nothing` for another."""
        return Block.of_seq(cls).filter(lambda member: member.pass_name == name).try_head()


# --- [CONSTANTS] ------------------------------------------------------------------------

_EMISSION: Final = Descriptor(root=bpy.types.ViewLayer, prop=bpy.types.ViewLayer.bl_rna.properties["use_pass_emit"])
_ENVIRONMENT: Final = Descriptor(root=bpy.types.ViewLayer, prop=bpy.types.ViewLayer.bl_rna.properties["use_pass_environment"])
_OCCLUSION: Final = Descriptor(root=bpy.types.ViewLayer, prop=bpy.types.ViewLayer.bl_rna.properties["use_pass_ambient_occlusion"])
DENOISING_ALBEDO: Final = "Denoising Albedo"
DENOISING_NORMAL: Final = "Denoising Normal"
_DEPTH: Final = Descriptor(root=bpy.types.ViewLayer, prop=bpy.types.ViewLayer.bl_rna.properties["use_pass_z"])
_ENGINE: Final = Descriptor(root=bpy.types.Scene, owner=("render",), prop=bpy.types.RenderSettings.bl_rna.properties["engine"])
_CYCLES: Final = Setting(descriptor=_ENGINE, accepted=("CYCLES",))

# --- [MODELS] ---------------------------------------------------------------------------


class PassFlags(msgspec.Struct, frozen=True, kw_only=True):
    """Writable `use_pass_*` flags of `ViewLayer` and its pointer structs in the running build, in RNA order with owning class."""

    every: tuple[tuple[type[Struct], Descriptor], ...]

    @classmethod
    def of(cls) -> Self:
        """Reads pass flags from the running build and registered engines."""
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
        named: defaultdict[str, set[Descriptor]] = defaultdict(set)
        for _, flag in self.every:
            named[flag.prop.identifier].add(flag)

        def slot(family: Family, kind: _LightSlot) -> Flags:
            return frozenset(named.get(f"use_pass_{str(family.name).lower()}_{kind}", ()))

        return {family: (slot(family, _LightSlot.DIRECT), slot(family, _LightSlot.INDIRECT), slot(family, _LightSlot.COLOR)) for family in Family}

    @property
    def lobes(self) -> Flags:
        """Every family flag."""
        return frozenset(flag for slots in self.families.values() for slot in slots for flag in slot)

    def enabled_for(self, totals: Family, derived: Derived, ids: CryptomatteKind) -> Flags:
        """Returns flags the families summed, the derived layers, and the ID kinds read."""
        families = self.families

        return frozenset(entity.flag for entity in ids).union(
            *(slot for family in totals for slot in families[family]),
            *(direct | color for direct, _, color in families.values() if Derived.DIRECT_LIGHT in derived),
            *(indirect | color for _, indirect, color in families.values() if Derived.INDIRECT_LIGHT in derived),
            {_DEPTH} if derived & (Derived.DEPTH_MAP | Derived.INVERSE_DEPTH) else (),
        )


class PassRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Passes, condition, and precision a graph needs of one view layer."""

    layer: str
    flags: Flags = frozenset()
    condition: Option[Condition] = Nothing
    precise: bool = False

    @classmethod
    def depth(cls, layer: str, *, precise: bool) -> Self:
        """Returns Depth pass request of a layer."""
        return cls(layer=layer, flags=frozenset({_DEPTH}), precise=precise)

    @classmethod
    def indexed(cls, layer: str, kinds: CryptomatteKind) -> Self:
        """Returns the request of the kinds' index passes under Cycles, the one engine writing them."""
        return cls(layer=layer, flags=frozenset(flag for kind in kinds for flag, _ in kind.index.to_list()), condition=Some(_CYCLES))

    @classmethod
    def matted(cls, layer: str, kinds: CryptomatteKind) -> Self:
        """Returns the request of the kinds' Cryptomatte passes under Cycles or EEVEE, the engines writing them."""
        return cls(layer=layer, flags=frozenset(kind.flag for kind in kinds), condition=Some(Setting(descriptor=_ENGINE, accepted=("CYCLES", "BLENDER_EEVEE"))))

    @classmethod
    def denoising(cls, layer: str) -> Outcome[Self]:
        """Returns the request of Cycles' denoising data passes under Cycles, `DENOISING_ALBEDO` and `DENOISING_NORMAL` among them, refused while Cycles holds no layer settings."""
        return Descriptor.resolve(bpy.types.ViewLayer, ("cycles", "denoising_store_passes"), bpy.types.BoolProperty).map(
            lambda flag: cls(layer=layer, flags=frozenset({flag}), condition=Some(_CYCLES))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LAYERS]


def _used(scene: bpy.types.Scene) -> tuple[bpy.types.ViewLayer, ...]:
    """Returns scene layers with `use` on, in list order."""
    return tuple(layer for layer in scene.view_layers if layer.use)


def _layer_of(scene: bpy.types.Scene, name: str) -> Outcome[bpy.types.ViewLayer]:
    """Returns the scene's view layer `name`, absent fault naming it."""
    return catch(lambda: scene.view_layers[name], exception=KeyError)().map_error(lambda cause: Fault.of(Reason.ABSENT, repr(scene.view_layers), cause, member=name))


def _soloed(layer: bpy.types.ViewLayer) -> bool:
    """Turns `use` on for the given layer alone, answering whether any layer's `use` changed."""
    scene: bpy.types.Scene = layer.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    changed = tuple(held for held in scene.view_layers if held.use != (held == layer))
    for held in changed:
        held.use = held == layer
    return bool(changed)


def _spread[T](
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


def _listed[T: msgspec.Struct](
    preset: Preset[T], captured: Callable[[bpy.types.ViewLayer], Outcome[T]], same: Callable[[T, T], bool], applied: Callable[[T, bpy.types.ViewLayer], Outcome[object]]
) -> Listing[T]:
    """Preset listing over the context layer's capture, a preset applied to every used layer of the context scene whose capture differs."""

    def current(context: bpy.types.Context) -> Outcome[T]:
        return rooted(bpy.types.ViewLayer, context).bind(captured)

    return Listing(
        preset=preset,
        current=current,
        matches=lambda context, values: current(context).map(lambda held: tuple(same(held, value) for value in values)),
        applied=lambda context, value: rooted(bpy.types.Scene, context).bind(lambda scene: _spread(value, _used(scene), captured, same, applied, f"Used view layers of {scene.name} match preset")),
    )


# --- [REQUESTS]


def _required(scene: bpy.types.Scene, context: bpy.types.Context, requests: tuple[PassRequest, ...]) -> Outcome[None]:
    """Enables the passes each request needs and full compositor precision where one asks, faulting unmet conditions."""

    def met(request: PassRequest) -> Outcome[None]:
        gate: Outcome[None] = request.condition.map(lambda condition: condition.check(context, Some(scene))).default_value(Ok(None))
        return (
            gate
            .bind(lambda _: _layer_of(scene, request.layer))
            .bind(
                lambda layer: accumulated((
                    *(written(layer, flag.path, value=True) for flag in sorted(request.flags, key=lambda flag: flag.path)),
                    *((written(scene, ("render", "compositor_precision"), "FULL"),) if request.precise else ()),
                ))
            )
            .map(lambda _: None)
        )

    return accumulated(map(met, requests)).map(lambda _: None)


def _synced(scene: bpy.types.Scene, identity: Identity) -> Outcome[None]:
    """Adds AOVs and the light groups scene emitters name to each used layer, removing an AOV Blender renames or invalidates."""

    def linked(node: bpy.types.Node) -> Outcome[str]:
        return _socket_at(node.inputs, "Color").map(lambda socket: "COLOR" if socket.is_linked else "VALUE")

    def scanned(trees: tuple[bpy.types.NodeTree, ...]) -> Outcome[Mapping[str, str]]:
        return accumulated(linked(held).map(lambda kind: (held.aov_name, kind)) for tree in _nested(trees) for held in tree.nodes if isinstance(held, bpy.types.ShaderNodeOutputAOV)).map(dict)

    def setup(material: bpy.types.Material) -> Outcome[Mapping[str, str]]:
        def kinds(held: Option[_ShadingSetup]) -> Mapping[str, str]:
            match held.to_optional():
                case Utility(passes=passes):
                    return {str(member.name).lower(): member.kind for member in passes}
                case _:
                    return dict[str, str]()

        tree: bpy.types.NodeTree = material.node_tree  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return stored(identity).read(tree).map(kinds)

    @functools.cache
    def wanted(material: bpy.types.Material | None, world: bpy.types.World | None) -> Outcome[Mapping[str, str]]:
        override = Option.of_optional(material)
        materials = override.map(lambda material: (material,)).default_with(lambda: tuple(slot.material for item in scene.objects for slot in item.material_slots if slot.material))
        trees: tuple[bpy.types.NodeTree, ...] = tuple(held.node_tree for held in materials) + tuple(held.node_tree for held in Option.of_optional(world).to_list())  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return override.filter(lambda material: Utility.ownership.marked(identity, material).is_some()).map(setup).default_with(lambda: scanned(trees))

    def aovs(layer: bpy.types.ViewLayer, names: Mapping[str, str]) -> Outcome[None]:
        held = {aov.name: aov for aov in layer.aovs}

        def synced_aov(name: str, kind: str) -> Outcome[None]:
            aov = held.get(name) or layer.aovs.add()
            if (
                answer := paired(written(aov, ("name",), name), written(aov, ("type",), kind))
                .filter_with(lambda _: aov.name == name and aov.is_valid, lambda _: Fault.of(Reason.TAKEN, name, member=aov.name))
                .map(lambda _: None)
            ).is_error() and name not in held:
                layer.aovs.remove(aov)
            return answer

        return accumulated(itertools.starmap(synced_aov, names.items())).map(lambda _: None)

    def layered(layer: bpy.types.ViewLayer) -> Outcome[None]:
        return (
            wanted(layer.material_override, layer.world_override or scene.world)
            .bind(lambda names: paired(aovs(layer, names), called(bpy.ops.scene.view_layer_add_used_lightgroups, {}, override=Some(bpy.context.temp_override(scene=scene, view_layer=layer)))))
            .map(lambda _: None)
        )

    return accumulated(map(layered, _used(scene))).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DENOISING_ALBEDO", "DENOISING_NORMAL", "CryptomatteKind", "Derived", "Family", "Flags", "PassFlags", "PassRequest"]
