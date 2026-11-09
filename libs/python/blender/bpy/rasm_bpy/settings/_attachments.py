"""Form group attachments with bound callbacks, locks, histories, startup seeds, recovery, removal, and groups saved in text blocks."""

from collections import defaultdict, deque
from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from dataclasses import dataclass
import datetime
from functools import partial
import inspect
from types import MappingProxyType
from typing import ClassVar, Generic, Never, TYPE_CHECKING, TypeVar

import bpy
from expression import Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
from idprop.types import IDPropertyGroup
import msgspec

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import _Startup, _Tier, Acquisitions, Handler, Timer
from rasm_bpy.data._ids.ownership import Ownership, Saved
from rasm_bpy.data._ids.store import _embedded
from rasm_bpy.data._rna.descriptors import _custom_read, Descriptor, stored
from rasm_bpy.data._rna.paths import Native, RnaPath, rooted, Struct
from rasm_bpy.data._rna.properties import _declaring, _pointed, annotated, Callbacks, Changed
from rasm_bpy.data._rna.restoration import _equal, _native, _persisted, BeforeAfter, NativeTree, NativeValue
from rasm_bpy.data._rna.snapshots import Applied, Snapshot
from rasm_records import accumulated, Dated, Error, Fault, History, merge, Nothing, Ok, Outcome, paired, Reason, Retention, transfer, Tree, typed

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Host = bpy.types.ID | bpy.types.Node | bpy.types.ViewLayer
type Holder = Host | bpy.types.PropertyGroup
_G_co = TypeVar("_G_co", bound=bpy.types.PropertyGroup, covariant=True, default=bpy.types.PropertyGroup)

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
    def keyed[V](identity: Identity, key: Key, decoder: msgspec.msgpack.Decoder[V], /, *, enc_hook: Option[Callable[[object], object]] = Nothing) -> "Stored[V]":
        """Returns the store under the extension's identifier of key, the decoder stating the record type, runtime-built types included, and its hook."""
        return Stored(name=identity.identifier(key), decoder=decoder, enc_hook=enc_hook)

    def read(self, host: Holder) -> Outcome[Option[T]]:
        """Returns decoded value, `Nothing` when absent."""
        request = f"{host!r}[{self.name!r}]"

        def decoded(kind: type, value: object) -> object:
            """Keeps values of declared kind and passes other custom types to the decode hook."""
            if isinstance(value, kind):
                return value
            return Option.of_optional(self.decoder.dec_hook).map(lambda hook: hook(kind, value)).default_with(lambda: _refused(kind))

        def extended(references: IDPropertyGroup, code: int, data: memoryview) -> object:
            """Resolves ID references and delegates other codes to the decoder's hook."""
            hook = self.decoder.ext_hook
            return references.get(bytes(data).decode()) if code == self.reference_code else hook(code, data) if hook is not None else msgspec.msgpack.Ext(code, data)

        def unpacked(references: IDPropertyGroup, data: bytes) -> Outcome[T]:
            reading = msgspec.msgpack.Decoder(self.decoder.type, strict=self.decoder.strict, dec_hook=decoded, ext_hook=partial(extended, references))
            return catch(reading.decode, exception=msgspec.DecodeError)(data).map_error(partial(Fault.of, Reason.OUTSIDE, request, member=repr(self.decoder.type)))

        def found(held: IDPropertyGroup) -> Outcome[T]:
            return paired(typed(IDPropertyGroup, request, held.get(self.references_key)), typed(bytes, request, held.get(self.data_key))).bind(lambda pair: unpacked(*pair))

        return (
            _custom_read(host, self.name)
            .map_error(partial(Fault.of, Reason.REFUSED, request))
            .bind(lambda held: Option.of_optional(held).map(lambda value: typed(IDPropertyGroup, request, value).bind(found).map(Some)).default_value(Ok(Nothing)))
        )

    def write(self, host: Holder, record: T) -> Outcome[None]:
        """Encodes record and replaces its stored group atomically."""
        refused = partial(Fault.of, Reason.REFUSED, f"{host!r}[{self.name!r}]")
        references: dict[str, bpy.types.ID] = {}

        def encoded(value: object) -> object:
            """Stores IDs by index and passes other custom values to the encode hook."""
            if isinstance(value, bpy.types.ID):
                references[key := str(len(references))] = value
                return msgspec.msgpack.Ext(self.reference_code, key.encode())
            return self.enc_hook.map(lambda hook: hook(value)).default_with(lambda: _refused(type(value)))

        return (
            catch(msgspec.msgpack.encode, exception=TypeError)(record, enc_hook=encoded, order="deterministic")
            .map_error(refused)
            .bind(lambda data: _property(host, self.name, Some({self.data_key: data, self.references_key: references})).map_error(refused))
            .map(lambda _: None)
        )

    def removed(self, host: Holder) -> Outcome[bool]:
        """Removes stored record, returning whether host held one."""
        return _property(host, self.name, Nothing).map_error(partial(Fault.of, Reason.REFUSED, f"{host!r}[{self.name!r}]")).map(lambda found: found is not None)


