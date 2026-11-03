"""Member bindings rooted at an owner type or an attachment, resolved on the root type or a live root and located at the drawing holder or context."""

from collections.abc import Callable, Mapping
from functools import partial
from itertools import pairwise
from typing import Any, overload, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.conditions import Scoped
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import Native, rna, RnaPath, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Declaration
from blender.bpy.extension.reports import alerted
from blender.bpy.settings.attachments import Attachment
from blender.records.faults import accumulated, Fault, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type At = tuple[Native, str]
type Reach = Callable[[bpy.types.Context], Option[Struct]]
type LocatedMembers = tuple[tuple[ResolvedMember, At], ...]
type _Kinds = tuple[type[Struct], ...]

# --- [MODELS] ---------------------------------------------------------------------------


class ResolvedMember[P: bpy.types.Property = bpy.types.Property](msgspec.Struct, frozen=True, kw_only=True):
    """RNA definition with the reach of its owner's root, the drawing holder or the context root while no reach is stated."""

    descriptor: Descriptor[P]
    reach: Option[Reach] = Nothing

    def at(self, holder: Struct, context: bpy.types.Context) -> Option[At]:
        """Locates the property owner through the reach, the drawing holder, or the context root."""
        descriptor = self.descriptor
        owner = self.reach.map(lambda reach: reach(context).bind(descriptor.owner_of)).default_with(lambda: descriptor.owner_of(context, Some(holder)))
        return owner.map(lambda found: (found, descriptor.prop.identifier))

    def accepts(self, kinds: _Kinds) -> Outcome[Self]:
        """Accepts a property or pointer target of the kinds, every kind while none is given."""
        prop = self.descriptor.prop
        target = prop.fixed_type if isinstance(prop, bpy.types.PointerProperty) else prop
        return Ok(self).filter_with(
            lambda _: not kinds or isinstance(prop, kinds) or isinstance(target, kinds),
            lambda _: Fault.of(Reason.OUTSIDE, rna(self.descriptor.path), type(target).__name__, member=", ".join(kind.__name__ for kind in kinds)),
        )

    def current(self, _kinds: _Kinds, _context: bpy.types.Context, /) -> Outcome[Option[Self]]:
        """Member resolved once at realization, the same at every draw."""
        return Ok(Some(self))


