"""Freestyle settings, line sets, and line styles as one value set."""

from collections.abc import Callable, Iterable, Mapping
import itertools
from types import MappingProxyType
from typing import Annotated, ClassVar, Final

import bpy
from expression import Error, Ok, Option, Result, Some
import msgspec

from blender.bpy.commands.operators import Call, Empty, Operator, Outcome, tallied, Unchanged
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import vacant
from blender.bpy.data.rna.descriptors import Descriptor, rooted
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import joined, Line, OperatorButton, placed, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.values import Listing
from blender.records.faults import accumulated, DomainFault, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

_LINESTYLE: Final = Descriptor.declared(bpy.types.FreestyleLineSet, "linestyle", bpy.types.PointerProperty)
_USE: Final = Descriptor.declared(bpy.types.ViewLayer, "use", bpy.types.BoolProperty)
_SETTINGS, _LINESET = (
    Descriptor.saved(kind, lambda prop: not prop.is_readonly and not (isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.FreestyleLineStyle)))
    for kind in (bpy.types.FreestyleSettings, bpy.types.FreestyleLineSet)
)
_STACKS: Final[Mapping[Descriptor[bpy.types.CollectionProperty], tuple[str, ...]]] = MappingProxyType({
    Descriptor.declared(bpy.types.FreestyleLineStyle, prop.identifier, bpy.types.CollectionProperty): tuple(item.identifier for item in parameter.enum_items)
    for prop in bpy.types.FreestyleLineStyle.bl_rna.properties
    if isinstance(prop, bpy.types.CollectionProperty) and prop.srna is not None and "new" in prop.srna.functions and "type" in prop.srna.functions["new"].parameters
    for parameter in (prop.srna.functions["new"].parameters["type"],)
    if isinstance(parameter, bpy.types.EnumProperty)
})

# --- [MODELS] ---------------------------------------------------------------------------


class StyleModifier(msgspec.Struct, frozen=True, kw_only=True):
    """Line style stack modifier."""

    name: Annotated[str, msgspec.Meta(min_length=1)]
    kind: str


class LineStyle(msgspec.Struct, frozen=True, kw_only=True):
    """Line style values and modifiers per stack identifier."""

    values: Snapshot
    modifiers: dict[str, tuple[StyleModifier, ...]] = msgspec.field(default_factory=dict)


class LineSet(msgspec.Struct, frozen=True, kw_only=True):
    """Line set values and line style."""

    name: Annotated[str, msgspec.Meta(min_length=1)]
    values: Snapshot
    style: LineStyle


class Freestyle(msgspec.Struct, frozen=True, kw_only=True):
    """Freestyle settings and line sets of one view layer."""

    settings: Snapshot
    linesets: tuple[LineSet, ...] = ()


class FreestyleSection(Line, frozen=True):
    """Row of Freestyle presets and copy button."""

    preset: ClassVar[Preset[Freestyle]] = Preset(key="freestyle", form=Freestyle)
    ownership: ClassVar[Ownership[bpy.types.FreestyleLineStyle]] = Ownership(kind=bpy.types.FreestyleLineStyle, key="linestyle", saved=Saved.USED, single=False)

    def listing(self, identity: Identity) -> Listing[Freestyle]:
        """Returns Freestyle presets over the context layer, applied to every used layer."""
        return Listing(
            preset=self.preset,
            current=lambda context: rooted(bpy.types.ViewLayer, context).bind(captured),
            matches=lambda context, values: rooted(bpy.types.ViewLayer, context).bind(captured).map(lambda held: tuple(held == value for value in values)),
            applied=lambda context, value: rooted(bpy.types.Scene, context).bind(lambda scene: onto(value, used(scene), identity)),
        )

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Draws preset listing and copy button."""
        return placed((self.listing(identity), OperatorButton(target=COPY)), identity).map(joined)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns preset listing, copy operator, and line style ownership."""
        return Ok((self.listing(identity), COPY, self.ownership))

    def section(self) -> Single[Empty]:
        """Returns Freestyle section."""
        return Single(key="freestyle", title="Freestyle", rows=(self,))


# --- [OPERATIONS] -----------------------------------------------------------------------


def kept[H, W](held: Iterable[H], wanted: Iterable[W], same: Callable[[H, W], bool]) -> int:
    """Returns the length of the prefix where held and wanted items pair as the same."""
    return sum(1 for _ in itertools.takewhile(lambda pair: same(*pair), zip(held, wanted)))


def used(scene: bpy.types.Scene) -> tuple[bpy.types.ViewLayer, ...]:
    """Returns scene layers with `use` on."""
    return tuple(layer for layer in scene.view_layers if bool(_USE.read(layer)))


def captured(layer: bpy.types.ViewLayer) -> BlenderResult[Freestyle]:
    """Captures layer Freestyle settings and line sets in layer order."""
    settings = layer.freestyle_settings

    def lineset(held: bpy.types.FreestyleLineSet) -> BlenderResult[LineSet]:
        def styled(style: bpy.types.FreestyleLineStyle) -> BlenderResult[LineSet]:
            modifiers = {stack.prop.identifier: tuple(StyleModifier(name=item.name, kind=item.type) for item in getattr(style, stack.prop.identifier)) for stack in _STACKS}
            return paired(Snapshot.capture(held, Some(_LINESET)), Snapshot.capture(style)).map(
                lambda pair: LineSet(name=held.name, values=pair[0], style=LineStyle(values=pair[1], modifiers=modifiers))
            )

        return Result[bpy.types.FreestyleLineStyle, BlenderFaults].bind(Option.of_optional(held.linestyle).to_result_with(lambda: (BlenderAbsentFault(owner=repr(held), key="linestyle"),)), styled)

    return paired(Snapshot.capture(settings, Some(_SETTINGS)), accumulated(map(lineset, settings.linesets))).map(lambda pair: Freestyle(settings=pair[0], linesets=pair[1]))