class Recovery[T](msgspec.Struct, frozen=True, kw_only=True):
    """Stored values the members an operation writes held before and after it, restored where live values still match after."""

    stored: Stored[T]
    before_after_of: Callable[[T], BeforeAfter]

    @staticmethod
    def before_after(identity: Identity, key: Key) -> "Recovery[BeforeAfter]":
        """Returns recovery holding unwrapped before and after records under the extension's identifier of key."""
        return Recovery(stored=Stored.keyed(identity, key, msgspec.msgpack.Decoder(BeforeAfter)), before_after_of=lambda ends: ends)

    def recorded(self, holder: Host, writes: tuple[tuple[Native, Snapshot], ...], held: Callable[[BeforeAfter], T]) -> Outcome[tuple[Applied, ...]]:
        """Stores the written members' live values, applies each snapshot, then stores both ends.

        Held record keeps its earliest before values and its after values of members these writes leave unchanged.

        Args:
            holder: Struct holding the record.
            writes: Owners and the snapshots applied to them, in write order.
            held: Record of the before and after values.

        Returns:
            Outcome per write, faults of every refused write and of the after store accumulated.
        """
        selections: defaultdict[Native, list[Descriptor]] = defaultdict(list)
        for owner, snapshot in writes:
            selections[owner].extend(snapshot.selection(owner))

        def prior() -> Outcome[Option[tuple[NativeTree, NativeTree]]]:
            """Returns decoded before and after trees of the held record, absent fault for one never completed."""
            return self.stored.read(holder).bind(lambda found: found.map(lambda kept: self.before_after_of(kept).decoded(holder, self.stored.name).map(Some)).default_value(Ok(Nothing)))

        def captured() -> Outcome[NativeTree]:
            return accumulated(_native(owner, Some(tuple(selection))) for owner, selection in selections.items()).map(Tree[NativeValue, RnaPath, RnaPath].joined)

        def written(current: NativeTree, prior: Option[tuple[NativeTree, NativeTree]]) -> Outcome[tuple[Applied, ...]]:
            before = prior.map(lambda ends: current.merged(ends[0])).default_value(current)

            def finished(after: NativeTree) -> Outcome[None]:
                latest = prior.map(lambda ends: ends[1].merged(merge(merge(current, transfer(current, ends[1], equal=_equal)), transfer(current, after, equal=_equal)))).default_value(after)
                return paired(_persisted(before), _persisted(latest)).bind(lambda pair: self.stored.write(holder, held(BeforeAfter(before=pair[0], after=pair[1]))))

            def changed(_: None) -> Outcome[tuple[Applied, ...]]:
                applied = tuple(snapshot.apply(owner) for owner, snapshot in writes)
                return paired(accumulated(outcome.outcome for outcome in applied), captured().bind(finished)).map(lambda _: applied)

            return _persisted(before).bind(lambda kept: self.stored.write(holder, held(BeforeAfter(before=kept)))).bind(changed)

        return paired(captured(), prior()).bind(lambda pair: written(*pair))

    def reverted(self, holder: Host) -> Outcome[bool]:
        """Restores members still at their after values, every before value of a record never completed, then removes the record.

        Args:
            holder: Struct holding the record.

        Returns:
            Whether holder held a record, the record kept when a restore write fails.
        """

        def restored(kept: T) -> Outcome[bool]:
            return self.before_after_of(kept).restored().bind(lambda _: self.stored.removed(holder))

        return self.stored.read(holder).bind(lambda found: found.map(restored).default_value(Ok(value=False)))


class RetentionSettings(bpy.types.PropertyGroup):
    """History capacity and merge interval of extension preferences, read as the retention record."""

    capacity: bpy.props.IntProperty(name="Capacity", description="Entries kept, newest first", min=1, max=100, default=Retention().capacity)  # ty: ignore[invalid-type-form]
    interval: bpy.props.FloatProperty(  # ty: ignore[invalid-type-form]
        name="Merge Interval", description="Seconds within which next entry replaces newest", unit="TIME_ABSOLUTE", min=0.0, max=600.0, default=Retention().interval.total_seconds()
    )

    @property
    def retention(self) -> Retention:
        """Retention record of the settings."""
        return Retention(capacity=self.capacity, interval=datetime.timedelta(seconds=self.interval))