class Member(msgspec.Struct, frozen=True, kw_only=True):
    """Property path rooted at its owner type, or at the form group an attachment holds on its owner, reached through context when the owner is no drawing holder, labelled as mixed while the owners `scope` selects differ.

    Attachment root locates under the extension's pointer once rows realize, each row declaring the attachment it reads.
    """

    root: type[Struct] | Attachment[Any]
    path: RnaPath
    reach: Option[Reach] = Nothing
    scope: Option[Scoped] = Nothing

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Attachment the root names."""
        return (self.root,) if isinstance(self.root, Attachment) else ()

    def located(self, identity: Identity, /) -> Outcome[Self]:
        """Roots an attachment's member at the owner type under the pointer the extension registered, any other member as declared."""
        match self.root:
            case Attachment() as attachment:
                return attachment.bound(identity).map(lambda attached: msgspec.structs.replace(self, root=attached.owner, path=(attached.name, *self.path)))
            case _:
                return Ok(self)

    @overload
    def resolved(self, *, root: Option[Struct] = Nothing, kinds: _Kinds = ()) -> Outcome[ResolvedMember]: ...

    @overload
    def resolved[P: bpy.types.Property](self, kind: type[P], *, root: Option[Struct] = Nothing, kinds: _Kinds = ()) -> Outcome[ResolvedMember[P]]: ...

    def resolved(self, kind: type[bpy.types.Property] = bpy.types.Property, *, root: Option[Struct] = Nothing, kinds: _Kinds = ()) -> Outcome[ResolvedMember]:
        """Resolves metadata within `kinds` on the root type, or on a live `root` of it whose refined definition answers, absent fault for an attachment root no row located."""
        match self.root:
            case Attachment(key=key):
                return Error(Fault.of(Reason.ABSENT, rna(self.path), member=f"located {key} attachment"))
            case owner:
                held: Outcome[type[Struct] | Struct] = root.map(
                    lambda actual: Ok(actual).filter_with(lambda found: isinstance(found, owner), lambda found: Fault.of(Reason.OUTSIDE, rna(self.path), type(found).__name__, member=owner.__name__))
                ).default_value(Ok(owner))
                return held.bind(lambda found: Descriptor.resolve(found, self.path, kind)).bind(lambda descriptor: ResolvedMember(descriptor=descriptor, reach=self.reach).accepts(kinds))

    @overload
    def realized(self, identity: Identity, /, *, kinds: _Kinds = ()) -> Outcome[ResolvedMember]: ...

    @overload
    def realized[P: bpy.types.Property](self, identity: Identity, kind: type[P], /, *, kinds: _Kinds = ()) -> Outcome[ResolvedMember[P]]: ...

    def realized(self, identity: Identity, kind: type[bpy.types.Property] = bpy.types.Property, /, *, kinds: _Kinds = ()) -> Outcome[ResolvedMember]:
        """Locates the member under the extension's pointers, then resolves its metadata on the root type within `kind` and `kinds`."""
        return self.located(identity).bind(lambda found: found.resolved(kind, kinds=kinds))

    def bound(self, kinds: _Kinds = (), /) -> Outcome[ResolvedMember | Self]:
        """Resolves a member with no reach once within the kinds, a reached member staying to resolve on its live root each draw."""
        match self.reach:
            case Option(tag="some"):
                return Ok(self)
            case _:
                return self.resolved(kinds=kinds)

    def current(self, kinds: _Kinds, context: bpy.types.Context, /) -> Outcome[Option[ResolvedMember]]:
        """Resolves on the root the reach finds this draw within the kinds, `Nothing` while it finds none."""
        return self.reach.bind(lambda reach: reach(context)).map(lambda root: self.resolved(root=Some(root), kinds=kinds).map(Some)).default_value(Ok(Nothing))

    def labelled(self, setting: Descriptor, context: bpy.types.Context) -> Outcome[Mapping[str, str | bool]]:
        """Mixed label keywords of the setting across the scope's owners, none without a scope or while the owners agree, the host labelling the property itself."""
        return (
            self.scope
            .map(lambda scope: scope.owners(context).bind(partial(marked, setting)))
            .default_value(Ok(Nothing))
            .map(lambda mixed: {"text": mixed.default_value(""), "translate": False} if mixed.is_some() else {})
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LABELS]


def marked[P: bpy.types.Property](setting: Descriptor[P], owners: tuple[Struct, ...], /) -> Outcome[Option[str]]:
    """Translated name of a setting marked as mixed while owner values differ, nothing while they agree, the host labelling it."""
    prop = setting.prop
    return accumulated(Snapshot.capture(owner, Some((setting,))) for owner in owners).map(
        lambda values: Some(f"{bpy.app.translations.pgettext_iface(prop.name, prop.translation_context)} —").filter(lambda _: any(left != right for left, right in pairwise(values)))
    )


# --- [BINDING]


def bound(
    members: tuple[Member, ...], place: Callable[[LocatedMembers, bpy.types.UILayout, Struct, bpy.types.Context], None], /, kinds: _Kinds = ()
) -> Outcome[Callable[[bpy.types.UILayout, Struct, bpy.types.Context], None]]:
    """Resolves fixed members once and reached members each draw, each within the kinds, drawing over the located owners while every one is found."""
    return accumulated(member.bound(kinds) for member in members).map(lambda held: partial(_bound, place, kinds, held))


def _bound(
    place: Callable[[LocatedMembers, bpy.types.UILayout, Struct, bpy.types.Context], None],
    kinds: _Kinds,
    held: tuple[ResolvedMember | Member, ...],
    layout: bpy.types.UILayout,
    holder: Struct,
    context: bpy.types.Context,
) -> None:
    """Draws over the members and owners this redraw locates, alerting refusals."""
    found = accumulated(each.current(kinds, context) for each in held)
    alerted(layout, found)
    for members in found.to_option().to_list():
        located = tuple((member, at) for each in members for member in each.to_list() for at in member.at(holder, context).to_list())
        if len(located) == len(held):
            place(located, layout, holder, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["At", "LocatedMembers", "Member", "Reach", "ResolvedMember", "bound", "marked"]
