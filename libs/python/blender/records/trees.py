"""Keyed owner trees with their edits, layered layouts of linked boxes, and walks reaching each node once."""

from collections.abc import Callable, Collection, Hashable, Iterable, Mapping
from difflib import SequenceMatcher
from enum import Enum
from graphlib import TopologicalSorter
from itertools import accumulate, chain
from operator import eq, sub
from types import MappingProxyType
from typing import Self

from expression import Option, Some
from expression.collections import Block
import msgspec

from blender.records.faults import Nothing

# --- [TYPES] ----------------------------------------------------------------------------

type Edit[V, K: Hashable, N: Hashable] = Inserted[V, K, N] | Deleted[V, K, N] | Moved[K] | Reparametrized[V, K, N]


class Toward(Enum):
    """One step through an ordered sequence, valued by the index change it makes."""

    UP = -1
    DOWN = 1

    def stepped(self, index: int, count: int) -> int:
        """Returns the index one step from `index`, held inside a sequence of `count` items."""
        return min(max(index + self.value, 0), count - 1)


# --- [MODELS] ---------------------------------------------------------------------------
# --- [TREES]


class Owner[V, K: Hashable, N: Hashable](msgspec.Struct, frozen=True, kw_only=True):
    """Owner value rows, any hashable name a JSON array encodes, with ordered child keys."""

    rows: tuple[tuple[N, V], ...] = ()
    children: tuple[K, ...] = ()

    @property
    def values(self) -> Mapping[N, V]:
        """Values by name."""
        return MappingProxyType(dict(self.rows))


class Place[K: Hashable](msgspec.Struct, frozen=True, kw_only=True):
    """Sibling index under parent or among roots."""

    parent: Option[K]
    index: int


class Tree[V, K: Hashable, N: Hashable](msgspec.Struct, frozen=True, kw_only=True):
    """Owners by stable key, ordered under parents."""

    roots: tuple[K, ...]
    rows: tuple[tuple[K, Owner[V, K, N]], ...]

    @property
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
        held = self.owners
        return type(self)(
            roots=tuple(dict.fromkeys((*self.roots, *other.roots))),
            rows=tuple({**held, **{key: msgspec.structs.replace(owner, rows=tuple(dict(chain(held[key].rows, owner.rows)).items())) if key in held else owner for key, owner in other.rows}}.items()),
        )

    def places(self) -> dict[K, Place[K]]:
        """Returns place per owner."""
        return {
            key: Place(parent=parent, index=index)
            for parent, children in chain(((Nothing, self.roots),), ((Some(key), owner.children) for key, owner in self.owners.items()))
            for index, key in enumerate(children)
        }


class _Placed[V, K: Hashable, N: Hashable](msgspec.Struct, frozen=True, kw_only=True):
    """Owner with value rows at place."""

    key: K
    place: Place[K]
    rows: tuple[tuple[N, V], ...] = ()


class Inserted[V, K: Hashable, N: Hashable](_Placed[V, K, N], frozen=True, kw_only=True):
    """Owner entering tree at place."""


class Deleted[V, K: Hashable, N: Hashable](_Placed[V, K, N], frozen=True, kw_only=True):
    """Owner leaving tree from place."""


class Moved[K: Hashable](msgspec.Struct, frozen=True, kw_only=True):
    """Owner changing parent or sibling position from before to place."""

    key: K
    before: Place[K]
    place: Place[K]


class Reparametrized[V, K: Hashable, N: Hashable](msgspec.Struct, frozen=True, kw_only=True):
    """Owner values changed or removed as prior rows, and changed or added as later rows."""

    key: K
    prior: tuple[tuple[N, V], ...] = ()
    later: tuple[tuple[N, V], ...] = ()

    @property
    def before(self) -> Mapping[N, V]:
        """Prior values by name."""
        return MappingProxyType(dict(self.prior))

    @property
    def after(self) -> Mapping[N, V]:
        """Later values by name."""
        return MappingProxyType(dict(self.later))

    @classmethod
    def of(cls, key: K, before: Mapping[N, V], after: Mapping[N, V], *, equal: Callable[[V, V], bool] = eq) -> Option[Self]:
        """Returns changed, removed, and added values."""
        old = tuple((name, value) for name, value in before.items() if name not in after or not equal(after[name], value))
        new = tuple((name, value) for name, value in after.items() if name not in before or not equal(before[name], value))
        return Some(cls(key=key, prior=old, later=new)) if old or new else Nothing

    @classmethod
    def shared(cls, key: K, before: Mapping[N, V], after: Mapping[N, V], *, equal: Callable[[V, V], bool] = eq) -> Option[Self]:
        """Returns changed values of names both hold."""
        return cls.of(key, {name: value for name, value in before.items() if name in after}, {name: value for name, value in after.items() if name in before}, equal=equal)


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
        feeds: dict[K, list[tuple[K, int]]] = {key: [] for key in sizes}
        for source, slot, target in links:
            if target in feeds:
                feeds[target].append((source, slot))
        order = tuple(TopologicalSorter({key: tuple(source for source, _ in fed) for key, fed in feeds.items()}).static_order())
        position = {key: index for index, key in enumerate(order)}
        column: dict[K, int] = {}
        for key in order:
            column[key] = max((column[source] + 1 for source, _ in feeds[key]), default=0)
        rank = {key: min(((position[source], slot) for source, slot in feeds[key]), default=(position[key], -1)) for key in order}
        grouped: dict[Option[K], dict[int, list[K]]] = {}
        widths: dict[int, float] = {}
        for key in order:
            depth, width = column[key], sizes[key][0]
            grouped.setdefault(bands[key], {}).setdefault(depth, []).append(key)
            widths[depth] = max(widths[depth], width) if depth in widths else width
        cells = {(band, depth): sorted(rows[depth], key=rank.__getitem__) for band, rows in grouped.items() for depth in sorted(rows)}
        left, top = origin(order[0])
        lefts = tuple(accumulate((widths[depth] + self.across for depth in range(len(widths))), initial=left))
        stacks = {band: tuple(sum(sizes[key][1] for key in cells[band, depth]) + self.down * (len(rows[depth]) - 1) for depth in rows) for band, rows in grouped.items()}
        tops = dict(zip(grouped, accumulate(((max((*heights, 0.0)) if len(heights) < len(widths) else max(heights)) + self.band for heights in stacks.values()), sub, initial=top)))
        return {key: (lefts[depth], y) for (band, depth), keys in cells.items() for key, y in zip(keys, accumulate((sizes[key][1] + self.down for key in keys), sub, initial=tops[band]))}


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [TREES]


