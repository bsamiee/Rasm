"""Freestyle settings, line sets, and line styles of a view layer as one preset value, copied across used layers."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial
import itertools
import operator
from types import MappingProxyType
from typing import ClassVar, Final

import bpy
from expression import Error, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Answer, Call, Empty, Operator
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.render.view_layers.passes import listed, spread, used
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import Line, OperatorButton, realized, RowDraw
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Listing
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason
from blender.records.fields import Name

# --- [CONSTANTS] ------------------------------------------------------------------------

_SETTINGS, _LINESET = (
    Descriptor.saved(kind, lambda prop: not prop.is_readonly and not (isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.FreestyleLineStyle)))
    for kind in (bpy.types.FreestyleSettings, bpy.types.FreestyleLineSet)
)
_STACKS: Final[Mapping[str, tuple[str, ...]]] = MappingProxyType({
    prop.identifier: tuple(item.identifier for item in parameter.enum_items)
    for prop in bpy.types.FreestyleLineStyle.bl_rna.properties
    if isinstance(prop, bpy.types.CollectionProperty) and prop.srna is not None and "new" in prop.srna.functions and "type" in prop.srna.functions["new"].parameters
    for parameter in (prop.srna.functions["new"].parameters["type"],)
    if isinstance(parameter, bpy.types.EnumProperty)
})

# --- [MODELS] ---------------------------------------------------------------------------


class StyleModifier(msgspec.Struct, frozen=True, kw_only=True):
    """Line style stack modifier."""

    name: Name
    kind: str


class LineSet(msgspec.Struct, frozen=True, kw_only=True):
    """Line set values with its line style's values and modifiers per stack identifier."""

    name: Name
    values: Snapshot
    style: Snapshot
    modifiers: Mapping[str, tuple[StyleModifier, ...]] = msgspec.field(default_factory=dict)


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
        return listed(self.preset, captured, operator.eq, partial(applied, identity))

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Draws preset listing and copy button."""
        return realized((self.listing(identity), OperatorButton(target=freestyle_copy)), identity)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns preset listing, copy operator, and line style ownership."""
        return Ok((self.listing(identity), freestyle_copy, self.ownership))


# --- [OPERATIONS] -----------------------------------------------------------------------


def captured(layer: bpy.types.ViewLayer) -> Outcome[Freestyle]:
    """Captures layer Freestyle settings and line sets in layer order."""
    settings = layer.freestyle_settings

    def lineset(held: bpy.types.FreestyleLineSet) -> Outcome[LineSet]:
        def styled(style: bpy.types.FreestyleLineStyle) -> Outcome[LineSet]:
            modifiers = {stack: tuple(StyleModifier(name=item.name, kind=item.type) for item in getattr(style, stack)) for stack in _STACKS}
            return paired(Snapshot.capture(held, Some(_LINESET)), Snapshot.capture(style)).map(lambda pair: LineSet(name=held.name, values=pair[0], style=pair[1], modifiers=modifiers))

        return Result[bpy.types.FreestyleLineStyle, Faults].bind(Option.of_optional(held.linestyle).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(held), member="linestyle")), styled)

    return paired(Snapshot.capture(settings, Some(_SETTINGS)), accumulated(map(lineset, settings.linesets))).map(lambda pair: Freestyle(settings=pair[0], linesets=pair[1]))


