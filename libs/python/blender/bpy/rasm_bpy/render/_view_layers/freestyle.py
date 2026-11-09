"""Freestyle settings, line sets, and line styles of a view layer as one preset value, copied across used layers."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial
import itertools
import operator
from typing import ClassVar, Final

import bpy
from expression import Result, Some
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands.operators import Answer, Call, Empty, Operator
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.ownership import Ownership, Saved
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.data._rna.paths import rooted
from rasm_bpy.data._rna.snapshots import Applied, Snapshot
from rasm_bpy.render._view_layers.passes import _listed, _spread, _used
from rasm_bpy.settings._presets import Preset
from rasm_bpy.ui._layout.rows import _realized, ConditionalRow, OperatorButton, RowDraw
from rasm_bpy.ui._layout.sections import Section
from rasm_bpy.ui._layout.values import Listing
from rasm_records import accumulated, Error, Fault, Faults, Name, Ok, Outcome, paired, Reason

# --- [CONSTANTS] ------------------------------------------------------------------------

_SETTINGS, _LINESET = (
    Descriptor.saved(kind, lambda prop: not prop.is_readonly and not (isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.FreestyleLineStyle)))
    for kind in (bpy.types.FreestyleSettings, bpy.types.FreestyleLineSet)
)
_STACKS: Final = tuple(
    prop.identifier
    for prop in bpy.types.FreestyleLineStyle.bl_rna.properties
    if isinstance(prop, bpy.types.CollectionProperty) and prop.srna is not None and "new" in prop.srna.functions and "type" in prop.srna.functions["new"].parameters
)

# --- [MODELS] ---------------------------------------------------------------------------


class _StyleModifier(msgspec.Struct, frozen=True, kw_only=True):
    """Line style stack modifier."""

    name: Name
    kind: str


class _LineSet(msgspec.Struct, frozen=True, kw_only=True):
    """Line set values with its line style's values and modifiers per stack identifier."""

    name: Name
    values: Snapshot
    style: Snapshot
    modifiers: Mapping[str, tuple[_StyleModifier, ...]] = msgspec.field(default_factory=dict)


class _Freestyle(msgspec.Struct, frozen=True, kw_only=True):
    """Freestyle settings and line sets of one view layer."""

    settings: Snapshot
    linesets: tuple[_LineSet, ...] = ()


class _FreestyleRow(ConditionalRow, frozen=True):
    """Row of Freestyle presets and copy button."""

    preset: ClassVar[Preset[_Freestyle]] = Preset(key="freestyle", form=_Freestyle)
    ownership: ClassVar[Ownership[bpy.types.FreestyleLineStyle]] = Ownership(kind=bpy.types.FreestyleLineStyle, key="linestyle", saved=Saved.USED, single=False)

    def listing(self, identity: Identity) -> Listing[_Freestyle]:
        """Returns Freestyle presets over the context layer, applied to every used layer."""
        return _listed(self.preset, _captured, operator.eq, partial(_applied, identity))

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Draws preset listing and copy button."""
        return _realized((self.listing(identity), OperatorButton(target=freestyle_copy)), identity)

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns preset listing, copy operator, and line style ownership."""
        return self.listing(identity), freestyle_copy, self.ownership


# --- [OPERATIONS] -----------------------------------------------------------------------


def _captured(layer: bpy.types.ViewLayer) -> Outcome[_Freestyle]:
    """Captures layer Freestyle settings and line sets in layer order."""
    settings = layer.freestyle_settings

    def lineset(held: bpy.types.FreestyleLineSet) -> Outcome[_LineSet]:
        def styled(style: bpy.types.FreestyleLineStyle) -> Outcome[_LineSet]:
            modifiers = {stack: tuple(_StyleModifier(name=item.name, kind=item.type) for item in getattr(style, stack)) for stack in _STACKS}
            return paired(Snapshot.capture(held, Some(_LINESET)), Snapshot.capture(style)).map(lambda pair: _LineSet(name=held.name, values=pair[0], style=pair[1], modifiers=modifiers))

        return styled(held.linestyle)

    return paired(Snapshot.capture(settings, Some(_SETTINGS)), accumulated(map(lineset, settings.linesets))).map(lambda pair: _Freestyle(settings=pair[0], linesets=pair[1]))


