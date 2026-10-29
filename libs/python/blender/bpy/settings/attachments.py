"""Form group attachments with value locks, histories, startup seeds, before and after recovery, and storage removal."""

from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
import datetime
from functools import partial
import inspect
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
from idprop.types import IDPropertyGroup
import msgspec

from blender.bpy.data.ids.store import embedded
from blender.bpy.data.rna.descriptors import custom_read, Descriptor
from blender.bpy.data.rna.mirrors import annotated, Changed, nested, pointed, Updated
from blender.bpy.data.rna.paths import Native, RnaPath, Struct
from blender.bpy.data.rna.restoration import Endpoints, native, NativeTree, NativeValue, persisted
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.extension.events import startup
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason, typed
from blender.records.fields import Dated, History, Retention
from blender.records.trees import merge, transfer, Tree

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Host = bpy.types.ID | bpy.types.Node | bpy.types.ViewLayer
type Holder = Host | bpy.types.PropertyGroup

# --- [CONSTANTS] ------------------------------------------------------------------------

_REFUSALS: Final[type[TypeError | ReferenceError]] = (TypeError, ReferenceError)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

# --- [MODELS] ---------------------------------------------------------------------------


class Stored[T](msgspec.Struct, frozen=True, kw_only=True):
    """MessagePack record with ID references under one ID property key.

    Extension code 0 holds ID references, and the hooks code every other custom type.
    """

    reference_code: ClassVar[int] = 0
    data_key: ClassVar[str] = ".data"
    references_key: ClassVar[str] = ".references"
    name: str
    decoder: msgspec.msgpack.Decoder[T]
    enc_hook: Option[Callable[[object], object]] = Nothing

    @staticmethod
    def keyed[V](identity: Identity, key: Key, decoder: msgspec.msgpack.Decoder[V], /, *, enc_hook: Option[Callable[[object], object]] = Nothing) -> "Outcome[Stored[V]]":
        """Returns the store under the extension's identifier of key, the decoder stating the record type and its decode hook, a record type built at runtime included."""
        return identity.identifier(key).map(lambda name: Stored(name=name, decoder=decoder, enc_hook=enc_hook))

    def read(self, host: Holder) -> Outcome[Option[T]]:
        """Returns decoded value, `Nothing` when absent."""
        request = f"{host!r}[{self.name!r}]"

        def decoded(kind: type, value: object) -> object:
            """Keeps values of declared kind and passes other custom types to the decode hook."""
            if isinstance(value, kind):
                return value
            return Option.of_optional(self.decoder.dec_hook).map(lambda hook: hook(kind, value)).default_with(lambda: _refused(kind))

        def extended(references: IDPropertyGroup, code: int, data: memoryview) -> object:
            """Resolves ID references, other codes kept as extension values."""
            return references.get(bytes(data).decode()) if code == self.reference_code else msgspec.msgpack.Ext(code, data)

        def unpacked(references: IDPropertyGroup, data: bytes) -> Outcome[T]:
            reading = msgspec.msgpack.Decoder(self.decoder.type, strict=self.decoder.strict, dec_hook=decoded, ext_hook=partial(extended, references))
            return catch(reading.decode, exception=msgspec.DecodeError)(data).map_error(lambda error: Fault.of(RecordReason.OUTSIDE, request, self.name, member=str(error)))

        def found(held: IDPropertyGroup) -> Outcome[T]:
            return paired(typed(IDPropertyGroup, request, held.get(self.references_key)), typed(bytes, request, held.get(self.data_key))).bind(lambda pair: unpacked(*pair))

        return (
            custom_read(host, self.name)
            .map_error(partial(Fault.of, BlenderReason.REFUSED, request))
            .bind(lambda held: Option.of_optional(held).map(lambda value: typed(IDPropertyGroup, request, value).bind(found).map(Some)).default_value(Ok(Nothing)))
        )

    def write(self, host: Holder, record: T) -> Outcome[None]:
        """Encodes record, replacing stored group when content differs."""
        refused = partial(Fault.of, BlenderReason.REFUSED, f"{host!r}[{self.name!r}]")
        references: dict[str, bpy.types.ID] = {}

        def encoded(value: object) -> object:
            """Stores IDs by index and passes other custom values to the encode hook."""
            if isinstance(value, bpy.types.ID):
                references[key := str(len(references))] = value
                return msgspec.msgpack.Ext(self.reference_code, key.encode())
            return self.enc_hook.map(lambda hook: hook(value)).default_with(lambda: _refused(type(value)))

        def replaced(packed: Mapping[str, object]) -> Outcome[None]:
            def stored(held: object) -> Outcome[None]:
                return Ok(None) if isinstance(held, IDPropertyGroup) and held.to_dict() == packed else _property(host, self.name, Some(packed)).map_error(refused).map(lambda _: None)

            return custom_read(host, self.name).map_error(refused).bind(stored)

        return (
            catch(msgspec.msgpack.encode, exception=TypeError)(record, enc_hook=encoded, order="deterministic")
            .map_error(refused)
            .map(lambda data: {self.data_key: data, self.references_key: references})
            .bind(replaced)
        )

    def removed(self, host: Holder) -> Outcome[bool]:
        """Removes stored record, returning whether host held one."""
        return _property(host, self.name, Nothing).map_error(partial(Fault.of, BlenderReason.REFUSED, f"{host!r}[{self.name!r}]")).map(lambda found: found is not None)


