"""Freestyle settings, line sets, and line styles as one value set."""

import itertools
import operator
from typing import Annotated, ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Empty, Operator, Outcome, tallied, Unchanged
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import vacant
from blender.bpy.data.rna.descriptors import Descriptor, rooted
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import joined, Line, OperatorButton, placed, Row, RowDraw
from blender.bpy.ui.layout.sections import Listing, Single
from blender.imaging.faults import accumulated, paired

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


class Stack(msgspec.Struct, frozen=True, kw_only=True):
    """Line style modifier stack and types its `new` accepts."""

    collection: Descriptor[bpy.types.CollectionProperty]
    kinds: tuple[str, ...]


class LineMembers(msgspec.Struct, frozen=True, kw_only=True):
    """RNA members `captured` and `applied` read, resolved once."""

    settings: tuple[Descriptor, ...]
    lineset: tuple[Descriptor, ...]
    stacks: tuple[Stack, ...]
    linestyle: Descriptor[bpy.types.PointerProperty]
    use: Descriptor[bpy.types.BoolProperty]

    @classmethod
    def of(cls) -> BlenderResult["LineMembers"]:
        """Resolves members, faulting each one the build lacks."""

        def selection(kind: type[bpy.types.FreestyleSettings | bpy.types.FreestyleLineSet]) -> tuple[Descriptor, ...]:
            struct: bpy.types.Struct = kind.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return tuple(
                Descriptor(root=kind, owner="", prop=prop)
                for prop in struct.properties
                if not prop.is_readonly and prop != struct.name_property and not (isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.FreestyleLineStyle))
            )

        def stacked(prop: bpy.types.CollectionProperty) -> Option[Stack]:
            return (
                Option
                .of_optional(prop.srna)
                .bind(lambda api: Option.of_optional(api.functions.get("new")))
                .bind(lambda function: Option.of_optional(function.parameters.get("type")))
                .bind(lambda parameter: Some(tuple(item.identifier for item in parameter.enum_items)) if isinstance(parameter, bpy.types.EnumProperty) else Nothing)  # type: ignore[var-annotated]
                .map(lambda found: Stack(collection=Descriptor(root=bpy.types.FreestyleLineStyle, owner="", prop=prop), kinds=found))
            )

        stacks = tuple(stack for prop in bpy.types.FreestyleLineStyle.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty) for stack in stacked(prop).to_list())

        def built(linestyle: Descriptor[bpy.types.PointerProperty], use: Descriptor[bpy.types.BoolProperty]) -> LineMembers:
            return cls(settings=selection(bpy.types.FreestyleSettings), lineset=selection(bpy.types.FreestyleLineSet), stacks=stacks, linestyle=linestyle, use=use)

        return paired(Descriptor.resolve(bpy.types.FreestyleLineSet, "linestyle", bpy.types.PointerProperty), Descriptor.resolve(bpy.types.ViewLayer, "use", bpy.types.BoolProperty)).map(
            lambda pair: built(*pair)
        )


class FreestyleSection(Line, frozen=True):
    """Row of Freestyle presets and copy button."""

    preset: ClassVar[Preset[Freestyle]] = Preset(key="freestyle", form=Freestyle)
    ownership: ClassVar[Ownership[bpy.types.FreestyleLineStyle]] = Ownership(kind=bpy.types.FreestyleLineStyle, key="linestyle", saved=Saved.USED, single=False)

    def parts(self, identity: Identity) -> BlenderResult[tuple[LineMembers, Listing[Freestyle], Operator[Empty]]]:
        """Builds members, preset listing, and copy operator."""

        def built(members: LineMembers) -> tuple[LineMembers, Listing[Freestyle], Operator[Empty]]:
            def onto(value: Freestyle, layers: tuple[bpy.types.ViewLayer, ...]) -> BlenderResult[Outcome]:
                def wrote(current: tuple[Freestyle, ...]) -> BlenderResult[Outcome]:
                    if not (differing := tuple(layer for layer, held in zip(layers, current) if held != value)):
                        return Ok(Unchanged(reason="Used view layers match Freestyle settings"))
                    return tallied(tuple(applied(value, layer, identity, members) for layer in differing))

                return accumulated(captured(layer, members) for layer in layers).bind(wrote)

            def used(scene: bpy.types.Scene) -> tuple[bpy.types.ViewLayer, ...]:
                return tuple(layer for layer in scene.view_layers if bool(members.use.read(layer)))

            listing = Listing(
                preset=self.preset,
                current=lambda context: rooted(bpy.types.ViewLayer, context).bind(lambda layer: captured(layer, members)),
                matches=lambda context, values: rooted(bpy.types.ViewLayer, context).bind(lambda layer: captured(layer, members)).map(lambda held: tuple(held == value for value in values)),
                applied=lambda context, value: rooted(bpy.types.Scene, context).bind(lambda scene: onto(value, used(scene))),
            )
            copy = Operator(
                key="freestyle_copy",
                title="Copy Lines to Used Layers",
                description="Copy active layer Freestyle settings and line sets to other used layers",
                form=Empty,
                run=lambda call: Result[tuple[bpy.types.Scene, bpy.types.ViewLayer], BlenderFaults].bind(
                    paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)),
                    lambda pair: captured(pair[1], members).bind(lambda value: onto(value, tuple(layer for layer in used(pair[0]) if layer != pair[1]))),
                ),
            )
            return members, listing, copy

        return LineMembers.of().map(built)

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Draws preset listing and copy button."""

        def rows(_: LineMembers, listing: Listing[Freestyle], copy: Operator[Empty]) -> tuple[Row, ...]:
            return (listing, OperatorButton(target=copy))

        return self.parts(identity).bind(lambda parts: placed(rows(*parts), identity)).map(joined)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns preset listing, copy operator, and line style ownership."""

        def acquired(_: LineMembers, listing: Listing[Freestyle], copy: Operator[Empty]) -> Acquisitions:
            return listing, copy, self.ownership

        return self.parts(identity).map(lambda parts: acquired(*parts))

    def section(self) -> Single[Empty]:
        """Returns Freestyle section."""
        return Single(key="freestyle", title="Freestyle", rows=(self,))


