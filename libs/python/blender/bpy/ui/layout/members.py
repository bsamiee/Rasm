"""Member bindings rooted at an owner type, located at the drawing holder, a context reach, or the context root, read across a scope's owners, and the conditions gating them."""

from collections.abc import Callable, Iterable, Mapping
from functools import partial
from itertools import pairwise, starmap
from typing import Generic, overload, Self

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor, P_co
from blender.bpy.data.rna.paths import Native, rna, RnaPath, root_in, Struct
from blender.bpy.data.rna.scopes import Scoped
from blender.bpy.data.rna.snapshots import Snapshot
from blender.records.faults import accumulated, Fault, Outcome, RecordReason

# --- [TYPES] ----------------------------------------------------------------------------

type At = tuple[Native, str]
type Reach = Callable[[bpy.types.Context], Option[Struct]]
type Read = Callable[[bpy.types.Context], Outcome[Option[tuple[ResolvedMember, ...]]]]
type _Located = Callable[[bpy.types.Context], Outcome[Option[ResolvedMember]]]

# --- [MODELS] ---------------------------------------------------------------------------


class ResolvedMember(msgspec.Struct, Generic[P_co], frozen=True, kw_only=True):
    """RNA definition with the reach of its owner's root, the drawing holder or the context root while no reach is stated."""

    descriptor: Descriptor[P_co]
    reach: Option[Reach] = Nothing

    def at(self, holder: Struct, context: bpy.types.Context) -> Option[At]:
        """Locates the property owner through the reach, the drawing holder, or the context root."""
        descriptor = self.descriptor
        root = self.reach.map(lambda reach: reach(context)).default_with(lambda: Some(holder) if isinstance(holder, descriptor.root) else root_in(descriptor.root, context))
        return root.bind(descriptor.owner_of).map(lambda owner: (owner, descriptor.prop.identifier))

    def accepts(self, kinds: tuple[type[Struct], ...]) -> Outcome[Self]:
        """Accepts a property or pointer target of the kinds, every kind while none is given."""
        prop = self.descriptor.prop
        target = prop.fixed_type if isinstance(prop, bpy.types.PointerProperty) else prop
        return Ok(self).filter_with(
            lambda _: not kinds or isinstance(prop, kinds) or isinstance(target, kinds),
            lambda _: Fault.of(RecordReason.OUTSIDE, rna(self.descriptor.path), type(target).__name__, member=", ".join(kind.__name__ for kind in kinds)),
        )


class Member(msgspec.Struct, frozen=True, kw_only=True):
    """Property path rooted at its owner type, reached through context when the owner is no drawing holder, labelled as mixed while the owners `scope` selects differ."""

    root: type[Struct]
    path: RnaPath
    reach: Option[Reach] = Nothing
    scope: Option[Scoped] = Nothing

    @overload
    def resolved(self, *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember]: ...

    @overload
    def resolved[P: bpy.types.Property](self, kind: type[P], *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember[P]]: ...

    def resolved(self, kind: type[bpy.types.Property] = bpy.types.Property, *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember]:
        """Resolves metadata on the root type, or on a live `root` of it that the member then stays at."""
        return root.map(
            lambda actual: (
                Ok(actual)
                .filter_with(lambda held: isinstance(held, self.root), lambda held: Fault.of(RecordReason.OUTSIDE, rna(self.path), type(held).__name__, member=self.root.__name__))
                .bind(lambda held: Descriptor.resolve(held, self.path, kind))
                .map(lambda descriptor: ResolvedMember(descriptor=descriptor, reach=Some(partial(_fixed_root, actual))))
            )
        ).default_with(lambda: Descriptor.resolve(self.root, self.path, kind).map(lambda descriptor: ResolvedMember(descriptor=descriptor, reach=self.reach)))

    def labelled(self, setting: Descriptor, context: bpy.types.Context) -> Outcome[Mapping[str, str | bool]]:
        """Label keywords of the setting across the scope's owners, none without a scope so the host labels the property itself."""
        return self.scope.map(lambda scope: scope.owners(context).bind(partial(marked, setting))).default_value(Ok({}))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CONDITIONS]


def admits(condition: Option[Condition], context: bpy.types.Context, item: Option[Struct] = Nothing, /) -> bool:
    """Whether a stated condition holds over the context and item, every context passing while none is stated."""
    return condition.map(lambda required: required.holds(context, item)).default_value(True)


# --- [LABELS]


def marked(setting: Descriptor, owners: tuple[Struct, ...], /) -> Outcome[Mapping[str, str | bool]]:
    """Label keywords of a setting, its name translated and marked as mixed while owner values differ."""
    prop = setting.prop
    return accumulated(Snapshot.capture(owner, Some((setting,))) for owner in owners).map(
        lambda values: (
            {"text": f"{bpy.app.translations.pgettext_iface(prop.name, prop.translation_context)} —", "translate": False}
            if any(left != right for left, right in pairwise(values))
            else {"text": prop.name, "text_ctxt": prop.translation_context}
        )
    )


# --- [READS]


def reader(bindings: Iterable[tuple[Member, tuple[type[Struct], ...]]], /) -> Outcome[Read]:
    """Resolves fixed members once and reached members on each read, each within its accepted kinds, the read holding `Nothing` while a reached owner is absent."""

    def located(member: Member, kinds: tuple[type[Struct], ...]) -> Outcome[_Located]:
        match member.reach:
            case Option(tag="some", some=reach):
                return Ok(partial(_live, member, kinds, reach))
            case _:
                return member.resolved().bind(lambda held: held.accepts(kinds)).map(lambda held: partial(_fixed, held))

    return accumulated(starmap(located, bindings)).map(lambda found: partial(_read, found))


def _fixed_root(root: Struct, _context: bpy.types.Context) -> Option[Struct]:
    """Live root a member resolved on."""
    return Some(root)


def _fixed(held: ResolvedMember, _context: bpy.types.Context) -> Outcome[Option[ResolvedMember]]:
    """Member resolved once on its root type."""
    return Ok(Some(held))


def _live(member: Member, kinds: tuple[type[Struct], ...], reach: Reach, context: bpy.types.Context) -> Outcome[Option[ResolvedMember]]:
    """Member resolved on the root the reach finds this draw."""
    return reach(context).map(lambda root: member.resolved(root=Some(root)).bind(lambda held: held.accepts(kinds)).map(Some)).default_value(Ok(Nothing))


def _read(located: tuple[_Located, ...], context: bpy.types.Context) -> Outcome[Option[tuple[ResolvedMember, ...]]]:
    """Members this draw locates, `Nothing` while one is absent."""
    return accumulated(read(context) for read in located).map(lambda found: Some(tuple(value for each in found for value in each.to_list())).filter(lambda present: len(present) == len(found)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["At", "Member", "Reach", "Read", "ResolvedMember", "admits", "marked", "reader"]
