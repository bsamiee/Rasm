"""Keyed owner trees with their diffs, merges, transfers, and reverts, layered layouts of linked boxes, and walks reaching each node once."""

from collections.abc import Callable, Collection, Hashable, Iterable, Mapping
from difflib import SequenceMatcher
from enum import Enum
from functools import cached_property, reduce
from graphlib import TopologicalSorter
from itertools import accumulate
from operator import sub
from types import MappingProxyType
from typing import Self

from expression import Nothing, Option, Some
from expression.collections import Block
import msgspec

# --- [TYPES] ----------------------------------------------------------------------------

type Edit[V, K: Hashable = str, N: Hashable = str] = Inserted[V, K, N] | Deleted[V, K, N] | Moved[K] | Reparametrized[V, K, N]


class Toward(Enum):
    """One step through an ordered sequence, valued by the index change it makes."""

    UP = -1
    DOWN = 1

    def stepped(self, index: int, count: int) -> int:
        """Returns the index one step from `index`, held inside a sequence of `count` items."""
        return min(max(index + self.value, 0), count - 1)


# --- [MODELS] ---------------------------------------------------------------------------
# --- [TREES]


class Owner[V, K: Hashable = str, N: Hashable = str](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Owner values as name and value rows, any hashable name a JSON array encodes, and ordered child keys."""

    rows: tuple[tuple[N, V], ...] = ()
    children: tuple[K, ...] = ()

    @cached_property
    def values(self) -> Mapping[N, V]:
        """Values by name."""
        return MappingProxyType(dict(self.rows))


class Place[K: Hashable = str](msgspec.Struct, frozen=True, kw_only=True):
    """Sibling index under parent or among roots."""

    parent: Option[K]
    index: int


class Tree[V, K: Hashable = str, N: Hashable = str](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Owners by stable key, ordered under parents."""

    roots: tuple[K, ...]
    rows: tuple[tuple[K, Owner[V, K, N]], ...]

    @cached_property
    def owners(self) -> Mapping[K, Owner[V, K, N]]:
        """Owners by key."""
        return MappingProxyType(dict(self.rows))

    @classmethod
    def joined(cls, trees: Collection[Self]) -> Self:
        """Concatenates trees in order."""
        return cls(roots=tuple(key for tree in trees for key in tree.roots), rows=tuple({key: owner for tree in trees for key, owner in tree.rows}.items()))

    def merged(self, other: Self) -> Self:
        """Returns owners of both trees, an owner both hold taking other's children and other's values over this tree's.

        Args:
            other: Tree whose owners and values win.

        Returns:
            Tree with this tree's roots, then other's roots it lacks.
        """
        return type(self)(
            roots=tuple(dict.fromkeys((*self.roots, *other.roots))),
            rows=tuple(
                {
                    **self.owners,
                    **{key: msgspec.structs.replace(owner, rows=tuple({**self.owners[key].values, **owner.values}.items())) if key in self.owners else owner for key, owner in other.rows},
                }.items()
            ),
        )

    def children(self, parent: Option[K]) -> tuple[K, ...]:
        """Returns ordered child keys under parent, roots for Nothing."""
        return parent.map(lambda key: self.owners[key].children).default_value(self.roots)

    def places(self) -> dict[K, Place[K]]:
        """Returns place per owner."""
        return {key: Place(parent=parent, index=index) for parent in (Nothing, *map(Some, self.owners)) for index, key in enumerate(self.children(parent))}


class _Placed[V, K: Hashable = str, N: Hashable = str](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Owner with value rows at place."""

    key: K
    place: Place[K]
    rows: tuple[tuple[N, V], ...] = ()

    @cached_property
    def values(self) -> Mapping[N, V]:
        """Values by name."""
        return MappingProxyType(dict(self.rows))


class Inserted[V, K: Hashable = str, N: Hashable = str](_Placed[V, K, N], frozen=True, kw_only=True, dict=True):
    """Owner entering tree at place."""


class Deleted[V, K: Hashable = str, N: Hashable = str](_Placed[V, K, N], frozen=True, kw_only=True, dict=True):
    """Owner leaving tree from place."""


class Moved[K: Hashable = str](msgspec.Struct, frozen=True, kw_only=True):
    """Owner changing parent or sibling position."""

    key: K
    before: Place[K]
    after: Place[K]


class Reparametrized[V, K: Hashable = str, N: Hashable = str](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Owner values changed or removed as prior rows, and changed or added as later rows."""

    key: K
    prior: tuple[tuple[N, V], ...] = ()
    later: tuple[tuple[N, V], ...] = ()

    @cached_property
    def before(self) -> Mapping[N, V]:
        """Prior values by name."""
        return MappingProxyType(dict(self.prior))

    @cached_property
    def after(self) -> Mapping[N, V]:
        """Later values by name."""
        return MappingProxyType(dict(self.later))

    @classmethod
    def of(cls, key: K, before: Mapping[N, V], after: Mapping[N, V]) -> Option[Self]:
        """Returns changed, removed, and added values."""
        old = tuple((name, value) for name, value in before.items() if name not in after or after[name] != value)
        new = tuple((name, value) for name, value in after.items() if name not in before or before[name] != value)
        return Some(cls(key=key, prior=old, later=new)) if old or new else Nothing

    @classmethod
    def shared(cls, key: K, before: Mapping[N, V], after: Mapping[N, V]) -> Option[Self]:
        """Returns changed values of names both hold."""
        return cls.of(key, {name: value for name, value in before.items() if name in after}, {name: value for name, value in after.items() if name in before})

    def applied(self, values: Mapping[N, V]) -> Mapping[N, V]:
        """Returns values with the removed names dropped and the changed and added values set."""
        return {**{name: value for name, value in values.items() if name not in self.before}, **self.after}


# --- [LAYOUTS]


class Gaps(msgspec.Struct, frozen=True, kw_only=True):
    """Layout gaps between columns, between rows, and between bands."""

    across: float = 80.0
    down: float = 20.0
    band: float = 60.0

    def arranged[K: Hashable](
        self, sizes: Mapping[K, tuple[float, float]], links: tuple[tuple[K, int, K], ...], bands: Mapping[K, Option[K]], origin: Callable[[K], tuple[float, float]]
    ) -> dict[K, tuple[float, float]]:
        """Returns the top left corner per box of `(width, height)` size, columns by depth over acyclic (source, slot, target) links, rows by source slot order, one band's boxes stacked together, growing right and down from the first box's origin."""
        if not sizes:
            return {}
        feeds = {key: tuple((source, slot) for source, slot, target in links if target == key) for key in sizes}
        order = tuple(TopologicalSorter({key: tuple(source for source, _ in fed) for key, fed in feeds.items()}).static_order())
        position = {key: index for index, key in enumerate(order)}
        column: dict[K, int] = reduce(lambda held, key: held | {key: max((held[source] + 1 for source, _ in feeds[key]), default=0)}, order, {})
        rank = {key: min(((position[source], slot) for source, slot in feeds[key]), default=(position[key], -1)) for key in order}
        banded, depths = tuple(dict.fromkeys(bands[key] for key in order)), range(max(column.values()) + 1)
        cells = {(band, depth): sorted((key for key in order if (bands[key], column[key]) == (band, depth)), key=rank.__getitem__) for band in banded for depth in depths}
        left, top = origin(order[0])
        lefts = tuple(accumulate((max(sizes[key][0] for key in order if column[key] == depth) + self.across for depth in depths), initial=left))
        stacks = {cell: sum(sizes[key][1] for key in keys) + self.down * (len(keys) - 1) if keys else 0.0 for cell, keys in cells.items()}
        tops = dict(zip(banded, accumulate((max(stacks[band, depth] for depth in depths) + self.band for band in banded), sub, initial=top)))
        return {key: (lefts[depth], y) for (band, depth), keys in cells.items() for key, y in zip(keys, accumulate((sizes[key][1] + self.down for key in keys), sub, initial=tops[band]))}


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [TREES]


def diff[V, K: Hashable, N: Hashable](before: Tree[V, K, N], after: Tree[V, K, N]) -> tuple[Edit[V, K, N], ...]:
    """Returns deletions, value changes, and target-ordered moves and insertions."""
    was, now = before.places(), after.places()
    stay = frozenset(key for key, place in now.items() if key in was and was[key].parent == place.parent)
    orders = tuple((tuple(key for key in before.children(parent) if key in stay), tuple(key for key in after.children(parent) if key in stay)) for parent in {now[key].parent for key in stay})
    kept = frozenset(key for old, new in orders for block in SequenceMatcher(None, old, new, autojunk=False).get_matching_blocks() for key in new[block.b : block.b + block.size])
    return (
        *(Deleted(key=key, place=was[key], rows=owner.rows) for key, owner in before.owners.items() if key not in after.owners),
        *(edit for key, owner in after.owners.items() if key in before.owners for edit in Reparametrized[V, K, N].of(key, before.owners[key].values, owner.values).to_list()),
        *(Moved(key=key, before=was[key], after=place) if key in was else Inserted(key=key, place=place, rows=after.owners[key].rows) for key, place in now.items() if key not in kept),
    )


def merge[V, K: Hashable, N: Hashable](state: Tree[V, K, N], edits: Iterable[Edit[V, K, N]]) -> Tree[V, K, N]:
    """Applies edits drawn against state, value changes of one owner in edit order and entering owners by target index."""
    held = tuple(edits)
    gone = frozenset(edit.key for edit in held if isinstance(edit, Deleted))
    lifted = gone | frozenset(edit.key for edit in held if isinstance(edit, Moved))
    changes = tuple(edit for edit in held if isinstance(edit, Reparametrized))
    revisions = {key: tuple(edit for edit in changes if edit.key == key) for key in dict.fromkeys(edit.key for edit in changes)}
    base = {**{key: owner.values for key, owner in state.owners.items() if key not in gone}, **{edit.key: edit.values for edit in held if isinstance(edit, Inserted)}}
    values = {key: reduce(lambda current, edit: edit.applied(current), revisions.get(key, ()), current) for key, current in base.items()}
    entering = sorted(((edit.place if isinstance(edit, Inserted) else edit.after, edit.key) for edit in held if isinstance(edit, Inserted | Moved)), key=lambda row: row[0].index)
    start = {parent: tuple(key for key in state.children(parent) if key not in lifted) for parent in (Nothing, *map(Some, state.owners))}
    order = {
        parent: reduce(lambda siblings, row: (*siblings[: row[0].index], row[1], *siblings[row[0].index :]), (row for row in entering if row[0].parent == parent), start.get(parent, ()))
        for parent in dict.fromkeys((*start, *(place.parent for place, _ in entering)))
    }
    return Tree(roots=order.get(Nothing, ()), rows=tuple((key, Owner(rows=tuple(owned.items()), children=order.get(Some(key), ()))) for key, owned in values.items()))


def transfer[V, K: Hashable, N: Hashable](target: Tree[V, K, N], source: Tree[V, K, N]) -> tuple[Reparametrized[V, K, N], ...]:
    """Returns source values for owners and names both share."""
    return tuple(edit for key, owner in source.owners.items() if key in target.owners for edit in Reparametrized[V, K, N].shared(key, target.owners[key].values, owner.values).to_list())


def revert[V, K: Hashable, N: Hashable](current: Tree[V, K, N], applied: Iterable[Reparametrized[V, K, N]]) -> tuple[Reparametrized[V, K, N], ...]:
    """Returns prior values where current matches applied."""
    return tuple(
        restored
        for edit in applied
        for held in Option.of_optional(current.owners.get(edit.key)).to_list()
        for restored in Reparametrized[V, K, N]
        .shared(edit.key, held.values, {name: value for name, value in edit.before.items() if name in edit.after and held.values.get(name) == edit.after[name]})
        .to_list()
    )


# --- [WALKS]


def reached[T: Hashable](seeds: Iterable[T], children: Callable[[T], Iterable[T]], /) -> Block[T]:
    """Returns seeds and every node children reach from them, depth first in child order, each node once at its first reach."""

    def step(state: tuple[tuple[T, ...], frozenset[T]]) -> Option[tuple[T, tuple[tuple[T, ...], frozenset[T]]]]:
        pending, seen = state
        match tuple(node for node in pending if node not in seen):
            case (node, *rest):
                return Some((node, ((*children(node), *rest), seen | {node})))
            case _:
                return Nothing

    start: tuple[tuple[T, ...], frozenset[T]] = (tuple(seeds), frozenset())
    return Block.unfold(step, start)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Deleted", "Edit", "Gaps", "Inserted", "Moved", "Owner", "Place", "Reparametrized", "Toward", "Tree", "diff", "merge", "reached", "revert", "transfer"]
