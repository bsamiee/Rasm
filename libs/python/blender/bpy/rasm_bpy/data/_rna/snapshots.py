"""RNA value snapshots: capture walks into value trees, values by owner path, host-ordered writes, projection, and clipboard transfer."""

from collections import defaultdict
from collections.abc import Callable, Collection, Generator, Iterable, Iterator, Mapping
from functools import partial
from types import MappingProxyType
from typing import Final, Protocol, Self, TypeIs

import bpy
from expression import Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np

from rasm_bpy.data._rna.descriptors import _effective, _LEADING, _sequenced, Descriptor, Value
from rasm_bpy.data._rna.paths import _defined, _definition, _rna, _struct_at, id_path, Native, RnaPath, Struct
from rasm_records import accumulated, Error, Fault, Faults, merge, Nothing, Ok, Outcome, Owner, Reason, transfer, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type _Enclosing = frozenset[tuple[int, str]]
type _Read[W] = Callable[[Descriptor, Native, _Enclosing, Option[Tree[W, RnaPath, RnaPath]]], Option[W]]
type _Walk = Generator[_Walk]


class _Elements(Protocol):
    """Struct collection the host grows and shrinks through its own calls."""

    def __len__(self) -> int: ...

    def __getitem__(self, index: int, /) -> object: ...

    def __iter__(self) -> Iterator[object]: ...

    def add(self, *values: object) -> object: ...

    def new(self, *values: object) -> object: ...

    def remove(self, item: object, /) -> object: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_ADDED: Final[Mapping[type[Struct], Callable[[_Elements, Mapping[str, object]], object]]] = MappingProxyType({
    bpy.types.CurveMapPoint: lambda items, values: items.new(*np.asarray(values["location"], dtype=np.float64)),
    bpy.types.CurveProfilePoint: lambda items, values: items.add(*np.asarray(values["location"], dtype=np.float64)),
    bpy.types.ColorRampElement: lambda items, values: items.new(values["position"]),
})

# --- [MODELS] ---------------------------------------------------------------------------


class Applied(msgspec.Struct, frozen=True, kw_only=True):
    """Snapshot write outcome per path."""

    written: tuple[RnaPath, ...] = ()
    unchanged: tuple[RnaPath, ...] = ()
    skipped: tuple[RnaPath, ...] = ()
    outcome: Outcome[None] = Ok(None)

    @classmethod
    def join(cls, outcomes: Iterable[Self]) -> Self:
        """Concatenates each path category in operation order."""
        held = tuple(outcomes)
        return cls(
            written=tuple(path for outcome in held for path in outcome.written),
            unchanged=tuple(path for outcome in held for path in outcome.unchanged),
            skipped=tuple(path for outcome in held for path in outcome.skipped),
            outcome=accumulated(outcome.outcome for outcome in held).map(lambda _: None),
        )

    @classmethod
    def refused(cls, faults: Faults) -> Self:
        """Returns outcome failing with faults."""
        return cls(outcome=Error(faults))

    @classmethod
    def reported(cls, path: RnaPath, answer: Outcome[Option[Value | Struct]]) -> Self:
        """Classifies one member write as written, unchanged, or refused, each fault naming the member by its full path."""
        return answer.map(lambda prior: cls(written=(path,)) if prior.is_some() else cls(unchanged=(path,))).default_with(
            lambda faults: cls.refused(tuple(msgspec.structs.replace(fault, subject=_rna(path)) for fault in faults))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        )

    def then(self, step: Callable[[], Self]) -> Self:
        """Joins the outcome of a step that runs once this one holds."""
        return type(self).join((self, step())) if self.outcome.is_ok() else self