# --- [OPERATIONS] -----------------------------------------------------------------------


def captured(layer: bpy.types.ViewLayer, members: LineMembers) -> BlenderResult[Freestyle]:
    """Captures layer Freestyle settings and line sets in layer order."""
    settings = layer.freestyle_settings

    def lineset(held: bpy.types.FreestyleLineSet) -> BlenderResult[LineSet]:
        modifiers = {
            stack.collection.prop.identifier: tuple(StyleModifier(name=item.name, kind=item.type) for item in getattr(held.linestyle, stack.collection.prop.identifier)) for stack in members.stacks
        }
        return paired(Snapshot.capture(held, Some(members.lineset)), Snapshot.capture(held.linestyle)).map(
            lambda pair: LineSet(name=held.name, values=pair[0], style=LineStyle(values=pair[1], modifiers=modifiers))
        )

    return paired(Snapshot.capture(settings, Some(members.settings)), accumulated(map(lineset, settings.linesets))).map(lambda pair: Freestyle(settings=pair[0], linesets=pair[1]))


def styled(row: LineStyle, style: bpy.types.FreestyleLineStyle, stacks: tuple[Stack, ...]) -> BlenderResult[Applied]:
    """Matches line style modifiers to captured ones, then applies captured values."""

    def stacked(stack: Stack) -> BlenderResult[None]:
        identifier = stack.collection.prop.identifier
        held = getattr(style, identifier)
        wanted = row.modifiers.get(identifier, ())
        kept = sum(itertools.takewhile(bool, map(operator.eq, ((modifier.name, modifier.type) for modifier in held), ((modifier.name, modifier.kind) for modifier in wanted))))
        remaining = tuple(held)[kept:]

        def removed(modifier: bpy.types.LineStyleModifier) -> BlenderResult[None]:
            return catch(lambda: held.remove(modifier), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(style), error=error),))

        def added(name: str, kind: str) -> BlenderResult[None]:
            return (
                catch(lambda: held.new(name=name, type=kind), exception=RuntimeError)().map(lambda _: None).map_error(lambda error: (BlenderEditFault(request=f"{style!r}.{identifier}", error=error),))
                if kind in stack.kinds
                else Error((BlenderChoiceFault(path=f"{style!r}.{identifier}", identifier=kind, accepted=stack.kinds),))
            )

        return accumulated(map(removed, remaining)).bind(lambda _: accumulated(added(modifier.name, modifier.kind) for modifier in wanted[kept:])).map(lambda _: None)

    def written() -> BlenderResult[Applied]:
        applied = row.values.apply(style)
        return applied.outcome.map(lambda _: applied)

    return accumulated(map(stacked, stacks)).bind(lambda _: written())


def applied(freestyle: Freestyle, layer: bpy.types.ViewLayer, identity: Identity, members: LineMembers) -> BlenderResult[Applied]:
    """Writes Freestyle settings onto layer, then reconciles line sets by name in order."""
    linesets = layer.freestyle_settings.linesets
    wanted = tuple(row.name for row in freestyle.linesets)
    kept = wanted[: len(tuple(itertools.takewhile(bool, map(operator.eq, (held.name for held in linesets), wanted))))]
    ownership = FreestyleSection.ownership

    def built(row: LineSet) -> BlenderResult[Applied]:
        held = Option.of_optional(linesets.get(row.name)).default_with(lambda: linesets.new(row.name))
        style = (
            Option
            .of_optional(held.linestyle)
            .filter(lambda current: Mark.read(identity, current).filter(lambda mark: mark.key == ownership.key).is_some())
            .map(Ok)
            .default_with(lambda: ownership.made(identity, lambda: Ok(bpy.data.linestyles.new(vacant(f"{layer.name} {row.name}", bpy.data.linestyles.keys())))))
        )

        def values(outcome: Applied) -> BlenderResult[Applied]:
            applied = row.values.apply(held)
            return applied.outcome.map(lambda _: Applied.join((applied, outcome)))

        return style.bind(lambda owned: members.linestyle.write(held, owned).bind(lambda _: styled(row.style, owned, members.stacks))).bind(values)

    def removed(held: bpy.types.FreestyleLineSet) -> BlenderResult[None]:
        return catch(lambda: linesets.remove(held), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(layer.freestyle_settings), error=error),))

    first = freestyle.settings.apply(layer.freestyle_settings)
    return (
        first.outcome
        .bind(lambda _: accumulated(removed(held) for held in tuple(linesets) if held.name not in kept))
        .bind(lambda _: accumulated(map(built, freestyle.linesets)))
        .map(lambda parts: Applied.join((first, *parts)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Freestyle", "FreestyleSection", "LineMembers", "LineSet", "LineStyle", "Stack", "StyleModifier", "applied", "captured", "styled"]
