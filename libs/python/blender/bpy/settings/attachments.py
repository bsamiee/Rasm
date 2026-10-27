"""Form group attachments with value locks, startup seeds, before and after recovery, and storage removal."""

from collections import deque
from collections.abc import Callable, Generator
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import StrEnum
import inspect
from itertools import islice
from typing import ClassVar, Final

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
from idprop.types import IDPropertyGroup
import msgspec

from blender.bpy.data.ids.store import Handle, holder
from blender.bpy.data.rna.descriptors import Descriptor, Native, RnaPath, Struct, struct_at
from blender.bpy.data.rna.snapshots import Applied, Endpoints, native, NativeValue, Snapshot
from blender.bpy.extension.events import startup
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.trees import merge, transfer, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type Host = bpy.types.ID | bpy.types.Node | bpy.types.ViewLayer
type Changed = Callable[[bpy.types.bpy_struct[object], bpy.types.Context], None]


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

    def read(self, host: Struct) -> Outcome[Option[T]]:
        """Returns decoded value, `Nothing` when absent."""
        try:
            held = host.get(self.name)
        except (TypeError, ReferenceError) as error:
            return Error((Fault(reason=BlenderReason.REFUSED, subject=f"{host!r}[{self.name!r}]", cause=error),))
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
            return Error((Fault(reason=RecordReason.OUTSIDE, subject=f"{host!r}[{self.name!r}]", member=str(error), cause=str(held)),))
        except ReferenceError as error:
            return Error((Fault(reason=BlenderReason.REFUSED, subject=f"{host!r}[{self.name!r}]", cause=error),))

    def write(self, host: Struct, record: T) -> Outcome[None]:
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
            return Error((Fault(reason=BlenderReason.REFUSED, subject=f"{host!r}[{self.name!r}]", cause=error),))
        return Ok(None)

    def removed(self, host: Struct) -> Outcome[bool]:
        """Removes stored record, returning whether host held one."""
        try:
            del host[self.name]
        except KeyError:
            return Ok(False)
        except (AttributeError, TypeError, ReferenceError) as error:
            return Error((Fault(reason=BlenderReason.REFUSED, subject=f"{host!r}[{self.name!r}]", cause=error),))
        return Ok(True)


class Recovery[T](msgspec.Struct, frozen=True, kw_only=True):
    """Stored values the members an operation writes held before and after it, restored where live values still match after."""

    stored: Stored[T]
    ends: Callable[[T], Endpoints]

    @staticmethod
    def endpoints(name: str) -> "Recovery[Endpoints]":
        """Returns recovery holding bare endpoints under one key."""
        return Recovery(stored=Stored(name=name, decoder=msgspec.msgpack.Decoder(Endpoints)), ends=lambda ends: ends)

    def prior(self, holder: Host) -> Outcome[Option[tuple[Tree[NativeValue, RnaPath], Tree[NativeValue, RnaPath]]]]:
        """Returns decoded before and after trees of held record, absent fault for one never completed."""
        return self.stored.read(holder).bind(lambda found: found.map(lambda kept: self.ends(kept).decoded(holder, self.stored.name).map(Some)).default_value(Ok(Nothing)))

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

        def captured() -> Outcome[Tree[NativeValue, RnaPath]]:
            return accumulated(native(owner, Some(selection)) for owner, selection in selections.items()).map(Tree[NativeValue, RnaPath].joined)

        def written(current: Tree[NativeValue, RnaPath], prior: Option[tuple[Tree[NativeValue, RnaPath], Tree[NativeValue, RnaPath]]]) -> Outcome[tuple[Applied, ...]]:
            before = prior.map(lambda ends: current.merged(ends[0])).default_value(current)

            def finished(after: Tree[NativeValue, RnaPath]) -> Outcome[None]:
                latest = prior.map(lambda ends: ends[1].merged(merge(merge(current, transfer(current, ends[1])), transfer(current, after)))).default_value(after)
                return paired(Snapshot.persisted(before), Snapshot.persisted(latest)).bind(lambda pair: self.stored.write(holder, held(Endpoints(before=pair[0], after=pair[1]))))

            def changed(_: None) -> Outcome[tuple[Applied, ...]]:
                applied = tuple(snapshot.apply(owner) for owner, snapshot in writes)
                return paired(accumulated(outcome.outcome for outcome in applied), captured().bind(finished)).map(lambda _: applied)

            return Snapshot.persisted(before).bind(lambda kept: self.stored.write(holder, held(Endpoints(before=kept)))).bind(changed)

        return paired(captured(), self.prior(holder)).bind(lambda pair: written(*pair))

    def reverted(self, holder: Host) -> Outcome[bool]:
        """Restores members still at their after values, every before value of a record never completed, then removes the record.

        Args:
            holder: Struct holding the record.

        Returns:
            Whether holder held a record, the record kept when a restore write fails.
        """

        def live(before: Tree[NativeValue, RnaPath]) -> Outcome[Tree[NativeValue, RnaPath]]:
            """Captures current values of every member the before tree holds."""
            return accumulated(
                native(struct, Some(tuple(descriptor for name in kept.values for descriptor in Descriptor.resolve(struct, name, bpy.types.Property).to_option().to_list())))
                for kept in before.owners.values()
                for target, _ in islice(kept.values.values(), 1)
                for struct in (struct_at(*target) if isinstance(target, tuple) else Some(target)).to_list()
            ).map(Tree[NativeValue, RnaPath].joined)

        def restored(kept: T) -> Outcome[bool]:
            ends = self.ends(kept)
            return (
                Snapshot
                .decoded(ends.before)
                .bind(lambda before: Option.of_optional(ends.after).map(Snapshot.decoded).default_with(lambda: live(before)).bind(lambda after: Snapshot.restore(before, after).outcome))
                .bind(lambda _: self.stored.removed(holder))
            )

        return self.stored.read(holder).bind(lambda found: found.map(restored).default_value(Ok(False)))


