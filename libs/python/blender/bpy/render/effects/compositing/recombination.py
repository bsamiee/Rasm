"""Scene-linear beauty from light passes and light groups."""

from collections.abc import Callable, Mapping
from enum import Enum
from typing import Annotated, ClassVar, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
from msgspec import structs

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import End, Expr, group_input, math, mixed, NodeGroup, switched
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.effects.detail.denoise import filing, Filters, Gate, gated, Pair, unit
from blender.bpy.render.effects.records import KeyPath
from blender.bpy.render.view_layers.passes import Family, Filing, lobed, PassFlags, PassRequest
from blender.bpy.scene.lights.groups import LightGroupEdits
from blender.bpy.settings.properties import Choices
from blender.imaging.compositing.recombination import GroupGain, LightGroups, Lobes, Recombination, Terms
from blender.imaging.effects.detail.denoise import Groups, Passes
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Gated

# --- [TYPES] ----------------------------------------------------------------------------


class Single(Enum):
    """Render Layers socket and pass flag of light outside lobe families."""

    EMISSION = "Emission", "use_pass_emit"
    ENVIRONMENT = "Environment", "use_pass_environment"
    OCCLUSION = "Ambient Occlusion", "use_pass_ambient_occlusion"

    def __init__(self, socket: str, flag: str) -> None:
        """Stores socket name and pass flag."""
        self.socket, self.flag = socket, flag


# --- [CHOICES] --------------------------------------------------------------------------


views = Choices(
    lambda _identity, _owner, context: (
        (layer.name, layer.name, "") for scene in Option.of_optional(context).bind(lambda held: Option.of_optional(held.scene)).to_list() for layer in scene.view_layers if layer.use
    )
)

# --- [MODELS] ---------------------------------------------------------------------------


class Denoising(msgspec.Struct, frozen=True, kw_only=True):
    """Filter group, entry panel key, and inlined target switch."""

    group: NodeGroup
    entry: Key
    switch: Key