class Snapshot(msgspec.Struct, frozen=True, kw_only=True):
    """Assignable values keyed by owner path under struct type, each value named by its member path under that owner."""

    # --- [STATE]

    struct: str
    tree: Tree[Value, RnaPath, RnaPath]

    # --- [CAPTURE]

    @classmethod
    def of(cls, struct: type[Struct], values: Mapping[RnaPath, Value]) -> Self:
        """Returns snapshot of member values, each path placed under the owner its last identifier names.

        Args:
            struct: Struct type the paths start from.
            values: Value per member path.

        Returns:
            Snapshot holding one owner row per owner path, an owner written before the owners beneath it.
        """
        rows: defaultdict[RnaPath, list[tuple[RnaPath, Value]]] = defaultdict(list)
        for path, value in values.items():
            last = max((index for index, step in enumerate(path) if isinstance(step, str)), default=0)
            rows[path[:last]].append((path[last:], value))

        owners = tuple(sorted(rows, key=len))
        return cls(struct=_definition(struct).identifier, tree=Tree(roots=owners, rows=tuple((owner, Owner[Value, RnaPath, RnaPath](rows=tuple(rows[owner]))) for owner in owners)))

    @classmethod
    def capture(cls, owner: Native, selection: Option[tuple[Descriptor, ...]] = Nothing, *, owned: Callable[[str], bool] = lambda _: False, inactive: bool = False) -> Outcome[Self]:
        """Captures assignable values keyed by owner path, the selected members alone when given.

        Args:
            owner: Struct whose members the snapshot holds.
            selection: Members to capture, every host-declared saved member when Nothing.
            owned: Whether the capturing extension declares a script property by its identifier, captured beside the host's own.
            inactive: Whether latent saved values disabled by their current governors join the snapshot.

        Returns:
            Snapshot, refused fault for a removed struct.
        """

        def stored(descriptor: Descriptor, struct: Native, _: _Enclosing, _compound: Option[Tree[Value, RnaPath, RnaPath]]) -> Option[Value]:
            return Some(descriptor.read(struct)).filter(
                lambda value: not (isinstance(descriptor.prop, bpy.types.EnumProperty) and not descriptor.prop.is_enum_flag and isinstance(value, str) and not value)
            )

        return (
            catch(_Capture[Value](owner, stored, selection, owned=owned, inactive=inactive).tree, exception=ReferenceError)()
            .map_error(partial(Fault.of, Reason.REFUSED, repr(owner)))
            .map(lambda tree: cls(struct=_definition(owner).identifier, tree=tree))
        )

    # --- [APPLICATION]

    def apply(self, owner: Native) -> Applied:
        """Writes snapshot to owner in host order."""
        match _definition(owner).identifier:
            case self.struct:
                return _applied(owner, self.tree, lambda _, value: value)
            case identifier:
                return Applied.refused(Fault.of(Reason.OUTSIDE, repr(owner), self.struct, member=identifier))

    def selection(self, owner: Native) -> tuple[Descriptor, ...]:
        """Resolves members and compound roots assigned on owner."""
        owners = self.tree.owners
        return (
            *(descriptor for key in owners if key for descriptor in Descriptor.resolve(owner, key, bpy.types.Property).to_option().to_list() if descriptor.compound),
            *(descriptor for key, held in owners.items() for name in held.values for descriptor in Descriptor.resolve(owner, (*key, *name), bpy.types.Property).to_option().to_list()),
        )

    def projected(self, owner: Native, current: "Snapshot", locked: frozenset[RnaPath] = frozenset()) -> Outcome[tuple["Snapshot", "Snapshot"]]:
        """Returns unlocked assignments with resulting current state.

        Args:
            owner: Struct both snapshots describe.
            current: Snapshot of owner's present values.
            locked: Paths whose members and owners stay as current holds them.

        Returns:
            Sparse assignments and the state they leave, outside fault for a snapshot of another struct.
        """
        identifier = _definition(owner).identifier
        owners, present = self.tree.owners, current.tree.owners

        def within(path: RnaPath, roots: Collection[RnaPath]) -> bool:
            return any(path[: len(root)] == root for root in roots)

        def split() -> tuple[Snapshot, Snapshot]:
            free = {
                key: Owner[Value, RnaPath, RnaPath](
                    rows=tuple((name, value) for name, value in held.rows if not locked or not within((*key, *name), locked)),
                    children=tuple(child for child in held.children if not within(child, locked)),
                )
                for key, held in owners.items()
                if not within(key, locked)
            }
            compounds = frozenset(key for key in free if key for descriptor in Descriptor.resolve(owner, key, bpy.types.Property).to_option().to_list() if descriptor.compound)
            base = {key: held for key, held in present.items() if not within(key, compounds)}
            values = base | {
                key: held
                if within(key, compounds)
                else Owner[Value, RnaPath, RnaPath](rows=tuple({**previous.values, **held.values}.items()), children=tuple(dict.fromkeys((*previous.children, *held.children))))
                for key, held in free.items()
                for previous in (base.get(key, Owner[Value, RnaPath, RnaPath]()),)
            }
            nested = frozenset(child for held in values.values() for child in held.children)
            sparse = msgspec.structs.replace(self, tree=Tree(roots=tuple(key for key in self.tree.roots if key in free), rows=tuple(free.items())))
            matching = msgspec.structs.replace(self, tree=Tree(roots=tuple(dict.fromkeys(key for key in (*current.tree.roots, *sparse.tree.roots) if key not in nested)), rows=tuple(values.items())))
            return sparse, matching

        return accumulated(
            Ok(snapshot).filter_with(lambda held: held.struct == identifier, lambda held: Fault.of(Reason.OUTSIDE, repr(owner), held.struct, member=identifier)) for snapshot in (self, current)
        ).map(lambda _: split())

    # --- [TEXT]

    @property
    def text(self) -> str:
        """JSON text of the snapshot in deterministic key order, the form a clipboard or text block holds."""
        return msgspec.json.encode(self, order="deterministic").decode()

    @classmethod
    def parsed(cls, subject: str, text: str) -> Outcome[Self]:
        """Returns the snapshot JSON text holds, unreadable fault over subject for text no snapshot wrote."""
        return catch(lambda: msgspec.json.decode(text, type=cls), exception=msgspec.DecodeError)().map_error(partial(Fault.of, Reason.UNREADABLE, subject))

    def copied(self, manager: bpy.types.WindowManager) -> None:
        """Copies snapshot to the system clipboard as JSON."""
        manager.clipboard = self.text

    @staticmethod
    def pasted(manager: bpy.types.WindowManager, owners: tuple[Struct, ...]) -> Outcome[tuple[Applied, ...]]:
        """Applies the clipboard snapshot to each owner of its struct over the values it holds now, outside fault when no owner is of its struct."""
        subject = f"{manager!r}.clipboard"
        return (
            Snapshot
            .parsed(subject, manager.clipboard)
            .map(lambda clip: (clip, tuple(owner for owner in owners if _definition(owner).identifier == clip.struct)))
            .filter_with(
                lambda found: bool(found[1]), lambda found: Fault.of(Reason.OUTSIDE, subject, found[0].struct, member=", ".join(dict.fromkeys(_definition(owner).identifier for owner in owners)))
            )
            .bind(
                lambda found: accumulated(
                    Snapshot
                    .capture(owner)
                    .map(lambda target: msgspec.structs.replace(found[0], tree=merge(target.tree, transfer(target.tree, found[0].tree))).apply(owner))
                    .bind(lambda applied: applied.outcome.map(lambda _: applied))
                    for owner in found[1]
                )
            )
        )