class Attached[G: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Registered pointer property over its target class and the form group it holds."""

    name: str
    owner: type[Host]
    form: type[G]

    # --- [VALUES]

    def struct(self, host: Host) -> G:
        """Returns attached form group of host."""
        return getattr(host, self.name)  # type: ignore[no-any-return]

    # --- [LOCKS]

    def locks(self, host: Host) -> frozenset[str]:
        """Returns stored locks on current form members."""
        return Option.of_optional(self.struct(host).get(Slot.LOCKS)).map(lambda held: frozenset(held.keys())).default_with(frozenset) & frozenset(annotated(self.form))

    def toggled(self, host: Host, key: str) -> None:
        """Toggles lock of one member in group storage."""
        self.struct(host)[Slot.LOCKS] = dict.fromkeys(self.locks(host) ^ {key}, True)

    def projected(self, host: Host, preset: Snapshot) -> Outcome[tuple[Snapshot, Snapshot, Snapshot]]:
        """Returns the held group, the preset's unlocked members, and the group they would leave."""
        group = self.struct(host)
        return Snapshot.capture(group).bind(lambda current: preset.projected(group, current, frozenset((key,) for key in self.locks(host))).map(lambda pair: (current, *pair)))

    def applied(self, host: Host, preset: Snapshot) -> Outcome[bool]:
        """Writes the preset's unlocked members when they differ from the held group, returning whether it wrote."""
        return self.projected(host, preset).bind(lambda ends: Ok(False) if ends[2] == ends[0] else ends[1].apply(self.struct(host)).outcome.map(lambda _: True))

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
            lambda name: (
                Ok(Attached(name=name, owner=self.owner, form=self.form))
                if name in self.owner.bl_rna.properties
                else Error((Fault(reason=BlenderReason.ABSENT, subject=self.owner.__name__, member=name),))
            )
        )

    def read(self, identity: Identity, host: Host) -> Outcome[G]:
        """Returns host form group through the registered binding."""
        return self.bound(identity).map(lambda attached: attached.struct(host))

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields form groups, pointer, and seed while holding change callback, entered into every value property's update."""
        groups: tuple[type[bpy.types.PropertyGroup], ...] = (*nested(self.form), self.form)
        with ExitStack() as stack:
            for change in self.changed.map(lambda build: stack.enter_context(build(identity))).to_list():
                updating(groups, change)
            pointer = bpy.props.PointerProperty(type=self.form, override={"LIBRARY_OVERRIDABLE"})
            pointer.keywords.update(Option.of_optional(inspect.getdoc(self.form)).map(lambda doc: {"description": doc}).default_value({}))
            yield identity.identifier(self.key).map(
                lambda name: (
                    *((Band.GROUPS, group) for group in groups),
                    (Band.ATTACHMENTS, pointed(self.owner, name, pointer)),
                    *(row for seed in self.seed.to_list() for row in startup(lambda: seed(identity).bind(Attached(name=name, owner=self.owner, form=self.form).seeded))),
                )
            )