class Attached(msgspec.Struct, Generic[_G_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Registered pointer property over its target class and the form group it holds, with the group's locks and snapshot history."""

    # --- [STATE]

    lock_store: ClassVar[Stored[frozenset[str]]] = Stored(name=".locks", decoder=msgspec.msgpack.Decoder(frozenset[str]))
    history_store: ClassVar[Stored[History[Snapshot]]] = Stored(name=".history", decoder=msgspec.msgpack.Decoder(History[Snapshot]))
    name: str
    owner: type[Host]
    form: type[_G_co]

    # --- [VALUES]

    def struct(self, host: Host) -> _G_co:
        """Returns attached form group of host."""
        group: _G_co = getattr(host, self.name)  # ty: ignore[unsound-assignment]
        return group

    # --- [LOCKS]

    def locks(self, host: Host) -> Outcome[frozenset[str]]:
        """Returns stored locks on current form members."""
        return self.lock_store.read(self.struct(host)).map(lambda found: found.default_with(frozenset[str]) & frozenset(annotated(self.form)))

    def toggled(self, host: Host, key: str) -> Outcome[None]:
        """Toggles lock of one member in group storage."""
        return self.locks(host).bind(lambda held: self.lock_store.write(self.struct(host), held ^ {key}))

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
        return self.projected(host, preset).bind(lambda ends: Ok(value=False) if ends[2] == ends[0] else ends[1].apply(self.struct(host)).outcome.map(lambda _: True))

    # --- [HISTORY]

    def history(self, host: Host) -> Outcome[History[Snapshot]]:
        """Returns the group's stored history, empty when none is stored."""
        return self.history_store.read(self.struct(host)).map(lambda found: found.default_with(History[Snapshot]))

    def recorded[V](self, host: Host, write: Callable[[], Outcome[Option[V]]], moment: datetime.datetime, retention: Retention) -> Outcome[Option[V]]:
        """Runs edit, adding the group's prior snapshot when the edit reports a change."""

        def recorded(record: Snapshot, history: History[Snapshot]) -> Outcome[Option[V]]:
            return write().bind(
                lambda change: change.map(lambda _: self.history_store.write(self.struct(host), history.add(Dated(moment=moment, value=record), retention)).map(lambda _: change)).default_value(
                    Ok(change)
                )
            )

        return paired(Snapshot.capture(self.struct(host)), self.history(host)).bind(lambda pair: recorded(*pair))

    def stepped(self, host: Host, offset: int, moment: datetime.datetime, retention: Retention) -> Outcome[Option[None]]:
        """Undoes for positive offset, redoes for negative, a refused write keeping both branches, the current snapshot on the branch opposite the target."""

        def navigated(record: Snapshot, history: History[Snapshot]) -> Outcome[Option[None]]:
            def moved(target: Snapshot, arrived: History[Snapshot], kept: History[Snapshot]) -> Outcome[Option[None]]:
                answer = target.apply(self.struct(host)).outcome
                return paired(answer, self.history_store.write(self.struct(host), answer.map(lambda _: arrived).default_value(kept))).map(lambda pair: Some(pair[0]))

            return history.step(offset, Dated(moment=moment, value=record), retention).map(lambda found: moved(*found)).default_value(Ok(Nothing))

        return paired(Snapshot.capture(self.struct(host)), self.history(host)).bind(lambda pair: navigated(*pair))

    def forgotten(self, host: Host, offset: int) -> Outcome[Option[Snapshot]]:
        """Removes the snapshot at signed one-based offset, `Nothing` past either end, leaving the live group unchanged."""
        return self.history(host).bind(
            lambda history: history.at(offset).map(lambda entry: self.history_store.write(self.struct(host), history.remove(entry.value)).map(lambda _: Some(entry.value))).default_value(Ok(Nothing))
        )

    # --- [HOSTS]

    def host(self, group: bpy.types.PropertyGroup) -> Option[Host]:
        """Returns host of attached root group, `Nothing` for a group no owner holds."""

        def hosting(parent: Struct) -> Option[Host]:
            return Option.of_optional(try_downcast(self.owner, parent))

        return Block.of_seq(reversed(group.rna_ancestors())).choose(hosting).try_head()

    def hosts(self) -> tuple[Host, ...]:
        """Returns file hosts of owner class, embedded IDs, nodes, and view layers included."""
        match self.owner:
            case owner if issubclass(owner, bpy.types.ViewLayer):
                return tuple(layer for scene in bpy.data.scenes for layer in scene.view_layers if isinstance(layer, owner))
            case owner:
                originals = tuple(bpy.data.all_ids)
                blocks = (*originals, *(tree for block in originals for tree in _embedded(block).to_list()))
                return (
                    tuple(node for tree in blocks if isinstance(tree, bpy.types.NodeTree) for node in tree.nodes if isinstance(node, owner))
                    if issubclass(owner, bpy.types.Node)
                    else tuple(block for block in blocks if isinstance(block, owner))
                )

    # --- [PERSISTENCE]

    def stored(self, host: Host) -> bool:
        """Returns whether a write left values in host's attachment, the empty group a read creates holding none."""
        return stored(host, self.name)

    def seeded(self, seed: Snapshot) -> Outcome[None]:
        """Writes seed snapshot to hosts without stored attachment."""
        return accumulated(seed.apply(self.struct(host)).outcome for host in self.hosts() if not self.stored(host)).map(lambda _: None)

    def purged(self) -> int:
        """Unsets stored attachment on every host, returning host count."""
        hosts = tuple(host for host in self.hosts() if self.stored(host))
        for host in hosts:
            host.property_unset(self.name)
        return len(hosts)


@dataclass(frozen=True, slots=True, kw_only=True, init=False)
class Attachment(Generic[_G_co]):  # ruff: ignore[non-pep695-generic-class]
    """Pointer property to a form group on its owner class, with startup seed, change callback, and property callbacks bound to the extension."""

    # --- [STATE]

    key: Key
    form: type[_G_co]
    owner: type[Host]
    seed: Option[Callable[[Identity], Outcome[Snapshot]]] = Nothing
    changed: Option[Callable[[Identity], AbstractContextManager[Changed]]] = Nothing
    _callbacks: Option[Callable[[Identity], AbstractContextManager[Mapping[str, Callbacks[Never]]]]] = Nothing

    def __init__(
        self,
        *,
        key: Key,
        form: type[_G_co],
        owner: type[Host],
        seed: Option[Callable[[Identity], Outcome[Snapshot]]] = Nothing,
        changed: Option[Callable[[Identity], AbstractContextManager[Changed]]] = Nothing,
        callbacks: Option[Callable[[Identity], AbstractContextManager[Mapping[str, Callbacks[_G_co]]]]] = Nothing,
    ) -> None:
        """Binds callback owners to form and retains each supplied acquisition scope.

        Args:
            key: Pointer name within extension.
            form: Property group held by pointer.
            owner: Host class holding pointer.
            seed: Startup snapshot producer.
            changed: Change callback acquisition scope.
            callbacks: Property callbacks acquisition scope over form owners.
        """
        object.__setattr__(self, "key", key)
        object.__setattr__(self, "form", form)
        object.__setattr__(self, "owner", owner)
        object.__setattr__(self, "seed", seed)
        object.__setattr__(self, "changed", changed)
        object.__setattr__(self, "_callbacks", callbacks)

    # --- [BINDING]

    def bound(self, identity: Identity) -> Outcome[Attached[_G_co]]:
        """Binds to registered pointer target."""
        name = identity.identifier(self.key)
        return Ok(Attached(name=name, owner=self.owner, form=self.form)) if name in self.owner.bl_rna.properties else Error(Fault.of(Reason.ABSENT, self.owner.__name__, member=name))

    def read(self, identity: Identity, host: Host) -> Outcome[_G_co]:
        """Returns host form group through the registered binding."""
        return self.bound(identity).map(lambda attached: attached.struct(host))

    def fields(self, layout: bpy.types.UILayout, context: bpy.types.Context, _properties: object, identity: Identity) -> None:
        """Draws every form field of the group the context's owner holds, nothing where the context reaches no group."""
        for group in rooted(self.owner, context).bind(lambda host: self.read(identity, host)).to_option().to_list():
            for name in annotated(self.form):
                layout.prop(group, name)

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Acquisitions]:
        """Yields form groups, pointer, and seed while form properties hold bound callbacks and the change callback follows each value property's update."""
        with ExitStack() as stack:
            callbacks = self._callbacks.map(lambda build: stack.enter_context(build(identity))).default_value(MappingProxyType({}))
            groups = stack.enter_context(_declaring(self.form, callbacks, self.changed.map(lambda build: stack.enter_context(build(identity)))))
            overridable: set[rna_enums.PropertyOverrideFlagItems] = {"LIBRARY_OVERRIDABLE"}
            pointing = partial(bpy.props.PointerProperty, type=self.form, override=overridable)
            pointer = Option.of_optional(inspect.getdoc(self.form)).map(lambda doc: pointing(description=doc)).default_with(pointing)
            name = identity.identifier(self.key)
            yield (
                *groups,
                (_Tier.ATTACHMENTS, _pointed(self.owner, name, pointer)),
                *(_Startup(body=lambda: seed(identity).bind(Attached(name=name, owner=self.owner, form=self.form).seeded)) for seed in self.seed.to_list()),
            )


class TextRecord[G: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Window manager form group saved as a snapshot in an owned fake-user text block."""

    # --- [STATE]

    key: Key
    form: type[G]

    # --- [STORAGE]

    @property
    def attachment(self) -> Attachment[G]:
        """Window manager attachment."""
        return Attachment(key=self.key, form=self.form, owner=bpy.types.WindowManager)

    @property
    def ownership(self) -> Ownership[bpy.types.Text]:
        """Text block holding the snapshot, kept by fake user."""
        return Ownership(kind=bpy.types.Text, key=self.key, saved=Saved.ALWAYS)

    def loaded(self, identity: Identity, attached: Attached[G], manager: bpy.types.WindowManager) -> Outcome[None]:
        """Applies the snapshot of the owned text block, defaults kept when absent."""
        return (
            self.ownership
            .found(identity)
            .map(lambda text: Snapshot.parsed(repr(text), text.as_string()).bind(lambda snapshot: snapshot.apply(attached.struct(manager)).outcome))
            .default_value(Ok(None))
        )

    def saved(self, identity: Identity, attached: Attached[G], manager: bpy.types.WindowManager) -> Outcome[None]:
        """Saves the window manager group snapshot into the owned text block, created on first save."""
        title = f"{identity.manifest.name} {bpy.path.display_name(self.key)}"

        def stored(snapshot: Snapshot) -> Outcome[None]:
            return self.ownership.made(identity, lambda: catch(bpy.data.texts.new, exception=RuntimeError)(title).map_error(partial(Fault.of, Reason.REFUSED, title))).map(
                lambda text: text.from_string(snapshot.text)
            )

        return Snapshot.capture(attached.struct(manager)).bind(stored)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns attachment, text ownership, load and save handlers, and a startup load for a window manager holding none."""

        def session() -> Outcome[tuple[Attached[G], bpy.types.WindowManager]]:
            return paired(self.attachment.bound(identity), rooted(bpy.types.WindowManager, bpy.context))

        def started() -> Outcome[Option[float]]:
            return session().bind(lambda pair: Ok(None) if pair[0].stored(pair[1]) else self.loaded(identity, *pair)).map(lambda _: Nothing)

        return (
            self.attachment,
            self.ownership,
            Handler(handlers=bpy.app.handlers.load_post, body=lambda: session().bind(lambda pair: self.loaded(identity, *pair))),
            Handler(handlers=bpy.app.handlers.save_pre, body=lambda: session().bind(lambda pair: self.saved(identity, *pair))),
            Timer(body=started),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _refused(kind: type) -> object:
    """Raises the type error msgspec reports as a validation error at decode and an unsupported type at encode."""
    raise TypeError(kind)


def _property(host: Holder, name: str, value: Option[Mapping[str, object]]) -> Result[object, TypeError | ReferenceError]:
    """Sets or pops one ID property, returning its value or the native exception for an unstorable value or a removed host."""
    try:
        match value:
            case Option(tag="some", some=held):
                host[name] = held
                return Result[object, TypeError | ReferenceError].Ok(held)
            case _:
                return Result[object, TypeError | ReferenceError].Ok(host.pop(name, None))
    except (TypeError, ReferenceError) as error:
        return Result[object, TypeError | ReferenceError].Error(error)


def _skipped(struct: Struct) -> frozenset[RnaPath]:
    """Paths of members the host skips in presets, through nested property groups."""
    pending: deque[tuple[Struct, RnaPath]] = deque(((struct, ()),))
    skipped: set[RnaPath] = set()
    while pending:
        owner, path = pending.popleft()
        members = Descriptor.saved(owner)
        skipped.update((*path, descriptor.prop.identifier) for descriptor in members if descriptor.prop.is_skip_preset)
        pending.extend(
            (group, (*path, descriptor.prop.identifier))
            for descriptor in members
            if isinstance(descriptor.prop, bpy.types.PointerProperty)
            for group in (getattr(owner, descriptor.prop.identifier),)
            if isinstance(group, bpy.types.PropertyGroup)
        )
    return frozenset(skipped)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Attached", "Attachment", "Holder", "Host", "Recovery", "RetentionSettings", "Stored", "TextRecord"]
