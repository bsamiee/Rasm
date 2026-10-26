"""Record attachments with value locks, startup seeds, before and after recovery, and storage removal."""

from collections import deque
from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import StrEnum
from itertools import islice
from types import MemberDescriptorType
from typing import ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from idprop.types import IDPropertyGroup
import msgspec

from blender.bpy.data.ids.store import Handle, holder
from blender.bpy.data.rna.descriptors import Descriptor, Native, struct_at
from blender.bpy.data.rna.edits import merge, transfer, Tree
from blender.bpy.data.rna.snapshots import Applied, Endpoints, native, NativeValue, Snapshot
from blender.bpy.extension.events.timers import Startup
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.settings.properties import Changed, Choices, RecordGroup
from blender.imaging.faults import Access, accumulated, ImagingFileFault, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Host = bpy.types.ID | bpy.types.Node | bpy.types.ViewLayer


class Slot(StrEnum):
    """ID property keys outside record field identifiers."""

    HISTORY = ".history"
    LOCKS = ".locks"
    DATA = ".data"
    REFERENCES = ".references"


# --- [MODELS] ---------------------------------------------------------------------------


class Stored[T](msgspec.Struct, frozen=True, kw_only=True):
    """MessagePack record with ID references under one ID property key.

    Extension code 0 holds ID references, caller codecs take other codes.
    """

    reference_code: ClassVar[int] = 0
    name: str
    decoder: msgspec.msgpack.Decoder[T]
    encoder: msgspec.msgpack.Encoder = msgspec.field(default_factory=msgspec.msgpack.Encoder)

    def read(self, host: "bpy.types.bpy_struct[object]") -> BlenderResult[Option[T]]:
        """Returns decoded value, `Nothing` when absent."""
        try:
            held = host.get(self.name)
        except (TypeError, ReferenceError) as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))
        if held is None:
            return Ok(Nothing)

        def decoded(kind: type[object], value: object) -> object:
            """Keeps values of declared kind and passes other custom types to caller codec."""
            if isinstance(value, kind):
                return value
            if self.decoder.dec_hook is None:
                raise TypeError(kind)
            return self.decoder.dec_hook(kind, value)

        def extended(code: int, data: memoryview) -> object:
            """Resolves ID references and passes other codes to caller codec."""
            match code:
                case self.reference_code:
                    return held[Slot.REFERENCES][bytes(data).decode()]
                case _:
                    return Option.of_optional(self.decoder.ext_hook).map(lambda hook: hook(code, data)).default_with(lambda: msgspec.msgpack.Ext(code, data))

        try:
            return Ok(Some(msgspec.msgpack.decode(held[Slot.DATA], type=self.decoder.type, strict=self.decoder.strict, dec_hook=decoded, ext_hook=extended)))
        except (msgspec.DecodeError, TypeError, KeyError, UnicodeError, NotImplementedError) as error:
            return Error((ImagingFileFault(access=Access.READ, request=f"{host!r}[{self.name!r}]", error=error),))
        except ReferenceError as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))

    def write(self, host: "bpy.types.bpy_struct[object]", record: T) -> BlenderResult[None]:
        """Encodes record, replacing stored group when content differs."""
        references: dict[str, bpy.types.ID] = {}

        def encoded(value: object) -> object:
            """Stores IDs by index and passes other custom values to caller codec."""
            if isinstance(value, bpy.types.ID):
                key = str(len(references))
                references[key] = value
                return msgspec.msgpack.Ext(self.reference_code, key.encode())
            match self.encoder.enc_hook:
                case None:
                    raise TypeError(type(value))
                case hook:
                    return hook(value)

        try:
            stored = {Slot.DATA: msgspec.msgpack.encode(record, enc_hook=encoded, order="deterministic"), Slot.REFERENCES: references}
            if not (isinstance(held := host.get(self.name), IDPropertyGroup) and held.to_dict() == stored):
                host[self.name] = stored
        except (msgspec.EncodeError, AttributeError, KeyError, TypeError, ValueError, OverflowError, ReferenceError, NotImplementedError) as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))
        return Ok(None)

    def removed(self, host: "bpy.types.bpy_struct[object]") -> BlenderResult[bool]:
        """Removes stored record, returning whether host held one."""
        try:
            del host[self.name]
        except KeyError:
            return Ok(False)
        except (AttributeError, TypeError, ReferenceError) as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))
        return Ok(True)


