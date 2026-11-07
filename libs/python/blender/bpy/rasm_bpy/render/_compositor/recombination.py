"""Beauty recombined from a view layer's light passes or light groups, each summed term under its own gain socket, every term read through the pass that renders it."""

from collections.abc import Callable, Mapping
from functools import partial
from typing import Final, Self

import bpy
from expression import Option, Some
import msgspec

from rasm_bpy.data._extension.identity import Key
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.nodes._desired import item_key
from rasm_bpy.nodes._graphs import Expr, math, mixed
from rasm_bpy.nodes._interfaces import Interface, InterfacePanel, Socket
from rasm_bpy.nodes.changes import NodeGroup
from rasm_bpy.render._compositor.colors import with_alpha
from rasm_bpy.render._view_layers.passes import _EMISSION, _ENVIRONMENT, _OCCLUSION, Family, PassFlags, PassRequest
from rasm_bpy.render._view_layers.sources import _lobed
from rasm_bpy.scene._lights.groups import _LightGroupEdits
from rasm_records import accumulated, Case, Error, Fault, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type LightPass = Family | Descriptor
type Recombination = Lobes | LightGroups
type TermFilter = Callable[[Key, Expr, Option[Family], Expr], Expr]

# --- [CONSTANTS] ------------------------------------------------------------------------

TERMS: Final[tuple[tuple[Key, LightPass, str], ...]] = (
    ("diffuse", Family.DIFFUSE, "Diffuse light gain"),
    ("glossy", Family.GLOSSY, "Glossy light gain"),
    ("transmission", Family.TRANSMISSION, "Transmitted light gain"),
    ("volume", Family.VOLUME, "Volume light gain"),
    ("emission", _EMISSION, "Gain of emitters seen by camera"),
    ("environment", _ENVIRONMENT, "Gain of background seen by camera, dropped under transparent film"),
)
_GAIN: Final = Socket(key="gain", kind=bpy.types.NodeSocketFloat, min_value=-32.0, max_value=32.0)
_STRENGTH: Final = Socket(key="strength", kind=bpy.types.NodeSocketFloatFactor, description="Ambient occlusion strength over family light", default_value=1.0)
_TINT: Final = Socket(key="tint", kind=bpy.types.NodeSocketColor, description="Color multiplying group light", default_value=(1.0, 1.0, 1.0, 1.0))
_OCCLUSION_PANEL: Final[Key] = "occlusion"
_GROUP: Final[Key] = "group"

# --- [MODELS] ---------------------------------------------------------------------------


class Lobes(Case, frozen=True, kw_only=True):
    """Light passes rebuilt under one gain per summed term and ambient occlusion, each term's panel toggling it."""


class LightGroups(Case, frozen=True, kw_only=True):
    """Light-group passes summed under group gains and tints, one panel per group in order."""

    groups: tuple[str, ...] = ()


