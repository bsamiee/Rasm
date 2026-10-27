"""Keyed owner trees with their diffs, merges, transfers, and reverts, layered layouts of linked boxes, dated histories under retention, preorder walks over any tree, and record lenses reading and rebuilding nested values."""

from collections import defaultdict
from collections.abc import Callable, Collection, Hashable, Iterable, Mapping
import datetime
from difflib import SequenceMatcher
from functools import cached_property, reduce
from graphlib import TopologicalSorter
from itertools import accumulate
from operator import sub
from types import MappingProxyType, MemberDescriptorType
from typing import Annotated, Self

from expression import Nothing, Option, Some
from expression.collections import Block
import msgspec

from blender.records.fields import NonNegative, Side

# --- [TYPES] ----------------------------------------------------------------------------

type Edit[V, K: Hashable = str] = Inserted[V, K] | Deleted[V, K] | Moved[K] | Reparametrized[V, K]
type Lens = tuple[object, ...]


# --- [MODELS] ---------------------------------------------------------------------------
# --- [TREES]


class Owner[V, K: Hashable = str](msgspec.Struct, frozen=True, kw_only=True):
    """Owner values by name and ordered child keys."""

    values: Mapping[str, V]
    children: tuple[K, ...] = ()


class _Place[K: Hashable = str](msgspec.Struct, frozen=True, kw_only=True):
    """Sibling index under parent or among roots."""

    parent: Option[K]
    index: int


