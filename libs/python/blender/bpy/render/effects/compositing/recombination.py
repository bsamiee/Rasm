"""Scene-linear beauty from light passes and light groups."""

from collections.abc import Callable
from enum import Enum
from itertools import starmap
from typing import Annotated, ClassVar, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
from msgspec import structs

from blender.bpy.data.rna.descriptors import Descriptor, IdName
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import End, ended, Fragment, INPUTS, math, mixed, node, NodeGroup, Operand, summed, vector
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.effects.detail.denoise import filing, Filters, Gate, gated, Pair, unit
from blender.bpy.render.view_layers.passes import Family, Filing, PassFlags, PassRequest, term
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
                families = frozenset(flag for family in flags.families.families.values() for flag in family.direct | family.indirect | family.color)
                return PassRequest(layer=self.layer, flags=families | frozenset(flags.singles.values()))
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
        outer: Callable[[Key], End],
        scene: bpy.types.Scene,
        offered: dict[str, str],
        lobe: NodeGroup,
        denoising: Option[Denoising],
        filed: Callable[[Filing, Key], BlenderResult[Fragment]],
    ) -> BlenderResult[tuple[Fragment, End]]:
        """Builds beauty sum, guided term filters, coverage, and filing."""
        render = f"{key}_render"
        filters = Filters.of()
        _, recombination = Recombined.__struct_fields__
        _, value = Gated.__struct_fields__

        def socket(name: str) -> BlenderResult[End]:
            return Option.of_optional(offered.get(name)).map(lambda identifier: End(node=render, socket=identifier)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(offered), key=name),))

        def gained(at: Key, source: Operand, path: tuple[Key | int | type[Case], ...]) -> BlenderResult[tuple[Fragment, End]]:
            return (
                math(f"{at}_power", "POWER", 2.0, outer(item_key(recombination, *path, value)))
                .bind(lambda power: math(f"{at}_gain", "MULTIPLY", power, outer(item_key(recombination, *path))))
                .bind(lambda gain: vector(at, "SCALE", source, gain))
            )

        def filtered(
            at: Key, source: tuple[Fragment, End], family: Option[Family], path: tuple[Key | int | type[Case], ...], target: type[Passes] | type[Groups]
        ) -> BlenderResult[tuple[Fragment, End]]:
            def built(held: Denoising) -> BlenderResult[tuple[Fragment, End]]:
                def control(inner: Key) -> End:
                    return End(node=INPUTS, socket=item_key(held.entry, inner))

                toggled = math(f"{at}_target", "MINIMUM", End(node=INPUTS, socket=held.entry), End(node=held.switch, socket=target.title))
                enabled = toggled.bind(lambda selected: math(f"{at}_enabled", "MINIMUM", selected, control(item_key(*path[:-1])))) if target is Passes else toggled
                return paired(unit(f"{at}_denoise", held.group, path, control, source, Pair.of(lambda name: End(node=render, socket=name), family)), enabled).bind(
                    lambda pair: gated(f"{at}_gate", source, pair[0], Gate(enabled=pair[1], amount=control(item_key("blend", "opacity"))))
                )

            return denoising.map(built).default_value(Ok(source))

        def total(at: Key, values: tuple[Operand, ...]) -> BlenderResult[Operand]:
            return summed(at, values) if values else Error((BlenderAbsentFault(owner=repr(offered), key=Family.DIFFUSE.socket("direct")),))

        def finished(parts: tuple[tuple[str, tuple[Fragment, End]], ...], beauty: Operand) -> BlenderResult[tuple[Fragment, End]]:
            output = socket("Alpha").bind(
                lambda alpha: node(f"{key}_alpha", bpy.types.CompositorNodeSetAlpha, {"Image": beauty, "Alpha": alpha, "Type": "Replace Alpha"}, {}).map(ended(f"{key}_alpha", "Image"))
            )
            filing_result = denoising.map(lambda _: filed(filing(tuple((label, source[1]) for label, source in parts)), f"{key}_denoised")).default_value(Ok(Fragment()))
            return paired(output, filing_result).bind(
                lambda pair: (
                    node(render, bpy.types.CompositorNodeRLayers, {}, {"scene": IdName.of(scene), "layer": self.layer})
                    .bind(lambda source: Fragment.merged((source, *(body for _, (body, _) in parts), pair[0][0], pair[1])))
                    .map(lambda joined: (joined, pair[0][1]))
                )
            )

        match self.recombination:
            case Lobes():

                def built_term(field: msgspec.structs.FieldInfo) -> Option[BlenderResult[tuple[str, tuple[Fragment, End]]]]:
                    source: Family | Single = getattr(self.sources, field.name)
                    at = f"{key}_{field.name}"
                    return (
                        term(at, source, ("direct", "indirect", "color"), render, offered, lobe).map(
                            lambda answer: answer.bind(lambda body: filtered(at, body, Some(source), getattr(filters.terms, field.name), Passes))
                        )
                        if isinstance(source, Family)
                        else Some(socket(source.socket).bind(lambda body: filtered(at, (Fragment(), body), Nothing, getattr(filters.terms, field.name), Passes)))
                    ).map(lambda answer: answer.map(lambda body: (field.name, body)))

                def summed_lobes(parts: tuple[tuple[str, tuple[Fragment, End]], ...]) -> BlenderResult[tuple[Fragment, End]]:
                    by_name = dict(parts)
                    light = accumulated(gained(f"{key}_{name}_scaled", source, (Lobes, "terms", name)) for name, source in parts if isinstance(getattr(self.sources, name), Family)).bind(
                        lambda values: total(f"{key}_lobes", values)
                    )
                    occlusion = socket(Single.OCCLUSION.socket).bind(lambda source: filtered(f"{key}_occlusion", (Fragment(), source), Nothing, filters.occlusion, Passes))
                    strength = math(f"{key}_occlusion_strength", "MULTIPLY", outer(item_key(recombination, Lobes, "occlusion")), outer(item_key(recombination, Lobes, "occlusion", value)))
                    extras = accumulated(
                        gained(f"{key}_{name}_scaled", by_name[name], (Lobes, "terms", name)) for name in ("emission", "environment") if name != "environment" or not scene.render.film_transparent
                    )
                    return paired(paired(light, extras), paired(occlusion, strength)).bind(
                        lambda built: (
                            mixed(f"{key}_occluded", "RGBA", built[1][1], built[0][0], built[1][0], "MULTIPLY")
                            .bind(lambda darkened: total(f"{key}_beauty", (darkened, *built[0][1])))
                            .bind(lambda beauty: finished((*parts, ("Ambient Occlusion", built[1][0])), beauty))
                        )
                    )

                return accumulated(answer for field in structs.fields(self.sources) for answer in built_term(field).to_list()).bind(summed_lobes)
            case LightGroups(groups=groups):

                def built_group(index: int, group: GroupGain) -> BlenderResult[tuple[str, tuple[Fragment, End]]]:
                    at = f"{key}_group_{index}"
                    return (
                        socket(f"Combined_{group.name}")
                        .bind(lambda source: filtered(at, (Fragment(), source), Nothing, filters.groups, Groups))
                        .bind(lambda source: mixed(f"{at}_tinted", "RGBA", 1.0, source, outer(item_key(recombination, LightGroups, "groups", index, "tint")), "MULTIPLY"))
                        .bind(lambda source: gained(at, source, (LightGroups, "groups", index, "gain")))
                        .map(lambda source: (group.name, source))
                    )

                return accumulated(starmap(built_group, enumerate(groups, 1))).bind(
                    lambda parts: total(f"{key}_beauty", tuple(source for _, source in parts)).bind(lambda beauty: finished(parts, beauty))
                )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Denoising", "RecombinationFlags", "Recombined", "Single", "views"]
