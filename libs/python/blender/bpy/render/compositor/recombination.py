"""Beauty recombined from a view layer's light passes or light groups, each summed term under its own gain socket, every term read through the pass that renders it."""

from collections.abc import Callable, Mapping
import functools
from typing import Final, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor, Struct
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.graphs import Expr, math, mixed, NodeGroup
from blender.bpy.nodes.interface import Interface, InterfacePanel, Socket
from blender.bpy.render.view_layers.passes import Family, lobed, PassFlags, PassRequest
from blender.bpy.scene.lights.groups import LightGroupEdits
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type LightPass = Family | Descriptor[bpy.types.BoolProperty]
type Recombination = Lobes | LightGroups
type TermFilter = Callable[[Key, Expr, Option[Family], Expr], Expr]

# --- [CONSTANTS] ------------------------------------------------------------------------

EMISSION = Descriptor.declared(bpy.types.ViewLayer, "use_pass_emit", bpy.types.BoolProperty)
ENVIRONMENT = Descriptor.declared(bpy.types.ViewLayer, "use_pass_environment", bpy.types.BoolProperty)
OCCLUSION = Descriptor.declared(bpy.types.ViewLayer, "use_pass_ambient_occlusion", bpy.types.BoolProperty)
TERMS: Final[tuple[tuple[Key, LightPass, str], ...]] = (
    ("diffuse", Family.DIFFUSE, "Diffuse light gain"),
    ("glossy", Family.GLOSSY, "Glossy light gain"),
    ("transmission", Family.TRANSMISSION, "Transmitted light gain"),
    ("volume", Family.VOLUME, "Volume light gain"),
    ("emission", EMISSION, "Gain of emitters seen by camera"),
    ("environment", ENVIRONMENT, "Gain of background seen by camera, dropped under transparent film"),
)
_GAIN: Final = {"min_value": -32.0, "max_value": 32.0}

# --- [CHOICES] --------------------------------------------------------------------------


@functools.cache
def _views(names: tuple[str, ...]) -> tuple[tuple[str, str, str], ...]:
    """Enum rows of used view layer names, cached so the strings Blender reads stay referenced."""
    return tuple((name, name, "") for name in names)


def views(_owner: Struct, context: bpy.types.Context | None) -> tuple[tuple[str, str, str], ...]:
    """Used view layer rows of the context scene."""
    return _views(tuple(layer.name for scene in Option.of_optional(context).bind(lambda held: Option.of_optional(held.scene)).to_list() for layer in scene.view_layers if layer.use))


# --- [MODELS] ---------------------------------------------------------------------------


class Lobes(Case, frozen=True, kw_only=True):
    """Light passes rebuilt under one gain per summed term and ambient occlusion, each term's panel toggling it."""


class LightGroups(Case, frozen=True, kw_only=True):
    """Light-group passes summed under group gains and tints, one panel per group in order."""

    groups: tuple[str, ...] = ()


