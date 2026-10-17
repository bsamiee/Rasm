"""Native record attachments with value locks, startup seeds, and explicit storage removal."""

from collections import deque
from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import StrEnum
from types import MemberDescriptorType
from typing import ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from idprop.types import IDPropertyGroup
import msgspec

from blender.bpy.data.ids.store import Handle, holder
from blender.bpy.data.rna.snapshots import Endpoints
from blender.bpy.extension.events.handlers import PathHandler
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult
from blender.bpy.settings.properties import Changed, Choices, RecordGroup
from blender.imaging.faults import accumulated, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Host = bpy.types.ID | bpy.types.Node | bpy.types.ViewLayer


class Slot(StrEnum):
    """Persisted ID-property keys outside the record's RNA field identifiers."""

    HISTORY = ".history"
    LOCKS = ".locks"
    SYNCED = ".synced"
    HELD = ".held"
    DATA = ".data"
    REFERENCES = ".references"


# --- [MODELS] ---------------------------------------------------------------------------


class Stored[T](msgspec.Struct, frozen=True, kw_only=True):
    """Typed MessagePack records and real ID references stored under one native key.

    Extension code zero is reserved for native ID references; caller codecs use other codes.
    """

    reference_code: ClassVar[int] = 0
    name: str
    decoder: msgspec.msgpack.Decoder[T]
    encoder: msgspec.msgpack.Encoder = msgspec.field(default_factory=msgspec.msgpack.Encoder)

    def read(self, host: "bpy.types.bpy_struct[object]") -> BlenderResult[Option[T]]:
        """Returns a decoded stored value, explicit absence, or native storage and codec faults."""
        try:
            held = host.get(self.name)
        except (TypeError, ReferenceError) as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))
        if held is None:
            return Ok(Nothing)

        def decoded(kind: type[object], value: object) -> object:
            """Retains values the declared kind already admits, resolved ID references among them, and delegates other custom types to the caller codec."""
            if isinstance(value, kind):
                return value
            if self.decoder.dec_hook is None:
                raise TypeError(kind)
            return self.decoder.dec_hook(kind, value)

        try:
            return Ok(
                Some(
                    msgspec.msgpack.decode(
                        held[Slot.DATA],
                        type=self.decoder.type,
                        strict=self.decoder.strict,
                        dec_hook=decoded,
                        ext_hook=lambda code, data: (
                            held[Slot.REFERENCES][bytes(data).decode()]
                            if code == self.reference_code
                            else self.decoder.ext_hook(code, data)
                            if self.decoder.ext_hook is not None
                            else msgspec.msgpack.Ext(code, data)
                        ),
                    )
                )
            )
        except (msgspec.DecodeError, TypeError, KeyError, UnicodeError, NotImplementedError) as error:
            return Error((BlenderReadFault(request=f"{host!r}[{self.name!r}]", error=error),))
        except ReferenceError as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))

    def write(self, host: "bpy.types.bpy_struct[object]", record: T) -> BlenderResult[None]:
        """Encodes once and replaces one native group holding other content, returning codec or native edit faults."""
        references: dict[str, bpy.types.ID] = {}

        def encoded(value: object) -> object:
            """Stores actual IDs beside their encoded indices and delegates custom values."""
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
        """Removes the stored record and its ID references, answering whether the host held one, preserving native refusals."""
        try:
            del host[self.name]
        except KeyError:
            return Ok(False)
        except (AttributeError, TypeError, ReferenceError) as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))
        return Ok(True)