def styled(row: LineStyle, style: bpy.types.FreestyleLineStyle) -> BlenderResult[Applied]:
    """Matches line style modifiers to captured ones, then applies captured values."""

    def stacked(stack: Descriptor[bpy.types.CollectionProperty], kinds: tuple[str, ...]) -> BlenderResult[None]:
        identifier = stack.prop.identifier
        held = getattr(style, identifier)
        wanted = row.modifiers.get(identifier, ())
        count = kept(held, wanted, lambda modifier, desired: (modifier.name, modifier.type) == (desired.name, desired.kind))

        def removed(modifier: bpy.types.LineStyleModifier) -> BlenderResult[None]:
            return BlenderEditFault.attempted(repr(style), lambda: held.remove(modifier))

        def added(modifier: StyleModifier) -> BlenderResult[None]:
            return (
                BlenderEditFault.attempted(f"{style!r}.{identifier}", lambda: held.new(name=modifier.name, type=modifier.kind)).map(lambda _: None)
                if modifier.kind in kinds
                else Error((DomainFault(subject=f"{style!r}.{identifier}", value=modifier.kind, accepted=", ".join(kinds)),))
            )

        return accumulated(map(removed, tuple(held)[count:])).bind(lambda _: accumulated(map(added, wanted[count:]))).map(lambda _: None)

    def written() -> BlenderResult[Applied]:
        applied = row.values.apply(style)
        return applied.outcome.map(lambda _: applied)

    return accumulated(itertools.starmap(stacked, _STACKS.items())).bind(lambda _: written())


def applied(freestyle: Freestyle, layer: bpy.types.ViewLayer, identity: Identity) -> BlenderResult[Applied]:
    """Writes Freestyle settings onto layer, then reconciles line sets by name in order."""
    linesets = layer.freestyle_settings.linesets
    wanted = tuple(row.name for row in freestyle.linesets)
    prefix = wanted[: kept((held.name for held in linesets), wanted, lambda current, desired: current == desired)]
    ownership = FreestyleSection.ownership

    def built(row: LineSet) -> BlenderResult[Applied]:
        held = Option.of_optional(linesets.get(row.name)).default_with(lambda: linesets.new(row.name))
        style = (
            Option
            .of_optional(held.linestyle)
            .filter(lambda current: Mark.read(identity, current).filter(lambda mark: mark.key == ownership.key).is_some())
            .map(Ok)
            .default_with(
                lambda: ownership.made(
                    identity, lambda: BlenderEditFault.attempted(f"{layer.name} {row.name}", lambda: bpy.data.linestyles.new(vacant(f"{layer.name} {row.name}", bpy.data.linestyles.keys())))
                )
            )
        )

        def values(outcome: Applied) -> BlenderResult[Applied]:
            applied = row.values.apply(held)
            return applied.outcome.map(lambda _: Applied.join((applied, outcome)))

        return style.bind(lambda owned: _LINESTYLE.write(held, owned).bind(lambda _: styled(row.style, owned))).bind(values)

    def removed(held: bpy.types.FreestyleLineSet) -> BlenderResult[None]:
        return BlenderEditFault.attempted(repr(layer.freestyle_settings), lambda: linesets.remove(held))

    first = freestyle.settings.apply(layer.freestyle_settings)
    return (
        first.outcome
        .bind(lambda _: accumulated(removed(held) for held in tuple(linesets) if held.name not in prefix))
        .bind(lambda _: accumulated(map(built, freestyle.linesets)))
        .map(lambda parts: Applied.join((first, *parts)))
    )


def onto(value: Freestyle, layers: tuple[bpy.types.ViewLayer, ...], identity: Identity) -> BlenderResult[Outcome]:
    """Applies Freestyle settings to each layer whose capture differs."""

    def wrote(current: tuple[Freestyle, ...]) -> BlenderResult[Outcome]:
        if not (differing := tuple(layer for layer, held in zip(layers, current) if held != value)):
            return Ok(Unchanged(reason="Used view layers match Freestyle settings"))
        return tallied(tuple(applied(value, layer, identity) for layer in differing))

    return accumulated(map(captured, layers)).bind(wrote)


def copying(call: Call[Empty]) -> BlenderResult[Outcome]:
    """Copies the active layer's Freestyle settings onto the other used layers."""
    return Result[tuple[bpy.types.Scene, bpy.types.ViewLayer], BlenderFaults].bind(
        paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)),
        lambda pair: captured(pair[1]).bind(lambda value: onto(value, tuple(layer for layer in used(pair[0]) if layer != pair[1]), call.identity)),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

COPY: Final = Operator(key="freestyle_copy", title="Copy Lines to Used Layers", description="Copy active layer Freestyle settings and line sets to other used layers", form=Empty, run=copying)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COPY", "Freestyle", "FreestyleSection", "LineSet", "LineStyle", "StyleModifier", "applied", "captured", "copying", "kept", "onto", "styled", "used"]