class Recovery[T](msgspec.Struct, frozen=True, kw_only=True):
    """Stored values the members an operation writes held before and after it, restored where live values still match after."""

    stored: Stored[T]
    ends: Callable[[T], Endpoints]

    @staticmethod
    def endpoints(identity: Identity, key: Key) -> "Outcome[Recovery[Endpoints]]":
        """Returns recovery holding bare endpoints under the extension's identifier of key."""
        return Stored.keyed(identity, key, msgspec.msgpack.Decoder(Endpoints)).map(lambda stored: Recovery(stored=stored, ends=lambda ends: ends))

    def recorded(self, holder: Host, writes: tuple[tuple[Native, Snapshot], ...], held: Callable[[Endpoints], T]) -> Outcome[tuple[Applied, ...]]:
        """Stores the written members' live values, applies each snapshot, then stores both ends.

        A held record keeps its earliest before values and its after values of members these writes leave unchanged.

        Args:
            holder: Struct holding the record.
            writes: Owners and the snapshots applied to them, in write order.
            held: Record of the endpoints.

        Returns:
            Outcome per write, faults of every refused write and of the after store accumulated.
        """
        selections = {owner: tuple(descriptor for target, snapshot in writes if target == owner for descriptor in snapshot.selection(owner)) for owner, _ in writes}

        def prior() -> Outcome[Option[tuple[NativeTree, NativeTree]]]:
            """Returns decoded before and after trees of the held record, absent fault for one never completed."""
            return self.stored.read(holder).bind(lambda found: found.map(lambda kept: self.ends(kept).decoded(holder, self.stored.name).map(Some)).default_value(Ok(Nothing)))

        def captured() -> Outcome[NativeTree]:
            return accumulated(native(owner, Some(selection)) for owner, selection in selections.items()).map(Tree[NativeValue, RnaPath, RnaPath].joined)

        def written(current: NativeTree, prior: Option[tuple[NativeTree, NativeTree]]) -> Outcome[tuple[Applied, ...]]:
            before = prior.map(lambda ends: current.merged(ends[0])).default_value(current)

            def finished(after: NativeTree) -> Outcome[None]:
                latest = prior.map(lambda ends: ends[1].merged(merge(merge(current, transfer(current, ends[1])), transfer(current, after)))).default_value(after)
                return paired(persisted(before), persisted(latest)).bind(lambda pair: self.stored.write(holder, held(Endpoints(before=pair[0], after=pair[1]))))

            def changed(_: None) -> Outcome[tuple[Applied, ...]]:
                applied = tuple(snapshot.apply(owner) for owner, snapshot in writes)
                return paired(accumulated(outcome.outcome for outcome in applied), captured().bind(finished)).map(lambda _: applied)

            return persisted(before).bind(lambda kept: self.stored.write(holder, held(Endpoints(before=kept)))).bind(changed)

        return paired(captured(), prior()).bind(lambda pair: written(*pair))

    def reverted(self, holder: Host) -> Outcome[bool]:
        """Restores members still at their after values, every before value of a record never completed, then removes the record.

        Args:
            holder: Struct holding the record.

        Returns:
            Whether holder held a record, the record kept when a restore write fails.
        """

        def restored(kept: T) -> Outcome[bool]:
            return self.ends(kept).restored().bind(lambda _: self.stored.removed(holder))

        return self.stored.read(holder).bind(lambda found: found.map(restored).default_value(Ok(False)))