class Attached[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """One registered native pointer and the record binding over its actual target class."""

    name: str
    owner: type[Host]
    group: RecordGroup[T]

    # --- [VALUES]

    def struct(self, host: Host) -> bpy.types.PropertyGroup:
        """Returns the attached native root group of a host."""
        return getattr(host, self.name)  # type: ignore[no-any-return]

    def read(self, host: Host) -> BlenderResult[T]:
        """Reads the attached record, returning its host conversion faults on refusal."""
        return self.group.read(self.struct(host))

    def write(self, host: Host, record: T) -> BlenderResult[None]:
        """Writes independent record leaves, returning every native write refusal."""
        return self.group.write(self.struct(host), record)

    # --- [LOCKS]

    def locks(self, host: Host) -> frozenset[str]:
        """Returns the stored locks belonging to fields of the current record."""
        return Option.of_optional(self.struct(host).get(Slot.LOCKS)).map(lambda held: frozenset(held.keys())).default_with(frozenset) & frozenset(self.group.record.__struct_fields__)

    def toggled(self, host: Host, key: str) -> None:
        """Flips the lock of one field in native group storage."""
        self.struct(host)[Slot.LOCKS] = dict.fromkeys(self.locks(host) ^ {key}, True)

    @staticmethod
    def kept(locks: frozenset[str], held: T, record: T) -> T:
        """Returns the desired record with each locked field taken from its held value."""
        return msgspec.structs.replace(record, **{key: getattr(held, key) for key in locks})

    def applied(self, host: Host, record: T) -> BlenderResult[bool]:
        """Applies a lock-kept preset where it differs from the held record, answering whether it wrote, or the read and native write refusals."""
        locks = self.locks(host)
        return self.read(host).bind(lambda held: Ok(False) if (kept := self.kept(locks, held, record)) == held else self.write(host, kept).map(lambda _: True))

    # --- [HOSTS]

    def host(self, group: bpy.types.PropertyGroup) -> Host:
        """Returns the actual host of an attached root group."""
        return next(parent for parent in reversed(group.rna_ancestors()) if isinstance(parent, self.owner))

    def hosts(self) -> tuple[Host, ...]:
        """Returns current file hosts, including embedded IDs, tree nodes, and view layers."""
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
        """Returns whether the native system-properties store contains this attachment."""
        return host.is_property_set(self.name, ghost=False)

    def seeded(self, record: T) -> BlenderResult[None]:
        """Seeds unstored hosts, returning every native write refusal."""
        return accumulated(self.write(host, record) for host in self.hosts() if not self.stored(host)).map(lambda _: None)

    def purged(self) -> int:
        """Removes persisted values explicitly and returns the number of hosts cleared."""
        hosts = tuple(host for host in self.hosts() if self.stored(host))
        for host in hosts:
            host.property_unset(self.name)
        return len(hosts)


class Attachment[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Native pointer declaration, its fresh class acquisition, and optional startup seed."""

    key: Key
    form: type[T]
    owner: type[Host]
    seed: Option[Callable[[Identity], BlenderResult[T]]] = Nothing
    changed: Option[Callable[[Identity], AbstractContextManager[Changed]]] = Nothing
    bindings: Option[Mapping[MemberDescriptorType, Choices]] = Nothing

    # --- [BINDING]

    def bound(self, identity: Identity) -> BlenderResult[Attached[T]]:
        """Binds to the registered native pointer target, returning absent or name faults."""
        return identity.name(Name.ATTACHMENT, self.key).bind(
            lambda name: (
                Option
                .of_optional(self.owner.bl_rna.properties.get(name))
                .map(lambda prop: type(prop.fixed_type))
                .to_result_with(lambda: (BlenderAbsentFault(owner=self.owner.__name__, key=name),))
                .map(lambda struct: Attached(name=name, owner=self.owner, group=RecordGroup(record=self.form, struct=struct)))
            )
        )

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds the change callback while yielding fresh native declarations or name faults."""
        with ExitStack() as stack:
            changed = self.changed.map(lambda build: stack.enter_context(build(identity)))
            yield (
                paired(identity.name(Name.ATTACHMENT, self.key), RecordGroup.bound(identity, f"{self.owner.__name__.lower()}_{self.key}", self.form, changed, bindings=self.bindings))
                .map(lambda pair: Attached(name=pair[0], owner=self.owner, group=pair[1]))
                .map(
                    lambda attached: (
                        *((Band.GROUPS, group) for group in (attached.group.struct, *attached.group.nested)),
                        (Band.ATTACHMENTS, pointed(self.owner, attached.name, attached.group.pointer)),
                        *self.seed.map(
                            lambda seed: (
                                PathHandler(handlers=bpy.app.handlers.load_post, body=lambda _: seed(identity).bind(attached.seeded)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                                Timer(body=lambda: seed(identity).bind(attached.seeded).map(lambda _: Nothing)),
                            )
                        ).default_value(()),
                    )
                )
            )


# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def pointed(owner: "type[bpy.types.bpy_struct[object]]", name: str, definition: bpy.props._PropertyDeferred) -> Generator[BlenderResult[None]]:
    """Acquires one unoccupied native property or returns its collision or declaration refusal."""
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
    """Builds the attachment update body deferring one reconcile of the holder its data-block names.

    Args:
        kind: Holder class the write resolves: a material, world, light, scene, object, or node group.
        write: Module-level write that resolves the handle, reads the owner's record, derives its graph, and reconciles inside the deferred call.

    Returns:
        The callback resource builder Attachment enters before compiling its native group.
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
    """Marks attempted preference writes dirty and returns their actual native outcome."""

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


def endpoints(name: str) -> Stored[Endpoints]:
    """Stores an operation's persisted before and after trees under one native key."""
    return Stored(name=name, decoder=msgspec.msgpack.Decoder(Endpoints))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Attached", "Attachment", "Host", "Slot", "Stored", "endpoints", "pointed", "preferred", "reconciling"]