# --- [SERVICES] -------------------------------------------------------------------------


class _Capture[W]:
    """One walk of a source into value tree rows under a value reader, a native walk holding compounds whole and keying top-level rows by ID session."""

    # --- [STATE]

    __slots__ = ("enclosing", "expanded", "inactive", "native", "owned", "owner", "owners", "read", "roots", "seen", "selection", "source", "valued")

    def __init__(
        self,
        owner: Native,
        read: _Read[W],
        selection: Option[tuple[Descriptor, ...]] = Nothing,
        *,
        prior: Option[Tree[W, RnaPath, RnaPath]] = Nothing,
        native: bool = False,
        nested: Option[tuple[Native, Descriptor, _Enclosing]] = Nothing,
        owned: Callable[[str], bool] = lambda _: False,
        valued: Option[Callable[[Descriptor], bool]] = Nothing,
        inactive: bool = False,
    ) -> None:
        """Holds the walk's reader, the prior tree it extends, and the members it selects.

        Args:
            owner: Struct the capture started from, root of native references.
            read: Value of one member from its descriptor, holder, enclosing addresses, and expanded compound tree, Nothing to leave it out.
            selection: Members to capture, every saved member when Nothing.
            prior: Tree whose values the walk keeps.
            native: Whether the walk holds compounds whole and keys top-level rows by ID session.
            nested: Struct, compound member, and enclosing structs of a native walk expanding that member alone.
            owned: Whether the walking extension declares a script property by its identifier, walked beside the host's own.
            valued: Which members the reader captures as values, the walk's assignment policy when absent.
            inactive: Whether the walk includes saved members and owners disabled by their current governors.
        """
        self.owner, self.read, self.native, self.owned = owner, read, native or nested.is_some(), owned
        self.inactive = inactive
        self.valued = valued.default_with(lambda: partial(Descriptor.valued, native=self.native))
        walk: tuple[Native, Option[Descriptor], _Enclosing] = nested.map(lambda held: (held[0], Some(held[1]), held[2])).default_value((owner, Nothing, frozenset[tuple[int, str]]()))
        self.source, self.expanded, self.enclosing = walk
        self.selection = nested.map(lambda held: tuple(held[1:2])).or_else(selection)
        self.roots = prior.map(lambda tree: tree.roots).default_value(())
        self.owners: dict[RnaPath, Owner[W, RnaPath, RnaPath]] = prior.map(lambda tree: dict(tree.rows)).default_with(dict)
        self.seen: set[tuple[int, str]] = set(self.enclosing)

    # --- [KEYS]

    @staticmethod
    def beneath(path: RnaPath, root: RnaPath) -> bool:
        """Whether path sits strictly under root."""
        return len(path) > len(root) and path[: len(root)] == root

    @staticmethod
    def address(struct: Native) -> tuple[int, str]:
        """Returns the walk identity of a struct or collection."""
        return (struct.as_pointer() if isinstance(struct, bpy.types.bpy_struct) else id(struct), _definition(struct).identifier)

    @staticmethod
    def arrayed(prop: bpy.types.Property, names: Collection[RnaPath]) -> TypeIs[bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty]:
        """Whether names hold an element of the numeric array member."""
        return isinstance(prop, bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty) and prop.is_array and any(len(name) > 1 and name[0] == prop.identifier for name in names)

    def keyed(self, struct: Native, path: RnaPath) -> RnaPath:
        """Returns the row key of a struct, a native top-level walk keying by session and path or by pointer."""
        if not (self.native and self.expanded.is_none()):
            return path
        block, at = _referenced(self.owner, struct, path)
        return (block.session_uid, *at) if isinstance(block, bpy.types.ID) else (self.address(struct)[0],)

    # --- [WALKS]

    def _read(self, descriptor: Descriptor, struct: Native, inside: _Enclosing, values: list[W]) -> _Walk:
        """Expands native compounds before reading one member into values."""
        enclosing = self.enclosing | inside
        compound: Option[Tree[W, RnaPath, RnaPath]] = Nothing
        if self.native and descriptor.compound:
            trees: list[Tree[W, RnaPath, RnaPath]] = []
            yield _Capture(  # ruff: ignore[private-member-access]
                self.owner, self.read, nested=Some((struct, Descriptor(root=type(struct), prop=descriptor.prop), enclosing)), owned=self.owned, valued=Some(self.valued), inactive=self.inactive
            )._captured(trees)
            compound = Some(trees[0])
        values.extend(self.read(descriptor, struct, enclosing, compound).to_list())

    def _branched(self, struct: Native, key: RnaPath, prop: bpy.types.Property, inside: _Enclosing, keys: list[RnaPath]) -> _Walk:
        """Appends pointer or collection child keys in native order."""
        path = (*key, prop.identifier)
        match prop:
            case bpy.types.PointerProperty(fixed_type=bpy.types.ID()) | bpy.types.CollectionProperty(fixed_type=bpy.types.ID()):
                return  # type: ignore[unreachable]
            case bpy.types.CollectionProperty() if path in self.owners:
                keys.extend(self.owners[path].children)
            case bpy.types.PointerProperty(is_readonly=True):
                yield from (self._walked(child, path, inside, keys) for child in Option.of_optional(getattr(struct, prop.identifier)).filter(lambda held: self.inactive or _effective(held)).to_list())
            case bpy.types.CollectionProperty():
                collection: bpy.types.bpy_prop_collection[Struct | None] = getattr(struct, prop.identifier)  # ty: ignore[unsound-assignment]
                items = tuple((index, item) for index, item in enumerate(collection) if item is not None)
                named: Option[tuple[RnaPath, ...]] = (  # ty: ignore[unsound-assignment]
                    Option
                    .of_optional(prop.fixed_type)
                    .bind(lambda fixed: Option.of_optional(fixed.name_property))
                    .map(lambda held: tuple(((getattr(item, held.identifier),),) for _, item in items))
                    .filter(lambda found: self.expanded.is_none() and len(set(found)) == len(found))
                )
                steps = named.default_with(lambda: tuple((index,) for index, _ in items))
                yield from (self._walked(item, (*path, *step), inside, keys) for step, (_, item) in zip(steps, items))
            case _:
                pass

    def _grouped(self, struct: Native, path: RnaPath, prop: bpy.types.Property, keys: tuple[RnaPath, ...]) -> tuple[RnaPath, ...]:
        """Returns the row of a struct collection holding keys, or keys themselves for any other member."""
        if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID) and (keys or not getattr(struct, prop.identifier)):
            self.owners.setdefault(row := (*path, prop.identifier), Owner(children=keys))
            return (row,)
        return keys

    def _walked(self, struct: Native, path: RnaPath, above: _Enclosing, keys: list[RnaPath]) -> _Walk:
        """Reserves one unseen owner, reads values and descendants, then appends its key."""
        if (identity := self.address(struct)) in self.seen:
            return
        self.seen.add(identity)
        inside, key = above | {identity}, self.keyed(struct, path)
        members = (
            *Descriptor.saved(struct, owned=self.owned, inactive=self.inactive),
            *(
                Descriptor(root=type(struct), prop=prop)
                for prop in Option.of_optional(_definition(struct).name_property).filter(lambda prop: self.expanded.is_some() and not prop.is_readonly).to_list()
            ),
        )
        held = self.owners.setdefault(key, Owner())
        previous = held.values
        values: dict[RnaPath, W] = {}
        for descriptor in members:
            if self.valued(descriptor) and (descriptor.prop.identifier,) not in previous and not self.arrayed(descriptor.prop, previous):
                read: list[W] = []
                yield self._read(descriptor, struct, inside, read)
                values.update(((descriptor.prop.identifier,), value) for value in read)
        branches: list[tuple[bpy.types.Property, list[RnaPath]]] = []
        for descriptor in members:
            if not (self.native and descriptor.compound):
                children: list[RnaPath] = []
                branches.append((descriptor.prop, children))
                yield self._branched(struct, path, descriptor.prop, inside, children)
        nested = tuple(at for prop, children in branches for at in self._grouped(struct, path, prop, tuple(children)))
        merged: dict[RnaPath, W] = {**previous, **values}
        self.owners[key] = Owner(rows=tuple(merged.items()), children=tuple(dict.fromkeys((*held.children, *nested))))
        for descriptor in members:
            if self.arrayed(descriptor.prop, previous):
                yield from (
                    self._picked(Descriptor(root=type(self.source), owner=path, prop=descriptor.prop, coordinates=at), inside, []) for at in np.ndindex(*Descriptor.dimensions(descriptor.prop))
                )
        keys.append(key)

    def _picked(self, descriptor: Descriptor, above: _Enclosing, keys: list[RnaPath]) -> _Walk:
        """Reads selected members, expanding array elements and enclosing native compounds through child walks."""
        match descriptor.owner_of(self.source, inactive=self.inactive):
            case Option(tag="some", some=struct):
                pass
            case _:
                return
        inside = above | {self.address(struct)}
        if self.native and self.expanded.is_none() and isinstance(self.source, bpy.types.bpy_struct) and isinstance(struct, bpy.types.bpy_struct):
            enclosing = (
                Descriptor(root=type(self.source), owner=at[len(base) :], prop=found.prop)
                for base, path in id_path(self.source).bind(lambda base: id_path(struct).map(lambda own: (base, (*own, descriptor.prop.identifier)))).to_list()
                for ancestor in struct.rna_ancestors()
                for at in id_path(ancestor).to_list()
                for found in Descriptor.saved(ancestor, owned=self.owned, inactive=self.inactive)
                if found.compound
                for prefix in ((*at, found.prop.identifier),)
                if self.beneath(prefix, base) and self.beneath(path, prefix)
            )
            match Block.of_seq(enclosing).try_head():
                case Option(tag="some", some=found):
                    yield self._picked(found, above, keys)
                    return
                case _:
                    pass
        if self.expanded != Some(descriptor) and self.valued(descriptor):
            key = self.keyed(struct, descriptor.owner)
            name: RnaPath = (descriptor.prop.identifier, *descriptor.coordinates)
            held = self.owners.get(key, Owner[W, RnaPath, RnaPath]())
            values = held.values
            if self.native and any(self.beneath(key, (*at, *held_name)) for at, kept in self.owners.items() for held_name in kept.values):
                return
            if name in values or (descriptor.prop.identifier,) in values:
                keys.append(key)
                return
            if not descriptor.coordinates and self.arrayed(descriptor.prop, values):
                yield from (self._picked(msgspec.structs.replace(descriptor, coordinates=coordinates), above, keys) for coordinates in np.ndindex(*Descriptor.dimensions(descriptor.prop)))
                return
            read: list[W] = []
            yield self._read(descriptor, struct, inside, read)
            for value in read:
                self.owners[key] = Owner(rows=(*held.rows, (name, value)), children=held.children)
                keys.append(key)
            return
        children: list[RnaPath] = []
        yield self._branched(struct, descriptor.owner, descriptor.prop, inside, children)
        nested = tuple(children)
        if isinstance(descriptor.prop, bpy.types.CollectionProperty):
            self.owners.setdefault(descriptor.path, Owner(children=nested))
            nested = (descriptor.path,)
        elif self.expanded != Some(descriptor):
            keys.extend(nested)
            return
        parent = self.owners.setdefault(descriptor.owner, Owner())
        self.owners[descriptor.owner] = msgspec.structs.replace(parent, children=tuple(dict.fromkeys((*parent.children, *nested))))
        keys.append(descriptor.owner)

    def _captured(self, trees: list[Tree[W, RnaPath, RnaPath]]) -> _Walk:
        """Appends a completed tree after selected or whole-source walks finish."""
        roots = list(self.roots)
        match self.selection:
            case Option(tag="some", some=descriptors):
                yield from (self._picked(descriptor, frozenset(), roots) for descriptor in descriptors)
            case _:
                yield self._walked(self.source, (), frozenset(), roots)
        nested = frozenset(child for held in self.owners.values() for child in held.children)
        trees.append(Tree(roots=tuple(dict.fromkeys(key for key in roots if key not in nested)), rows=tuple(self.owners.items())))

    def tree(self) -> Tree[W, RnaPath, RnaPath]:
        """Returns selected or whole-source values under owner paths, preserving prior roots and rows."""
        trees: list[Tree[W, RnaPath, RnaPath]] = []
        _walk(self._captured(trees))
        return trees[0]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WALKS]


