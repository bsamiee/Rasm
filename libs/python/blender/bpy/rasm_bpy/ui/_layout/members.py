"""Member bindings rooted at an owner type or an attachment, resolved on the root type or a live root and located at the drawing holder or context."""

from collections.abc import Callable, Mapping
from functools import partial
from itertools import pairwise
from typing import Generic, overload, TypeVar

import bpy
from expression import Option, Some
import msgspec

from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Declaration
from rasm_bpy.data._rna.conditions import Scoped
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.data._rna.paths import Native, RnaPath, Struct
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_bpy.settings._attachments import Attachment
from rasm_records import accumulated, Nothing, Ok, Outcome

# --- [TYPES] ----------------------------------------------------------------------------

type At = tuple[Native, str]
type Reach = Callable[[bpy.types.Context], Option[Struct]]
type LocatedMembers = tuple[tuple[Descriptor, At], ...]
_R_co = TypeVar("_R_co", bound=type[Struct] | Attachment[bpy.types.PropertyGroup], covariant=True, default=type[Struct] | Attachment[bpy.types.PropertyGroup])

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


class Member(msgspec.Struct, Generic[_R_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Property path rooted at its owner type, or at the form group an attachment holds on its owner, reached through context when the owner is no drawing holder, labelled as mixed while the owners `scope` selects differ.

    The root parameter separates declarations from native roots: `located` roots attachments under the registered pointer; `resolved` and `bound` take native roots.
    """

    root: _R_co
    path: RnaPath
    reach: Option[Reach] = Nothing
    scope: Option[Scoped] = Nothing

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Attachment the root names."""
        return (self.root,) if isinstance(self.root, Attachment) else ()

    @overload
    def located[M: Member[type[Struct]]](self: M, identity: Identity, /) -> Outcome[M]: ...

    @overload
    def located(self, identity: Identity, /) -> "Outcome[Member[type[Struct]]]": ...

    def located(self, identity: Identity, /) -> "Outcome[Member]":
        """Roots an attachment member under its registered pointer, retaining its type and fields; native members return unchanged."""
        match self.root:
            case Attachment() as attachment:
                return attachment.bound(identity).map(lambda attached: msgspec.structs.replace(self, root=attached.owner, path=(attached.name, *self.path)))
            case _:
                return Ok(self)

    @overload
    def resolved(self: "Member[type[Struct]]", *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember]: ...

    @overload
    def resolved[P: bpy.types.Property](self: "Member[type[Struct]]", kind: type[P], *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember[P]]: ...

    def resolved(self: "Member[type[Struct]]", kind: type[bpy.types.Property] = bpy.types.Property, *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember]:
        """Resolves metadata on the native root type or live `root`."""
        return Descriptor.resolve(root.default_value(self.root), self.path, kind).map(lambda descriptor: ResolvedMember(descriptor=descriptor, reach=self.reach))

    @overload
    def realized(self, identity: Identity, /) -> Outcome[ResolvedMember]: ...

    @overload
    def realized[P: bpy.types.Property](self, identity: Identity, kind: type[P], /) -> Outcome[ResolvedMember[P]]: ...

    def realized(self, identity: Identity, kind: type[bpy.types.Property] = bpy.types.Property, /) -> Outcome[ResolvedMember]:
        """Locates the member under the extension's pointers, then resolves its metadata on the root type within `kind`."""
        return self.located(identity).bind(lambda found: found.resolved(kind))

    def bound[M: Member[type[Struct]]](self: M) -> Outcome[ResolvedMember | M]:  # ruff: ignore[custom-type-var-for-self]
        """Resolves a member with no reach once, a reached member staying to resolve on its live root each draw."""
        match self.reach:
            case Option(tag="some"):
                return Ok(self)
            case _:
                return self.resolved()

    def labelled(self, setting: Descriptor, context: bpy.types.Context) -> Outcome[Mapping[str, str | bool]]:
        """Mixed label keywords of the setting across the scope's owners, none without a scope or while the owners agree, the host labelling the property itself."""
        return (
            self.scope
            .map(lambda scope: scope.owners(context).bind(partial(_marked, setting)))
            .default_value(Ok(Nothing))
            .map(lambda mixed: {"text": mixed.default_value(""), "translate": False} if mixed.is_some() else dict[str, str | bool]())
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [IDENTITY]


def _owner_key(struct: Native) -> str:
    """Identifier per native owner: ID `session_uid`, pointer identity for other structs."""
    if isinstance(struct, bpy.types.bpy_prop_collection):
        return _owner_key(struct.data)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    return str(struct.session_uid if isinstance(struct, bpy.types.ID) else struct.as_pointer())


# --- [LABELS]


def _marked(setting: Descriptor, owners: tuple[Struct, ...], /) -> Outcome[Option[str]]:
    """Translated name of a setting marked as mixed while owner values differ, nothing while they agree, the host labelling it."""
    prop = setting.prop
    return accumulated(Snapshot.capture(owner, Some((setting,))) for owner in owners).map(
        lambda values: Some(f"{bpy.app.translations.pgettext_iface(prop.name, prop.translation_context)} —").filter(lambda _: any(left != right for left, right in pairwise(values)))
    )


# --- [BINDING]


def _resolved_at(member: Member[type[Struct]] | ResolvedMember, holder: Struct, context: bpy.types.Context, /) -> Outcome[Option[tuple[Descriptor, At]]]:
    """Returns metadata and owner from one reach, nothing for an absent root or owner, or faults from resolving metadata."""
    match member:
        case Member(reach=Option(tag="some", some=reach)):
            return (
                reach(context)
                .map(lambda root: member.resolved(root=Some(root)).map(lambda found: found.descriptor.owner_of(root).map(lambda owner: (found.descriptor, (owner, found.descriptor.prop.identifier)))))
                .default_value(Ok(Nothing))
            )
        case Member():
            return member.resolved().bind(lambda found: _resolved_at(found, holder, context))
        case ResolvedMember():
            return Ok(member.at(holder, context).map(lambda at: (member.descriptor, at)))


def _bound(
    identity: Identity, members: tuple[Member[type[Struct]], ...], place: Callable[[LocatedMembers, bpy.types.UILayout, Struct, bpy.types.Context], None], /
) -> Outcome[Callable[[bpy.types.UILayout, Struct, bpy.types.Context], None]]:
    """Resolves fixed members once and reached members each draw, drawing over the located owners while every one is found."""

    def drawn(held: tuple[ResolvedMember | Member[type[Struct]], ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws over present owners, logging every refusal."""
        for current in identity.reports.settled(accumulated(_resolved_at(each, holder, context) for each in held)).to_list():
            located = tuple(member for each in current for member in each.to_list())
            if len(located) == len(held):
                place(located, layout, holder, context)

    return accumulated(member.bound() for member in members).map(lambda held: partial(drawn, held))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["At", "LocatedMembers", "Member", "Reach", "ResolvedMember"]