class Recombined(Case, frozen=True, kw_only=True):
    """View layer's light passes or light groups recombined into beauty."""

    layer: str
    recombination: Recombination = Lobes()

    @property
    def interface(self) -> Interface:
        """Gain panels of the summed terms, each panel's toggle keeping its term."""
        match self.recombination:
            case Lobes():
                return Interface(
                    panels=(
                        *(
                            InterfacePanel.of(key, Interface(inputs=(msgspec.structs.replace(_GAIN, description=f"{description} in stops"),)), description=description, toggle=Some(True))
                            for key, _, description in TERMS
                        ),
                        InterfacePanel.of(_OCCLUSION_PANEL, Interface(inputs=(_STRENGTH,)), description="Ambient occlusion over family light", toggle=Some(False)),
                    )
                )
            case LightGroups(groups=groups):
                return Interface(
                    panels=tuple(
                        InterfacePanel.of(
                            item_key(_GROUP, index),
                            Interface(inputs=(msgspec.structs.replace(_GAIN, description="Group light in stops"), _TINT)),
                            name=name,
                            description="View layer light group",
                            toggle=Some(True),
                        )
                        for index, name in enumerate(groups, 1)
                    )
                )

    def requested(self, flags: PassFlags) -> PassRequest:
        """Requests every family, term, and occlusion pass flag, or Cycles for light groups."""
        match self.recombination:
            case Lobes():
                passes = frozenset().union(*(slot for _, held, _ in TERMS if isinstance(held, Family) for slot in flags.families[held]))
                return PassRequest(layer=self.layer, flags=passes | {held for _, held, _ in TERMS if isinstance(held, Descriptor)} | {_OCCLUSION})
            case LightGroups():
                return PassRequest(layer=self.layer, condition=_LightGroupEdits.cycles)

    def synced(self, names: tuple[str, ...]) -> Self:
        """Holds the given light group names in order."""
        match self.recombination:
            case LightGroups():
                return msgspec.structs.replace(self, recombination=LightGroups(groups=names))
            case Lobes():
                return self

    def recombined(
        self, outer: Callable[[Key], Expr], render: Expr, scene: bpy.types.Scene, offered: Mapping[str, str], flags: PassFlags, lobe: NodeGroup, filtered: Option[TermFilter] = Nothing
    ) -> Outcome[Expr]:
        """Builds beauty with render alpha from the layer's Render Layers node, each term gained and, under a term filter, filtered before it sums.

        Args:
            outer: Group input of an interface key under the source.
            render: Render Layers node of the source view layer.
            scene: Scene whose transparent film drops the environment term.
            offered: Socket identifier per enabled Render Layers output name.
            flags: Pass flags naming each family's Render Layers outputs.
            lobe: Shared lobe total group.
            filtered: Filter of each term by key, pass, family, and the Render Layers node its guides read.

        Returns:
            `Ok` of beauty with render alpha. `Error` of every absent pass and `absent` naming the sum when no term is offered.
        """
        sockets = {name: render.at(identifier, bpy.types.NodeSocketColor) for name, identifier in offered.items()}

        def socket(name: str) -> Outcome[Expr]:
            return Option.of_optional(sockets.get(name)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(tuple(offered)), member=name))

        def gained(source: Expr, key: Key) -> Expr:
            return source * (math("POWER", 2.0, outer(item_key(key, _GAIN.key))) * outer(key))

        def total(terms: tuple[Expr, ...], requested: Key) -> Outcome[Expr]:
            return Ok(sum(terms[1:], terms[0])) if terms else Error(Fault.of(Reason.ABSENT, repr(tuple(offered)), member=requested))

        def finished(terms: tuple[tuple[Key, Expr, Option[Family]], ...], combined: Callable[[Mapping[Key, Expr]], Outcome[Expr]]) -> Outcome[Expr]:
            picked = filtered.map(lambda term: {key: term(key, source, family, render) for key, source, family in terms}).default_with(lambda: {key: source for key, source, _ in terms})
            return paired(socket("Alpha"), combined(picked)).map(lambda parts: with_alpha(parts[1], parts[0]))

        match self.recombination:
            case Lobes():
                kept = tuple((key, held) for key, held, _ in TERMS if not (held is _ENVIRONMENT and scene.render.film_transparent))
                families = tuple((key, held) for key, held in kept if isinstance(held, Family))

                def term(key: Key, held: LightPass) -> Option[Outcome[tuple[Key, Expr, Option[Family]]]]:
                    match held:
                        case Descriptor():
                            return Some(socket(held.prop.name).map(lambda placed: (key, placed, Nothing)))
                        case family:
                            return _lobed(flags.families[family], sockets, lobe).map(lambda placed: Ok((key, placed, Some(family))))

                def lobes(terms: Mapping[Key, Expr]) -> Outcome[Expr]:
                    strength = math("MULTIPLY", outer(_OCCLUSION_PANEL), outer(item_key(_OCCLUSION_PANEL, _STRENGTH.key)))
                    light = total(tuple(gained(terms[key], key) for key, _ in families if key in terms), ", ".join(key for key, _ in families))
                    return light.map(lambda lit: sum((gained(terms[key], key) for key, held in kept if not isinstance(held, Family)), mixed(strength, lit, terms[_OCCLUSION_PANEL], blend="MULTIPLY")))

                occluded = socket(_OCCLUSION.prop.name).map(lambda source: (_OCCLUSION_PANEL, source, Nothing))
                return accumulated((*(answer for key, held in kept for answer in term(key, held).to_list()), occluded)).bind(lambda terms: finished(terms, lobes))
            case LightGroups(groups=groups):

                def grouped(terms: Mapping[Key, Expr]) -> Outcome[Expr]:
                    tinted = tuple(gained(mixed(1.0, terms[name], outer(item_key(_GROUP, index, _TINT.key)), blend="MULTIPLY"), item_key(_GROUP, index)) for index, name in enumerate(groups, 1))
                    return total(tinted, "groups")

                return accumulated(socket(f"Combined_{name}") for name in groups).bind(lambda sources: finished(tuple((name, source, Nothing) for name, source in zip(groups, sources)), grouped))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TERMS", "LightGroups", "LightPass", "Lobes", "Recombination", "Recombined", "TermFilter"]