class Recovery[T](msgspec.Struct, frozen=True, kw_only=True):
    """Stored values the members an operation writes held before and after it, restored where live values still match after."""

    stored: Stored[T]
    ends: Callable[[T], Endpoints]

    @staticmethod
    def endpoints(name: str) -> "Recovery[Endpoints]":
        """Returns recovery holding bare endpoints under one key."""
        return Recovery(stored=Stored(name=name, decoder=msgspec.msgpack.Decoder(Endpoints)), ends=lambda ends: ends)

    def prior(self, holder: Host) -> BlenderResult[Option[tuple[Tree[NativeValue], Tree[NativeValue]]]]:
        """Returns decoded before and after trees of held record, absent fault for one never completed."""
        return self.stored.read(holder).bind(lambda found: found.map(lambda kept: self.ends(kept).decoded(holder, self.stored.name).map(Some)).default_value(Ok(Nothing)))

    def recorded(self, holder: Host, writes: tuple[tuple[Native, Snapshot], ...], held: Callable[[Endpoints], T]) -> BlenderResult[tuple[Applied, ...]]:
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

        def captured() -> BlenderResult[Tree[NativeValue]]:
            return accumulated(native(owner, Some(selection)) for owner, selection in selections.items()).map(Tree.joined)

        def written(current: Tree[NativeValue], prior: Option[tuple[Tree[NativeValue], Tree[NativeValue]]]) -> BlenderResult[tuple[Applied, ...]]:
            before = prior.map(lambda ends: current.merged(ends[0])).default_value(current)

            def finished(after: Tree[NativeValue]) -> BlenderResult[None]:
                latest = prior.map(lambda ends: ends[1].merged(merge(merge(current, transfer(current, ends[1])), transfer(current, after)))).default_value(after)
                return paired(Snapshot.persisted(before), Snapshot.persisted(latest)).bind(lambda pair: self.stored.write(holder, held(Endpoints(before=pair[0], after=pair[1]))))

            def changed(_: None) -> BlenderResult[tuple[Applied, ...]]:
                applied = tuple(snapshot.apply(owner) for owner, snapshot in writes)
                return paired(accumulated(outcome.outcome for outcome in applied), captured().bind(finished)).map(lambda _: applied)

            return Snapshot.persisted(before).bind(lambda kept: self.stored.write(holder, held(Endpoints(before=kept)))).bind(changed)

        return paired(captured(), self.prior(holder)).bind(lambda pair: written(*pair))

    def reverted(self, holder: Host) -> BlenderResult[bool]:
        """Restores members still at their after values, every before value of a record never completed, then removes the record.

        Args:
            holder: Struct holding the record.

        Returns:
            Whether holder held a record, the record kept when a restore write fails.
        """

        def live(before: Tree[NativeValue]) -> BlenderResult[Tree[NativeValue]]:
            """Captures current values of every member the before tree holds."""
            return accumulated(
                native(struct, Some(tuple(descriptor for name in kept.values for descriptor in Descriptor.resolve(struct, name, bpy.types.Property).to_option().to_list())))
                for kept in before.owners.values()
                for target, _ in islice(kept.values.values(), 1)
                for struct in (struct_at(*target) if isinstance(target, tuple) else Some(target)).to_list()
            ).map(Tree.joined)

        def restored(kept: T) -> BlenderResult[bool]:
            ends = self.ends(kept)
            return (
                Snapshot
                .decoded(ends.before)
                .bind(lambda before: Option.of_optional(ends.after).map(Snapshot.decoded).default_with(lambda: live(before)).bind(lambda after: Snapshot.restore(before, after).outcome))
                .bind(lambda _: self.stored.removed(holder))
            )

        return self.stored.read(holder).bind(lambda found: found.map(restored).default_value(Ok(False)))