class Tree[V, K: Hashable = str](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Owners by stable key, ordered under parents."""

    roots: tuple[K, ...]
    rows: tuple[tuple[K, Owner[V, K]], ...]

    @cached_property
    def owners(self) -> Mapping[K, Owner[V, K]]:
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
                {**self.owners, **{key: msgspec.structs.replace(owner, values={**self.owners[key].values, **owner.values}) if key in self.owners else owner for key, owner in other.rows}}.items()
            ),
        )

    def children(self, parent: Option[K]) -> tuple[K, ...]:
        """Returns ordered child keys under parent, roots for Nothing."""
        return parent.map(lambda key: self.owners[key].children).default_value(self.roots)

    def places(self) -> dict[K, _Place[K]]:
        """Returns place per owner."""
        return {key: _Place(parent=parent, index=index) for parent in (Nothing, *map(Some, self.owners)) for index, key in enumerate(self.children(parent))}


class _Placed[V, K: Hashable = str](msgspec.Struct, frozen=True, kw_only=True):
    """Owner with values at place."""

    key: K
    place: _Place[K]
    values: Mapping[str, V]


class Inserted[V, K: Hashable = str](_Placed[V, K], frozen=True, kw_only=True):
    """Owner entering tree at place."""


class Deleted[V, K: Hashable = str](_Placed[V, K], frozen=True, kw_only=True):
    """Owner leaving tree from place."""


class Moved[K: Hashable = str](msgspec.Struct, frozen=True, kw_only=True):
    """Owner changing parent or sibling position."""

    key: K
    before: _Place[K]
    after: _Place[K]


class Reparametrized[V, K: Hashable = str](msgspec.Struct, frozen=True, kw_only=True):
    """Owner values changed, removed, or added."""

    key: K
    before: Mapping[str, V]
    after: Mapping[str, V]

    @classmethod
    def of(cls, key: K, before: Mapping[str, V], after: Mapping[str, V]) -> Option[Self]:
        """Returns changed, removed, and added values."""
        old = {name: value for name, value in before.items() if name not in after or after[name] != value}
        new = {name: value for name, value in after.items() if name not in before or before[name] != value}
        return Some(cls(key=key, before=old, after=new)) if old or new else Nothing

    @classmethod
    def shared(cls, key: K, before: Mapping[str, V], after: Mapping[str, V]) -> Option[Self]:
        """Returns changed values of names both hold."""
        return cls.of(key, {name: value for name, value in before.items() if name in after}, {name: value for name, value in after.items() if name in before})


# --- [LAYOUTS]


class Size(msgspec.Struct, frozen=True, kw_only=True):
    """Drawn box size in layout units."""

    width: NonNegative
    height: NonNegative


class Gaps(msgspec.Struct, frozen=True, kw_only=True):
    """Layout gaps between columns, between rows, and between bands."""

    across: NonNegative = 80.0
    down: NonNegative = 20.0
    band: NonNegative = 60.0

    def arranged[K: Hashable](
        self, sizes: Mapping[K, Size], links: tuple[tuple[K, int, K], ...], bands: Mapping[K, Option[K]], origin: Callable[[K], tuple[float, float]]
    ) -> dict[K, tuple[float, float]]:
        """Returns the top left corner per box, columns by depth over acyclic (source, slot, target) links, rows by source slot order, one band's boxes stacked together, growing right and down from the first box's origin."""
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
        lefts = tuple(accumulate((max(sizes[key].width for key in order if column[key] == depth) + self.across for depth in depths), initial=left))
        stacks = {cell: sum(sizes[key].height for key in keys) + self.down * (len(keys) - 1) if keys else 0.0 for cell, keys in cells.items()}
        tops = dict(zip(banded, accumulate((max(stacks[band, depth] for depth in depths) + self.band for band in banded), sub, initial=top)))
        return {key: (lefts[depth], y) for (band, depth), keys in cells.items() for key, y in zip(keys, accumulate((sizes[key].height + self.down for key in keys), sub, initial=tops[band]))}


# --- [HISTORY]


class Retention(msgspec.Struct, frozen=True, kw_only=True):
    """History capacity in entries and merge interval in seconds."""

    capacity: Side = 10
    interval: NonNegative = 5.0


class Dated[T](msgspec.Struct, frozen=True, kw_only=True):
    """Value with the moment it entered a history."""

    moment: Annotated[datetime.datetime, msgspec.Meta(tz=True)]
    value: T


class History[T](msgspec.Struct, frozen=True, kw_only=True):
    """Past and future entries nearest first."""

    past: tuple[Dated[T], ...] = ()
    future: tuple[Dated[T], ...] = ()

    def add(self, entry: Dated[T], retention: Retention) -> Self:
        """Adds entry as the nearest past and drops the future, an entry within the merge interval of the nearest past merging into it, a held value leaving its place."""
        held = tuple(present for present in self.past if present.value != entry.value)
        match held:
            case (head, *rest) if not self.future and 0 <= (entry.moment - head.moment).total_seconds() < retention.interval:
                past = (msgspec.structs.replace(head, moment=entry.moment), *rest)
            case _:
                past = (entry, *held)
        return type(self)(past=past[: retention.capacity])

    def at(self, offset: int) -> Option[Dated[T]]:
        """Returns the entry at a signed one-based offset, past for positive and future for negative, `Nothing` at zero or past either end."""
        source, distance = self.past if offset > 0 else self.future, abs(offset)
        return Some(source[distance - 1]) if 0 < distance <= len(source) else Nothing

    def step(self, offset: int, current: Dated[T], retention: Retention) -> Option[tuple[T, "History[T]", "History[T]"]]:
        """Returns the value at a signed one-based offset, the history arrived there with current and the entries passed held across from it, and the history a refused write leaves with both branches kept and current nearest across."""
        distance = abs(offset)
        source, other = (self.past, self.future) if offset > 0 else (self.future, self.past)

        def oriented(near: tuple[Dated[T], ...], far: tuple[Dated[T], ...]) -> "History[T]":
            return type(self)(past=near, future=far[: retention.capacity]) if offset > 0 else type(self)(past=far[: retention.capacity], future=near)

        return self.at(offset).map(lambda entry: (entry.value, oriented(source[distance:], (*reversed(source[: distance - 1]), current, *other)), oriented(source, (current, *other))))

    def remove(self, value: T) -> Self:
        """Removes the entries holding value."""
        return type(self)(past=tuple(entry for entry in self.past if entry.value != value), future=tuple(entry for entry in self.future if entry.value != value))


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [TREES]


def diff[V, K: Hashable](before: Tree[V, K], after: Tree[V, K]) -> tuple[Edit[V, K], ...]:
    """Returns deletions, value changes, and target-ordered moves and insertions."""
    was, now = before.places(), after.places()
    stay = frozenset(key for key, place in now.items() if key in was and was[key].parent == place.parent)
    orders = tuple((tuple(key for key in before.children(parent) if key in stay), tuple(key for key in after.children(parent) if key in stay)) for parent in {now[key].parent for key in stay})
    kept = frozenset(key for old, new in orders for block in SequenceMatcher(None, old, new, autojunk=False).get_matching_blocks() for key in new[block.b : block.b + block.size])
    return (
        *(Deleted(key=key, place=was[key], values=owner.values) for key, owner in before.owners.items() if key not in after.owners),
        *(edit for key, owner in after.owners.items() if key in before.owners for edit in Reparametrized[V, K].of(key, before.owners[key].values, owner.values).to_list()),
        *(Moved(key=key, before=was[key], after=place) if key in was else Inserted(key=key, place=place, values=after.owners[key].values) for key, place in now.items() if key not in kept),
    )


def merge[V, K: Hashable](state: Tree[V, K], edits: Iterable[Edit[V, K]]) -> Tree[V, K]:
    """Applies edits drawn against state."""
    values = {key: owner.values for key, owner in state.owners.items()}
    lifted: set[K] = set()
    entering: list[tuple[_Place[K], K]] = []
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
                values[key] = {**{name: value for name, value in values[key].items() if name not in old}, **new}
    order = defaultdict[Option[K], list[K]](list, {parent: [key for key in state.children(parent) if key not in lifted] for parent in (Nothing, *map(Some, state.owners))})
    for place, key in sorted(entering, key=lambda row: row[0].index):
        order[place.parent].insert(place.index, key)
    return Tree(roots=tuple(order[Nothing]), rows=tuple((key, Owner(values=owned, children=tuple(order[Some(key)]))) for key, owned in values.items()))


def transfer[V, K: Hashable](target: Tree[V, K], source: Tree[V, K]) -> tuple[Reparametrized[V, K], ...]:
    """Returns source values for owners and names both share."""
    return tuple(edit for key, owner in source.owners.items() if key in target.owners for edit in Reparametrized[V, K].shared(key, target.owners[key].values, owner.values).to_list())


def revert[V, K: Hashable](current: Tree[V, K], applied: Iterable[Reparametrized[V, K]]) -> tuple[Reparametrized[V, K], ...]:
    """Returns prior values where current matches applied."""
    return tuple(
        restored
        for edit in applied
        for held in Option.of_optional(current.owners.get(edit.key)).to_list()
        for restored in Reparametrized[V, K]
        .shared(edit.key, held.values, {name: value for name, value in edit.before.items() if name in edit.after and held.values.get(name) == edit.after[name]})
        .to_list()
    )


# --- [WALKS]


def preorder[T](seed: T, children: Callable[[T], Iterable[T]]) -> Block[T]:
    """Returns seed and every node children reach from it, depth first in child order."""
    frontier: tuple[T, ...] = (seed,)
    return Block.unfold(lambda pending: Some((pending[0], (*children(pending[0]), *pending[1:]))) if pending else Nothing, frontier)


# --- [PATHS]


def _walked(value: object, lens: Lens) -> Option[tuple[object, Callable[[object], object]]]:
    match lens, value:
        case (), _:
            return Some((value, lambda new: new))
        case (MemberDescriptorType() as field, *rest), msgspec.Struct() if isinstance(value, field.__objclass__):
            return _walked(field.__get__(value), tuple(rest)).map(lambda found: (found[0], lambda new: msgspec.structs.replace(value, **{field.__name__: found[1](new)})))
        case (int(index), *rest), tuple() if 0 <= index < len(value):
            return _walked(value[index], tuple(rest)).map(lambda found: (found[0], lambda new: (*value[:index], found[1](new), *value[index + 1 :])))
        case _:
            return Nothing


def focused(value: object, lens: Lens) -> Option[object]:
    """Returns the value a lens of struct fields and 0-based tuple indexes reaches, `Nothing` where a field's struct misses the held value or an index falls outside its tuple."""
    return _walked(value, lens).map(lambda found: found[0])


def amended[T: msgspec.Struct](record: T, lens: Lens, change: Callable[[object], object]) -> Option[T]:
    """Returns record with the value a lens opening at one of its fields reaches replaced by its change, every enclosing struct and tuple rebuilt, `Nothing` where the lens reaches nothing."""
    match lens:
        case (MemberDescriptorType() as field, *rest) if isinstance(record, field.__objclass__):
            return _walked(field.__get__(record), tuple(rest)).map(lambda found: msgspec.structs.replace(record, **{field.__name__: found[1](change(found[0]))}))
        case _:
            return Nothing


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Dated",
    "Deleted",
    "Edit",
    "Gaps",
    "History",
    "Inserted",
    "Lens",
    "Moved",
    "Owner",
    "Reparametrized",
    "Retention",
    "Size",
    "Tree",
    "amended",
    "diff",
    "focused",
    "merge",
    "preorder",
    "revert",
    "transfer",
]