class Beauty(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Recombined beauty, the beauty of denoised terms with the target item choosing it, and the denoised terms' filing."""

    plain: Expr
    denoised: Option[tuple[Expr, End]] = Nothing
    filing: Option[Expr] = Nothing


class RecombinationFlags(msgspec.Struct, frozen=True, kw_only=True):
    """Family and single pass flag descriptors."""

    families: PassFlags
    singles: dict[Single, Descriptor[bpy.types.BoolProperty]]

    @classmethod
    def of(cls) -> BlenderResult[Self]:
        """Resolves pass flag descriptors, accumulating refusals."""
        return paired(PassFlags.of(), accumulated(Descriptor.resolve(bpy.types.ViewLayer, single.flag, bpy.types.BoolProperty).map(lambda flag: (single, flag)) for single in Single)).map(
            lambda pair: cls(families=pair[0], singles=dict(pair[1]))
        )


class Recombined(Case, frozen=True, kw_only=True, tag=4):
    """View layer's light passes or light groups recombined into beauty."""

    title = "Recombined Passes"
    sources: ClassVar[Terms[Family | Single]] = Terms(
        diffuse=Family.DIFFUSE, glossy=Family.GLOSSY, transmission=Family.TRANSMISSION, volume=Family.VOLUME, emission=Single.EMISSION, environment=Single.ENVIRONMENT
    )
    layer: Annotated[str, msgspec.Meta(title="View Layer", description="Source view layer"), views]
    recombination: Annotated[Recombination, msgspec.Meta(title="Sum", description="Light passes or light groups summed into beauty")] = Lobes()

    def requested(self, flags: RecombinationFlags) -> PassRequest:
        """Requests lobe and single passes, or Cycles for light groups."""
        match self.recombination:
            case Lobes():
                return PassRequest(layer=self.layer, flags=flags.families.lobes | frozenset(flags.singles.values()))
            case LightGroups():
                return PassRequest(layer=self.layer, condition=LightGroupEdits.cycles)

    def synced(self, names: tuple[str, ...]) -> Self:
        """Orders gains by given names, new groups at default gain."""
        match self.recombination:
            case LightGroups(groups=groups):
                held = {group.name: group for group in groups}
                return structs.replace(self, recombination=LightGroups(groups=tuple(held[name] if name in held else GroupGain(name=name) for name in names)))
            case Lobes():
                return self

    def recombined(
        self,
        key: Key,
        outer: Callable[[Key], Expr],
        render: Expr,
        scene: bpy.types.Scene,
        offered: Mapping[str, str],
        lobe: NodeGroup,
        denoising: Option[Denoising],
        filed: Callable[[Filing, Key], BlenderResult[Expr]],
    ) -> BlenderResult[Beauty]:
        """Builds beauty from the layer's Render Layers node, each term gained, and with denoising the beauty of filtered terms.

        The denoised beauty never feeds the denoise effect, whose target menu chooses it, so the menu reading the plain beauty closes no cycle.

        Args:
            key: Layer key naming the filing.
            outer: Group input of a recombination field key.
            render: Render Layers node of the source view layer.
            scene: Scene whose transparent film drops the environment term.
            offered: Socket identifier per enabled Render Layers output name.
            lobe: Shared lobe total group.
            denoising: Spatial filter controls when the layer denoises its terms.
            filed: File Output node of a filing under a key.

        Returns:
            `Ok` of beauties with render alpha and the filing of terms the target denoises. `Error` of every absent pass and `absent` naming the terms a sum
            requested of none offered.
        """
        filters = Filters.of()
        _, recombination = Recombined.__struct_fields__
        _, value = Gated.__struct_fields__
        target = Passes if isinstance(self.recombination, Lobes) else Groups

        def socket(name: str) -> BlenderResult[Expr]:
            return (
                Option
                .of_optional(offered.get(name))
                .map(lambda identifier: structs.replace(render, socket=identifier, shape="RGBA"))
                .to_result_with(lambda: (BlenderAbsentFault(owner=repr(offered), key=name),))
            )

        def gained(source: Expr, path: KeyPath) -> Expr:
            return source * (math("POWER", 2.0, outer(item_key(recombination, *path, value))) * outer(item_key(recombination, *path)))

        def total(terms: tuple[Expr, ...], requested: str) -> BlenderResult[Expr]:
            return Ok(sum(terms[1:], terms[0])) if terms else Error((BlenderAbsentFault(owner=repr(offered), key=requested),))

        def filtered(held: Denoising, source: Expr, family: Option[Family], path: KeyPath) -> Expr:
            def control(inner: Key) -> Expr:
                return group_input(item_key(held.entry, inner))

            enabled = control(item_key(*path[:-1])) if target is Passes else 1.0
            pair = Pair.of(lambda name: structs.replace(render, socket=name), family)
            return gated(source, unit(held.group, path, control, source, pair), Gate(enabled=enabled, amount=control(item_key("blend", "opacity"))))

        def finished(terms: tuple[tuple[str, Expr, KeyPath, Option[Family]], ...], combined: Callable[[Mapping[str, Expr]], BlenderResult[Expr]]) -> BlenderResult[Beauty]:
            def alpha(beauty: Expr, mask: Expr) -> Expr:
                return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", beauty), ("Alpha", mask), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

            def denoised(held: Denoising) -> BlenderResult[tuple[Expr, End, Expr]]:
                chosen = End(node=held.switch, socket=target.title)
                cleaned = {label: filtered(held, source, family, path) for label, source, path, family in terms}
                selected = math("MINIMUM", group_input(held.entry), chosen)
                kept = tuple((label, switched(selected, source, cleaned[label])) for label, source, _, _ in terms)
                return paired(combined(cleaned), filed(filing(kept), f"{key}_denoised")).map(lambda parts: (parts[0], chosen, parts[1]))

            return paired(socket("Alpha"), combined({label: source for label, source, _, _ in terms}), denoising.map(lambda held: denoised(held).map(Some)).default_value(Ok(Nothing))).map(
                lambda parts: Beauty(plain=alpha(parts[1], parts[0]), denoised=parts[2].map(lambda held: (alpha(held[0], parts[0]), held[1])), filing=parts[2].map(lambda held: held[2]))
            )

        match self.recombination:
            case Lobes():
                families = tuple((name, source) for name in Terms.__struct_fields__ for source in (getattr(self.sources, name),) if isinstance(source, Family))

                def offering(name: str, source: Family | Single) -> Option[BlenderResult[tuple[str, Expr, KeyPath, Option[Family]]]]:
                    path = getattr(filters.terms, name)
                    match source:
                        case Family():
                            offers = {label: structs.replace(render, socket=identifier, shape="RGBA") for label, identifier in offered.items()}
                            return lobed(source, ("direct", "indirect", "color"), offers, lobe).map(lambda placed: Ok((name, placed, path, Some(source))))
                        case Single():
                            return Some(socket(source.socket).map(lambda placed: (name, placed, path, Nothing)))

                def lobes(terms: Mapping[str, Expr]) -> BlenderResult[Expr]:
                    strength = math("MULTIPLY", outer(item_key(recombination, Lobes, "occlusion")), outer(item_key(recombination, Lobes, "occlusion", value)))
                    extras = tuple(gained(terms[name], (Lobes, "terms", name)) for name in ("emission", "environment") if name != "environment" or not scene.render.film_transparent)
                    light = total(tuple(gained(terms[name], (Lobes, "terms", name)) for name, _ in families if name in terms), ", ".join(source.socket("direct") for _, source in families))
                    return light.map(lambda lit: sum(extras, mixed(strength, lit, terms[Single.OCCLUSION.socket], blend="MULTIPLY")))

                occlusion = socket(Single.OCCLUSION.socket).map(lambda source: (Single.OCCLUSION.socket, source, filters.occlusion, Nothing))
                listed = accumulated((*(answer for name in Terms.__struct_fields__ for answer in offering(name, getattr(self.sources, name)).to_list()), occlusion))
                return listed.bind(lambda terms: finished(terms, lobes))
            case LightGroups(groups=groups):

                def grouped(terms: Mapping[str, Expr]) -> BlenderResult[Expr]:
                    tinted = tuple(
                        gained(mixed(1.0, terms[group.name], outer(item_key(recombination, LightGroups, "groups", index, "tint")), blend="MULTIPLY"), (LightGroups, "groups", index, "gain"))
                        for index, group in enumerate(groups, 1)
                    )
                    return total(tinted, item_key(recombination, LightGroups, "groups"))

                sockets = accumulated(socket(f"Combined_{group.name}") for group in groups)
                return sockets.bind(lambda sources: finished(tuple((group.name, source, filters.groups, Nothing) for group, source in zip(groups, sources)), grouped))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Beauty", "Denoising", "RecombinationFlags", "Recombined", "Single", "views"]
