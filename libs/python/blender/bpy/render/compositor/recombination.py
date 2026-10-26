"""Beauty recombined from a view layer's light passes or light groups, each summed term under its own gain, every term read through the pass that renders it."""

from collections.abc import Callable, Mapping
from typing import Annotated, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import Expr, math, mixed, NodeGroup
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.view_layers.passes import Family, lobed, PassFlags, PassRequest
from blender.bpy.scene.lights.groups import LightGroupEdits
from blender.bpy.settings.properties import Choices
from blender.imaging.color.processors import Stops
from blender.records.faults import accumulated, paired
from blender.records.fields import Case, Gated, markers, Presentation, Rgb, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type LightPass = Family | Descriptor[bpy.types.BoolProperty]
type Recombination = Lobes | LightGroups
type TermFilter = Callable[[Key, Expr, Option[Family], Expr], Expr]

# --- [CONSTANTS] ------------------------------------------------------------------------

EMISSION = Descriptor.declared(bpy.types.ViewLayer, "use_pass_emit", bpy.types.BoolProperty)
ENVIRONMENT = Descriptor.declared(bpy.types.ViewLayer, "use_pass_environment", bpy.types.BoolProperty)
OCCLUSION = Descriptor.declared(bpy.types.ViewLayer, "use_pass_ambient_occlusion", bpy.types.BoolProperty)

# --- [CHOICES] --------------------------------------------------------------------------

views = Choices.named(
    lambda _identity, _owner, context: (layer.name for scene in Option.of_optional(context).bind(lambda held: Option.of_optional(held.scene)).to_list() for layer in scene.view_layers if layer.use)
)

# --- [MODELS] ---------------------------------------------------------------------------


class Lobes(Case, frozen=True, kw_only=True):
    """Light passes rebuilt under one gain per summed term and ambient occlusion, each gain marked with the family or pass flag rendering its term."""

    title = "Light Passes"
    diffuse: Annotated[Gated[Stops], msgspec.Meta(description="Diffuse light gain"), Family.DIFFUSE] = Gated(enabled=True, value=0.0)
    glossy: Annotated[Gated[Stops], msgspec.Meta(description="Glossy light gain"), Family.GLOSSY] = Gated(enabled=True, value=0.0)
    transmission: Annotated[Gated[Stops], msgspec.Meta(description="Transmitted light gain"), Family.TRANSMISSION] = Gated(enabled=True, value=0.0)
    volume: Annotated[Gated[Stops], msgspec.Meta(description="Volume light gain"), Family.VOLUME] = Gated(enabled=True, value=0.0)
    emission: Annotated[Gated[Stops], msgspec.Meta(description="Gain of emitters seen by camera"), EMISSION] = Gated(enabled=True, value=0.0)
    environment: Annotated[Gated[Stops], msgspec.Meta(description="Gain of background seen by camera, dropped under transparent film"), ENVIRONMENT] = Gated(enabled=True, value=0.0)
    occlusion: Annotated[Gated[Unit], msgspec.Meta(description="Ambient occlusion strength over family light"), Presentation(soft=Some((0.0, 1.0)))] = Gated(enabled=False, value=1.0)

    @classmethod
    def terms(cls) -> tuple[tuple[msgspec.structs.FieldInfo, LightPass], ...]:
        """Gain fields with the family or pass flag of their term, in declaration order."""

        def marked(field: msgspec.structs.FieldInfo) -> tuple[LightPass, ...]:
            return (*markers(field.type, Family), *markers(field.type, Descriptor))

        return tuple((field, held) for field in msgspec.structs.fields(cls) for held in marked(field)[:1])


class GroupGain(msgspec.Struct, frozen=True, kw_only=True):
    """Gain and tint of a view layer light group."""

    name: Annotated[str, msgspec.Meta(title="Light Group", description="View layer light group")]
    gain: Annotated[Gated[Stops], msgspec.Meta(description="Group light in stops")] = Gated(enabled=True, value=0.0)
    tint: Annotated[Rgb, msgspec.Meta(description="Color multiplying group light")] = Rgb(red=1.0, green=1.0, blue=1.0)


class LightGroups(Case, frozen=True, kw_only=True):
    """Light-group passes summed under group gains and tints."""

    groups: Annotated[tuple[GroupGain, ...], msgspec.Meta(description="Gain and tint per light group")] = ()