class Lit(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """One summed term: its key, Render Layers pass, and the pass after a term filter."""

    key: Key
    source: Expr
    filtered: Expr


class Beauty(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Recombined beauty with render alpha, and under a term filter the beauty of filtered terms with each term."""

    plain: Expr
    filtered: Option[Expr] = Nothing
    terms: tuple[Lit, ...] = ()


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
                            InterfacePanel(
                                key=key,
                                name=bpy.path.display_name(key),
                                description=description,
                                toggle=Some(True),
                                inputs=(Socket(key=f"{key}_gain", name="Gain", kind=bpy.types.NodeSocketFloat, description=f"{description} in stops", members=_GAIN),),
                            )
                            for key, _, description in TERMS
                        ),
                        InterfacePanel(
                            key="occlusion",
                            name="Occlusion",
                            description="Ambient occlusion over family light",
                            toggle=Some(False),
                            inputs=(
                                Socket(
                                    key="occlusion_strength",
                                    name="Strength",
                                    kind=bpy.types.NodeSocketFloat,
                                    description="Ambient occlusion strength over family light",
                                    members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": 1.0},
                                ),
                            ),
                        ),
                    )
                )
            case LightGroups(groups=groups):
                return Interface(
                    panels=tuple(
                        InterfacePanel(
                            key=f"group_{index}",
                            name=name,
                            description="View layer light group",
                            toggle=Some(True),
                            inputs=(
                                Socket(key=f"group_{index}_gain", name="Gain", kind=bpy.types.NodeSocketFloat, description="Group light in stops", members=_GAIN),
                                Socket(
                                    key=f"group_{index}_tint", name="Tint", kind=bpy.types.NodeSocketColor, description="Color multiplying group light", members={"default_value": (1.0, 1.0, 1.0, 1.0)}
                                ),
                            ),
                        )
                        for index, name in enumerate(groups, 1)
                    )
                )

    def requested(self, flags: PassFlags) -> PassRequest:
        """Requests every family, term, and occlusion pass flag, or Cycles for light groups."""
        match self.recombination:
            case Lobes():
                passes = frozenset().union(*(slot for _, held, _ in TERMS if isinstance(held, Family) for slot in flags.families[held]))
                return PassRequest(layer=self.layer, flags=passes | {held for _, held, _ in TERMS if isinstance(held, Descriptor)} | {OCCLUSION})
            case LightGroups():
                return PassRequest(layer=self.layer, condition=LightGroupEdits.cycles)

    def synced(self, names: tuple[str, ...]) -> Self:
        """Holds the given light group names in order."""
        match self.recombination:
            case LightGroups():
                return msgspec.structs.replace(self, recombination=LightGroups(groups=names))
            case Lobes():
                return self

    def recombined(
        self, outer: Callable[[Key], Expr], render: Expr, scene: bpy.types.Scene, offered: Mapping[str, str], flags: PassFlags, lobe: NodeGroup, filtered: Option[TermFilter] = Nothing
    ) -> Outcome[Beauty]:
        """Builds beauty from the layer's Render Layers node, each term gained, and under a term filter the beauty of filtered terms.

        Args:
            outer: Group input of an interface key under the source.
            render: Render Layers node of the source view layer.
            scene: Scene whose transparent film drops the environment term.
            offered: Socket identifier per enabled Render Layers output name.
            flags: Pass flags naming each family's Render Layers outputs.
            lobe: Shared lobe total group.
            filtered: Filter of each term by key, pass, family, and the Render Layers node its guides read.

        Returns:
            `Ok` of beauties with render alpha. `Error` of every absent pass and `absent` naming the sum when no term is offered.
        """
        sockets = {name: msgspec.structs.replace(render, socket=identifier, shape="RGBA") for name, identifier in offered.items()}

        def socket(name: str) -> Outcome[Expr]:
            return Option.of_optional(sockets.get(name)).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(tuple(offered)), member=name),))

        def gained(source: Expr, key: Key) -> Expr:
            return source * (math("POWER", 2.0, outer(f"{key}_gain")) * outer(key))

        def total(terms: tuple[Expr, ...], requested: Key) -> Outcome[Expr]:
            return Ok(sum(terms[1:], terms[0])) if terms else Error((Fault(reason=BlenderReason.ABSENT, subject=repr(tuple(offered)), member=requested),))

        def finished(terms: tuple[tuple[Key, Expr, Option[Family]], ...], combined: Callable[[Mapping[Key, Expr]], Outcome[Expr]]) -> Outcome[Beauty]:
            def alpha(beauty: Expr, mask: Expr) -> Expr:
                return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", beauty), ("Alpha", mask), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

            lit = filtered.map(lambda term: tuple(Lit(key=key, source=source, filtered=term(key, source, family, render)) for key, source, family in terms))
            cleaned = lit.map(lambda held: combined({term.key: term.filtered for term in held}).map(Some)).default_value(Ok(Nothing))
            return paired(socket("Alpha"), combined({key: source for key, source, _ in terms}), cleaned).map(
                lambda parts: Beauty(plain=alpha(parts[1], parts[0]), filtered=parts[2].map(lambda held: alpha(held, parts[0])), terms=lit.default_value(()))
            )

        match self.recombination:
            case Lobes():
                kept = tuple((key, held) for key, held, _ in TERMS if not (held is ENVIRONMENT and scene.render.film_transparent))
                families = tuple((key, held) for key, held in kept if isinstance(held, Family))

                def term(key: Key, held: LightPass) -> Option[Outcome[tuple[Key, Expr, Option[Family]]]]:
                    match held:
                        case Descriptor():
                            return Some(socket(held.prop.name).map(lambda placed: (key, placed, Nothing)))
                        case family:
                            return lobed(flags.families[family], sockets, lobe).map(lambda placed: Ok((key, placed, Some(family))))

                def lobes(terms: Mapping[Key, Expr]) -> Outcome[Expr]:
                    strength = math("MULTIPLY", outer("occlusion"), outer("occlusion_strength"))
                    light = total(tuple(gained(terms[key], key) for key, _ in families if key in terms), ", ".join(key for key, _ in families))
                    return light.map(lambda lit: sum((gained(terms[key], key) for key, held in kept if not isinstance(held, Family)), mixed(strength, lit, terms["occlusion"], blend="MULTIPLY")))

                occluded = socket(OCCLUSION.prop.name).map(lambda source: ("occlusion", source, Nothing))
                return accumulated((*(answer for key, held in kept for answer in term(key, held).to_list()), occluded)).bind(lambda terms: finished(terms, lobes))
            case LightGroups(groups=groups):

                def grouped(terms: Mapping[Key, Expr]) -> Outcome[Expr]:
                    tinted = tuple(gained(mixed(1.0, terms[name], outer(f"group_{index}_tint"), blend="MULTIPLY"), f"group_{index}") for index, name in enumerate(groups, 1))
                    return total(tinted, "groups")

                return accumulated(socket(f"Combined_{name}") for name in groups).bind(lambda sources: finished(tuple((name, source, Nothing) for name, source in zip(groups, sources)), grouped))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EMISSION", "ENVIRONMENT", "OCCLUSION", "TERMS", "Beauty", "LightGroups", "LightPass", "Lit", "Lobes", "Recombination", "Recombined", "TermFilter", "views"]