# --- [OPERATIONS] -----------------------------------------------------------------------


def annotated(form: type[Struct]) -> dict[str, bpy.props._PropertyDeferred]:
    """Property definitions a form class and its bases declare, bases first, as Blender registers them."""
    return {name: definition for kind in reversed(form.__mro__) for name, definition in vars(kind).get("__annotations__", {}).items() if isinstance(definition, bpy.props._PropertyDeferred)}


def nested(form: type[bpy.types.PropertyGroup]) -> tuple[type[bpy.types.PropertyGroup], ...]:
    """Property group classes a form's pointer and collection properties hold, children first."""
    return tuple(
        child
        for definition in annotated(form).values()
        if definition.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}
        for target in (definition.keywords["type"],)
        if issubclass(target, bpy.types.PropertyGroup)
        for child in (*nested(target), target)
    )


def updating(groups: tuple[type[bpy.types.PropertyGroup], ...], change: Changed) -> None:
    """Enters the change callback into the update of every value property the groups declare, ahead of their registration."""
    for definition in (definition for group in groups for definition in annotated(group).values()):
        if definition.function is not bpy.props.CollectionProperty and not issubclass(definition.keywords.get("type", bpy.types.ID), bpy.types.PropertyGroup):
            definition.keywords["update"] = change


_POINTED: Final[Callable[[type[Struct], str, bpy.props._PropertyDeferred], Result[None, AttributeError | TypeError | ValueError]]] = catch(exception=AttributeError)(
    catch(exception=TypeError)(catch(setattr, exception=ValueError))
)  # ty: ignore[invalid-assignment]


@contextmanager
def pointed(owner: type[Struct], name: str, definition: bpy.props._PropertyDeferred) -> Generator[Outcome[None]]:
    """Registers property under a free name for scope duration, the host refusing a name past its byte limit."""
    request = f"{owner.__name__}.{name}"
    rna: bpy.types.Struct = owner.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    with ExitStack() as stack:
        yield (
            Result[None, Faults](ok=None)
            .filter_with(
                lambda _: not (name in rna.properties or name in rna.functions or hasattr(owner, name)), lambda _: (Fault(reason=BlenderReason.TAKEN, subject=request, member=owner.__name__),)
            )
            .bind(lambda _: _POINTED(owner, name, definition).map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=request, cause=error),)))
            .map(lambda _: stack.callback(delattr, owner, name))
            .map(lambda _: None)
        )


def reconciling[T: bpy.types.ID](kind: type[T], write: Callable[[Handle[T], Identity], Outcome[Option[str]]]) -> Callable[[Identity], AbstractContextManager[Changed]]:
    """Builds update callback deferring one reconcile per changed holder.

    Args:
        kind: Holder class write resolves.
        write: Deferred write reconciling holder.

    Returns:
        Callback scope `Attachment` enters before registering its groups.
    """

    @contextmanager
    def built(identity: Identity) -> Generator[Changed]:
        with identity.reports.deferred(write) as deferred:

            def changed(owner: Struct, _context: bpy.types.Context) -> None:
                for found in Option.of_optional(owner.id_data).bind(holder).to_list():
                    if isinstance(found, kind):
                        deferred(Handle.of(found), identity)

            yield changed

    return built


def preferred[T](identity: Identity, preferences: bpy.types.Preferences, write: Callable[[bpy.types.AddonPreferences], Outcome[T]]) -> Outcome[T]:
    """Writes add-on preferences and marks them dirty."""

    def marked(holder: bpy.types.AddonPreferences) -> Outcome[T]:
        answer = write(holder)
        preferences.is_dirty = True
        return answer

    return (
        Option
        .of_optional(preferences.addons.get(identity.package))
        .bind(lambda addon: Option.of_optional(addon.preferences))
        .map(marked)
        .default_with(lambda: Error((Fault(reason=BlenderReason.ABSENT, subject=repr(preferences), member=identity.package),)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Attached", "Attachment", "Changed", "Host", "Recovery", "Slot", "Stored", "annotated", "nested", "pointed", "preferred", "reconciling", "updating"]