class Lit(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """One summed term: its record key, Render Layers pass, and the pass after a term filter."""

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

    title = "Recombined Passes"
    layer: Annotated[str, msgspec.Meta(title="View Layer", description="Source view layer"), views]
    recombination: Annotated[Recombination, msgspec.Meta(title="Sum", description="Light passes or light groups summed into beauty")] = Lobes()

    def requested(self, flags: PassFlags) -> PassRequest:
        """Requests every family, term, and occlusion pass flag, or Cycles for light groups."""
        match self.recombination:
            case Lobes():
                passes = frozenset().union(*(slot for _, held in Lobes.terms() if isinstance(held, Family) for slot in flags.families[held]))
                return PassRequest(layer=self.layer, flags=passes | {held for _, held in Lobes.terms() if isinstance(held, Descriptor)} | {OCCLUSION})
            case LightGroups():
                return PassRequest(layer=self.layer, condition=LightGroupEdits.cycles)

    def synced(self, names: tuple[str, ...]) -> Self:
        """Orders gains by given names, new groups at default gain."""
        match self.recombination:
            case LightGroups(groups=groups):
                held = {group.name: group for group in groups}
                return msgspec.structs.replace(self, recombination=LightGroups(groups=tuple(held[name] if name in held else GroupGain(name=name) for name in names)))
            case Lobes():
                return self

    def recombined(
        self, outer: Callable[[Key], Expr], render: Expr, scene: bpy.types.Scene, offered: Mapping[str, str], lobe: NodeGroup, filtered: Option[TermFilter] = Nothing
    ) -> BlenderResult[Beauty]:
        """Builds beauty from the layer's Render Layers node, each term gained, and under a term filter the beauty of filtered terms.

        Args:
            outer: Group input of a recombination field key.
            render: Render Layers node of the source view layer.
            scene: Scene whose transparent film drops the environment term.
            offered: Socket identifier per enabled Render Layers output name.
            lobe: Shared lobe total group.
            filtered: Filter of each term by record key, pass, family, and the Render Layers node its guides read.

        Returns:
            `Ok` of beauties with render alpha. `Error` of every absent pass and `absent` naming the sum when no term is offered.
        """
        recombination = "recombination"
        sockets = {name: msgspec.structs.replace(render, socket=identifier, shape="RGBA") for name, identifier in offered.items()}

        def socket(name: str) -> BlenderResult[Expr]:
            return Option.of_optional(sockets.get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(tuple(offered)), key=name),))

        def gained(source: Expr, *path: Key | int | type[Case]) -> Expr:
            return source * (math("POWER", 2.0, outer(item_key(recombination, *path, "value"))) * outer(item_key(recombination, *path)))

        def total(terms: tuple[Expr, ...], requested: Key) -> BlenderResult[Expr]:
            return Ok(sum(terms[1:], terms[0])) if terms else Error((BlenderAbsentFault(owner=repr(tuple(offered)), key=requested),))

        def finished(terms: tuple[tuple[Key, Expr, Option[Family]], ...], combined: Callable[[Mapping[Key, Expr]], BlenderResult[Expr]]) -> BlenderResult[Beauty]:
            def alpha(beauty: Expr, mask: Expr) -> Expr:
                return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", beauty), ("Alpha", mask), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

            lit = filtered.map(lambda term: tuple(Lit(key=key, source=source, filtered=term(key, source, family, render)) for key, source, family in terms))
            cleaned = lit.map(lambda held: combined({term.key: term.filtered for term in held}).map(Some)).default_value(Ok(Nothing))
            return paired(socket("Alpha"), combined({key: source for key, source, _ in terms}), cleaned).map(
                lambda parts: Beauty(plain=alpha(parts[1], parts[0]), filtered=parts[2].map(lambda held: alpha(held, parts[0])), terms=lit.default_value(()))
            )

        match self.recombination:
            case Lobes():
                kept = tuple((field.name, held) for field, held in Lobes.terms() if not (held is ENVIRONMENT and scene.render.film_transparent))
                families = tuple((name, held) for name, held in kept if isinstance(held, Family))

                def term(name: Key, held: LightPass) -> Option[BlenderResult[tuple[Key, Expr, Option[Family]]]]:
                    match held:
                        case Family():
                            return lobed(held, ("direct", "indirect", "color"), sockets, lobe).map(lambda placed: Ok((name, placed, Some(held))))
                        case Descriptor():
                            return Some(socket(held.prop.name).map(lambda placed: (name, placed, Nothing)))

                def lobes(terms: Mapping[Key, Expr]) -> BlenderResult[Expr]:
                    occlusion = "occlusion"
                    strength = math("MULTIPLY", outer(item_key(recombination, Lobes, occlusion)), outer(item_key(recombination, Lobes, occlusion, "value")))
                    light = total(tuple(gained(terms[name], Lobes, name) for name, _ in families if name in terms), ", ".join(family.label for _, family in families))
                    return light.map(
                        lambda lit: sum((gained(terms[name], Lobes, name) for name, held in kept if not isinstance(held, Family)), mixed(strength, lit, terms[occlusion], blend="MULTIPLY"))
                    )

                occluded = socket(OCCLUSION.prop.name).map(lambda source: ("occlusion", source, Nothing))
                return accumulated((*(answer for name, held in kept for answer in term(name, held).to_list()), occluded)).bind(lambda terms: finished(terms, lobes))
            case LightGroups(groups=groups):

                def grouped(terms: Mapping[Key, Expr]) -> BlenderResult[Expr]:
                    tinted = tuple(
                        gained(mixed(1.0, terms[group.name], outer(item_key(recombination, LightGroups, "groups", index, "tint")), blend="MULTIPLY"), LightGroups, "groups", index, "gain")
                        for index, group in enumerate(groups, 1)
                    )
                    return total(tinted, item_key(recombination, LightGroups, "groups"))

                return accumulated(socket(f"Combined_{group.name}") for group in groups).bind(
                    lambda sources: finished(tuple((group.name, source, Nothing) for group, source in zip(groups, sources)), grouped)
                )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EMISSION", "ENVIRONMENT", "OCCLUSION", "Beauty", "GroupGain", "LightGroups", "LightPass", "Lit", "Lobes", "Recombination", "Recombined", "TermFilter", "views"]