def _walk(steps: _Walk) -> None:
    """Runs child walks depth first, propagating native invalidation through suspended owners."""
    pending = [steps]
    failed: Option[ReferenceError] = Nothing
    while pending:
        try:
            child = failed.map(pending[-1].throw).default_with(lambda: next(pending[-1]))
        except StopIteration:
            pending.pop()
            failed = Nothing
        except ReferenceError as error:
            pending.pop()
            if not pending:
                raise
            failed = Some(error)
        else:
            pending.append(child)
            failed = Nothing


# --- [CAPTURE]


def _referenced(owner: Native, struct: Native, path: RnaPath) -> tuple[Native, RnaPath]:
    """Returns struct as its ID and path, a collection through owner's ID path, else struct itself under an empty path."""
    match struct, owner:
        case bpy.types.bpy_struct(id_data=bpy.types.ID() as block), _:
            return id_path(struct).map(lambda found: (block, found)).default_value((struct, ()))  # ty: ignore[unsound-return-statement]
        case bpy.types.bpy_prop_collection(), bpy.types.bpy_struct(id_data=bpy.types.ID() as block):
            return id_path(owner).map(lambda found: (block, (*found, *path))).default_value((struct, ()))  # ty: ignore[unsound-return-statement]
        case _:
            return (struct, ())