def applied(identity: Identity, freestyle: Freestyle, layer: bpy.types.ViewLayer) -> Outcome[Applied]:
    """Writes Freestyle settings onto layer, then reconciles line sets by name in order, each line style's modifier stacks by name and type."""

    def kept[H, W](held: Iterable[H], wanted: Iterable[W], same: Callable[[H, W], bool]) -> int:
        return sum(1 for _ in itertools.takewhile(lambda pair: same(*pair), zip(held, wanted)))

    linesets = layer.freestyle_settings.linesets
    wanted = tuple(row.name for row in freestyle.linesets)
    prefix = wanted[: kept((held.name for held in linesets), wanted, operator.eq)]
    ownership = FreestyleSection.ownership

    def stacked(row: LineSet, style: bpy.types.FreestyleLineStyle, identifier: str, kinds: tuple[str, ...]) -> Outcome[None]:
        held = getattr(style, identifier)
        desired = row.modifiers.get(identifier, ())
        count = kept(held, desired, lambda modifier, modified: (modifier.name, modifier.type) == (modified.name, modified.kind))

        def removed(modifier: bpy.types.LineStyleModifier) -> Outcome[None]:
            return catch(held.remove, exception=RuntimeError)(modifier).map_error(partial(Fault.of, Reason.REFUSED, repr(style)))

        def added(modifier: StyleModifier) -> Outcome[None]:
            return (
                catch(held.new, exception=RuntimeError)(name=modifier.name, type=modifier.kind).map(lambda _: None).map_error(partial(Fault.of, Reason.REFUSED, f"{style!r}.{identifier}"))
                if modifier.kind in kinds
                else Error(Fault.of(Reason.OUTSIDE, f"{style!r}.{identifier}", modifier.kind, member=", ".join(kinds)))
            )

        return accumulated(map(removed, tuple(held)[count:])).bind(lambda _: accumulated(map(added, desired[count:]))).map(lambda _: None)

    def built(row: LineSet) -> Outcome[Applied]:
        held = Option.of_optional(linesets.get(row.name)).default_with(lambda: linesets.new(row.name))
        style = (
            Option
            .of_optional(held.linestyle)
            .filter(lambda current: Mark.read(identity, current).filter(lambda mark: mark.key == ownership.key).is_some())
            .map(Ok)
            .default_with(
                lambda: ownership.made(
                    identity, lambda: catch(bpy.data.linestyles.new, exception=RuntimeError)(f"{layer.name} {row.name}").map_error(partial(Fault.of, Reason.REFUSED, f"{layer.name} {row.name}"))
                )
            )
        )

        def written(owned: bpy.types.FreestyleLineStyle) -> Outcome[Applied]:
            def finished() -> Outcome[Applied]:
                parts = (row.style.apply(owned), row.values.apply(held))
                return accumulated(part.outcome for part in parts).map(lambda _: Applied.join(parts))

            held.linestyle = owned
            return accumulated(itertools.starmap(partial(stacked, row, owned), _STACKS.items())).bind(lambda _: finished())

        return style.bind(written)

    def removed(held: bpy.types.FreestyleLineSet) -> Outcome[None]:
        return catch(linesets.remove, exception=RuntimeError)(held).map_error(partial(Fault.of, Reason.REFUSED, repr(layer.freestyle_settings)))

    first = freestyle.settings.apply(layer.freestyle_settings)
    return (
        first.outcome
        .bind(lambda _: accumulated(removed(held) for held in tuple(linesets) if held.name not in prefix))
        .bind(lambda _: accumulated(map(built, freestyle.linesets)))
        .map(lambda parts: Applied.join((first, *parts)))
    )


@Operator.of(Empty)
def freestyle_copy(call: Call[Empty]) -> Outcome[Answer]:
    """Copy active layer Freestyle settings and line sets to other used layers."""
    return Result[tuple[bpy.types.Scene, bpy.types.ViewLayer], Faults].bind(
        paired(rooted(bpy.types.Scene, call.context), rooted(bpy.types.ViewLayer, call.context)),
        lambda pair: captured(pair[1]).bind(
            lambda value: spread(
                value, tuple(layer for layer in used(pair[0]) if layer != pair[1]), captured, operator.eq, partial(applied, call.identity), "Used view layers match Freestyle settings"
            )
        ),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

FREESTYLE: Final = Section(key="freestyle", title="Freestyle", rows=(FreestyleSection(),))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FREESTYLE", "Freestyle", "FreestyleSection", "LineSet", "StyleModifier", "applied", "captured", "freestyle_copy"]