class RetentionSettings(bpy.types.PropertyGroup):
    """History capacity and merge interval of extension preferences, read as the retention record."""

    capacity: bpy.props.IntProperty(name="Capacity", description="Entries kept, newest first", min=1, max=100, default=Retention().capacity)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    interval: bpy.props.FloatProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Merge Interval", description="Seconds within which next entry replaces newest", unit="TIME_ABSOLUTE", min=0.0, max=600.0, default=Retention().interval.total_seconds()
    )

    @property
    def retention(self) -> Retention:
        """Retention record of the settings."""
        return Retention(capacity=self.capacity, interval=datetime.timedelta(seconds=self.interval))


class Attached[G: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Registered pointer property over its target class and the form group it holds, with the group's locks and snapshot history."""

    locking: ClassVar[Stored[frozenset[str]]] = Stored(name=".locks", decoder=msgspec.msgpack.Decoder(frozenset[str]))
    holding: ClassVar[Stored[History[Snapshot]]] = Stored(name=".history", decoder=msgspec.msgpack.Decoder(History[Snapshot]))
    name: str
    owner: type[Host]
    form: type[G]

    # --- [VALUES]

    def struct(self, host: Host) -> G:
        """Returns attached form group of host."""
        return getattr(host, self.name)  # type: ignore[no-any-return]

    # --- [LOCKS]

    def locks(self, host: Host) -> Outcome[frozenset[str]]:
        """Returns stored locks on current form members."""
        return self.locking.read(self.struct(host)).map(lambda found: found.default_with(frozenset) & frozenset(annotated(self.form)))

    def toggled(self, host: Host, key: str) -> Outcome[None]:
        """Toggles lock of one member in group storage."""
        return self.locks(host).bind(lambda held: self.locking.write(self.struct(host), held ^ {key}))

    # --- [PRESETS]

    def preset(self, host: Host) -> Outcome[Snapshot]:
        """Captures the held group as a preset, members the host skips in presets left out."""
        group = self.struct(host)
        return Snapshot.capture(group).bind(lambda current: current.projected(group, current, _skipped(group)).map(lambda pair: pair[0]))

    def projected(self, host: Host, preset: Snapshot) -> Outcome[tuple[Snapshot, Snapshot, Snapshot]]:
        """Returns the held group, the preset's members neither locked nor skipped in presets, and the group they would leave."""
        group = self.struct(host)
        return paired(Snapshot.capture(group), self.locks(host)).bind(
            lambda pair: preset.projected(group, pair[0], frozenset((key,) for key in pair[1]) | _skipped(group)).map(lambda ends: (pair[0], *ends))
        )

    def applied(self, host: Host, preset: Snapshot) -> Outcome[bool]:
        """Writes the preset's unlocked members when they differ from the held group, returning whether it wrote."""
        return self.projected(host, preset).bind(lambda ends: Ok(False) if ends[2] == ends[0] else ends[1].apply(self.struct(host)).outcome.map(lambda _: True))

    # --- [HISTORY]

    def held(self, host: Host) -> Outcome[History[Snapshot]]:
        """Returns the group's stored history, empty when none is stored."""
        return self.holding.read(self.struct(host)).map(lambda found: found.default_with(History[Snapshot]))

    def _kept(self, host: Host, history: History[Snapshot]) -> Outcome[None]:
        """Stores the group's history."""
        return self.holding.write(self.struct(host), history)

    def replaced[V](self, host: Host, write: Callable[[], Outcome[Option[V]]], moment: datetime.datetime, retention: Retention) -> Outcome[Option[V]]:
        """Runs edit, adding the group's prior snapshot when the edit reports a change."""

        def recorded(record: Snapshot, history: History[Snapshot]) -> Outcome[Option[V]]:
            return write().bind(lambda change: change.map(lambda _: self._kept(host, history.add(Dated(moment=moment, value=record), retention)).map(lambda _: change)).default_value(Ok(change)))

        return paired(Snapshot.capture(self.struct(host)), self.held(host)).bind(lambda pair: recorded(*pair))

    def stepped(self, host: Host, offset: int, moment: datetime.datetime, retention: Retention) -> Outcome[Option[None]]:
        """Undoes for positive offset, redoes for negative offset, a refused write keeping both branches with the current snapshot across from the target."""

        def navigated(record: Snapshot, history: History[Snapshot]) -> Outcome[Option[None]]:
            def moved(target: Snapshot, arrived: History[Snapshot], parked: History[Snapshot]) -> Outcome[Option[None]]:
                answer = target.apply(self.struct(host)).outcome
                return paired(answer, self._kept(host, answer.map(lambda _: arrived).default_value(parked))).map(lambda pair: Some(pair[0]))

            return history.step(offset, Dated(moment=moment, value=record), retention).map(lambda found: moved(*found)).default_value(Ok(Nothing))

        return paired(Snapshot.capture(self.struct(host)), self.held(host)).bind(lambda pair: navigated(*pair))

    def forgotten(self, host: Host, offset: int) -> Outcome[Option[Snapshot]]:
        """Removes the snapshot at signed one-based offset, `Nothing` past either end, leaving the live group unchanged."""
        return self.held(host).bind(lambda history: history.at(offset).map(lambda entry: self._kept(host, history.remove(entry.value)).map(lambda _: Some(entry.value))).default_value(Ok(Nothing)))

    # --- [HOSTS]

    def host(self, group: bpy.types.PropertyGroup) -> Option[Host]:
        """Returns host of attached root group, `Nothing` for a group no owner holds."""

        def hosting(parent: Struct) -> Option[Host]:
            return Option.of_optional(try_downcast(self.owner, parent))

        return Block.of_seq(reversed(group.rna_ancestors())).choose(hosting).try_head()

    def hosts(self) -> tuple[Host, ...]:
        """Returns file hosts of owner class, embedded IDs, nodes, and view layers included."""
        blocks = (*bpy.data.all_ids, *(tree for block in bpy.data.all_ids for tree in embedded(block).to_list()))
        match self.owner:
            case owner if issubclass(owner, bpy.types.ViewLayer):
                return tuple(layer for scene in bpy.data.scenes for layer in scene.view_layers if isinstance(layer, owner))
            case owner if issubclass(owner, bpy.types.Node):
                return tuple(node for tree in blocks if isinstance(tree, bpy.types.NodeTree) for node in tree.nodes if isinstance(node, owner))
            case _:
                return tuple(block for block in blocks if isinstance(block, self.owner))

    # --- [PERSISTENCE]

    def stored(self, host: Host) -> bool:
        """Returns whether host system properties hold attachment."""
        return host.is_property_set(self.name, ghost=False)

    def seeded(self, seed: Snapshot) -> Outcome[None]:
        """Writes seed snapshot to hosts without stored attachment."""
        return accumulated(seed.apply(self.struct(host)).outcome for host in self.hosts() if not self.stored(host)).map(lambda _: None)

    def purged(self) -> int:
        """Unsets stored attachment on every host, returning host count."""
        hosts = tuple(host for host in self.hosts() if self.stored(host))
        for host in hosts:
            host.property_unset(self.name)
        return len(hosts)


class Attachment[G: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Pointer property to a form group on its owner class, with startup seed and change callback."""

    key: Key
    form: type[G]
    owner: type[Host]
    seed: Option[Callable[[Identity], Outcome[Snapshot]]] = Nothing
    changed: Option[Callable[[Identity], AbstractContextManager[Changed]]] = Nothing

    # --- [BINDING]

    def bound(self, identity: Identity) -> Outcome[Attached[G]]:
        """Binds to registered pointer target."""
        return identity.identifier(self.key).bind(
            lambda name: Ok(Attached(name=name, owner=self.owner, form=self.form)) if name in self.owner.bl_rna.properties else Error(Fault.of(BlenderReason.ABSENT, self.owner.__name__, member=name))
        )

    def read(self, identity: Identity, host: Host) -> Outcome[G]:
        """Returns host form group through the registered binding."""
        return self.bound(identity).map(lambda attached: attached.struct(host))

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields form groups, pointer, and seed while the change callback runs after each value property's declared update."""
        groups: tuple[type[bpy.types.PropertyGroup], ...] = (*nested(self.form), self.form)
        with ExitStack() as stack:
            declared: tuple[Updated | type[bpy.types.PropertyGroup], ...] = self.changed.map(lambda build: (Updated(groups=groups, change=stack.enter_context(build(identity))),)).default_value(groups)
            overridable: set[rna_enums.PropertyOverrideFlagItems] = {"LIBRARY_OVERRIDABLE"}
            pointing = partial(bpy.props.PointerProperty, type=self.form, override=overridable)
            pointer = Option.of_optional(inspect.getdoc(self.form)).map(lambda doc: pointing(description=doc)).default_with(pointing)
            yield identity.identifier(self.key).map(
                lambda name: (
                    *declared,
                    (Band.ATTACHMENTS, pointed(self.owner, name, pointer)),
                    *(row for seed in self.seed.to_list() for row in startup(lambda: seed(identity).bind(Attached(name=name, owner=self.owner, form=self.form).seeded))),
                )
            )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _refused(kind: type) -> object:
    """Raises the type error msgspec reports as a validation error at decode and an unsupported type at encode."""
    raise TypeError(kind)


@catch(exception=_REFUSALS)
def _property(host: Holder, name: str, value: Option[Mapping[str, object]]) -> object:
    """Sets one ID property to value or pops it, returning what it set or removed, the host raising for an unstorable value or a removed host."""
    match value:
        case Option(tag="some", some=held):
            host[name] = held
            return held
        case _:
            return host.pop(name, None)


def _skipped(struct: Struct, path: RnaPath = ()) -> frozenset[RnaPath]:
    """Paths of members the host skips in presets, through nested property groups."""
    return frozenset({
        *((*path, descriptor.prop.identifier) for descriptor in Descriptor.saved(struct, lambda prop: prop.is_skip_preset)),
        *(
            found
            for descriptor in Descriptor.saved(struct, lambda prop: isinstance(prop, bpy.types.PointerProperty))
            for group in (getattr(struct, descriptor.prop.identifier),)
            if isinstance(group, bpy.types.PropertyGroup)
            for found in _skipped(group, (*path, descriptor.prop.identifier))
        ),
    })


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Attached", "Attachment", "Holder", "Host", "Recovery", "RetentionSettings", "Stored"]