def _applied(identity: Identity, freestyle: _Freestyle, layer: bpy.types.ViewLayer) -> Outcome[Applied]:
    """Writes Freestyle settings onto layer, then reconciles line sets by name in order, each line style's modifier stacks by name and type."""

    def kept[H, W](held: Iterable[H], wanted: Iterable[W], same: Callable[[H, W], bool]) -> int:
        return sum(1 for _ in itertools.takewhile(lambda pair: same(*pair), zip(held, wanted)))

    linesets = layer.freestyle_settings.linesets
    wanted = tuple(row.name for row in freestyle.linesets)
    prefix = wanted[: kept((held.name for held in linesets), wanted, operator.eq)]
    ownership = _FreestyleRow.ownership

    def stacked(row: _LineSet, style: bpy.types.FreestyleLineStyle, identifier: str) -> Outcome[None]:
        held = getattr(style, identifier)
        desired = row.modifiers.get(identifier, ())
        count = kept(held, desired, lambda modifier, modified: (modifier.name, modifier.type) == (modified.name, modified.kind))

        def removed(modifier: bpy.types.LineStyleModifier) -> Outcome[None]:
            return catch(held.remove, exception=RuntimeError)(modifier).map(lambda _: None).map_error(partial(Fault.of, Refusal.REFUSED, repr(style)))

        def added(modifier: _StyleModifier) -> Outcome[None]:
            try:
                held.new(name=modifier.name, type=modifier.kind)
            except (RuntimeError, TypeError) as error:
                return Error(Fault.of(Reason.OUTSIDE if isinstance(error, TypeError) else Refusal.REFUSED, f"{style!r}.{identifier}", error))
            return Ok(None)

        return accumulated(map(removed, tuple(held)[count:])).bind(lambda _: accumulated(map(added, desired[count:]))).map(lambda _: None)

    def built(row: _LineSet) -> Outcome[Applied]:
        match catch(lambda: linesets[row.name], exception=KeyError)():
            case Result(tag="ok", ok=held):
                current = held.linestyle
                style = (
                    Ok(current)
                    if current.is_editable and ownership.marked(identity, current).is_some() and current.users == 1
                    else ownership.created(identity, bpy.data.linestyles.new, f"{layer.name} {row.name}")
                )
            case _:
                held = linesets.new(row.name)
                created = held.linestyle
                created.name = f"{layer.name} {row.name}"
                style = ownership.owned(identity, created)

        def written(owned: bpy.types.FreestyleLineStyle) -> Outcome[Applied]:
            def finished() -> Outcome[Applied]:
                parts = (row.style.apply(owned), row.values.apply(held))
                return accumulated(part.outcome for part in parts).map(lambda _: Applied.join(parts))

            held.linestyle = owned
            return accumulated(stacked(row, owned, identifier) for identifier in _STACKS).bind(lambda _: finished())

        return style.bind(written)

    def removed(held: bpy.types.FreestyleLineSet) -> Outcome[None]:
        return catch(linesets.remove, exception=RuntimeError)(held).map_error(partial(Fault.of, Refusal.REFUSED, repr(layer.freestyle_settings)))

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
        lambda pair: _captured(pair[1]).bind(
            lambda value: _spread(
                value, tuple(layer for layer in _used(pair[0]) if layer != pair[1]), _captured, operator.eq, partial(_applied, call.identity), "Used view layers match Freestyle settings"
            )
        ),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

FREESTYLE: Final = Section(key="freestyle", rows=(_FreestyleRow(),))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FREESTYLE", "freestyle_copy"]