class Attached[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Registered pointer property and record binding over its target class."""

    name: str
    owner: type[Host]
    group: RecordGroup[T]

    # --- [VALUES]

    def struct(self, host: Host) -> bpy.types.PropertyGroup:
        """Returns attached root group of host."""
        return getattr(host, self.name)  # type: ignore[no-any-return]

    def read(self, host: Host) -> BlenderResult[T]:
        """Reads attached record."""
        return self.group.read(self.struct(host))

    def write(self, host: Host, record: T) -> BlenderResult[None]:
        """Writes record leaves, accumulating every refused write."""
        return self.group.write(self.struct(host), record)

    # --- [LOCKS]

    def locks(self, host: Host) -> frozenset[str]:
        """Returns stored locks on current record fields."""
        return Option.of_optional(self.struct(host).get(Slot.LOCKS)).map(lambda held: frozenset(held.keys())).default_with(frozenset) & frozenset(self.group.record.__struct_fields__)

    def toggled(self, host: Host, key: str) -> None:
        """Toggles lock of one field in group storage."""
        self.struct(host)[Slot.LOCKS] = dict.fromkeys(self.locks(host) ^ {key}, True)

    @staticmethod
    def kept(locks: frozenset[str], held: T, record: T) -> T:
        """Returns desired record with locked fields taken from held record."""
        return msgspec.structs.replace(record, **{key: getattr(held, key) for key in locks})

    def applied(self, host: Host, record: T) -> BlenderResult[bool]:
        """Writes record with locked fields kept when it differs from held record, returning whether it wrote."""
        locks = self.locks(host)
        return self.read(host).bind(lambda held: Ok(False) if (kept := self.kept(locks, held, record)) == held else self.write(host, kept).map(lambda _: True))

    # --- [HOSTS]

    def host(self, group: bpy.types.PropertyGroup) -> Host:
        """Returns host of attached root group."""
        return next(parent for parent in reversed(group.rna_ancestors()) if isinstance(parent, self.owner))

    def hosts(self) -> tuple[Host, ...]:
        """Returns file hosts of owner class, embedded IDs, nodes, and view layers included."""
        pending = deque(block for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty) for block in getattr(bpy.data, prop.identifier))
        blocks: dict[int, bpy.types.ID] = {}
        while pending:
            if (key := (block := (pending.popleft())).as_pointer()) not in blocks:
                blocks[key] = block
                pending.extend(
                    child
                    for prop in block.bl_rna.properties
                    if isinstance(prop, bpy.types.PointerProperty) and not prop.is_deprecated and isinstance(prop.fixed_type, bpy.types.ID)
                    for child in Option.of_optional(getattr(block, prop.identifier)).to_list()
                    if child.is_embedded_data
                )
        match self.owner:
            case owner if issubclass(owner, bpy.types.ViewLayer):
                return tuple(layer for scene in bpy.data.scenes for layer in scene.view_layers if isinstance(layer, owner))
            case owner if issubclass(owner, bpy.types.Node):
                return tuple(node for tree in blocks.values() if isinstance(tree, bpy.types.NodeTree) for node in tree.nodes if isinstance(node, owner))
            case _:
                return tuple(block for block in blocks.values() if isinstance(block, self.owner))

    # --- [PERSISTENCE]

    def stored(self, host: Host) -> bool:
        """Returns whether host system properties hold attachment."""
        return host.is_property_set(self.name, ghost=False)

    def seeded(self, record: T) -> BlenderResult[None]:
        """Writes seed record to hosts without stored attachment."""
        return accumulated(self.write(host, record) for host in self.hosts() if not self.stored(host)).map(lambda _: None)

    def purged(self) -> int:
        """Unsets stored attachment on every host, returning host count."""
        hosts = tuple(host for host in self.hosts() if self.stored(host))
        for host in hosts:
            host.property_unset(self.name)
        return len(hosts)


class Attachment[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Pointer property declaration with per-registration classes and startup seed."""

    key: Key
    form: type[T]
    owner: type[Host]
    seed: Option[Callable[[Identity], BlenderResult[T]]] = Nothing
    changed: Option[Callable[[Identity], AbstractContextManager[Changed]]] = Nothing
    bindings: Option[Mapping[MemberDescriptorType, Choices]] = Nothing

    # --- [BINDING]

    def bound(self, identity: Identity) -> BlenderResult[Attached[T]]:
        """Binds to registered pointer target."""
        return identity.name(Name.ATTACHMENT, self.key).bind(
            lambda name: (
                Option
                .of_optional(self.owner.bl_rna.properties.get(name))
                .map(lambda prop: type(prop.fixed_type))
                .to_result_with(lambda: (BlenderAbsentFault(owner=self.owner.__name__, key=name),))
                .map(lambda struct: Attached(name=name, owner=self.owner, group=RecordGroup(record=self.form, struct=struct)))
            )
        )

    def read(self, identity: Identity, host: Host) -> BlenderResult[T]:
        """Reads host record through the registered binding."""
        return self.bound(identity).bind(lambda attached: attached.read(host))

    def revised(self, identity: Identity, host: Host, change: Callable[[T], BlenderResult[T]]) -> BlenderResult[T]:
        """Writes change of host record, answering the written record."""
        return self.bound(identity).bind(lambda attached: attached.read(host).bind(change).bind(lambda record: attached.write(host, record).map(lambda _: record)))

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Yields built declarations while holding change callback."""
        with ExitStack() as stack:
            changed = self.changed.map(lambda build: stack.enter_context(build(identity)))
            yield (
                paired(identity.name(Name.ATTACHMENT, self.key), RecordGroup.bound(identity, f"{self.owner.__name__.lower()}_{self.key}", self.form, changed, bindings=self.bindings))
                .map(lambda pair: Attached(name=pair[0], owner=self.owner, group=pair[1]))
                .map(
                    lambda attached: (
                        *((Band.GROUPS, group) for group in (attached.group.struct, *attached.group.nested)),
                        (Band.ATTACHMENTS, pointed(self.owner, attached.name, attached.group.pointer)),
                        *self.seed.map(lambda seed: Startup(body=lambda: seed(identity).bind(attached.seeded))).to_list(),
                    )
                )
            )


# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def pointed(owner: "type[bpy.types.bpy_struct[object]]", name: str, definition: bpy.props._PropertyDeferred) -> Generator[BlenderResult[None]]:
    """Registers property under a free name for scope duration."""
    request = f"{owner.__name__}.{name}"
    if name in owner.bl_rna.properties or name in owner.bl_rna.functions or hasattr(owner, name):  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        yield Result[None, BlenderFaults](error=(BlenderCollisionFault(request=request, error=owner.__name__),))
        return
    try:
        setattr(owner, name, definition)
    except (AttributeError, TypeError, ValueError) as error:
        yield Result[None, BlenderFaults](error=(BlenderEditFault(request=request, error=error),))
        return
    try:
        yield Result[None, BlenderFaults](ok=None)
    finally:
        delattr(owner, name)


def reconciling[T: bpy.types.ID](kind: type[T], write: Callable[[Handle[T], Identity], BlenderResult[Option[str]]]) -> Callable[[Identity], AbstractContextManager[Changed]]:
    """Builds update callback deferring one reconcile per changed holder.

    Args:
        kind: Holder class write resolves.
        write: Deferred write reconciling holder.

    Returns:
        Callback scope `Attachment` enters before building its group.
    """

    @contextmanager
    def built(identity: Identity) -> Generator[Changed]:
        with identity.reports.deferred(write) as deferred:

            def changed(owner: "bpy.types.bpy_struct[object]", _context: bpy.types.Context, _property: bpy.types.Property) -> None:
                for found in Option.of_optional(owner.id_data).bind(holder).to_list():
                    if isinstance(found, kind):
                        deferred(Handle.of(found), identity)

            yield changed

    return built


def preferred[T](identity: Identity, preferences: bpy.types.Preferences, write: Callable[[bpy.types.AddonPreferences], BlenderResult[T]]) -> BlenderResult[T]:
    """Writes add-on preferences and marks them dirty."""

    def marked(holder: bpy.types.AddonPreferences) -> BlenderResult[T]:
        answer = write(holder)
        preferences.is_dirty = True
        return answer

    return (
        Option
        .of_optional(preferences.addons.get(identity.package))
        .bind(lambda addon: Option.of_optional(addon.preferences))
        .map(marked)
        .default_with(lambda: Error((BlenderAbsentFault(owner=repr(preferences), key=identity.package),)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Attached", "Attachment", "Host", "Recovery", "Slot", "Stored", "pointed", "preferred", "reconciling"]