# --- [WRITES]


def _applied[W](owner: Native, tree: Tree[W, RnaPath, RnaPath], projected: Callable[[Descriptor, W], Value | Struct | Tree[W, RnaPath, RnaPath]]) -> Applied:
    """Writes stored or native values in host order, every write, resize, and skip reported once.

    `LEADING` members go first across every owner, then each root writes its members, resizes its collections, and descends.
    Each write re-reads its holder through its ID, an earlier write (`type`, `dimensions`) refining its class, and missing members resolve again.

    Args:
        owner: Struct or collection the tree's owner paths start from.
        tree: Values keyed by owner path and member path.
        projected: Host value of one held value, a tree for a compound written beneath it.

    Returns:
        Writes, unchanged and skipped paths, and accumulated faults.
    """

    def application(owner: Native, tree: Tree[W, RnaPath, RnaPath], reports: list[Applied]) -> _Walk:
        owners = tree.owners
        values = {key: held.values for key, held in owners.items()}
        leads = tuple(
            (struct, key, Descriptor(root=type(struct), prop=prop), held[prop.identifier,])
            for prop in _LEADING
            for key, held in values.items()
            if (prop.identifier,) in held
            for struct in _struct_at(owner, key, bpy.types.bpy_struct).to_list()
            if _definition(struct).properties.get(prop.identifier) == prop
        )
        completed = frozenset((key, (descriptor.prop.identifier,)) for _, key, descriptor, _ in leads)

        def current(struct: Native) -> Native:
            match struct:
                case bpy.types.bpy_struct(id_data=bpy.types.ID() as block):
                    return id_path(struct).bind(lambda path: _struct_at(block, path)).default_value(struct)
                case _:
                    return struct

        def member(struct: Native, key: RnaPath, descriptor: Descriptor, value: W, answers: list[Applied]) -> _Walk:
            desired, holder = projected(descriptor, value), current(struct)
            if isinstance(desired, Tree):
                yield application(holder, desired, answers)
            else:
                answers.append(Applied.reported((*key, *descriptor.path), descriptor.write(holder, desired)))

        @catch(exception=RuntimeError)
        def changed(items: _Elements, path: RnaPath, rows: tuple[Mapping[str, object], ...], adding: Callable[[_Elements, Mapping[str, object]], object], *, indexed: bool) -> Applied:
            report = Applied(written=(path,) if len(items) != len(rows) else ())
            for values in rows[len(items) :]:
                adding(items, values)
            for _ in range(len(items) - len(rows)):
                items.remove(len(items) - 1 if indexed else items[-1])
            return report

        def resized(struct: Native, path: RnaPath, prop: bpy.types.CollectionProperty, fixed: Struct) -> Applied:
            if path not in owners:
                return Applied()
            held = _definition(fixed).properties
            rows = tuple(
                {
                    identifier: projected(Descriptor(root=type(fixed), prop=held[identifier]), value)
                    for name, value in values[at].items()
                    for identifier in name[:1]
                    if len(name) == 1 and isinstance(identifier, str) and identifier in held
                }
                for at in owners[path].children
            )
            items: _Elements = getattr(struct, prop.identifier)  # ty: ignore[unsound-assignment]
            subject = f"{struct!r}.{prop.identifier}"
            match _ADDED.get(type(fixed)), fixed:
                case None, bpy.types.PropertyGroup() if prop.is_runtime:
                    return changed(items, path, rows, lambda grown, _: grown.add(), indexed=True).map_error(partial(Fault.of, Reason.REFUSED, subject)).default_with(Applied.refused)
                case None, _:
                    present = sum(item is not None for item in items)
                    return Applied() if present == len(rows) else Applied.refused(Fault.of(Reason.OUTSIDE, subject, str(len(rows)), member=str(present)))
                case adding, _:
                    return changed(items, path, rows, adding, indexed=False).map_error(partial(Fault.of, Reason.REFUSED, subject)).default_with(Applied.refused)

        def members(struct: Native, key: RnaPath, names: tuple[tuple[RnaPath, W], ...], answers: list[Applied]) -> _Walk:
            written: list[Applied] = []
            while True:
                found = tuple((name, value, Descriptor.resolve(struct, name, bpy.types.Property).to_option()) for name, value in names)
                ordered = _sequenced(
                    (descriptor.owner, holder, descriptor, value) for _, value, held in found for descriptor in held.to_list() for holder in _struct_at(struct, descriptor.owner).to_list()
                )
                rest = tuple[tuple[RnaPath, W], ...]((name, value) for name, value, held in found if held.is_none())
                batch: list[Applied] = []
                yield from (member(holder, key, descriptor, value, batch) for _, holder, descriptor, value in ordered)
                report = Applied.join(batch)
                written.append(report)
                if not report.outcome.is_ok():
                    break
                if rest and len(rest) < len(names):
                    struct, names = current(struct), rest
                    continue
                written.append(Applied(skipped=tuple((*key, *name) for name, _ in rest)))
                break
            answers.append(Applied.join(written))

        def wrote(struct: Native, key: RnaPath, answers: list[Applied]) -> _Walk:
            written: list[Applied] = []
            yield members(struct, key, tuple((name, value) for name, value in values[key].items() if (key, name) not in completed), written)
            refined = current(struct)
            result = written[0].then(
                lambda: Applied.join(
                    resized(refined, (*key, prop.identifier), prop, fixed)
                    for held in _defined(refined).to_list()
                    for prop in held.properties
                    if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID)
                    for fixed in Option.of_optional(prop.fixed_type).to_list()
                )
            )
            answers.append(result)
            if result.outcome.is_ok():
                yield from (located(child, answers) for child in owners[key].children)
            if isinstance(refined, bpy.types.CurveMapping | bpy.types.CurveProfile):
                refined.update()

        def located(key: RnaPath, answers: list[Applied]) -> _Walk:
            match _struct_at(owner, key):
                case Option(tag="some", some=struct):
                    yield wrote(struct, key, answers)
                case _:
                    answers.append(Applied(skipped=(key,)))

        outcomes: list[Applied] = []
        try:
            leading = Applied()
            for prop in _LEADING:
                batch: list[Applied] = []
                yield from (member(struct, key, descriptor, value, batch) for struct, key, descriptor, value in (lead for lead in leads if lead[2].prop == prop))
                if not (leading := leading.then(lambda: Applied.join(batch))).outcome.is_ok():
                    break
            outcomes.append(leading)
            if leading.outcome.is_ok():
                yield from (located(key, outcomes) for key in tree.roots)
        except ReferenceError as error:
            reports.append(Applied.refused(Fault.of(Reason.REFUSED, repr(owner), error)))
        else:
            reports.append(Applied.join(outcomes))

    reports: list[Applied] = []
    _walk(application(owner, tree, reports))
    return reports[0]


def assigned(root: Struct, members: Mapping[RnaPath, Value]) -> Outcome[None]:
    """Writes members under root through the snapshot writer, in host order.

    Args:
        root: Struct the member paths start from.
        members: Value per member path.

    Returns:
        Faults of every refused write and of every path naming no member, accumulated.
    """
    written = Snapshot.of(type(root), members).apply(root)
    return accumulated((written.outcome, *(Error(Fault.of(Reason.ABSENT, repr(root), member=_rna(path))) for path in written.skipped))).map(lambda _: None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Applied", "Snapshot", "assigned"]
