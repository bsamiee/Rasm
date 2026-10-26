"""Settings tree diffs and merges."""

from collections import defaultdict
from collections.abc import Iterable
from difflib import SequenceMatcher
from typing import Self

from expression import Nothing, Option, Some
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Edit[V] = Inserted[V] | Deleted[V] | Moved | Reparametrized[V]


# --- [MODELS] ---------------------------------------------------------------------------


class Owner[V](msgspec.Struct, frozen=True, kw_only=True):
    """Owner settings values and child keys."""

    values: dict[str, V]
    children: tuple[str, ...] = ()


class Place(msgspec.Struct, frozen=True, kw_only=True):
    """Sibling index under parent or among roots."""

    parent: Option[str]
    index: int


class Tree[V](msgspec.Struct, frozen=True, kw_only=True):
    """Settings owners by stable key, ordered under parents."""

    roots: tuple[str, ...]
    owners: dict[str, Owner[V]]

    @classmethod
    def joined(cls, trees: Iterable["Tree[V]"]) -> Self:
        """Concatenates trees in order."""
        held = tuple(trees)
        return cls(roots=tuple(key for tree in held for key in tree.roots), owners={key: value for tree in held for key, value in tree.owners.items()})

    def merged(self, other: "Tree[V]") -> Self:
        """Returns owners of both trees, an owner both hold taking other's children and other's values over this tree's.

        Args:
            other: Tree whose owners and values win.

        Returns:
            Tree with this tree's roots, then other's roots it lacks.
        """
        return type(self)(
            roots=tuple(dict.fromkeys((*self.roots, *other.roots))),
            owners=self.owners | {key: msgspec.structs.replace(owner, values=self.owners[key].values | owner.values) if key in self.owners else owner for key, owner in other.owners.items()},
        )

    def children(self, parent: Option[str]) -> tuple[str, ...]:
        """Returns ordered child keys under parent, roots for Nothing."""
        return parent.map(lambda key: self.owners[key].children).default_value(self.roots)

    def places(self) -> dict[str, Place]:
        """Returns place per owner."""
        return {key: Place(parent=parent, index=index) for parent in (Nothing, *map(Some, self.owners)) for index, key in enumerate(self.children(parent))}


class Inserted[V](msgspec.Struct, frozen=True, kw_only=True):
    """Owner entering tree at place."""

    key: str
    place: Place
    values: dict[str, V]


class Deleted[V](msgspec.Struct, frozen=True, kw_only=True):
    """Owner leaving tree from place."""

    key: str
    place: Place
    values: dict[str, V]


class Moved(msgspec.Struct, frozen=True, kw_only=True):
    """Owner changing parent or sibling position."""

    key: str
    before: Place
    after: Place


class Reparametrized[V](msgspec.Struct, frozen=True, kw_only=True):
    """Owner values changed, removed, or added."""

    key: str
    before: dict[str, V]
    after: dict[str, V]

    @classmethod
    def of(cls, key: str, before: dict[str, V], after: dict[str, V], *, shared: bool = False) -> Option[Self]:
        """Returns changed values, shared names alone when shared."""
        names = before.keys() & after.keys() if shared else before.keys() | after.keys()
        old = {name: value for name, value in before.items() if name in names and (name not in after or after[name] != value)}
        new = {name: value for name, value in after.items() if name in names and (name not in before or before[name] != value)}
        return Some(cls(key=key, before=old, after=new)) if old or new else Nothing


# --- [OPERATIONS] -----------------------------------------------------------------------


def diff[V](before: Tree[V], after: Tree[V]) -> tuple[Edit[V], ...]:
    """Returns deletions, value changes, and target-ordered moves and insertions."""
    was, now = before.places(), after.places()
    stay = frozenset(key for key, place in now.items() if key in was and was[key].parent == place.parent)
    orders = tuple((tuple(key for key in before.children(parent) if key in stay), tuple(key for key in after.children(parent) if key in stay)) for parent in {now[key].parent for key in stay})
    kept = frozenset(key for old, new in orders for block in SequenceMatcher(None, old, new, autojunk=False).get_matching_blocks() for key in new[block.b : block.b + block.size])
    return (
        *(Deleted(key=key, place=was[key], values=owner.values) for key, owner in before.owners.items() if key not in after.owners),
        *(edit for key, owner in after.owners.items() if key in before.owners for edit in Reparametrized.of(key, before.owners[key].values, owner.values).to_list()),
        *(Moved(key=key, before=was[key], after=place) if key in was else Inserted(key=key, place=place, values=after.owners[key].values) for key, place in now.items() if key not in kept),
    )


def merge[V](state: Tree[V], edits: Iterable[Edit[V]]) -> Tree[V]:
    """Applies edits drawn against state."""
    values = {key: owner.values for key, owner in state.owners.items()}
    lifted: set[str] = set()
    entering: list[tuple[Place, str]] = []
    for edit in edits:
        match edit:
            case Inserted(key=key, place=place, values=inserted):
                values[key] = inserted
                entering.append((place, key))
            case Deleted(key=key):
                del values[key]
                lifted.add(key)
            case Moved(key=key, after=place):
                lifted.add(key)
                entering.append((place, key))
            case Reparametrized(key=key, before=old, after=new):
                values[key] = {name: value for name, value in values[key].items() if name not in old} | new
    order = defaultdict[Option[str], list[str]](list, {parent: [key for key in state.children(parent) if key not in lifted] for parent in (Nothing, *map(Some, state.owners))})
    for place, key in sorted(entering, key=lambda row: row[0].index):
        order[place.parent].insert(place.index, key)
    return Tree(roots=tuple(order[Nothing]), owners={key: Owner(values=owned, children=tuple(order[Some(key)])) for key, owned in values.items()})


def transfer[V](target: Tree[V], source: Tree[V]) -> tuple[Reparametrized[V], ...]:
    """Returns source values for owners and names both share."""
    return tuple(edit for key, owner in source.owners.items() if key in target.owners for edit in Reparametrized.of(key, target.owners[key].values, owner.values, shared=True).to_list())


def revert[V](current: Tree[V], applied: Iterable[Reparametrized[V]]) -> tuple[Reparametrized[V], ...]:
    """Returns prior values where current matches applied."""
    return tuple(
        restored
        for edit in applied
        if edit.key in current.owners
        for values in (current.owners[edit.key].values,)
        for restored in Reparametrized.of(
            edit.key, values, {name: value for name, value in edit.before.items() if name in edit.after and name in values and values[name] == edit.after[name]}, shared=True
        ).to_list()
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Deleted", "Edit", "Inserted", "Moved", "Owner", "Place", "Reparametrized", "Tree", "diff", "merge", "revert", "transfer"]