def diff[V, K: Hashable, N: Hashable](before: Tree[V, K, N], after: Tree[V, K, N], *, equal: Callable[[V, V], bool] = eq) -> tuple[Edit[V, K, N], ...]:
    """Returns deletions, value changes, and target-ordered moves and insertions."""
    was, now = before.places(), after.places()
    old, new = before.owners, after.owners
    stay = frozenset(key for key, place in now.items() if key in was and was[key].parent == place.parent)
    orders = tuple(
        (
            tuple(key for key in parent.map(lambda key: old[key].children).default_value(before.roots) if key in stay),
            tuple(key for key in parent.map(lambda key: new[key].children).default_value(after.roots) if key in stay),
        )
        for parent in {now[key].parent for key in stay}
    )
    kept = frozenset(key for old, new in orders for block in SequenceMatcher(None, old, new, autojunk=False).get_matching_blocks() for key in new[block.b : block.b + block.size])
    return (
        *(Deleted(key=key, place=was[key], rows=owner.rows) for key, owner in old.items() if key not in new),
        *(edit for key, owner in new.items() if key in old for edit in Reparametrized[V, K, N].of(key, old[key].values, owner.values, equal=equal).to_list()),
        *(Moved(key=key, before=was[key], place=place) if key in was else Inserted(key=key, place=place, rows=new[key].rows) for key, place in now.items() if key not in kept),
    )


def merge[V, K: Hashable, N: Hashable](state: Tree[V, K, N], edits: Iterable[Edit[V, K, N]]) -> Tree[V, K, N]:
    """Applies edits drawn against state, value changes of one owner in edit order and entering owners by target index."""
    held = tuple(edits)
    gone = frozenset(edit.key for edit in held if isinstance(edit, Deleted))
    lifted = gone | frozenset(edit.key for edit in held if isinstance(edit, Moved))
    owners = state.owners
    base = {**{key: dict(owner.rows) for key, owner in owners.items() if key not in gone}, **{edit.key: dict(edit.rows) for edit in held if isinstance(edit, Inserted)}}
    for edit in held:
        if isinstance(edit, Reparametrized) and edit.key in base:
            for name, _ in edit.prior:
                base[edit.key].pop(name, None)
            base[edit.key].update(edit.later)
    entering = sorted(((edit.place, edit.key) for edit in held if isinstance(edit, Inserted | Moved)), key=lambda row: row[0].index)
    order = {parent: [key for key in children if key not in lifted] for parent, children in chain(((Nothing, state.roots),), ((Some(key), owner.children) for key, owner in owners.items()))}
    for place, key in entering:
        order.setdefault(place.parent, []).insert(place.index, key)
    return Tree(roots=tuple(order.get(Nothing, ())), rows=tuple((key, Owner(rows=tuple(owned.items()), children=tuple(order.get(Some(key), ())))) for key, owned in base.items()))


def transfer[V, K: Hashable, N: Hashable](target: Tree[V, K, N], source: Tree[V, K, N], *, equal: Callable[[V, V], bool] = eq) -> tuple[Reparametrized[V, K, N], ...]:
    """Returns source values for owners and names both share."""
    held = target.owners
    return tuple(edit for key, owner in source.owners.items() if key in held for edit in Reparametrized[V, K, N].shared(key, held[key].values, owner.values, equal=equal).to_list())


def revert[V, K: Hashable, N: Hashable](current: Tree[V, K, N], applied: Iterable[Reparametrized[V, K, N]], *, equal: Callable[[V, V], bool] = eq) -> tuple[Reparametrized[V, K, N], ...]:
    """Returns prior values where current matches applied."""
    owners = current.owners
    restored = list[Reparametrized[V, K, N]]()
    for edit in applied:
        if (owner := owners.get(edit.key)) is not None:
            values = owner.values
            restored.extend(
                Reparametrized[V, K, N].shared(edit.key, {name: value for name, value in edit.after.items() if name in values and equal(values[name], value)}, edit.before, equal=equal).to_list()
            )
    return tuple(restored)


# --- [WALKS]


def reached[T: Hashable](seeds: Iterable[T], children: Callable[[T], Iterable[T]], /) -> Block[T]:
    """Returns seeds and every node children reach from them, depth first in child order, each node once at its first reach."""

    def walked() -> Iterable[T]:
        pending, seen = [iter(seeds)], set[T]()
        while pending:
            for node in pending[-1]:
                if node not in seen:
                    seen.add(node)
                    yield node
                    pending.append(iter(children(node)))
                    break
            else:
                pending.pop()

    return Block(walked())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Deleted", "Edit", "Gaps", "Inserted", "Moved", "Owner", "Place", "Reparametrized", "Toward", "Tree", "diff", "merge", "reached", "revert", "transfer"]
